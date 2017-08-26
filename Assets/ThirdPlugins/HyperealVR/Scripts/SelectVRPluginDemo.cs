using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectVRPluginDemo : MonoBehaviour
{
    // Use this for initialization
    public GameObject normalRig = null;
    public GameObject hyperealVRRig = null;
    public GameObject[] VRCameras;
    public float delayEnableVR = 2.0f;
    public float switchInterval = 2.0f;

    private int currentVRIdx = 0;
    private float oldtimeScale = 0.0f;

    void Awake()
    {
        if (delayEnableVR <= 0.0f)
            StartVR();
        else
            StartCoroutine(DelayStartVR());
        StartCoroutine(SwitchBetweenCameras());
        Hypereal.HyperealVR.Instance.EnableSettingUI(true);

        oldtimeScale = Time.timeScale;
        Hypereal.HyperealVR.OnVisibilityChange += OnVisibilityChange;
    }

    void OnDestroy()
    {
        Hypereal.HyperealVR.OnVisibilityChange -= OnVisibilityChange;
        StopAllCoroutines();
    }

    void OnVisibilityChange()
    {
        if (!Hypereal.HyperealVR.Instance.IsVisible)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = oldtimeScale;
        }
    }

    IEnumerator DelayStartVR()
    {
        yield return new WaitForSeconds(delayEnableVR);
        StartVR();
    }

    IEnumerator SwitchBetweenCameras()
    {
        while(true)
        {
            yield return new WaitForSeconds(switchInterval <= 0.0f ? 0.0f : switchInterval);

            if(VRCameras != null && switchInterval > 0.0f)
            {
                currentVRIdx = ++currentVRIdx % VRCameras.Length;
                foreach (var v in VRCameras)
                {
                    v.SetActive(false);
                }
                VRCameras[currentVRIdx].SetActive(true);
            }
        }
    }

    void StartVR()
    {
        if (Hypereal.HyperealVR.IsHyperealPresent)
        {

            if (hyperealVRRig != null)
                hyperealVRRig.SetActive(true);
            if (normalRig != null)
                normalRig.SetActive(false);
        }
        else
        {
            if (normalRig != null)
                normalRig.SetActive(true);
            if (hyperealVRRig != null)
                hyperealVRRig.SetActive(false);
        }
    }
}
