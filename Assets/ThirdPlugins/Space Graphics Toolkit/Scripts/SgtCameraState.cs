using UnityEngine;
using System.Collections.Generic;

public class SgtCameraState
{
	// The camera associated with this state
	public Camera Camera;

	public static T Save<T>(ref List<T> cameraStates, Camera camera)
		where T : SgtCameraState, new()
	{
		if (cameraStates == null)
		{
			cameraStates = new List<T>();
		}

		for (var i = cameraStates.Count - 1; i >= 0; i--)
		{
			var cameraState = cameraStates[i];

			if (cameraState == null)
			{
				cameraStates.RemoveAt(i); continue;
			}

			if (cameraState.Camera == null)
			{
				SgtClassPool<T>.Add(cameraState); cameraStates.RemoveAt(i); continue;
			}

			if (cameraState.Camera == camera)
			{
				return cameraState;
			}
		}

		var newCameraState = SgtClassPool<T>.Pop() ?? new T();
		
		newCameraState.Camera = camera;

		cameraStates.Add(newCameraState);

		return newCameraState;
	}
	
	public static T Restore<T>(List<T> cameraStates, Camera camera)
		where T : SgtCameraState
	{
		if (cameraStates != null)
		{
			for (var i = cameraStates.Count - 1; i >= 0; i--)
			{
				var cameraState = cameraStates[i];

				if (cameraState.Camera == camera)
				{
					return cameraState;
				}
			}
		}

		return null;
	}
	
	public static void Clear<T>(List<T> cameraStates)
		where T : SgtCameraState
	{
		if (cameraStates != null)
		{
			for (var i = cameraStates.Count - 1; i >= 0; i--)
			{
				SgtClassPool<T>.Add(cameraStates[i]);
			}

			cameraStates.Clear();
		}
	}
}