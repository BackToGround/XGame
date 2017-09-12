#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;

[CreateAssetMenu(fileName = "Data", menuName = "Lighting Profile", order = 1)]
public class LightingProfile : ScriptableObject {

	public string objectName = "LightingProfile";
	[Header("Profiles")]
	public PostProcessingProfile postProcessingProfile;

	[Header("Global")]
	public Render_Path renderPath;
	public  LightingMode lightingMode;
	public  float bakedResolution = 10f;
	public  LightSettings lightSettings;
	public MyColorSpace colorSpace;

	[Header("Environment")]
	public Material skyBox;
	public  AmbientLight ambientLight;
	public  Color ambientColor = Color.white;

	[Header("Sun")]
	public  Color sunColor = Color.white;
	public float sunIntensity = 2.1f;
	public Flare sunFlare;

	[Header("Fog")]
	public CustomFog fogMode;
	public float fogDistance = 0;
	public float fogHeight = 30f;
	public float fogHeightDensity = 0.5f;
	public Color fogColor = Color.white;
	public float fogDensity;

	[Header("Other")]
	public VolumetricLightType volumetricLight;
	public LightsShadow lightsShadow;
	public LightProbeMode lightProbesMode;
	public bool automaticLightmap;
	public DOFType dofType;
	public float dofDistance = 1f;
}
#endif