using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // 24.11.2026: Juice pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: proseduraaliset HUD-spritet.
    public partial class Juice
    {
        // ------------------------------------------------------------------ HUD-sprite:t (proseduraaliset, ei binaaritiedostoja)
        static Sprite barFrame, barFill, starIcon, clockIcon;

        static Sprite MakeSprite(int w, int h, System.Func<int, int, Color32> f, Vector4 border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        static Color32 C32(float r, float g, float b, float a) { return new Color32((byte)(Mathf.Clamp01(r) * 255f), (byte)(Mathf.Clamp01(g) * 255f), (byte)(Mathf.Clamp01(b) * 255f), (byte)(Mathf.Clamp01(a) * 255f)); }

        // Teraskehys, jonka paat ovat teravat (kuvan "tulppa"-palkki muunnettuna Pakettiporinaan).
        public static Sprite BarFrameSprite()
        {
            if (barFrame != null) return barFrame;
            const int W = 96, H = 32; const float Ch = 12f;
            barFrame = MakeSprite(W, H, (x, y) =>
            {
                float fy = y + 0.5f, fx = x + 0.5f;
                float xmin = Ch * Mathf.Abs(fy - H / 2f) / (H / 2f);
                float inL = fx - xmin, inR = (W - xmin) - fx;
                float inside = Mathf.Min(inL, inR);
                if (inside < 0f) return C32(0, 0, 0, 0);
                float a = Mathf.Clamp01(inside + 0.5f);
                float t = fy / H;                        // 0 alhaalla, 1 ylhaalla
                float v = Mathf.Lerp(0.30f, 0.78f, t);   // pystygradientti: teras
                if (fy > H - 4f && fy < H - 2f) v = 0.92f; // kirkas yläreuna
                if (inside < 2f || fy < 1.5f || fy > H - 1.5f) v = 0.14f; // tumma ääriviiva
                return C32(v * 0.92f, v * 0.96f, v, a);
            }, new Vector4(Ch + 3f, 7f, Ch + 3f, 7f));
            return barFrame;
        }

        // Kiiltava palkin taytto (valkoinen -> varjataan Image.colorilla).
        public static Sprite BarFillSprite()
        {
            if (barFill != null) return barFill;
            const int W = 32, H = 16, R = 6;
            barFill = MakeSprite(W, H, (x, y) =>
            {
                float cx = Mathf.Clamp(x + 0.5f, R, W - R), cy = Mathf.Clamp(y + 0.5f, R, H - R);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float a = Mathf.Clamp01(R - d + 0.5f);
                float t = (y + 0.5f) / H;
                float v = Mathf.Lerp(0.62f, 1f, t);
                if (t > 0.72f && t < 0.9f) v = Mathf.Min(1.25f, v + 0.25f);   // kiillejuova
                return C32(v, v, v, a);
            }, new Vector4(R, R, R, R));
            return barFill;
        }

        static bool InPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) && p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) inside = !inside;
            return inside;
        }

        // Tahti-ikoni = pisteet/kerätyt tähdet.
        public static Sprite StarIcon()
        {
            if (starIcon != null) return starIcon;
            const int N = 64;
            Vector2[] Poly(float scale)
            {
                var v = new Vector2[10];
                for (int i = 0; i < 10; i++)
                {
                    float r = (i % 2 == 0 ? 29f : 12f) * scale;
                    float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                    v[i] = new Vector2(N / 2f + Mathf.Cos(a) * r, N / 2f + 1f + Mathf.Sin(a) * r);
                }
                return v;
            }
            var outer = Poly(1f); var inner = Poly(0.78f);
            starIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (InPoly(p, inner)) return C32(1f, Mathf.Lerp(0.78f, 0.95f, y / (float)N), 0.15f, 1f);
                if (InPoly(p, outer)) return C32(0.75f, 0.4f, 0.05f, 1f);
                return C32(0, 0, 0, 0);
            }, Vector4.zero);
            return starIcon;
        }

        // LISATTY 24.11.2026: kolikko-ikoni = POISTETTAVAT pisteet (tarrat, palkkio). Tahti on varattu ajotahdille
        // (lukitukset) ja radan bonustahdille, jotta ne eivat sekoitu pisteisiin.
        static Sprite coinIcon;
        public static Sprite CoinIcon()
        {
            if (coinIcon != null) return coinIcon;
            const int N = 64;
            var c = new Vector2(N / 2f, N / 2f);
            coinIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                if (d > 30f) return C32(0, 0, 0, 0);
                if (d > 25f) return C32(0.72f, 0.45f, 0.05f, 1f);               // tumma kultareunus
                if (d > 21f) return C32(1f, 0.9f, 0.4f, 1f);                    // vaalea rengas
                // keskella "p"-merkki: pysty + pallo (pisteet)
                bool stem = Mathf.Abs(p.x - (c.x - 5f)) < 3.2f && p.y > c.y - 14f && p.y < c.y + 14f;
                bool bowl = Vector2.Distance(p, new Vector2(c.x + 1f, c.y + 5f)) < 9.5f && Vector2.Distance(p, new Vector2(c.x + 1f, c.y + 5f)) > 4f && p.x > c.x - 5f;
                if (stem || bowl) return C32(0.72f, 0.45f, 0.05f, 1f);
                return C32(1f, Mathf.Lerp(0.72f, 0.88f, y / (float)N), 0.2f, 1f);
            }, Vector4.zero);
            return coinIcon;
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        // Kello-ikoni = jaljella oleva aika.
        public static Sprite ClockIcon()
        {
            if (clockIcon != null) return clockIcon;
            const int N = 64;
            var c = new Vector2(N / 2f, N / 2f);
            clockIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                if (d > 30f) return C32(0, 0, 0, 0);
                if (SegDist(p, c, c + new Vector2(0f, 20f)) < 2.6f || SegDist(p, c, c + new Vector2(13f, -7f)) < 2.6f) return C32(0.1f, 0.15f, 0.3f, 1f);
                if (d > 24f) return C32(0.2f, 0.55f, 1f, 1f);   // sininen kehys
                return C32(0.97f, 0.97f, 1f, 1f);
            }, Vector4.zero);
            return clockIcon;
        }

        // Maalin konfetti UI-tasossa (maailman partikkelit jaivat portin taakse). Putoavia varikkaita paloja.
        public static void UIConfetti(RectTransform panel, int count = 70)
        {
            if (panel == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.UIConfettiRoutine(panel, count));
        }

        IEnumerator UIConfettiRoutine(RectTransform panel, int count)
        {
            var canvas = panel.GetComponentInParent<Canvas>();
            var croot = canvas != null ? (RectTransform)canvas.rootCanvas.transform : panel;
            float W = Mathf.Max(300f, croot.rect.width), H = Mathf.Max(500f, croot.rect.height);
            Color[] cs = { new Color(1f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.1f), new Color(0.2f, 0.85f, 0.4f), new Color(0.3f, 0.55f, 1f), new Color(0.9f, 0.35f, 0.95f), Color.white };
            var pieces = new List<RectTransform>();
            var vel = new List<Vector3>();   // x = sway-vaihe, y = putoamisnopeus, z = pyorimisnopeus
            float size = Mathf.Max(18f, W * 0.025f);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(panel, false);
                rt.SetSiblingIndex(Mathf.Min(1, panel.childCount - 1));   // scrimin paalle, tekstien alle
                rt.sizeDelta = new Vector2(size, size * Random.Range(0.5f, 1.3f));
                rt.localPosition = new Vector3(Random.Range(-W * 0.5f, W * 0.5f), H * 0.5f + Random.Range(0f, H * 0.35f), 0f);
                var im = go.GetComponent<Image>();
                im.color = cs[Random.Range(0, cs.Length)];
                im.raycastTarget = false;
                pieces.Add(rt);
                vel.Add(new Vector3(Random.value * 6.28f, Random.Range(H * 0.35f, H * 0.7f), Random.Range(-360f, 360f)));
            }
            float e = 0f, dur = 4f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == null) continue;
                    var p = pieces[i].localPosition;
                    p.y -= vel[i].y * Time.unscaledDeltaTime;
                    p.x += Mathf.Sin(e * 3f + vel[i].x) * 60f * Time.unscaledDeltaTime;
                    pieces[i].localPosition = p;
                    pieces[i].localRotation = Quaternion.Euler(0f, 0f, vel[i].z * e);
                }
                yield return null;
            }
            foreach (var p in pieces) if (p != null) Destroy(p.gameObject);
        }
    }
}
