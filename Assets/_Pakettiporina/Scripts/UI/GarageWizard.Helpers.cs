using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pakettiporina
{
    // 24.11.2026: GarageWizard pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: sprite- ja UI-apurit.
    public partial class GarageWizard
    {
        // ---------------------------------------------------------------- sprite-apurit
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static Sprite circle;

        static Sprite Load(string name)
        {
            Sprite s;
            if (cache.TryGetValue(name, out s)) return s;
            var tex = Resources.Load<Texture2D>("WizardUI/" + name);
            if (tex != null) s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            cache[name] = s;
            return s;
        }

        static Sprite Circle()
        {
            if (circle != null) return circle;
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(N / 2f, N / 2f));
                    px[y * N + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(N / 2f - d + 0.5f) * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            circle = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }

        // ---------------------------------------------------------------- UI-apurit
        static RectTransform R(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static Image Img(RectTransform rt, Sprite s, Color c, bool sliced = false, float ppum = 0.3f, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = s; img.color = c; img.raycastTarget = raycast;
            if (sliced && s != null && s.border != Vector4.zero) { img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = ppum; }
            else img.preserveAspect = true;
            return img;
        }

        static Image Pill(RectTransform rt, Color c, float ppum = 0.3f, bool raycast = false)
        {
            return Img(rt, Juice.PillSprite(), c, true, ppum, raycast);
        }

        static TextMeshProUGUI Txt(Transform parent, string s, float size, Color c, TextAlignmentOptions al = TextAlignmentOptions.Center, float x0 = 0, float y0 = 0, float x1 = 1, float y1 = 1)
        {
            var rt = R("Text", parent, x0, y0, x1, y1);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = s; t.color = c; t.alignment = al; t.fontStyle = FontStyles.Bold;
            t.enableAutoSizing = true; t.fontSizeMax = size; t.fontSizeMin = Mathf.Max(24f, size * 0.55f);
            t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false;
            return t;
        }

        static Button Btn(RectTransform rt, UnityAction a)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) { img = rt.gameObject.AddComponent<Image>(); img.color = new Color(1, 1, 1, 0); }
            img.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img; b.transition = Selectable.Transition.None;
            rt.gameObject.AddComponent<JuiceButtonPress>();   // heti painuma (ei odota Juicen skannausta)
            b.onClick.AddListener(a);
            return b;
        }

        // Pehmea nappi: varjo + paalaatta. Palauttaa paalaatan rectin (varjo on sisarena sen alla).
        static RectTransform Chunky(string name, Transform parent, float x0, float y0, float x1, float y1, Color face, Color shadow, out Image faceImg, out Image shadowImg)
        {
            var holder = R(name, parent, x0, y0, x1, y1);
            var sh = R("Shadow", holder, 0, 0, 1, 1);
            sh.offsetMin = new Vector2(0, -14); sh.offsetMax = new Vector2(0, -14);
            shadowImg = Pill(sh, shadow);
            var f = R("Face", holder, 0, 0, 1, 1);
            faceImg = Pill(f, face, 0.3f, true);
            return holder;
        }
    }
}
