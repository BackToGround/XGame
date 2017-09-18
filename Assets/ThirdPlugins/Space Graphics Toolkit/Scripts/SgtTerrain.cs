using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrain))]
public class SgtTerrain_Editor : SgtEditor<SgtTerrain>
{
	protected override void OnInspector()
	{
		var updateMeshes    = false;
		var updateMaterials = false;
		var dirtyMeshes     = false;

		DrawDefault("Material", ref updateMaterials);
		DrawDefault("Atmosphere", ref updateMaterials);
		DrawDefault("BoundsOffset"); // Updated automatically

		Separator();

		BeginError(Any(t => t.Target == null));
			DrawDefault("Target", ref updateMeshes);
		EndError();
		BeginError(Any(t => t.Radius <= 0.0));
			DrawDefault("Radius", ref dirtyMeshes, ref updateMeshes);
		EndError();
		DrawDefault("Detail", ref dirtyMeshes, ref updateMeshes);
		BeginError(Any(t => t.MaxColliderDepth < 0 || (t.Distances != null && t.MaxColliderDepth > t.Distances.Count)));
			DrawDefault("MaxColliderDepth", ref dirtyMeshes, ref updateMeshes);
		EndError();
		BeginError(Any(t => DistancesValid(t.Distances) == false));
			DrawDefault("Distances", ref dirtyMeshes, ref updateMeshes);
		EndError();

		Separator();

		if (Button("Add Distance") == true)
		{
			Each(t => AddDistance(ref t.Distances));
		}

		if (dirtyMeshes     == true) DirtyEach(t => { serializedObject.ApplyModifiedProperties(); t.DirtyMeshes(); });
		if (updateMeshes    == true) DirtyEach(t => t.UpdateMeshes   ());
		if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
	}

	private static void AddDistance(ref List<double> distances)
	{
		if (distances == null)
		{
			distances = new List<double>();
		}

		var lastDistance = 2.0;

		if (distances.Count > 0)
		{
			var distance = distances[distances.Count - 1];

			if (distance > 0.0)
			{
				lastDistance = distance;
			}
		}

		distances.Add(lastDistance * 0.5);
	}

	private static bool DistancesValid(List<double> distances)
	{
		if (distances == null)
		{
			return false;
		}

		var lastDistance = double.PositiveInfinity;

		for (var i = 0; i < distances.Count; i++)
		{
			var distance = distances[i];

			if (distance <= 0.0 || distance >= lastDistance)
			{
				return false;
			}

			lastDistance = distance;
		}

		return true;
	}
}
#endif

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain")]
public partial class SgtTerrain : MonoBehaviour
{
	public class Ring
	{
		public long Limit;
		public long Detail;

		public SgtRectL Inner;

		public SgtRectL Outer;

		public SgtVector3D Center;

		public SgtVector3D ExtentsX;

		public SgtVector3D ExtentsY;
	}

	public class Shell
	{
		public long Limit; // Clamped maximum size of the shell
		public long Detail; // Virtual grid size of the shell
		public SgtBoundsL Inner;
		public SgtBoundsL Outer;
	}

	public delegate void CalculateHeightDelegate(SgtVector3D localPosition, ref float displacement);

	public delegate void CalculateNormalDelegate(SgtVector3D localPosition, ref Vector3 normal);

	//public delegate void CalculateColorDelegate(SgtVector3D localPosition, float height, ref Color color);

	public delegate void CalculateMaterialDelegate(SgtTerrainLevel level, ref Material material);

	public delegate void CalculateQuadDelegate(SgtTerrainLevel level, Ring ring);

	// All active and enabled terrains in the scene
	public static List<SgtTerrain> AllTerrains = new List<SgtTerrain>();

	[Tooltip("The base material for the terrain (can be overirdden by each face)")]
	public Material Material;

	[Tooltip("The atmosphere applied to this terrain")]
	public SgtAtmosphere Atmosphere;

	[Tooltip("The distance the bounds are shifted toward the current camera")]
	public float BoundsOffset;

	[Tooltip("The transform the LOD will be based around (e.g. your main camera)")]
	public Transform Target;

	[Tooltip("The base radius of the terrain")]
	public float Radius = 1.0f;

	[Tooltip("The detail of each LOD level (NOTE: If you set this too high with high LOD distances, you will get errors)")]
	[Range(1, 25)]
	public int Detail = 5;

	[Tooltip("The maximum LOD depth that colliders will be generated for (0 = none)")]
	public int MaxColliderDepth;

	[Tooltip("The LOD distances in local space, these should be sorted from high to low")]
	public List<double> Distances;

	// Each face handles a cube face
	public SgtTerrainFace NegativeX;
	public SgtTerrainFace NegativeY;
	public SgtTerrainFace NegativeZ;
	public SgtTerrainFace PositiveX;
	public SgtTerrainFace PositiveY;
	public SgtTerrainFace PositiveZ;

	// Called when the displacement of a specific point is being calculated
	public CalculateHeightDelegate OnCalculateHeight;

	// Called when a level's area changes
	public CalculateQuadDelegate OnCalculateLevel;

	// Called when a level's material needs to be set
	public CalculateMaterialDelegate OnCalculateMaterial;

	// Called when a vertex color needs to be calculated
	//public CalculateColorDelegate OnCalculateColor;

	// Shells are recalculated every update so they can be static
	private static List<Shell> shells = new List<Shell>();

	// Used to assign renderer.sharedMaterials
	private static Material[] materials1 = new Material[1];
	private static Material[] materials2 = new Material[2];

	private SgtVector3D CalculateTarget()
	{
		var point = new SgtVector3D(transform.InverseTransformPoint(Target.position));

		// Deform by cube shape
		if (point.sqrMagnitude > 0.0)
		{
			var cube = InvCube(point.normalized);

			point *= cube.magnitude;
		}

		// Deform by terrain displacement
		var height = GetLocalHeight(point);

		if (height > 0.0f)
		{
			point /= height;
		}

		return point;
	}

	public Material GetMaterial(SgtTerrainLevel level)
	{
		var finalMaterial = Material;

		if (level != null && level.Face != null && level.Face.Material != null)
		{
			finalMaterial = level.Face.Material;
		}

		if (OnCalculateMaterial != null)
		{
			OnCalculateMaterial(level, ref finalMaterial);
		}

		return finalMaterial;
	}

	public Material[] GetMaterials(SgtTerrainLevel level)
	{
		var finalMaterial = GetMaterial(level);

		if (SgtHelper.Enabled(Atmosphere) == true)
		{
			materials2[0] = finalMaterial;
			materials2[1] = Atmosphere.InnerMaterial;

			return materials2;
		}
		else
		{
			materials1[0] = finalMaterial;

			return materials1;
		}
	}

	// Gets the surface height under the input point in local space
	public float GetLocalHeight(SgtVector3D localPoint)
	{
		var height = Radius;

		if (OnCalculateHeight != null)
		{
			OnCalculateHeight(localPoint, ref height);
		}

		return height;
	}

	// Gets the surface height under the input point in local space
	public float GetLocalHeight(Vector3 localPoint)
	{
		return GetLocalHeight(new SgtVector3D(localPoint));
	}

	// Gets the surface height under the input point in world space
	public float GetWorldHeight(Vector3 worldPoint)
	{
		worldPoint = GetWorldPoint(worldPoint);

		return Vector3.Distance(worldPoint, transform.position);
	}

	// Gets the surface point under the input point in local space
	public SgtVector3D GetLocalPoint(SgtVector3D localPoint)
	{
		return localPoint.normalized * GetLocalHeight(localPoint);
	}

	// Gets the surface point under the input point in local space
	public Vector3 GetLocalPoint(Vector3 localPoint)
	{
		return localPoint.normalized * GetLocalHeight(localPoint);
	}

	// Gets the surface point under the input point in world space
	public Vector3 GetWorldPoint(Vector3 worldPoint, float offset = 0.0f)
	{
		var localPoint = transform.InverseTransformPoint(worldPoint);

		localPoint = GetLocalPoint(localPoint);

		var newWorldPoint = transform.TransformPoint(localPoint);

		if (offset != 0.0f)
		{
			var worldNormal = worldPoint - transform.position;

			newWorldPoint += worldNormal.normalized * offset;
		}

		return newWorldPoint;
	}

	// Gets the normal under the input point in local space based on 2 samples
	public Vector3 GetLocalNormalFast(SgtVector3D localPoint, SgtVector3D right, SgtVector3D forward)
	{
		var a = GetLocalPoint(localPoint);
		var b = GetLocalPoint(localPoint + right);
		var c = GetLocalPoint(localPoint + forward);

		return (Vector3)SgtVector3D.Cross(a - b, c - a).normalized;
	}

	// Gets the normal under the input point in local space based on 2 samples, and assumes localPoint is already on the surface
	public Vector3 GetLocalNormalFast2(SgtVector3D localPoint, SgtVector3D right, SgtVector3D forward)
	{
		var b = GetLocalPoint(localPoint + right);
		var c = GetLocalPoint(localPoint + forward);

		return (Vector3)SgtVector3D.Cross(localPoint - b, c - localPoint).normalized;
	}

	// Gets the normal under the input point in local space based on 4 samples
	public Vector3 GetLocalNormal(SgtVector3D localPoint, SgtVector3D right, SgtVector3D forward)
	{
		var a = GetLocalPoint(localPoint - right);
		var b = GetLocalPoint(localPoint + right);
		var c = GetLocalPoint(localPoint - forward);
		var d = GetLocalPoint(localPoint + forward);

		return (Vector3)SgtVector3D.Cross(a - b, d - c).normalized;
	}

	// Gets the normal under the input point in local space based on 4 samples
	public Vector3 GetLocalNormal2(SgtVector3D localPoint, SgtVector3D right, SgtVector3D forward)
	{
		var a  = GetLocalPoint(localPoint - right);
		var b  = GetLocalPoint(localPoint + right);
		var c  = GetLocalPoint(localPoint - forward);
		var d  = GetLocalPoint(localPoint + forward);

		return (Vector3)SgtVector3D.Cross(a - b, d - c).normalized;
	}

	/*
	public Color GetLocalTangentNormalFast(SgtVector3D localPoint, SgtVector3D right, SgtVector3D forward)
	{
		var a  = GetLocalPoint(localPoint - right);
		var b  = GetLocalPoint(localPoint + right);
		var c  = GetLocalPoint(localPoint - forward);
		var d  = GetLocalPoint(localPoint + forward);

		var abM = (a - b).magnitude;
		var abH = a.magnitude - b.magnitude;
		var cdM = (c - d).magnitude;
		var cdH = c.magnitude - d.magnitude;

		var x = abH / abM;
		var y = cdH / cdM;
		var z = x * x + y * y;

		z = System.Math.Max(z, 0.0);
		z = System.Math.Min(z, 1.0);
		z = System.Math.Sqrt(1.0 - z);

		return new Color((float)x, (float)y, (float)z, (float)x);
	}
	*/

	// Gets the terrain normal ignoring the displacement
	public Vector3 GetWorldNormal(Vector3 worldPoint)
	{
		return (transform.position - worldPoint).normalized;
	}

	// Gets the surface normal under the input point in world space
	public Vector3 GetWorldNormal(Vector3 worldPoint, Vector3 right, Vector3 forward)
	{
		var a  = GetWorldPoint(worldPoint);
		var b  = GetWorldPoint(worldPoint + right);
		var c  = GetWorldPoint(worldPoint + forward);
		var ab = a - b;
		var ac = c - a;

		return Vector3.Cross(ab, ac);
	}

	// Gets the surface normal under the input point in world space
	public Vector3 GetWorldNormalFast(Vector3 worldPoint, Vector3 right, Vector3 forward)
	{
		var b = GetWorldPoint(worldPoint + right);
		var c = GetWorldPoint(worldPoint + forward);
		var ab = worldPoint - b;
		var ac = c - worldPoint;

		return Vector3.Cross(ab, ac);
	}

	[ContextMenu("Dirty Meshes")]
	public void DirtyMeshes()
	{
		if (NegativeX != null) NegativeX.Dirty();
		if (NegativeY != null) NegativeY.Dirty();
		if (NegativeZ != null) NegativeZ.Dirty();
		if (PositiveX != null) PositiveX.Dirty();
		if (PositiveY != null) PositiveY.Dirty();
		if (PositiveZ != null) PositiveZ.Dirty();
	}

	[ContextMenu("Update Materials")]
	public void UpdateMaterials()
	{
		if (NegativeX != null) NegativeX.UpdateMaterials();
		if (NegativeY != null) NegativeY.UpdateMaterials();
		if (NegativeZ != null) NegativeZ.UpdateMaterials();
		if (PositiveX != null) PositiveX.UpdateMaterials();
		if (PositiveY != null) PositiveY.UpdateMaterials();
		if (PositiveZ != null) PositiveZ.UpdateMaterials();
	}

	public void UpdateMeshes()
	{
		if (Detail > 0)
		{
			if (NegativeX == null) NegativeX = CreateFace(CubemapFace.NegativeX);
			if (NegativeY == null) NegativeY = CreateFace(CubemapFace.NegativeY);
			if (NegativeZ == null) NegativeZ = CreateFace(CubemapFace.NegativeZ);
			if (PositiveX == null) PositiveX = CreateFace(CubemapFace.PositiveX);
			if (PositiveY == null) PositiveY = CreateFace(CubemapFace.PositiveY);
			if (PositiveZ == null) PositiveZ = CreateFace(CubemapFace.PositiveZ);

			UpdateShells();

			NegativeX.Mark();
			NegativeY.Mark();
			NegativeZ.Mark();
			PositiveX.Mark();
			PositiveY.Mark();
			PositiveZ.Mark();

			for (var i = 0; i < shells.Count; i++)
			{
				var shell  = shells[i];
				var detail = shell.Detail;

				NegativeX.Write(new SgtBoundsL(-detail, -detail, -detail, -detail + 1,  detail    ,  detail    ), shell, 0, i);
				NegativeY.Write(new SgtBoundsL(-detail, -detail, -detail,  detail    , -detail + 1,  detail    ), shell, 1, i);
				NegativeZ.Write(new SgtBoundsL(-detail, -detail, -detail,  detail    ,  detail    , -detail + 1), shell, 2, i);

				PositiveX.Write(new SgtBoundsL( detail - 1, -detail    , -detail    , detail, detail, detail), shell, 3, i);
				PositiveY.Write(new SgtBoundsL(-detail    ,  detail - 1, -detail    , detail, detail, detail), shell, 4, i);
				PositiveZ.Write(new SgtBoundsL(-detail    , -detail    ,  detail - 1, detail, detail, detail), shell, 5, i);
			}

			NegativeX.Sweep();
			NegativeY.Sweep();
			NegativeZ.Sweep();
			PositiveX.Sweep();
			PositiveY.Sweep();
			PositiveZ.Sweep();
		}
	}

	public Bounds GetBounds()
	{
		var center = Vector3.zero;
		var size   = new Vector3(Radius * 2.0f, Radius * 2.0f, Radius * 2.0f);

		return new Bounds(center, size);
	}

	public Bounds GetBounds(Camera camera)
	{
		var center = Vector3.zero;

		if (BoundsOffset != 0.0f)
		{
			center = transform.InverseTransformPoint(camera.transform.position).normalized * BoundsOffset;
		}

		var size = new Vector3(Radius * 2.0f, Radius * 2.0f, Radius * 2.0f);

		return new Bounds(center, size);
	}

	public static SgtVector3D Cube(SgtVector3D v)
	{
		return v.normalized;
	}

	public static SgtVector3D InvCube(SgtVector3D v)
	{
		var a = new SgtVector3D(System.Math.Abs(v.x), System.Math.Abs(v.y) , System.Math.Abs(v.z));

		if (a.x > a.y && a.x > a.z)
		{
			return v / a.x;
		}
		else if (a.y > a.x && a.y > a.z)
		{
			return v / a.y;
		}
		else
		{
			return v / a.z;
		}
	}

	public static SgtTerrain CreateTerrain(int layer = 0, Transform parent = null)
	{
		return CreateTerrain(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtTerrain CreateTerrain(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = SgtHelper.CreateGameObject("Terrain", layer, parent, localPosition, localRotation, localScale);
		var terrain    = gameObject.AddComponent<SgtTerrain>();

		return terrain;
	}

#if UNITY_EDITOR
	[MenuItem(SgtHelper.GameObjectMenuPrefix + "Terrain", false, 10)]
	public static void CreateTerrainMenuItem()
	{
		var parent  = SgtHelper.GetSelectedParent();
		var terrain = CreateTerrain(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(terrain);
	}
#endif

	private void ClearShells()
	{
		for (var i = shells.Count - 1; i >= 0; i--)
		{
			SgtClassPool<Shell>.Add(shells[i]);
		}

		shells.Clear();
	}

	private Shell AddShell(long detail)
	{
		var shell = SgtClassPool<Shell>.Pop() ?? new Shell();

		shell.Detail = detail;
		shell.Inner  = default(SgtBoundsL);

		shells.Add(shell);

		return shell;
	}

	private long GetSize(double position, double distance, long detail)
	{
		position  = System.Math.Abs(position) - 1.0;
		position /= distance;

		if (position >= 1.0)
		{
			return 0;
		}

		position = position * position * position * position;
		position = System.Math.Cos(position * System.Math.PI / 2);

		return (long)System.Math.Ceiling(distance * detail * position);
	}

	private void UpdateShells()
	{
		ClearShells();

		if (Target != null && Detail > 0)
		{
			var target = CalculateTarget();
			var detail = (long)Detail;
			var shell  = AddShell(detail);

			shell.Limit = detail * 2;
			shell.Outer = new SgtBoundsL(-detail, -detail, -detail, detail, detail, detail);

			if (Distances != null)
			{
				var lastDistance = double.PositiveInfinity;
				var axis         = CalculateAxis(target);

				for (var i = 0; i < Distances.Count; i++)
				{
					var distance = Distances[i];

					if (distance > 0.0 && distance < lastDistance)
					{
						var size  = (long)System.Math.Ceiling(distance * detail);
						var cellX = (long)(target.x * detail);
						var cellY = (long)(target.y * detail);
						var cellZ = (long)(target.z * detail);

						// Adjust shell size so each LOD smoothly expands in, rather than fully popping into view
						var sizeX = size;
						var sizeY = size;
						var sizeZ = size;

						switch (axis)
						{
							case 0: sizeY = sizeZ = GetSize(target.x, distance, detail); break;
							case 1: sizeX = sizeZ = GetSize(target.y, distance, detail); break;
							case 2: sizeX = sizeY = GetSize(target.z, distance, detail); break;
						}

						shell.Inner = new SgtBoundsL(cellX - sizeX , cellY - sizeY, cellZ - sizeZ, cellX + sizeX , cellY + sizeY, cellZ + sizeZ);

						detail *= 2;
						size   *= 2;

						var newShell = AddShell(detail);

						newShell.Limit = System.Math.Min(size * 2, detail * 2);
						newShell.Outer = shell.Inner.Double;

						shell        = newShell;
						lastDistance = distance;
					}
				}
			}
		}
	}

	protected virtual void OnEnable()
	{
		AllTerrains.Add(this);

		Camera.onPreRender += PreRender;

		if (NegativeX != null) NegativeX.gameObject.SetActive(true);
		if (NegativeY != null) NegativeY.gameObject.SetActive(true);
		if (NegativeZ != null) NegativeZ.gameObject.SetActive(true);
		if (PositiveX != null) PositiveX.gameObject.SetActive(true);
		if (PositiveY != null) PositiveY.gameObject.SetActive(true);
		if (PositiveZ != null) PositiveZ.gameObject.SetActive(true);

		UpdateMaterials();
	}

	protected virtual void Update()
	{
		UpdateMeshes();
	}

	protected virtual void OnDisable()
	{
		AllTerrains.Remove(this);

		Camera.onPreRender -= PreRender;

		if (NegativeX != null) NegativeX.gameObject.SetActive(false);
		if (NegativeY != null) NegativeY.gameObject.SetActive(false);
		if (NegativeZ != null) NegativeZ.gameObject.SetActive(false);
		if (PositiveX != null) PositiveX.gameObject.SetActive(false);
		if (PositiveY != null) PositiveY.gameObject.SetActive(false);
		if (PositiveZ != null) PositiveZ.gameObject.SetActive(false);
	}

	protected virtual void OnDestroy()
	{
		SgtTerrainFace.MarkForDestruction(NegativeX);
		SgtTerrainFace.MarkForDestruction(NegativeY);
		SgtTerrainFace.MarkForDestruction(NegativeZ);
		SgtTerrainFace.MarkForDestruction(PositiveX);
		SgtTerrainFace.MarkForDestruction(PositiveY);
		SgtTerrainFace.MarkForDestruction(PositiveZ);
	}

	private void PreRender(Camera camera)
	{
		var bounds = GetBounds(camera);

		if (NegativeX != null) NegativeX.UpdateBounds(bounds);
		if (NegativeY != null) NegativeY.UpdateBounds(bounds);
		if (NegativeZ != null) NegativeZ.UpdateBounds(bounds);
		if (PositiveX != null) PositiveX.UpdateBounds(bounds);
		if (PositiveY != null) PositiveY.UpdateBounds(bounds);
		if (PositiveZ != null) PositiveZ.UpdateBounds(bounds);
	}

	private SgtTerrainFace CreateFace(CubemapFace side)
	{
		var face = SgtTerrainFace.Create(side.ToString(), gameObject.layer, transform);

		face.Terrain = this;
		face.Side    = side;

		return face;
	}

	private int CalculateAxis(SgtVector3D vector)
	{
		vector.x = System.Math.Abs(vector.x);
		vector.y = System.Math.Abs(vector.y);
		vector.z = System.Math.Abs(vector.z);

		if (vector.y > vector.x && vector.y > vector.z) return 1;
		if (vector.z > vector.x && vector.z > vector.y) return 2;

		return 0;
	}
}