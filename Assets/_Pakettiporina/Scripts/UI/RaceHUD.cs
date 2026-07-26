using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Ajon HUD: tahdet, aika, LAHTOLASKENTA, ajonapit, maali-paneeli ja paluu valikkoon.
    public class RaceHUD : MonoBehaviour
    {
        [Header("Viittaukset")]
        public RaceManager race;
        public GameObject finishPanel;
        [Tooltip("Ajonappien vanhempiobjekti — piilotetaan lahtolaskennan ja maalin ajaksi")]
        public GameObject controls;
        public TMP_Text messageText;
        public TMP_Text starText;
        public TMP_Text timeText;

        [Header("Lahtolaskenta")]
        [Tooltip("Iso teksti keskella: 3, 2, 1, AJA!")]
        public TMP_Text countdownText;
        [Tooltip("Kuinka kauan AJA! nakyy sekunteina")]
        public float goVisibleSeconds = 0.7f;

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";

        void OnEnable()
        {
            GameEvents.OnRaceStart += HandleStart;
            GameEvents.OnStarCollected += HandleStar;
            GameEvents.OnFinish += HandleFinish;
            GameEvents.OnCountdown += HandleCountdown;
            GameEvents.OnGo += HandleGo;
        }

        void OnDisable()
        {
            GameEvents.OnRaceStart -= HandleStart;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
            GameEvents.OnCountdown -= HandleCountdown;
            GameEvents.OnGo -= HandleGo;
        }

        void HandleStart()
        {
            if (finishPanel != null) finishPanel.SetActive(false);
            if (controls != null) controls.SetActive(false);   // napit piiloon laskennan ajaksi
            if (starText != null) starText.text = "Tahdet: 0";
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = "";
            }
        }

        void HandleCountdown(int n)
        {
            if (countdownText != null) countdownText.text = n.ToString();
        }

        void HandleGo()
        {
            if (countdownText != null) countdownText.text = "AJA!";
            if (controls != null) controls.SetActive(true);    // napit kayttoon
            StopAllCoroutines();
            StartCoroutine(HideCountdown());
        }

        IEnumerator HideCountdown()
        {
            yield return new WaitForSeconds(goVisibleSeconds);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }

        void HandleStar(int total)
        {
            if (starText != null) starText.text = "Tahdet: " + total;
        }

        void HandleFinish()
        {
            if (finishPanel != null) finishPanel.SetActive(true);
            if (controls != null) controls.SetActive(false);
            if (messageText != null) messageText.text = "Maali!";
        }

        void Update()
        {
            if (timeText != null && race != null && race.IsRacing)
                timeText.text = race.Elapsed.ToString("F1") + " s";
        }

        public void OnRestartButton()
        {
            Time.timeScale = 1f;
            if (race != null) race.Restart();
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Palataan paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}