using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // 24.11.2026: WashScreen pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: korjausvaihe, edistymispalkki ja kolhut.
    public partial class WashScreen
    {
        // ---------- Korjausvaihe (kolhut) - SAMA MEKANIIKKA KUIN PESU ----------

        void StartRepairPhase()
        {
            // KORJATTU 16.8.2026 (tarkistuksessa loydetty softlock-riski): tarkistetaan
            // repairedCount >= activeDentCount EIKA vain activeDentCount <= 0 - muuten
            // jos pelaaja painaa "Uudestaan" pesuvaiheen jalkeen kun korjaus oli JO tehty
            // taman Pesu-kaynnin aikana (repairedCount == activeDentCount, kaikki
            // DentSpotit jo pois paalta), tama olisi avannut RepairArean uudestaan
            // vaikka mitaan napautettavaa ei enaa ollut - DonePanel (ja sen sisalla
            // Jatkoon-nappi) ei olisi koskaan naytetty uudelleen -> pelaaja jumissa.
            if (activeDentCount <= 0 || repairedCount >= activeDentCount)
            {
                Debug.Log("[Wash] Ei kolhuja korjattavana - korjausvaihe ohitetaan.");
                ShowDone();
                return;
            }

            repairPhaseActive = true;
            // Play-testi 23.11.2026: saippuakuplat jaivat korjauskuvakkeiden paalle/ylle eika
            // "Puhtaus 100 %" poistunut. Nyt: kuplat ja puhtausteksti piiloon, korjauskuvakkeet
            // isompina ja kaiken paalle.
            StartCoroutine(HideBubblesSoon()); // viive: viimeisen kuplan oma coroutine ehtii kaynnistya
            if (progressText != null) progressText.gameObject.SetActive(false);
            if (dentsText != null) dentsText.gameObject.SetActive(true);
            if (repairArea != null)
            {
                foreach (var tool in repairArea.GetComponentsInChildren<RepairTool>(true))
                {
                    tool.scaleMin = 1.3f;
                    tool.scaleMax = 1.8f;
                    tool.riseSpeedMax = Mathf.Min(tool.riseSpeedMax, 110f); // hitaammin -> helpompi napauttaa
                }
                repairArea.SetAsLastSibling();
                repairArea.gameObject.SetActive(true);
            }
            UpdateDentsText();
            Debug.Log($"[Wash] Korjausvaihe alkoi. Kolhuja: {activeDentCount}. Napauta tyokaluja korjataksesi.");
        }

        // ---- LISATTY 23.11.2026: kolhumerkit autoon + edistymispalkki (Play-testi) ----------------

        // DentSpotit olivat scenessa kaukana autosta (y=-1250), joten kolhut eivat nakyneet autossa.
        // Sijoitetaan merkit auton Imagen paalle (kiintea kuvio auton korin alueelle).
        static readonly Vector2[] DentSlots =
        {
            new Vector2(0.22f, 0.50f), new Vector2(0.78f, 0.52f), new Vector2(0.38f, 0.68f), new Vector2(0.62f, 0.66f),
            new Vector2(0.50f, 0.50f), new Vector2(0.30f, 0.34f), new Vector2(0.70f, 0.36f), new Vector2(0.14f, 0.40f),
            new Vector2(0.86f, 0.42f), new Vector2(0.50f, 0.74f)
        };

        void PlaceDentsOnCar()
        {
            if (carImage == null || dentSpots.Count == 0) return;
            var corners = new Vector3[4];
            carImage.rectTransform.GetWorldCorners(corners); // 0=alavasen, 2=ylaoikea
            // Kolhumerkit piirtyivat auton TAAKSE (DentSpots oli Carin edella hierarkiassa) -> nostetaan auton ja
            // likatahrojen jalkeen.
            if (dentSpotsParent != null)
            {
                int after = carImage.transform.GetSiblingIndex();
                if (dirtSpotsParent != null && dirtSpotsParent.parent == dentSpotsParent.parent)
                    after = Mathf.Max(after, dirtSpotsParent.GetSiblingIndex());
                if (carImage.transform.parent == dentSpotsParent.parent)
                    dentSpotsParent.SetSiblingIndex(Mathf.Min(after + 1, dentSpotsParent.parent.childCount - 1));
                else
                    dentSpotsParent.SetAsLastSibling();
                // kuplaalue ja korjauskuvakkeet pysyvat kolhujen paalla
                if (bubbleArea != null && bubbleArea.parent == dentSpotsParent.parent) bubbleArea.SetAsLastSibling();
                if (repairArea != null && repairArea.parent == dentSpotsParent.parent) repairArea.SetAsLastSibling();
            }
            Vector3 bl = corners[0], tr = corners[2];
            float h = Mathf.Abs(tr.y - bl.y);
            for (int i = 0; i < dentSpots.Count; i++)
            {
                var d = dentSpots[i];
                if (d == null) continue;
                var s = DentSlots[i % DentSlots.Length];
                d.rectTransform.position = new Vector3(Mathf.Lerp(bl.x, tr.x, s.x), Mathf.Lerp(bl.y, tr.y, s.y), bl.z);
                d.rectTransform.sizeDelta = new Vector2(70f, 70f);
                d.raycastTarget = false;
            }
        }

        Text hudLabel;
        Image hudFill;

        void BuildProgressHud()
        {
            var canvas = carImage != null ? carImage.canvas : FindObjectOfType<Canvas>();
            if (canvas == null) return;
            canvas = canvas.rootCanvas;

            var root = new GameObject("ProgressHud", typeof(RectTransform));
            var rt = (RectTransform)root.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            // 24.11.2026: palkki siirretty tauko-napin ALAPUOLELLE (tauko-nappi ja "Korjaus: 1 / 8" menivat paallekkain
            // ylareunassa); tauko-nappi saa olla ylhaalla yksin, palkki ja otsikko omalla rivillaan sen alla.
            rt.anchoredPosition = new Vector2(0f, -250f);
            rt.sizeDelta = new Vector2(640f, 104f);

            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)bg.transform;
            bgRt.SetParent(rt, false);
            bgRt.anchorMin = new Vector2(0f, 0f); bgRt.anchorMax = new Vector2(1f, 0.34f);
            bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = Juice.PillSprite(); bgImg.type = Image.Type.Sliced; bgImg.pixelsPerUnitMultiplier = 0.8f;
            bgImg.color = new Color(0.45f, 0.36f, 0.22f, 0.35f); bgImg.raycastTarget = false;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fRt = (RectTransform)fill.transform;
            fRt.SetParent(bgRt, false);
            fRt.anchorMin = Vector2.zero; fRt.anchorMax = new Vector2(0f, 1f);
            fRt.pivot = new Vector2(0f, 0.5f);
            fRt.offsetMin = fRt.offsetMax = Vector2.zero;
            hudFill = fill.GetComponent<Image>();
            hudFill.sprite = Juice.PillSprite(); hudFill.type = Image.Type.Sliced; hudFill.pixelsPerUnitMultiplier = 0.8f;
            hudFill.color = new Color(0.35f, 0.75f, 0.95f); hudFill.raycastTarget = false;

            var lbl = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var lRt = (RectTransform)lbl.transform;
            lRt.SetParent(rt, false);
            lRt.anchorMin = new Vector2(0f, 0.38f); lRt.anchorMax = new Vector2(1f, 1f);
            lRt.offsetMin = lRt.offsetMax = Vector2.zero;
            hudLabel = lbl.GetComponent<Text>();
            hudLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hudLabel.fontSize = 46;
            hudLabel.fontStyle = FontStyle.Bold;
            hudLabel.alignment = TextAnchor.MiddleCenter;
            hudLabel.color = new Color(0.15f, 0.12f, 0.1f);
            hudLabel.raycastTarget = false;
            hudLabel.text = "";

            // 24.11.2026: kolhuteksti (scenessa y=-280) olisi nyt palkin paalla -> siirretaan palkin alle.
            if (dentsText != null)
            {
                var dr = dentsText.rectTransform;
                dr.anchoredPosition = new Vector2(dr.anchoredPosition.x, -420f);   // palkki ulottuu -354:aan; varmuusvali jos tekstin pivot on keskella
            }
            // Vanha TMP-puhtausteksti korvataan taman palkin otsikolla (ei kahta paallekkaista).
            if (progressText != null) progressText.gameObject.SetActive(false);
            SetHud("Pesu: 0\u00A0%", 0f, new Color(0.35f, 0.75f, 0.95f));
        }

        void SetHud(string label, float fraction, Color color)
        {
            if (hudLabel != null) hudLabel.text = label;
            if (hudFill != null)
            {
                hudFill.color = color;
                var r = hudFill.rectTransform;
                r.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
            }
        }

        IEnumerator HideBubblesSoon()
        {
            yield return new WaitForSeconds(0.35f);
            if (bubbleArea != null) bubbleArea.gameObject.SetActive(false);
        }

        // Kutsutaan RepairTool-skriptista, kun pelaaja napauttaa tyokalua (sama periaate
        // kuin Bubble.cs -> OnBubblePopped, ks. luokkakommentti).
        public void OnRepairToolTapped()
        {
            if (finished || !repairPhaseActive) return;
            RepairNextDent();
        }

        void RepairNextDent()
        {
            // etsi seuraava viela nakyva JA VARAAMATON kolhu-merkki (ks. dentClaimed-kommentti)
            for (int i = 0; i < dentSpots.Count; i++)
            {
                var spot = dentSpots[i];
                if (spot == null || !spot.gameObject.activeSelf) continue;
                if (i < dentClaimed.Length && dentClaimed[i]) continue;
                if (i < dentClaimed.Length) dentClaimed[i] = true;
                Juice.UIBurst(spot.transform as RectTransform, new Color(1f, 0.85f, 0.25f), 12); // korjauksen kipina
                StartCoroutine(FadeOutSpot(spot, dentFadeDuration));
                repairedCount++;
                // M5.17/16.8.2026: puretaan PYSYVAA mittaria heti per napautus, sama
                // periaate kuin CleanNextSpot()/CleanOneDirt yllä.
                GameManager.Instance?.RepairOneDent();
                UpdateDentsText();
                Debug.Log($"[Wash] Kolhu korjattu ({repairedCount}/{activeDentCount}).");
                if (repairedCount >= activeDentCount) FinishRepairAll();
                return;
            }
        }

        void UpdateDentsText()
        {
            // Korjauksen edistyminen nakyviin omaan palkkiin (23.11.2026: "korjaukselle ei ollut seurantaa").
            if (activeDentCount > 0 && repairPhaseActive)
                SetHud($"Korjaus: {repairedCount} / {activeDentCount}", repairedCount / (float)activeDentCount, new Color(0.95f, 0.6f, 0.2f));
            if (dentsText == null) return;
            if (activeDentCount <= 0) { dentsText.text = "Ei kolhuja"; return; }
            if (!repairPhaseActive) { dentsText.text = $"Kolhuja: {activeDentCount}"; return; }
            int pct = Mathf.Clamp(Mathf.RoundToInt(100f * repairedCount / Mathf.Max(1, activeDentCount)), 0, 100);
            dentsText.text = repairedCount == 0 ? "Napauta korjauskuplia!" : $"Korjattu: {pct}\u00A0%";
        }

        // Kaikki kolhut korjattu - palkkio, aani+efekti (kayttaja: "korjaamiseen tulisi
        // liittya jokin aani ja efekti"), sitten Valmis-paneeliin.
        void FinishRepairAll()
        {
            var gm = GameManager.Instance;
            repairReward = pointsPerDentRepair * repairedCount;
            // M5.17/16.8.2026: EI enaa gm.RepairDents() tassa - DentMeter puretaan jo
            // asteittain RepairNextDent()issa (gm.RepairOneDent() per napautus).
            if (gm != null && repairReward > 0)
            {
                gm.AddPoints(repairReward);
            }

            // Aani soi jo joka napautuksella (RepairTool.cs, sama periaate kuin
            // Bubble.cs/bubblePop) - tassa vain visuaalinen "kaikki valmis" -efekti,
            // aivan kuten shineEffect nakyy pesun paatteeksi ilman erillista aanta.
            repairPhaseActive = false;
            if (repairEffect != null) repairEffect.SetActive(true);

            Debug.Log($"[Wash] Kaikki kolhut korjattu! Palkkio +{repairReward}.");
            StartCoroutine(FinishRepairAfterEffect());
        }

        IEnumerator FinishRepairAfterEffect()
        {
            yield return new WaitForSeconds(repairEffectDuration);
            if (repairEffect != null) repairEffect.SetActive(false);
            if (repairArea != null) repairArea.gameObject.SetActive(false);
            ShowDone();
        }
    }
}
