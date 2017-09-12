//--------------------------------------------------------------
//
//           Contact me : aliyeredon@gmail.com
//
//--------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum ControllerType
{
	Xbox360,
	Keyboard
}

public class InputSystem : MonoBehaviour
{
	CarController controller;
	public ControllerType inputType;
	GameManager gameManager;

	float motorInput,steerInput;
	bool handBrake;

	bool reversing;


	IEnumerator Start ()    
	{
		yield return new WaitForEndOfFrame ();

		controller = GameObject.FindObjectOfType<CarController> ();

		gameManager = GameObject.FindObjectOfType<GameManager> ();
	}

	void Update ()
	{

		if (!controller)
			return;

		if (inputType == ControllerType.Keyboard) {
			if (Input.GetAxis ("Vertical") > 0 || Input.GetAxis ("Vertical") < 0)
				motorInput = Input.GetAxis ("Vertical");
			else
				motorInput = 0;			
		}
		if (inputType == ControllerType.Xbox360) {
			if (Input.GetAxis ("RightTrigger") > 0)
				motorInput = Input.GetAxis ("RightTrigger");
			else {
				if (Input.GetAxis ("LeftTrigger") > 0)
					motorInput = -Input.GetAxis ("LeftTrigger");
				else
					motorInput = 0;
			}		
		}

		steerInput = Input.GetAxis ("Horizontal");


		if (Input.GetKey (KeyCode.JoystickButton1) || Input.GetKey (KeyCode.Space)) {
			handBrake = true;
			controller.SlipFriction ();
		} else {
			handBrake = false;
			controller.NormalFriction ();
		}


		if (Input.GetKey (KeyCode.H)) {
			if (gameManager.car)
				gameManager.car.GetComponent<AudioController> ().StartHorn (true);
		}
		if (Input.GetKeyUp (KeyCode.H)) {
			if(gameManager.car)
				gameManager.car.GetComponent<AudioController> ().StartHorn (false);
		}
		
		/*
		if (Input.GetKey (KeyCode.H))
			hornComponent.HornOn ();

		if (Input.GetKeyUp (KeyCode.H)  || Input.GetKeyUp(KeyCode.Space))
			hornComponent.HornOff ();

		if (Input.GetKeyDown (KeyCode.C))
			GameObject.FindObjectOfType<CameraSwitch>().NextCamera ();*/

		controller.Move (motorInput, steerInput, handBrake);
	}

	public void UpdateController(CarController carController)
	{
		controller = carController;
	}

}