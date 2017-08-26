using UnityEngine;
using System.Collections;
using UnityEditor;

#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using Hypereal;

namespace HyperealEditor
{
    [CustomEditor(typeof(HyUI))]
    public class HyUIEditor : Editor
    {
        // Use this for initialization
        HyUI hyUI = null;
        void Awake()
        {
            hyUI = (HyUI)target;
            hyUI.Initialize();
        }

        public override void OnInspectorGUI()
        {
            float maxLabelWidth = 100;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quad Size", GUILayout.MaxWidth(maxLabelWidth));
            hyUI.QuadSize = EditorGUILayout.Vector2Field("", hyUI.QuadSize);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("DPI", GUILayout.MaxWidth(maxLabelWidth));
            hyUI.DPI = EditorGUILayout.IntPopup(hyUI.DPI, new string[] { "200", "300", "400" }, new int[] { 200, 300, 400 });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Canvas Scalar", GUILayout.MaxWidth(maxLabelWidth));
            hyUI.CanvasScalar = EditorGUILayout.Slider(hyUI.CanvasScalar, 1.0f, 4.0f);
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                hyUI.UpdateParam();
#if UNITY_5_3_OR_NEWER
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
#else
                EditorUtility.SetDirty(target);
#endif
            }
        }
    }
}

