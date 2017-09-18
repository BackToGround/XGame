using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtAuroraModel))]
public class SgtAuroraModel_Editor : SgtEditor<SgtAuroraModel>
{
	protected override void OnInspector()
	{
		BeginDisabled();
			DrawDefault("Aurora");
		EndDisabled();
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("")]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SgtAuroraModel : MonoBehaviour
{
	public class CameraState : SgtCameraState
	{
		public Vector3 LocalPosition;
	}

	[Tooltip("The aurora this belongs to")]
	public SgtAurora Aurora;

	[System.NonSerialized]
	private MeshFilter meshFilter;

	[System.NonSerialized]
	private MeshRenderer meshRenderer;

	[System.NonSerialized]
	private Mesh mesh;

	[System.NonSerialized]
	private Material material;

	[System.NonSerialized]
	private List<CameraState> cameraStates;
	
	public void SetMesh(Mesh newMesh)
	{
		if (newMesh != mesh)
		{
			if (meshFilter == null) meshFilter = gameObject.GetComponent<MeshFilter>();
			
			mesh = meshFilter.sharedMesh = newMesh;
		}
	}

	public void SetMaterial(Material newMaterial)
	{
		if (newMaterial != material)
		{
			if (meshRenderer == null) meshRenderer = gameObject.GetComponent<MeshRenderer>();
			
			material = meshRenderer.sharedMaterial = newMaterial;
		}
	}
	
	public void Save(Camera camera)
	{
		var cameraState = SgtCameraState.Save(ref cameraStates, camera);

		cameraState.LocalPosition = transform.localPosition;
	}
	
	public void Restore(Camera camera)
	{
		var cameraState = SgtCameraState.Restore(cameraStates, camera);

		if (cameraState != null)
		{
			transform.localPosition = cameraState.LocalPosition;
		}
	}

	public void Revert()
	{
		transform.localPosition = Vector3.zero;
	}
	
	public static SgtAuroraModel Create(SgtAurora aurora)
	{
		var model = SgtComponentPool<SgtAuroraModel>.Pop(aurora.transform, "Plane", aurora.gameObject.layer);

		model.Aurora = aurora;

		return model;
	}

	public static void Pool(SgtAuroraModel model)
	{
		if (model != null)
		{
			model.Aurora = null;

			SgtComponentPool<SgtAuroraModel>.Add(model);
		}
	}

	public static void MarkForDestruction(SgtAuroraModel model)
	{
		if (model != null)
		{
			model.Aurora = null;

			model.gameObject.SetActive(true);
		}
	}

	protected virtual void Update()
	{
		if (Aurora == null)
		{
			Pool(this);
		}
	}
}