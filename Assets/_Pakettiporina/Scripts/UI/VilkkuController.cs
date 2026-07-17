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
        [Tooltip("Sama FitText jonka GarageScreen paivittaa — Vilkku lukee siita tilan")]
        public TMP_Text fitText;

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
            string f = fitText != null ? fitText.text : "";
            if (f.Contains("Sopii")) return "Hienoa! Auto sopii tähän keikkaan.";
            if (f.Contains("Tarvitsee")) return "Vinkki: " + f + ". Vaihda oikea osa!";
            if (tips.Length == 0) return "Hei, olen Vilkku!";
            string t = tips[tipIndex % tips.Length];
            tipIndex++;
            return t;
        }

        void Hide() { if (bubble != null) bubble.SetActive(false); }
        IEnumerator HideAfter() { yield return new WaitForSeconds(autoHideSeconds); Hide(); }
    }
}