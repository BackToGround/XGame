using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Hypereal
{
    public class HyInputPointer : MonoBehaviour
    {
        public virtual bool PointerDown() { return false; }
        public virtual bool PointerUp() { return false; }
        public virtual bool PointerPressed() { return false; }

        [Tooltip("The attached controller's device id.")]
        public HyDevice ControllerID = HyDevice.Device_Controller0;

        public bool EnableUIPointer { get; set; }
        public HyInput InputController { get; private set; }

        //Emit the click event when pointer down.
        public bool EmitClickWhenDown { get; set; }

        [Tooltip("The maximum pointer ray length.")]
        public float PointerLength = 50.0f;

        [HideInInspector]
        public PointerEventData pointerEventData = null;
        [HideInInspector]
        public GameObject currentExecuteObject = null;

        private bool uiPointerEnabled = false;
        static int PointerID = 0;

        protected Ray pointerRay;

        public bool IsOverGameObject(ref float distance)
        {
            if (pointerEventData != null && pointerEventData.pointerCurrentRaycast.gameObject != null)
            {
                distance = pointerEventData.pointerCurrentRaycast.distance;
                return true;
            }
            return false;
        }
        public bool IsSenderOf(GameObject receiver)
        {
            if (receiver == null || currentExecuteObject == null)
                return false;
            return (receiver == currentExecuteObject || receiver.transform.IsChildOf(currentExecuteObject.transform));
        }

        protected void Awake()
        {
            PointerID++;
            EmitClickWhenDown = false;
            //ConfigureUIPointer(true);
        }

        protected void OnEnable()
        {
            InputController = HyperealVR.GetInputDevice(ControllerID);
            ConfigureUIPointer(true);
        }

        protected void OnDisable()
        {
            ConfigureUIPointer(false);
        }

        public virtual void HandlePointerExitAndEnter(PointerEventData pointerEventData, GameObject hitControl)
        {
            if(pointerEventData.pointerEnter != hitControl)
            {
                GameObject enterObj = null;
                if (pointerEventData.pointerEnter != null)
                    ExecuteEvents.ExecuteHierarchy(pointerEventData.pointerEnter, pointerEventData, ExecuteEvents.pointerExitHandler);
                if(hitControl != null)
                     enterObj = ExecuteEvents.ExecuteHierarchy(hitControl, pointerEventData, ExecuteEvents.pointerEnterHandler);
                pointerEventData.pointerEnter = (enterObj != null ? enterObj : hitControl);
            }
        }

        public virtual Ray GetPointerRay()
        {
            pointerRay.origin = transform.position;
            pointerRay.direction = transform.forward;
            return pointerRay;
        }

        public virtual float GetPointerRayLength()
        {
            return Mathf.Clamp(PointerLength * transform.lossyScale.z, 10.0f, 10000.0f);
        }

        void ConfigureUIPointer(bool bEnable)
        {
            if (uiPointerEnabled == bEnable)
                return;

            uiPointerEnabled = bEnable;

            if (bEnable)
            {
                if (HyInputModule.Instance == null)
                {
                    EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();

                    if (!eventSystem)
                        eventSystem = HyRender.Instance.gameObject.AddComponent<EventSystem>();

                    //disable existing standalone input module
                    StandaloneInputModule standaloneInputModule = eventSystem.gameObject.GetComponent<StandaloneInputModule>();
                    if (standaloneInputModule && standaloneInputModule.enabled)
                        standaloneInputModule.enabled = false;

                    //if it doesn't already exist, add the custom event system
                    HyInputModule inputSystem = eventSystem.GetComponent<HyInputModule>();
                    if (inputSystem == null)
                    {
                        inputSystem = eventSystem.gameObject.AddComponent<HyInputModule>();
                        inputSystem.EventSystem = eventSystem;
                        inputSystem.StandInputModule = standaloneInputModule;
                    }
                }

                if (HyInputModule.Instance == null)
                    return;

                HyInputModule.Instance.enabled = true;
                if (HyInputModule.Instance.StandInputModule)
                    HyInputModule.Instance.StandInputModule.enabled = false;

                pointerEventData = new PointerEventData(HyInputModule.Instance.EventSystem);
                pointerEventData.pointerId = PointerID << 16;
                if (!HyInputModule.Instance.pointers.Contains(this))
                    HyInputModule.Instance.pointers.Add(this);
            }
            else
            {
                if (HyInputModule.Instance == null)
                    return;

                if (HyInputModule.Instance.pointers.Contains(this))
                    HyInputModule.Instance.pointers.Remove(this);
                if(HyInputModule.Instance.pointers.Count == 0)
                {
                    HyInputModule.Instance.enabled = false;
                    if (HyInputModule.Instance.StandInputModule)
                        HyInputModule.Instance.StandInputModule.enabled = true;
                }
            }
        }
    }
}
