using UnityEngine;
using System.Collections;

namespace Hypereal
{

    // ThackObjRig only manage controllers position because they should always 
    // transform with the HMD.
    [ExecuteInEditMode]
    public class HyTrackObjRig : MonoBehaviour
    {
        public bool ShowModel = true;
        public GameObject leftController = null;
        public GameObject rightController = null;

        private HyRenderModel componentL;
        private HyRenderModel componentR;
        private void Start()
        {
            if (leftController == null)
            {
                Transform t = this.transform.Find("ControllerLeft");
                leftController = t != null ? t.gameObject : null;
            }
            if (rightController == null)
            {
                Transform t = this.transform.Find("ControllerRight");
                rightController = t != null ? t.gameObject : null; 
            }
            componentL = leftController.GetComponent(typeof(HyRenderModel)) as HyRenderModel;
            componentR = rightController.GetComponent(typeof(HyRenderModel)) as HyRenderModel;
        }

        // Update is called once per frame
        private void Update()
        {
            if (!HyperealVR.IsStereoEnabled)
                return;


            if (componentL != null) componentL.ShowModel = ShowModel;
            if (componentR != null) componentR.ShowModel = ShowModel;

            HyCamera curr = HyRender.GetLastCamera();
            if (curr != null && this.transform.parent != curr.origin)
            {
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
                this.transform.localScale = Vector3.one;
                this.transform.SetParent(curr.origin, false);
            }
        }
    }
}

