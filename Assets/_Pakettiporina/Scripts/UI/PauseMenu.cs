using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Tauko: pysayttaa ajan, nayttaa tauko-paneelin ja PIILOTTAA ohjausnapit.
    public class PauseMenu : MonoBehaviour
    {
        [Header("Viittaukset")]
        public GameObject pausePanel;
        [Tooltip("Ohjausnappien vanhempi � piilotetaan tauon ajaksi")]
        public GameObject controls;

        [Header("Tauko/jatka-napin kuva (valinnainen)")]
        public Image toggleIcon;
        public Sprite pauseSprite;
        public Sprite playSprite;

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";
        [Tooltip("Valinnainen: jos tauko-paneelissa on Halliin-nappi (esim. Pesu-scenessa)")]
        public string garageScene = "Garage";

        public bool IsPaused { get; private set; }

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            SetIcon(false);
        }

        void Update()
        {
            // Androidin takaisin-nappi = tauko/jatka
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();
        }

        // Kytke tauko-nappiin.
        public void Toggle()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            if (controls != null) controls.SetActive(false);   // napit piiloon
            SetIcon(true);
            Debug.Log("[Pause] Tauko � ajonapit piilotettu.");
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            // Napit takaisin vain jos keikka on kaynnissa (ei maalin jalkeen)
            bool racing = RaceManager.Instance != null && RaceManager.Instance.IsRacing;
            if (controls != null) controls.SetActive(racing);
            SetIcon(false);
            Debug.Log("[Pause] Jatketaan.");
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[Pause] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }

        // Valinnainen "Halliin"-nappi tauko-paneelissa (esim. Pesu-scenessa).
        public void GoToGarage()
        {
            Time.timeScale = 1f;
            Debug.Log("[Pause] Halliin: " + garageScene);
            SceneManager.LoadScene(garageScene);
        }

        void SetIcon(bool paused)
        {
            if (toggleIcon == null) return;
            if (paused && playSprite != null) toggleIcon.sprite = playSprite;
            else if (!paused && pauseSprite != null) toggleIcon.sprite = pauseSprite;
        }
    }
}