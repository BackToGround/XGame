using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtMouseLookFree))]
public class SgtMouseLookFree_Editor : SgtEditor<SgtMouseLookFree>
{
	protected override void OnInspector()
	{
		DrawDefault("Camera");
		DrawDefault("Require");
		DrawDefault("Sensitivity");
		BeginError(Any(t => t.Dampening < 0.0f));
			DrawDefault("Dampening");
		EndError();
	}
}
#endif

// This component handles mouselook when attached to the camera
[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Mouse Look")]
public class SgtMouseLookFree : MonoBehaviour
{
	[Tooltip("The camera whose FOV will be used to scale the sensitivity (default = MainCamera)")]
	public Camera Camera;

	[Tooltip("The key that needs to be held down to look")]
	public KeyCode Require = KeyCode.Mouse0;

	[Tooltip("How quickly this rotates relative to the mouse movement")]
	public float Sensitivity = 2.0f;

	[Tooltip("The speed at which this approaches the target rotation")]
	public float Dampening = 10.0f;

	// Remaining euler rotation
	public Vector3 Remaining;

	protected virtual void Update()
	{
		var sensitivity = Sensitivity;

		var camera = Camera;

		if (camera == null)
		{
			camera = Camera.main;
		}

		if (camera != null && camera.orthographic == false)
		{
			sensitivity *= camera.fieldOfView / 60.0f;
		}

		if (Require == KeyCode.None || Input.GetKey(Require) == true)
		{
			Remaining.x -= Input.GetAxisRaw("Mouse Y") * sensitivity;
			Remaining.y += Input.GetAxisRaw("Mouse X") * sensitivity;
		}

		var dampened = SgtHelper.Dampen3(Remaining, Vector3.zero, Dampening, Time.deltaTime, 0.1f);
		var delta    = Remaining - dampened;

		Remaining = dampened;

		transform.Rotate(delta);
	}
}
