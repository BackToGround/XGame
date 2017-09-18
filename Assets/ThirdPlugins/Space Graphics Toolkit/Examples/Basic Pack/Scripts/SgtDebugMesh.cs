using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SgtDebugMesh))]
public class SgtDebugMesh_Editor : SgtEditor<SgtDebugMesh>
{
	protected override void OnInspector()
	{
		BeginError(Any(t => t.DrawScale <= 0.0f));
			DrawDefault("DrawScale");
		EndError();
		DrawDefault("TriangleColor");
		DrawDefault("NormalColor");
		DrawDefault("TangentColor");
	}
}
#endif

// This component draws debug mesh info in the scene window
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Debug Mesh")]
public class SgtDebugMesh : MonoBehaviour
{
	[Tooltip("The scale of the normal andtangent lines")]
	public float DrawScale = 1.0f;

	[Tooltip("The color of the normals")]
	public Color TriangleColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);

	[Tooltip("The color of the normals")]
	public Color NormalColor = new Color(0.0f, 1.0f, 0.0f, 0.5f);

	[Tooltip("The color of the tangents")]
	public Color TangentColor = new Color(0.0f, 0.0f, 1.0f, 0.5f);

	[System.NonSerialized]
	private MeshFilter meshFilter;

#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

		var mesh = meshFilter.sharedMesh;

		if (mesh != null)
		{
			var indices   = mesh.triangles;
			var positions = mesh.vertices;
			var normals   = mesh.normals;
			var tangents  = mesh.tangents;

			if (indices.Length > 0)
			{
				Gizmos.matrix = transform.localToWorldMatrix;

				for (var i = 0; i < indices.Length; i += 3)
				{
					var index0    = indices[i + 0];
					var index1    = indices[i + 1];
					var index2    = indices[i + 2];
					var position0 = positions[index0];
					var position1 = positions[index1];
					var position2 = positions[index2];

					Gizmos.color = TriangleColor;

					Gizmos.DrawLine(position0, position1);
					Gizmos.DrawLine(position1, position2);
					Gizmos.DrawLine(position2, position0);

					if (normals.Length > 0)
					{
						Gizmos.color  = NormalColor;

						Gizmos.DrawLine(position0, position0 + normals[index0] * DrawScale);
						Gizmos.DrawLine(position1, position1 + normals[index1] * DrawScale);
						Gizmos.DrawLine(position2, position2 + normals[index2] * DrawScale);
					}

					if (tangents.Length > 0)
					{
						Gizmos.color = TangentColor;

						Gizmos.DrawLine(position0, position0 + (Vector3)tangents[index0] * DrawScale);
						Gizmos.DrawLine(position1, position1 + (Vector3)tangents[index1] * DrawScale);
						Gizmos.DrawLine(position2, position2 + (Vector3)tangents[index2] * DrawScale);
					}
				}
			}
		}
	}
#endif
}