using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
public class PostVolumeTrigger : MonoBehaviour {

	Texture2D fadeOutTexture ;
	public float fadeSpeed = 1f;
	float alphaFadeValue = 0;
	public float flashDelay = 0.73f;
	bool fadingState = true;
	public PostProcessingProfile profile;
	PostProcessingProfile profileRef;

	void Start()
	{
		fadeOutTexture = Resources.Load ("FadeBlack")as Texture2D;
	}

	void OnGUI()
	{
		/*if (fadingState) {
			if (alphaFadeValue > 0)
				alphaFadeValue -= Time.deltaTime * fadeSpeed;
		} else {
			if (alphaFadeValue < 1f)
				alphaFadeValue += Time.deltaTime * fadeSpeed;
		}

		GUI.color = new Color(0, 0, 0, alphaFadeValue);
		GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), fadeOutTexture );*/
	}
	IEnumerator doFlash(bool entered)
	{
		fadeIn ();
		yield return new WaitForSeconds (flashDelay);
		if(entered)
			Camera.main.GetComponent<PostProcessingBehaviour> ().profile = profile;
		else
			Camera.main.GetComponent<PostProcessingBehaviour> ().profile = profileRef;
		fadeOut();
	}

	void fadeIn(){
		fadingState = false;
	}

	//--------------------------------------------------------------------

	void fadeOut(){
		fadingState = true;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "MainCamera") {
			profileRef = Camera.main.GetComponent<PostProcessingBehaviour> ().profile;
			if (fadingState)
				StartCoroutine (doFlash (true));
		}
	}
	void OnTriggerExit(Collider col)
	{
		if (col.tag == "MainCamera")
		{
			if (fadingState)
				StartCoroutine (doFlash (false));
		}
	}
}
