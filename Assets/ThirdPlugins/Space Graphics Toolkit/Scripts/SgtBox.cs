using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtBox))]
public class SgtBox_Editor : SgtEditor<SgtBox>
{
	protected override void OnInspector()
	{
		BeginError(Any(t => t.Extents == Vector3.zero));
			DrawDefault("Extents");
		EndError();
		DrawDefault("Ease");
		DrawDefault("Power");
	}
}
#endif

[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Box")]
public class SgtBox : SgtShape
{
	[Tooltip("The min/max size of the cube")]
	public Vector3 Extents = Vector3.one;

	[Tooltip("The transtion style between minimum and maximum density")]
	public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

	[Tooltip("How quickly the density increases when inside the sphere")]
	public float Power = 2.0f;

	public override float GetDensity(Vector3 worldPoint)
	{
		var localPoint = transform.InverseTransformPoint(worldPoint);
		var distanceX  = Mathf.InverseLerp(Extents.x, 0.0f, Mathf.Abs(localPoint.x));
		var distanceY  = Mathf.InverseLerp(Extents.y, 0.0f, Mathf.Abs(localPoint.y));
		var distanceZ  = Mathf.InverseLerp(Extents.z, 0.0f, Mathf.Abs(localPoint.z));
		var distance   = Mathf.Min(distanceX, Mathf.Min(distanceY, distanceZ));

		return SgtHelper.Pow(SgtEase.Evaluate(Ease, distance), Power);
	}

	public static SgtBox CreateBox(int layer = 0, Transform parent = null)
	{
		return CreateBox(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtBox CreateBox(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = SgtHelper.CreateGameObject("Cube", layer, parent, localPosition, localRotation, localScale);
		var cube       = gameObject.AddComponent<SgtBox>();

		return cube;
	}

#if UNITY_EDITOR
	[MenuItem(SgtHelper.GameObjectMenuPrefix + "Cube", false, 10)]
	public static void CreateDebrisGridMenuItem()
	{
		var parent = SgtHelper.GetSelectedParent();
		var cube   = CreateBox(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(cube);
	}
#endif

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		for (var i = 0; i <= 10; i++)
		{
			var distance = i * 0.1f;
			var size     = GetDensity(transform.TransformPoint(distance * Extents)) * Extents;

			Gizmos.DrawWireCube(Vector3.zero, size * 2.0f);
		}
	}
#endif
}