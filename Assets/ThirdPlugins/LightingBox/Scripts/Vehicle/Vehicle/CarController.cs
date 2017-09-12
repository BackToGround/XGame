
using UnityEngine;
using System.Collections;

public enum WheelDriveType{
	FrontDrive,
	BackDrive,
	AllDrive
}
public class CarController : MonoBehaviour {

	public bool canControl;

	[Header("Wheels")]
	public WheelDriveType driveType = WheelDriveType.BackDrive;
	public WheelCollider[] Wheel_Colliders;

	public Transform[] Wheel_Transforms;

	public Transform steeringWheel;

	public GameObject driverModel;

	[HideInInspector] public float currentSpeed;

	[Header("Vehicle Setup")]
	public float enginePower = 1400f ;
	public float brakePower = 1400f;
	public float[] gearsPower;

	public float maxSteer = 43f;
	public float steerSpeed = 10f;
	public float maxSpeed = 74f;

	// Slip friction for hand break mode
	public float slipFriction = 0.3f;

	public Transform COM;

	float normalFriction;

	// Input values
	float throttleInput;
	float steerInput;
	bool handBrake;

	// Used for detecting reverse mode (if localVel.z <0 => reversing, if localVel.z>0 => is not reversing)    
	Vector3 velocity;
	Vector3 localVel;
	[HideInInspector]public bool isReversing;

	// Catch rigidbody
	Rigidbody rigid;

	[Header("Lights")]
	// Vehicle lights
	public Light[] brakeLights;
	public Light[] reverseLights;
	public Light[] frontLights;
	public Material backLightMaterial;

	[Header("Effects")]
	public ParticleSystem roadParticle;
	ParticleSystem.EmissionModule roadEmission;
	public float smokeSpeedLimit = 30f;

	[Header("Camera Settings")]
	public float distance = 5f;
	public float height = 3f;
	public float offset = 1f;

	[Header("Body Settings")]
	public Transform forcePoint;
	public float bodyForce = 43f;


	[HideInInspector] public bool isGrounded;
	[HideInInspector] public bool isChangingGear;
	Quaternion lastRotation;

	// Wheels visual alignment across wheel colliders
	WheelHit wHit = new WheelHit();

	IEnumerator Start()
	{

		roadEmission = roadParticle.emission;

		normalFriction = Wheel_Colliders [3].sidewaysFriction.stiffness;

		// from unity standard assets car demo
		Wheel_Colliders [0].attachedRigidbody.centerOfMass = new Vector3(0,0,0);

		rigid = GetComponent<Rigidbody> ();

		// used to smoothing smooth follow camera movement behind vehicle
		rigid.interpolation = RigidbodyInterpolation.Interpolate;

		// Set center of mass 
		rigid.centerOfMass =  COM.localPosition;

		// Used to reset flipped car rotation - press 'K' to reset the car
		while(true)
		{
			lastRotation = transform.rotation;
			yield return new WaitForSeconds(7f);
		}

	}

	void Update () 
	{
		if (canControl) 
		{		
			ForceController ();
			VehicleEngine ();
		}

		if(roadParticle)
			Smoke ();

		if(Input.GetKeyDown(KeyCode.K))
			transform.rotation = lastRotation;

			// Update current speed and multiply to 3 for better understand
		currentSpeed = rigid.velocity.magnitude * 2.23693629f;

		// Find vehicle reversing state
		velocity = rigid.velocity;
		localVel = transform.InverseTransformDirection(velocity);

		if (localVel.z < 0)
			isReversing = true;
		else
			isReversing = false;
		    
		if(Wheel_Colliders[0].GetGroundHit(out wHit))
			isGrounded = true;
		else
			isGrounded = false;
		
		// Align wheel mesh across wheel collider rotation and position
		for (int i = 0; i < Wheel_Colliders.Length; i++) 
		{
			Quaternion quat;
			Vector3 position;
			Wheel_Colliders [i].GetWorldPose (out position, out quat);
			Wheel_Transforms [i].transform.position = position;
			Wheel_Transforms [i].transform.rotation = quat;
		}

		if (steeringWheel)
			steeringWheel.rotation = 
				transform.rotation * Quaternion.Euler (10,   0,(Wheel_Colliders[0].steerAngle ) * -1.1f);

	}

	[HideInInspector]public int currentGear = 0;

	public void VehicleEngine()
	{
			if (currentSpeed >= maxSpeed)
				rigid.drag = 0.3f;
			else
				rigid.drag = 0.1f;
			
		if (driveType == WheelDriveType.BackDrive) {
			Wheel_Colliders [2].motorTorque = enginePower * throttleInput * gearsPower [currentGear];
			Wheel_Colliders [3].motorTorque = enginePower * throttleInput * gearsPower [currentGear];

			Wheel_Colliders [2].motorTorque = Mathf.Clamp (Wheel_Colliders [2].motorTorque, -enginePower / 2, enginePower);
			Wheel_Colliders [3].motorTorque = Mathf.Clamp (Wheel_Colliders [3].motorTorque, -enginePower / 2, enginePower);
		}
		if (driveType == WheelDriveType.FrontDrive) {
			Wheel_Colliders [0].motorTorque = enginePower * throttleInput * gearsPower [currentGear];
			Wheel_Colliders [1].motorTorque = enginePower * throttleInput * gearsPower [currentGear];

			Wheel_Colliders [0].motorTorque = Mathf.Clamp (Wheel_Colliders [0].motorTorque, -enginePower / 2, enginePower);
			Wheel_Colliders [1].motorTorque = Mathf.Clamp (Wheel_Colliders [1].motorTorque, -enginePower / 2, enginePower);
		}
		if (driveType == WheelDriveType.AllDrive) {
			Wheel_Colliders [0].motorTorque = enginePower * throttleInput * gearsPower [currentGear];
			Wheel_Colliders [1].motorTorque = enginePower * throttleInput * gearsPower [currentGear];
			Wheel_Colliders [2].motorTorque = enginePower * throttleInput * gearsPower [currentGear];
			Wheel_Colliders [3].motorTorque = enginePower * throttleInput * gearsPower [currentGear];

			Wheel_Colliders [0].motorTorque = Mathf.Clamp (Wheel_Colliders [0].motorTorque, -enginePower / 2, enginePower);
			Wheel_Colliders [1].motorTorque = Mathf.Clamp (Wheel_Colliders [1].motorTorque, -enginePower / 2, enginePower);
			Wheel_Colliders [2].motorTorque = Mathf.Clamp (Wheel_Colliders [2].motorTorque, -enginePower / 2, enginePower);
			Wheel_Colliders [3].motorTorque = Mathf.Clamp (Wheel_Colliders [3].motorTorque, -enginePower / 2, enginePower);
		}
			Wheel_Colliders [0].steerAngle = maxSteer * steerInput * (steerSpeed * Time.deltaTime);
			Wheel_Colliders [1].steerAngle = maxSteer * steerInput * (steerSpeed * Time.deltaTime);

			Wheel_Colliders [1].steerAngle = Mathf.Clamp (Wheel_Colliders [1].steerAngle, -(maxSteer / (currentSpeed / 30)), (maxSteer / (currentSpeed / 30)));
			Wheel_Colliders [0].steerAngle = Mathf.Clamp (Wheel_Colliders [0].steerAngle, -(maxSteer / (currentSpeed / 30)), (maxSteer / (currentSpeed / 30)));
		

		if (handBrake || (throttleInput < 0 && !isReversing) || (throttleInput > 0 && isReversing)) {
			if (handBrake) 
			{
				if (driveType == WheelDriveType.BackDrive) {
					Wheel_Colliders [2].brakeTorque = brakePower;
					Wheel_Colliders [3].brakeTorque = brakePower;
				}
				if (driveType == WheelDriveType.FrontDrive) {
					Wheel_Colliders [0].brakeTorque = brakePower;
					Wheel_Colliders [1].brakeTorque = brakePower;
				}
				if (driveType == WheelDriveType.AllDrive) {
					Wheel_Colliders [0].brakeTorque = brakePower;
					Wheel_Colliders [1].brakeTorque = brakePower;
					Wheel_Colliders [2].brakeTorque = brakePower;
					Wheel_Colliders [3].brakeTorque = brakePower;
				}
				LightIntensity (0, 1f);
				if(backLightMaterial)
					backLightMaterial.SetFloat ("_Intensity", 1f);
				LightIntensity (1, 0);
			} else {
				Wheel_Colliders [0].brakeTorque = Mathf.Abs(brakePower * throttleInput);
				Wheel_Colliders [1].brakeTorque = Mathf.Abs(brakePower * throttleInput);
				Wheel_Colliders [2].brakeTorque = Mathf.Abs(brakePower * (throttleInput / 2));
				Wheel_Colliders [3].brakeTorque = Mathf.Abs(brakePower * (throttleInput / 2));
				LightIntensity (0, 1f);
				if(backLightMaterial)
					backLightMaterial.SetFloat ("_Intensity", 1f);
				LightIntensity (1, 0);
			}
		} else {

			Wheel_Colliders [2].brakeTorque = 0;
			Wheel_Colliders [3].brakeTorque = 0;
			Wheel_Colliders [0].brakeTorque = 0;
			Wheel_Colliders [1].brakeTorque = 0;
			LightIntensity (0, 0);
			if(backLightMaterial)
				backLightMaterial.SetFloat ("_Intensity", 0.3f);
			LightIntensity (1, 0);
		}
			if (isReversing && throttleInput < 0) {

				LightIntensity (0, 0);
			if(backLightMaterial)
				backLightMaterial.SetFloat ("_Intensity", 0.3f);
				LightIntensity (1, 1f);
			}
	}

	bool isMobile;

	public void Move(float motor,float steer,bool hand)
	{
		throttleInput = motor;
		steerInput = steer;
		handBrake = hand;
	}

	void LightIntensity(int type,float value)
	{
		if (type == 0) {
			for (int a = 0; a < brakeLights.Length; a++)
				brakeLights [a].intensity = value;
		} else {
			for (int a = 0; a < reverseLights.Length; a++)
				reverseLights [a].intensity = value;
		}
	}

	public void ToggleFrontLight(bool status)
	{
		for (int a = 0; a < frontLights.Length; a++)
			frontLights [a].enabled = status;		
	}

	void Smoke()
	{
		if (currentSpeed < smokeSpeedLimit) 
		{
			if (roadEmission.enabled)
				roadEmission.enabled = false;
		}
		else    
		{
			if (!roadEmission.enabled)
				roadEmission.enabled = true;
		}
	}

	public void StopCar()
	{
		Wheel_Colliders [2].brakeTorque = brakePower;
		Wheel_Colliders [3].brakeTorque = brakePower;
		Wheel_Colliders [2].motorTorque = 0;
		Wheel_Colliders [3].motorTorque = 0;
		LightIntensity (0, 1f);
		if(backLightMaterial)
		backLightMaterial.SetFloat ("_Intensity", 1f);
		LightIntensity (1, 0);
	}

	public void SlipFriction()
	{
		WheelFrictionCurve swFriction = new WheelFrictionCurve ();

		swFriction.extremumSlip = 0;
		swFriction.extremumValue = 1f;
		swFriction.asymptoteSlip = 0.5f;
		swFriction.asymptoteValue = 0.75f;
		swFriction.stiffness = slipFriction;

		for (int a = 0; a < Wheel_Colliders.Length; a++) {
			Wheel_Colliders [a].sidewaysFriction = swFriction;
		}
	}
	public void NormalFriction()
	{
		WheelFrictionCurve swFriction = new WheelFrictionCurve ();

		swFriction.extremumSlip = 0.2f;
		swFriction.extremumValue = 1f;
		swFriction.asymptoteSlip = 0.5f;
		swFriction.asymptoteValue = 0.75f;
		swFriction.stiffness = normalFriction;

		for (int a = 0; a < Wheel_Colliders.Length; a++) {
			Wheel_Colliders [a].sidewaysFriction = swFriction;
		}

	}

	public void ForceController()
	{
		float downForce = ((-(bodyForce * 100) * (throttleInput / (currentSpeed / 10))));
		downForce = Mathf.Clamp (downForce,-bodyForce * 100, bodyForce * 100);

		if (float.IsNaN (downForce))
			return;
		if(!isChangingGear)
			rigid.AddForceAtPosition(new Vector3(0,downForce,0), forcePoint.position);
		else
			rigid.AddForceAtPosition(new Vector3(0,-downForce*2,0), forcePoint.position);

	}

	[HideInInspector]public float engineRPM;

	public void EngineRPMCalculation()
	{
		engineRPM = ((Mathf.Abs((Wheel_Colliders[2].rpm * Wheel_Colliders[2].radius) + (Wheel_Colliders[3].rpm * Wheel_Colliders[3].radius)) / 2f) / 3.25f) * gearsPower[currentGear];

	}
}


