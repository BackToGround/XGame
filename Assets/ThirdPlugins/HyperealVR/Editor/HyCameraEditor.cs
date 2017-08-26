using UnityEngine;
using System.Collections;
using UnityEditor;

#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using Hypereal;

namespace HyperealEditor
{
    [CustomEditor(typeof(HyCamera))]
    public class HyCameraEditor : Editor
    {
        // Use this for initialization
        SerializedProperty script;
        SerializedProperty stereo_mask;
        SerializedProperty left_mask;
        SerializedProperty right_mask;
        void Awake()
        {
            script = serializedObject.FindProperty("m_Script");
            stereo_mask = serializedObject.FindProperty("StereoCullingMask");
            left_mask = serializedObject.FindProperty("CullingMaskLeft");
            right_mask = serializedObject.FindProperty("CullingMaskRight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(script);
            EditorGUILayout.PropertyField(stereo_mask);
            EditorGUILayout.PropertyField(left_mask);
            EditorGUILayout.PropertyField(right_mask);

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                HyCamera inst = (HyCamera)target;
                bool isExpanded = inst.isExpanded;
                string title = isExpanded ? "Collapse" : "Expand";
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(title))
                {
                    GameObject newObj = null;
                    if (isExpanded)
                    {
                        newObj = inst.Collapse();                        
                        HyCamera ins = newObj.GetComponent<HyCamera>();
                        if(ins != null)
                        {
                            newObj.AddComponent<HyCamera>();
                            DestroyImmediate(ins);
                        }
                    }
                    else
                        newObj = inst.Expand();
                    if (isExpanded)
#if UNITY_5_3_OR_NEWER
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#else
                    if (newObj != null) EditorUtility.SetDirty(newObj);    
#endif
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}

