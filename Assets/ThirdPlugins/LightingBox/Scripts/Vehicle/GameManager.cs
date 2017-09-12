using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	[HideInInspector] public Transform car,player;
	[HideInInspector]public Transform exitPoint;
	SmoothFollow cameraScript;
	InputSystem inputSystem;

	[Header("In Car Camera")]
	public float cDistance = 5f;
	public float cHeight = 2f;
	public float cOffset = 1f;
	[Header("Out Car Camera")]
	public float pDistance = 3f;
	public float pHeight = 2f;
	public float pOffset = 1.44f;

	ControllerType inputType;
	void Start () {
		inputSystem = GetComponent<InputSystem> ();
		car = GameObject.FindGameObjectWithTag ("Car").transform;
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		cameraScript = GameObject.FindObjectOfType<SmoothFollow>();
		inputType = GameObject.FindObjectOfType<InputSystem> ().inputType;
	}
	
	bool entered;
	[HideInInspector]public bool canEnter;
	void Update () {
		
		if (canEnter) {
			
			if (Input.GetKeyDown (KeyCode.F) || Input.GetKeyDown (KeyCode.JoystickButton3)) {
				entered = true;

				if (entered) {
					player.gameObject.SetActive (false);

					car.gameObject.GetComponent<CarController> ().canControl = true;
					car.gameObject.GetComponent<AudioController> ().EnableAudio();
					car.gameObject.GetComponent<CarController> ().driverModel.SetActive (true);
					cameraScript.target = car;
					inputSystem.UpdateController (car.GetComponent<CarController> ());
					cameraScript.distance = car.gameObject.GetComponent<CarController> ().distance;
					cameraScript.height = car.gameObject.GetComponent<CarController> ().height;
					cameraScript.offset.y = car.gameObject.GetComponent<CarController> ().offset;

					canEnter = false;
				}
			}
		} else {
			if (entered) {
				if (Input.GetKeyDown (KeyCode.F) || Input.GetKeyDown (KeyCode.JoystickButton3)) {
					entered = false;

					if (!entered) {

						player.gameObject.SetActive (true);
						player.position = exitPoint.position;
						car.gameObject.GetComponent<CarController> ().canControl = false;
						car.gameObject.GetComponent<AudioController> ().DisableAudio();
						car.gameObject.GetComponent<CarController> ().driverModel.SetActive (false);
						car.gameObject.GetComponent<CarController> ().StopCar ();
						cameraScript.distance = pDistance;
						cameraScript.height = pHeight;
						cameraScript.offset.y = pOffset; 
						cameraScript.target = player;
					}
				}
			}
		}		
	}
}
