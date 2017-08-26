using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

using Hypereal;

namespace HyperealEditor
{
    [InitializeOnLoad]
    public class HyAbout : EditorWindow
    {
        static HyAbout hyperealAbout = null;

        [MenuItem("Hypereal/About")]
        static void About()
        {
            if (hyperealAbout == null)
            {
                hyperealAbout = ScriptableObject.CreateInstance<HyAbout>();
                hyperealAbout.minSize = new Vector2(400, 320);
                hyperealAbout.maxSize = hyperealAbout.minSize;
            }
            hyperealAbout.Show();
        }

        static string currentVersion;
        const string versionUrl = "";
        const string notesUrl = "";
        const string pluginUrl = "";

        static string notes =
            "   We are not just making virtual reality, we give you a change to fulfill your dream.\n" +
            "   You sit in frond of your laptop with your smartphone in your hands, everything in the reality make you feel dull and drain your imagination." +
            "Have you ever imagined a hyper reality world to give your the chance  to fulfill your dream, and to make yourself to be the god in the world." +
            "You are the architects of the planet, and we are the technicians to build your world. So, who are we? We are robot engineers, graphics gurus, artists, and game producers.\n" +
            "   We came from the reality that you are in, but we aimed to create a hyper reality for you to fit in." +
            " Close your eyes and put on HYPEREAL, then open your eyes to touch the world just made for you.\n";

        static string latestVersion = "1.1.0.0";

        Vector2 scrollPosition;

        static bool checkUpdate = true;
        static bool needUpdate = false;

        static WWW wwwVersion;
        static WWW wwwNotes;

        static bool supportedUnityVersion = false;

        static HyAbout()
        {
            currentVersion = HyVerNum.currentPluginVersion;
            wwwVersion = new WWW(versionUrl);
            wwwNotes = new WWW(notesUrl);
            EditorApplication.update += Update;

            supportedUnityVersion = HyVersion.Compare(Application.unityVersion, HyperealVR.minimumUnityVersion) >= 0;
        }

        static void Update()
        {
            if (!supportedUnityVersion)
                Debug.LogError("Unsupported unity version: " + Application.unityVersion +
                    ". The minimum unity version supported is: " + HyperealVR.minimumUnityVersion + ".");

            if (wwwVersion != null)
            {
                if (!wwwVersion.isDone)
                    return;

                if (UrlSuccess(wwwVersion))
                    latestVersion = wwwVersion.text;

                wwwVersion = null;

                if (HyVersion.Compare(latestVersion, currentVersion) > 0)
                {
                    needUpdate = true;
                }
            }

            if (wwwNotes != null)
            {
                if (!wwwNotes.isDone)
                    return;

                if (UrlSuccess(wwwNotes))
                    notes = wwwNotes.text;

                wwwNotes = null;

                if (notes != "")
                {
                    if (hyperealAbout != null)
                        hyperealAbout.Repaint();
                }
            }

            checkUpdate = false;
            EditorApplication.update -= Update;
        }

        void OnGUI()
        {
            var resourcePath = GetResourcePath();
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "logo.png");
            var rect = GUILayoutUtility.GetRect(logo.width, logo.height, GUI.skin.box);
            if (logo)
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
            EditorGUILayout.LabelField("       Version: " + currentVersion);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.TextArea(notes, GUILayout.MaxHeight(200));

            EditorGUILayout.EndScrollView();

            if (checkUpdate)
                EditorGUI.ProgressBar(new Rect(0, 260, position.width, 20), 0.5f, "Checking update...");
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Latest Version: " + latestVersion);
                if (needUpdate)
                {
                    if (GUILayout.Button("Get Latest Version"))
                    {
                        Application.OpenURL(pluginUrl);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!supportedUnityVersion)
                EditorGUILayout.HelpBox("Unsupported unity version: " + Application.unityVersion +
                    ". The minimum unity version supported is: " + HyperealVR.minimumUnityVersion + ".", MessageType.Error);
            EditorGUILayout.HelpBox("Unity VR native plugin powered by Hypereal Soft.\nCopyright (C) 2016 Hypereal, Inc.", MessageType.None);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Editor".Length) + "Textures/";
        }

        static bool UrlSuccess(WWW www)
        {
            if (!string.IsNullOrEmpty(www.error))
                return false;
            if (Regex.IsMatch(www.text, "404 not found", RegexOptions.IgnoreCase))
                return false;
            return true;
        }

    }

}
