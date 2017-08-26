using UnityEngine;
using System.Collections;

namespace Hypereal
{
    public class HyPlayZone
    {
        public Vector2[] Points = null;

        public HyPlayZone()
        {

        }

        float P2LineDistance(Vector3 P, Vector2 PA, Vector2 PB)
        {
            // point to line's signed distance
            float xx = PB.x - PA.x;
            float yy = PB.y - PA.y;
            return (-(yy * P.x + xx * P.z + PB.x * PA.y - PB.y * PA.x) / Mathf.Sqrt(xx * xx + yy * yy));
        }

        float P2LineSegDistance(Vector3 P3, Vector2 PA, Vector2 PB)
        {
            Vector2 P = new Vector2(P3.x, P3.z);
            Vector2 PAB = PB - PA;
            Vector2 PAP = P - PA;

            float len2 = PAB.SqrMagnitude();
            if (len2 <= 0.00001 && len2 >= -0.00001)
                return PAP.magnitude;

            float t = Mathf.Max(0.0f, Mathf.Min(1.0f, Vector2.Dot(PAP, PAB) / len2));
            Vector2 Proj = PA + t * PAB;
            return Vector2.Distance(P, Proj);
        }

        bool IsInsidePoly(Vector3 p, Vector2[] playArea)
        {
            if (playArea == null || playArea.Length < 3)
                return false;

            int i;
            int j;
            bool c = false;
            int nvert = playArea.Length;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((playArea[i].y > p.z) != (playArea[j].y > p.z)) &&
                 (p.x < (playArea[j].x - playArea[i].x) * (p.z - playArea[i].y) / (playArea[j].y - playArea[i].y) + playArea[i].x))
                    c = !c;
            }
            return c;
        }

        public void UpdateState(HyperealVR.PlayZoneStateHandler handler)
        {            
            if (!HyperealVR.IsStereoEnabled && 
                !HyperealVR.Instance.IsTrackingEnabled)
                return;
            if (Points == null)
                return;
            if (handler == null)
                return;

            Vector2 PA;
            Vector2 PB;
            const float max_float = 99999.9f;
            HyDevice[] trackingDevice = HyperealVR.Instance.GetTrackedDevice();
            float[] minDistances = new float[trackingDevice.Length];
            for (int i = 0; i < minDistances.Length; i++)
                minDistances[i] = max_float;

            int vertexCount = Points.Length;
            int lastIndex = vertexCount - 1;

            for (int d = 0; d < trackingDevice.Length; d++)
            {
                HyTrackingState state = HyperealVR.Instance.GetTrackingStateRaw(trackingDevice[d]);
                if (!state.isPoseTracked())
                    continue;

                bool insidePoly = IsInsidePoly(state.pose.position, Points);
                for (int i = 0; i < vertexCount; ++i)
                {
                    PA = Points[i];
                    if (i == lastIndex)
                        PB = Points[0];
                    else
                        PB = Points[i + 1];

                    float dist = P2LineSegDistance(state.pose.position, PA, PB);
                    if (dist < minDistances[d])
                        minDistances[d] = dist;

                    //Debug.DrawLine(new Vector3(PA.x, 0, PA.y), new Vector3(PB.x, 0, PB.y), Color.red);
                }
                if (!insidePoly && minDistances[d] < max_float)
                    minDistances[d] = -minDistances[d];

                //Debug.Log("Distance: " + minDistances[d].ToString());
            }

            for (int i = 0; i < minDistances.Length; i++)
            {
                //TODO: calculate the normal
                if (minDistances[i] < max_float)
                    handler((HyDevice)i, minDistances[i], Vector3.forward);
            }
        }
    }

}
