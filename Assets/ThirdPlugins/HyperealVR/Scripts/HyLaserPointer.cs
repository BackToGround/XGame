using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Hypereal
{
    public class HyLaserPointer : HyInputPointer
    {
        public enum HyToggleMode
        {
            Pressing = 0,
            Click,
        }

        public HyToggleMode ToggleMode = HyToggleMode.Pressing;

        [Tooltip("The laser pointer will be enabled when this key is pressing. The laser will always be enabled if no toggle key is assigned.")]
        public HyInputKey ToggleKey = HyInputKey.Touchpad;

        [Tooltip("The key used for emit pointer down/up/click events.")]
        public HyInputKey TriggerKey = HyInputKey.IndexTrigger;

        [Tooltip("The layer ignored by the laser.")]
        public LayerMask layersIgnored;

        [Range(0.0005f, 0.005f)]
        public float Thickness = 0.002f;
        public Color LaserMainColor = Color.red;
        public Color LaserHitColor = Color.green;

        [Range(0.001f, 0.1f)]
        public float HitSignScale = 0.02f;
        public Color HitSignColor = Color.red;

        [Tooltip("The position offset for the laser to the controller.")]
        public Vector3 RayPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);

        [Tooltip("The rotation offset for the laser to the controller.")]
        public Vector3 RayRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);

        GameObject laserObject;
        GameObject hitObject;

        LayerMask ignoreLayer;
        LineRenderer lineRender;
        MeshRenderer hitSignRender;

        Vector3 lastHitPosition = Vector3.zero;

        public override bool PointerDown() { return InputController != null ? InputController.GetPressDown(TriggerKey) : false; }
        public override bool PointerUp() { return InputController != null ? InputController.GetPressUp(TriggerKey) : false; }
        public override bool PointerPressed() { return InputController != null ? InputController.GetPress(TriggerKey) : false; }

        public override Ray GetPointerRay()
        {
            Quaternion r = transform.rotation * Quaternion.Euler(RayRotationOffset);

            Vector3 offset = new Vector3(RayPositionOffset.x * transform.lossyScale.x, RayPositionOffset.y * transform.lossyScale.y, RayPositionOffset.z * transform.lossyScale.z);
            pointerRay.origin = transform.position + transform.rotation * offset;
            pointerRay.direction = r * Vector3.forward;
            return pointerRay;
        }

        void Initialize()
        {
            if (laserObject != null)
                return;

            ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");

            laserObject = new GameObject("Laser");
            laserObject.transform.parent = transform;
            laserObject.SetActive(false);
            laserObject.layer = ignoreLayer;

            lineRender = laserObject.AddComponent<LineRenderer>();
            lineRender.material = new Material(Shader.Find("HyperealVR/Laser"));
            lineRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRender.receiveShadows = false;

            hitObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitObject.name = "HitSign";
            DestroyObject(hitObject.GetComponent<SphereCollider>());

            hitObject.transform.parent = transform;
            hitObject.transform.localScale = new Vector3(HitSignScale, HitSignScale, HitSignScale);
            hitObject.SetActive(false);
            hitObject.layer = ignoreLayer;

            hitSignRender = hitObject.GetComponent<MeshRenderer>();
            hitSignRender.material = new Material(Shader.Find("HyperealVR/Laser"));
        }

        new void OnEnable()
        {
            base.OnEnable();

            Initialize();

            if (ToggleKey == HyInputKey.None)
            {
                if(ToggleMode == HyToggleMode.Pressing)
                    EnableLaser(true);
                return;
            }
            if (InputController != null)
            {
                InputController.AddEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Down);
                InputController.AddEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Up);
                InputController.AddEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Click);
            }
        }

        new void OnDisable()
        {           
            if (ToggleKey == HyInputKey.None)
            {
                EnableLaser(false);
                return;
            }
			if (InputController != null) 
			{
                InputController.RemoveEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Down);
                InputController.RemoveEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Up);
                InputController.RemoveEventListener(OnToggleClick, ToggleKey, HyInputKeyEventType.Press_Click);
			}
            base.OnDisable();
        }

        void LateUpdate()
        {
            if (!laserObject.activeSelf)
                return;
            GetPointerRay();
            ApplyLaser();
        }

        void OnToggleClick(object sender, HyInputKey key, HyInputKeyEventType type)
        {
            if(ToggleMode == HyToggleMode.Pressing)
            {
                if (type == HyInputKeyEventType.Press_Down)
                    EnableLaser(true);
                if (type == HyInputKeyEventType.Press_Up)
                    EnableLaser(false);
            }
            if (ToggleMode == HyToggleMode.Click && type == HyInputKeyEventType.Press_Click)
                EnableLaser(!EnableUIPointer);
        }

        void EnableLaser(bool enable)
        {
            EnableUIPointer = enable;
            if(laserObject != null)
                laserObject.SetActive(EnableUIPointer);
            if(hitObject != null)
                hitObject.SetActive(EnableUIPointer);
            if(EnableUIPointer)
                ApplyLaser();
        }

        void ApplyLaser()
        {
            RaycastHit pointerCollidedWith;

            float rayLength = GetPointerRayLength();
            float distance = rayLength;
            var rayHit = Physics.Raycast(pointerRay, out pointerCollidedWith, rayLength, ~(layersIgnored | ignoreLayer));
            if (rayHit)
            {
                if (pointerCollidedWith.distance < rayLength)
                    distance = pointerCollidedWith.distance;
            }
            float d = float.MaxValue;
            bool overUI = IsOverGameObject(ref d);
            if (overUI)
            {
                if (d > 0.0001f && d < distance)
                    distance = d;
            }
            rayHit |= overUI;

            Vector3 hitPos = pointerRay.origin + pointerRay.direction * distance;
            if (lastHitPosition == Vector3.zero)
                lastHitPosition = hitPos;

            hitSignRender.sharedMaterial.color = HitSignColor;
            hitObject.transform.position = lastHitPosition;
  
            hitObject.SetActive(rayHit);
            
            lineRender.sharedMaterial.color = rayHit ? LaserHitColor : LaserMainColor;

            float scaleThickness = Thickness * (transform.lossyScale.x + transform.lossyScale.y) * 0.5f;

#if UNITY_5_5_OR_NEWER
            lineRender.startWidth = scaleThickness;
            lineRender.endWidth = scaleThickness;
#else
            lineRender.SetWidth(scaleThickness, scaleThickness);
#endif
            lineRender.SetPosition(0, pointerRay.origin);
            lineRender.SetPosition(1, hitPos);

            lastHitPosition = hitPos;
        }
    }
}
