using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtTerrainRidgedSimplex))]
public class SgtTerrainRidgedSimplex_Editor : SgtEditor<SgtTerrainRidgedSimplex>
{
	protected override void OnInspector()
	{
		var updateNoise  = false;
		var dirtyTerrain = false;

		BeginError(Any(t => t.Density == 0.0f));
			DrawDefault("Density", ref dirtyTerrain);
		EndError();
		BeginError(Any(t => t.Strength == 0.0f));
			DrawDefault("Strength", ref dirtyTerrain);
		EndError();
		DrawDefault("Octaves", ref dirtyTerrain, ref updateNoise);
		DrawDefault("Invert", ref dirtyTerrain, ref updateNoise);
		DrawDefault("Seed", ref dirtyTerrain, ref updateNoise);

		if (updateNoise  == true) DirtyEach(t => t.UpdateNoise ());
		if (dirtyTerrain == true) DirtyEach(t => t.DirtyTerrain());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Ridged Simplex")]
public class SgtTerrainRidgedSimplex : SgtTerrainModifier
{
	[Tooltip("The density/frequency/tiling of the displacement")]
	public float Density = 10;

	[Tooltip("The +- strength of the displacement")]
	public float Strength = 0.5f;

	[Tooltip("The detail of the simplex noise")]
	[Range(1, 20)]
	public int Octaves = 5;

	[Tooltip("Invert the ridges?")]
	public bool Invert;

	[Tooltip("The random seed used for the simplex noise")]
	[SgtSeed]
	public int Seed;

	[System.NonSerialized]
	private SgtSimplex[] generators;

	[System.NonSerialized]
	private float scale = 1.0f;

	public void UpdateNoise()
	{
		if (generators == null || generators.Length != Octaves)
		{
			generators = new SgtSimplex[Octaves];
		}

		var weight = 1.0f;
		var total  = 0.0f;

		for (var i = 0; i < Octaves; i++)
		{
			var generator = generators[i];

			if (generator == null)
			{
				generator = generators[i] = new SgtSimplex();
			}

			generator.SetSeed(Seed + i * 999);

			total  += weight;
			weight *= 0.5f;
		}

		if (total > 0.0f)
		{
			scale = 1.0f / total;
		}
		else
		{
			scale = 1.0f;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		terrain.OnCalculateHeight += CalculateHeight;

		UpdateNoise();
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		terrain.OnCalculateHeight -= CalculateHeight;
	}

	private void CalculateHeight(SgtVector3D localPosition, ref float height)
	{
		localPosition /= localPosition.magnitude;
		localPosition *= Density;

		var weight = 1.0f;
		var total  = 0.0f;

		for (var i = 0; i < Octaves; i++)
		{
			total         += generators[i].Generate((float)localPosition.x, (float)localPosition.y, (float)localPosition.z) * weight;
			weight        *= 0.5f;
			localPosition *= 2.0;
		}

		// Scale to -2 .. 2
		total *= scale * 2.0f;

		// Abs to 0 .. 2
		total = System.Math.Abs(total);

		if (Invert == true)
		{
			total = 2.0f - total;
		}

		// Shift to -1 .. 1
		total -= 1.0f;

		// Scale to strength and add
		height += total * Strength;
	}
}