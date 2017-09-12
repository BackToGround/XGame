using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrigger : MonoBehaviour {

	GameManager manager;
	public Transform exitPoint;
	public Transform carParent;

	void Start () {
		manager = GameObject.FindObjectOfType<GameManager> ();	
	}
	
	void OnTriggerEnter (Collider col) {
		if (col.tag == "Player") {
			manager.canEnter = true;
			manager.exitPoint = exitPoint;
			manager.car = carParent;
		}
	}
	void OnTriggerExit (Collider col) {
		if (col.tag == "Player") {
			manager.canEnter = false;
			manager.exitPoint = exitPoint;
		}
	}
}
