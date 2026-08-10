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
        [Tooltip("M5: Valinnainen. Nayttaa esim. 'Kolhuja: 2, likatahroja: 3' - korjataan Pesussa/Hallissa.")]
        public TMP_Text damageText;

        [Header("Lahtolaskenta")]
        public TMP_Text countdownText;
        public float goVisibleSeconds = 0.7f;

        [Header("Scenet")]
        [Tooltip("M4: 'Takaisin halliin' -nappi vie ENSIN tahan pesusceneen, joka jatkaa itse halliin.")]
        public string washScene = "Wash";
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
            lastShownTime = -1f;
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

            // M5: kertoo heti maalissa mita korjattavaa autossa on (kolhut Hallissa,
            // lika Pesussa) - ei rangaistus, vain tieto seuraavaa vaihetta varten.
            if (damageText != null)
            {
                var gm = GameManager.Instance;
                int dents = gm != null ? gm.RaceDents : 0;
                int dirt = gm != null ? gm.RaceDirtHits : 0;
                damageText.text = (dents == 0 && dirt == 0)
                    ? "Auto pysyi puhtaana ja ehjana!"
                    : $"Kolhuja: {dents}  |  Likatahroja: {dirt}";
            }
        }

        float lastShownTime = -1f;

        // Paivittaa aikatekstin vain kun nakyva lukema (0.1 s tarkkuudella) oikeasti
        // muuttuu, ei joka ruutu - .ToString() joka Update-kutsulla allokoisi roskaa
        // ihan turhaan silloinkin kun ruudulla nakyva luku ei muuttuisi ollenkaan.
        void Update()
        {
            if (timeText == null || race == null || !race.IsRacing) return;

            float rounded = Mathf.Floor(race.Elapsed * 10f) / 10f;
            if (rounded == lastShownTime) return;
            lastShownTime = rounded;
            timeText.text = rounded.ToString("F1") + " s";
        }

        // --- napit ---
        public void OnRestartButton()
        {
            Time.timeScale = 1f;
            if (race != null) race.Restart();
        }

        // "Takaisin halliin" — M4: sulkee silmukan PESUN kautta, auto sailyy.
        // (Nappia ei tarvitse kytkea uudelleen Unityssa — sama OnClick, uusi kohde.)
        public void OnGarageButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Maalista pesuun: " + washScene);
            SceneManager.LoadScene(washScene);
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}