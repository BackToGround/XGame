using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Hypereal
{
    public class HyUI : MonoBehaviour
    {
        public Vector2 QuadSize = new Vector2(4, 4);
        public float CanvasScalar = 2.0f;
        public int DPI = 200;

        Camera uiCamera;
        Canvas uiCanvas;

        CanvasScaler canvasScaler;

        int uiLayer = 0;

        MeshRenderer quadRender;

        RenderTexture uiTexture;
        Material uiMaterial;

        static Mesh QuadMesh = null;

        static void InitQuadMesh()
        {
            if(QuadMesh == null)
            {
                QuadMesh = new Mesh();
                QuadMesh.name = "HyQuadMesh";
                float hw = 0.5f;
                float hh = 0.5f;

                QuadMesh.vertices = new Vector3[]
                {
                    new Vector3( hw, hh, 0),
                    new Vector3( hw, -hh, 0),
                    new Vector3(-hw, hh, 0),
                    new Vector3(-hw, -hh, 0),
                };

                QuadMesh.uv = new Vector2[]
                {
                    new Vector2(1, 1),
                    new Vector2(1, 0),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                };

                QuadMesh.triangles = new int[]
                {
                    0, 1, 2,
                    2, 1, 3,
                };
            }
        }

        // Use this for initialization
        public void Initialize()
        {
            //create shader mesh
            InitQuadMesh();

            uiLayer = LayerMask.NameToLayer("UI");
            gameObject.layer = uiLayer;

            //create texture and material
            CreateRenderTexture();
            if (uiMaterial == null)
            {
                uiMaterial = new Material(Shader.Find("HyperealVR/AlphaBlended"));
                uiMaterial.mainTexture = uiTexture;
            }

            //initialize camera
            uiCamera = GetComponent<Camera>();
            if (uiCamera == null)
                uiCamera = gameObject.AddComponent<Camera>();

            uiCamera.orthographic = true;
            uiCamera.nearClipPlane = -0.01f;
            uiCamera.farClipPlane = 0.01f;
            uiCamera.cullingMask = (1 << uiLayer);
            uiCamera.clearFlags = CameraClearFlags.SolidColor;
            uiCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            uiCamera.targetTexture = uiTexture;

            //initialize canvas
            if (uiCanvas == null || canvasScaler == null)
            {
                Transform t = transform.Find("Canvas");
                if(t == null)
                {
                    t = new GameObject("Canvas").transform;
                    t = t.gameObject.AddComponent<Canvas>().transform;
                    t.gameObject.AddComponent<CanvasScaler>();
                    t.gameObject.AddComponent<GraphicRaycaster>();
                    t.SetParent(transform, false);
                    t.gameObject.layer = uiLayer;
                };
                uiCanvas = t.GetComponent<Canvas>();
                canvasScaler = t.GetComponent<CanvasScaler>();
            }

            uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvas.worldCamera = uiCamera;
            uiCanvas.planeDistance = 0.0f;
            
            //initialize quad mesh game object
            if (quadRender == null)
            {
                Transform t = transform.Find("QuadMesh");
                if(t == null)
                {
                    t = new GameObject("QuadMesh").transform;
                    t.gameObject.AddComponent<MeshFilter>();
                    t.gameObject.AddComponent<MeshRenderer>();
                    t.SetParent(transform, false);
                    t.gameObject.layer = 0;
                }
                MeshFilter filter = t.GetComponent<MeshFilter>();
                filter.mesh = QuadMesh;

                quadRender = t.GetComponent<MeshRenderer>();
                quadRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                quadRender.receiveShadows = false;
                quadRender.material = uiMaterial;
            }

            UpdateParam();
        }
        void Awake()
        {
            Initialize();
        }
        void OnEnable()
        {
            UpdateParam();
        }
        void OnDisable()
        {
            UpdateParam();
        }
        void Update()
        {
            uiCamera.enabled = quadRender.isVisible;
        }

        float ModClamp(float x, float b)
        {
            int p = Mathf.CeilToInt(x / b);
            return p * b;
        }
        void GetTextureSize(ref int w, ref int h)
        {
            int maxW = 4096;
            int maxH = 4096;

            float x = ModClamp((int)(QuadSize.x * DPI), 100.0f);
            float y = ModClamp((int)(QuadSize.y * DPI), 100.0f);

            if (x > maxW)
                x = maxW;

            if (y > maxH)
                y = maxH;

            DPI = Mathf.Min(DPI, (int)(x / QuadSize.x));
            DPI = Mathf.Min(DPI, (int)(y / QuadSize.y));

            w = (int)(QuadSize.x * DPI);
            h = (int)(QuadSize.y * DPI);
        }

        void CreateRenderTexture()
        {
            int w = 0;
            int h = 0;
            GetTextureSize(ref w, ref h);
            if (uiTexture == null ||
                uiTexture.width != w || uiTexture.height != h)
            {
                if (uiTexture != null)
                {
                    if(uiMaterial != null)
                        uiMaterial.mainTexture = null;
                    if(uiCamera != null)
                        uiCamera.targetTexture = null;

                    DestroyImmediate(uiTexture);
                }

                uiTexture = new RenderTexture(w, h, 0);
                uiTexture.name = "HyUITexture";
                uiTexture.Create();
            }
        }

        public void UpdateParam()
        {
            CreateRenderTexture();
            uiMaterial.mainTexture = uiTexture;

            quadRender.transform.localScale = new Vector3(QuadSize.x, QuadSize.y, 1.0f);

            Vector3 scale = quadRender.transform.lossyScale;

            uiCamera.targetTexture = uiTexture;
            uiCamera.aspect = scale.x / scale.y;
            uiCamera.pixelRect = new Rect(0.0f, 0.0f, QuadSize.x * DPI, QuadSize.y * DPI);

            uiCamera.orthographicSize = Mathf.Min(scale.x, scale.y) * 0.5f;

            //canvasScaler.scaleFactor = DPI / 150.0f;
            canvasScaler.scaleFactor = CanvasScalar;

            uiMaterial.mainTextureScale = new Vector2(uiCamera.rect.width, uiCamera.rect.height);

            uiCanvas.transform.localScale = new Vector3(0.01f, 0.01f);
        }
    }
}

