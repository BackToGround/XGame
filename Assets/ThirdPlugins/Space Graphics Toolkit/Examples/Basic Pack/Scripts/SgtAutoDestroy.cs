using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtAutoDestroy))]
public class SgtAutoDestroy_Editor : SgtEditor<SgtAutoDestroy>
{
	protected override void OnInspector()
	{
		DrawDefault("Seconds");
	}
}
#endif

// This component handles adding/removing itself from a spacetime's well list
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Auto Destroy")]
public class SgtAutoDestroy : MonoBehaviour
{
	[Tooltip("The remaining time until this GameObject is destroyed")]
	public float Seconds = 1.0f;
	
	protected virtual void Update()
	{
		Seconds -= Time.deltaTime;

		if (Seconds <= 0.0f)
		{
			Destroy(gameObject);
		}
	}
}