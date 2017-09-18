using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainObject))]
public class SgtTerrainObject_Editor : SgtEditor<SgtTerrainObject>
{
	protected override void OnInspector()
	{
		DrawDefault("Pool");
		DrawDefault("ScaleMin");
		DrawDefault("ScaleMax");
		DrawDefault("AlignToNormal");

		Separator();

		BeginDisabled();
			DrawDefault("Prefab");
		EndDisabled();
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Object")]
public class SgtTerrainObject : MonoBehaviour
{
	// Called when this object is spawned (if pooling is enabled)
	public System.Action OnSpawn;

	// Called when this object is despawned (if pooling is enabled)
	public System.Action OnDespawn;

	[Tooltip("Can this particle be pooled?")]
	public bool Pool;

	[Tooltip("The minimum scale this prefab is multiplied by when spawned")]
	public float ScaleMin = 1.0f;

	[Tooltip("The maximum scale this prefab is multiplied by when spawned")]
	public float ScaleMax = 1.1f;

	[Tooltip("How far from the center the height samples are taken to align to the surface normal in world coordinates (0 = no alignment)")]
	public float AlignToNormal;

	public long X;

	public long Y;

	[Tooltip("The prefab this was instantiated from")]
	public SgtTerrainObject Prefab;

	public void Spawn(SgtTerrain terrain, SgtTerrainLevel level, SgtVector3D localPoint)
	{
		if (OnSpawn != null) OnSpawn();

		transform.SetParent(level.transform, false);

		// Snap to surface
		localPoint = terrain.GetLocalPoint(localPoint);

		// Rotate up
		var up = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f) * Vector3.up;

		// Spawn on surface
		transform.localPosition = (Vector3)localPoint;
		transform.localRotation = Quaternion.FromToRotation(up, terrain.transform.TransformDirection(transform.localPosition));
		transform.localScale    = Prefab.transform.localScale * Random.Range(ScaleMin, ScaleMax);
		//transform.rotation   = Quaternion.FromToRotation(up, terrain.transform.TransformDirection(localPosition));
		
		if (AlignToNormal != 0.0f)
		{
			//var worldRight   = transform.right   * AlignToNormal;
			//var worldForward = transform.forward * AlignToNormal;
			//var worldNormal  = terrain.GetLocalNormal(localPoint, worldRight, worldForward);

			//transform.rotation = Quaternion.FromToRotation(up, worldNormal);
		}
	}

	public void Despawn()
	{
		if (OnDespawn != null) OnDespawn();

		if (Pool == true)
		{
			SgtComponentPool<SgtTerrainObject>.Add(this);
		}
		else
		{
			SgtHelper.Destroy(gameObject);
		}
	}
}
