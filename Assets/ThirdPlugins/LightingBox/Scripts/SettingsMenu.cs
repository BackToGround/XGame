using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour {

	// Access unity post effects
	PostProcessingBehaviour[] pb;

	[Header("Effects")]
	// Settings ui elements
	public Dropdown antiAliasing;
	public Dropdown ambientOcclusion;
	public Dropdown screenSpaceReflections;
	public Dropdown depthOfField;
	public Dropdown motionBlur;
	public Dropdown bloom;
	public Dropdown chromaticAberration;
	public Dropdown vignette;

	[Header("Quality")]
	// Quality ui elements
	public Dropdown grass;
	public Dropdown terrain;
	public Dropdown textureResolution;
	public Dropdown textureAnistropic;
	public Dropdown shadows;
	public Dropdown realtimeReflections;
	public Dropdown softParticles;

	[Header("Display")]
	// Display ui elements
	public Dropdown targetFPS;
	public Dropdown vSync;
	public Dropdown fullScreen;

	// Device info
	public Text deviceInfo;

	void Start ()
	{

		// Load default game settings on first ruuning of game   
		if (PlayerPrefs.GetInt ("FirstRun") != 3) 
		{ // 3=>true , 0=>false
			PlayerPrefs.SetString ("Grass","Medium");
			PlayerPrefs.SetString ("Terrain","Medium");
			PlayerPrefs.SetString ("TextureResolution","High");
			PlayerPrefs.SetString ("TextureAnistropic","Enable");
			PlayerPrefs.SetString ("Shadows","Medium");
			PlayerPrefs.SetString ("Reflections","On");
			PlayerPrefs.SetString ("SoftParticles","On");
			PlayerPrefs.SetString ("FPS","60");
			PlayerPrefs.SetString ("VSync","On");
			PlayerPrefs.SetString ("FullScreen","On");
			PlayerPrefs.SetInt ("FirstRun",3);
		}

		deviceInfo.text = SystemInfo.graphicsDeviceName.ToString ();

		pb = GameObject.FindObjectsOfType<PostProcessingBehaviour> ();

		//  Effects  Start//////////////////////////////////////////////////////////////////////////////////////////
		// Load default settings into UI component elements
		for (int a = 0; a < pb.Length; a++) {
			PostProcessingBehaviour pbMain = Camera.main.GetComponent<PostProcessingBehaviour> ();
			// AA settings
			if (pbMain.profile.antialiasing.enabled == false)
				antiAliasing.value = 0;
			if (pbMain.profile.antialiasing.settings.method == AntialiasingModel.Method.Fxaa)
				antiAliasing.value = 1;
			if (pbMain.profile.antialiasing.settings.method == AntialiasingModel.Method.Taa)
				antiAliasing.value = 2;

			// AO settings
			if (pbMain.profile.ambientOcclusion.enabled == false)
				ambientOcclusion.value = 0;
			if (pbMain.profile.ambientOcclusion.settings.sampleCount == AmbientOcclusionModel.SampleCount.Lowest)
				ambientOcclusion.value = 1;
			if (pbMain.profile.ambientOcclusion.settings.sampleCount == AmbientOcclusionModel.SampleCount.Low)
				ambientOcclusion.value = 2;
			if (pbMain.profile.ambientOcclusion.settings.sampleCount == AmbientOcclusionModel.SampleCount.Medium)
				ambientOcclusion.value = 3;
			if (pbMain.profile.ambientOcclusion.settings.sampleCount == AmbientOcclusionModel.SampleCount.High)
				ambientOcclusion.value = 4;

			// SSR settings
			if (pbMain.profile.screenSpaceReflection.enabled == false)
				screenSpaceReflections.value = 0;
			else {
				if (pbMain.profile.screenSpaceReflection.settings.reflection.reflectionQuality == ScreenSpaceReflectionModel.SSRResolution.Low)
					screenSpaceReflections.value = 1;
				if (pbMain.profile.screenSpaceReflection.settings.reflection.reflectionQuality == ScreenSpaceReflectionModel.SSRResolution.High)
					screenSpaceReflections.value = 2;
			}

			// DOF settings
			if (pbMain.profile.depthOfField.enabled == false)
				depthOfField.value = 0;
			if (pbMain.profile.depthOfField.enabled == true)
				depthOfField.value = 1;

			// Motion Blur settings
			if (pbMain.profile.motionBlur.enabled == false)
				motionBlur.value = 0;
			if (pbMain.profile.motionBlur.enabled == true)
				motionBlur.value = 1;

			// Bloom settings
			if (pbMain.profile.bloom.enabled == false)
				bloom.value = 0;
			if (pbMain.profile.bloom.enabled == true)
				bloom.value = 1;

			// Chromattic Abberation settings
			if (pbMain.profile.chromaticAberration.enabled == false)
				chromaticAberration.value = 0;
			if (pbMain.profile.chromaticAberration.enabled == true)
				chromaticAberration.value = 1;

			// Vignette settings
			if (pbMain.profile.vignette.enabled == false)
				vignette.value = 0;
			if (pbMain.profile.vignette.enabled == true)
				vignette.value = 1;
		}

		//  Effects  End//////////////////////////////////////////////////////////////////////////////////////////

		//  Quality  Start//////////////////////////////////////////////////////////////////////////////////////////


		Terrain[] t = GameObject.FindObjectsOfType<Terrain> ();

		for (int a = 0; a < t.Length; a++) {
			// Grass
			if (PlayerPrefs.GetString ("Grass") == "Low") {
				t [a].detailObjectDensity = 0.3f;
				t [a].detailObjectDistance = 90f;
				grass.value = 0;
			}
					
			if (PlayerPrefs.GetString ("Grass") == "Medium") {
				t [a].detailObjectDensity = 0.5f;
				t [a].detailObjectDistance = 140f;
				grass.value = 1;
			}
					
			if (PlayerPrefs.GetString ("Grass") == "High") {
				t [a].detailObjectDensity = 1f;
				t [a].detailObjectDistance = 243f;
				grass.value = 2;
			}
			// Terrain
			if (PlayerPrefs.GetString ("Terrain") == "Low") {
				t [a].heightmapPixelError = 170f;
				terrain.value = 0;
			}
			if (PlayerPrefs.GetString ("Terrain") == "Medium") {
				t [a].heightmapPixelError = 73f;
				terrain.value = 1;
			}
			if (PlayerPrefs.GetString ("Terrain") == "High") {
				t [a].heightmapPixelError = 5f;
				terrain.value = 2;
			}
		}
		// Texture Resolution   
		if (PlayerPrefs.GetString ("TextureResolution") == "Low") {
			QualitySettings.masterTextureLimit = 2;
			textureResolution.value = 0;
		}
		if (PlayerPrefs.GetString ("TextureResolution") == "Medium") {
			QualitySettings.masterTextureLimit = 1;
			textureResolution.value = 1;
		}
		if (PlayerPrefs.GetString ("TextureResolution") == "High") {
			QualitySettings.masterTextureLimit = 0;
			textureResolution.value = 2;
		}

		// Texture Anistropic   
		if (PlayerPrefs.GetString ("TextureAnistropic") == "Disable") {
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			textureAnistropic.value = 0;
		}
		if (PlayerPrefs.GetString ("TextureAnistropic") == "Enable") {
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			textureAnistropic.value = 1;
		}
		if (PlayerPrefs.GetString ("TextureAnistropic") == "ForceEnable") {
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;

			textureAnistropic.value = 2;
		}


		// Shadows
		if (PlayerPrefs.GetString ("Shadows") == "Low") {
			QualitySettings.shadowResolution = ShadowResolution.Low;
			shadows.value = 0;
		}
		if (PlayerPrefs.GetString ("Shadows") == "Medium") {
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			shadows.value = 1;
		}
		if (PlayerPrefs.GetString ("Shadows") == "High") {
			QualitySettings.shadowResolution = ShadowResolution.High;
			shadows.value = 2;
		}
		if (PlayerPrefs.GetString ("Shadows") == "VeryHigh") {
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			shadows.value = 3;
		}

		// Reflections
		if (PlayerPrefs.GetString ("Reflections") == "Off") {
			QualitySettings.realtimeReflectionProbes = false;
			realtimeReflections.value = 0;
		}
		if (PlayerPrefs.GetString ("Reflections") == "On") {
			QualitySettings.realtimeReflectionProbes = true;
			realtimeReflections.value = 1;
		}
		
		// Reflections
		if (PlayerPrefs.GetString ("SoftParticles") == "Off") {
			QualitySettings.softParticles = false;
			softParticles.value = 0;
		}
		if (PlayerPrefs.GetString ("SoftParticles") == "On") {
			QualitySettings.softParticles = true;
			softParticles.value = 1;
		}
		
		//  Quality  End//////////////////////////////////////////////////////////////////////////////////////////

		//   Display settigs

		// Traget fps
		if (PlayerPrefs.GetString ("FPS") == "30") {
			Application.targetFrameRate = 30;
			targetFPS.value = 0;
		}
		if (PlayerPrefs.GetString ("FPS") == "60") {
			Application.targetFrameRate = 60;
			targetFPS.value = 1;
		}

		// Vsync
		if (PlayerPrefs.GetString ("VSync") == "On") {
			QualitySettings.vSyncCount = 1;
			vSync.value = 0;
		}
		if (PlayerPrefs.GetString ("VSync") == "Off") {
			QualitySettings.vSyncCount = 0;
			vSync.value = 1;
		}

		//  Fullscreen
		if (PlayerPrefs.GetString ("FullScreen") == "On") {
			Screen.fullScreen = true;
			fullScreen.value = 0;
		}
		if (PlayerPrefs.GetString ("FullScreen") == "Off") {
			Screen.fullScreen = false;
			fullScreen.value = 1;
		}
		/////////////////////////////////////////

	}
	/////////////////////////////////////////////////////////////////////////////////1
	// AntiAliasing
	public void Change_AntiAliasing()
	{
		StartCoroutine ("save_AntiAliasing");
	}

	IEnumerator save_AntiAliasing()
	{
		
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			AntialiasingModel m = pb [a].profile.antialiasing;

			if (antiAliasing.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("AA", "Off");
			}		
			if (antiAliasing.value == 1) 
			{				
				AntialiasingModel.Settings s = pb [a].profile.antialiasing.settings;	
				m.enabled = true;
				s.method = AntialiasingModel.Method.Fxaa;
				m.settings = s;
				PlayerPrefs.SetString ("AA", "FXAA");
			}
			if (antiAliasing.value == 2) 
			{
				AntialiasingModel.Settings s = pb [a].profile.antialiasing.settings;
				m.enabled = true;
				s.method = AntialiasingModel.Method.Taa;
				m.settings = s;
				PlayerPrefs.SetString ("AA", "TAA");
			}

			pb [a].profile.antialiasing = m;

		}
			
	}
	/////////////////////////////////////////////////////////////////////////////////2
	/// 
	/// // AmbientOcclusion
	public void Change_AmbientOcclusion()
	{
		StartCoroutine ("save_AmbientOcclusion");
	}

	IEnumerator save_AmbientOcclusion()
	{
		yield return new WaitForEndOfFrame ();


		for (int a = 0; a < pb.Length; a++) {
			AmbientOcclusionModel m = pb [a].profile.ambientOcclusion;

			if (ambientOcclusion.value == 0) {
				m.enabled = false;
				PlayerPrefs.SetString ("AO", "Off");
			}

			if (ambientOcclusion.value == 1) {
				m.enabled = true;

				AmbientOcclusionModel.Settings s = pb [a].profile.ambientOcclusion.settings;
				s.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
				m.settings = s;
				PlayerPrefs.SetString ("AO", "Lowest");
			}
			if (ambientOcclusion.value == 2) {
				m.enabled = true;

				AmbientOcclusionModel.Settings s = pb [a].profile.ambientOcclusion.settings;
				s.sampleCount = AmbientOcclusionModel.SampleCount.Low;
				m.settings = s;
				PlayerPrefs.SetString ("AO", "Low");
			}
			if (ambientOcclusion.value == 3) {
				m.enabled = true;

				AmbientOcclusionModel.Settings s = pb [a].profile.ambientOcclusion.settings;
				s.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				m.settings = s;
				PlayerPrefs.SetString ("AO", "Medium");
			}
			if (ambientOcclusion.value == 4) {
				m.enabled = true;

				AmbientOcclusionModel.Settings s = pb [a].profile.ambientOcclusion.settings;
				s.sampleCount = AmbientOcclusionModel.SampleCount.High;
				m.settings = s;
				PlayerPrefs.SetString ("AO", "High");
			}
		}
	}
	/////////////////////////////////////////////////////////////////////////////////3
	/// /// // Screen Space Reflections
	public void Change_ScreenSpaceReflections()
	{
		StartCoroutine ("save_ScreenSpaceReflections");
	}

	IEnumerator save_ScreenSpaceReflections()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			ScreenSpaceReflectionModel m = pb [a].profile.screenSpaceReflection;

			if (screenSpaceReflections.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("SSR", "Off");
			}		
			if (screenSpaceReflections.value == 1) 
			{				
				ScreenSpaceReflectionModel.Settings s = pb [a].profile.screenSpaceReflection.settings;	
				m.enabled = true;
				s.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.Low;
				m.settings = s;
				PlayerPrefs.SetString ("SSR", "Low");
			}
			if (screenSpaceReflections.value == 2) 
			{
				ScreenSpaceReflectionModel.Settings s = pb [a].profile.screenSpaceReflection.settings;
				m.enabled = true;
				s.reflection.reflectionQuality = ScreenSpaceReflectionModel.SSRResolution.High;
				m.settings = s;
				PlayerPrefs.SetString ("SSR", "High");
			}

			pb [a].profile.screenSpaceReflection = m;

		}
	}
	/////////////////////////////////////////////////////////////////////////////////4
	/// /// // Depth Of Field
	public void Change_DepthOfField()
	{
		StartCoroutine ("save_DepthOfField");
	}

	IEnumerator save_DepthOfField()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++) {
			DepthOfFieldModel m = pb [a].profile.depthOfField;

			if (depthOfField.value == 0) {
				m.enabled = false;
				PlayerPrefs.SetString ("DOF", "Off");
			}		
			if (depthOfField.value == 1) {				
				m.enabled = true;
				PlayerPrefs.SetString ("DOF", "On");
			}

			pb [a].profile.depthOfField = m;
		}
	}
	/////////////////////////////////////////////////////////////////////////////////5
	/// /// // Motion Blur
	public void Change_MotionBlur()
	{
		StartCoroutine ("save_MotionBlur");
	}

	IEnumerator save_MotionBlur()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			MotionBlurModel m = pb [a].profile.motionBlur;

			if (motionBlur.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("MotionBlur", "Off");
			}		
			if (motionBlur.value == 1) 
			{				
				m.enabled = true;
				PlayerPrefs.SetString ("MotionBlur", "On");
			}

			pb [a].profile.motionBlur = m;

		}
	}
	/////////////////////////////////////////////////////////////////////////////////7
	/// /// // Bloom
	public void Change_Bloom()
	{
		StartCoroutine ("save_Bloom");
	}

	IEnumerator save_Bloom()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			BloomModel m = pb [a].profile.bloom;

			if (bloom.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("Bloom", "Off");
			}		
			if (bloom.value == 1) 
			{				
				m.enabled = true;
				PlayerPrefs.SetString ("Bloom", "On");
			}

			pb [a].profile.bloom = m;

		}
	}
	/////////////////////////////////////////////////////////////////////////////////8
	/// /// // Chromatic Aberration
	public void Change_ChromaticAberration()
	{
		StartCoroutine ("save_ChromaticAberration");
	}

	IEnumerator save_ChromaticAberration()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			ChromaticAberrationModel m = pb [a].profile.chromaticAberration;

			if (chromaticAberration.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("ChromaticAberration", "Off");
			}		
			if (chromaticAberration.value == 1) 
			{				
				m.enabled = true;
				PlayerPrefs.SetString ("ChromaticAberration", "On");
			}

			pb [a].profile.chromaticAberration = m;

		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Vignette
	public void Change_Vignette()
	{
		StartCoroutine ("save_Vignette");
	}

	IEnumerator save_Vignette()
	{
		yield return new WaitForEndOfFrame ();

		for (int a = 0; a < pb.Length; a++)
		{
			VignetteModel m = pb [a].profile.vignette;

			if (vignette.value == 0) 
			{
				m.enabled = false;
				PlayerPrefs.SetString ("Vignette", "Off");
			}		
			if (vignette.value == 1) 
			{				
				m.enabled = true;
				PlayerPrefs.SetString ("Vignette", "On");
			}

			pb [a].profile.vignette = m;

		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Grass
	public void Change_Grass()
	{
		StartCoroutine ("save_Grass");
	}

	IEnumerator save_Grass()
	{
		yield return new WaitForEndOfFrame ();


		Terrain[] t = GameObject.FindObjectsOfType<Terrain> ();

		for (int a = 0; a < t.Length; a++) {
			if (grass.value == 0) {
				t [a].detailObjectDensity = 0.3f;
				t [a].detailObjectDistance = 90f;
				PlayerPrefs.SetString ("Grass", "Low");
			}
			if (grass.value == 1) {
				t [a].detailObjectDensity = 0.5f;
				t [a].detailObjectDistance = 140f;
				PlayerPrefs.SetString ("Grass", "Medium");
			}
			if (grass.value == 2) {
				t [a].detailObjectDensity = 1f;
				t [a].detailObjectDistance = 243f;
				PlayerPrefs.SetString ("Grass", "High");
			}
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Terrain
	public void Change_Terrain()
	{
		StartCoroutine ("save_Terrain");
	}

	IEnumerator save_Terrain()
	{
		yield return new WaitForEndOfFrame ();


		Terrain[] t = GameObject.FindObjectsOfType<Terrain> ();

		for (int a = 0; a < t.Length; a++) {
			if (terrain.value == 0) {
				t [a].heightmapPixelError = 170f;
				PlayerPrefs.SetString ("Terrain", "Low");
			}
			if (terrain.value == 1) {
				t [a].heightmapPixelError = 73f;
				PlayerPrefs.SetString ("Terrain", "Medium");
			}
			if (terrain.value == 2) {
				t [a].heightmapPixelError = 5f;
				PlayerPrefs.SetString ("Terrain", "High");
			}
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Texture Resolution
	public void Change_TextureResolution()
	{
		StartCoroutine ("save_TextureResolution");
	}

	IEnumerator save_TextureResolution()
	{
		yield return new WaitForEndOfFrame ();

		if (textureResolution.value == 0) {
			QualitySettings.masterTextureLimit = 2;
			PlayerPrefs.SetString ("TextureResolution", "Low");
		}
		if (textureResolution.value == 1) {
			QualitySettings.masterTextureLimit = 1;
			PlayerPrefs.SetString ("TextureResolution", "Medium");
		}
		if (textureResolution.value == 2) {
			QualitySettings.masterTextureLimit = 0;
			PlayerPrefs.SetString ("TextureResolution", "High");
		}

	}
	/////////////////////////////////////////////////////////////////////////////////
	// Texture Anistropic
	public void Change_TextureAnistropic()
	{
		StartCoroutine ("save_TextureAnistropic");
	}

	IEnumerator save_TextureAnistropic()
	{
		yield return new WaitForEndOfFrame ();

		if (textureAnistropic.value == 0) 
		{
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable ;
			PlayerPrefs.SetString ("TextureAnistropic", "Disable");
		}
		if (textureAnistropic.value == 1) 
		{
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable ;
			PlayerPrefs.SetString ("TextureAnistropic", "Enable");
		}
		if (textureAnistropic.value == 2)
		{
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable ;
			PlayerPrefs.SetString ("TextureAnistropic", "ForceEnable");
		}

	}
	/////////////////////////////////////////////////////////////////////////////////
	// Shadows
	public void Change_Shadows()
	{
		StartCoroutine ("save_Shadows");
	}

	IEnumerator save_Shadows()
	{
		yield return new WaitForEndOfFrame ();

		if (shadows.value == 0) 
		{
			QualitySettings.shadowResolution = ShadowResolution.Low;
			PlayerPrefs.SetString ("Shadows", "Low");
		}
		if (shadows.value == 1) 
		{
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			PlayerPrefs.SetString ("Shadows", "Medium");
		}
		if (shadows.value == 2)
		{
			QualitySettings.shadowResolution = ShadowResolution.High;
			PlayerPrefs.SetString ("Shadows", "High");
		}

		if (shadows.value == 3)
		{
			QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
			PlayerPrefs.SetString ("Shadows", "VeryHigh");
		}
	}
	//////////////////////////////////////////////////////////////////////////////
	// Realtime Reflections
	public void Change_Reflections()
	{
		StartCoroutine ("save_Reflections");
	}

	IEnumerator save_Reflections()
	{
		yield return new WaitForEndOfFrame ();

		if (realtimeReflections.value == 0) 
		{
			QualitySettings.realtimeReflectionProbes = false;
			PlayerPrefs.SetString ("Reflections", "Off");
		}
		if (realtimeReflections.value == 1) 
		{
			QualitySettings.realtimeReflectionProbes = true;
			PlayerPrefs.SetString ("Reflections", "On");
		}
	}
	/////////////////////////////////////////////////////////////////////////////////
	// Soft Particles
	public void Change_SoftParticles()
	{
		StartCoroutine ("save_SoftParticles");
	}

	IEnumerator save_SoftParticles()
	{
		yield return new WaitForEndOfFrame ();

		if (softParticles.value == 0) 
		{
			QualitySettings.softParticles = false;
			PlayerPrefs.SetString ("SoftParticles", "Off");
		}
		if (softParticles.value == 1) 
		{
			QualitySettings.softParticles = true;
			PlayerPrefs.SetString ("SoftParticles", "On");
		}
	}
	//////////////////////////////////////////////////////////////////////////////
	/// 
	/// 
	public void SetTrue(GameObject target)
	{
		target.SetActive (true);
	}
	public void SetFalse(GameObject target)
	{
		target.SetActive (false);
	}
	public void ToggleObject(GameObject target)
	{
		target.SetActive (!target.activeSelf);
	}

	// target fps 30 or 60   
	public void Change_targetFPS()
	{
		StartCoroutine ("save_targetFPS");
	}

	IEnumerator save_targetFPS()
	{
		yield return new WaitForEndOfFrame ();

		if (targetFPS.value == 0) 
		{
			Application.targetFrameRate = 30;
			PlayerPrefs.SetString ("FPS", "30");
		}
		if (targetFPS.value == 1) 
		{
			Application.targetFrameRate = 60;
			PlayerPrefs.SetString ("FPS", "60");
		}
	}
	//////////////////////////////////////////////////////////////////////////////
	/// // VSync  
	public void Change_VSync()
	{
		StartCoroutine ("save_VSync");
	}

	IEnumerator save_VSync()
	{
		yield return new WaitForEndOfFrame ();

		if (vSync.value == 0) 
		{
			QualitySettings.vSyncCount = 1;
			PlayerPrefs.SetString ("VSync", "On");
		}
		if (vSync.value == 1) 
		{
			QualitySettings.vSyncCount = 0;
			PlayerPrefs.SetString ("VSync", "Off");
		}
	}
	//////////////////////////////////////////////////////////////////////////////
	/// // full screen  
	public void Change_FullScreen()
	{
		StartCoroutine ("save_FullScreen");
	}

	IEnumerator save_FullScreen()
	{
		yield return new WaitForEndOfFrame ();

		if (fullScreen.value == 0) 
		{
			Screen.fullScreen = true;
			PlayerPrefs.SetString ("FullScreen", "On");
		}
		if (fullScreen.value == 1) 
		{
			Screen.fullScreen = false;
			PlayerPrefs.SetString ("FullScreen", "Off");
		}
	}
	//////////////////////////////////////////////////////////////////////////////
}