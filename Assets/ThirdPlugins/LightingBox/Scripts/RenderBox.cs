// orginally from unity manual - edited by ALIyerEdon
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectResolution
{
	_720P,_1080P,_4K,_8K,Custom
}
public class RenderBox : MonoBehaviour {

// Capture frames as a screenshot sequence. Images are
// stored as PNG files in a folder - these can be combined into
// a movie using image utility software (eg, QuickTime Pro).

	// The folder to contain our screenshots.
	// If the folder exists we will append numbers to create an empty folder.

	[Header("Video Settings")]
	[Space(3)]
	public string sequencePath = "C:/RenderBox/Video";
	public int frameRate = 60;
	public bool onStart;
	public KeyCode videoCaptureKey = KeyCode.F1;
	public SelectResolution videoResolution;
	[Header("Custom")]
	public int videoWidth = 1280; 
	public int videoHeight = 720;

	[Header("ScreenShot Settings")]
	[Space(3)]
	public string screenshotPath = "C:/RenderBox/ScreenShot";
	public KeyCode screenshotCaptureKey = KeyCode.F2;
	public SelectResolution screenShotResolution;
	[Header("Custom")]
	public int resWidth = 1920; 
	public int resHeight = 1080;

	[Header("Global Settings")]
	[Space(3)]
	public Camera customCamera;

	// Private variables
	bool captureNow = false;
	int captureTemp;

	void Start()
	{
		captureTemp = Time.captureFramerate;


		// Create the folder
		if(!System.IO.Directory.Exists(sequencePath))
			System.IO.Directory.CreateDirectory(sequencePath);
		if(!System.IO.Directory.Exists(screenshotPath))
			System.IO.Directory.CreateDirectory(screenshotPath);

		captureNow = onStart;

		if (captureNow)
			StartCoroutine ("RecordVideo");
		
	}


	void Update()
	{
		if (Input.GetKeyDown (videoCaptureKey)) 
		{
			captureNow = !captureNow;
			if (captureNow)
				StartCoroutine ("RecordVideo");
			else
				StopCoroutine ("RecordVideo");
		}

		if (!captureNow)
			return;
	}


	IEnumerator RecordVideo()
	{
		// Set the playback framerate (real time will not relate to game time after this).
		Time.captureFramerate = frameRate;
		while (true) {

			yield return new WaitForEndOfFrame ();
			// Append filename to folder name (format is '0005 shot.png"')
			string name = string.Format("{0}/{1:D04} shot.png", sequencePath, Time.frameCount);

			// Capture the screenshot to the specified file.
			//ScreenCapture.CaptureScreenshot(name,);
			float tScale = Time.timeScale;
			Time.timeScale = 0;

			RenderTexture rt = null;

			if(videoResolution == SelectResolution._720P)
				rt	= new RenderTexture(1280, 720, 24);
			if(videoResolution == SelectResolution._1080P)
				rt	= new RenderTexture(1920, 1080, 24);
			if(videoResolution == SelectResolution._4K)
				rt	= new RenderTexture(3840, 2160, 24);
			if(videoResolution == SelectResolution._8K)
				rt	= new RenderTexture(7680, 4320, 24);
			if(videoResolution == SelectResolution.Custom)
				rt	= new RenderTexture(videoWidth, videoHeight, 24);
			
			if(customCamera)
				customCamera.targetTexture = rt;
			else
				Camera.main.targetTexture = rt;
			
			Texture2D screenShot = null;

			if(videoResolution == SelectResolution._720P)
				screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
			if(videoResolution == SelectResolution._1080P)
				screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
			if(videoResolution == SelectResolution._4K)
				screenShot = new Texture2D(3840, 2160, TextureFormat.RGB24, false);
			if(videoResolution == SelectResolution._8K)
				screenShot = new Texture2D(7680, 4320, TextureFormat.RGB24, false);
			if(videoResolution == SelectResolution.Custom)
				screenShot = new Texture2D(videoWidth, videoHeight, TextureFormat.RGB24, false);
			
			if(customCamera)
				customCamera.Render();
			else
				Camera.main.Render();
			
			RenderTexture.active = rt;

			if(videoResolution == SelectResolution._720P)
				screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
			if(videoResolution == SelectResolution._1080P)
				screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
			if(videoResolution == SelectResolution._4K)
				screenShot.ReadPixels(new Rect(0, 0, 3840, 2160), 0, 0);
			if(videoResolution == SelectResolution._8K)
				screenShot.ReadPixels(new Rect(0, 0, 7680, 4320), 0, 0);
			if(videoResolution == SelectResolution.Custom)
				screenShot.ReadPixels(new Rect(0, 0, videoWidth, videoHeight), 0, 0);

			if(customCamera)				
				customCamera.targetTexture = null;
			else
				Camera.main.targetTexture = null;
			
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();

			System.IO.File.WriteAllBytes(name, bytes);
			Time.timeScale = tScale;
		}
		// Set the playback framerate (real time will not relate to game time after this).
		Time.captureFramerate = captureTemp;
	}

	// Capture high resolution screenshot
	// Source : http://answers.unity3d.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html

	void LateUpdate() 
	{
		if (Input.GetKeyDown(screenshotCaptureKey)) 
			TakeScreenShot ();
	}


	void TakeScreenShot()
	{
		float tScale = Time.timeScale;
		Time.timeScale = 0;

		RenderTexture rt = null;

		if(screenShotResolution == SelectResolution._720P)
			rt	= new RenderTexture(1280, 720, 24);
		if(screenShotResolution == SelectResolution._1080P)
			rt	= new RenderTexture(1920, 1080, 24);
		if(screenShotResolution == SelectResolution._4K)
			rt	= new RenderTexture(3840, 2160, 24);
		if(screenShotResolution == SelectResolution._8K)
			rt	= new RenderTexture(7680, 4320, 24);
		if(screenShotResolution == SelectResolution.Custom)
			rt	= new RenderTexture(resWidth, resHeight, 24);


		if(customCamera)
			customCamera.targetTexture = rt;
		else
			Camera.main.targetTexture = rt;

		Texture2D screenShot = null;

		if(screenShotResolution == SelectResolution._720P)
			screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
		if(screenShotResolution == SelectResolution._1080P)
			screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
		if(screenShotResolution == SelectResolution._4K)
			screenShot = new Texture2D(3840, 2160, TextureFormat.RGB24, false);
		if(screenShotResolution == SelectResolution._8K)
			screenShot = new Texture2D(7680, 4320, TextureFormat.RGB24, false);
		if(screenShotResolution == SelectResolution.Custom)
			screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

		if(customCamera)
			customCamera.Render();
		else
			Camera.main.Render();	

		RenderTexture.active = rt;

		if(screenShotResolution == SelectResolution._720P)
			screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
		if(screenShotResolution == SelectResolution._1080P)
			screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
		if(screenShotResolution == SelectResolution._4K)
			screenShot.ReadPixels(new Rect(0, 0, 3840, 2160), 0, 0);
		if(screenShotResolution == SelectResolution._8K)
			screenShot.ReadPixels(new Rect(0, 0, 7680, 4320), 0, 0);
		if(screenShotResolution == SelectResolution.Custom)
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

		if(customCamera)				
			customCamera.targetTexture = null;
		else
			Camera.main.targetTexture = null;

		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		byte[] bytes = screenShot.EncodeToPNG();
		PlayerPrefs.SetInt ("ScreenShotNumber", PlayerPrefs.GetInt ("ScreenShotNumber") + 1);
		string filename = screenshotPath + "ScreenShot"+PlayerPrefs.GetInt ("ScreenShotNumber").ToString () + ".png";

		System.IO.File.WriteAllBytes(filename, bytes);
		Time.timeScale = tScale;
	}
}
