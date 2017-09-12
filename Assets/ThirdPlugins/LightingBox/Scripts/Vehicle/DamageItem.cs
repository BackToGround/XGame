using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageItem : MonoBehaviour {

	public float damageVelocity = 30f,releaseTime = 10f;
	Transform[] childs;
	public bool ignoreCarCollision = true;
	void Start()
	{
		childs = GetComponentsInChildren<Transform> ();

	}
	void OnCollisionEnter(Collision col)
	{
		if (col.collider.tag == "Car") {
			if (col.relativeVelocity.magnitude >= damageVelocity) {
				Destroy( GetComponent<BoxCollider> ());
				foreach (Transform t in childs) {	
					if (t != transform) {
						if (!t.GetComponent<Rigidbody> ()) {
							t.gameObject.AddComponent<Rigidbody> ();
							t.GetComponent<Rigidbody> ().mass = 100f;
							t.GetComponent<Rigidbody> ().collisionDetectionMode = CollisionDetectionMode.Continuous;

						/*	// Calculate Angle Between the collision point and the player
							Vector3 dir = col.contacts[0].point - transform.position;
							// We then get the opposite (-Vector3) and normalize it
							dir = -dir.normalized;
							// And finally we add force in the direction of dir and multiply it by force. 
							// This will push back the player
							t.GetComponent<Rigidbody> ().AddForce(dir*col.relativeVelocity.magnitude);*/
						}
						if (!t.GetComponent<MeshCollider> ()) {
							t.gameObject.AddComponent<MeshCollider> ();
							t.GetComponent<MeshCollider> ().convex = true;
							if(ignoreCarCollision)
								Physics.IgnoreCollision(col.collider.GetComponent<BoxCollider> (),t.GetComponent<MeshCollider>());
							WheelCollider[] wc = col.collider.GetComponentsInChildren<WheelCollider> ();
							for(int a = 0;a<wc.Length;a++)
								Physics.IgnoreCollision(wc[a],t.GetComponent<MeshCollider>());
						}
					}
				}

				StopCoroutine ("ReleaseCollisions");
				StartCoroutine ("ReleaseCollisions");
			}
		}
	}

	IEnumerator ReleaseCollisions()
	{
		yield return new WaitForSeconds (releaseTime);
		foreach (Transform t in childs) 
		{
			if (t != transform) {
				Destroy (t.GetComponent<Rigidbody> ());
				Destroy (t.GetComponent<MeshCollider> ());
			}
		}
	}
}

