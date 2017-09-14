using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTGXGame
{
    public struct PlaneBaseNode
    {
        public long mPlaneID;

        public float mPlaneRadius;

        public Vector3 mPlanePosition;

        public Transform mPlaneTransform;

        PlaneBaseNode(long planeID, float planeRadius, Vector3 planePosition, Transform planeTransform)
        {
            mPlaneID = planeID;
            mPlaneRadius = planeRadius;
            mPlanePosition = planePosition;
            mPlaneTransform = planeTransform;
        }
    }
}
