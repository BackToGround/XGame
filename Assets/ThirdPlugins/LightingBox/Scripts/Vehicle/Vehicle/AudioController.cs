
// This script used for vehicle audio system

using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

public class AudioController : MonoBehaviour
{
	
	public AudioClip engineSound,startEngineSound,hornSound;

	public AudioClip[] collisionSounds;

	public string[] collisionTags;
	public bool playRandom;

	[HideInInspector]public AudioSource gearSource,collisionSource,hornSource;
	public AudioClip gearShiftClip; 
	public float gearVolume = 1f;
	public float crashVolume = 1f;
	public float crashVelocity = 10f;

	public float pitchMultiplier = 1f;

	public float PitchMin = 0.7f;

	public float PitchMax = 1.4f;

	[Space(3)]
	[Header("Gears Setup for Audio")]
	[Space(3)]
	// Maximus speed can reach
	public float nextGearSpeed = 300f;
	// Total number of gears for truck sound system
	public  int numberOfGears = 20;
	// How much time need to delay before switch to next or previous gears
	public float GearShiftDelay = 0.3f;

	// Current gear
	[HideInInspector]public int currentGear;
	// gear factor is a normalised representation of the current speed within the current gear's range of speeds.
	private float GearFactor;
	// Truck Rigidbody
	[HideInInspector]
	public float Revs;
	[HideInInspector]public AudioSource engineSource;

	bool isRevering;
	bool isGrounded;
	float currentSpeed;

	CarController carController;

	public bool useRPMSound;
	public float maxRPM = 7000f;
	float engineRPM;
	public float onFlyPitch = 2f;

	private void Start ()
	{		
		if (collisionTags.Length == 0)
			collisionTags = new string[collisionSounds.Length];
		for(int a = 0;a<collisionTags.Length;a++)
			collisionTags[a] = "Default";
		if (GetComponent<CarController> ())
			carController = GetComponent<CarController> ();
		
		engineSource = gameObject.AddComponent<AudioSource> ();

		engineSource.loop = true;
		engineSource.playOnAwake = false;
		engineSource.spatialBlend = 1f;
		engineSource.volume = 1f;   
		engineSource.hideFlags = HideFlags.HideInInspector;

		engineSource.clip = engineSound;

		engineSource.loop = true;

		engineSource.Play ();

		hornSource = gameObject.AddComponent<AudioSource> ();

		hornSource.loop = true;
		hornSource.playOnAwake = false;
		hornSource.spatialBlend = 1f;
		hornSource.volume = gearVolume;   
		hornSource.hideFlags = HideFlags.HideInInspector;
		hornSource.clip = hornSound;

		gearSource = gameObject.AddComponent<AudioSource> ();

		gearSource.loop = false;
		gearSource.playOnAwake = false;
		gearSource.spatialBlend = 1f;
		gearSource.volume = gearVolume;   
		gearSource.hideFlags = HideFlags.HideInInspector;


		collisionSource = gameObject.AddComponent<AudioSource> ();
		collisionSource.loop = false;
		collisionSource.playOnAwake = false;
		collisionSource.spatialBlend = 1f;
		collisionSource.volume = crashVolume;   
		collisionSource.hideFlags = HideFlags.HideInInspector;

		// Start gear managment system
		StartCoroutine (GearChanging ());

		DisableAudio ();
	}

	private void Update ()
	{
		engineRPM = carController.engineRPM;
		isRevering = carController.isReversing;
		isGrounded = carController.isGrounded;
		currentSpeed = carController.currentSpeed;
		carController.currentGear = currentGear;
	
		// The pitch is interpolated between the min and max values, according to the truck's revs.
		float pitch = ULerp (PitchMin, PitchMax , Revs);

		// clamp to minimum pitch (note, not clamped to max for high revs while burning out)
		pitch = Mathf.Min (PitchMax, pitch);

		if (useRPMSound) {
			if (carController.isGrounded)
				engineSource.pitch = Mathf.Abs (engineRPM / maxRPM) + 0.3f + pitch;
			else
				engineSource.pitch = Mathf.Lerp(engineSource.pitch,onFlyPitch,Time.deltaTime * 5);
		} else {
			if(carController.isGrounded)
				engineSource.pitch = pitch * pitchMultiplier;
			else
				engineSource.pitch = Mathf.Lerp(engineSource.pitch,onFlyPitch,Time.deltaTime * 5);
		}

		CalculateRevs ();

	
	}	

	private static float ULerp (float from, float to, float value)
	{
		return (1.0f - value) * from + value * to;   
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > crashVelocity)
		{
			if (!collisionSource.isPlaying)
			{				
				if (playRandom) {
					int rand = Random.Range (0, collisionSounds.Length);
					if (collisionSounds.Length > 0)
						collisionSource.PlayOneShot (collisionSounds [rand]);
				} else {//
					if (collisionSounds.Length > 0)
					{
						for(int a = 0;a<collisionTags.Length;a++)
						{
							if(collision.collider.tag == collisionTags[a])
								collisionSource.PlayOneShot (collisionSounds [a]);
						}
					}
				}
			}
		}
	}

	public void ChangeGear()
	{
		gearSource.clip = gearShiftClip;;
		gearSource.Play ();

	}

	IEnumerator GearChanging ()
	{
		while (true) 
		{
			yield return new WaitForSeconds (0.01f);
			if (!isRevering && isGrounded) 
			{
				float f = Mathf.Abs (currentSpeed / nextGearSpeed);
				float upgearlimit = (1 / (float)numberOfGears) * (currentGear + 1);
				float downgearlimit = (1 / (float)numberOfGears) * currentGear;

				// Changinbg gear down
				if (currentGear > 0 && f < downgearlimit) {
					// Reduce engine audio volume when changing gear
					engineSource.volume = 0.7f;
					carController.isChangingGear = true;
					engineSource.pitch = 0;
					ChangeGear ();
					// Delay time for changing gear down
					yield return new WaitForSeconds (0);
					engineSource.volume = 1f;
					carController.isChangingGear = false;
					currentGear--;
				}

				// Changing gear Up
				if (f > upgearlimit && (currentGear < (numberOfGears - 1))) {
					// Reduce engine audio volume when changing gear
					engineSource.volume = 0.3f;
					carController.isChangingGear = true;
					engineSource.pitch = 0;
					ChangeGear ();
					// Delay before changing gear up
					yield return new WaitForSeconds (GearShiftDelay);
					engineSource.volume = 1f;
					carController.isChangingGear = false;
					currentGear++;
				}
			}
			else 
			{
				if (isRevering && isGrounded)
					currentGear = 0;
			}
		}
	}

	// simple function to add a curved bias towards 1 for a value in the 0-1 range
	private static float CurveFactor (float factor)
	{
		return 1 - (1 - factor) * (1 - factor);
	}

	// Used for engine sound system    
	private void CalculateGearFactor ()
	{
		float f = (1 / (float)numberOfGears);
		// gear factor is a normalised representation of the current speed within the current gear's range of speeds.
		// We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
		var targetGearFactor = Mathf.InverseLerp (f * currentGear, f * (currentGear + 1), Mathf.Abs (currentSpeed / nextGearSpeed));
		GearFactor = Mathf.Lerp (GearFactor, targetGearFactor, Time.deltaTime * 5f);
	}

	// Used for engine sound system
	private void CalculateRevs ()
	{
		// calculate engine revs (for display / sound)
		// (this is done in retrospect - revs are not used in force/power calculations)
		CalculateGearFactor ();
		var gearNumFactor = currentGear / (float)numberOfGears;
		var revsRangeMin = ULerp (0f, 1f, CurveFactor (gearNumFactor));
		var revsRangeMax = ULerp (1f, 1f, gearNumFactor);
		Revs = ULerp (revsRangeMin, revsRangeMax, GearFactor);
	}

	public void DisableAudio()
	{
		engineSource.Stop();

	}
	public void EnableAudio()
	{
		StopCoroutine ("startEngine");
		StartCoroutine ("startEngine");
	}

	IEnumerator startEngine()
	{
		carController.canControl = false;
		gearSource.PlayOneShot (startEngineSound);
		yield return new WaitForSeconds (1f);
		engineSource.Play();
		carController.canControl = true;
	}

	public void StartHorn(bool hornOnOff)
	{
		if (hornSource) {
			if (hornOnOff) {
				if (!hornSource.isPlaying)
					hornSource.Play ();
			} else {
				if (hornSource.isPlaying)
					hornSource.Stop ();
			}
		}
	}
}

