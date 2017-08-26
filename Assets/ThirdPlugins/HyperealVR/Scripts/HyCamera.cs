using UnityEngine;
using System.Collections;

namespace Hypereal
{
#pragma warning disable 0168

    [RequireComponent(typeof(Camera))]
    public class HyCamera : MonoBehaviour
    {
        [HideInInspector]
        public HyEyeType CurrentRenderedEye { get; set; }

        internal Transform head = null;
        internal Transform origin = null;
        private const string eyeSuffix = "(eye)";
        private const string headSuffix = "(head)";
        private const string originSuffix = "(origin)";
        public bool StereoCullingMask = false;
        public LayerMask CullingMaskLeft = -1;
        public LayerMask CullingMaskRight = -1;
        internal Camera GetHeadCamera()
        {
            if (head != null)
                return head.GetComponent<Camera>();
            return null;
        }
        private void Awake()
        {
            EnsureAtLast();
        }
        private void OnEnable()
        {
            if (!isExpanded)
            {
                this.enabled = false;
                return;
            }

            EnsureHeadOrigin();

            var hyInst = HyperealVR.Instance;
            if(!HyperealVR.IsStereoEnabled)
            {
                this.enabled = false;
                if (head != null)
                {
                    head.GetComponent<HyHead>().enabled = false;
                    head.GetComponent<HyTrackObj>().enabled = false;
                }
                return;
            }
            
            HyRender.AddCamera(this);

            Camera camera = gameObject.GetComponent<Camera>();
            camera.eventMask = 0;           // disable mouse events
            camera.orthographic = false;    // force perspective
            camera.enabled = false;         // manually rendered by HyRender

            if (camera.actualRenderingPath != RenderingPath.Forward && QualitySettings.antiAliasing > 1)
            {
                Debug.LogWarning("MSAA only supported in Forward rendering path. (disabling MSAA)");
                QualitySettings.antiAliasing = 0;
            }

            // Ensure game view camera hdr setting matches
            var headCam = head.GetComponent<Camera>();
            if (headCam != null)
            {
#if (UNITY_5_6_OR_NEWER)
                headCam.allowHDR = camera.isHDR();
#else
                headCam.hdr = camera.isHDR();
#endif
                headCam.renderingPath = camera.renderingPath;
            }            
        }

        private void OnDisable()
        {
            HyRender.RemoveCamera(this);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {            
            if (HyRender.GetLastCamera() == this && HyperealVR.Instance.IsVisible)
            {
                if (CurrentRenderedEye == HyEyeType.EyeType_Left)
                    HyperealVR.IssueRenderEvent((int)HyRenderEvent.RE_SubmitLeft);
                if (CurrentRenderedEye == HyEyeType.EyeType_Right)
                    HyperealVR.IssueRenderEvent((int)HyRenderEvent.RE_SubmitRight);
            }
            Graphics.Blit(src, dst);
        }

        private void EnsureAtLast()
        {
            Component[] comps = this.GetComponents<MonoBehaviour>();
            for(int i = 0; i < comps.Length; i++)
            {
                var c = comps[i] as HyCamera;
                if (c != null && c != this)
                    DestroyImmediate(c);
            }
            if(comps[comps.Length - 1] != this)
            {
                var go = this.gameObject;
                DestroyImmediate(this);
                go.AddComponent<HyCamera>();
            }
        }

#region Editor
        public bool isExpanded
        {
            get
            {
                HyHead hyHead = null;
                if (head == null)
                {
                    Transform p = this.transform.parent;
                    if(p != null)
                        hyHead = p.GetComponent<HyHead>();
                }
                else
                    hyHead = head.GetComponent<HyHead>();
                return (hyHead != null);
            }
        }

        void EnsureHeadOrigin()
        {
            if (head == null)
            {
                Transform p = this.transform.parent;
                if (p.GetComponent<HyHead>() != null)
                    head = p; ;
            }
            if(origin == null)
            {
                Transform p = transform.parent;
                if (p != null) p = p.parent;
                if (p != null && p.name.EndsWith(originSuffix))
                    origin = p;
            }
        }

#if UNITY_EDITOR
        public GameObject Expand()
        {
            //create parent
            Transform parent = transform.parent;
            origin = new GameObject(transform.gameObject.name + originSuffix).transform;
            origin.localPosition = transform.localPosition;
            origin.localRotation = transform.localRotation;
            origin.localScale = transform.localScale;
            origin.SetParent(parent, false);

            // head
            head = new GameObject(transform.gameObject.name + headSuffix).transform;
            head.SetParent(origin.transform, false);
            head.gameObject.AddComponent<HyHead>();
            head.gameObject.AddComponent<HyTrackObj>();
            head.GetComponent<HyTrackObj>().device = HyDevice.Device_HMD0;

            //head camera just used for mirror output and raycast for HyInputPointer
            var camera = head.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.cullingMask = 0;
            camera.eventMask = 0;
            camera.orthographic = false;
            camera.orthographicSize = 1;
            camera.nearClipPlane = 0;
            camera.farClipPlane = 1;
            camera.useOcclusionCulling = false;
#if UNITY_5_6_OR_NEWER
            camera.allowHDR = GetComponent<Camera>().isHDR();
#else
            camera.hdr = GetComponent<Camera>().isHDR();
#endif
            head.tag = tag;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            transform.SetParent(head.transform, false);

            while (transform.childCount > 0)
                transform.GetChild(0).SetParent(head.transform, false);

            var guiLayer = GetComponent<GUILayer>();
            if (guiLayer != null)
            {
                DestroyImmediate(guiLayer);
                head.gameObject.AddComponent<GUILayer>();
            }

            var audioListener = GetComponent<AudioListener>();
            if (audioListener != null)
            {
                DestroyImmediate(audioListener);
                head.gameObject.AddComponent<AudioListener>();
            }

            if (!name.EndsWith(eyeSuffix))
                name += eyeSuffix;

            return origin.gameObject;
        }

        public GameObject Collapse()
        {
            if (!isExpanded)
                return gameObject;

            EnsureHeadOrigin();

            //move children and components from head back to camera.
            transform.SetParent(null, false);

            while (head.childCount > 0)
                head.GetChild(0).SetParent(transform, false);

            var guiLayer = head.GetComponent<GUILayer>();
            if (guiLayer != null)
            {
                DestroyImmediate(guiLayer);
                gameObject.AddComponent<GUILayer>();
            }

            var audioListener = head.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                DestroyImmediate(audioListener);
                gameObject.AddComponent<AudioListener>();
            }
            
            //move children in origin back to parent
            Transform parent = origin.parent;
            while (origin.childCount > 0)
                origin.GetChild(0).SetParent(parent, true);

            //move camera to origin place
            transform.localPosition = origin.localPosition;
            transform.localRotation = origin.localRotation;
            transform.localScale = origin.localScale;
            transform.SetParent(parent, false);

            if (name.EndsWith(eyeSuffix))
                name = name.Substring(0, name.Length - eyeSuffix.Length);

            DestroyImmediate(head.gameObject);
            DestroyImmediate(origin.gameObject);

            head = null;
            origin = null;

            return gameObject;
        }
#endif
#endregion
        }
}

