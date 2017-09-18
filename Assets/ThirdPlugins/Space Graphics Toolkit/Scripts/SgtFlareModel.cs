using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtFlareModel))]
public class SgtFlareModel_Editor : SgtEditor<SgtFlareModel>
{
	protected override void OnInspector()
	{
		BeginDisabled();
			DrawDefault("Flare");
		EndDisabled();
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("")]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SgtFlareModel : MonoBehaviour
{
	public class CameraState : SgtCameraState
	{
		public Vector3    LocalPosition;
		public Quaternion LocalRotation;
		public Vector3    LocalScale;
	}

	[Tooltip("The flare this belongs to")]
	public SgtFlare Flare;
	
	[System.NonSerialized]
	private MeshFilter meshFilter;
	
	[System.NonSerialized]
	private MeshRenderer meshRenderer;

	[System.NonSerialized]
	private List<CameraState> cameraStates;
	
	public void SetMesh(Mesh mesh)
	{
		if (meshFilter == null) meshFilter = gameObject.GetComponent<MeshFilter>();

		if (meshFilter.sharedMesh != mesh)
		{
			meshFilter.sharedMesh = mesh;
		}
	}

	public void SetMaterial(Material material)
	{
		if (meshRenderer == null) meshRenderer = gameObject.GetComponent<MeshRenderer>();

		if (meshRenderer.sharedMaterial != material)
		{
			meshRenderer.sharedMaterial = material;
		}
	}

	public void SetRotation(Quaternion rotation)
	{
		transform.localRotation = rotation;
	}

	public static SgtFlareModel Create(SgtFlare flare)
	{
		var model = SgtComponentPool<SgtFlareModel>.Pop(flare.transform, "Flare Model", flare.gameObject.layer);

		model.Flare = flare;

		return model;
	}

	public static void Pool(SgtFlareModel model)
	{
		if (model != null)
		{
			model.Flare = null;

			SgtComponentPool<SgtFlareModel>.Add(model);
		}
	}

	public static void MarkForDestruction(SgtFlareModel model)
	{
		if (model != null)
		{
			model.Flare = null;

			model.gameObject.SetActive(true);
		}
	}
	
	public void Save(Camera camera)
	{
		var cameraState = SgtCameraState.Save(ref cameraStates, camera);
		
		cameraState.LocalPosition = transform.localPosition;
		cameraState.LocalRotation = transform.localRotation;
		cameraState.LocalScale    = transform.localScale   ;
	}

	public void Restore(Camera camera)
	{
		var cameraState = SgtCameraState.Restore(cameraStates, camera);

		if (cameraState != null)
		{
			transform.localPosition = cameraState.LocalPosition;
			transform.localRotation = cameraState.LocalRotation;
			transform.localScale    = cameraState.LocalScale   ;
		}
	}

	public void Revert()
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale    = Vector3.one;
	}
	
	protected virtual void Start()
	{
		if (Flare == null)
		{
			Flare = GetComponent<SgtFlare>();
		}
	}

	protected virtual void Update()
	{
		if (Flare == null)
		{
			Pool(this);
		}
	}
}