using UnityEngine;
using TMPro;
using System.Collections;

namespace Pakettiporina
{
    // Vilkku on klikattava: nayttaa puhekuplan, joka reagoi pelin tilaan (sopivuus).
    // Liita Vilkku-objektiin, jossa on Button. Kytke Vilkun Button.OnClick -> OnVilkkuClick.
    public class VilkkuController : MonoBehaviour
    {
        [Header("Puhekupla")]
        public GameObject bubble;     // puhekupla-paneeli (piilota oletuksena)
        public TMP_Text bubbleText;

        [Header("Tila")]
        [Tooltip("GarageScreen, jolta Vilkku lukee sopivuustilan (CurrentFit) suoraan - " +
                 "EI paattele tilaa mistaan nakyvasta UI-tekstista, jotta sanamuodon " +
                 "myohempi muuttaminen ei hiljaa riko Vilkun logiikkaa.")]
        public GarageScreen garage;

        [Header("Yleisvinkit (kun ei keikkatietoa)")]
        [TextArea]
        public string[] tips = {
            "Rakenna auto ja aja keikka!",
            "Kokeile eri osia — mittarit muuttuvat!",
            "Valitse ensin keikka, sitten sopiva auto."
        };
        [Tooltip("Kupla piiloutuu itsestaan taman ajan jalkeen (0 = ei piiloudu)")]
        public float autoHideSeconds = 4f;

        int tipIndex = 0;
        Coroutine hideRoutine;

        void Start()
        {
            if (bubble != null) bubble.SetActive(false);
        }

        // Kytke Vilkun napin OnClickiin.
        public void OnVilkkuClick()
        {
            if (bubble == null) return;
            if (bubble.activeSelf) { Hide(); return; }

            if (bubbleText != null) bubbleText.text = BuildLine();
            bubble.SetActive(true);
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            if (autoHideSeconds > 0) hideRoutine = StartCoroutine(HideAfter());
            Debug.Log("[Vilkku] " + (bubbleText != null ? bubbleText.text : ""));
        }

        string BuildLine()
        {
            if (garage != null)
            {
                var fit = garage.CurrentFit;
                bool hasPackage = fit.message != "Ei valittua pakettia";
                if (fit.fits && hasPackage) return "Hienoa! Auto sopii tähän keikkaan.";
                if (!fit.fits) return "Vinkki: " + fit.message + ". Vaihda oikea osa!";
            }
            if (tips.Length == 0) return "Hei, olen Vilkku!";
            string t = tips[tipIndex % tips.Length];
            tipIndex++;
            return t;
        }

        void Hide() { if (bubble != null) bubble.SetActive(false); }
        IEnumerator HideAfter() { yield return new WaitForSeconds(autoHideSeconds); Hide(); }
    }
}