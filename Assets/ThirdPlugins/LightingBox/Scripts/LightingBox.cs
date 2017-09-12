// AliyerEdon@gmail.com/
// Share lighting box to help people create awesome graphics   
#if UNITY_EDITOR   
using UnityEngine;   
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

#region Emum Types
public enum AmbientLight
{
	Skybox,
	Color
}
public enum LightingMode
{
	FullyRealtime,
	RealtimeGI,
	Baked
}
public enum LightSettings
{
	Default,
	Realtime,
	Mixed,
	Baked
}
public enum MyColorSpace
{
	Linear,
	Gamma
}
public enum VolumetricLightType
{
	Off,
	OnlyDirectional,
	AllLightSources
}
public enum VLightLevel
{
	Level1,	Level2,Level3,Level4
}
public enum CustomFog
{
	Off,Global,
	Height,
	Distance
}
public enum LightsShadow
{
	OnlyDirectionalHard,OnlyDirectionalSoft,
	AllLightsSoft,AllLightsHard,
	Off
}
public enum LightProbeMode
{
	Blend,
	Proxy
}
public enum Render_Path
{
	Default,Forward,Deferred
}
public enum DOFType
{
	On,Off

}
#endregion

[ExecuteInEditMode]
public class LightingBox : EditorWindow
{
	#region Variables
	public Render_Path renderPath;
	public LightingProfile lightingProfile;
	public PostProcessingProfile postProcessingProfile;
	public LightingMode lightingMode;
	public AmbientLight ambientLight;
	public LightSettings lightSettings;
	public LightProbeMode lightprobeMode;
	public DOFType dofType;
	public float dofDistance = 1f;
	public Material skyBox;
	public Light sunLight;
	public Flare sunFlare;
	public Color sunColor = Color.white;
	public bool autoMode;
	public MyColorSpace colorSpace;
	public VolumetricLightType vLight;
	public VLightLevel vLightLevel;
	CustomFog vFog;
	float fDistance = 0;
	float fHeight = 30f;
	[Range(0,1)]
	float fheightDensity = 0.5f;
	Color fColor = Color.white;
	[Range(0,10)]
	float fDensity = 1f;
	public LightsShadow psShadow;
	public float sunIntensity = 2.1f;
	public float bakedResolution = 10f;
	public Color ambientColor;
	public bool helpBox;

	Color redColor;
	bool lightError;
	bool lightChecked;
	GUIStyle myFoldoutStyle;
	bool showLogs;
	// Display window elements (Lighting Box)   
	Vector2 scrollPos;
	#endregion

	#region Init()
	// Add menu named "My Window" to the Window menu
	[MenuItem("Window/Lighting Box %E")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
		LightingBox window = (LightingBox)EditorWindow.GetWindow(typeof(LightingBox));
		window.Show();
		window.autoRepaintOnSceneChange = true;
	}
	#endregion

	void OnNewSceneOpened()
	{

		if (currentScene != EditorSceneManager.GetActiveScene().name)
		{
			if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name)))
				lightingProfile = Resources.Load ("DefaultSettings")as LightingProfile;
			else {
				lightingProfile = (LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LightingProfile));
			}

			OnLoad ();
			currentScene = EditorSceneManager.GetActiveScene().name;
		}
	}

	void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnNewSceneOpened;
	}

	void OnEnable()
	{
		EditorApplication.hierarchyWindowChanged += OnNewSceneOpened;

		currentScene = EditorSceneManager.GetActiveScene().name;
		if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name)))
			lightingProfile = Resources.Load ("DefaultSettings")as LightingProfile;
		else
			lightingProfile = (LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LightingProfile));
		
		OnLoad ();
	}

	void OnGUI()
	{
		#region GUI start implementation
		Undo.RecordObject (this,"lb");
		if(sunLight)
			sunLight.intensity = sunIntensity;

		scrollPos = EditorGUILayout.BeginScrollView (scrollPos,
			false,
			false,
			GUILayout.Width(Screen.width ),
			GUILayout.Height(Screen.height));
		
		EditorGUILayout.Space ();

		GUILayout.Label ("Lighting Box - ALIyerEdon@gmail.com", EditorStyles.helpBox);

		if (!helpBox) 
		{
			if (GUILayout.Button ("Show Help")) {
				helpBox = !helpBox;
			}
		} else 
		{
			if (GUILayout.Button ("Hide Help")) {
				helpBox = !helpBox;
			}
		}
		if (helpBox)
			EditorGUILayout.HelpBox("Update PlayMode changes after stop the game",MessageType.Info);
		
		if (GUILayout.Button ("Refresh"))
		{
			UpdateSettings();
			UpdatePostEffects();
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		if (helpBox)
			EditorGUILayout.HelpBox("1. LightingBox settings profile   2.Post Processing Stack Profile",MessageType.Info);
		
		var lightingProfileRef = lightingProfile;

		lightingProfile = EditorGUILayout.ObjectField("Profile", lightingProfile, typeof(LightingProfile), true) as LightingProfile;

		if (lightingProfileRef != lightingProfile)
		{
			OnLoad();
			EditorPrefs.SetString (EditorSceneManager.GetActiveScene ().name, AssetDatabase.GetAssetPath (lightingProfile));
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}

		var pProfileRef = postProcessingProfile;

		postProcessingProfile = EditorGUILayout.ObjectField("Post Processing Profile", postProcessingProfile, typeof(PostProcessingProfile), true) as PostProcessingProfile;

		if(pProfileRef != postProcessingProfile)
		{
			if(postProcessingProfile)
			{
				if(lightingProfile) lightingProfile.postProcessingProfile = postProcessingProfile;
				if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
			}
			UpdatePostEffects ();
		}
		EditorGUILayout.Space ();EditorGUILayout.Space ();
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();
		#endregion

		#region Lighting Mode
		if (helpBox)
			EditorGUILayout.HelpBox ("Fully realtime without GI, Enlighten Realtime GI or Baked Progressive Lightmapper", MessageType.Info);
		
		var lightingModeRef = lightingMode;

		// Choose lighting mode (realtime GI or baked GI)
		lightingMode = (LightingMode)EditorGUILayout.EnumPopup("Lighting Mode",lightingMode,GUILayout.Width(343));

		if (lightingMode == LightingMode.Baked) 
		{
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Baked lightmapping resolution. Higher value needs more RAM and longer bake time. Check task manager about RAM usage during bake time", MessageType.Info);

			// Baked lightmapping resolution   
			bakedResolution = EditorGUILayout.FloatField ("Baked Resolution", bakedResolution);
			LightmapEditorSettings.bakeResolution = bakedResolution;
			if(lightingProfile) lightingProfile.bakedResolution = bakedResolution;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);

		}

		if (lightingMode != lightingModeRef)
		{
			//----------------------------------------------------------------------
			// Update Lighting Mode
			if (lightingMode == LightingMode.RealtimeGI)
			{
				Lightmapping.realtimeGI = true;
				Lightmapping.bakedGI = false;
				LightmapEditorSettings.giBakeBackend = LightmapEditorSettings.GIBakeBackend.Radiosity;
			}
			if (lightingMode == LightingMode.Baked)
			{
				Lightmapping.realtimeGI = false;
				Lightmapping.bakedGI = true;
				LightmapEditorSettings.giBakeBackend = LightmapEditorSettings.GIBakeBackend.PathTracer;
			}
			if (lightingMode == LightingMode.FullyRealtime) {
				Lightmapping.realtimeGI = false;
				Lightmapping.bakedGI = false;
			}
			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.lightingMode = lightingMode;  
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		#endregion

		#region Color Space
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox("Choose between Linear or Gamma color space , default should be Linear for my settings and next-gen platfroms   ",MessageType.Info);

		var colorSpaceRef = colorSpace;

		// Choose color space (Linear,Gamma) i have used Linear inpost effect setting s
		colorSpace = (MyColorSpace)EditorGUILayout.EnumPopup("Color Space",colorSpace,GUILayout.Width(343));

		if(colorSpaceRef !=colorSpace)
		{
			// Color Space
			if (colorSpace == MyColorSpace.Gamma) 
				PlayerSettings.colorSpace = ColorSpace.Gamma;
			else
				PlayerSettings.colorSpace = ColorSpace.Linear;
			if(lightingProfile) lightingProfile.colorSpace = colorSpace;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		#endregion

		#region Render Path
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox("Choose between Forward and Deferred rendering path for cameras. Deferred needed for Screen Spacwe Reflection effect. Forward has better performance in unity",MessageType.Info);

		var renderPathRef = renderPath;

		renderPath = (Render_Path)EditorGUILayout.EnumPopup("Render Path",renderPath,GUILayout.Width(343));

		if(renderPathRef !=renderPath)
		{
			Camera[] cams = GameObject.FindObjectsOfType<Camera>();
			foreach(Camera c in cams)
			{
				if (renderPath == Render_Path.Forward) 
					c.renderingPath = RenderingPath.Forward;
				if (renderPath == Render_Path.Deferred) 
					c.renderingPath = RenderingPath.DeferredShading;
				if (renderPath == Render_Path.Default) 
					c.renderingPath = RenderingPath.UsePlayerSettings;

				c.allowHDR = true;
				c.allowMSAA = false;
				c.useOcclusionCulling = true;
			}
			if(lightingProfile) lightingProfile.renderPath = renderPath;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}

		#endregion

		#region Light Types
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox("Changing the type of all light sources (Realtime,Baked,Mixed)",MessageType.Info);

		var lightSettingsRef  = lightSettings;

		// Change file lightmapping type mixed,realtime baked
		lightSettings = (LightSettings)EditorGUILayout.EnumPopup("Lights Type",lightSettings,GUILayout.Width(343));


		//----------------------------------------------------------------------
		// Light Types
		if(lightSettingsRef != lightSettings)
		{
			if (lightSettings == LightSettings.Baked) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 2;
					serialLightSource.ApplyModifiedProperties ();
				}
			} 
			if (lightSettings == LightSettings.Realtime) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 4;
					serialLightSource.ApplyModifiedProperties ();
				}
			}
			if (lightSettings == LightSettings.Mixed) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 1;
					serialLightSource.ApplyModifiedProperties ();
				}

			}
			if(lightingProfile) lightingProfile.lightSettings = lightSettings;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		//----------------------------------------------------------------------
		#endregion

		#region Light Shadows Settings
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox("Activate shadows for point and spot lights   ",MessageType.Info);

		var psshadRef = psShadow;
		// Choose hard shadows state on off for spot and point lights
		psShadow = (LightsShadow)EditorGUILayout.EnumPopup("Shadows",psShadow,GUILayout.Width(343));

		if(psshadRef !=psShadow)
		{

			// Shadows
			if (psShadow == LightsShadow.AllLightsSoft) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.Soft;
				}
			}
			if (psShadow == LightsShadow.AllLightsHard) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.Hard;
				}
			}
			if (psShadow == LightsShadow.OnlyDirectionalSoft) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.None;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.None;
				}
			}
			if (psShadow == LightsShadow.OnlyDirectionalHard) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.None;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.None;
				}
			}
			if (psShadow == LightsShadow.Off) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) 
				{
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.None;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.None;
				}
			}
			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.lightsShadow = psShadow;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		#endregion

		#region Light Probes
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox ("Adjust light probes settings for non-static objects, Blend mode is more optimized", MessageType.Info);

		var lightprobeModeRef = lightprobeMode;

		lightprobeMode = (LightProbeMode)EditorGUILayout.EnumPopup("Light Probes",lightprobeMode,GUILayout.Width(343));

		if(lightprobeModeRef != lightprobeMode)
		{

			// Light Probes
			if (lightprobeMode == LightProbeMode.Blend) {

				MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

				foreach (MeshRenderer mr in renderers) 
				{
					if (!mr.gameObject.isStatic) {
						if (mr.gameObject.GetComponent<LightProbeProxyVolume> ())
							DestroyImmediate (mr.gameObject.GetComponent<LightProbeProxyVolume> ());
						mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
					}
				}
			}
			if (lightprobeMode == LightProbeMode.Proxy) {

				MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

				foreach (MeshRenderer mr in renderers) {

					if (!mr.gameObject.isStatic) {
						if(!mr.gameObject.GetComponent<LightProbeProxyVolume> ())
							mr.gameObject.AddComponent<LightProbeProxyVolume> ();
						mr.gameObject.GetComponent<LightProbeProxyVolume> ().resolutionMode = LightProbeProxyVolume.ResolutionMode.Custom;
						mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.UseProxyVolume;
					}
				}
			}
			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.lightProbesMode = lightprobeMode;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

		#endregion

		#region Ambient

		if (helpBox)
			EditorGUILayout.HelpBox("Assign scene skybox material here   ",MessageType.Info);
		
		var skyboxRef = skyBox;

		skyBox = EditorGUILayout.ObjectField ("SkyBox Material", skyBox, typeof(Material), true) as Material;

		if (skyboxRef != skyBox) {

			if (skyBox)
				RenderSettings.skybox = skyBox;

			if (lightingProfile)
				lightingProfile.skyBox = skyBox;
			if (lightingProfile)
				EditorUtility.SetDirty (lightingProfile);
		}


		if (helpBox)
			EditorGUILayout.HelpBox("Set ambient lighting source as Skybox(IBL) or a simple color",MessageType.Info);

		var ambientLightRef = ambientLight;

		// choose ambient lighting mode   (color or skybox)
		ambientLight = (AmbientLight)EditorGUILayout.EnumPopup("Ambient Source",ambientLight,GUILayout.Width(343));

		if (ambientLight == AmbientLight.Color)
		{
			var ambientColorRef = ambientColor;

			ambientColor = EditorGUILayout.ColorField ("Color", ambientColor);

			if(ambientColorRef !=ambientColor)
			{
				RenderSettings.ambientLight = ambientColor;
				if(lightingProfile) lightingProfile.ambientColor = ambientColor;
				if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
			}
		}

		//----------------------------------------------------------------------
		// Update Ambient
		if(ambientLightRef != ambientLight)
		{
			if (ambientLight == AmbientLight.Color)
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			else
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

			RenderSettings.ambientLight = ambientColor;
			if(lightingProfile) lightingProfile.ambientColor = ambientColor;
			if(lightingProfile) lightingProfile.ambientLight = ambientLight;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);

		}
		//----------------------------------------------------------------------
		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

		#endregion

		#region Sun Light
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
		if (helpBox)
			EditorGUILayout.HelpBox("Sun /  Moon light settings",MessageType.Info);
		
			EditorGUILayout.BeginHorizontal ();
			sunLight = EditorGUILayout.ObjectField ("Sun Light", sunLight, typeof(Light), true) as Light;
			if (!sunLight) {
				if (GUILayout.Button ("Find"))
					FindSun ();
			}
			EditorGUILayout.EndHorizontal ();
			var sunColorRef = sunColor;

			sunColor = EditorGUILayout.ColorField ("Color", sunColor);

			var sunIntensityRef = sunIntensity;

			sunIntensity = EditorGUILayout.FloatField ("Intenity", sunIntensity);

			var sunFlareRef = sunFlare;

			sunFlare = EditorGUILayout.ObjectField ("Lens Flare", sunFlare, typeof(Flare), true) as Flare;

			if (sunColorRef != sunColor) {
				if (sunLight)
					sunLight.color = sunColor;
				else
					FindSun ();
				if (lightingProfile)
					lightingProfile.sunColor = sunColor;
				if (lightingProfile)
					EditorUtility.SetDirty (lightingProfile);
			}
		
			if (sunIntensityRef != sunIntensity) {
				if (sunLight)
					sunLight.intensity = sunIntensity;
				else
					FindSun ();
				if (lightingProfile)
					lightingProfile.sunIntensity = sunIntensity;
				if (lightingProfile)
					EditorUtility.SetDirty (lightingProfile);
			}
		if (sunFlareRef != sunFlare) 
		{
			if (sunFlare)
			{
				if(sunLight)
					sunLight.flare = sunFlare;
			}
			
			if (lightingProfile)
				lightingProfile.sunFlare = sunFlare;
			if (lightingProfile)
				EditorUtility.SetDirty (lightingProfile);
		}
		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();
			#endregion

		#region Volumetric Light
		EditorGUILayout.Space ();
		if (helpBox)
			EditorGUILayout.HelpBox("Activate Volumetric Lights For All Light Sources",MessageType.Info);

		var vLightRef = vLight;
		var vLightLevelRef = vLightLevel;

		// Activate or deactivate volumetric lighting for all light sources
		vLight = (VolumetricLightType)EditorGUILayout.EnumPopup("Volumetric Light",vLight,GUILayout.Width(343));

		if(vLight != VolumetricLightType.Off)
			vLightLevel = (VLightLevel)EditorGUILayout.EnumPopup("Intensity",vLightLevel,GUILayout.Width(343));
		
		if(vLightRef !=vLight || vLightLevelRef !=vLightLevel)
		{
			
			// Volumetric Light
			if (vLight == VolumetricLightType.AllLightSources) {

				Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

				foreach (Camera c in cams) {
					if (!c.gameObject.GetComponent<VolumetricLightRenderer> ()) {
						c.gameObject.AddComponent<VolumetricLightRenderer> ();
						c.gameObject.GetComponent<VolumetricLightRenderer> ().Resolution = VolumetricLightRenderer.VolumtericResolution.Quarter;
						c.gameObject.GetComponent<VolumetricLightRenderer> ().DefaultSpotCookie = Resources.Load ("spot_Cookie_") as Texture;
					}
				}

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (!l.gameObject.GetComponent<VolumetricLight> ()) 
					{
						l.gameObject.AddComponent<VolumetricLight> ();
					}
						l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;
						if(l.type == LightType.Directional)
						{
							if(vLightLevel == VLightLevel.Level1)
								l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
							if(vLightLevel == VLightLevel.Level2)
								l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
							if(vLightLevel == VLightLevel.Level3)
								l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
							if(vLightLevel == VLightLevel.Level4)
								l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
						}
						else
						{
							if(vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
						if(vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
						if(vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
						if(vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
						}

						l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
						l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
						l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
						l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
						l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
						l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;
						if (l.type == LightType.Directional)
							l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
						else {
							l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

							if (l.type == LightType.Spot) {
								if (l.range == 10f)
									l.range = 43f;
								if (l.spotAngle == 30f)
									l.spotAngle = 43f;
							}
						}

						l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
						l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;

				}
			}


			/////////////////////////
			/// 
			if (vLight == VolumetricLightType.OnlyDirectional) {

				Camera[] cams = GameObject.FindObjectsOfType<Camera> ();
				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Camera c in cams) {
					if (c.gameObject.GetComponent<VolumetricLightRenderer> ())
						DestroyImmediate (c.gameObject.GetComponent<VolumetricLightRenderer> ());
				}

				foreach (Light l in lights) {
					if (l.gameObject.GetComponent<VolumetricLight> ())
						DestroyImmediate(l.gameObject.GetComponent<VolumetricLight> ());
				}

				foreach (Camera c in cams) {
					if (!c.gameObject.GetComponent<VolumetricLightRenderer> ()) {
						c.gameObject.AddComponent<VolumetricLightRenderer> ();
						c.gameObject.GetComponent<VolumetricLightRenderer> ().Resolution = VolumetricLightRenderer.VolumtericResolution.Quarter;
						c.gameObject.GetComponent<VolumetricLightRenderer> ().DefaultSpotCookie = Resources.Load ("spot_Cookie_") as Texture;
					}
				}

				foreach (Light l in lights) {
					if(l.type == LightType.Directional)
					{
					if (!l.gameObject.GetComponent<VolumetricLight> ()) {
						l.gameObject.AddComponent<VolumetricLight> ();

						l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;
						if(l.type == LightType.Directional)
							{
								if(vLightLevel == VLightLevel.Level1)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
								if(vLightLevel == VLightLevel.Level2)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
								if(vLightLevel == VLightLevel.Level3)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
								if(vLightLevel == VLightLevel.Level4)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
							}
							else
							{
								if(vLightLevel == VLightLevel.Level1)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
								if(vLightLevel == VLightLevel.Level2)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
								if(vLightLevel == VLightLevel.Level3)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
								if(vLightLevel == VLightLevel.Level4)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
							}

						l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
						l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
						l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
						l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
						l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
						l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;
						if (l.type == LightType.Directional)
							l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
						else {
							l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

						}

						l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
						l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
					}
				}
			}
			}
			/// //////////////
			if (vLight == VolumetricLightType.Off) {
				Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

				foreach (Camera c in cams) {
					if (c.gameObject.GetComponent<VolumetricLightRenderer> ())
						DestroyImmediate (c.gameObject.GetComponent<VolumetricLightRenderer> ());
				}

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.gameObject.GetComponent<VolumetricLight> ())
						DestroyImmediate(l.gameObject.GetComponent<VolumetricLight> ());
				}
			}
			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.volumetricLight = vLight;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}

		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();


		#endregion

		#region Global Fog
		if (helpBox)
			EditorGUILayout.HelpBox("Activate Global Fog for the scene. Combined with unity Lighting Window Fog parameters",MessageType.Info);

		var vfogRef = vFog;

		vFog = (CustomFog)EditorGUILayout.EnumPopup("Global Fog",vFog,GUILayout.Width(343));

		//-----Distance--------------------------------------------------------------------
		if (vFog == CustomFog.Distance)
		{
			float fDistanceRef = fDistance;
			Color fColorRef = fColor;
			float fogDensityRef = fDensity;

			fDistance = (float)EditorGUILayout.FloatField("Start Distance",fDistance);
			fDensity = (float)EditorGUILayout.Slider("Destiny",fDensity,0,30f);
			fColor = (Color)EditorGUILayout.ColorField("Color",fColor);

			if(fDistanceRef != fDistance || fColorRef != fColor || fogDensityRef != fDensity )
			{
				UpdateFog(1);
				if(lightingProfile) lightingProfile.fogDistance = fDistance;
				if(lightingProfile) lightingProfile.fogColor = fColor;
				if(lightingProfile) lightingProfile.fogDensity = fDensity;
				if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
			}
		}
		//-----Global--------------------------------------------------------------------
		if (vFog == CustomFog.Global)
		{
			Color fColorRef = fColor;
			float fogDensityRef = fDensity;

			fDensity = (float)EditorGUILayout.Slider("Destiny",fDensity,0,30f);
			fColor = (Color)EditorGUILayout.ColorField("Color",fColor);

			if(fColorRef != fColor || fogDensityRef != fDensity )
			{
				UpdateFog(2);
				if(lightingProfile) lightingProfile.fogColor = fColor;
				if(lightingProfile) lightingProfile.fogDensity = fDensity;
				if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
			}
		}
		//-----Height--------------------------------------------------------------------
		if (vFog == CustomFog.Height)
		{
			float fDistanceRef = fDistance;
			float fHeightRef = fHeight;
			Color fColorRef = fColor;
			float fheightDensityRef = fheightDensity;

			fDistance = (float)EditorGUILayout.FloatField("Start Distance",fDistance);
			fHeight = (float)EditorGUILayout.FloatField("Height",fHeight);
			fheightDensity = (float)EditorGUILayout.Slider("Height Density",fheightDensity,0,1f);
			fColor = (Color)EditorGUILayout.ColorField("Color",fColor);

			if(fHeightRef != fHeight || fheightDensityRef != fheightDensity || fColorRef != fColor || fDistanceRef != fDistance)
			{
				UpdateFog(0);
				if(lightingProfile) lightingProfile.fogHeight = fHeight;
				if(lightingProfile) lightingProfile.fogHeightDensity = fheightDensity;
				if(lightingProfile) lightingProfile.fogColor = fColor;
				if(lightingProfile) lightingProfile.fogDistance = fDistance;

				if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
			}
		}
		//-----Global Fog Type--------------------------------------------------------------------
		if(vfogRef != vFog)
		{
			if (vFog == CustomFog.Height) 
			{
				UpdateFog(0);
			}
			if (vFog == CustomFog.Distance) 
			{
				UpdateFog(1);
			}if (vFog == CustomFog.Global) 
			{
				UpdateFog(2);
			}
			if (vFog == CustomFog.Off) 
			{
				UpdateFog(3);
			}
			//-------------------------------------------------------------------
			if(lightingProfile) lightingProfile.fogMode = vFog;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}

		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

		#endregion

		#region Depth of Field
		if (helpBox)
			EditorGUILayout.HelpBox ("Activate Depth Of Field for the camera", MessageType.Info);

		var dofTypeRef = dofType;
		var dofDistanceRef = dofDistance;

		dofType = (DOFType)EditorGUILayout.EnumPopup("Depth Of Field",dofType,GUILayout.Width(343));

		if (dofType == DOFType.On) 
			dofDistance = (float)EditorGUILayout.Slider("Distance",dofDistance,0,3f);

		if(dofTypeRef != dofType || dofDistanceRef != dofDistance)
		{
			
			// Depth of Field
			if (dofType == DOFType.On) 
			{
				postProcessingProfile.depthOfField.enabled = true;

				DepthOfFieldModel m = postProcessingProfile.depthOfField;
				DepthOfFieldModel.Settings s = postProcessingProfile.depthOfField.settings;	

				s.aperture = 0.9f;
				s.focalLength = 12f;
				s.focusDistance = dofDistance;
				s.kernelSize = DepthOfFieldModel.KernelSize.Small;
				s.useCameraFov = false;
				m.settings = s;
			}
			if (dofType == DOFType.Off) 
				postProcessingProfile.depthOfField.enabled = false;

			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.dofType = dofType;
			if(lightingProfile) lightingProfile.dofDistance = dofDistance;

			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

		#endregion

		#region Buttons

		var automodeRef = autoMode;

		if (helpBox)
			EditorGUILayout.HelpBox ("Automatic lightmap baking", MessageType.Info);


		autoMode = EditorGUILayout.Toggle ("Auto Mode", autoMode);

		if(automodeRef != autoMode)
		{
			// Auto Mode
			if(autoMode)
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
			else
				Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
			//----------------------------------------------------------------------
			if(lightingProfile) lightingProfile.automaticLightmap = autoMode;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}

		// Start bake
		if (!Lightmapping.isRunning) {
			
			if (helpBox)
				EditorGUILayout.HelpBox ("Bake lightmap", MessageType.Info);
			
			if (GUILayout.Button ("Bake")) 
			{
				if (!Lightmapping.isRunning) {
					Lightmapping.BakeAsync ();
				}
			}

			if (helpBox)
				EditorGUILayout.HelpBox ("Clear lightmap data", MessageType.Info);
			
			if(GUILayout.Button("Clear"))
			{
				Lightmapping.Clear ();
			}
		}else {

			if (helpBox)
				EditorGUILayout.HelpBox ("Cancel lightmap baking", MessageType.Info);
			
			if (GUILayout.Button ("Cancel")) {
				if (Lightmapping.isRunning) {
					Lightmapping.Cancel ();
				}
			}
		}

		if (Input.GetKey (KeyCode.F)) {
			if (Lightmapping.isRunning)
				Lightmapping.Cancel ();
		}
		if (Input.GetKey (KeyCode.LeftControl) && Input.GetKey (KeyCode.E)) {
			if (!Lightmapping.isRunning)
				Lightmapping.BakeAsync ();
		}

		if (helpBox) {
			EditorGUILayout.HelpBox ("Bake : Shift + B", MessageType.Info);
			EditorGUILayout.HelpBox ("Cancel : Shift + C", MessageType.Info);
			EditorGUILayout.HelpBox ("Clear : Shift + E", MessageType.Info);

		}
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();

		if (helpBox)
			EditorGUILayout.HelpBox ("Open unity Lighting Settings window", MessageType.Info);
		
		if (GUILayout.Button ("Lighting Window")) {

			EditorApplication.ExecuteMenuItem("Window/Lighting/Settings");
		}

		if (helpBox)
			EditorGUILayout.HelpBox ("Debug scene and peoject lighting settings automaticity", MessageType.Info);
		
		if (GUILayout.Button ("Debug Lighting")) 
		{
			showLogs = !showLogs;
		}		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

		#endregion

		#region Log System
		// Log window
		//===================================================
		if (showLogs) 
		{
			myFoldoutStyle = new GUIStyle(GUI.skin.button);
			redColor = new Color32 (184, 26, 26, 255);
			myFoldoutStyle.normal.textColor = redColor;
			myFoldoutStyle.fontStyle = FontStyle.Bold;
			myFoldoutStyle.fontStyle = FontStyle.Bold;

			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			CheckColorSpace ();

			CheckLightingMode ();

			CheckSettings ();

			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();

		}


		EditorGUILayout.EndScrollView();

		EditorUtility.SetDirty (this);		EditorGUILayout.Space (); GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});EditorGUILayout.Space ();

            		#endregion

	}

	#region Debug Lighting
	public void CheckColorSpace()
	{
		if (PlayerSettings.colorSpace == ColorSpace.Gamma) {
			if (GUILayout.Button ("Prefered color space is Linear, Current is Gamma", myFoldoutStyle))
				PlayerSettings.colorSpace = ColorSpace.Linear;
		}
	}

	public void CheckLightingMode()
	{
		Light[] lights = GameObject.FindObjectsOfType<Light> ();
		foreach (Light l in lights) 
		{
			// Check realtime state light types
			if (Lightmapping.realtimeGI == true) 
			{
				SerializedObject serialLightSource = new SerializedObject(l);
				SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
				if (SerialProperty.intValue == 2) {
					
					if (GUILayout.Button (l.name + " : Change light type to realtime in realtime lighting mode", myFoldoutStyle)) {
						SerialProperty.intValue = 1;
						serialLightSource.ApplyModifiedProperties ();
					}
				}
			}

			// Check baked state light types
			if (Lightmapping.bakedGI == true) 
			{
				SerializedObject serialLightSource = new SerializedObject(l);
				SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
				if (SerialProperty.intValue == 4)
				{
					if (GUILayout.Button (l.name + " : Change light type to Baked/Mixed in Baked lighting mode", myFoldoutStyle)) {
						SerialProperty.intValue = 2;
						serialLightSource.ApplyModifiedProperties ();
					}
				}
			}

			if (vLight != VolumetricLightType.Off) {

				if(vLight != VolumetricLightType.OnlyDirectional)
				{
					if (!l.GetComponent<VolumetricLight> ())
					{
						if (GUILayout.Button (l.name + " : Don't has VolumetricLight compoennt", myFoldoutStyle)) {
							l.gameObject.AddComponent<VolumetricLight> ();

							l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;
							if (l.type == LightType.Directional) {
								if (vLightLevel == VLightLevel.Level1)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
								if (vLightLevel == VLightLevel.Level2)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
								if (vLightLevel == VLightLevel.Level3)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
								if (vLightLevel == VLightLevel.Level4)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
							}
							else
							{
								if(vLightLevel == VLightLevel.Level1)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
								if(vLightLevel == VLightLevel.Level2)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
								if(vLightLevel == VLightLevel.Level3)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
								if(vLightLevel == VLightLevel.Level4)
									l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
							}

							l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
							l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
							l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
							l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
							l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
							l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;
							if (l.type == LightType.Directional)
								l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
							else {
								l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

								if (l.type == LightType.Spot) {
									if (l.range == 10f)
										l.range = 43f;
									if (l.spotAngle == 30f)
										l.spotAngle = 43f;
								}
							}

							l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
							l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
							l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
							l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
							l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
						}
					}
				}
				if(vLight == VolumetricLightType.OnlyDirectional)
				{
					if (l.type == LightType.Directional) {
						if (!l.GetComponent<VolumetricLight> ()) {
							if (GUILayout.Button (l.name + " : Don't has VolumetricLight compoennt", myFoldoutStyle)) {
								l.gameObject.AddComponent<VolumetricLight> ();

								l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;
								if (l.type == LightType.Directional) {
									if (vLightLevel == VLightLevel.Level1)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
									if (vLightLevel == VLightLevel.Level2)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
									if (vLightLevel == VLightLevel.Level3)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
									if (vLightLevel == VLightLevel.Level4)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
								}
								else
								{
									if(vLightLevel == VLightLevel.Level1)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
									if(vLightLevel == VLightLevel.Level2)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
									if(vLightLevel == VLightLevel.Level3)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
									if(vLightLevel == VLightLevel.Level4)
										l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
								}

								l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
								l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
								l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
								l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
								l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
								l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;
								if (l.type == LightType.Directional)
									l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
								else {
									l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

									if (l.type == LightType.Spot) {
										if (l.range == 10f)
											l.range = 43f;
										if (l.spotAngle == 30f)
											l.spotAngle = 43f;
									}
								}

								l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
								l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
								l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
								l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
								l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
							}
						}
					}
				}
			}
		}
		if (!sunLight)
		{
			if (GUILayout.Button ("Sunlight could not be found", myFoldoutStyle))
				EditorApplication.ExecuteMenuItem("GameObject/Light/Directional Light");
		}
	}

	void CheckSettings()
	{
		Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

		foreach (Camera c in cams) {
			if (c.allowMSAA) {
				if (GUILayout.Button ("Disable MSAA in camera component", myFoldoutStyle))
					c.allowMSAA = false;
			}
			if (!c.allowHDR) {
				if (GUILayout.Button ("Enable Allow HDR in camera component", myFoldoutStyle))
					c.allowHDR = true;
			}

			if(vLight != VolumetricLightType.Off)
			{
				if (!c.GetComponent<VolumetricLightRenderer> ()) {
					if (GUILayout.Button (c.name + ": VolumetricLightRenderer component is missing on camera", myFoldoutStyle))
						c.gameObject.AddComponent<VolumetricLightRenderer> ();
				}
			}
			if(vLight == VolumetricLightType.Off)
			{
				if (c.GetComponent<VolumetricLightRenderer> ()) {
					if (GUILayout.Button (c.name + ": VolumetricLightRenderer component is not necessary", myFoldoutStyle))
						c.gameObject.AddComponent<VolumetricLightRenderer> ();
				}
			}
		}

		if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
		   EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ||
		   EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL) {
			GUILayout.Label ("Current build target is not compatible for next-gen effects");
			if (GUILayout.Button ("Switch to PC", myFoldoutStyle))
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone ,BuildTarget.StandaloneWindows64);
			if (GUILayout.Button ("Switch to PS4", myFoldoutStyle))
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.PS4 ,BuildTarget.PS4);
			if (GUILayout.Button ("Switch to Xbox One", myFoldoutStyle))
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.XboxOne ,BuildTarget.XboxOne);
			if (GUILayout.Button ("Switch to OSX Universal", myFoldoutStyle))
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone ,BuildTarget.StandaloneOSXUniversal);
		}
	}
           	#endregion

	#region Global Fog Update

	void UpdateFog(int fogType) // 0 Height , 1 Distance , 2 Global , 3 Off
	{

		//-------Height---------------------------------------------------------------------
		if (fogType == 0) {
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				if (!c.gameObject.GetComponent<GlobalFog> ())
				{
					c.gameObject.AddComponent<GlobalFog> ();
					c.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = true;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;

					c.gameObject.GetComponent<GlobalFog> ().heightDensity = fheightDensity;
					c.gameObject.GetComponent<GlobalFog> ().height = fHeight;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = false;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;

				} else {
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = true;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;

					c.gameObject.GetComponent<GlobalFog> ().heightDensity = fheightDensity;
					c.gameObject.GetComponent<GlobalFog> ().height = fHeight;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = false;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;

				}
			}
		}
		//----------------------------------------------------------------------------

		//-------Distance---------------------------------------------------------------------
		if (fogType == 1) {
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				if (!c.gameObject.GetComponent<GlobalFog> ())
				{
					c.gameObject.AddComponent<GlobalFog> ();
					c.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = true;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = false;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = true;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;
				} else {
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = true;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = false;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = true;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;
				}
			}
		}
		//----------------------------------------------------------------------------

		//-------Global---------------------------------------------------------------------
		if (fogType == 2) {
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				if (!c.gameObject.GetComponent<GlobalFog> ())
				{
					c.gameObject.AddComponent<GlobalFog> ();
					c.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = false;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = true;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;
				} else {
					c.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
					c.gameObject.GetComponent<GlobalFog> ().heightFog = false;
					c.gameObject.GetComponent<GlobalFog> ().startDistance = fDistance;
					c.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
					c.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
					RenderSettings.fog = true;
					RenderSettings.fogColor = fColor;
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fDensity/1000;
				}
			}
		}
		//----------------------------------------------------------------------------

		//-------Off---------------------------------------------------------------------
		if (fogType == 3) {
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) 
			{
				if (c.gameObject.GetComponent<GlobalFog> ()) {
					DestroyImmediate (c.gameObject.GetComponent<GlobalFog> ());
					RenderSettings.fog = false;
				}
			}
		}
		//----------------------------------------------------------------------------
	}

	#endregion

	#region Update Settings
	void UpdateSettings()
	{
		if (sunFlare)
		{
			if(sunLight)
				sunLight.flare = sunFlare;
		}

		if(skyBox)
			RenderSettings.skybox = skyBox;
		
		// Update Lighting Mode
		if (lightingMode == LightingMode.RealtimeGI) {
			Lightmapping.realtimeGI = true;
			Lightmapping.bakedGI = false;
			LightmapEditorSettings.giBakeBackend = LightmapEditorSettings.GIBakeBackend.Radiosity;
		}
		if (lightingMode == LightingMode.Baked)
		{
			Lightmapping.realtimeGI = false;
			Lightmapping.bakedGI = true;
			LightmapEditorSettings.giBakeBackend = LightmapEditorSettings.GIBakeBackend.PathTracer;
		}
		if (lightingMode == LightingMode.FullyRealtime) {
			Lightmapping.realtimeGI = false;
			Lightmapping.bakedGI = false;
		}
		//----------------------------------------------------------------------
		// Update Ambient

		if (ambientLight == AmbientLight.Color) {
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight = ambientColor;
		}
		else
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

		//----------------------------------------------------------------------
		if (lightSettings == LightSettings.Baked) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				SerializedObject serialLightSource = new SerializedObject(l);
				SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
				SerialProperty.intValue = 2;
				serialLightSource.ApplyModifiedProperties ();
			}
		} 
		if (lightSettings == LightSettings.Realtime) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				SerializedObject serialLightSource = new SerializedObject(l);
				SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
				SerialProperty.intValue = 4;
				serialLightSource.ApplyModifiedProperties ();
			}
		}
		if (lightSettings == LightSettings.Mixed) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				SerializedObject serialLightSource = new SerializedObject(l);
				SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
				SerialProperty.intValue = 1;
				serialLightSource.ApplyModifiedProperties ();
			}

		}
		//----------------------------------------------------------------------
		// Color Space
		if (colorSpace == MyColorSpace.Gamma) 
			PlayerSettings.colorSpace = ColorSpace.Gamma;
		else
			PlayerSettings.colorSpace = ColorSpace.Linear;
		//----------------------------------------------------------------------
		// Render Path
		Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
		foreach(Camera c in allCameras)
		{
			if (renderPath == Render_Path.Forward) 
				c.renderingPath = RenderingPath.Forward;
			if (renderPath == Render_Path.Deferred) 
				c.renderingPath = RenderingPath.DeferredShading;
			if (renderPath == Render_Path.Default) 
				c.renderingPath = RenderingPath.UsePlayerSettings;

			c.allowHDR = true;
			c.allowMSAA = false;
			c.useOcclusionCulling = true;
		}
		//----------------------------------------------------------------------
		// Volumetric Light
		if (vLight != VolumetricLightType.Off) {

			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				
				if (!c.gameObject.GetComponent<VolumetricLightRenderer> ()) {
					c.gameObject.AddComponent<VolumetricLightRenderer> ();
					c.gameObject.GetComponent<VolumetricLightRenderer> ().Resolution = VolumetricLightRenderer.VolumtericResolution.Quarter;
					c.gameObject.GetComponent<VolumetricLightRenderer> ().DefaultSpotCookie = Resources.Load ("spot_Cookie_") as Texture;
				}
			}

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (!l.gameObject.GetComponent<VolumetricLight> ())
					l.gameObject.AddComponent<VolumetricLight> ();

					l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;
					if (l.type == LightType.Directional) {
						if (vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
						if (vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
						if (vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
						if (vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
					}
					else
					{
						if(vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
						if(vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
						if(vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
						if(vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
					}

					l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
					l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
					l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
					l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
					l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
					l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;
					if (l.type == LightType.Directional)
						l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
					else {
						l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

						if (l.type == LightType.Spot) {
							if (l.range == 10f)
								l.range = 43f;
							if (l.spotAngle == 30f)
								l.spotAngle = 43f;
						}
					}

					l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
					l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
			}
		}
		if (vLight == VolumetricLightType.Off) {
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				if (c.gameObject.GetComponent<VolumetricLightRenderer> ())
					DestroyImmediate (c.gameObject.GetComponent<VolumetricLightRenderer> ());
			}

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (l.gameObject.GetComponent<VolumetricLight> ())
					DestroyImmediate(l.gameObject.GetComponent<VolumetricLight> ());
			}
		}

		// Shadows
		if (psShadow == LightsShadow.AllLightsSoft) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (l.type == LightType.Directional)
					l.shadows = LightShadows.Soft;

				if (l.type == LightType.Spot)
					l.shadows = LightShadows.Soft;

				if (l.type == LightType.Point)
					l.shadows = LightShadows.Soft;
			}
		}
		if (psShadow == LightsShadow.AllLightsHard) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (l.type == LightType.Directional)
					l.shadows = LightShadows.Hard;

				if (l.type == LightType.Spot)
					l.shadows = LightShadows.Hard;

				if (l.type == LightType.Point)
					l.shadows = LightShadows.Hard;
			}
		}
		if (psShadow == LightsShadow.OnlyDirectionalSoft) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (l.type == LightType.Directional)
					l.shadows = LightShadows.Soft;

				if (l.type == LightType.Spot)
					l.shadows = LightShadows.None;

				if (l.type == LightType.Point)
					l.shadows = LightShadows.None;
			}
		}
		if (psShadow == LightsShadow.OnlyDirectionalHard) {

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				if (l.type == LightType.Directional)
					l.shadows = LightShadows.Hard;

				if (l.type == LightType.Spot)
					l.shadows = LightShadows.None;

				if (l.type == LightType.Point)
					l.shadows = LightShadows.None;
			}
		}
		if (psShadow == LightsShadow.Off) {
			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) 
			{
				if (l.type == LightType.Directional)
					l.shadows = LightShadows.Hard;

				if (l.type == LightType.Spot)
					l.shadows = LightShadows.None;

				if (l.type == LightType.Point)
					l.shadows = LightShadows.None;
			}
		}
		//----------------------------------------------------------------------

		// Depth of Field
		if (dofType == DOFType.On) {
			postProcessingProfile.depthOfField.enabled = true;

			DepthOfFieldModel m = postProcessingProfile.depthOfField;
			DepthOfFieldModel.Settings s = postProcessingProfile.depthOfField.settings;	

			s.aperture = 0.9f;
			s.focalLength = 12f;
			s.focusDistance = dofDistance;
			s.kernelSize = DepthOfFieldModel.KernelSize.Small;
			s.useCameraFov = false;
			m.settings = s;
		}
		if (dofType == DOFType.Off) 
			postProcessingProfile.depthOfField.enabled = false;
		//----------------------------------------------------------------------

		// Light Probes
		if (lightprobeMode == LightProbeMode.Blend) {

			MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

			foreach (MeshRenderer mr in renderers) 
			{
				if (!mr.gameObject.isStatic) {
					if (mr.gameObject.GetComponent<LightProbeProxyVolume> ())
						DestroyImmediate (mr.gameObject.GetComponent<LightProbeProxyVolume> ());
					mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
				}
			}
		}
		if (lightprobeMode == LightProbeMode.Proxy) {

			MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

			foreach (MeshRenderer mr in renderers) {

				if (!mr.gameObject.isStatic) {
					if(!mr.gameObject.GetComponent<LightProbeProxyVolume> ())
						mr.gameObject.AddComponent<LightProbeProxyVolume> ();
					mr.gameObject.GetComponent<LightProbeProxyVolume> ().resolutionMode = LightProbeProxyVolume.ResolutionMode.Custom;
					mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.UseProxyVolume;
				}
			}
		}
		//----------------------------------------------------------------------
		// Auto Mode
		if(autoMode)
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
		else
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
		//----------------------------------------------------------------------

		// Global Fog
		if (vFog == CustomFog.Distance)
		{
			UpdateFog(1);
			if(lightingProfile) lightingProfile.fogDistance = fDistance;
			if(lightingProfile) lightingProfile.fogColor = fColor;
			if(lightingProfile) lightingProfile.fogDensity = fDensity;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		if (vFog == CustomFog.Global)
		{
			UpdateFog(2);
			if(lightingProfile) lightingProfile.fogColor = fColor;
			if(lightingProfile) lightingProfile.fogDensity = fDensity;
			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		//-----Height--------------------------------------------------------------------
		if (vFog == CustomFog.Height)
		{
			UpdateFog(0);
			if(lightingProfile) lightingProfile.fogHeight = fHeight;
			if(lightingProfile) lightingProfile.fogHeightDensity = fheightDensity;
			if(lightingProfile) lightingProfile.fogColor = fColor;
			if(lightingProfile) lightingProfile.fogDistance = fDistance;

			if(lightingProfile) EditorUtility.SetDirty(lightingProfile);
		}
		//-----Global Fog Type--------------------------------------------------------------------

		if (vFog == CustomFog.Height) 
		{
			UpdateFog(0);
		}
		if (vFog == CustomFog.Distance) 
		{
			UpdateFog(1);
		}if (vFog == CustomFog.Global) 
		{
			UpdateFog(2);
		}
		if (vFog == CustomFog.Off) 
		{
			UpdateFog(3);
		}
		//-------------------------------------------------------------------
		if(lightingProfile) lightingProfile.fogMode = vFog;
		if(lightingProfile) EditorUtility.SetDirty(lightingProfile);



	}
	#endregion

	#region Find Sun light
	// Find latest directional as sun light source
	void FindSun()
	{
		if (!sunLight) 
		{
			if (!RenderSettings.sun) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional) {
						sunLight = l;

						if (lightingProfile.sunColor != Color.clear)
							sunColor = lightingProfile.sunColor;
						else
							sunColor = Color.white;

						sunLight.shadowNormalBias = 0.05f;  
						sunLight.color = sunColor;
						if (sunLight.bounceIntensity == 1f)
							sunLight.bounceIntensity = 1.7f;
					}
				}
			} else 
			{				
				sunLight = RenderSettings.sun;

				if (lightingProfile.sunColor != Color.clear)
					sunColor = lightingProfile.sunColor;
				else
					sunColor = Color.white;

				sunLight.shadowNormalBias = 0.05f;  
				sunLight.color = sunColor;
				if (sunLight.bounceIntensity == 1f)
					sunLight.bounceIntensity = 1.7f;
			}
		}
	}

	#endregion

	#region On Load
	// load saved data based on project and scene name
	void OnLoad()
	{
		if (lightingProfile) 
		{

			lightingMode = lightingProfile.lightingMode;
			if (lightingProfile.skyBox)
				skyBox = lightingProfile.skyBox;
			else
				skyBox = RenderSettings.skybox;
			sunFlare = lightingProfile.sunFlare;
			ambientLight = lightingProfile.ambientLight;
			renderPath = lightingProfile.renderPath;
			lightSettings = lightingProfile.lightSettings;
			sunColor = lightingProfile.sunColor;
			colorSpace = lightingProfile.colorSpace;
			vLight = lightingProfile.volumetricLight;
			psShadow = lightingProfile.lightsShadow;
			vFog = lightingProfile.fogMode;
			fDistance = lightingProfile.fogDistance;
			fHeight = lightingProfile.fogHeight;
			fheightDensity = lightingProfile.fogHeightDensity;
			fColor = lightingProfile.fogColor;
			fDensity = lightingProfile.fogDensity;
			dofType = lightingProfile.dofType;
			dofDistance = lightingProfile.dofDistance;

			bakedResolution = lightingProfile.bakedResolution;
			ambientColor = lightingProfile.ambientColor;
			sunIntensity = lightingProfile.sunIntensity;
			autoMode = lightingProfile.automaticLightmap;
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();
			foreach (Camera c in cams)
			{
				c.allowHDR = true;
				c.allowMSAA = false;
			}
			if (lightingProfile.postProcessingProfile)
				postProcessingProfile = lightingProfile.postProcessingProfile;
		}

		UpdatePostEffects ();

		UpdateSettings ();

		FindSun ();
	}
#endregion

	#region Update Post Effects Settings
	public void UpdatePostEffects()
	{
		if (postProcessingProfile){
			Camera[] cams = GameObject.FindObjectsOfType<Camera> ();

			foreach (Camera c in cams) {
				if (!c.GetComponent<PostProcessingBehaviour> ()) {
					c.gameObject.AddComponent<PostProcessingBehaviour> ();
					c.GetComponent<PostProcessingBehaviour> ().profile = postProcessingProfile;
				} else 
				{
					c.GetComponent<PostProcessingBehaviour> ().profile = postProcessingProfile;
				}
			}
		}
	}
	#endregion

	#region Scene Delegate

	string currentScene;
	void SceneChanging ()
	{
		if (currentScene != EditorSceneManager.GetActiveScene().name)
		{
			if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name)))
				lightingProfile = Resources.Load ("DefaultSettings")as LightingProfile;
			else 
				lightingProfile = (LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LightingProfile));

			OnLoad ();
			currentScene = EditorSceneManager.GetActiveScene().name;
		}
	}
	#endregion
}
	
#endif