using UnityEngine;
using UnityEngine.UI;
using Hypereal;

public class HySettingUI : MonoBehaviour {
    // Use this for initialization
    public Transform settingUIVR;

    public InputField scaleRateMaxVR;
    public Slider scaleRateVR;
    public Slider pixelDensityVR;
    public Dropdown trackingOriginVR;
    public Dropdown mirrorModeVR;
    public Dropdown mirrorTypeVR;

    public Button recenterPosVR;
    public Button recenterRotVR;
    public Button recenterPosRotVR;

    public Transform settingUI;

    public InputField scaleRateMax;
    public Slider scaleRate;
    public Slider pixelDensity;
    public Dropdown trackingOrigin;
    public Dropdown mirrorMode;
    public Dropdown mirrorType;
    
    public Button recenterPos;
    public Button recenterRot;
    public Button recenterPosRot;

    public HyInputKey menuTriggerKey = HyInputKey.Menu;
    public KeyCode normalUITrigger = KeyCode.M;

    public float distance = 3.5f;

    public HyTrackingOrigin TrackingOrigin = HyTrackingOrigin.Tracking_Floor;
    public HyMirrorMode MirrorMode = HyMirrorMode.Adaption;
    public HyMirrorType MirrorType = HyMirrorType.Right;

    public float IPD = -1.0f;
    public float FOV = -1.0f;
    public float PixelDensity = 1.0f;
    public float RenderScale = 1.0f;

    HyRecenterType recenterType = HyRecenterType.Recenter_None;

    float maxScaleRate = 50.0f;
    InputField scaleRateCur;
    InputField pixelDensityCur;
    InputField scaleRateCurVR;
    InputField pixelDensityCurVR;

    Vector3 stickPos;
    Quaternion stickQuat;
    Vector3 stickForward;
    float stickScale;

    bool valueChanged = false;

    private void Start()
    {
        if (!HyperealVR.IsStereoEnabled)
        {
            this.enabled = false;
            return;
        }

        PixelDensity = HyperealVR.Instance.PixelDensity;
        RenderScale = HyperealVR.Instance.RenderScale;

        scaleRateCurVR = scaleRateVR.transform.parent.Find("InputField").GetComponent<InputField>();
        scaleRateCur = scaleRate.transform.parent.Find("InputField").GetComponent<InputField>();
        pixelDensityCurVR = pixelDensityVR.transform.parent.Find("InputField").GetComponent<InputField>();
        pixelDensityCur = pixelDensity.transform.parent.Find("InputField").GetComponent<InputField>();

#if UNITY_5_3_OR_NEWER
        scaleRateMaxVR.onValueChanged.AddListener(OnScaleRateMaxChanged);
        scaleRateMax.onValueChanged.AddListener(OnScaleRateMaxChanged);

        scaleRateCurVR.onValueChanged.AddListener(OnScaleRateChanged);
        scaleRateCur.onValueChanged.AddListener(OnScaleRateChanged);

        pixelDensityCurVR.onValueChanged.AddListener(OnPixelDensityChanged);
        pixelDensityCur.onValueChanged.AddListener(OnPixelDensityChanged);
#else
        scaleRateMaxVR.onValueChange.AddListener(OnScaleRateMaxChanged);
        scaleRateMax.onValueChange.AddListener(OnScaleRateMaxChanged);

        scaleRateCurVR.onValueChange.AddListener(OnScaleRateChanged);
        scaleRateCur.onValueChange.AddListener(OnScaleRateChanged);

        pixelDensityCurVR.onValueChange.AddListener(OnPixelDensityChanged);
        pixelDensityCur.onValueChange.AddListener(OnPixelDensityChanged);
#endif

        scaleRateVR.onValueChanged.AddListener(OnScaleRateChanged);
        scaleRate.onValueChanged.AddListener(OnScaleRateChanged);

        pixelDensityVR.onValueChanged.AddListener(OnPixelDensityChanged);
        pixelDensity.onValueChanged.AddListener(OnPixelDensityChanged);

        trackingOriginVR.onValueChanged.AddListener(OnTrackingOriginChanged);
        trackingOrigin.onValueChanged.AddListener(OnTrackingOriginChanged);

        mirrorModeVR.onValueChanged.AddListener(OnMirrorModeChanged);
        mirrorMode.onValueChanged.AddListener(OnMirrorModeChanged);

        mirrorTypeVR.onValueChanged.AddListener(OnMirrorTypeChanged);
        mirrorType.onValueChanged.AddListener(OnMirrorTypeChanged);

        recenterPosVR.onClick.AddListener(OnRecenterPos);
        recenterPos.onClick.AddListener(OnRecenterPos);
        recenterRotVR.onClick.AddListener(OnRecenterRot);
        recenterRot.onClick.AddListener(OnRecenterRot);
        recenterPosRotVR.onClick.AddListener(OnRecenterPosRot);
        recenterPosRot.onClick.AddListener(OnRecenterPosRot);

        SyncVRNormalUI();

        HyInput input = HyperealVR.GetInputDevice(HyDevice.Device_Controller0);
        if (input != null)
            input.AddEventListener(OnSettingTrigger, menuTriggerKey, HyInputKeyEventType.Press_Click);
    }

    private void Update()
    {
        if (!HyperealVR.IsStereoEnabled)
            return;

        if(Input.GetKeyDown(normalUITrigger))
        {
            settingUI.gameObject.SetActive(!settingUI.gameObject.activeSelf);
        }

        if (!valueChanged)
            return;

        valueChanged = false;

        HyperealVR.Instance.TrackingOrigin = TrackingOrigin;
        HyperealVR.Instance.MirrorMode = MirrorMode;
        HyperealVR.Instance.MirrorType = MirrorType;
        HyperealVR.Instance.IPD = IPD;
        HyperealVR.Instance.FOV = FOV;
        HyperealVR.Instance.PixelDensity = PixelDensity;
        HyperealVR.Instance.RenderScale = RenderScale;

        if(recenterType != HyRecenterType.Recenter_None)
        {
            HyperealVR.Instance.RecenterBase(recenterType);
            recenterType = HyRecenterType.Recenter_None;
        }

        SyncVRNormalUI();
    }

    private void SyncVRNormalUI()
    {
        scaleRateMaxVR.text = maxScaleRate.ToString();
        scaleRateMax.text = maxScaleRate.ToString();

        scaleRateVR.value = RenderScale;
        scaleRate.value = RenderScale;

        scaleRateVR.maxValue = maxScaleRate;
        scaleRate.maxValue = maxScaleRate;

        scaleRateCurVR.text = RenderScale.ToString();
        scaleRateCur.text = RenderScale.ToString();

        pixelDensityVR.value = PixelDensity;
        pixelDensity.value = PixelDensity;

        pixelDensityCurVR.text = PixelDensity.ToString();
        pixelDensityCur.text = PixelDensity.ToString();

        trackingOriginVR.value = (int)TrackingOrigin;
        trackingOrigin.value = (int)TrackingOrigin;

        mirrorModeVR.value = (int)MirrorMode;
        mirrorMode.value = (int)MirrorMode;

        mirrorTypeVR.value = (int)MirrorType;
        mirrorType.value = (int)MirrorType;
    }

    private void UpdateVRUITransform(bool repos = false)
    {
        HyCamera hyCamera = HyRender.GetLastCamera();
        if (hyCamera == null || hyCamera.head == null)
            return;

        if (settingUIVR.gameObject.activeSelf)
        {
            if (repos)
            {
                Transform head = hyCamera.head;
                stickQuat = head.rotation;
                stickPos = head.position;
                stickForward = head.forward;
                stickScale = RenderScale;
            }
            float scaleFactor = RenderScale / stickScale;
            settingUIVR.transform.rotation = stickQuat;
            settingUIVR.transform.localScale = new Vector3(RenderScale, RenderScale, RenderScale);
            settingUIVR.transform.position = (stickPos * scaleFactor + stickForward * RenderScale * distance);
        }
    }

    private void OnSettingTrigger(HyInput input, HyInputKey key, HyInputKeyEventType type)
    {
        bool currentStatus = settingUIVR.gameObject.activeSelf;
        settingUIVR.gameObject.SetActive(!currentStatus);
        settingUI.gameObject.SetActive(!currentStatus);
        UpdateVRUITransform(true);
    }

    private void OnScaleRateMaxChanged(string value)
    {
        float maxV = maxScaleRate;
        if (float.TryParse(value, out maxV))
        {
            maxScaleRate = maxV;
            valueChanged = true;
        }
    }

    private void OnScaleRateChanged(string value)
    {
        float scale = RenderScale;
        if (float.TryParse(value, out scale))
        {
            RenderScale = scale;            
            valueChanged = true;
            UpdateVRUITransform();
        }
    }

    private void OnPixelDensityChanged(string value)
    {
        float p = PixelDensity;
        if (float.TryParse(value, out p))
        {
            PixelDensity = p;
            valueChanged = true;
        }
    }

    private void OnScaleRateChanged(float value)
    {
        RenderScale = value;
        valueChanged = true;
        UpdateVRUITransform();
    }

    private void OnPixelDensityChanged(float value)
    {        
        PixelDensity = value;
        valueChanged = true;
    }

    private void OnTrackingOriginChanged(int value)
    {
        TrackingOrigin = (HyTrackingOrigin)(value);
        valueChanged = true;
    }

    private void OnMirrorModeChanged(int value)
    {
        MirrorMode = (HyMirrorMode)(value);
        valueChanged = true;
    }

    private void OnMirrorTypeChanged(int value)
    {
        MirrorType = (HyMirrorType)(value);
        valueChanged = true;
    }

    private void OnRecenterPos()
    {
        recenterType = HyRecenterType.Recenter_Position;
        valueChanged = true;
    }

    private void OnRecenterRot()
    {
        recenterType = HyRecenterType.Recenter_Rotation_Yaw;
        valueChanged = true;
    }

    private void OnRecenterPosRot()
    {
        recenterType = HyRecenterType.Recenter_All;
        valueChanged = true;
    }
}
