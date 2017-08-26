using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hypereal
{
#pragma warning disable 0168

    public class HyRender : MonoBehaviour
    {
        static private HyRender _instance;
        static public HyRender Instance
        {
            get
            {
                if (_instance == null && !HyperealVR.IsQuiting)
                {
                    _instance = GameObject.FindObjectOfType<HyRender>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("[HyperealVR - Render]").AddComponent<HyRender>();
                        GameObject.DontDestroyOnLoad(_instance);
                    }
                }
                return _instance;
            }
        }

        #region Properties
        float pixelDensity = 1.0f;
        float overrideIPD = -1.0f;
        float overrideFOV = -1.0f;
        bool fovChanged = false;
        bool pixelDensityChanged = false;
        float renderScale = 1.0f;        

        HyFovPort[] EyeDefaultFOV;
        HyFovPort[] EyeRenderFOV;
        HyPosef[] EyeRenderPose;
        WaitForEndOfFrame waitForEndOfFrame;

        List<HyCamera> cameras = new List<HyCamera>();
        RenderTexture[] sceneTexture;

        internal GameObject settingUIGO = null;        
        #endregion

        #region Internal Functions
        internal float IPD
        {
            get { return overrideIPD; }
            set { overrideIPD = value; }
        }

        internal float FOV
        {
            get { return overrideFOV; }
            set { if (overrideFOV != value) fovChanged = true; overrideFOV = value; }
        }

        internal float PixelDensity
        {
            get { return pixelDensity; }
            set { float p = Mathf.Clamp(value, 0.3f, 3.0f); if (pixelDensity != p) pixelDensityChanged = true; pixelDensity = p; }
        }

        internal float RenderScale
        {
            get { return renderScale; }
            set
            {
                renderScale = Mathf.Clamp(value, 0.0001f, 10000.0f);
                foreach(var c in cameras)
                {
                    if (c.origin != null)
                        c.origin.transform.localScale = new Vector3(renderScale, renderScale, renderScale);
                }
            }
        }

        internal RenderTexture MirrorTexture = null;
        internal Material MirrorBlitMat = null;

        internal RenderTexture GetSceneTexture(HyEyeType eye, bool hdr) { return sceneTexture[(int)eye * 2 + (hdr ? 1 : 0)]; }
        internal void SetSceneTexture(HyEyeType eye, bool hdr, RenderTexture tex) { sceneTexture[(int)eye * 2 + (hdr ? 1 : 0)] = tex; }

        internal static void AddCamera(HyCamera c)
        {
            if (HyperealVR.IsQuiting)
                return;
            
            Instance.cameras.Add(c);
            Instance.cameras.Sort((lhs, rhs) =>
            {
                Camera lc = lhs.GetComponent<Camera>();
                Camera rc = rhs.GetComponent<Camera>();
                return lc.depth.CompareTo(rc.depth);
            });
        }

        internal static void RemoveCamera(HyCamera c)
        {
            if (HyperealVR.IsQuiting || _instance == null)
                return;

            Instance.cameras.Remove(c);
        }

        public static HyCamera GetLastCamera()
        {
            if (HyperealVR.IsQuiting || _instance == null)
                return null;
            int len = _instance.cameras.Count;
            if (len > 0)
                return _instance.cameras[len - 1];
            return null;
        }
        #endregion

        #region Mono
        private void OnApplicationQuit()
        {
            HyperealVR.Destory();
        }

        private void Awake()
        {
            //initialize render part
            var hyInst = HyperealVR.Instance;

            waitForEndOfFrame = new WaitForEndOfFrame();

            EyeDefaultFOV = new HyFovPort[(int)HyEyeType.EyeType_Count];
            EyeRenderFOV = new HyFovPort[(int)HyEyeType.EyeType_Count];
            EyeRenderPose = new HyPosef[(int)HyEyeType.EyeType_Count];
            sceneTexture = new RenderTexture[(int)HyEyeType.EyeType_Count * 2];
            for (int i = 0; i < 2; i++)
            {
                EyeDefaultFOV[i] = new HyFovPort();
                EyeRenderFOV[i] = new HyFovPort();
                EyeRenderPose[i] = new HyPosef();

                HyEyeType eyeType = (HyEyeType)i;
                Vector2 resolution = Vector2.zero;
                HyperealApi.GetEyeResolution(eyeType, ref resolution);
                HyperealApi.GetEyeRawFovPort(eyeType, ref EyeDefaultFOV[i]);

                float vtan = Mathf.Max(EyeDefaultFOV[i].Up, EyeDefaultFOV[i].Down);
                float htan = vtan * (resolution.x / resolution.y);

                EyeDefaultFOV[i].Up = vtan;
                EyeDefaultFOV[i].Down = vtan;
                EyeDefaultFOV[i].Left = htan;
                EyeDefaultFOV[i].Right = htan;

                EyeRenderFOV[i] = EyeDefaultFOV[i];
                HyperealApi.ConfigureRendering(eyeType, ref EyeDefaultFOV[i]);
            }

            CreateMirrorTexture();

            RenderScale = 1.0f;

            GameObject temp = Resources.Load<GameObject>("Prefabs/HySettingUI");
            if (temp != null)
            {
                settingUIGO = Instantiate(temp) as GameObject;
                settingUIGO.transform.SetParent(this.transform, false);
                settingUIGO.SetActive(false);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(RenderLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            HyperealApi.ReleaseMirrorTexture();
            _instance = null;
        }

        private void Update()
        {
            HyperealVR.Instance.Update();
        }
        #endregion

        #region Render
        internal IEnumerator RenderLoop()
        {
            while (true)
            {
                yield return waitForEndOfFrame;

                if (!HyperealVR.IsStereoEnabled)
                    continue;

                bool isPausing = !HyperealVR.Instance.IsVisible || HyLoadingHelper.loading;

                if (isPausing) continue;

                HyCamera hyCam = GetLastCamera();
                if(hyCam != null)
                {
                    bool srgb = true;
                    if (hyCam.GetComponent<Camera>().isHDR() == true && QualitySettings.activeColorSpace == ColorSpace.Linear) 
                        srgb = false;
                    HyperealApi.ConfigureGraphic(srgb, PixelDensity);
                }

                //apply the tracking states.
                HyperealVR.Instance.ApplyTrackingPose();

                //get the eye render pose
                HyTrackingState headPose = HyperealVR.Instance.GetTrackingStateRaw(HyDevice.Device_HMD0);

                //override ipd and fov support
                if (headPose.isConnected())
                {
                    HyperealApi.GetEyeRenderPose(headPose.pose, IPD, ref EyeRenderPose);
                }

                //render for eyes.
                RenderEye(HyEyeType.EyeType_Left, fovChanged);
                RenderEye(HyEyeType.EyeType_Right, fovChanged);

                fovChanged = false;
                pixelDensityChanged = false;
            }
        }

        void RenderEye(HyEyeType eye, bool fovChanged)
        {
            int eyeIdx = (int)eye;

            // override fov
            HyDeviceInfo devInfo = HyperealVR.Instance.DeviceInfo;
            float aspect = (float)devInfo.resolutionX * 0.5f / (float)devInfo.resolutionY;
            if (FOV > 0.0f)
            {
                float vtan = Mathf.Tan(Mathf.Deg2Rad * FOV * 0.5f);
                float htan = vtan * aspect;
                EyeRenderFOV[eyeIdx].Up = vtan;
                EyeRenderFOV[eyeIdx].Down = vtan;
                EyeRenderFOV[eyeIdx].Left = htan;
                EyeRenderFOV[eyeIdx].Right = htan;
            }
            else
                EyeRenderFOV[eyeIdx] = EyeDefaultFOV[eyeIdx];

            if (fovChanged)
                HyperealApi.ConfigureRendering(eye, ref EyeRenderFOV[eyeIdx]);

            foreach (var hyCam in cameras)
            {
                // @@kouzhe: allow to use multiple hyCamera
                //HyCamera hyCam = GetLastCamera();
                if (hyCam != null)
                {
                    Camera cam = hyCam.GetComponent<Camera>();

                    int orgCullingMask = cam.cullingMask;
                    if (hyCam.StereoCullingMask) cam.cullingMask = (eye == HyEyeType.EyeType_Left ? hyCam.CullingMaskLeft : hyCam.CullingMaskRight);

                    ConfigRenderTexture(eye, cam.isHDR());

                    cam.targetTexture = GetSceneTexture(eye, cam.isHDR());

                    HyFovPort fov = EyeRenderFOV[eyeIdx];
                    cam.fieldOfView = Mathf.Atan(fov.Up) * 2.0f * Mathf.Rad2Deg;
                    cam.aspect = aspect;

                    cam.transform.localPosition = EyeRenderPose[eyeIdx].position;
                    cam.transform.localRotation = EyeRenderPose[eyeIdx].orientation;

                    float oldNearZ = cam.nearClipPlane;
                    float oldFarZ = cam.farClipPlane;

                    cam.nearClipPlane = Mathf.Clamp(oldNearZ * cam.transform.lossyScale.z, 0.0001f, 10.0f);
                    cam.farClipPlane = Mathf.Clamp(oldFarZ * cam.transform.lossyScale.z, 10.0f, 320000.0f);

                    //update head camera's z-clip plane use for raycast for HyInputPointer
                    Camera headCam = hyCam.GetHeadCamera();
                    if (headCam != null)
                    {
                        headCam.nearClipPlane = cam.nearClipPlane;
                        headCam.farClipPlane = cam.farClipPlane;
                    }

                    Matrix4x4 projMat = Matrix4x4.identity;
                    HyperealApi.GetProjectMatrix(ref projMat, ref fov, cam.nearClipPlane, cam.farClipPlane);
                    cam.projectionMatrix = projMat;

                    hyCam.CurrentRenderedEye = eye;
                    cam.Render();

                    cam.nearClipPlane = oldNearZ;
                    cam.farClipPlane = oldFarZ;

                    cam.cullingMask = orgCullingMask;
                }
            }
        }

        void ConfigRenderTexture(HyEyeType eye, bool hdr)
        {
            //configure render texture
            RenderTexture renderEyeTexture = GetSceneTexture(eye, hdr);
            if (!pixelDensityChanged && renderEyeTexture != null)
                return;

            Vector2 size = Vector2.zero;
            HyperealApi.GetEyeResolution(eye, ref size);
            int w = (int)size.x;
            int h = (int)size.y;
            if (w == 0 || h == 0)
                return;

            int aa = QualitySettings.antiAliasing == 0 ? 1 : QualitySettings.antiAliasing;
            var format = hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;

            if (renderEyeTexture != null)
            {
                if (renderEyeTexture.width != w || renderEyeTexture.height != h || renderEyeTexture.antiAliasing != aa || renderEyeTexture.format != format)
                {
                    GameObject.Destroy(renderEyeTexture);
                    renderEyeTexture = null;
                }
            }

            if (renderEyeTexture == null)
            {
#if UNITY_5_6_OR_NEWER
                renderEyeTexture = new RenderTexture(w, h, 24, format);
#else 
                renderEyeTexture = new RenderTexture(w, h, 0, format);
#endif
                renderEyeTexture.name = ("EyeTexture" + (int)eye);
                renderEyeTexture.antiAliasing = aa;
                renderEyeTexture.wrapMode = TextureWrapMode.Clamp;
                renderEyeTexture.Create();
            }
            SetSceneTexture(eye, hdr, renderEyeTexture);
        }

        void CreateMirrorTexture()
        {
            if (!HyperealVR.IsStereoEnabled)
                return;

            HyDeviceInfo devInfo = HyperealVR.Instance.DeviceInfo;
            if (devInfo.resolutionX > 0 && devInfo.resolutionY > 0)
            {
                if (MirrorTexture == null)
                {
                    MirrorTexture = new RenderTexture(devInfo.resolutionX, devInfo.resolutionY, 0);
                    if(MirrorTexture.Create())
                        HyperealApi.SetMirrorTexture(MirrorTexture.GetNativeTexturePtr(), devInfo.resolutionX, devInfo.resolutionY);
                }

                if (MirrorBlitMat == null)
                    MirrorBlitMat = new Material(Shader.Find("HyperealVR/Mirror"));
            }
        }
#endregion

    }
}

