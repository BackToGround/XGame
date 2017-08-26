using UnityEngine;
using System.Collections;

namespace Hypereal
{
    public class HyTrackObj : MonoBehaviour
    {
        public HyDevice device;

        void OnEnable()
        {
            HyperealVR.OnNewPose += OnNewPose;
        }

        void OnDisable()
        {
            HyperealVR.OnNewPose -= OnNewPose;
        }

        void OnNewPose()
        {
            HyTrackingState pose = HyperealVR.Instance.GetTrackingState(device);
            if (!pose.isConnected())
            {
                //TODO:
                return;
            }

            transform.localRotation = pose.pose.orientation;
            transform.localPosition = pose.pose.position;
        }
    }
}

