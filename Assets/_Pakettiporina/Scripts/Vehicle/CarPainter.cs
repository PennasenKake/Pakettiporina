using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Maalaa ajossa olevan auton hallissa valitun Maali-osan varilla.
    // Liita samaan objektiin kuin ArcadeCarController (auton juureen).
    //
    // KORJATTU 23.11.2026 (vari ei nakynyt ajossa): ajossa nakyva auto on Car1.fbx ("Visual"),
    // jonka kaikki osat kayttavat YHTA paletti-materiaalia (Auto_Paletti + 256x256 paletti-PNG).
    // Aiempi versio maalasi vain Car.prefabin Body-kuution "Auto_kori"-materiaalia, jota ei
    // nay. Nyt maalataan:
    //   1) kaikki lasten Rendererit joiden materiaalin nimessa on "kori" (vanha polku), ja
    //   2) paletti-materiaalit: luodaan paletista oma kopio, jossa korin paletti-ruudun
    //      (paletteBodyColor) pikselit korvataan valitulla varilla. Renkaat, ikkunat yms.
    //      eivat varity mukana.
    public class CarPainter : MonoBehaviour
    {
        [Tooltip("Valinnainen: auton korin Renderer ('kori'-materiaali). Tyhja = etsitaan automaattisesti.")]
        public Renderer bodyRenderer;

        [Tooltip("Osuma materiaalin nimesta joka tunnistaa korin (pieni/iso kirjain ei valita)")]
        public string materialNameContains = "kori";

        [Tooltip("Osuma paletti-materiaalin nimesta (Car1.fbx kayttaa Auto_Paletti).")]
        public string paletteNameContains = "paletti";

        [Tooltip("Korin vari paletissa (Auto_Paletti.png:n punainen ruutu 227,74,74 - varmistettu Play-testissa).")]
        public Color32 paletteBodyColor = new Color32(227, 74, 74, 255);

        [Tooltip("Sallittu poikkeama (0-255 per kanava) paletin korin varin tunnistuksessa.")]
        public int paletteTolerance = 14;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        MaterialPropertyBlock mpb;
        Texture2D paintedPalette;
        Color32 paintedFor;
        bool hasPainted;
        bool warned;

        void Start()
        {
            if (GetComponent<Rigidbody>() != null && GetComponent<CarRecovery>() == null)
                gameObject.AddComponent<CarRecovery>(); // putoamissuoja (23.11.2026)
            ApplySelectedColor();
            // Uusitaan yhden framen jalkeen, jos joku muu ehtii muuttaa materiaaleja.
            StartCoroutine(ReapplyNextFrame());
        }

        void OnEnable()
        {
            if (hasPainted) ApplySelectedColor();
        }

        void OnDestroy()
        {
            if (paintedPalette != null) Destroy(paintedPalette);
        }

        System.Collections.IEnumerator ReapplyNextFrame()
        {
            yield return null;
            ApplySelectedColor();
        }

        // Hakee hallissa valitun Maali-osan varin GameManagerista ja maalaa autoon.
        public void ApplySelectedColor()
        {
            if (!CarColor.TryGetSelected(out var chosen))
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("[CarPainter] Maalivalintaa ei loytynyt (GameManager.SelectedParts ei sisalla Maali-osaa) - jatetaan oletusvari.");
                }
                return;
            }

            int painted = 0;
            Color32 chosen32 = chosen;

            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || r is ParticleSystemRenderer || r is TrailRenderer) continue;
                var shared = r.sharedMaterials;
                Material[] instances = null;

                for (int i = 0; i < shared.Length; i++)
                {
                    var sm = shared[i];
                    if (sm == null) continue;
                    string n = sm.name.ToLowerInvariant();

                    bool isBody = n.Contains(materialNameContains.ToLowerInvariant());
                    bool isPalette = !isBody && n.Contains(paletteNameContains.ToLowerInvariant());
                    if (!isBody && !isPalette) continue;

                    if (instances == null) instances = r.materials; // instansseja vain taman auton renderereille
                    var mat = instances[i];

                    if (isBody)
                    {
                        if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, chosen);
                        if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, chosen);
                        painted++;
                    }
                    else if (PaintPalette(mat, chosen32))
                    {
                        painted++;
                    }
                }
            }

            // Vanha polku: erikseen asetettu bodyRenderer saa myos varin PropertyBlockina.
            if (bodyRenderer != null && bodyRenderer.sharedMaterial != null &&
                bodyRenderer.sharedMaterial.name.ToLowerInvariant().Contains(materialNameContains.ToLowerInvariant()))
            {
                if (mpb == null) mpb = new MaterialPropertyBlock();
                bodyRenderer.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, chosen);
                mpb.SetColor(ColorId, chosen);
                bodyRenderer.SetPropertyBlock(mpb);
            }

            hasPainted = true;
            if (painted == 0)
                Debug.LogWarning("[CarPainter] Mikaan materiaali ei tasmannyt ('" + materialNameContains + "' / '" + paletteNameContains + "') - autoa ei maalattu.");
            else
                Debug.Log("[CarPainter] Auton vari asetettu: " + chosen + " (" + painted + " materiaalia)");
        }

        // Luo paletista kopion jossa korin ruutu on valittu vari ja asettaa sen materiaali-instanssiin.
        bool PaintPalette(Material mat, Color32 chosen)
        {
            int texId = mat.HasProperty(BaseMapId) ? BaseMapId : (mat.HasProperty(MainTexId) ? MainTexId : 0);
            if (texId == 0) return false;

            if (paintedPalette == null || !paintedFor.Equals(chosen))
            {
                var src = ReadableSource(mat, texId);
                if (src == null) return false;

                var px = src.GetPixels32();
                int w = src.width, h = src.height;
                int cell = Mathf.Max(1, w / 4);   // paletti on 4x4 ruutua
                // Valitaan ruutu jonka keskipiste on lahinna korin paletti-varia
                // (ei kiinteaa toleranssia -> kestaa varinhallinnan pienet erot).
                int bestCx = 0, bestCy = 0; float bestD = float.MaxValue;
                for (int cy = 0; cy < 4; cy++)
                    for (int cx = 0; cx < 4; cx++)
                    {
                        var c = px[(cy * cell + cell / 2) * w + (cx * cell + cell / 2)];
                        float d = Mathf.Abs(c.r - paletteBodyColor.r) + Mathf.Abs(c.g - paletteBodyColor.g) + Mathf.Abs(c.b - paletteBodyColor.b);
                        if (d < bestD) { bestD = d; bestCx = cx; bestCy = cy; }
                    }
                for (int y = bestCy * cell; y < (bestCy + 1) * cell && y < h; y++)
                    for (int x = bestCx * cell; x < (bestCx + 1) * cell && x < w; x++)
                        px[y * w + x] = new Color32(chosen.r, chosen.g, chosen.b, 255);
                Debug.Log("[CarPainter] Paletin ruutu (" + bestCx + "," + bestCy + ") maalattu, ero alkuperaiseen " + bestD);
                src.SetPixels32(px);
                src.Apply(true);
                src.filterMode = FilterMode.Point;

                if (paintedPalette != null && paintedPalette != src) Destroy(paintedPalette);
                paintedPalette = src;
                paintedFor = chosen;
            }

            mat.SetTexture(texId, paintedPalette);
            // Varmistetaan ettei materiaalin oma savy sotke maalattua palettia.
            if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, Color.white);
            if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, Color.white);
            return true;
        }

        // Palauttaa luettavan kopion materiaalin paletista (alkuperainen PNG ei ole Read/Write).
        // Jos materiaalissa on jo meidan maalaama kopio, luetaan alkuperainen sen tilalle.
        Texture2D ReadableSource(Material mat, int texId)
        {
            var tex = mat.GetTexture(texId);
            if (tex == null) return null;

            if (tex == paintedPalette && paintedPalette != null)
            {
                // Maalattu kopio on jo luettava - mutta alkuperaiset varit on ylikirjoitettu,
                // joten tarvitaan alkuperainen. Haetaan se jaetusta materiaalista.
                foreach (var r in GetComponentsInChildren<Renderer>(true))
                {
                    var shared = r.sharedMaterial;
                    if (shared != null && shared.HasProperty(texId))
                    {
                        var orig = shared.GetTexture(texId);
                        if (orig != null && orig != paintedPalette) { tex = orig; break; }
                    }
                }
            }

            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;
            Graphics.Blit(tex, rt);
            RenderTexture.active = rt;
            var copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, true, false);
            copy.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            copy.Apply(true);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            copy.wrapMode = tex.wrapMode;
            return copy;
        }
    }
}
