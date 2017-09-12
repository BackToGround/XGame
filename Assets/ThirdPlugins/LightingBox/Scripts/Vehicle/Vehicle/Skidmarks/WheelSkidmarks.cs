using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheelSkidmarks : MonoBehaviour 
{

	public Rigidbody carParent;
	public float startSlipValue = 0.4f;
	private Skidmarks skidmarks = null;//To hold the skidmarks object
	private int lastSkidmark = -1;//To hold last skidmarks data
	private WheelCollider wheel_col;//To hold self wheel collider
	public float markWidth = 0.275f;		// The width of the skidmarks. Should match the width of the wheel that it is used for. In meters.

	void Start()
	{
		if (!carParent)
			carParent = GetComponentInParent<Rigidbody> ();
				
		wheel_col = GetComponent<WheelCollider> ();

		if (FindObjectOfType<Skidmarks>())
			skidmarks = FindObjectOfType<Skidmarks>();
		else
			Debug.Log ("No skidmarks object found. Skidmarks will not be drawn");
	}

	void FixedUpdate () 
	{
		WheelHit GroundHit;
		wheel_col.GetGroundHit(out GroundHit );
	    var wheelSlipAmount = Mathf.Abs(GroundHit.sidewaysSlip);

		if (wheelSlipAmount > startSlipValue) {
			Vector3 skidPoint = GroundHit.point + 2 * (carParent.velocity) * Time.deltaTime;

			lastSkidmark = skidmarks.AddSkidMark (skidPoint, GroundHit.normal, wheelSlipAmount / 2.0f, lastSkidmark,markWidth);	
			//skidmarks.PlaySkidSound (true);
		} else {
			lastSkidmark = -1;				
			///skidmarks.PlaySkidSound (false);
		}
	}


}
