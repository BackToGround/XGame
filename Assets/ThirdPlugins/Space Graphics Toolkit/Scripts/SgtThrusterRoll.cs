using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtThrusterRoll))]
public class SgtThrusterRoll_Editor : SgtEditor<SgtThrusterRoll>
{
	protected override void OnInspector()
	{
		DrawDefault("Rotation");
	}
}
#endif

// This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics
[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Thruster Roll")]
public class SgtThrusterRoll : MonoBehaviour
{
	public class CameraState : SgtCameraState
	{
		public Quaternion LocalRotation;
	}

	[Tooltip("The rotation offset in degrees")]
	public Vector3 Rotation = new Vector3(0.0f, 90.0f, 90.0f);
	
	[System.NonSerialized]
	private List<CameraState> cameraStates;

	protected virtual void OnEnable()
	{
		Camera.onPreCull    += CameraPreCull;
		Camera.onPreRender  += CameraPreRender;
		Camera.onPostRender += CameraPostRender;
	}

	protected virtual void OnDisable()
	{
		Camera.onPreCull    -= CameraPreCull;
		Camera.onPreRender  -= CameraPreRender;
		Camera.onPostRender -= CameraPostRender;
	}
	
	private void CameraPreCull(Camera camera)
	{
		Revert();
		{
			var direction = transform.forward;
			var adjacent  = transform.position - camera.transform.position;
			var cross     = Vector3.Cross(direction, adjacent);
			
			if (cross != Vector3.zero)
			{
				transform.rotation = Quaternion.LookRotation(cross, direction) * Quaternion.Euler(Rotation);
			}
		}
		Save(camera);
	}

	private void CameraPreRender(Camera camera)
	{
		Restore(camera);
	}

	private void CameraPostRender(Camera camera)
	{
		Revert();
	}
	
	public void Save(Camera camera)
	{
		var cameraState = SgtCameraState.Save(ref cameraStates, camera);
		
		cameraState.LocalRotation = transform.localRotation;
	}

	private void Restore(Camera camera)
	{
		var cameraState = SgtCameraState.Restore(cameraStates, camera);

		if (cameraState != null)
		{
			transform.localRotation = cameraState.LocalRotation;
		}
	}
	
	public void Revert()
	{
		transform.localRotation = Quaternion.identity;
	}
}