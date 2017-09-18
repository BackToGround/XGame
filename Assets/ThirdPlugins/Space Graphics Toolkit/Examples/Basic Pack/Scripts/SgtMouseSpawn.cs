using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtMouseSpawn))]
public class SgtMouseSpawn_Editor : SgtEditor<SgtMouseSpawn>
{
	protected override void OnInspector()
	{
		DrawDefault("Camera");
		DrawDefault("Require");
		BeginError(Any(t => t.Prefab == null));
			DrawDefault("Prefab");
		EndError();
	}
}
#endif

// This component handles mouselook when attached to the camera
[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Mouse Spawn")]
public class SgtMouseSpawn : MonoBehaviour
{
	[Tooltip("The camera used to spawn in front of (default = MainCamera)")]
	public Camera Camera;

	[Tooltip("The key that needs to be held down to look")]
	public KeyCode Require = KeyCode.Mouse0;

	public GameObject Prefab;
	
	protected virtual void Update()
	{
		var camera = Camera;

		if (camera == null)
		{
			camera = Camera.main;
		}

		if (camera != null)
		{
			if (Require == KeyCode.None || Input.GetKeyDown(Require) == true)
			{
				var ray = camera.ScreenPointToRay(Input.mousePosition);
				var hit = default(RaycastHit);

				if (Physics.Raycast(ray, out hit) == true)
				{
					Instantiate(Prefab, hit.point, Quaternion.identity);
				}
			}
		}
	}
}