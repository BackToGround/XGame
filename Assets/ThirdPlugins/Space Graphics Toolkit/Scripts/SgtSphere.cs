using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtSphere))]
public class SgtSphere_Editor : SgtEditor<SgtSphere>
{
	protected override void OnInspector()
	{
		BeginError(Any(t => t.Radius <= 0.0f));
			DrawDefault("Radius");
		EndError();
		DrawDefault("Ease");
		DrawDefault("Power");
	}
}
#endif

[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Sphere")]
public class SgtSphere : SgtShape
{
	[Tooltip("The radius of this sphere in local coordinates")]
	public float Radius = 1.0f;

	[Tooltip("The transtion style between minimum and maximum density")]
	public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

	[Tooltip("How quickly the density increases when inside the sphere")]
	public float Power = 2.0f;

	public override float GetDensity(Vector3 worldPoint)
	{
		var localPoint = transform.InverseTransformPoint(worldPoint);
		var distance   = Mathf.InverseLerp(Radius, 0.0f, localPoint.magnitude);

		return SgtHelper.Pow(SgtEase.Evaluate(Ease, distance), Power);
	}

	public static SgtSphere CreateSphere(int layer = 0, Transform parent = null)
	{
		return CreateSphere(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
	}

	public static SgtSphere CreateSphere(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = SgtHelper.CreateGameObject("Sphere", layer, parent, localPosition, localRotation, localScale);
		var sphere     = gameObject.AddComponent<SgtSphere>();

		return sphere;
	}

#if UNITY_EDITOR
	[MenuItem(SgtHelper.GameObjectMenuPrefix + "Sphere", false, 10)]
	public static void CreateDebrisGridMenuItem()
	{
		var parent = SgtHelper.GetSelectedParent();
		var sphere = CreateSphere(parent != null ? parent.gameObject.layer : 0, parent);

		SgtHelper.SelectAndPing(sphere);
	}
#endif

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.DrawWireSphere(Vector3.zero, Radius);

		for (var i = 0; i < 10; i++)
		{
			var distance = i * 0.1f;

			distance = GetDensity(transform.TransformPoint(0.0f, 0.0f, distance * Radius));

			Gizmos.DrawWireSphere(Vector3.zero, Radius * distance);
		}
	}
#endif
}