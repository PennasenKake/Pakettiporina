using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // M4: Pesukohtaus maalin ja hallin valissa. Pelaaja napauttaa saippuakuplat
    // rikki; kun kaikki on poksautettu, nayttaa "Valmis"-paneelin ja antaa
    // pienen pistepalkkion. "Jatkoon"-nappi vie halliin.
    public class WashScreen : MonoBehaviour
    {
        public static WashScreen Instance { get; private set; }

        [Header("Kuplat")]
        [Tooltip("Kuplien yhteinen vanhempi. Jos tyhja, etsitaan kaikki Bubble-komponentit scenesta.")]
        public Transform bubbleArea;

        [Header("Edistyminen")]
        [Tooltip("Esim. 'Poksautit 3/8 kuplaa!'")]
        public TMP_Text progressText;

        [Header("Valmis-paneeli")]
        public GameObject donePanel;
        public TMP_Text doneText;

        [Header("Palkkio")]
        [Tooltip("Pistepalkkio kun kaikki kuplat on poksautettu")]
        public int rewardPoints = 5;

        [Header("Scene")]
        public string garageSceneName = "Garage";

        readonly List<Bubble> bubbles = new List<Bubble>();
        int popped;
        bool finished;

        void Awake() { Instance = this; }

        void Start()
        {
            CollectBubbles();
            popped = 0;
            finished = false;
            if (donePanel != null) donePanel.SetActive(false);
            UpdateProgress();

            if (bubbles.Count == 0)
            {
                Debug.LogWarning("[Wash] Kuplia ei loytynyt scenesta � siirrytaan suoraan halliin.");
                Finish();
            }
            else
            {
                Debug.Log($"[Wash] Pesu alkoi. Kuplia: {bubbles.Count}");
            }
        }

        void CollectBubbles()
        {
            bubbles.Clear();
            if (bubbleArea != null)
                bubbles.AddRange(bubbleArea.GetComponentsInChildren<Bubble>(true));
            else
                bubbles.AddRange(FindObjectsOfType<Bubble>(true));
        }

        // Kutsutaan Bubble-skriptista, kun kupla poksahtaa.
        public void OnBubblePopped()
        {
            if (finished) return;
            popped++;
            UpdateProgress();
            Debug.Log($"[Wash] Kupla poksautettu ({popped}/{bubbles.Count}).");
            if (popped >= bubbles.Count) Finish();
        }

        void UpdateProgress()
        {
            if (progressText != null)
                progressText.text = $"Poksautit {popped}/{bubbles.Count} kuplaa!";
        }

        void Finish()
        {
            if (finished) return;
            finished = true;

            if (GameManager.Instance != null) GameManager.Instance.AddPoints(rewardPoints);

            if (donePanel != null) donePanel.SetActive(true);
            if (doneText != null) doneText.text = $"Kiiltavan puhdas auto! Palkkio: +{rewardPoints}";

            Debug.Log($"[Wash] Pesu valmis! Palkkio +{rewardPoints}.");
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
    }
}
