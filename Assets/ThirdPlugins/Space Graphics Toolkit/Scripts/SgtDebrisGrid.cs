using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtDebrisGrid))]
public class SgtDebrisGrid_Editor : SgtEditor<SgtDebrisGrid>
{
	protected override void OnInspector()
	{
		var clearUpdate = false;

		BeginError(Any(t => t.Target == null));
			DrawDefault("Target");
		EndError();
		DrawDefault("SpawnInside", ref clearUpdate);

		Separator();

		BeginError(Any(t => t.CellSize <= 0.0f));
			DrawDefault("CellSize", ref clearUpdate);
		EndError();
		DrawDefault("CellNoise", ref clearUpdate);
		BeginError(Any(t => t.DebrisCountTarget <= 0));
			DrawDefault("DebrisCountTarget", ref clearUpdate);
		EndError();
		DrawDefault("Seed", ref clearUpdate);

		Separator();

		BeginError(Any(t => t.ShowDistance <= 0.0f || t.ShowDistance > t.HideDistance));
			DrawDefault("ShowDistance");
		EndError();
		BeginError(Any(t => t.HideDistance < 0.0f || t.ShowDistance > t.HideDistance));
			DrawDefault("HideDistance");
		EndError();

		Separator();

		BeginError(Any(t => t.ScaleMin < 0.0f || t.ScaleMin > t.ScaleMax));
			DrawDefault("ScaleMin");
			DrawDefault("ScaleMax");
		EndError();
		DrawDefault("ScaleBias");
		DrawDefault("RandomRotation");

		Separator();

		BeginError(Any(t => t.Prefabs == null || t.Prefabs.Count == 0 || t.Prefabs.Contains(null) == true));
			DrawDefault("Prefabs", ref clearUpdate);
		EndError();

		if (clearUpdate == true) DirtyEach(t => { t.ClearDebris(); t.UpdateDebris(); });
	}

	private bool InvalidShapes(List<SgtShape> shapes)
	{
		if (shapes == null || shapes.Count == 0)
		{
			return true;
		}

		for (var i = shapes.Count - 1; i >= 0; i--)
		{
			if (shapes[i] == null)
			{
				return true;
			}
		}

		return false;
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debris Grid")]
public class SgtDebrisGrid : MonoBehaviour
{
	[Tooltip("The transform the debris will spawn around (e.g. MainCamera)")]
	public Transform Target;

	[Tooltip("The shapes the debris will spawn inside")]
	public SgtShapeGroup SpawnInside;

	[Tooltip("The distance from the follower that debris begins spawning")]
	public float ShowDistance = 0.9f;

	[Tooltip("The distance from the follower that debris gets hidden")]
	public float HideDistance = 1.0f;

	[Tooltip("The size of each cell in world space")]
	public double CellSize = 1.0f;

	[Tooltip("How far from the center of each cell the debris can be spawned. This should be decreated to stop debris intersecting")]
	[Range(0.0f, 0.5f)]
	public float CellNoise = 0.5f;

	[Tooltip("The maximum expected amount of debris based on the cell size settings")]
	public float DebrisCountTarget = 100;

	[Tooltip("The minimum scale multiplier of the debris")]
	public float ScaleMin = 1.0f;

	[Tooltip("The maximum scale multiplier of the debris")]
	public float ScaleMax = 2.0f;

	[Tooltip("The amount the random scale is biased toward the min or max values")]
	public float ScaleBias = 2.0f;

	[Tooltip("Should the debris be given a random rotation, or inherit from the prefab that spawned it?")]
	public bool RandomRotation = true;

	[Tooltip("Random seed used when generating the debris")]
	[SgtSeed]
	public int Seed;

	[Tooltip("These prefabs are randomly picked from when spawning new debris")]
	public List<SgtDebris> Prefabs;

	// The currently spawned debris
	public List<SgtDebris> Debris;

	private float minScale = 0.001f;

	[SerializeField]
	private SgtBoundsL bounds;

	// Used during find
	private SgtDebris targetPrefab;

	[ContextMenu("Clear Debris")]
	public void ClearDebris()
	{
		if (Debris != null)
		{
			for (var i = Debris.Count - 1; i >= 0; i--)
			{
				var debris = Debris[i];

				if (debris != null)
				{
					Despawn(debris);
				}
			}

			Debris.Clear();
		}

		bounds.Clear();
	}

	public void UpdateDebris()
	{
		var size = (long)System.Math.Ceiling(SgtHelper.Divide(HideDistance, (float)CellSize));

		if (Target != null && CellSize > 0.0f && Prefabs != null && DebrisCountTarget > 0 && size > 0)
		{
			var worldPoint = Target.position;
			var centerX    = (long)System.Math.Round(worldPoint.x / CellSize);
			var centerY    = (long)System.Math.Round(worldPoint.y / CellSize);
			var centerZ    = (long)System.Math.Round(worldPoint.z / CellSize);
			
			var newBounds  = new SgtBoundsL(centerX, centerY, centerZ, size);

			if (newBounds != bounds)
			{
				var probability = DebrisCountTarget / (size * size * size);
				var cellMin     = (float)CellSize * (0.5f - CellNoise);
				var cellMax     = (float)CellSize * (0.5f + CellNoise);

				for (var z = newBounds.minZ; z <= newBounds.maxZ; z++)
				{
					for (var y = newBounds.minY; y <= newBounds.maxY; y++)
					{
						for (var x = newBounds.minX; x <= newBounds.maxX; x++)
						{
							if (bounds.Contains(x, y, z) == false)
							{
								// Calculate seed for this grid cell and try to minimize visible symmetry
								var seed = Seed ^ (x * (1<<8) ) ^ (y * (1<<16) ) ^ (z * (1<<24) );

								SgtHelper.BeginRandomSeed((int)(seed % int.MaxValue));
								{
									// Can debris potentially spawn in this cell?
									if (Random.value < probability)
									{
										var debrisX     = x * CellSize + Random.Range(cellMin, cellMax);
										var debrisY     = y * CellSize + Random.Range(cellMin, cellMax);
										var debrisZ     = z * CellSize + Random.Range(cellMin, cellMax);
										var debrisPoint = new Vector3((float)debrisX, (float)debrisY, (float)debrisZ);

										// Spawn everywhere, or only inside specified shapes?
										if (SpawnInside == null || Random.value < SpawnInside.GetDensity(debrisPoint))
										{
											Spawn(x, y, z, debrisPoint);
										}
									}
								}
								SgtHelper.EndRandomSeed();
							}
						}
					}
				}

				bounds = newBounds;

				if (Debris != null)
				{
					for (var i = Debris.Count - 1; i >= 0; i--)
					{
						var debris = Debris[i];

						if (debris == null)
						{
							Debris.RemoveAt(i);
						}
						else if (bounds.Contains(debris.Cell) == false)
						{
							Despawn(debris, i);
						}
					}
				}
			}

			UpdateDebrisScale(worldPoint);
		}
		else
		{
			ClearDebris();
		}
	}

	public void UpdateDebrisScale(Vector3 worldPoint)
	{
		if (Debris != null)
		{
			var hideSqrDistance = HideDistance * HideDistance;
			var showSqrDistance = ShowDistance * ShowDistance;

			for (var i = Debris.Count - 1; i >= 0; i--)
			{
				var debris = Debris[i];

				if (debris != null)
				{
					var debrisTransform = debris.transform;
					var sqrDistance     = Vector3.SqrMagnitude(debrisTransform.position - worldPoint);
					
					if (sqrDistance >= hideSqrDistance)
					{
						if (debris.State != SgtDebris.StateType.Hide)
						{
							debris.State = SgtDebris.StateType.Hide;

							debrisTransform.localScale = debris.Scale * minScale;
						}
					}
					else if (sqrDistance <= showSqrDistance)
					{
						if (debris.State != SgtDebris.StateType.Show)
						{
							debris.State = SgtDebris.StateType.Show;

							debrisTransform.localScale = debris.Scale;
						}
					}
					else
					{
						debris.State = SgtDebris.StateType.Fade;

						debrisTransform.localScale = debris.Scale * Mathf.Max(Mathf.InverseLerp(HideDistance, ShowDistance, Mathf.Sqrt(sqrDistance)), minScale);
					}
				}
			}
		}
	}

	public static SgtDebrisGrid CreateDebrisGrid(int layer = 0, Transform parent = null)
	{
		return CreateDebrisGrid(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtDebrisGrid CreateDebrisGrid(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = SgtHelper.CreateGameObject("Debris Grid", layer, parent, localPosition, localRotation, localScale);
		var debrisGrid = gameObject.AddComponent<SgtDebrisGrid>();

		return debrisGrid;
	}

#if UNITY_EDITOR
	[MenuItem(SgtHelper.GameObjectMenuPrefix + "Debris Grid", false, 10)]
	public static void CreateDebrisGridMenuItem()
	{
		var parent        = SgtHelper.GetSelectedParent();
		var debrisSpawner = CreateDebrisGrid(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(debrisSpawner);
	}
#endif

	protected virtual void Update()
	{
		UpdateDebris();
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		if (Target != null)
		{
			var point = Target.position;

			Gizmos.DrawWireSphere(point, ShowDistance);
			Gizmos.DrawWireSphere(point, HideDistance);

			if (CellSize > 0.0f)
			{
			/*
				point.x = Mathf.Floor(point.x / CellSize) * CellSize;
				point.y = Mathf.Floor(point.y / CellSize) * CellSize;
				point.z = Mathf.Floor(point.z / CellSize) * CellSize;

				var show = Mathf.Floor(ShowDistance / CellSize) * CellSize * 2.0f;
				var hide = Mathf.Floor(HideDistance / CellSize) * CellSize * 2.0f;

				Gizmos.DrawWireCube(point, Vector3.one * show);
				Gizmos.DrawWireCube(point, Vector3.one * hide);*/
			}
		}
	}
#endif

	private void Spawn(long x, long y, long z, Vector3 point)
	{
		var index  = Random.Range(0, Prefabs.Count);
		var prefab = Prefabs[index];

		if (prefab != null)
		{
			var debris = Spawn(prefab);

			debris.Cell.x = x;
			debris.Cell.y = y;
			debris.Cell.z = z;

			debris.transform.position = point;

			if (RandomRotation == true)
			{
				debris.transform.localRotation = Random.rotation;
			}
			else
			{
				debris.transform.localRotation = prefab.transform.rotation;
			}

			debris.State = SgtDebris.StateType.Fade;
			debris.Scale = prefab.transform.localScale * Mathf.Lerp(ScaleMin, ScaleMax, SgtHelper.Pow(Random.value, ScaleBias));

			if (debris.OnSpawn != null) debris.OnSpawn();

			Debris.Add(debris);
		}
	}

	private SgtDebris Spawn(SgtDebris prefab)
	{
		if (prefab.Pool == true)
		{
			targetPrefab = prefab;

			var debris = SgtComponentPool<SgtDebris>.Pop(DebrisMatch);

			if (debris != null)
			{
				debris.transform.SetParent(transform, false);

				return debris;
			}
		}

		return Instantiate(prefab, transform);
	}

	private void Despawn(SgtDebris debris)
	{
		if (debris.OnDespawn != null) debris.OnDespawn();

		if (debris.Pool == true)
		{
			SgtComponentPool<SgtDebris>.Add(debris);
		}
		else
		{
			SgtHelper.Destroy(debris.gameObject);
		}
	}

	private void Despawn(SgtDebris debris, int index)
	{
		Despawn(debris);

		Debris.RemoveAt(index);
	}

	private bool DebrisMatch(SgtDebris debris)
	{
		return debris != null && debris.Prefab == targetPrefab;
	}
}