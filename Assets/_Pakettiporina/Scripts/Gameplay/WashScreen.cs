using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // M4: Pesukohtaus maalin ja hallin valissa. Kuplat kelluvat jatkuvasti ja
    // MIELLYTTAVASTI taustalla (ei rangaistusta, ei karkaamista) - jokainen
    // napautus poistaa YHDEN likatahran nakyvista autosta. Kun kaikki likatahrat
    // on poistettu, pesu on valmis ja antaa palkkion.
    public class WashScreen : MonoBehaviour
    {
        public static WashScreen Instance { get; private set; }

        [Header("Kuplat (koristeellisia, ei vaikuta suoraan puhtauteen)")]
        [Tooltip("Kuplien yhteinen vanhempi. Valinnainen, kaytetaan lahinna Editor-tarkistukseen.")]
        public Transform bubbleArea;

        [Header("Likatahrat autossa")]
        [Tooltip("Nakyvat likatahra-kuvat autossa. Jos tyhja ja Dirt Spots Parent on asetettu, kerataan sen lapsista.")]
        public List<Image> dirtSpots = new List<Image>();
        [Tooltip("Valinnainen: vanhempi jonka Image-lapset kerataan automaattisesti Dirt Spots -listaan Startissa.")]
        public Transform dirtSpotsParent;
        [Tooltip("Likatahran haviamisanimaation kesto")]
        public float dirtFadeDuration = 0.25f;

        [Header("Edistyminen")]
        [Tooltip("Esim. 'Puhtaus: 40 %'")]
        public TMP_Text progressText;
        [Tooltip("Valinnainen mittari (Image, Image Type = Filled), tayttyy puhtauden mukaan")]
        public Image cleanBar;

        [Header("Kiiltoefekti (valinnainen)")]
        [Tooltip("Nakyviin kun auto on taysin puhdas, esim. tahtia/kimalletta autossa")]
        public GameObject shineEffect;

        [Header("Valmis-paneeli")]
        public GameObject donePanel;
        public TMP_Text doneText;

        [Header("Palkkio")]
        [Tooltip("Pistetta jokaisesta poistetusta likatahrasta")]
        public int pointsPerDirt = 5;

        [Header("Scene")]
        public string garageSceneName = "Garage";
        public string mainMenuScene = "MainMenu";

        int cleanedCount;
        bool finished;

        void Awake() { Instance = this; }

        void Start()
        {
            if (dirtSpots.Count == 0 && dirtSpotsParent != null)
                dirtSpots.AddRange(dirtSpotsParent.GetComponentsInChildren<Image>(true));

            foreach (var d in dirtSpots)
                if (d != null) { d.gameObject.SetActive(true); SetAlpha(d, 1f); }

            if (shineEffect != null) shineEffect.SetActive(false);
            if (donePanel != null) donePanel.SetActive(false);

            cleanedCount = 0;
            finished = false;
            UpdateProgress();

            if (dirtSpots.Count == 0)
            {
                Debug.LogWarning("[Wash] Likatahroja ei loytynyt scenesta � siirrytaan suoraan halliin.");
                Finish();
            }
            else
            {
                Debug.Log($"[Wash] Pesu alkoi. Likatahroja: {dirtSpots.Count}");
            }
        }

        // Kutsutaan Bubble-skriptista, kun pelaaja napauttaa kuplan.
        public void OnBubblePopped()
        {
            if (finished) return;
            CleanNextSpot();
        }

        void CleanNextSpot()
        {
            // etsi seuraava viela nakyva likatahra
            for (int i = 0; i < dirtSpots.Count; i++)
            {
                var spot = dirtSpots[i];
                if (spot == null || !spot.gameObject.activeSelf) continue;
                StartCoroutine(FadeOutSpot(spot));
                cleanedCount++;
                UpdateProgress();
                Debug.Log($"[Wash] Likatahra puhdistettu ({cleanedCount}/{dirtSpots.Count}).");
                if (cleanedCount >= dirtSpots.Count) Finish();
                return;
            }
        }

        IEnumerator FadeOutSpot(Image spot)
        {
            float t = 0f;
            while (t < dirtFadeDuration)
            {
                t += Time.deltaTime;
                SetAlpha(spot, Mathf.Lerp(1f, 0f, t / dirtFadeDuration));
                yield return null;
            }
            SetAlpha(spot, 0f);
            spot.gameObject.SetActive(false);
        }

        static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }

        void UpdateProgress()
        {
            int total = Mathf.Max(1, dirtSpots.Count);
            int pct = Mathf.RoundToInt(100f * cleanedCount / total);
            if (progressText != null) progressText.text = $"Puhtaus: {pct} %";
            if (cleanBar != null) cleanBar.fillAmount = cleanedCount / (float)total;
        }

        void Finish()
        {
            if (finished) return;
            finished = true;

            int total = pointsPerDirt * Mathf.Max(cleanedCount, dirtSpots.Count > 0 ? 0 : 1);
            if (GameManager.Instance != null) GameManager.Instance.AddPoints(total);

            if (shineEffect != null) shineEffect.SetActive(true);
            if (donePanel != null) donePanel.SetActive(true);
            if (doneText != null) doneText.text = $"Kiiltavan puhdas auto! Palkkio: +{total}";

            Debug.Log($"[Wash] Pesu valmis! Palkkio +{total}.");
        }

        // "Jatkoon"-napin OnClick.
        public void OnContinueButton()
        {
            Debug.Log("[Wash] Jatketaan halliin: " + garageSceneName);
            SceneManager.LoadScene(garageSceneName);
        }

        // Valinnainen "Ohita pesu" -nappi (esim. testausta varten).
        public void OnSkipButton()
        {
            Debug.Log("[Wash] Pesu ohitettu.");
            SceneManager.LoadScene(garageSceneName);
        }

        // "Koti"-nappi (sama kuva/toiminto kuin Ajon maali-paneelissa) -> paavalikkoon.
        public void OnMainMenuButton()
        {
            Debug.Log("[Wash] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }

        // "Uudestaan"-nappi: kaynnistaa PESUN uudestaan (likatahrat palaavat nakyviin).
        public void OnRestartButton()
        {
            Debug.Log("[Wash] Pesu aloitetaan uudestaan.");
            RestartWash();
        }

        void RestartWash()
        {
            finished = false;
            cleanedCount = 0;
            if (donePanel != null) donePanel.SetActive(false);
            if (shineEffect != null) shineEffect.SetActive(false);
            foreach (var d in dirtSpots)
            {
                if (d == null) continue;
                d.gameObject.SetActive(true);
                SetAlpha(d, 1f);
            }
            UpdateProgress();
            Debug.Log($"[Wash] Pesu nollattu. Likatahroja: {dirtSpots.Count}");
        }
    }
}
