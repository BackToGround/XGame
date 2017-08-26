using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Hypereal
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HyDeviceInfo
    {
        public int resolutionX;
        public int resolutionY;
        public float refreshRate;
        public float trackerFrustumFovH;
        public float trackerFrustumFovV;
        public float trackerFrustumNearZ;
        public float trackerFrustumFarZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HyPosef
    {
        public Quaternion orientation;
        public Vector3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HyTrackingState
    {
        public HyPosef pose;        
        public Vector3 angularVelocity;
        public Vector3 velocity;
        public Vector3 angularAcceleration;
        public Vector3 acceleration;        
        public int status;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HyPlayAreaRect
    {
        Vector3 v0;
        Vector3 v1;
        Vector3 v2;
        Vector3 v4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HyFovPort
    {
        public float Up;
        public float Down;
        public float Left;
        public float Right;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HyInputState
    {
        public int keyStatus;
        public int touchStatus;
        public float indexTriggerValue;
        public float sideTriggerValue;
        public float indexTriggerProximity;
        public float touchpadProximity;
        public Vector2 touchpadValue;
    }

    public enum HyResult
    {
        HySucess = 0,
        HyError = 1,
        HyNotInitialized = 2,
        HyRequestQuit = 3,

        HyInitializeFailed = 10,
        HyCreateSessionFailed = 11,
        HyCreateContextFailed = 12,

        HyDeviceNotSupported = 20,
    }

    public enum HyEyeType
    {
        EyeType_Left = 0,
        EyeType_Right = 1,
        EyeType_Count = 2,
    }

    public enum HyDevice : int
    {
        Device_Unknown = -1,
        Device_HMD = 0,
        Device_HMD0 = Device_HMD,
        Device_Controller = 0x100,
        Device_Controller0 = Device_Controller,
        Device_Controller1 = Device_Controller + 1,
        Device_Tracker = 0x200,
        Device_Tracker0 = Device_Tracker,
        Device_Tracker1 = Device_Tracker + 1
    }

    public enum HyRenderEvent
    {
        RE_SubmitLeft = 0,
        RE_SubmitRight,
    }

    public enum HyTrackingOrigin
    {
        Tracking_Unknown = 0,
        Tracking_Eye,
        Tracking_Floor,
    }

    enum HyTrackingStatus
    {
        Status_None = 0,
        Status_Connected = 1,
        Status_PoseTracked = 2,
    }

    public enum HyEventMessage
    {
        Event_BeginLoading = 0,
        Event_UpdateLoadingTex,
        Event_UpdateLoadingProgress,
        Event_EndLoading,
        Event_Reserved0,
        Event_Reserved1,
        Event_Reserved2,
        Event_Reserved3,
    };

    public class HyMsg
    {
        public enum MsgType
        {
            HY_MSG_SELF = 0,
            HY_MSG_NONE = 0,                /**< no more msg for now; ask again next frame */
            HY_MSG_APP_STATUS_CHANGE,   /**< HyDevice for the application is started */
            HY_MSG_PENDING_QUIT,        /**< application should quit now */
            HY_MSG_DEVICE_INVALIDATED,  /**< this HyDevice instance is no longer valid */
            HY_MSG_VIEW_FOCUS_CHANGED,  /**< view focus changed; this app get focus when m_id is 0, otherwise focus is lost */
            HY_MSG_INPUT_FOCUS_CHANGED, /**< input focus changed; this app get focus when m_id is 0, otherwise focus is lost */
            HY_MSG_SUBDEVICE_STATUS_CHANGED,    /**< sub device connection status changed; value 0 means disconnected; otherwise connected */
            HY_MSG_SUBDEVICE_BATTERY_CHANGED,   /**< sub device battery status changed; value is percentage */
            HY_MSG_SUBDEVICE_TRACKING_CHANGED,  /**< sub device tracking state change; value can be cast to HyTrackingFlag */
            HY_MSG_IPD_CHANGED,                 /**< hmd ipd changed */
            HY_MSG_NOTIFY_STATUS_CHANGED,       /**< notify overlay staus change */

            HY_MSG_USER = 1000,
        };

        public enum MsgIdConst
        {
            HY_ID_SELF_IN_MSG = 0
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MsgHeader
        {
            public UInt16 size;
            public UInt16 type;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgAppInfo
        {
            public MsgHeader header;
            public UInt16 id;
            public UInt16 reserved;
            public UInt16 action;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgWithReason
        {
            public MsgHeader header;
            public UInt32 reason;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgFocusChange
        {
            public MsgHeader header;
            public UInt16 id;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgSubdeviceChange
        {
            public MsgHeader header;
            public UInt16 subdevice;
            public UInt16 value;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgIpdChange
        {
            public MsgHeader header;
            public float value;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MsgOverlayChange
        {
            public MsgHeader header;
            public UInt16 id;
            public UInt16 reserved;
            public UInt16 value;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MsgData
        {
            [FieldOffset(0)]
            public MsgHeader header;
            [FieldOffset(0)]
            public MsgAppInfo appInfo;
            [FieldOffset(0)]
            public MsgWithReason reason;
            [FieldOffset(0)]
            public MsgFocusChange focusChange;
            [FieldOffset(0)]
            public MsgSubdeviceChange subdeviceChange;
            [FieldOffset(0)]
            public MsgIpdChange ipdChange;
            [FieldOffset(0)]
            public MsgOverlayChange overlayChange;
        }
    }

    public static class HyperealApi
    {
        [DllImport("HyperealPlugin", EntryPoint = "Initialize")]
        public static extern int Initialize();

        [DllImport("HyperealPlugin", EntryPoint = "Shutdown")]
        public static extern void Shutdown();

        [DllImport("HyperealPlugin", EntryPoint = "IsPresent")]
        public static extern bool IsPresent();

        [DllImport("HyperealPlugin", EntryPoint = "GetDeviceInfo")]
        public static extern int GetDeviceInfo(ref HyDeviceInfo deviceInfo);

        [DllImport("HyperealPlugin", EntryPoint = "ConfigureGraphic")]
        public static extern int ConfigureGraphic(bool srgb, float pixelDensity);

        [DllImport("HyperealPlugin", EntryPoint = "ConfigureRendering")]
        public static extern int ConfigureRendering(HyEyeType eye, ref HyFovPort fov);

        [DllImport("HyperealPlugin", EntryPoint = "GetEyeRawFovPort")]
        public static extern int GetEyeRawFovPort(HyEyeType eye, ref HyFovPort fov);

        [DllImport("HyperealPlugin", EntryPoint = "GetEyeResolution")]
        public static extern int GetEyeResolution(HyEyeType eye, ref Vector2 resolution);

        [DllImport("HyperealPlugin", EntryPoint = "GetEyeRenderPose")]
        static extern int GetEyeRenderPoseRaw(ref HyPosef headPose, float ipd, [In, Out] HyPosef[] eyePoses);

        [DllImport("HyperealPlugin", EntryPoint = "SetMirrorTexture")]
        public static extern int SetMirrorTexture(IntPtr nativeTex, int w, int h);

        [DllImport("HyperealPlugin", EntryPoint = "ReleaseMirrorTexture")]
        public static extern void ReleaseMirrorTexture();

        [DllImport("HyperealPlugin", EntryPoint = "GetProjectMatrix")]
        public static extern int GetProjectMatrix(ref Matrix4x4 projMat, ref HyFovPort fov, float znear, float zfar);

        [DllImport("HyperealPlugin", EntryPoint = "GetTrackingStates")]
        public static extern int GetTrackingStates([In, Out] HyTrackingState[] outPoses, [In, Out] HyDevice[] devices, uint count);

        [DllImport("HyperealPlugin", EntryPoint = "GetInputStates")]
        public static extern int GetInputStates(HyDevice device, ref HyInputState inputState);

        [DllImport("HyperealPlugin", EntryPoint = "SetHapticVibration")]
        public static extern int SetHapticVibration(HyDevice device, float frequency, float amplitude);

        [DllImport("HyperealPlugin", EntryPoint = "SetTrackingOrigin")]
        public static extern int SetTrackingOrigin(HyTrackingOrigin level);

        [DllImport("HyperealPlugin", EntryPoint = "GetTrackingOrigin")]
        public static extern int GetTrackingOrigin();

        [DllImport("HyperealPlugin", EntryPoint = "GetTrackingEnable")]
        public static extern int GetTrakcingEnable(ref bool trackingEnable);

        [DllImport("HyperealPlugin", EntryPoint = "GetPlayAreaVertexCount")]
        public static extern int GetPlayAreaVertexCount(ref int count);

        [DllImport("HyperealPlugin", EntryPoint = "GetPlayAreaVertex")]
        public static extern int GetPlayAreaVertex([In, Out] Vector2[] outVertices, int count);

        [DllImport("HyperealPlugin", EntryPoint = "SetDefaultChaperone")]
        public static extern int SetDefaultChaperone(bool enable);

        [DllImport("HyperealPlugin", EntryPoint = "GetRenderEventHandler")]
        public static extern IntPtr GetRenderEventHandler();

        [DllImport("HyperealPlugin", EntryPoint = "SendMsg")]
        static extern int SendEventMsgProxy(HyEventMessage eventId, IntPtr param);

        [DllImport("HyperealPlugin", EntryPoint = "RetrieveMsg")]
        public static extern int RetrieveMsg(ref HyMsg.MsgData msg);

        [DllImport("HyperealPlugin", EntryPoint = "SetInEditor")]
        public static extern int SetInEditor(bool bInEditor);

        public static bool isConnected(this HyTrackingState p)
        {
            return (p.status & (int)HyTrackingStatus.Status_Connected) != 0;
        }

        public static bool isPoseTracked(this HyTrackingState p)
        {
            return (p.status & (int)HyTrackingStatus.Status_PoseTracked) != 0;
        }

        public static HyPosef EyeToHmdPose(this HyPosef eye, HyPosef head)
        {
            eye.orientation = Quaternion.identity;
            eye.position = Quaternion.Inverse(head.orientation) * (eye.position - head.position);
            return eye;
        }

        public static void GetEyeRenderPose(HyPosef head, float ipd, ref HyPosef[] eyePose)
        {
            GetEyeRenderPoseRaw(ref head, ipd, eyePose);
            for (int i = 0; i < 2; i++)
            {
                eyePose[i] = eyePose[i].EyeToHmdPose(head);
            }
        }
        public static int SendEventMessage(HyEventMessage type)
        {
            return SendEventMsgProxy(type, IntPtr.Zero);
        }
        public static int SendEventMessage(HyEventMessage type, IntPtr ptr)
        {
            return SendEventMsgProxy(type, ptr);
        }
        public static int SendEventMessage<T>(HyEventMessage type, T param)
        {
            int lenght = Marshal.SizeOf(param);
            IntPtr pA = Marshal.AllocHGlobal(lenght);
            Marshal.StructureToPtr(param, pA, true);
            int ret = SendEventMsgProxy(type, pA);
            Marshal.FreeHGlobal(pA);
            return ret;
        }
        public static bool isHDR(this Camera cam)
        {
#if UNITY_5_6_OR_NEWER
            return cam.allowHDR;
#else
            return cam.hdr;
#endif
        }
    }
}
