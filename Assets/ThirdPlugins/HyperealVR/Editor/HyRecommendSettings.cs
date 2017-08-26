using UnityEditor;
using UnityEngine;

namespace HyperealEditor
{
    [InitializeOnLoad]
    public class HyRecommendSettings : EditorWindow
    {
        const bool forceShow = false; // set it true in case you ignore 

        const string ignore = "ignore.";
        const string useRecommended = "Use recommended ({0})";
        const string currentValue = " (current = {0})";


        const string vSync = "Turn off V-Sync";
        const string vSyncDesc = "Since we already lock the frame in HyService, the Unity vSync count will lock the frame to the flush rate of monitor. We highly recommend to turn it off.";
        const int recommended_vSyncCount = 0;

        static HyRecommendSettings settingWindow;

        // Use this for initialization
        static HyRecommendSettings()
        {
            EditorApplication.update += Update;
        }

        // Update is called once per frame
        static void Update()
        {
            bool show =
                (!EditorPrefs.HasKey(ignore + vSync) &&
                (QualitySettings.vSyncCount != recommended_vSyncCount)) ||
                forceShow;

            if (show)
            {
                settingWindow = GetWindow<HyRecommendSettings>(true);
                settingWindow.Show();
            }

            EditorApplication.update -= Update;
        }

        public void OnGUI()
        {
            if ( ! EditorPrefs.HasKey(ignore + vSync) &&
                QualitySettings.vSyncCount != recommended_vSyncCount)
            {
                GUILayout.Label(vSync + string.Format(currentValue, QualitySettings.vSyncCount));
                EditorGUILayout.HelpBox(vSyncDesc, MessageType.Warning);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(string.Format(useRecommended, recommended_vSyncCount)))
                {
                    QualitySettings.vSyncCount = recommended_vSyncCount;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Ignore"))
                {
                    EditorPrefs.SetBool(ignore + vSync, true);
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Clear Ignores"))
            {
                EditorPrefs.DeleteKey(ignore + vSync);
            }
        }
    }
}