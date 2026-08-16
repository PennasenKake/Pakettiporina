using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

        [Header("Lika-/kolhumittarit (M5.17, 16.8.2026)")]
        [Tooltip("Tayttyva palkki (Image, Image Type = Filled) - lika. Kasvaa Latakko-osumista, pysyy seuraavaan ajoon asti.")]
        public Image dirtMeterBar;
        [Tooltip("Tayttyva palkki (Image, Image Type = Filled) - kolhut. Kasvaa Kartio/Laatikko-osumista.")]
        public Image dentMeterBar;

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

            // M5.17/16.8.2026: live-paivitys lika-/kolhupalkkeihin heti kun osuma
            // tapahtuu ajon aikana, ei vain maalissa. GameManager voi puuttua viela
            // taalla jos scenea testataan suoraan (ei paavalikon kautta) - siksi
            // null-tarkistus, ja arvo paivitetaan kerran heti tilaukseen kun onnistuu.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDirtMeterChanged += HandleDirtMeterChanged;
                GameManager.Instance.OnDentMeterChanged += HandleDentMeterChanged;
                HandleDirtMeterChanged(GameManager.Instance.DirtMeter, GameManager.DIRT_MAX);
                HandleDentMeterChanged(GameManager.Instance.DentMeter, GameManager.DENT_MAX);
            }
        }
        void OnDisable()
        {
            GameEvents.OnRaceStart -= HandleStart;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
            GameEvents.OnCountdown -= HandleCountdown;
            GameEvents.OnGo -= HandleGo;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDirtMeterChanged -= HandleDirtMeterChanged;
                GameManager.Instance.OnDentMeterChanged -= HandleDentMeterChanged;
            }
        }

        void HandleDirtMeterChanged(int current, int max)
        {
            if (dirtMeterBar != null) dirtMeterBar.fillAmount = max > 0 ? (float)current / max : 0f;
        }

        void HandleDentMeterChanged(int current, int max)
        {
            if (dentMeterBar != null) dentMeterBar.fillAmount = max > 0 ? (float)current / max : 0f;
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

            // M5.17/16.8.2026: nayttaa AUTON NYKYISEN, PYSYVAN tilan (ei enaa vain taman
            // yhden ajon osumia) - lika/kolhut kertyvat ajosta toiseen kunnes pelaaja
            // kayy Pesussa, joten teksti kertoo "auton kunto juuri nyt", ei "mita tassa
            // ajossa tapahtui".
            if (damageText != null)
            {
                var gm = GameManager.Instance;
                int dents = gm != null ? gm.DentMeter : 0;
                int dirt = gm != null ? gm.DirtMeter : 0;
                damageText.text = (dents == 0 && dirt == 0)
                    ? "Auto on puhdas ja ehja!"
                    : $"Kolhuja: {dents}/{GameManager.DENT_MAX}  |  Likatahroja: {dirt}/{GameManager.DIRT_MAX}";
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

        // Maalin "Pesuun"-nappi (GameObjectin nimi on "OnGarageButton" jaanteena
        // vanhasta suorasta Halli-reitista - metodin nimea EI vaihdettu koska OnClick
        // on jo kytketty Unity-scenessa taman metodin mukaan, mutta pelaajalle
        // nakyva teksti/ikoni on korjattu PakettiporinaSetup.cs:ssa "Pesuun"-nakoiseksi).
        // M4: vie ENSIN Pesuun (jossa pesu -> korjaus -> Jatkoon-nappi vie vasta Halliin).
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