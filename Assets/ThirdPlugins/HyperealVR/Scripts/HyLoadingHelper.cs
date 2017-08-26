using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;

namespace Hypereal
{

    public class HyLoadingHelper : MonoBehaviour
    {
        private static HyLoadingHelper instance = null;

        public static bool loading { get { return instance != null; } }

        public static float progress
        {
            get { return (instance != null && instance.async != null) ? instance.async.progress : 0.0f; }
        }

        // the texture 
        public Texture2D loadingTex;

        private Texture2D currTex;

        // If true, call LoadLevelAdditiveAsync instead of LoadLevelAsync.
        public bool loadAdditive;

        // Async load causes crashes in some apps.
        public bool loadAsync = true;

        AsyncOperation async; // used to track level load progress

        public string levelName;

        // If true, start coroutine if when the GameObject enable
        public bool autoTriggerOnEnable = false;

        void OnEnable()
        {
            if (autoTriggerOnEnable)
                Trigger();
        }

        // This can be manually called 
        public void Trigger()
        {
            if (!loading && !string.IsNullOrEmpty(levelName))
                StartCoroutine("LoadLevel");
        }

        // 
        public static void Begin(string levelName = null, Texture2D loadingTex = null)
        {
            if (HyRender.Instance == null) return;

            var loader = new GameObject("loader").AddComponent<HyLoadingHelper>();

            loader.levelName = levelName;
            if(loadingTex != null)
                loader.loadingTex = FlipTexture(loadingTex);
            loader.Trigger();
        }

        void Update()
        {
            if (instance != this)
            {
                return;
            }

            var progress = (async != null) ? async.progress : 0.0f;
            HyperealApi.SendEventMessage(HyEventMessage.Event_UpdateLoadingProgress, progress);

            if (loadingTex != null && currTex != loadingTex)
            {
                var handle = loadingTex.GetNativeTexturePtr();
                Debug.Log("handle tex = " + handle);

                HyperealApi.SendEventMessage(HyEventMessage.Event_UpdateLoadingTex, handle);
                currTex = loadingTex;
            }
        }

        IEnumerator LoadLevel()
        {
            instance = this;

            if (levelName == null)
            {
                yield return null;
            }

            HyperealApi.SendEventMessage(HyEventMessage.Event_BeginLoading);
            HyperealApi.SendEventMessage(HyEventMessage.Event_UpdateLoadingProgress, 0.0f);
            yield return new WaitForSeconds(0.1f);

            if (loadingTex != null)
            {
                var pTex = loadingTex.GetNativeTexturePtr();
                HyperealApi.SendEventMessage(HyEventMessage.Event_UpdateLoadingTex, pTex);
            }

            // don't destroy this 'loader' or it will destroy while load new scene.
            DontDestroyOnLoad(gameObject);

            yield return null;

#if UNITY_5_3_OR_NEWER
            var mode = loadAdditive ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single;
            if (loadAsync)
            {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
                async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName, mode);

                while (!async.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelName, mode);
            }
#else
            if (loadAsync)
            {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
                async = Application.LoadLevelAsync(levelName);

                while (!async.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                Application.LoadLevel(levelName);
            }
#endif
            yield return new WaitForSeconds(0.1f);

            GC.Collect();

            yield return new WaitForSeconds(0.1f);

            // manually destroy this loader.
            Destroy(gameObject);
            instance = null;

            HyperealApi.SendEventMessage(HyEventMessage.Event_EndLoading);
        }

        static Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
        {

            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;


            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    if (upSideDown)
                    {
                        flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                    }
                    else
                    {
                        flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                    }
                }
            }
            flipped.Apply();

            return flipped;
        }

    }
}