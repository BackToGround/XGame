using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SgtKeyboardThruster))]
public class SgtKeyboardThruster_Editor : SgtEditor<SgtKeyboardThruster>
{
	protected override void OnInspector()
	{
		DrawDefault("Groups");
	}
}
#endif

// This component handles keyboard controls of thrusters
[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Keyboard Thruster")]
public class SgtKeyboardThruster : MonoBehaviour
{
	[System.Serializable]
	public class Group
	{
		[Tooltip("The control axis used for these thrusters")]
		public string Axis;

		public bool Inverse;

		public bool Bidirectional;

		public List<SgtThruster> Thrusters;
	}
	
	public List<Group> Groups = new List<Group>();
	
	protected virtual void Update()
	{
		if (Groups != null)
		{
			for (var i = Groups.Count - 1; i >= 0; i--)
			{
				var group = Groups[i];

				if (group != null)
				{
					var throttle = Input.GetAxisRaw(group.Axis);

					if (group.Inverse == true)
					{
						throttle = -throttle;
					}

					if (group.Bidirectional == false)
					{
						if (throttle < 0.0f)
						{
							throttle = 0.0f;
						}
					}

					for (var j = group.Thrusters.Count - 1; j >= 0; j--)
					{
						var thruster = group.Thrusters[j];

						if (thruster != null)
						{
							thruster.Throttle = throttle;
						}
					}
				}
			}
		}
	}
}