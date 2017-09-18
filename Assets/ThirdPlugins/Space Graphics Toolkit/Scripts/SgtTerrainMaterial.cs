using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainMaterial))]
public class SgtTerrainMaterial_Editor : SgtEditor<SgtTerrainMaterial>
{
	protected override void OnInspector()
	{
		var dirtyTerrain = false;

		BeginError(Any(t => t.Material == null));
			DrawDefault("Material", ref dirtyTerrain);
		EndError();
		DrawDefault("AllSides", ref dirtyTerrain);
		if (Any(t => t.AllSides == false))
		{
			BeginIndent();
				DrawDefault("RequiredSide", ref dirtyTerrain);
			EndIndent();
		}
		BeginError(Any(t => t.LevelMin < 0 || t.LevelMin > t.LevelMax));
			DrawDefault("LevelMin", ref dirtyTerrain);
			DrawDefault("LevelMax", ref dirtyTerrain);
		EndError();

		if (dirtyTerrain == true) DirtyEach(t => t.DirtyTerrain());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Material")]
public class SgtTerrainMaterial : SgtTerrainModifier
{
	[Tooltip("The material that will be assigned")]
	public Material Material;

	[Tooltip("Apply this material to all sides?")]
	public bool AllSides = true;

	[Tooltip("The side this material will be applied to")]
	public CubemapFace RequiredSide;

	[Tooltip("The minimum LOD level this material will be applied to")]
	public int LevelMin = 5;

	[Tooltip("The maximum LOD level this material will be applied to")]
	public int LevelMax = 5;

	protected override void OnEnable()
	{
		base.OnEnable();

		terrain.OnCalculateMaterial += CalculateMaterial;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		terrain.OnCalculateMaterial -= CalculateMaterial;
	}

	private void CalculateMaterial(SgtTerrainLevel level, ref Material material)
	{
		if (level.Index >= LevelMin && level.Index <= LevelMax)
		{
			if (AllSides == true || level.Face.Side == RequiredSide)
			{
				material = Material;
			}
		}
	}
}