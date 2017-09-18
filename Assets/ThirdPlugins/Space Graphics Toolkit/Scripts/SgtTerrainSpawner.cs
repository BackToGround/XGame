using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainSpawner))]
public class SgtTerrainSpawner_Editor : SgtEditor<SgtTerrainSpawner>
{
	protected override void OnInspector()
	{
		var updateTerrain = false;

		BeginError(Any(t => t.Depth < 0));
			DrawDefault("Depth", ref updateTerrain);
		EndError();
		DrawDefault("SpawnProbability", ref updateTerrain);
		DrawDefault("HeightMin", ref updateTerrain);
		DrawDefault("HeightMax", ref updateTerrain);

		Separator();

		BeginError(Any(t => InvalidPrefabs(t.Prefabs)));
			DrawDefault("Prefabs", ref updateTerrain);
		EndError();

		if (updateTerrain == true) DirtyEach(t => t.DirtyTerrain());
	}

	private bool InvalidPrefabs(List<SgtTerrainObject> prefabs)
	{
		if (prefabs == null || prefabs.Count == 0)
		{
			return false;
		}

		for (var i = prefabs.Count - 1; i >= 0; i--)
		{
			if (prefabs[i] == null)
			{
				return false;
			}
		}

		return true;
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Spawner")]
public class SgtTerrainSpawner : SgtTerrainModifier
{
	[System.Serializable]
	public class Face
	{
		public SgtTerrainLevel Level;

		public SgtRectL Rect;

		public List<SgtTerrainObject> Clones;

		public void Clear()
		{
			if (Clones != null)
			{
				for (var i = Clones.Count - 1; i >= 0; i--)
				{
					var clone = Clones[i];

					if (clone != null)
					{
						Despawn(clone);
					}
				}

				Clones.Clear();
			}
		}

		public void Clear(SgtTerrain.Ring ring)
		{
			if (Clones != null)
			{
				for (var i = Clones.Count - 1; i >= 0; i--)
				{
					var clone = Clones[i];

					if (clone != null)
					{
						if (ring.Outer.Contains(clone.X, clone.Y) == false)
						{
							Despawn(clone);

							Clones.RemoveAt(i);
						}
					}
					else
					{
						Clones.RemoveAt(i);
					}
				}
			}
		}
	}

	[Tooltip("The patch depth required for these objects to spawn")]
	public int Depth;

	[Tooltip("This decides how many prefabs get spawned based on a random 0..1 sample on the x axis")]
	[Range(0.0f, 1.0f)]
	public float SpawnProbability;

	public float HeightMin = 1.0f;

	public float HeightMax = 1.1f;

	[Tooltip("The prefabs we want to spawn on the terrain patch")]
	public List<SgtTerrainObject> Prefabs;

	[SerializeField]
	private List<Face> faces;

	[SerializeField]
	private List<SgtTerrainObject> terrainObjects;

	protected override void OnEnable()
	{
		base.OnEnable();

		terrain.OnCalculateLevel += CalculateLevel;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		terrain.OnCalculateLevel += CalculateLevel;

		Clear();
	}

	private Face GetFace(SgtTerrainLevel level)
	{
		if (faces == null)
		{
			faces = new List<Face>();
		}

		for (var i = faces.Count - 1; i >= 0; i--)
		{
			var face = faces[i];

			if (face != null && face.Level != null)
			{
				if (face.Level == level)
				{
					return face;
				}
			}
			else
			{
				faces.RemoveAt(i);
			}
		}

		return null;
	}

	public void Clear()
	{
		if (faces != null)
		{
			for (var i = faces.Count - 1; i >= 0; i--)
			{
				var face = faces[i];

				if (face != null)
				{
					face.Clear();
				}
			}

			faces.Clear();
		}
	}

	private void AddObject(Face face, SgtTerrain.Ring ring, long x, long y)
	{
		if (Prefabs != null && Prefabs.Count > 0)
		{
			var index  = Random.Range(0, Prefabs.Count);
			var prefab = Prefabs[index];

			if (prefab != null)
			{
				var stepX  = ring.ExtentsX / ring.Detail;
				var stepY  = ring.ExtentsY / ring.Detail;
				var point  = ring.Center + x * stepX + y * stepY;
				var height = terrain.GetLocalHeight(point);

				if (height >= HeightMin && height < HeightMax)
				{
					var clone = Spawn(prefab);

					clone.Prefab = prefab;
					clone.X      = x;
					clone.Y      = y;

					clone.Spawn(terrain, face.Level, point);

					if (face.Clones == null)
					{
						face.Clones = new List<SgtTerrainObject>();
					}

					face.Clones.Add(clone);
				}
			}
		}
	}

	private void CalculateLevel(SgtTerrainLevel level, SgtTerrain.Ring ring)
	{
		var face = GetFace(level);

		if (level.Index == Depth && ring != null)
		{
			if (face == null)
			{
				face = new Face();

				face.Level = level;

				faces.Add(face);
			}

			face.Clear(ring);

			for (var y = ring.Outer.minY; y <= ring.Outer.maxY; y++)
			{
				for (var x = ring.Outer.minX; x <= ring.Outer.maxX; x++)
				{
					if (x >= face.Rect.minX && x < face.Rect.maxX && y >= face.Rect.minY && y < face.Rect.maxY)
					{
						continue;
					}

					if (Random.value < SpawnProbability)
					{
						AddObject(face, ring, x, y);
					}
				}
			}

			face.Rect = ring.Outer;
		}
		else
		{
			if (face != null)
			{
				face.Clear();

				faces.Remove(face);
			}
		}
	}

	private static SgtTerrainObject targetPrefab;

	private static SgtTerrainObject Despawn(SgtTerrainObject prefab)
	{
		if (prefab.Pool == true)
		{
			SgtComponentPool<SgtTerrainObject>.Add(prefab);
		}
		else
		{
			prefab.Despawn();
		}

		return null;
	}

	private static SgtTerrainObject Spawn(SgtTerrainObject prefab)
	{
		if (prefab.Pool == true)
		{
			targetPrefab = prefab;

			var clone = SgtComponentPool<SgtTerrainObject>.Pop(ObjectMatch);

			if (clone != null)
			{
				return clone;
			}
		}

		return Instantiate(prefab);
	}

	private static bool ObjectMatch(SgtTerrainObject instance)
	{
		return instance != null && instance.Prefab == targetPrefab;
	}
}
