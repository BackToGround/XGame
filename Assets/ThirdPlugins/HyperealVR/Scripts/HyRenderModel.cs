using UnityEngine;
using System.Collections;

namespace Hypereal
{
#pragma warning disable 0414

    [ExecuteInEditMode]
    public class HyRenderModel : MonoBehaviour
    {
        // Use this for initialization
        public HyDevice device;
        public bool ShowModel = true;
        private GameObject model = null;
        private GameObject baseModel = null;
        private GameObject camModel = null;
        private bool listenPose = false;
        void OnEnable()
        {
            HyTrackObj track = this.GetComponent<HyTrackObj>();
            if (track != null) device = track.device;

            Transform t = this.transform.Find("Model");
            model = t != null ? t.gameObject : null;
            if (model == null)
            {
                string strPrefab = "";
                switch(device)
                {
                    case HyDevice.Device_Controller0:
                        strPrefab = "Prefabs/HyFeelLeft";
                        break;
                    case HyDevice.Device_Controller1:
                        strPrefab = "Prefabs/HyFeelRight";
                        break;
                    case HyDevice.Device_Tracker0:
                    case HyDevice.Device_Tracker1:
                        strPrefab = "Prefabs/HyCamera";
                        break;
                }
                GameObject temp = Resources.Load<GameObject>(strPrefab);
                if (temp != null)
                {
                    model = Instantiate(temp) as GameObject;
                    model.name = "Model";
                    model.transform.SetParent(this.transform, false);
                }
            }

            if (HyperealVR.GetDeviceType(device) == HyDevice.Device_Tracker)
            {
                if (track != null) track.enabled = false;
                HyperealVR.OnNewPose += OnNewPose;
                listenPose = true;
                baseModel = this.transform.Find("Model/Base").gameObject;
                camModel = this.transform.Find("Model/Cam").gameObject;
            }
            else if(track == null)
            {
                this.gameObject.AddComponent<HyTrackObj>();
            }
        }

        void Update()
        {
            if (HyperealVR.GetDeviceType(device) == HyDevice.Device_Tracker)
            {
                baseModel.SetActive(ShowModel);
                camModel.SetActive(ShowModel);
            }
            else
            {
                model.SetActive(ShowModel);
            }
        }
       

        private void OnDisable()
        {
            if (listenPose)
                HyperealVR.OnNewPose -= OnNewPose;
            listenPose = false;            
            model = null;
        }

        private void OnNewPose()
        {
            HyTrackingState pose = HyperealVR.Instance.GetTrackingState(device);
            if (!pose.isConnected())
            {
                //TODO:
                return;
            }

            transform.localRotation = pose.pose.orientation;
            transform.localPosition = pose.pose.position;
            if (HyperealVR.GetDeviceType(device) == HyDevice.Device_Tracker)
            {
                Vector3 euler = transform.localRotation.eulerAngles;
                transform.localRotation = Quaternion.Euler(0.0f, euler.y, 0.0f);
                if (camModel != null) camModel.transform.localRotation = Quaternion.Euler(euler.x, 0.0f, 0.0f);
            }
        }
    }
}

