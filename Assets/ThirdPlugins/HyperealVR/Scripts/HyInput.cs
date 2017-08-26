using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Hypereal
{
    #region Predefine Type - Key Type, Event Type, Event Define
    //currently we only support touch event on index trigger/touch pad
    public enum HyInputKey
    {
        None = 0,        
        Menu = 0x01,
        IndexTrigger = 0x02,
        SideTrigger = 0x04,
        Touchpad = 0x08,
        Touchpad_Up = 0x10,
        Touchpad_Down = 0x20,
        Touchpad_Left = 0x40,
        Touchpad_Right = 0x80,
    }

    public enum HyInputKeyEventType
    {
        Press = 0,
        Press_Down,
        Press_Up,
        Press_Click,
        Press_DClick,
        Touch,
        Touch_Down,
        Touch_Up,
        Touch_Tap,
        Touch_DTap
    }

    public class HyInputKeyEvent : UnityEvent<HyInput, HyInputKey, HyInputKeyEventType>
    {

    }

    #endregion

    public class HyInput
    {
        #region Raw Function For Input State
        public bool GetPressDown(HyInputKey key) { int keyMask = (int)key; return ((preState.keyStatus & keyMask) == 0) && ((curState.keyStatus & keyMask) != 0); }
        public bool GetPressUp(HyInputKey key) { int keyMask = (int)key; return ((preState.keyStatus & keyMask) != 0) && ((curState.keyStatus & keyMask) == 0); }
        public bool GetPress(HyInputKey key) { int keyMask = (int)key; return (curState.keyStatus & keyMask) != 0; }
        public bool GetTouchDown(HyInputKey key) { int touchMask = (int)key; return ((preState.touchStatus & touchMask) == 0) && ((curState.touchStatus & touchMask) != 0); }
        public bool GetTouchUp(HyInputKey key) { int touchMask = (int)key; return ((preState.touchStatus & touchMask) != 0) && ((curState.touchStatus & touchMask) == 0); }
        public bool GetTouch(HyInputKey key) { int touchMask = (int)key; return (curState.touchStatus & touchMask) != 0; }
        public float GetTriggerAxis(HyInputKey key)
        {
            switch (key)
            {
                case HyInputKey.IndexTrigger:
                    return curState.indexTriggerValue;
                case HyInputKey.SideTrigger:
                    return curState.sideTriggerValue;
            }
            return 0.0f;
        }
        public float GetTouchProximity(HyInputKey key)
        {
            switch (key)
            {
                case HyInputKey.IndexTrigger:
                    return curState.indexTriggerProximity;
                case HyInputKey.Touchpad:
                    return curState.touchpadProximity;
            }
            return 0.0f;
        }
        public Vector2 GetTouchpadAxis() { return curState.touchpadValue; }
        #endregion

        #region Properties Export for Inspector
        public HyDevice deviceID;
        public float triggerTolerance = 0.7f;
        public float clickTolerance = 0.5f;
        public float dClickTolerance = 0.2f;
        #endregion

        #region Private Members
        class KeyState
        {
            public HyInputKey key;
            public float lastPressDownTime;
            public float lastPressUpTime;
            public int clickCount;
            public float lastTouchDownTime;
            public float lastTouchUpTime;
            public int tapCount;

            public KeyState()
            {
                key = HyInputKey.None;
                lastPressDownTime = 0.0f;
                lastPressUpTime = 0.0f;
                lastTouchDownTime = 0.0f;
                lastTouchUpTime = 0.0f;
                clickCount = 0;
                tapCount = 0;
            }
        }

        static class Constants
        {
            public static Vector2 UpDir = new Vector2(0.0f, 1.0f);
            public static Vector2 RightDir = new Vector2(1.0f, 0.0f);
            public static float Cos45Deg = 0.7071f;
        }
  
        KeyState[] keyStates = null;

        HyInputState preState;
        HyInputState curState;

        Dictionary<int, HyInputKeyEvent> inputKeyEvents = null;
        #endregion

        public HyInput(int i)
        {
            deviceID = HyDevice.Device_Controller0 + i;

            // Button state
            Array keys = Enum.GetValues(typeof(HyInputKey));
            Array keyTypes = Enum.GetValues(typeof(HyInputKeyEventType));
            inputKeyEvents = new Dictionary<int, HyInputKeyEvent>();
            for (int k = 1; k < keys.Length; ++k)
            {
                for (int t = 0; t < keyTypes.Length; ++t)
                    inputKeyEvents.Add(GetInputEventKey((HyInputKey)keys.GetValue(k), (HyInputKeyEventType)keyTypes.GetValue(t)), new HyInputKeyEvent());
            }

            keyStates = new KeyState[keys.Length - 1];
            for (int k = 0; k < keyStates.Length; ++k)
            {
                keyStates[k] = new KeyState();
                keyStates[k].key = (HyInputKey)keys.GetValue(k + 1);
            }
        }

        public void AddEventListener(UnityAction<HyInput, HyInputKey, HyInputKeyEventType> listener, HyInputKey key, HyInputKeyEventType type)
        {
            HyInputKeyEvent e = GetInputEventHandler(key, type);
            if (e != null)
                e.AddListener(listener);
#if UNITY_EDITOR
            if (key != HyInputKey.IndexTrigger && key < HyInputKey.Touchpad && type >= HyInputKeyEventType.Touch)
                Debug.LogError("[HyperealVR][HyInput] - " + key.ToString() + ":" + type.ToString() + 
                    " Currently, we only support Touch Event on indextrigger and touchpad.");
#endif
        }

        public void RemoveEventListener(UnityAction<HyInput, HyInputKey, HyInputKeyEventType> listener, HyInputKey key, HyInputKeyEventType type)
        {
            HyInputKeyEvent e = GetInputEventHandler(key, type);
            if (e != null)
                e.RemoveListener(listener);
        }

        public void Update(HyInputState state)
        {
            if (state.indexTriggerValue >= triggerTolerance)
                state.keyStatus |= (int)HyInputKey.IndexTrigger;

            if (state.sideTriggerValue >= triggerTolerance)
                state.keyStatus |= (int)HyInputKey.SideTrigger;

            preState = curState;
            curState = state;

            foreach (var k in keyStates)
            {
                UpdateKey(k);
                UpdateTouch(k);
            }
        }

        void UpdateKey(KeyState state)
        {
            if (GetPressDown(state.key))
            {
                //reset the click count while the interval between two click is beyond the tolerance.
                if (Time.time - state.lastPressUpTime > dClickTolerance)
                {
                    state.clickCount = 0;
                }
                state.lastPressDownTime = Time.time;
                EmitEvent(state, HyInputKeyEventType.Press_Down);
            }
            else if (GetPress(state.key))
            {
                EmitEvent(state, HyInputKeyEventType.Press);
            }

            if (GetPressUp(state.key))
            {
                EmitEvent(state, HyInputKeyEventType.Press_Up);
                //if (Time.time - state.lastPressDownTime <= clickTolerance)
                EmitEvent(state, HyInputKeyEventType.Press_Click);

                state.lastPressUpTime = Time.time;
                state.clickCount++;
                if (state.clickCount == 2)
                {
                    state.clickCount = 0;
                    EmitEvent(state, HyInputKeyEventType.Press_DClick);
                }
            }
        }

        void UpdateTouch(KeyState state)
        {
            if (GetTouchDown(state.key))
            {
                //reset the click count while the interval between two click is beyond the tolerance.
                if (Time.time - state.lastTouchUpTime > dClickTolerance)
                {
                    state.tapCount = 0;
                }
                state.lastTouchDownTime = Time.time;
                EmitEvent(state, HyInputKeyEventType.Touch_Down);
            }
            else if (GetTouch(state.key))
            {
                EmitEvent(state, HyInputKeyEventType.Touch);
            }

            if (GetTouchUp(state.key))
            {
                EmitEvent(state, HyInputKeyEventType.Touch_Up);
                //if (Time.time - state.lastTouchDownTime <= clickTolerance)
                EmitEvent(state, HyInputKeyEventType.Touch_Tap);

                state.lastTouchUpTime = Time.time;
                state.tapCount++;
                if (state.tapCount == 2)
                {
                    state.tapCount = 0;
                    EmitEvent(state, HyInputKeyEventType.Touch_DTap);
                }
            }
        }

        int GetInputEventKey(HyInputKey key, HyInputKeyEventType type)
        {
            return ((int)key << 4) + (int)type;
        }

        HyInputKeyEvent GetInputEventHandler(HyInputKey k, HyInputKeyEventType type)
        {
            int key = GetInputEventKey(k, type);
            if (inputKeyEvents.ContainsKey(key))
                return inputKeyEvents[key];
            return null;
        }

        void EmitEvent(KeyState state, HyInputKeyEventType type)
        {
            HyInputKeyEvent handler = GetInputEventHandler(state.key, type);
            if (handler != null)
                handler.Invoke(this, state.key, type);
        }

    }

    public class HyInputManager
    {
        HyInputState[] inputStates;
        HyInput[] inputHandlers;

        int inputDeviceCount = 2;
        int frameCount = 0;

        static HyInputManager _instance = null;
        internal static HyInputManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new HyInputManager();
                }
                return _instance;
            }
        }
        public HyInputManager()
        {
            inputStates = new HyInputState[inputDeviceCount];
            inputHandlers = new HyInput[inputDeviceCount];

            for (int i = 0; i < inputDeviceCount; i++)
            {
                inputStates[i] = new HyInputState();
                inputHandlers[i] = new HyInput(i);
            }
        }

        public void Update()
        {
            if (Time.frameCount != frameCount)
            {
                UpdateInput();
                frameCount = Time.frameCount;
            }
        }

        void UpdateInput()
        {
            if (!HyperealVR.IsStereoEnabled)
                return;

            HyperealApi.GetInputStates(HyDevice.Device_Controller0, ref inputStates[0]);
            HyperealApi.GetInputStates(HyDevice.Device_Controller1, ref inputStates[1]);

            for (int i = 0; i < inputDeviceCount; i++)
                inputHandlers[i].Update(inputStates[i]);
        }

        public HyInput GetInputDevice(HyDevice deviceId)
        {
            if (deviceId == HyDevice.Device_Controller0)
                return inputHandlers[0];
            if (deviceId == HyDevice.Device_Controller1)
                return inputHandlers[1];
            return null;
        }

    }
}
