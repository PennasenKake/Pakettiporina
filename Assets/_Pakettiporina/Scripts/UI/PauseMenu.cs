using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Tauko: pysayttaa pelin ja nayttaa taukopaneelin.
    // Esc (kone) tai Android-takaisinnappi avaa/sulkee tauon.
    public class PauseMenu : MonoBehaviour
    {
        [Header("Viittaukset")]
        [Tooltip("Taukopaneeli (piilota oletuksena)")]
        public GameObject pausePanel;

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";

        bool paused;

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f; // varmista normaali nopeus scenen alkaessa
        }

        void Update()
        {
            // Esc kannella, Android-takaisinnappi puhelimessa -> avaa/sulkee tauon.
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
        }

        public void TogglePause()
        {
            if (paused) Resume(); else Pause();
        }

        // "Tauko"-napin OnClick.
        public void Pause()
        {
            paused = true;
            Time.timeScale = 0f; // pysayttaa fysiikan ja liikkeen (Update jatkaa, UI toimii)
            if (pausePanel != null) pausePanel.SetActive(true);
            Debug.Log("[Pause] Peli tauolla.");
        }

        // "Jatka"-napin OnClick.
        public void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            Debug.Log("[Pause] Peli jatkuu.");
        }

        // "Paavalikko"-napin OnClick (taukopaneelissa).
        public void GoToMainMenu()
        {
            Time.timeScale = 1f; // TARKEAA: nollaa ennen scenen vaihtoa
            Debug.Log("[Pause] Palataan paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
