using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Tauko: pysayttaa pelin, piilottaa ajonapit, vaihtaa napin kuvan pause<->play.
    // Ei salli taukoa lahtolaskennan aikana.
    public class PauseMenu : MonoBehaviour
    {
        [Header("Viittaukset")]
        public GameObject pausePanel;
        [Tooltip("Ajonappien vanhempiobjekti (Controls) — piilotetaan tauolla")]
        public GameObject controls;

        [Header("Tauko/jatka-napin kuva (valinnainen)")]
        public Image toggleIcon;
        public Sprite pauseSprite;   // nakyy kun peli kay
        public Sprite playSprite;    // nakyy kun tauolla

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";

        public bool IsPaused { get; private set; }

        void Start()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            UpdateIcon();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();
        }

        public void Toggle()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            var race = RaceManager.Instance;
            if (race != null && race.IsCountingDown)
            {
                Debug.Log("[Pause] Ei taukoa lahtolaskennan aikana.");
                return;
            }
            if (race != null && !race.IsRacing)
            {
                Debug.Log("[Pause] Ei taukoa (ei ajossa).");
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            if (controls != null) controls.SetActive(false);   // ajonapit piiloon
            UpdateIcon();
            Debug.Log("[Pause] Tauko — ajonapit piilotettu.");
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (controls != null) controls.SetActive(true);    // ajonapit takaisin
            UpdateIcon();
            Debug.Log("[Pause] Jatketaan — ajonapit takaisin.");
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[Pause] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }

        void UpdateIcon()
        {
            if (toggleIcon == null) return;
            if (IsPaused && playSprite != null) toggleIcon.sprite = playSprite;
            else if (!IsPaused && pauseSprite != null) toggleIcon.sprite = pauseSprite;
        }
    }
}