using UnityEngine;
using System.Collections;

public class TimedObjectDetroyer : MonoBehaviour {

	public float destroyTime = 3f;

	IEnumerator Start () {
		yield return new WaitForSeconds (destroyTime);
		Destroy (gameObject);
	}
}
