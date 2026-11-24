using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: elava paavalikko. Rakentaa itsensa koodissa (ei scene-muutoksia):
    //  - liikkuvat pilvet (proseduraalinen pilvisprite)
    //  - pieni auto ajaa ruudun poikki (Resources/MenuCar.png)
    //  - Vilkku tervehtii pelin tilan mukaan (MascotAnimator.speechLines)
    // MainMenu.Start() lisaa taman automaattisesti.
    public class MainMenuLife : MonoBehaviour
    {
        public int cloudCount = 5;
        [Tooltip("Pieni auto ajaa ruudun poikki. Pois oletuksena: Play-testissa se peitti napit ja tuplasi taustakuvan auton.")]
        public bool showCar = false;
        public float carHeightFraction = 0.22f;   // auton korkeus suhteessa ruudun korkeuteen
        public float carBottomFraction = 0.06f;   // auton alareuna suhteessa ruudun korkeuteen
        public float carSpeed = 140f;             // px/s (canvas-yksikoita)
        public float carPauseSeconds = 5f;        // tauko ennen seuraavaa ajoa

        class Mover { public RectTransform rt; public float speed; public float phase; public float baseY; public bool isCar; public float wait; }
        readonly List<Mover> movers = new List<Mover>();
        RectTransform layer;
        Rect area;
        static Sprite cloudSprite;
        static Sprite carSprite;

        void Start()
        {
            // KORJATTU 23.11.2026: FindObjectOfType<Canvas>() loysi SceneFaderin pysyvan (DontDestroyOnLoad)
            // Canvasin, jolloin pilvet jaivat nakyviin KAIKKIIN sceneihin. Valitaan vain TAMAN scenen Canvas.
            Canvas canvas = null;
            foreach (var c in FindObjectsOfType<Canvas>())
            {
                if (c == null || !c.isRootCanvas) continue;
                if (c.gameObject.scene != gameObject.scene) continue;        // ei DontDestroyOnLoad-canvaseja
                if (c.GetComponentInParent<SceneFader>() != null) continue;
                if (canvas == null || c.sortingOrder < canvas.sortingOrder) canvas = c;
            }
            if (canvas == null) return;

            BuildLayer(canvas);
            if (layer != null)
            {
                BuildClouds();
                if (showCar) BuildCar();
            }
            SetupGreeting();
        }

        // ---- taso ----------------------------------------------------------------------------
        void BuildLayer(Canvas canvas)
        {
            var root = (RectTransform)canvas.transform;
            var go = new GameObject("LifeLayer", typeof(RectTransform));
            layer = (RectTransform)go.transform;
            layer.SetParent(root, false);
            layer.anchorMin = Vector2.zero; layer.anchorMax = Vector2.one;
            layer.offsetMin = Vector2.zero; layer.offsetMax = Vector2.zero;

            // Sijoita heti taustakuvan jalkeen (pilvet taustan paalle, mutta kaikkien paneelien alle).
            int idx = 0;
            for (int i = 0; i < root.childCount; i++)
            {
                var c = root.GetChild(i);
                if (c == layer) continue;
                var im = c.GetComponent<Image>();
                if (im != null && im.sprite != null && im.sprite.name.ToLowerInvariant().Contains("tausta")) { idx = i + 1; break; }
            }
            layer.SetSiblingIndex(Mathf.Min(idx, root.childCount - 1));
            area = root.rect;
        }

        // ---- pilvet --------------------------------------------------------------------------
        void BuildClouds()
        {
            if (cloudSprite == null) cloudSprite = MakeCloudSprite();
            for (int i = 0; i < cloudCount; i++)
            {
                var go = new GameObject("Pilvi" + i, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(layer, false);
                var img = go.GetComponent<Image>();
                img.sprite = cloudSprite;
                img.raycastTarget = false;
                img.color = new Color(1f, 1f, 1f, Random.Range(0.55f, 0.9f));
                float w = area.width * Random.Range(0.14f, 0.26f);
                rt.sizeDelta = new Vector2(w, w * 0.5f);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                float y = area.height * Random.Range(0.18f, 0.46f);
                float x = Random.Range(-area.width * 0.5f, area.width * 0.5f);
                rt.anchoredPosition = new Vector2(x, y);
                movers.Add(new Mover { rt = rt, speed = Random.Range(8f, 26f), phase = Random.value * 6.28f, baseY = y });
            }
        }

        internal static Sprite MakeCloudSprite()
        {
            const int W = 128, H = 64;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[W * H];
            // muutama paallekkainen pehmea ympyra
            var blobs = new[] { new Vector3(36, 26, 20), new Vector3(60, 34, 26), new Vector3(86, 28, 20), new Vector3(50, 24, 18), new Vector3(74, 22, 17) };
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float a = 0f;
                    foreach (var b in blobs)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(b.x, b.y)) / b.z;
                        a = Mathf.Max(a, 1f - Mathf.SmoothStep(0.65f, 1f, d));
                    }
                    if (y < 14) a *= y / 14f; // litteahko pohja
                    px[y * W + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---- auto ----------------------------------------------------------------------------
        void BuildCar()
        {
            if (carSprite == null)
            {
                var tex = Resources.Load<Texture2D>("MenuCar");
                if (tex == null) return;
                carSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            var go = new GameObject("MenuAuto", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(layer, false);
            var img = go.GetComponent<Image>();
            img.sprite = carSprite;
            img.raycastTarget = false;
            float h = area.height * carHeightFraction;
            float w = h * carSprite.rect.width / carSprite.rect.height;
            rt.sizeDelta = new Vector2(w, h);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            float y = area.height * carBottomFraction + h * 0.5f;
            rt.anchoredPosition = new Vector2(-area.width * 0.5f - w, y);
            movers.Add(new Mover { rt = rt, speed = carSpeed, baseY = y, isCar = true, wait = 1.5f });
        }

        CanvasGroup layerGroup;
        MascotAnimator mascotRef;

        void OnDestroy()
        {
            if (layer != null) Destroy(layer.gameObject);
        }

        void Update()
        {
            if (movers.Count == 0) return;

            // Piilota paneelien (asetukset, tietoja, tietosuoja...) ollessa auki - sama lista kuin maskotilla.
            if (layer != null)
            {
                if (layerGroup == null) layerGroup = layer.gameObject.AddComponent<CanvasGroup>();
                if (mascotRef == null) mascotRef = FindObjectOfType<MascotAnimator>(true);
                bool hide = false;
                if (mascotRef != null && mascotRef.hideWhilePanelsActive != null)
                    foreach (var g in mascotRef.hideWhilePanelsActive)
                        if (g != null && g.activeInHierarchy) { hide = true; break; }
                layerGroup.alpha = hide ? 0f : 1f;
            }
            float dt = Time.unscaledDeltaTime;
            float t = Time.unscaledTime;
            float halfW = area.width * 0.5f;

            foreach (var m in movers)
            {
                if (m.rt == null) continue;
                var p = m.rt.anchoredPosition;
                float w = m.rt.sizeDelta.x;

                if (m.isCar)
                {
                    if (m.wait > 0f) { m.wait -= dt; continue; }
                    p.x += m.speed * dt;
                    // pieni pomppu + kallistus ajon aikana
                    p.y = m.baseY + Mathf.Abs(Mathf.Sin(t * 9f)) * 4f;
                    m.rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 9f) * 0.8f);
                    if (p.x > halfW + w)
                    {
                        p.x = -halfW - w;
                        m.wait = carPauseSeconds;
                    }
                    m.rt.anchoredPosition = p;
                }
                else
                {
                    p.x += m.speed * dt;
                    p.y = m.baseY + Mathf.Sin(t * 0.4f + m.phase) * 6f;
                    if (p.x > halfW + w) p.x = -halfW - w;
                    m.rt.anchoredPosition = p;
                }
            }
        }

        // ---- Vilkun tervehdys ----------------------------------------------------------------
        void SetupGreeting()
        {
            var mascot = FindObjectOfType<MascotAnimator>(true);
            if (mascot == null) return;

            var gm = GameManager.Instance;
            var lines = new List<string>();

            if (gm == null || (gm.Points == 0 && gm.SelectedParts.Count == 0))
            {
                lines.Add("Hei! Olen Vilkku. Rakennetaan sinulle auto!");
            }
            else
            {
                lines.Add(gm.Points == 1 ? "Tervetuloa takaisin! Sinulla on 1 kolikko." : $"Tervetuloa takaisin! Sinulla on {gm.Points} kolikkoa.");
                if (gm.DirtMeter > 0 || gm.DentMeter > 0)
                    lines.Add("Autosi kaipaa pesua ja korjausta!");
                else
                    lines.Add("Auto kiiltää. Onko uusi keikka vuorossa?");
            }
            lines.Add("Paina Pelaa ja lähdetään!");

            // Tilannekohtaiset rivit ensin, sitten maskotin omat vanhat rivit.
            if (mascot.speechLines != null) lines.AddRange(mascot.speechLines);
            mascot.speechLines = lines.ToArray();
        }
    }
}
