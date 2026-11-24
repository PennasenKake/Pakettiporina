using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026 (kayttaja: pilvet piti olla paavalikossa taivaalla ja pelisceneissa
    // taivaalla niin ettei tule tielle). Pelisceneissa (joissa on RaceManager) hitaasti
    // liikkuvat pilvet piirretaan VAIN ruudun ylalaitaan (taivaan alue), omalla Canvasilla
    // HUDin alapuolella. Muissa sceneissa (Halli, Pesu) ei pilvia.
    public static class SkyClouds
    {
        const int CloudCount = 4;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            SceneManager.sceneLoaded += (scene, mode) => Build(scene);
            Build(SceneManager.GetActiveScene());
        }

        static void Build(Scene scene)
        {
            if (Object.FindObjectOfType<RaceManager>() == null) return;
            if (GameObject.Find("SkyCloudsCanvas") != null) return;

            var go = new GameObject("SkyCloudsCanvas");
            SceneManager.MoveGameObjectToScene(go, scene);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -50; // HUDin alla
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<SkyCloudsMover>();
        }
    }

    public class SkyCloudsMover : MonoBehaviour
    {
        class C { public RectTransform rt; public float speed; public float x; }
        readonly List<C> clouds = new List<C>();
        float halfW, top;

        void Start()
        {
            var root = (RectTransform)transform;
            var sprite = MainMenuLife.MakeCloudSprite();
            halfW = root.rect.width * 0.5f;
            top = root.rect.height * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                var g = new GameObject("Pilvi" + i, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)g.transform;
                rt.SetParent(root, false);
                var img = g.GetComponent<Image>();
                img.sprite = sprite; img.raycastTarget = false;
                img.color = new Color(1f, 1f, 1f, Random.Range(0.75f, 0.95f));   // 24.11.2026: nakyvammat (ajonakymassa taivas on vain ylin ~19 %)
                float w = root.rect.width * Random.Range(0.18f, 0.3f);
                rt.sizeDelta = new Vector2(w, w * 0.5f);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                // vain ylin kaistale (taivas): 6-16 % ruudun ylareunasta
                // 24.11.2026: kaistaa laskettu 10-17 %:iin, jotta pilvet eivat jaa kokonaan HUD-mittarien taakse
                float y = top - root.rect.height * Random.Range(0.10f, 0.17f);
                float x = Random.Range(-halfW, halfW);
                rt.anchoredPosition = new Vector2(x, y);
                clouds.Add(new C { rt = rt, speed = Random.Range(10f, 28f), x = x });
            }
        }

        void Update()
        {
            foreach (var c in clouds)
            {
                if (c.rt == null) continue;
                c.x += c.speed * Time.unscaledDeltaTime;
                if (c.x > halfW + c.rt.sizeDelta.x) c.x = -halfW - c.rt.sizeDelta.x;
                var p = c.rt.anchoredPosition; p.x = c.x; c.rt.anchoredPosition = p;
            }
        }
    }
}
