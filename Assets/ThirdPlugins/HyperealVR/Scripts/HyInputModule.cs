using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace Hypereal
{
    public class HyInputModule : StandaloneInputModule
    {
        [HideInInspector]
        public List<HyInputPointer> pointers = new List<HyInputPointer>();
        [NonSerialized]
        List<RaycastResult> m_CachedResults = new List<RaycastResult>();
        [NonSerialized]
        static readonly List<Canvas> s_SortedCanvas = new List<Canvas>();

        static public HyInputModule Instance { get; set; }

        public EventSystem EventSystem { get; set; }
        public StandaloneInputModule StandInputModule { get; set; }

        protected override void Awake()
        {            
            base.Awake();            
            Instance = this; 
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        public override void Process()
        {
            foreach (HyInputPointer pointer in pointers)
            {
                ProcessPointer(pointer);
            }
            base.Process();
        }

        bool CheckTransformTree(Transform target, Transform source)
        {
            if (target == null)
                return false;
            if (target == source)
                return true;
            return CheckTransformTree(target.transform.parent, source);
        }

        bool IsHovering(PointerEventData pointerEventData, GameObject obj)
        {
            if (obj == null)
                return false;
            foreach (var hoveredObject in pointerEventData.hovered)
                if(CheckTransformTree(hoveredObject.transform, obj.transform))
                    return true;
            return false;
        }

        void ProcessGraphicRaycast(HyInputPointer pointer)
        {
            //clear the hovered game objects
            pointer.pointerEventData.hovered.Clear();
            
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            if (canvases == null || canvases.Length == 0)
            {
                pointer.pointerEventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                return;
            }

            s_SortedCanvas.Clear();

            //sorting canvas by sorting order
            foreach (Canvas c in canvases)
                s_SortedCanvas.Add(c);
            s_SortedCanvas.Sort((g1, g2) => g2.sortingOrder.CompareTo(g1.sortingOrder));

            Ray ray = pointer.GetPointerRay();

            RaycastResult? nearestResult = null;
            foreach (Canvas canvas in s_SortedCanvas)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    continue;

                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    //we only take care of the hypereal camera space ui.
                    if (canvas.gameObject.GetComponentInParent<HyUI>() == null)
                        continue;
                }
                
                if(canvas.worldCamera == null)
                {
                    HyCamera hyCam = HyRender.GetLastCamera();
                    if (hyCam != null) 
                        canvas.worldCamera = hyCam.GetHeadCamera();
                }
                GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
                if (gr == null || !gr.enabled)
                    continue;

                float distance = pointer.GetPointerRayLength();
                float dot = Vector3.Dot(gr.transform.forward, ray.direction);
                if (dot <= -0.0001f || dot >= 0.0001f)
                    distance = (Vector3.Dot(gr.transform.forward, gr.transform.position - ray.origin) / dot);

                if (distance < 0)
                    continue;

                if (gr.eventCamera == null)
                    continue;

                Vector3 position = ray.GetPoint(distance);
                Vector2 pointerPosition = gr.eventCamera.WorldToScreenPoint(position);
                
                RectTransform rt = (RectTransform)canvas.transform;
                if (!RectTransformUtility.RectangleContainsScreenPoint(rt, pointerPosition, gr.eventCamera))
                    continue;

                pointer.pointerEventData.position = pointerPosition;
                pointer.pointerEventData.scrollDelta = Vector2.zero;

                m_CachedResults.Clear();
                gr.Raycast(pointer.pointerEventData, m_CachedResults);

                RaycastResult? currResult = null;

                //the default distance is from the camera to the hit point not the controller position
                for (int i = 0; i < m_CachedResults.Count; i++)
                {
                    RaycastResult rr = m_CachedResults[i];
                    rr.distance = distance;
                    
                    m_RaycastResultCache.Add(rr);

                    //add to the hovered game objects list.
                    pointer.pointerEventData.hovered.Add(rr.gameObject);

                    //only take the first hit result of the canvas.
                    if (!currResult.HasValue) currResult = rr;
                }

                if (!nearestResult.HasValue ||
                    (currResult.HasValue && nearestResult.Value.distance > currResult.Value.distance))
                    nearestResult = currResult;
            }
            if (nearestResult.HasValue)
                pointer.pointerEventData.pointerCurrentRaycast = nearestResult.Value;
            else
                pointer.pointerEventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            if (pointer.pointerEventData.pointerCurrentRaycast.isValid)
                pointer.pointerEventData.position = pointer.pointerEventData.pointerCurrentRaycast.screenPosition;
            pointer.pointerEventData.scrollDelta = pointer.InputController.GetTouchpadAxis();
        }

        void ProcessPointerDown(HyInputPointer pointer, GameObject hitControl)
        {
            pointer.pointerEventData.pressPosition = pointer.pointerEventData.position;
            pointer.pointerEventData.pointerPressRaycast = pointer.pointerEventData.pointerCurrentRaycast;
            pointer.pointerEventData.pointerDrag = null;
            pointer.pointerEventData.pointerPress = null;
            pointer.pointerEventData.rawPointerPress = null;
            SetCurrentExecute(pointer, hitControl);
            if (hitControl != null)
            {
                pointer.pointerEventData.rawPointerPress = hitControl;
                var pressed = ExecuteEvents.ExecuteHierarchy(hitControl, pointer.pointerEventData, ExecuteEvents.pointerDownHandler);

                if (pressed == null)
                    pressed = hitControl;

                SetCurrentExecute(pointer, pressed);
                if (pointer.EmitClickWhenDown)
                    ExecuteEvents.ExecuteHierarchy(pressed, pointer.pointerEventData, ExecuteEvents.pointerClickHandler);

                pointer.pointerEventData.pointerPress = pressed;

                ExecuteEvents.ExecuteHierarchy(pressed, pointer.pointerEventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.ExecuteHierarchy(pressed, pointer.pointerEventData, ExecuteEvents.beginDragHandler);
                pointer.pointerEventData.pointerDrag = ExecuteEvents.ExecuteHierarchy(pressed, pointer.pointerEventData, ExecuteEvents.dragHandler);
            }
            eventSystem.SetSelectedGameObject(pointer.pointerEventData.pointerPress);
        }

        void ProcessPointerUp(HyInputPointer pointer)
        {
            if (pointer.pointerEventData.pointerDrag != null)
            {
                SetCurrentExecute(pointer, pointer.pointerEventData.pointerDrag);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.endDragHandler);
                if (pointer.pointerEventData.rawPointerPress != null)
                {
                    SetCurrentExecute(pointer, pointer.pointerEventData.rawPointerPress);
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.rawPointerPress, pointer.pointerEventData, ExecuteEvents.dropHandler);
                }
            }
            if (pointer.pointerEventData.pointerPress != null)
            {
                SetCurrentExecute(pointer, pointer.pointerEventData.pointerPress);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerUpHandler);
                if (!pointer.EmitClickWhenDown && IsHovering(pointer.pointerEventData, pointer.pointerEventData.pointerPress))
                    ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerPress, pointer.pointerEventData, ExecuteEvents.pointerClickHandler);
            }
            pointer.pointerEventData.pointerPress = null;
            pointer.pointerEventData.rawPointerPress = null;
            pointer.pointerEventData.pointerDrag = null;
            SetCurrentExecute(pointer, null);
        }

        void ProcessPointer(HyInputPointer pointer)
        {
            if (!pointer.EnableUIPointer)
            {
                //handle the exit, up, end drag, drop etc. event when disable the pointer.
                ProcessPointerUp(pointer);
                pointer.HandlePointerExitAndEnter(pointer.pointerEventData, null);
                return;
            }

            m_RaycastResultCache.Clear();

            ProcessGraphicRaycast(pointer);
            
            var hitControl = pointer.pointerEventData.pointerCurrentRaycast.gameObject;

            SetCurrentExecute(pointer, hitControl);
            pointer.HandlePointerExitAndEnter(pointer.pointerEventData, hitControl);

            if (pointer.PointerPressed() && pointer.pointerEventData.pointerDrag != null)
            {
                SetCurrentExecute(pointer, pointer.pointerEventData.pointerDrag);
                ExecuteEvents.ExecuteHierarchy(pointer.pointerEventData.pointerDrag, pointer.pointerEventData, ExecuteEvents.dragHandler);
            }

            if (pointer.PointerDown())
                ProcessPointerDown(pointer, hitControl);

            if (pointer.PointerUp())
                ProcessPointerUp(pointer);

            if (pointer.pointerEventData.scrollDelta != Vector2.zero)
            {
                ExecuteEvents.ExecuteHierarchy(hitControl, pointer.pointerEventData, ExecuteEvents.scrollHandler);
            }
        }

        void SetCurrentExecute(HyInputPointer pointer, GameObject go)
        {
            pointer.currentExecuteObject = go;
        }
    }
}
