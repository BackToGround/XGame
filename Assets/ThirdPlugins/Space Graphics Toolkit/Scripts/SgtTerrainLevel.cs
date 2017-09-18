using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainLevel))]
public class SgtTerrainLevel_Editor : SgtEditor<SgtTerrainLevel>
{
	protected override void OnInspector()
	{
		BeginDisabled();
			DrawDefault("Face");
			DrawDefault("Index");
		EndDisabled();
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SgtTerrainLevel : MonoBehaviour
{
	[Tooltip("The terrain face this belongs to")]
	public SgtTerrainFace Face;

	[Tooltip("The index of this LOD level")]
	public int Index;

	// Marked for sweeping?
	[System.NonSerialized]
	public bool Marked;

	[System.NonSerialized]
	private long size;

	[System.NonSerialized]
	private SgtRectL basicRegion;

	[System.NonSerialized]
	private SgtRectL outerRegion;

	[System.NonSerialized]
	private SgtRectL innerRegion;

	[System.NonSerialized]
	private Mesh mesh;

	[System.NonSerialized]
	private Mesh colliderMesh;

	[System.NonSerialized]
	private MeshFilter meshFilter;

	[System.NonSerialized]
	private MeshRenderer meshRenderer;

	[System.NonSerialized]
	private MeshCollider meshCollider;

	[System.NonSerialized]
	private Vector3[] positions;

	[System.NonSerialized]
	private Vector3[] normals;

	[System.NonSerialized]
	private Vector2[] coords1;

	[System.NonSerialized]
	private Vector2[] coords2;

	[System.NonSerialized]
	private static List<int> indices = new List<int>();

	public Mesh Mesh
	{
		get
		{
			return mesh;
		}
	}

	public void UpdateMaterials()
	{
		if (Face != null)
		{
			var terrain = Face.Terrain;

			if (terrain != null)
			{
				if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

				meshRenderer.sharedMaterials = terrain.GetMaterials(this);
			}
		}
	}

	public void Dirty()
	{
		basicRegion.Clear();
		innerRegion.Clear();
		outerRegion.Clear();
	}

	public void PoolMeshNow()
	{
		if (mesh != null)
		{
			mesh.Clear(false);

			mesh = SgtObjectPool<Mesh>.Add(mesh);
		}
	}

	public void PoolColliderMeshNow(bool destroyCollider = true)
	{
		if (colliderMesh != null)
		{
			if (destroyCollider == true && meshCollider != null)
			{
				meshCollider = SgtHelper.Destroy(meshCollider);
			}

			colliderMesh.Clear(false);

			colliderMesh = SgtObjectPool<Mesh>.Add(colliderMesh);
		}
	}

	public Mesh GetMesh()
	{
		if (mesh == null)
		{
			mesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();
#if UNITY_EDITOR
			mesh.hideFlags = HideFlags.DontSave;
#endif
			mesh.name = "Terrain";
		}

		if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

		meshFilter.sharedMesh = mesh;

		return mesh;
	}

	public Mesh GetColliderMesh()
	{
		if (colliderMesh == null)
		{
			colliderMesh = SgtObjectPool<Mesh>.Pop() ?? new Mesh();
#if UNITY_EDITOR
			colliderMesh.hideFlags = HideFlags.DontSave;
#endif
			colliderMesh.name = "Terrain (Capped)";
		}

		return colliderMesh;
	}

	public static int Wrap(long x, long y, long size)
	{
		x %= size; if (x < 0) x += size;
		y %= size; if (y < 0) y += size;

		return (int)(x + y * size);
	}

	public void Write(SgtTerrain.Ring ring)
	{
		if (ring.Outer == outerRegion && ring.Inner == innerRegion)
		{
			return;
		}

		size = ring.Limit + 3;

		var terrain        = Face.Terrain;
		var total          = size * size;
		var newBasicRegion = ring.Outer.GetExpanded(1);

		if (positions == null || positions.Length != total)
		{
			basicRegion.Clear();
			outerRegion.Clear();

			positions = new Vector3[total];
			normals   = new Vector3[total];
			coords1   = new Vector2[total];
			coords2   = new Vector2[total];
		}

		// Write position & uv data
		var stepX = ring.ExtentsX / ring.Detail;
		var stepY = ring.ExtentsY / ring.Detail;

		for (var y = newBasicRegion.minY; y <= newBasicRegion.maxY; y++)
		{
			for (var x = newBasicRegion.minX; x <= newBasicRegion.maxX; x++)
			{
				if (basicRegion.Contains(x, y) == false)
				{
					var index     = Wrap(x, y, size);
					var point     = ring.Center + x * stepX + y * stepY;
					var coord1U   = (x / (double)ring.Detail + 1.0) / 2.0; // 0 .. 1
					var coord1V   = (y / (double)ring.Detail + 1.0) / 2.0; // 0 .. 1
					var coord2U   = x / (double)ring.Limit; // -u .. u
					var coord2V   = y / (double)ring.Limit; // -v .. v
					var height    = terrain.GetLocalHeight(point);
					var direction = point.normalized;
					var position  = direction * height;

					positions[index] = (Vector3)position;

					coords1[index] = new Vector2((float)coord1U, (float)coord1V);
					coords2[index] = new Vector2((float)coord2U, (float)coord2V);
				}
			}
		}

		var parent = Face.LastLevel;

		WriteNormals(ring.Outer, outerRegion);

		if (parent != null)
		{
			// Write edges from parent to avoid normal seams from resolution shift
			if (ring.Outer.minX > -ring.Detail) WriteParentNormals(parent, ring.Outer.minX, ring.Outer.minY, ring.Outer.minX, ring.Outer.maxY); // Left
			if (ring.Outer.maxX <  ring.Detail) WriteParentNormals(parent, ring.Outer.maxX, ring.Outer.minY, ring.Outer.maxX, ring.Outer.maxY); // Right
			if (ring.Outer.minY > -ring.Detail) WriteParentNormals(parent, ring.Outer.minX, ring.Outer.minY, ring.Outer.maxX, ring.Outer.minY); // Bottom
			if (ring.Outer.maxY <  ring.Detail) WriteParentNormals(parent, ring.Outer.minX, ring.Outer.maxY, ring.Outer.maxX, ring.Outer.maxY); // Top
		}

		// Write indices
		WriteIndices(ring.Outer, ring.Inner, ring.Detail, size);

		var mesh = GetMesh();

		mesh.Clear();
		mesh.vertices = positions;
		mesh.normals  = normals;
		mesh.uv       = coords1;
		mesh.uv2      = coords2;
		mesh.SetTriangles(indices, 0);

		mesh.bounds = terrain.GetBounds();

		if (terrain.MaxColliderDepth > Index)
		{
			if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
			if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();

			// Is this the last collider, but not the last LOD level?
			if (terrain.MaxColliderDepth == Index + 1 && ring.Inner.SizeX > 0 && ring.Inner.SizeY > 0)
			{
				var colliderMesh = GetColliderMesh();

				WriteIndices(ring.Outer, default(SgtRectL), ring.Detail, size);

				colliderMesh.Clear();
				colliderMesh.vertices = positions;
				colliderMesh.SetTriangles(indices, 0);

				meshCollider.sharedMesh = colliderMesh;
			}
			// If not, use the visual mesh
			else
			{
				PoolColliderMeshNow(false);

				meshCollider.sharedMesh = mesh;
			}
		}
		else
		{
			PoolColliderMeshNow();
		}

		basicRegion = newBasicRegion;
		outerRegion = ring.Outer;
		innerRegion = ring.Inner;

		Face.LastLevel = this;

		if (terrain.OnCalculateLevel != null)
		{
			terrain.OnCalculateLevel(this, ring);
		}
	}

	public static SgtTerrainLevel Create(string name, int layer, Transform parent)
	{
		return SgtComponentPool<SgtTerrainLevel>.Pop(parent, name, layer);
	}

	public static SgtTerrainLevel Pool(SgtTerrainLevel level)
	{
		if (level != null)
		{
			level.Face = null;
			
			level.PoolMeshNow();
			level.PoolColliderMeshNow();

			SgtComponentPool<SgtTerrainLevel>.Add(level);
		}

		return null;
	}

	public static SgtTerrainLevel MarkForDestruction(SgtTerrainLevel level)
	{
		if (level != null)
		{
			level.Face = null;

			level.gameObject.SetActive(true);
		}

		return null;
	}

	protected virtual void OnEnable()
	{
		Dirty();
		UpdateMaterials();
	}

	protected virtual void OnDestroy()
	{
		PoolMeshNow();
	}

	protected virtual void Update()
	{
		if (Face == null)
		{
			Pool(this);
		}
	}

	private void WriteNormals(SgtRectL outer, SgtRectL inner)
	{
		inner.ClampTo(outer);

		WriteNormals(outer.minX, inner.maxY, outer.maxX, outer.maxY); // Top
		WriteNormals(outer.minX, outer.minY, outer.maxX, inner.maxY); // Bottom
		WriteNormals(outer.minX, inner.minX, inner.minX, inner.maxY); // Left
		WriteNormals(inner.maxX, inner.minX, inner.maxX, inner.maxY); // Right
	}

	private void WriteNormals(long minX, long minY, long maxX, long maxY)
	{
		for (var y = minY; y <= maxY; y++)
		{
			for (var x = minX; x <= maxX; x++)
			{
				var index     = Wrap(x, y, size);
				var positionL = positions[Wrap(x - 1, y, size)];
				var positionR = positions[Wrap(x + 1, y, size)];
				var positionB = positions[Wrap(x, y - 1, size)];
				var positionT = positions[Wrap(x, y + 1, size)];
				var normal    = -Vector3.Cross(positionR - positionL, positionT - positionB);

				normals[index] = normal.normalized;
			}
		}
	}

	private void WriteParentNormals(SgtTerrainLevel parent, long minX, long minY, long maxX, long maxY)
	{
		for (var y = minY; y <= maxY; y++)
		{
			for (var x = minX; x <= maxX; x++)
			{
				var index   = Wrap(x, y, size);
				var parentX = x / 2.0;
				var parentY = y / 2.0;
				var normal  = parent.normals[Wrap((long)parentX, (long)parentY, parent.size)];

				normals[index] = normal;
			}
		}
	}

	private void WriteIndices(SgtRectL outer, SgtRectL inner, long detail, long size)
	{
		SgtHelper.ClearCapacity(indices, 1024);

		for (var y = outer.minY; y < outer.maxY; y++)
		{
			for (var x = outer.minX; x < outer.maxX; x++)
			{
				if (x >= inner.minX && x < inner.maxX && y >= inner.minY && y < inner.maxY)
				{
					continue;
				}

				var aX = x    ; var aY = y    ;
				var bX = x + 1; var bY = y    ;
				var cX = x    ; var cY = y + 1;
				var dX = x + 1; var dY = y + 1;

				if (y == outer.minY && y > -detail)
				{
					if (x % 2 == 0) bX -= 1; else aX -= 1;
				}

				if (y == outer.maxY - 1 && y <  detail - 1)
				{
					if (x % 2 == 0) dX += 1; else cX += 1;
				}

				if (x == outer.minX && x > -detail)
				{
					if (y % 2 == 0) cY -= 1; else aY -= 1;
				}

				if (x == outer.maxX - 1 && x <  detail - 1)
				{
					if (y % 2 == 0) dY += 1; else bY += 1;
				}

				var a = Wrap(aX, aY, size);
				var b = Wrap(bX, bY, size);
				var c = Wrap(cX, cY, size);
				var d = Wrap(dX, dY, size);
				/*
				var posA = positions[a];
				var posB = positions[b];
				var posC = positions[c];
				var posD = positions[d];

				var areaA = Area(posA, posB, posC) + Area(posD, posC, posB);
				var areaB = Area(posA, posB, posD) + Area(posD, posC, posA);

				if (a < b)
				{
					var t = a;

					a = b;
					b = d;
					d = c;
					c = t;
				}*/
				
				indices.Add(a); indices.Add(c); indices.Add(b);
				indices.Add(d); indices.Add(b); indices.Add(c);
			}
		}
	}

	private static float Area(Vector3 a, Vector3 b, Vector3 c)
	{
		return Vector3.Cross(a - b, a - c).magnitude;
	}
}