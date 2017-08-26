using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hypereal
{

    [RequireComponent(typeof(Camera))]
    public class HyHead : MonoBehaviour
    {
        // Use this for initialization
        bool RenderStereo(RenderTexture dst)
        {
            bool hdr = GetComponent<Camera>().isHDR();
            Texture left = HyRender.Instance.GetSceneTexture(HyEyeType.EyeType_Left, hdr);
            Texture right = HyRender.Instance.GetSceneTexture(HyEyeType.EyeType_Right, hdr);

            int width = left.width + right.width;
            int height = (int)((left.height + right.height) * 0.5);

            float aw = 1.0f;
            float ah = 1.0f;

            HyMirrorMode mode = HyperealVR.Instance.MirrorMode;

            if (mode != HyMirrorMode.Stretch)
            {
                aw = (float)width / (float)Screen.width;
                ah = (float)height / (float)Screen.height;
                float a = (mode == HyMirrorMode.Adaption ? Mathf.Max(aw, ah) : Mathf.Min(aw, ah));
                aw /= a;
                ah /= a;
            }

            Vector4 uvScaleOffset = new Vector4(0.5f - 0.5f * aw, 0.5f + 0.5f * aw, 0.5f - 0.5f * ah, 0.5f + 0.5f * ah);
            HyRender.Instance.MirrorBlitMat.SetVector("_UVAspectRatio", uvScaleOffset);
            HyRender.Instance.MirrorBlitMat.SetVector("_UVClipFlip", new Vector4(0, 1.0f, 0.0f, 0.0f));
            HyRender.Instance.MirrorBlitMat.SetTexture("_AlternativeTex", right);
            Graphics.Blit(left, dst, HyRender.Instance.MirrorBlitMat, 1);
            return true;
        }

        bool RenderOther(RenderTexture dst)
        {
            bool hdr = GetComponent<Camera>().isHDR();
            Texture tex = HyRender.Instance.MirrorTexture;
            float scale = 1.0f;
            float offset = 0.0f;
            float flipV = 1.0f;
            float cropScale = 1.0f;
            switch (HyperealVR.Instance.MirrorType)
            {
                case HyMirrorType.Left_Distorted:
                    {
                        cropScale = 1.1f;
                        scale = 0.5f;
                        offset = 0.0f;
                    }
                    break;
                case HyMirrorType.Right_Distorted:
                    {
                        cropScale = 1.1f;
                        scale = 0.5f;
                        offset = 0.5f;
                    }
                    break;
                case HyMirrorType.Stereo_Distorted:
                    {
                        cropScale = 1.2f;
                        scale = 1.0f;
                        offset = 0.0f;
                    }
                    break;
                case HyMirrorType.Left:
                    {
                        tex = HyRender.Instance.GetSceneTexture(HyEyeType.EyeType_Left, hdr);
                        flipV = 0.0f;
                    }
                    break;
                case HyMirrorType.Right:
                    {
                        tex = HyRender.Instance.GetSceneTexture(HyEyeType.EyeType_Right, hdr);
                        flipV = 0.0f;
                    }
                    break;
            }

            if (tex == null)
                return false;

            float aw = 1.0f;
            float ah = 1.0f;

            HyMirrorMode mode = HyperealVR.Instance.MirrorMode;
            if (mode != HyMirrorMode.Stretch)
            {
                aw = (float)tex.width * scale / (float)Screen.width;
                ah = (float)tex.height / (float)Screen.height;
                if(mode == HyMirrorMode.Adaption)
                {
                    float a = Mathf.Max(aw, ah);
                    aw /= a;
                    ah /= a;
                }
                else
                {
                    float a = Mathf.Min(aw, ah);
                    aw /= a;
                    ah /= a;
                    aw *= cropScale;
                    ah *= cropScale;
                }
            }

            Vector4 uvScaleOffset = new Vector4(0.5f - 0.5f * aw, 0.5f + 0.5f * aw, 0.5f - 0.5f * ah, 0.5f + 0.5f * ah);
            HyRender.Instance.MirrorBlitMat.SetVector("_UVAspectRatio", uvScaleOffset);
            HyRender.Instance.MirrorBlitMat.SetVector("_UVClipFlip", new Vector4(offset, scale, 0.0f, flipV));
            Graphics.Blit(tex, dst, HyRender.Instance.MirrorBlitMat, 0);
            return true;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!HyperealVR.IsStereoEnabled ||
                 HyRender.Instance.MirrorBlitMat == null ||
                 HyRender.Instance.MirrorTexture == null)
            {
                Graphics.Blit(src, dst);
                return;
            }

            bool blitted = RenderOther(dst);
            if (!HyperealVR.Instance.IsVisible 
                || HyLoadingHelper.loading)
            {
                // since we don't submit when pausing, the frame will unlimited when V-Sync is off.
                // sleep to pretend the waste of system resources.
                System.Threading.Thread.Sleep(100); 
                if (!blitted) Graphics.Blit(src, dst);
                return;
            }
         
            if (HyperealVR.Instance.MirrorType == HyMirrorType.Stereo)
                blitted = RenderStereo(dst);
            if (!blitted)
                Graphics.Blit(src, dst);
        }

        void UpdateTransform(Transform t)
        {
            Vector3 localPos = t.localPosition;
            Quaternion localRot = t.localRotation;
            Vector3 localScale = t.localScale;
            Transform p = t.parent;
            t.parent = null;

            t.localPosition = localPos;
            t.localRotation = localRot;
            t.localScale = localScale;

            t.SetParent(p, false);
        }

        internal void UpdateTransformImmediately()
        {
            UpdateTransform(this.transform);

            List<Transform> children = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                children.Add(transform.GetChild(i));
            }

            foreach (Transform child in children)
            {
                UpdateTransform(child);
            }
        }
    }
}
