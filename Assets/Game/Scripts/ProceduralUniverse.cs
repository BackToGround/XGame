using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BTGXGame
{
    /*********************************************
     * 先计算星球所占屏幕空间中的大小，然后依据屏幕大小来调整生成模型的精细度
     *******************************************/

    public class ProceduralUniverse : MonoBehaviour
    {
        public float UniverseSize = float.MaxValue;

        public float PlaneInitSize = 100000f;

        public Transform PlaneClone;

        private PlaneBaseNode[] mLoadedPlanes;

        private Camera mCamera;

        void Start()
        {
            LoadPlanes();

            mCamera = Camera.main;
            mCamera.farClipPlane = PlaneInitSize;
            mCamera.transform.position = mLoadedPlanes[0].mPlanePosition + Vector3.one * mLoadedPlanes[0].mPlaneRadius * 0.501f;
            mCamera.transform.LookAt(mLoadedPlanes[0].mPlaneTransform);
        }

        /// <summary>
        /// 加载星球
        /// </summary>
        void LoadPlanes()
        {
            int totalPlanesCount = 12;
            float initPlaneSize = PlaneInitSize;

            mLoadedPlanes = new PlaneBaseNode[totalPlanesCount];
            float distance = 0f;
            System.Text.StringBuilder planeName = new System.Text.StringBuilder();
            for (int i = 0; i < totalPlanesCount; i += 4)
            {
                int currentPlaneIndex = i;

                mLoadedPlanes[currentPlaneIndex].mPlaneID = currentPlaneIndex;
                mLoadedPlanes[currentPlaneIndex].mPlaneRadius = currentPlaneIndex != 0 ? currentPlaneIndex * initPlaneSize : initPlaneSize;
                distance = currentPlaneIndex != 0 ? distance + mLoadedPlanes[currentPlaneIndex - 1].mPlaneRadius + mLoadedPlanes[currentPlaneIndex].mPlaneRadius : 0f;
                mLoadedPlanes[currentPlaneIndex].mPlanePosition = new Vector3(distance, UnityEngine.Random.Range(100f, 10000f), currentPlaneIndex);
                Transform plane = GameObject.Instantiate(PlaneClone) as Transform;
                mLoadedPlanes[currentPlaneIndex].mPlaneTransform = plane;
                planeName.Remove(0, planeName.Length);
                planeName.Append("Plane_");
                planeName.Append(currentPlaneIndex);
                plane.name = planeName.ToString();
                plane.position = mLoadedPlanes[currentPlaneIndex].mPlanePosition;
                plane.rotation = Quaternion.identity;
                plane.localScale = Vector3.one * mLoadedPlanes[currentPlaneIndex].mPlaneRadius;

                currentPlaneIndex++;
                mLoadedPlanes[currentPlaneIndex].mPlaneID = currentPlaneIndex;
                mLoadedPlanes[currentPlaneIndex].mPlaneRadius = currentPlaneIndex != 0 ? currentPlaneIndex * initPlaneSize : initPlaneSize;
                distance = currentPlaneIndex != 0 ? distance + mLoadedPlanes[currentPlaneIndex - 1].mPlaneRadius + mLoadedPlanes[currentPlaneIndex].mPlaneRadius : 0f;
                mLoadedPlanes[currentPlaneIndex].mPlanePosition = new Vector3(-distance, UnityEngine.Random.Range(100f, 10000f), currentPlaneIndex);
                plane = GameObject.Instantiate(PlaneClone) as Transform;
                mLoadedPlanes[currentPlaneIndex].mPlaneTransform = plane;
                planeName.Remove(0, planeName.Length);
                planeName.Append("Plane_");
                planeName.Append(currentPlaneIndex);
                plane.name = planeName.ToString();
                plane.position = mLoadedPlanes[currentPlaneIndex].mPlanePosition;
                plane.rotation = Quaternion.identity;
                plane.localScale = Vector3.one * mLoadedPlanes[currentPlaneIndex].mPlaneRadius;

                currentPlaneIndex++;
                mLoadedPlanes[currentPlaneIndex].mPlaneID = currentPlaneIndex;
                mLoadedPlanes[currentPlaneIndex].mPlaneRadius = currentPlaneIndex != 0 ? currentPlaneIndex * initPlaneSize : initPlaneSize;
                distance = currentPlaneIndex != 0 ? distance + mLoadedPlanes[currentPlaneIndex - 1].mPlaneRadius + mLoadedPlanes[currentPlaneIndex].mPlaneRadius : 0f;
                mLoadedPlanes[currentPlaneIndex].mPlanePosition = new Vector3(currentPlaneIndex, UnityEngine.Random.Range(100f, 10000f), distance);
                plane = GameObject.Instantiate(PlaneClone) as Transform;
                mLoadedPlanes[currentPlaneIndex].mPlaneTransform = plane;
                planeName.Remove(0, planeName.Length);
                planeName.Append("Plane_");
                planeName.Append(currentPlaneIndex);
                plane.name = planeName.ToString();
                plane.position = mLoadedPlanes[currentPlaneIndex].mPlanePosition;
                plane.rotation = Quaternion.identity;
                plane.localScale = Vector3.one * mLoadedPlanes[currentPlaneIndex].mPlaneRadius;

                currentPlaneIndex++;
                mLoadedPlanes[currentPlaneIndex].mPlaneID = currentPlaneIndex;
                mLoadedPlanes[currentPlaneIndex].mPlaneRadius = currentPlaneIndex != 0 ? currentPlaneIndex * initPlaneSize : initPlaneSize;
                distance = currentPlaneIndex != 0 ? distance + mLoadedPlanes[currentPlaneIndex - 1].mPlaneRadius + mLoadedPlanes[currentPlaneIndex].mPlaneRadius : 0f;
                mLoadedPlanes[currentPlaneIndex].mPlanePosition = new Vector3(currentPlaneIndex, UnityEngine.Random.Range(100f, 10000f), -distance);
                plane = GameObject.Instantiate(PlaneClone) as Transform;
                mLoadedPlanes[currentPlaneIndex].mPlaneTransform = plane;
                planeName.Remove(0, planeName.Length);
                planeName.Append("Plane_");
                planeName.Append(currentPlaneIndex);
                plane.name = planeName.ToString();
                plane.position = mLoadedPlanes[currentPlaneIndex].mPlanePosition;
                plane.rotation = Quaternion.identity;
                plane.localScale = Vector3.one * mLoadedPlanes[currentPlaneIndex].mPlaneRadius;
            }
        }

        /// <summary>
        /// 根据玩家当前位置判断星球是否处于可是范围内
        /// </summary>
        /// <param name="planeID"></param>
        /// <returns></returns>
        bool IsPlaneInArea(long planeID)
        {
            if (planeID < 0 || planeID >= mLoadedPlanes.Length) return false;

            return true;
        }

        /// <summary>
        /// 获得当前玩家可视范围内的所有星球
        /// </summary>
        /// <param name="playerPosition"></param>
        void GetAllPlanesInArea(double playerPosition)
        {

        }

        /// <summary>
        /// 向指定星球移动
        /// </summary>
        /// <param name="planeID"></param>
        void MoveToPlane(long planeID)
        {
            Vector3 direction = (mLoadedPlanes[planeID].mPlanePosition - mCamera.transform.position).normalized;
            mCamera.transform.position += direction * 1000f * Time.deltaTime;
        }

        private void Update()
        {
            MoveToPlane(0);
        }
    }
}
