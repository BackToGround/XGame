using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtAuroraFadeNear))]
public class SgtAuroraFadeNear_Editor : SgtEditor<SgtAuroraFadeNear>
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
		DrawDefault("Format", ref updateTexture);
		
		Separator();
		
		DrawDefault("Ease", ref updateTexture);
		DrawDefault("Power", ref updateTexture);

		if (updateTexture == true) DirtyEach(t => t.UpdateTextures());
		if (updateApply   == true) DirtyEach(t => t.UpdateApply   ());
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Aurora Fade Near")]
public class SgtAuroraFadeNear : MonoBehaviour
{
	[Tooltip("The aurora the generated texture gets applied to")]
	public SgtAurora Aurora;
	
	[Tooltip("The resolution of the fade transition")]
	public int Width = 256;
	
	[Tooltip("The format of the generated texture")]
	public TextureFormat Format = TextureFormat.ARGB32;
	
	[Tooltip("The transition style")]
	public SgtEase.Type Ease = SgtEase.Type.Smoothstep;

	[Tooltip("The sharpness of the transition")]
	public float Power = 2.0f;
	
	[System.NonSerialized]
	private Texture2D generatedTexture;

	[SerializeField]
	[HideInInspector]
	private bool startCalled;
	
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
		var importer = SgtHelper.ExportTextureDialog(generatedTexture, "Aurora Fade Near");

		if (importer != null)
		{
			importer.textureCompression  = TextureImporterCompression.Uncompressed;
			importer.alphaSource         = TextureImporterAlphaSource.FromInput;
			importer.wrapMode            = TextureWrapMode.Clamp;
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
		if (Width > 0)
		{
			// Destroy if invalid
			if (generatedTexture != null)
			{
				if (generatedTexture.width != Width || generatedTexture.height != 1 || generatedTexture.format != Format)
				{
					generatedTexture = SgtHelper.Destroy(generatedTexture);
				}
			}

			// Create?
			if (generatedTexture == null)
			{
				generatedTexture = SgtHelper.CreateTempTexture2D("Aurora Fade Near (Generated)", Width, 1, Format);

				generatedTexture.wrapMode = TextureWrapMode.Clamp;

				UpdateApply();
			}

			var color = Color.clear;
			var stepY = 1.0f / (Width - 1);

			for (var x = 0; x < Width; x++)
			{
				var u = x * stepY;

				WriteTexture(u, x);
			}
			
			generatedTexture.Apply();
		}
	}
	
	private void WriteTexture(float u, int x)
	{
		var fade  = 1.0f - SgtEase.Evaluate(Ease, 1.0f - SgtHelper.Pow(u, Power));
		var color = new Color(fade, fade, fade, fade);
		
		generatedTexture.SetPixel(x, 0, color);
	}
	
	[ContextMenu("Update Apply")]
	public void UpdateApply()
	{
		if (Aurora != null)
		{
			Aurora.FadeNearTex = generatedTexture;

			Aurora.UpdateFadeNearTex();
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