using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // 24.11.2026: RaceHUD pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: lika/kolhumittarit ja HUD-asettelu.
    public partial class RaceHUD
    {
        void HandleDirtMeterChanged(int current, int max)
        {
            float target = max > 0 ? (float)current / max : 0f;
            EnsureWidthSetup(dirtState);
            ApplyLevelColor(dirtState, target, true);
            if (dirtSynced && target >= 0.8f && target > lastDirt) Juice.Play("warning");   // "uh-oh" kun palkki lahes taynna
            lastDirt = target;
            // Ensimmainen synkronointi (OnEnable, scenen alussa) hypataan suoraan -
            // vain OIKEAT osumat ajon aikana animoituvat.
            if (!dirtSynced)
            {
                SetBarWidth(dirtState, target);
                dirtSynced = true;
            }
            else if (dirtState.rt != null)
            {
                if (dirtState.routine != null) StopCoroutine(dirtState.routine);
                dirtState.routine = StartCoroutine(HitFeedback(dirtState, target));
            }
            // KORJATTU 19.8.2026: "Lika X/Y" -teksti poistettu kayttajan pyynnosta -
            // palkki (koon muutos + pomppu/vilahdus) riittaa nyt yksinaan.
        }

        void HandleDentMeterChanged(int current, int max)
        {
            float target = max > 0 ? (float)current / max : 0f;
            EnsureWidthSetup(dentState);
            ApplyLevelColor(dentState, target, false);
            if (dentSynced && target >= 0.8f && target > lastDent) Juice.Play("warning");
            lastDent = target;
            if (!dentSynced)
            {
                SetBarWidth(dentState, target);
                dentSynced = true;
            }
            else if (dentState.rt != null)
            {
                if (dentState.routine != null) StopCoroutine(dentState.routine);
                dentState.routine = StartCoroutine(HitFeedback(dentState, target));
            }
            // KORJATTU 19.8.2026: "Kolhut X/Y" -teksti poistettu kayttajan pyynnosta -
            // palkki (koon muutos + pomppu/vilahdus) riittaa nyt yksinaan.
        }
        bool dirtSynced, dentSynced;
        float lastDirt, lastDent;

        // KORJATTU NELJANNEN KERRAN 19.8.2026 (ks. luokkakommentti hitFeedbackDuration-
        // kentan yllä): palkin LEVEYS (RectTransform.sizeDelta.x - kirjaimellinen koon
        // muutos, ei Image.fillAmount-maskaus) seka pomppu+vilahdus animoidaan yhdessa,
        // KIINTEAKESTOISESSA coroutinessa - AINA taman metodin koko keston (esim. 0.45s)
        // ajan, riippumatta siita kuinka pieni muutos on.
        IEnumerator HitFeedback(MeterBarState st, float targetFraction)
        {
            var bar = st.bar;
            var rt = st.rt;
            if (!st.colorCached) { st.baseColor = bar.color; st.colorCached = true; }
            Color baseColor = st.baseColor;

            Vector3 baseScale = Vector3.one;
            float startWidth = rt.sizeDelta.x;
            float targetWidth = st.fullWidth * Mathf.Clamp01(targetFraction);
            float dur = Mathf.Max(0.05f, hitFeedbackDuration);
            float e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(e / dur);
                // Koon muutos: ease-out (nopea alku, pehmea loppu) - koko ajan nakyva liike.
                float eased = 1f - (1f - k) * (1f - k);
                var sd = rt.sizeDelta;
                sd.x = Mathf.Lerp(startWidth, targetWidth, eased);
                rt.sizeDelta = sd;
                // Pomppu+vilahdus: nousee puoliväliin, laskee takaisin (huippu keskella).
                float pulse = k < 0.5f ? (k / 0.5f) : (1f - (k - 0.5f) / 0.5f);
                rt.localScale = Vector3.Lerp(baseScale, baseScale * hitPunchScale, pulse);
                bar.color = Color.Lerp(baseColor, hitFlashColor, pulse);
                yield return null;
            }
            var final = rt.sizeDelta;
            final.x = targetWidth;
            rt.sizeDelta = final;
            rt.localScale = baseScale;
            bar.color = baseColor;
        }


        void HandleStart()
        {
            if (finishPanel != null) finishPanel.SetActive(false);
            if (controls != null) controls.SetActive(false);
            if (starText != null) starText.text = "0";   // tahti-ikoni kertoo merkityksen (23.11.2026)
            if (countdownText != null) { countdownText.gameObject.SetActive(true); countdownText.text = ""; }
            lastShownTime = -1f;
        }

        // ---- Mittarien siisti asettelu + saavutettavuus (23.11.2026) -----------------------------
        // Kayttajan palaute: "Kolhut ja Lika palkit voisivat olla paremmin asetellut ja siistimmat".
        // Konttien (DirtMeterBar/DentMeterBar) sijainti ja koko asetetaan koodissa: kaksi samankokoista
        // pilleria vierekkain ylhaalla, sisalla [ikoni][selite][palkki].
        IEnumerator InitMeters()
        {
            yield return null; // Canvas-skaalaaja ehtii laskea koon
            ArrangeMeters();
            Canvas.ForceUpdateCanvases();
            var gm = GameManager.Instance;
            if (gm != null)
            {
                HandleDirtMeterChanged(gm.DirtMeter, GameManager.DIRT_MAX);
                HandleDentMeterChanged(gm.DentMeter, GameManager.DENT_MAX);
            }
        }

        bool metersArranged;

        void ArrangeMeters()
        {
            if (metersArranged) return;
            if (dirtMeterBar == null || dentMeterBar == null) return;
            var canvas = dirtMeterBar.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var croot = (RectTransform)canvas.rootCanvas.transform;
            float W = croot.rect.width, H = croot.rect.height;
            if (W < 10f || H < 10f) return;
            metersArranged = true;

            // 23.11.2026 (kayttaja: "tee pidemmat jotta vaikutus nakyy progress barissa"): kaksi pitkaa pillia
            // allekkain (leveys 62 % ruudusta) -> palkki on moninkertaisesti pidempi kuin vierekkain.
            float w = W * 0.62f, h = Mathf.Max(34f, H * 0.032f), vgap = H * 0.006f, top = H * 0.011f;
            ArrangeOne(dirtMeterBar, dirtMeterLabel, "Lika", 0f, -top, w, h);
            ArrangeOne(dentMeterBar, dentMeterLabel, "Kolhut", 0f, -(top + h + vgap), w, h);
        }

        void ArrangeOne(Image fill, TMP_Text label, string text, float x, float y, float w, float h)
        {
            var box = fill.transform.parent as RectTransform;
            if (box == null) return;
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 1f);
            box.pivot = new Vector2(0.5f, 1f);
            box.anchoredPosition = new Vector2(x, y);
            box.sizeDelta = new Vector2(w, h);
            box.localScale = Vector3.one;
            // Teraskehys teravin paatyin (kayttajan referenssikuva muunnettuna), sisalla tumma ura + kiiltava taytto.
            var bg = box.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = Juice.BarFrameSprite();
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }

            // ikoni vasemmalle (neliö), kehyksen terava paa huomioitu
            float icon = h - 14f;
            float iconX = 22f;
            foreach (Transform c in box)
            {
                if (!c.name.EndsWith("_Icon")) continue;
                var ir = (RectTransform)c;
                ir.anchorMin = ir.anchorMax = new Vector2(0f, 0.5f);
                ir.pivot = new Vector2(0f, 0.5f);
                ir.anchoredPosition = new Vector2(iconX, 0f);
                ir.sizeDelta = new Vector2(icon, icon);
            }

            // selite ikonin viereen
            float labelW = w * 0.22f;
            if (label != null)
            {
                var lr = label.rectTransform;
                label.gameObject.SetActive(true);
                lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(0f, 1f);
                lr.pivot = new Vector2(0f, 0.5f);
                lr.anchoredPosition = new Vector2(iconX + icon + 6f, 0f);
                lr.sizeDelta = new Vector2(labelW, 0f);
                label.text = text;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.enableAutoSizing = false;       // sama koko molemmille selitteille (Kolhut oli pienempi kuin Lika)
                label.fontSize = h * 0.44f;
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
                StyleHudText(label, Color.white);
            }

            // palkki (taytto) selitteen oikealle puolelle; vasen reuna talletetaan EnsureWidthSetupissa
            var fr = fill.rectTransform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;
            float left = iconX + icon + 6f + labelW + 6f;
            float vIn = Mathf.Max(8f, h * 0.24f);
            fr.offsetMin = new Vector2(left, vIn);
            fr.offsetMax = new Vector2(-20f, -vIn);
            fill.sprite = Juice.BarFillSprite();
            fill.type = Image.Type.Sliced;

            // tumma ura palkin taakse (nakyy tyhjan osan kohdalla)
            if (box.Find("Track") == null)
            {
                var tg = new GameObject("Track", typeof(RectTransform), typeof(Image));
                var tr = (RectTransform)tg.transform;
                tr.SetParent(box, false);
                tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
                tr.offsetMin = fr.offsetMin; tr.offsetMax = fr.offsetMax;
                var ti = tg.GetComponent<Image>();
                ti.sprite = Juice.PillSprite(); ti.type = Image.Type.Sliced;
                ti.color = new Color(0.05f, 0.06f, 0.09f, 0.95f);
                ti.raycastTarget = false;
                tr.SetSiblingIndex(fill.transform.GetSiblingIndex()); // juuri taytön alle
            }
        }

        // Taustalaatta tekstin taakse: aina riittava kontrasti vaalealla taivaalla (WCAG-tavoite >= 4.5:1).
        class PlateInfo { public RectTransform plate; public TMP_Text text; public RectTransform icon; public string lastText; public float nextLayout; }
        readonly System.Collections.Generic.List<PlateInfo> plates = new System.Collections.Generic.List<PlateInfo>();

        void AddPlate(TMP_Text t, Sprite iconSprite = null)
        {
            if (t == null) return;
            foreach (var p in plates) if (p.text == t) return;
            t.enableWordWrapping = false;
            t.overflowMode = TextOverflowModes.Overflow;
            var go = new GameObject(t.name + "_Plate", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(t.transform.parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            go.AddComponent<LayoutElement>().ignoreLayout = true;   // ei sotkeudu Headerin asetteluryhmaan
            var im = go.GetComponent<Image>();
            im.sprite = Juice.PillSprite();
            im.type = Image.Type.Sliced;
            im.color = new Color(0.05f, 0.06f, 0.1f, 0.66f);
            im.raycastTarget = false;
            rt.SetSiblingIndex(t.transform.GetSiblingIndex()); // tekstin alapuolelle
            RectTransform iconRt = null;
            if (iconSprite != null)
            {
                // Ikoni laatan lapsena: kertoo mita luku tarkoittaa (tahti = pisteet, kello = aika)
                var ig = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconRt = (RectTransform)ig.transform;
                iconRt.SetParent(rt, false);
                iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                var ii = ig.GetComponent<Image>();
                ii.sprite = iconSprite; ii.raycastTarget = false; ii.preserveAspect = true;
            }
            plates.Add(new PlateInfo { plate = rt, text = t, icon = iconRt });
        }
    }
}
