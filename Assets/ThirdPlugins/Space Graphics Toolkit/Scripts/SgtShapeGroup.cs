using UnityEngine;
using System.Collections.Generic;

public class SgtShapeGroup : MonoBehaviour
{
	[Tooltip("The shapes associated with this group")]
	public List<SgtShape> Shapes;

	// Returns a 0..1 value, where 1 is full density
	public float GetDensity(Vector3 worldPosition)
	{
		var highestDensity = 0.0f;

		if (Shapes != null)
		{
			for (var i = Shapes.Count - 1; i >= 0; i--)
			{
				var shape = Shapes[i];

				if (shape != null)
				{
					var density = shape.GetDensity(worldPosition);

					if (density > highestDensity)
					{
						highestDensity = density;
					}
				}
			}
		}

		return highestDensity;
	}
}