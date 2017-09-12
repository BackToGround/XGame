using UnityEngine;
using System.Collections;

public class AntiRoll : MonoBehaviour {

	public WheelCollider WheelL;
	public WheelCollider WheelR;
	public float AntiRollValue = 3000;

	// Update is called once per frame
	void FixedUpdate ()
	{
		WheelHit hit;
		float travelL = 1.0F;
		float travelR = 1.0F;

		bool groundedL = WheelL.GetGroundHit (out hit);
		if (groundedL) {
			travelL = (-WheelL.transform.InverseTransformPoint (hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
		}
		bool groundedR = WheelR.GetGroundHit (out hit);
		if (groundedR) {
			travelR = (-WheelR.transform.InverseTransformPoint (hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
		}

		float antiRollForce = (travelL - travelR) * AntiRollValue;
		if(groundedL){
			GetComponent<Rigidbody>().AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);
		}
		if(groundedR){
			GetComponent<Rigidbody>().AddForceAtPosition(WheelR.transform.up * -antiRollForce, WheelR.transform.position);
		}
	}
}

