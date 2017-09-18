using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SgtObjectPool))]
public class SgtObjectPool_Editor : SgtEditor<SgtObjectPool>
{
	protected override void OnInspector()
	{
		BeginDisabled();
			EditorGUILayout.TextField("Type", Target.TypeName);
			EditorGUILayout.IntField("Count", Target.Elements.Count);
		EndDisabled();
		EditorGUILayout.HelpBox("SgtObjectPools are not saved to your scene, so don't worry if you see it in edit mode.", MessageType.Info);
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("")]
public class SgtObjectPool : MonoBehaviour
{
	public static List<SgtObjectPool> AllObjectPools = new List<SgtObjectPool>();

	public string TypeName;

	public List<Object> Elements = new List<Object>();
	
	protected virtual void OnEnable()
	{
		AllObjectPools.Add(this);
	}

	protected virtual void OnDisable()
	{
		AllObjectPools.Remove(this);
	}

	protected virtual void OnDestroy()
	{
		for (var i = Elements.Count - 1; i >= 0; i--)
		{
			Object.DestroyImmediate(Elements[i]);
		}
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if (Application.isPlaying == false)
		{
			SgtHelper.Destroy(gameObject);
		}
	}
#endif
}

public static class SgtObjectPool<T>
	where T : Object
{
	private static SgtObjectPool pool;
	
	static SgtObjectPool()
	{
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			Debug.LogError("Attempting to use " + typeof(T).Name + " with SgtObjectPool. Use SgtComponentPool instead.");
		}
	}

	public static T Add(T entry)
	{
		return Add(entry, null);
	}

	public static T Add(T element, System.Action<T> onAdd)
	{
		if (element != null)
		{
			if (onAdd != null)
			{
				onAdd(element);
			}

			UpdateComponent(true);
			
			pool.Elements.Add(element);
		}

		return null;
	}
	
	public static T Pop()
	{
		UpdateComponent(false);
		
		if (pool != null)
		{
			var elements = pool.Elements;
			var count    = elements.Count;

			if (count > 0)
			{
				var index   = count - 1;
				var element = (T)elements[index];

				elements.RemoveAt(index);
#if UNITY_EDITOR
				if (element != null)
				{
					element.hideFlags = HideFlags.None;
				}
#endif
				return element;
			}
		}

		return null;
	}
	
	private static void UpdateComponent(bool allowCreation)
	{
		if (pool == null)
		{
			var typeName = typeof(T).Name;

			pool = SgtObjectPool.AllObjectPools.Find(p => p.TypeName == typeName);

			if (pool == null && allowCreation == true)
			{
				pool = new GameObject("SgtObjectPool<" + typeName + ">").AddComponent<SgtObjectPool>();

				if (Application.isPlaying == true)
				{
					Object.DontDestroyOnLoad(pool);
				}
#if UNITY_EDITOR
				pool.gameObject.hideFlags = HideFlags.DontSave;
#endif
			}
		}
	}
}