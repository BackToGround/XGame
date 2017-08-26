using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Hypereal
{
#pragma warning disable 0219

    public enum HyMirrorType
    {
        Left = 0,
        Right,
        Stereo,
        Left_Distorted,
        Right_Distorted,
        Stereo_Distorted
    };

    public enum HyMirrorMode
    {
        Adaption = 0,
        Stretch,
        Crop
    }

    public enum HyRecenterType
    {
        Recenter_None = 0,
        Recenter_Position = 1,
        Recenter_Rotation_Roll = 2,     //rotate along z
        Recenter_Rotation_Pitch = 4,    //rotate along x
        Recenter_Rotation_Yaw = 8,      //rotate along y
        Recenter_Rotation = Recenter_Rotation_Roll | Recenter_Rotation_Pitch | Recenter_Rotation_Yaw,
        Recenter_All = Recenter_Position | Recenter_Rotation,
    }

    public class HyperealVR
    {
        public static string minimumUnityVersion = "5.2.0f3";
        public static bool SupportedUnityVersion = false;
        #region Hypreal VR Instance
        static private HyperealVR _instance;
        static public HyperealVR Instance
        {
            get
            {
                if (_instance == null && !IsQuiting)
                {
                    SupportedUnityVersion = (HyVersion.Compare(Application.unityVersion, minimumUnityVersion) >= 0);
                    if(!SupportedUnityVersion)
                        Debug.LogError("Unsupported unity version: " + Application.unityVersion +
                            ". The minimum unity version supported is: " + minimumUnityVersion + ".");

                    _instance = new HyperealVR();
                }
                return _instance;
            }
        }

        // enable or disable stereo
        public bool EnableStereo
        {
            set
            {
                if(!SupportedUnityVersion)
                    return;

                if (value == true) Instance.StartVR();
                else Instance.StopVR();
            }
        }

        // check stereo is enabled or disabled
        static public bool IsStereoEnabled { get { return _instance != null && _instance.stereoEnabled; } }

        // checking hypereal vr device is present or not, then decide whether we should use hypereal vr plugin or not.
        static public bool IsHyperealPresent { get { return HyperealApi.IsPresent(); } }

        #endregion

        #region Properties
        public HyDeviceInfo DeviceInfo;

        public HyTrackingOrigin TrackingOrigin
        {
            get { return trackingOrigin; }
            set
            {
                if (trackingOrigin == value) return;
                trackingOrigin = value;
                if(IsStereoEnabled)
                    HyperealApi.SetTrackingOrigin(TrackingOrigin);
            }
        }

        public HyMirrorMode MirrorMode = HyMirrorMode.Adaption;
        public HyMirrorType MirrorType = HyMirrorType.Right;
        public bool IsTrackingEnabled { get; private set; }

        public bool IsPausing { get; private set; }
        public bool IsVisible { get; private set; }

        public float IPD
        {
            get { return (HyRender.Instance != null ? HyRender.Instance.IPD : 0.0f); }
            set { if (HyRender.Instance != null) HyRender.Instance.IPD = value; }
        }
        public float FOV
        {
            get { return (HyRender.Instance != null ? HyRender.Instance.FOV : 0.0f); }
            set { if (HyRender.Instance != null) HyRender.Instance.FOV = value; }
        }
        public float PixelDensity
        {
            get { return (HyRender.Instance != null ? HyRender.Instance.PixelDensity : 0.0f); }
            set { if (HyRender.Instance != null) HyRender.Instance.PixelDensity = value; }
        }
        public float RenderScale
        {
            get { return (HyRender.Instance != null ? HyRender.Instance.RenderScale : 1.0f); }
            set { if (HyRender.Instance != null) HyRender.Instance.RenderScale = value; }
        }
        public void EnableSettingUI(bool bEn)
        {
            if (HyRender.Instance == null)
                return;
            HyRender.Instance.settingUIGO.SetActive(bEn);
        }
        #endregion

        #region Message Event from SDK
        public delegate void NewPoseHandler();
        public static event NewPoseHandler OnNewPose;

        public delegate void PlayZoneStateHandler(HyDevice deviceId, float distance, Vector3 forward);
        public static event PlayZoneStateHandler OnPlayZoneStateChanged;

        public delegate void HyMsgHandler(HyMsg.MsgData msg);
        public static event HyMsgHandler OnHyMessage;

        public delegate void VisibilityHandler();
        public static event VisibilityHandler OnVisibilityChange;
        #endregion

        #region Private Members
        bool stereoEnabled = false;

        HyDevice[] trackingDevice;
        HyTrackingState[] trackingState;
        HyTrackingState dummyState = new HyTrackingState();
        Dictionary<HyDevice, HyTrackingState> trackedState;
        Dictionary<HyDevice, HyTrackingState> rawTrackedState;
        
        Coroutine hapticOp = null;

        HyMsg.MsgData msgData = new HyMsg.MsgData();
   
        #region Rebase
        Vector3 originPos = Vector3.zero;
        Quaternion originInvOri = Quaternion.identity;
        #endregion

        HyPlayZone playZone = null;

        HyTrackingOrigin trackingOrigin = HyTrackingOrigin.Tracking_Floor;

        internal static bool IsQuiting { get; private set; }
        #endregion

        #region Functions
        public HyTrackingState GetTrackingState(HyDevice device)
        {
            if (trackedState.ContainsKey(device))
                return trackedState[device];
            return dummyState;
        }
        public HyTrackingState GetTrackingStateRaw(HyDevice device)
        {
            if (rawTrackedState.ContainsKey(device))
                return rawTrackedState[device];
            return dummyState;
        }
        public static void IssueRenderEvent(int eventID)
        {
            if (IsStereoEnabled)
#if UNITY_5_0 || UNITY_5_1
                GL.IssuePluginEvent(eventID);
#else
                GL.IssuePluginEvent(HyperealApi.GetRenderEventHandler(), eventID);
#endif
        }
        public static HyDevice GetDeviceTypeID(HyDevice devType, int idx)
        {
            return (HyDevice)((int)devType + idx);
        }
        public static HyDevice GetDeviceType(HyDevice devTypeID)
        {
            int iid = (int)devTypeID;
            return (HyDevice)(iid - iid % 0x100);
        }
        public static int GetDeviceIndex(HyDevice devTypeID)
        {
            return ((int)devTypeID % 0x100);
        }
        public static HyInput GetInputDevice(HyDevice device)
        {
            if (IsQuiting)
                return null;
            return HyInputManager.Instance.GetInputDevice(device);
        }
        public HyInputPointer GetSenderPointer(GameObject receiver)
        {
            HyInputModule inputSystem = GameObject.FindObjectOfType<HyInputModule>();
            if (inputSystem == null)
                return null;
            foreach(var pointer in inputSystem.pointers)
            {
                if (pointer.IsSenderOf(receiver))
                    return pointer;
            }
            return null;
        }
        public void SetHapticFeedback(HyDevice device, float vibraDuration, float strength)
        {
            vibraDuration *= 1000.0f;    //milliseconds
            strength = Mathf.Clamp(strength, 0.0f, 1.0f);
            HyperealApi.SetHapticVibration(device, vibraDuration, strength);
        }
        public void TriggerHapticPulse(HyDevice device, float vibraDuration, float strength, float duration, float pulse)
        {
            if (HyRender.Instance == null)
                return;

            if (hapticOp != null) HyRender.Instance.StopCoroutine(hapticOp);
            hapticOp = HyRender.Instance.StartCoroutine(TriggerHapticPulseEnum(device, vibraDuration, strength, duration, pulse));
        }
        public void RecenterBase(HyRecenterType type)
        {
            HyTrackingState[] head = new HyTrackingState[] { new HyTrackingState()};
            HyDevice[] device = new HyDevice[] { HyDevice.Device_HMD0 };
            HyperealApi.GetTrackingStates(head, device, 1);
            if((type & HyRecenterType.Recenter_Position) != 0)
                originPos = head[0].pose.position;

            if (CheckNan(head[0].pose.position))
                head[0].pose.position = Vector3.zero;
            if (CheckNan(head[0].pose.orientation))
                head[0].pose.orientation = Quaternion.identity;

            Vector3 eulers = head[0].pose.orientation.eulerAngles;

            if ((type & HyRecenterType.Recenter_Rotation_Roll) == 0)
                eulers.z = 0.0f;
            if ((type & HyRecenterType.Recenter_Rotation_Pitch) == 0)
                eulers.x = 0.0f;
            if ((type & HyRecenterType.Recenter_Rotation_Yaw) == 0)
                eulers.y = 0.0f;
            if ((type & HyRecenterType.Recenter_Rotation) != 0)
                originInvOri = Quaternion.Inverse(Quaternion.Euler(eulers));
        }
        void RebaseTransfrom(ref HyTrackingState pose, HyTrackingState rawPose)
        {
            pose = rawPose;

            pose.pose.position = originInvOri * (pose.pose.position - originPos);
            pose.pose.orientation = originInvOri * pose.pose.orientation;

            pose.velocity = originInvOri * pose.velocity;
            pose.acceleration = originInvOri * pose.acceleration;
            pose.angularVelocity = originInvOri * pose.angularVelocity;
            pose.angularAcceleration = originInvOri * pose.angularAcceleration;
        }
        static bool CheckNan(Vector3 v)
        {
            return (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z));
        }
        static bool CheckNan(Quaternion q)
        {
            return (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w));
        }
        internal void ApplyTrackingPose()
        {
            bool isPausing = !IsVisible || HyLoadingHelper.loading;
            if (!Application.runInBackground && isPausing) return;

            uint trackedDeviceCount = (uint)trackingDevice.Length;
            HyperealApi.GetTrackingStates(trackingState, trackingDevice, trackedDeviceCount);
            for (uint i = 0; i < trackedDeviceCount; i++)
            {
                if (CheckNan(trackingState[i].pose.position))
                    trackingState[i].pose.position = Vector3.zero;
                if (CheckNan(trackingState[i].pose.orientation))
                    trackingState[i].pose.orientation = Quaternion.identity;

                HyDevice device = trackingDevice[i];

                rawTrackedState[device] = trackingState[i];

                if (trackingState[i].isConnected())
                {
                    HyTrackingState state = trackedState[device];
                    RebaseTransfrom(ref state, trackingState[i]);
                    trackedState[device] = state;
                }                    
            }
            if (OnNewPose != null)
                OnNewPose();
        }
        internal HyDevice[] GetTrackedDevice() { return trackingDevice; }        
        #endregion

        #region Mono
        HyperealVR()
        {
            IsPausing = false;
            IsVisible = true;
            _instance = this;
            if(SupportedUnityVersion)
                StartVR();
        }

        internal static void Destory()
        {
            IsQuiting = true;
            if(_instance != null)
                _instance.StopVR();
            _instance = null;
        }

        void StartVR()
        {
            if (stereoEnabled)
                return;

            if(!HyVersion.IsDllMatchedPlugin())
            {
                Debug.LogWarning(
                    "[Hypereal] Oops, it seems the Hypereal VR plugin's version does not match the dll version!"
                    + " Please restart the Unity Editor and reimport the Hypereal VR plugin."
                    );
            }

            HyResult hrResult = (HyResult)HyperealApi.Initialize();
            if (hrResult == HyResult.HyRequestQuit)
            {
                Application.Quit();
                return;
            }

            stereoEnabled = (hrResult == (int)HyResult.HySucess);
#if UNITY_EDITOR
            HyperealApi.SetInEditor(true);
#endif

            DeviceInfo = new HyDeviceInfo();

            if (IsStereoEnabled)
            {
                bool trackingEnabled = true;
                HyperealApi.GetDeviceInfo(ref DeviceInfo);
                HyperealApi.GetTrakcingEnable(ref trackingEnabled);

                IsTrackingEnabled = trackingEnabled;

                int count = 0;
                HyperealApi.GetPlayAreaVertexCount(ref count);
                if (count > 0)
                {
                    if (playZone == null)
                        playZone = new HyPlayZone();
                    playZone.Points = new Vector2[count];
                    HyperealApi.GetPlayAreaVertex(playZone.Points, count);
                }
                TrackingOrigin = trackingOrigin;
            }

            trackingDevice = new HyDevice[5] { HyDevice.Device_HMD0, HyDevice.Device_Controller0, HyDevice.Device_Controller1,
                HyDevice.Device_Tracker0, HyDevice.Device_Tracker1 };
            trackingState = new HyTrackingState[trackingDevice.Length];
            trackedState = new Dictionary<HyDevice, HyTrackingState>();
            rawTrackedState = new Dictionary<HyDevice, HyTrackingState>();
            for (int i = 0; i < trackingDevice.Length; i++)
            {
                trackingState[i] = new HyTrackingState();
                trackedState.Add(trackingDevice[i], new HyTrackingState());
                rawTrackedState.Add(trackingDevice[i], new HyTrackingState());
            }

            Application.targetFrameRate = -1;
            Application.runInBackground = true;
            QualitySettings.maxQueuedFrames = -1;
            // please don't set the vSyncCount in code. 
            // The mirror texture will be flushed by unity even the mirror texture is not used in anywhere 
            // when the setting in code and in Project Settings->Quality->VSync does not match
            // which will casue crash as the mirror texture is also used in our native SDK.
            //
            //QualitySettings.vSyncCount = 0; 
        }

        void StopVR()
        {
            stereoEnabled = false;
#if UNITY_EDITOR
            // Only call this in Editor, because we cannot guarantee the time graphic stuffs work completed before call it.
            HyperealApi.Shutdown();
#endif
        }

        internal void Update()
        {
            if (!IsStereoEnabled)
                return;

            UpdateMsg();

            // for game thread
            ApplyTrackingPose();

            HyInputManager.Instance.Update();

            if (playZone != null)
                playZone.UpdateState(OnPlayZoneStateChanged);
        }

        void UpdateMsg()
        {
            if (!IsStereoEnabled)
                return;

            HyperealApi.RetrieveMsg(ref msgData);
            HyMsg.MsgType msgType = (HyMsg.MsgType)msgData.header.type;
            if (msgType == HyMsg.MsgType.HY_MSG_NONE)
                return;
            switch(msgType)
            {
                case HyMsg.MsgType.HY_MSG_PENDING_QUIT:
                    Application.Quit();
                    break;
                case HyMsg.MsgType.HY_MSG_INPUT_FOCUS_CHANGED:
                    // Since currently we don't have a scene to use this 
                    // event, just remove the usage of IsPausing
                    IsPausing = (msgData.focusChange.id != (int)HyMsg.MsgType.HY_MSG_SELF);
                    break;
                case HyMsg.MsgType.HY_MSG_VIEW_FOCUS_CHANGED:
                    var oldIsVisible = IsVisible;
                    IsVisible = (msgData.focusChange.id == (int)HyMsg.MsgType.HY_MSG_SELF);

                    if (oldIsVisible != IsVisible)
                    {
                        if (OnVisibilityChange != null) OnVisibilityChange();
                    }
                    break;
                case HyMsg.MsgType.HY_MSG_IPD_CHANGED:
                    break;
            }

            if (OnHyMessage != null) OnHyMessage(msgData);
        }

        IEnumerator TriggerHapticPulseEnum(HyDevice device, float vibraDuration, float strength, float duration, float pulse)
        {
            if (duration <= 0.0f)
                yield break;
            while (duration > 0.0f)
            {
                SetHapticFeedback(device, vibraDuration, strength);
                yield return new WaitForSeconds(pulse);
                duration -= pulse;
            }
        }
        #endregion
    }
}

