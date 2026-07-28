using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Ajon HUD: tahdet, aika, lahtolaskenta, ajonapit, maali-paneeli (palkkio) ja paluu halliin.
    public class RaceHUD : MonoBehaviour
    {
        [Header("Viittaukset")]
        public RaceManager race;
        public GameObject finishPanel;
        public GameObject controls;
        public TMP_Text messageText;
        public TMP_Text starText;
        public TMP_Text timeText;

        [Header("Maali-paneeli")]
        [Tooltip("Nayttaa keikan palkkion, esim. 'Palkkio: +40'")]
        public TMP_Text rewardText;

        [Header("Lahtolaskenta")]
        public TMP_Text countdownText;
        public float goVisibleSeconds = 0.7f;

        [Header("Scenet")]
        public string garageScene = "Garage";
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
            if (controls != null) controls.SetActive(false);
            if (starText != null) starText.text = "Tahdet: 0";
            if (countdownText != null) { countdownText.gameObject.SetActive(true); countdownText.text = ""; }
        }

        void HandleCountdown(int n)
        {
            if (countdownText != null) countdownText.text = n.ToString();
        }

        void HandleGo()
        {
            if (countdownText != null) countdownText.text = "AJA!";
            if (controls != null) controls.SetActive(true);
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
            if (messageText != null) messageText.text = race != null && race.LastFit ? "Hienoa, perillä!" : "Perillä!";
            if (rewardText != null && race != null)
                rewardText.text = "Palkkio: +" + race.LastReward;
        }

        void Update()
        {
            if (timeText != null && race != null && race.IsRacing)
                timeText.text = race.Elapsed.ToString("F1") + " s";
        }

        // --- napit ---
        public void OnRestartButton()
        {
            Time.timeScale = 1f;
            if (race != null) race.Restart();
        }

        // "Takaisin halliin" — sulkee silmukan, auto sailyy.
        public void OnGarageButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Takaisin halliin: " + garageScene);
            SceneManager.LoadScene(garageScene);
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}