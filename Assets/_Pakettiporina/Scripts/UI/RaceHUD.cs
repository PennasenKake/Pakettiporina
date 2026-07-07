using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Ajon HUD: tahtimaara, aika, ajonapit, maali-paneeli, restart ja paluu valikkoon.
    public class RaceHUD : MonoBehaviour
    {
        [Header("Viittaukset")]
        public RaceManager race;
        [Tooltip("Paneeli joka nakyy vasta maalissa (piilota oletuksena)")]
        public GameObject finishPanel;
        [Tooltip("Ajonappien (Vasen/Kaasu/Oikea) vanhempiobjekti — piilotetaan maalissa")]
        public GameObject controls;
        public TMP_Text messageText; // "Maali!"
        public TMP_Text starText;    // "Tahdet: 0"
        public TMP_Text timeText;    // kuluva aika

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";

        void OnEnable()
        {
            GameEvents.OnRaceStart += HandleStart;
            GameEvents.OnStarCollected += HandleStar;
            GameEvents.OnFinish += HandleFinish;
        }

        void OnDisable()
        {
            GameEvents.OnRaceStart -= HandleStart;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
        }

        void HandleStart()
        {
            if (finishPanel != null) finishPanel.SetActive(false);
            if (controls != null) controls.SetActive(true);   // nayta ajonapit
            if (starText != null) starText.text = "Tahdet: 0";
        }

        void HandleStar(int total)
        {
            if (starText != null) starText.text = "Tahdet: " + total;
        }

        void HandleFinish()
        {
            if (finishPanel != null) finishPanel.SetActive(true);
            if (controls != null) controls.SetActive(false);  // piilota ajonapit maalissa
            if (messageText != null) messageText.text = "Maali!";
        }

        void Update()
        {
            if (timeText != null && race != null && race.IsRacing)
                timeText.text = race.Elapsed.ToString("F1") + " s";
        }

        // "Uudestaan"-napin OnClick.
        public void OnRestartButton()
        {
            Time.timeScale = 1f;          // varmuudeksi, jos oli tauolla
            if (race != null) race.Restart();
        }

        // "Paavalikko"-napin OnClick (maali-paneelissa).
        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Palataan paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}