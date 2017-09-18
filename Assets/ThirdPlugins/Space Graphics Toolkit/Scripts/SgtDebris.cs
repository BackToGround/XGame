using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtDebris))]
public class SgtDebris_Editor : SgtEditor<SgtDebris>
{
	protected override void OnInspector()
	{
		DrawDefault("Pool");
		
		Separator();

		BeginDisabled();
			DrawDefault("State");
			DrawDefault("Prefab");
			DrawDefault("Scale");
			DrawDefault("Cell");
		EndDisabled();
	}
}
#endif

[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debris")]
public class SgtDebris : MonoBehaviour
{
	public enum StateType
	{
		Hide,
		Fade,
		Show,
	}

	// Called when this debris is spawned (if pooling is enabled)
	public System.Action OnSpawn;

	// Called when this debris is despawned (if pooling is enabled)
	public System.Action OnDespawn;

	[Tooltip("Can this particle be pooled?")]
	public bool Pool;

	[Tooltip("The current state of the scaling")]
	public StateType State;

	[Tooltip("The prefab this was instantiated from")]
	public SgtDebris Prefab;

	[Tooltip("This gets automatically copied when spawning debris")]
	public Vector3 Scale;

	[Tooltip("The cell this debris was spawned in")]
	public SgtVector3L Cell;

	// The initial scale-in
	public float Show;
}