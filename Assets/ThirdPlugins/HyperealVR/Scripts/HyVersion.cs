using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.IO;

namespace Hypereal
{
    public class HyVersion
    {
        public int major { get; private set; }
        public int minor { get; private set; }
        public int change { get; private set; }
        public char type { get; private set; }
        public int release { get; private set; }

        public static string currPluginVersion;

        private static string folder = Application.dataPath + "/Plugins";

        internal enum Platform
        {
            Editor_x86,
            Editor_x64,
            Runtime
        }

        public static bool IsDllMatchedPlugin()
        {
            UnityEngine.Debug.Log("HVR Plugin version : " + currPluginVersion);
#if UNITY_EDITOR
            string currDllVersion_x64 = GetDllVersion(Platform.Editor_x64);
            string currDllVersion_x86 = GetDllVersion(Platform.Editor_x86);
            UnityEngine.Debug.Log("HVR x64 DLL version : " + currDllVersion_x64);
            UnityEngine.Debug.Log("HVR x86 DLL version : " + currDllVersion_x86);
            return currPluginVersion == currDllVersion_x64
                    && currPluginVersion == currDllVersion_x86;
#else
            string currDllVersion = GetDllVersion(Platform.Runtime);
            UnityEngine.Debug.Log("HVR DLL version : " + currDllVersion);
            return currPluginVersion == currDllVersion;
#endif
        }

        private static string GetDllVersion(Platform platform)
        {
            string platStr = null;
#if UNITY_EDITOR
            var plat = Application.platform;
            if (plat == RuntimePlatform.WindowsEditor)
            {
                if (platform == Platform.Editor_x64)
                {
                    if (Directory.Exists(folder + "/x86_64"))
                        platStr = "/x86_64";
                    else
                        platStr = "/x64";
                }
                else if (platform == Platform.Editor_x86)
                {
                    platStr = "/x86";
                }

            }
#endif
            string pathStr = folder + platStr + "/HyperealPlugin.dll";
            if (File.Exists(pathStr))
            {
                FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(pathStr);
                if(dllVersionInfo != null)
                    return dllVersionInfo.FileVersion;
                else
                    UnityEngine.Debug.LogError("[HVR] The Dll(" + pathStr + ") is invalid, please reimport the Hypereal Plugin");
            }
            else
            {
                UnityEngine.Debug.LogError("[HVR] Cannot find the dll: " + pathStr);
            }

            return null;
        }


        public HyVersion(string unityVersion)
        {
            // Split the version string at non-numbers.
            string nonNumbers = "[^0-9]";
            string[] tokens = Regex.Split(unityVersion, nonNumbers);

            int parsedMajor = 0;
            int parsedMinor = 0;
            int parsedChange = 0;
            char parsedType = 'a';
            int parsedRelease = 1;

            if (tokens.Length >= 4)
            {
                int.TryParse(tokens[0], out parsedMajor);
                int.TryParse(tokens[1], out parsedMinor);
                int.TryParse(tokens[2], out parsedChange);
                int.TryParse(tokens[3], out parsedRelease);

                char lastChar = unityVersion.LastOrDefault(char.IsLetter);
                if (lastChar != '\0')
                    parsedType = lastChar;
            }

            major = parsedMajor;
            minor = parsedMinor;
            change = parsedChange;
            type = parsedType;
            release = parsedRelease;

            // Release candidates (rc) and final builds (f) use the same set of numbers.
            if (type == 'c')
                type = 'f';

            currPluginVersion = HyVerNum.currentPluginVersion;
        }

        public int CompareTo(HyVersion other)
        {
            int majorCheck = this.major.CompareTo(other.major);
            if (majorCheck != 0)
                return majorCheck;

            int minorCheck = this.minor.CompareTo(other.minor);
            if (minorCheck != 0)
                return minorCheck;

            int changeCheck = this.change.CompareTo(other.change);
            if (changeCheck != 0)
                return changeCheck;

            int typeCheck = this.type.CompareTo(other.type);
            if (typeCheck != 0)
                return typeCheck;

            int releaseCheck = this.release.CompareTo(other.release);
            if (releaseCheck != 0)
                return releaseCheck;

            return 0;
        }

        public int CompareTo(string version)
        {
            if (string.IsNullOrEmpty(version))
                return 1;
            return CompareTo(new HyVersion(version));
        }

        static public int Compare(string lhs, string rhs)
        {
            HyVersion lhsV = new HyVersion(lhs);
            return lhsV.CompareTo(new HyVersion(rhs));
        }
    }
}
