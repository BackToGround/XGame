using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainHeightmap))]
public class SgtTerrainHeightmap_Editor : SgtEditor<SgtTerrainHeightmap>
{
	protected override void OnInspector()
	{
		var dirtyTerrain = false;

		BeginError(Any(t => t.Heightmap == null));
			DrawDefault("Heightmap", ref dirtyTerrain);
		EndError();
		DrawDefault("Encoding", ref dirtyTerrain);
		BeginError(Any(t => t.DisplacementMin >= t.DisplacementMax));
			DrawDefault("DisplacementMin", ref dirtyTerrain);
			DrawDefault("DisplacementMax", ref dirtyTerrain);
		EndError();

		if (dirtyTerrain == true) DirtyEach(t => t.DirtyTerrain());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Heightmap")]
public class SgtTerrainHeightmap : SgtTerrainModifier
{
	public enum EncodingType
	{
		Alpha,
		RedGreen
	}

	[Tooltip("The heightmap texture using a cylindrical (equirectangular) projection")]
	public Texture2D Heightmap;

	[Tooltip("The way the height data is stored in the texture")]
	public EncodingType Encoding = EncodingType.Alpha;

	[Tooltip("The height displacement represented by alpha = 0")]
	public float DisplacementMin = 0.0f;

	[Tooltip("The height displacement represented by alpha = 255")]
	public float DisplacementMax = 0.1f;

	protected override void OnEnable()
	{
		base.OnEnable();

		terrain.OnCalculateHeight += CalculateHeight;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		terrain.OnCalculateHeight -= CalculateHeight;
	}
	
	private void CalculateHeight(SgtVector3D localPosition, ref float height)
	{
		if (Heightmap != null)
		{
			var uv    = SgtHelper.CartesianToPolarUV((Vector3)localPosition);
			var color = SampleBilinear(uv);

			switch (Encoding)
			{
				case EncodingType.Alpha:
				{
					height += Mathf.Lerp(DisplacementMin, DisplacementMax, color.a);
				}
				break;

				case EncodingType.RedGreen:
				{
					height += Mathf.Lerp(DisplacementMin, DisplacementMax, (color.r * 255.0f + color.g) / 256.0f);
				}
				break;
			}
		}
	}

	private Color SampleBilinear(Vector2 uv)
	{
		return Heightmap.GetPixelBilinear(uv.x, uv.y);
	}
}