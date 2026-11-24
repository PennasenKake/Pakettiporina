using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 24.11.2026 (kayttaja: "hallissa valittu auton vari ei nay ajossa eika
    // pesu/korjaus-tilassa"). Yksi yhteinen paikka halliin valitun Maali-osan varille,
    // jota kayttavat CarPainter (ajo, 3D-auto) ja WashScreen (pesu/korjaus, 2D-auto).
    public static class CarColor
    {
        // Palauttaa true ja varin jos pelaajalla on valittuna Maali-osa.
        public static bool TryGetSelected(out Color color)
        {
            color = Color.white;
            var gm = GameManager.Instance;
            if (gm == null || gm.SelectedParts == null) return false;
            foreach (var part in gm.SelectedParts)
            {
                if (part != null && part.category == PartCategory.Maali)
                {
                    color = part.color;
                    color.a = 1f;
                    return true;
                }
            }
            return false;
        }

        // Varittaa 2D-auton (Image) samalla tavalla kuin Hallin esikatselu
        // (GarageScreen.carPreview.color = part.color), jotta vari on sama kaikissa nakymissa.
        public static void TintImage(Image img)
        {
            if (img == null) return;
            if (TryGetSelected(out var c)) Paint(img, c);
        }

        // ---- 2D-auton oikea uudelleenvarjays (23.11.2026) --------------------------------------
        // Play-testi: pelkka Image.color-kertoma vaarensi varin (punainen auto x keltainen = oranssi).
        // Nyt etsitaan spriten hallitseva "kori"-vari ja korvataan vain se valitulla varilla
        // (varjostus sailyy), ikkunat/renkaat pysyvat ennallaan. Tulos valimuistissa.
        static readonly Dictionary<Sprite, Sprite> originalOf = new Dictionary<Sprite, Sprite>();
        static readonly Dictionary<(int, Color32), Sprite> cache = new Dictionary<(int, Color32), Sprite>();

        public static void Paint(Image img, Color target)
        {
            if (img == null) return;
            var src = img.sprite;
            if (src == null) { img.color = target; return; }
            if (originalOf.TryGetValue(src, out var orig)) src = orig;

            Color32 t32 = target;
            var key = (src.GetInstanceID(), t32);
            if (!cache.TryGetValue(key, out var painted) || painted == null)
            {
                painted = Recolor(src, t32);
                if (painted != null) { cache[key] = painted; originalOf[painted] = src; }
            }
            if (painted == null) { img.color = target; return; }
            img.sprite = painted;
            img.color = Color.white;
        }

        static Sprite Recolor(Sprite src, Color32 target)
        {
            var tex = src.texture;
            if (tex == null) return null;
            Rect r = src.textureRect;
            int w = (int)r.width, h = (int)r.height;

            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;
            // KORJATTU 23.11.2026 (auton ala-osa leikkautui Hallissa/Pesussa): ReadPixels RT:sta rajattuna
            // palautti vaaran alueen (RT:n y-akseli kaanteinen). Luetaan KOKO tekstuuri ja rajataan
            // tulos itse tekstuuriavaruuden (textureRect) koordinaateilla.
            var full = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false, false);
            full.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            full.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            var all = full.GetPixels32();
            Object.Destroy(full);
            int rx = (int)r.x, ry = (int)r.y;
            var px = new Color32[w * h];
            for (int yy = 0; yy < h; yy++)
                System.Array.Copy(all, (ry + yy) * tex.width + rx, px, yy * w, w);
            var copy = new Texture2D(w, h, TextureFormat.RGBA32, false, false);

            // 1) hallitseva kylladinen vari (histogrammi 16x16x16)
            var bins = new Dictionary<int, int>();
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i];
                if (p.a < 200) continue;
                Color.RGBToHSV(p, out _, out float s, out float v);
                if (s < 0.45f || v < 0.4f) continue;
                int k = (p.r >> 4) << 8 | (p.g >> 4) << 4 | (p.b >> 4);
                bins.TryGetValue(k, out int c);
                bins[k] = c + 1;
            }
            if (bins.Count == 0) { Object.Destroy(copy); return null; }
            int bestK = 0, bestC = -1;
            foreach (var kv in bins) if (kv.Value > bestC) { bestC = kv.Value; bestK = kv.Key; }
            int br = ((bestK >> 8) & 15) * 16 + 8, bg = ((bestK >> 4) & 15) * 16 + 8, bb = (bestK & 15) * 16 + 8;
            float refLum = Mathf.Max(0.05f, (0.299f * br + 0.587f * bg + 0.114f * bb) / 255f);

            // 2) korvaa kori-sävyn pikselit (laaja toleranssi varjostuksille)
            for (int i = 0; i < px.Length; i++)
            {
                var p = px[i];
                if (p.a == 0) continue;
                int d = Mathf.Abs(p.r - br) + Mathf.Abs(p.g - bg) + Mathf.Abs(p.b - bb);
                if (d > 150) continue;
                float lum = (0.299f * p.r + 0.587f * p.g + 0.114f * p.b) / 255f;
                float k = Mathf.Clamp(lum / refLum, 0.3f, 1.25f);
                float blend = Mathf.Clamp01(1f - d / 150f); // reunat sulavasti
                byte nr = (byte)Mathf.Clamp(target.r * k, 0, 255);
                byte ng = (byte)Mathf.Clamp(target.g * k, 0, 255);
                byte nb = (byte)Mathf.Clamp(target.b * k, 0, 255);
                px[i] = new Color32(
                    (byte)Mathf.Lerp(p.r, nr, blend),
                    (byte)Mathf.Lerp(p.g, ng, blend),
                    (byte)Mathf.Lerp(p.b, nb, blend), p.a);
            }
            copy.SetPixels32(px);
            copy.Apply();
            copy.filterMode = FilterMode.Bilinear;
            copy.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(copy, new Rect(0, 0, w, h), new Vector2(src.pivot.x / w, src.pivot.y / h), src.pixelsPerUnit);
        }
    }
}
