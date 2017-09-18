using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtAuroraMainTex))]
public class SgtAuroraMainTex_Editor : SgtEditor<SgtAuroraMainTex>
{
	protected override void OnInspector()
	{
		var updateTexture = false;
		var updateApply   = false;
		
		BeginError(Any(t => t.Aurora == null));
			DrawDefault("Aurora", ref updateApply);
		EndError();
		BeginError(Any(t => t.Width < 1));
			DrawDefault("Width", ref updateTexture);
		EndError();
		BeginError(Any(t => t.Height < 1));
			DrawDefault("Height", ref updateTexture);
		EndError();
		DrawDefault("Format", ref updateTexture);
		
		Separator();
		
		DrawDefault("NoiseStrength", ref updateTexture);
		BeginError(Any(t => t.NoisePoints <= 0));
			DrawDefault("NoisePoints", ref updateTexture);
		EndError();
		DrawDefault("NoiseSeed", ref updateTexture);
		
		Separator();

		DrawDefault("TopEase", ref updateTexture);
		DrawDefault("TopPower", ref updateTexture);

		Separator();

		DrawDefault("MiddlePoint", ref updateTexture);
		DrawDefault("MiddleColor", ref updateTexture);
		DrawDefault("MiddleEase", ref updateTexture);
		DrawDefault("MiddlePower", ref updateTexture);

		Separator();

		DrawDefault("BottomEase", ref updateTexture);
		DrawDefault("BottomPower", ref updateTexture);

		if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
		if (updateApply   == true) DirtyEach(t => t.UpdateApply   ());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Aurora MainTex")]
public class SgtAuroraMainTex : MonoBehaviour
{
	public SgtAurora Aurora;

	[Tooltip("The resolution of the noise samples")]
	public int Width = 256;

	[Tooltip("The resolution of the bottom/middle/top transition")]
	public int Height = 64;
	
	[Tooltip("The format of this texture")]
	public TextureFormat Format = TextureFormat.ARGB32;
	
	[Tooltip("The strength of the noise points")]
	[Range(0.0f, 1.0f)]
	public float NoiseStrength = 0.5f;

	[Tooltip("The amount of noise points")]
	public int NoisePoints = 10;

	[Tooltip("The random seed used when generating this texture")]
	[SgtSeed]
	public int NoiseSeed;

	[Tooltip("The transition style between the top and middle")]
	public SgtEase.Type TopEase = SgtEase.Type.Smoothstep;

	[Tooltip("The transition strength between the top and middle")]
	public float TopPower = 1.0f;
	
	[Tooltip("The point separating the top from bottom")]
	[Range(0.0f, 1.0f)]
	public float MiddlePoint = 0.25f;
	
	[Tooltip("The base color of the aurora starting from the bottom")]
	public Color MiddleColor = Color.green;

	[Tooltip("The transition style between the bottom and top of the aurora")]
	public SgtEase.Type MiddleEase = SgtEase.Type.Exponential;

	[Tooltip("The strength of the color transition between the bottom and top")]
	public float MiddlePower = 4.0f;

	[Tooltip("The transition style between the bottom and middle")]
	public SgtEase.Type BottomEase = SgtEase.Type.Exponential;

	[Tooltip("The transition strength between the bottom and middle")]
	public float BottomPower = 2.0f;
	
	[System.NonSerialized]
	private Texture2D generatedTexture;

	[SerializeField]
	[HideInInspector]
	private bool startCalled;

	private static List<float> noisePoints = new List<float>();
	
	public Texture2D GeneratedTexture
	{
		get
		{
			return generatedTexture;
		}
	}

#if UNITY_EDITOR
	[ContextMenu("Export Texture")]
	public void ExportTexture()
	{
		var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Aurora MainTex");

		if (importer != null)
		{
			importer.textureCompression  = TextureImporterCompression.Uncompressed;
			importer.alphaSource         = TextureImporterAlphaSource.FromInput;
			importer.wrapMode            = TextureWrapMode.Repeat;
			importer.filterMode          = FilterMode.Trilinear;
			importer.anisoLevel          = 16;
			importer.alphaIsTransparency = true;

			importer.SaveAndReimport();
		}
	}
#endif

	[ContextMenu("Update Textures")]
	public void UpdateTextures()
	{
		if (Width > 0 && Height > 0 && NoisePoints > 0)
		{
			// Destroy if invalid
			if (generatedTexture != null)
			{
				if (generatedTexture.width != Width || generatedTexture.height != Height || generatedTexture.format != Format)
				{
					generatedTexture = SgtHelper.Destroy(generatedTexture);
				}
			}

			// Create?
			if (generatedTexture == null)
			{
				generatedTexture = SgtHelper.CreateTempTexture2D("Aurora MainTex (Generated)", Width, Height, Format);

				generatedTexture.wrapMode = TextureWrapMode.Repeat;

				UpdateApply();
			}
			
			SgtHelper.BeginRandomSeed(NoiseSeed);
			{
				noisePoints.Clear();

				for (var i = 0; i < NoisePoints; i++)
				{
					noisePoints.Add(1.0f - Random.Range(0.0f, NoiseStrength));
				}
			}
			SgtHelper.EndRandomSeed();

			var stepX = 1.0f / (Width  - 1);
			var stepY = 1.0f / (Height - 1);

			for (var y = 0; y < Height; y++)
			{
				var v = y * stepY;

				for (var x = 0; x < Width; x++)
				{
					var u = x * stepX;

					WriteTexture(u, v, x, y);
				}
			}

			generatedTexture.Apply();
		}
	}
	
	private void WriteTexture(float u, float v, int x, int y)
	{
		var noise      = u * NoisePoints;
		var noiseIndex = (int)noise;
		var noiseFrac  = noise % 1.0f;
		var noiseA     = noisePoints[(noiseIndex + 0) % NoisePoints];
		var noiseB     = noisePoints[(noiseIndex + 1) % NoisePoints];
		var noiseC     = noisePoints[(noiseIndex + 2) % NoisePoints];
		var noiseD     = noisePoints[(noiseIndex + 3) % NoisePoints];
		var color      = MiddleColor;

		if (v < MiddlePoint)
		{
			color.a = SgtEase.Evaluate(BottomEase, SgtHelper.Pow(Mathf.InverseLerp(0.0f, MiddlePoint, v), BottomPower));
		}
		else
		{
			color.a = SgtEase.Evaluate(TopEase, SgtHelper.Pow(Mathf.InverseLerp(1.0f, MiddlePoint, v), TopPower));
		}

		var middle = SgtEase.Evaluate(MiddleEase, SgtHelper.Pow(1.0f - v, MiddlePower));

		color.a *= SgtHelper.HermiteInterpolate(noiseA, noiseB, noiseC, noiseD, noiseFrac);

		color.r *= middle * color.a;
		color.g *= middle * color.a;
		color.b *= middle * color.a;
		color.a *= 1.0f - middle;
		
		generatedTexture.SetPixel(x, y, color);
	}
	
	[ContextMenu("Update Apply")]
	public void UpdateApply()
	{
		if (Aurora != null)
		{
			Aurora.MainTex = generatedTexture;

			Aurora.UpdateMainTex();
		}
	}

	protected virtual void OnEnable()
	{
		if (startCalled == true)
		{
			CheckUpdateCalls();
		}
	}

	protected virtual void Start()
	{
		if (startCalled == false)
		{
			startCalled = true;

			if (Aurora == null)
			{
				Aurora = GetComponent<SgtAurora>();
			}

			CheckUpdateCalls();
		}
	}

	protected virtual void OnDestroy()
	{
		SgtHelper.Destroy(generatedTexture);
	}

	private void CheckUpdateCalls()
	{
		if (generatedTexture == null)
		{
			UpdateTextures();
		}

		UpdateApply();
	}
}