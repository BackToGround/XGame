using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace Hypereal
{
    public class HyTouchPointer : HyInputPointer
    {
        public override bool PointerDown()
        {
            if(TriggerKey != HyInputKey.None)
                return (currState && KeyPointerDown());
            return (!lastState && currState);
        }
        public override bool PointerUp()
        {
            if (TriggerKey != HyInputKey.None)
                return ((lastState && !currState) || KeyPointerUp());
            return (lastState && !currState);
        }
        public override bool PointerPressed()
        {
            if (TriggerKey != HyInputKey.None)
                return (currState && KeyPointerPressed());
            return currState;
        }
        
        [Tooltip("The key used for emit pointer down/up/click events.")]
        public HyInputKey TriggerKey = HyInputKey.None;

        [Tooltip("The offset distance from the ui to the position of the controller.")]
        public float TouchOffset = 0.14f;

        bool KeyPointerDown() { return InputController != null ? InputController.GetPressDown(TriggerKey) : false; }
        bool KeyPointerUp() { return InputController != null ? InputController.GetPressUp(TriggerKey) : false; }
        bool KeyPointerPressed() { return InputController != null ? InputController.GetPress(TriggerKey) : false; }

        new void OnEnable()
        {
            base.OnEnable();
            EnableUIPointer = true;
            if(TriggerKey == HyInputKey.None)
                EmitClickWhenDown = true;
            else
            {
                EmitClickWhenDown = false;
            }
        }

        new void OnDisable()
        {
            base.OnDisable();
            EnableUIPointer = false;
        }

        GameObject lastOverObject = null;
        bool lastState = false;
        bool currState = false;
        void Update()
        {
            lastState = currState;
            GameObject currObject = pointerEventData.pointerCurrentRaycast.gameObject;
            if (lastOverObject == null)
                lastOverObject = currObject;

            float currDist = pointerEventData.pointerCurrentRaycast.distance;
            if(lastOverObject != null)
            {
                if (currObject == null || lastOverObject != currObject)
                {
                    currState = false;
                    lastOverObject = currObject;
                }
                else
                {
                    currState = (currDist >= -TouchOffset && currDist <= TouchOffset);
                    lastOverObject = currObject;
                }
            }
        }
    }
}
