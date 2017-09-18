using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainFace))]
public class SgtTerrainFace_Editor : SgtEditor<SgtTerrainFace>
{
	protected override void OnInspector()
	{
		var updateMaterials = false;

		DrawDefault("Material", ref updateMaterials);

		Separator();

		BeginDisabled();
			DrawDefault("Terrain");
			DrawDefault("Side");
		EndDisabled();

		if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("")]
public class SgtTerrainFace : MonoBehaviour
{
	[Tooltip("The material applied to all renderers on this face")]
	public Material Material;

	[Tooltip("The terrain this belongs to")]
	public SgtTerrain Terrain;

	[Tooltip("The index of this face")]
	public CubemapFace Side;

	// The LOD levels for this face
	public List<SgtTerrainLevel> Levels;

	[System.NonSerialized]
	public SgtTerrainLevel LastLevel;

	private static SgtVector3D negativeX = new SgtVector3D(-1.0,  0.0,  0.0);
	private static SgtVector3D negativeY = new SgtVector3D( 0.0, -1.0,  0.0);
	private static SgtVector3D negativeZ = new SgtVector3D( 0.0,  0.0, -1.0);
	private static SgtVector3D positiveX = new SgtVector3D( 1.0,  0.0,  0.0);
	private static SgtVector3D positiveY = new SgtVector3D( 0.0,  1.0,  0.0);
	private static SgtVector3D positiveZ = new SgtVector3D( 0.0,  0.0,  1.0);

	private static SgtTerrain.Ring ring = new SgtTerrain.Ring();

	public void Mark()
	{
		LastLevel = null;

		if (Levels == null)
		{
			Levels = new List<SgtTerrainLevel>();
		}

		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level != null)
			{
				level.Marked = true;
			}
			else
			{
				Levels.RemoveAt(i);
			}
		}
	}

	public SgtTerrainLevel GetLevel(int index)
	{
		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level.Index == index)
			{
				level.Marked = false;

				return level;
			}
		}

		var newLevel = SgtTerrainLevel.Create("Level " + index, gameObject.layer, transform);

		newLevel.Marked = false;
		newLevel.Index  = index;
		newLevel.Face   = this;
		newLevel.UpdateMaterials();

		Levels.Add(newLevel);

		return newLevel;
	}

	public void UpdateMaterials()
	{
		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level != null)
			{
				level.UpdateMaterials();
			}
		}
	}

	public void Dirty()
	{
		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level != null)
			{
				level.Dirty();
			}
		}
	}

	public void Sweep()
	{
		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level.Marked == true)
			{
				if (Terrain != null && Terrain.OnCalculateLevel != null)
				{
					Terrain.OnCalculateLevel(level, null);
				}

				SgtTerrainLevel.MarkForDestruction(level);

				Levels.RemoveAt(i);
			}
		}
	}

	public void UpdateBounds(Bounds bounds)
	{
		for (var i = Levels.Count - 1; i >= 0; i--)
		{
			var level = Levels[i];

			if (level != null && level.Mesh != null)
			{
				level.Mesh.bounds = bounds;
			}
		}
	}

	public void Write(SgtBoundsL bounds, SgtTerrain.Shell shell, int side, int index)
	{
		ring.Limit  = shell.Limit;
		ring.Detail = shell.Detail;

		switch (side)
		{
			case 0:
			{
				if (bounds.minX >= shell.Outer.minX && bounds.maxX <= shell.Outer.maxX)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectZY;
					ring.Inner    = shell.Inner.IsInsideX(bounds.minX) == true ? shell.Inner.RectZY : default(SgtRectL);
					ring.Center   = negativeX;
					ring.ExtentsX = negativeZ;
					ring.ExtentsY = positiveY;

					ring.Outer.SwapX(); ring.Inner.SwapX();

					GetLevel(index).Write(ring);
				}
			}
			break;

			case 3:
			{
				if (bounds.minX >= shell.Outer.minX && bounds.maxX <= shell.Outer.maxX)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectZY;
					ring.Inner    = shell.Inner.IsInsideX(bounds.maxX - 1) == true ? shell.Inner.RectZY : default(SgtRectL);
					ring.Center   = positiveX;
					ring.ExtentsX = positiveZ;
					ring.ExtentsY = positiveY;

					GetLevel(index).Write(ring);
				}
			}
			break;

			case 1:
			{
				if (bounds.minY >= shell.Outer.minY && bounds.maxY <= shell.Outer.maxY)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectXZ;
					ring.Inner    = shell.Inner.IsInsideY(bounds.minY) == true ? shell.Inner.RectXZ : default(SgtRectL);
					ring.Center   = negativeY;
					ring.ExtentsX = negativeX;
					ring.ExtentsY = positiveZ;

					ring.Outer.SwapX(); ring.Inner.SwapX();

					GetLevel(index).Write(ring);
				}
			}
			break;

			case 4:
			{
				if (bounds.minY >= shell.Outer.minY && bounds.maxY <= shell.Outer.maxY)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectXZ;
					ring.Inner    = shell.Inner.IsInsideY(bounds.maxY - 1) == true ? shell.Inner.RectXZ : default(SgtRectL);
					ring.Center   = positiveY;
					ring.ExtentsX = negativeX;
					ring.ExtentsY = negativeZ;

					ring.Outer.SwapX(); ring.Outer.SwapY(); ring.Inner.SwapX(); ring.Inner.SwapY();

					GetLevel(index).Write(ring);
				}
			}
			break;

			case 2:
			{
				if (bounds.minZ >= shell.Outer.minZ && bounds.maxZ <= shell.Outer.maxZ)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectXY;
					ring.Inner    = shell.Inner.IsInsideZ(bounds.minZ) == true ? shell.Inner.RectXY : default(SgtRectL);
					ring.Center   = negativeZ;
					ring.ExtentsX = positiveX;
					ring.ExtentsY = positiveY;

					GetLevel(index).Write(ring);
				}
			}
			break;

			case 5:
			{
				if (bounds.minZ >= shell.Outer.minZ && bounds.maxZ <= shell.Outer.maxZ)
				{
					bounds.ClampTo(shell.Outer);

					ring.Outer    = bounds.RectXY;
					ring.Inner    = shell.Inner.IsInsideZ(bounds.maxZ - 1) == true ? shell.Inner.RectXY : default(SgtRectL);
					ring.Center   = positiveZ;
					ring.ExtentsX = negativeX;
					ring.ExtentsY = positiveY;

					ring.Outer.SwapX(); ring.Inner.SwapX();

					GetLevel(index).Write(ring);
				}
			}
			break;
		}
	}

	public static SgtTerrainFace Create(string name, int layer, Transform parent)
	{
		return SgtComponentPool<SgtTerrainFace>.Pop(parent, name, layer);
	}

	public static SgtTerrainFace Pool(SgtTerrainFace face)
	{
		if (face != null)
		{
			face.Terrain  = null;
			face.Material = null;

			SgtComponentPool<SgtTerrainFace>.Add(face);
		}

		return null;
	}

	public static SgtTerrainFace MarkForDestruction(SgtTerrainFace face)
	{
		if (face != null)
		{
			face.Terrain = null;

			face.gameObject.SetActive(true);
		}

		return null;
	}

	protected virtual void Update()
	{
		if (Terrain == null)
		{
			Pool(this);
		}
	}
}