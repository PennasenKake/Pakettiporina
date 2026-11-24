using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // 24.11.2026: Juice pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: UI-hiukkaset, pilleri-sprite, UI-konfetti.
    public partial class Juice
    {
        // ------------------------------------------------------------------ UI-hiukkaset
        // Kuplan poksahdus: pienia pisaroita lentaa ulos annetun UI-elementin kohdalta.
        public static void UIBurst(RectTransform at, Color color, int count = 10)
        {
            if (at == null || at.parent == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.UIBurstRoutine(at, color, count));
        }

        IEnumerator UIBurstRoutine(RectTransform at, Color color, int count)
        {
            var parent = at.parent;
            var dots = new List<RectTransform>();
            var vel = new List<Vector2>();
            Vector3 origin = at.localPosition;
            float size = Mathf.Clamp(Mathf.Min(at.rect.width, at.rect.height) * 0.12f, 10f, 28f);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("JuiceDot", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(parent, false);
                rt.localPosition = origin;
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(size, size);
                var im = go.GetComponent<Image>();
                im.sprite = DotSprite();
                im.raycastTarget = false;
                im.color = Color.Lerp(color, Color.white, Random.value * 0.6f);
                rt.SetAsLastSibling();
                float ang = (i / (float)count) * Mathf.PI * 2f + Random.value * 0.5f;
                vel.Add(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(180f, 420f));
                dots.Add(rt);
            }
            float e = 0f, dur = 0.5f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                float k = e / dur;
                for (int i = 0; i < dots.Count; i++)
                {
                    if (dots[i] == null) continue;
                    var v = vel[i];
                    v.y -= 600f * Time.unscaledDeltaTime;
                    vel[i] = v;
                    dots[i].localPosition += (Vector3)(v * Time.unscaledDeltaTime);
                    dots[i].localScale = Vector3.one * (1f - k * 0.8f);
                    var im = dots[i].GetComponent<Image>();
                    var c = im.color; c.a = 1f - k; im.color = c;
                }
                yield return null;
            }
            foreach (var d in dots) if (d != null) Destroy(d.gameObject);
        }

        // ------------------------------------------------------------------ pilleri-sprite + UI-konfetti
        static Sprite pillSprite;

        // Pyoristetty suorakulmio (9-slice), kaytetaan HUD-tekstien taustalaattoihin.
        public static Sprite PillSprite()
        {
            if (pillSprite != null) return pillSprite;
            const int W = 64, H = 32, R = 16;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, R, W - R), cy = Mathf.Clamp(y + 0.5f, R, H - R);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(R - d + 0.5f);
                    px[y * W + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            pillSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(R, R, R, R));
            return pillSprite;
        }

    }
}
