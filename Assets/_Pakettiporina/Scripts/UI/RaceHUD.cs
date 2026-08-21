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

        [Header("Lika-/kolhumittarit (M5.17, 16.8.2026; tyylipäivitys 17.8.2026)")]
        [Tooltip("Tayttyva palkki (Image, Image Type = Filled) - lika. Kasvaa Latakko-osumista, pysyy seuraavaan ajoon asti.")]
        public Image dirtMeterBar;
        [Tooltip("Tayttyva palkki (Image, Image Type = Filled) - kolhut. Kasvaa Kartio/Laatikko-osumista.")]
        public Image dentMeterBar;
        [Tooltip("Pieni teksti palkin ylapuolella, esim. 'Lika 3/15' - nakyy aina lukumaarana, ei vain arvauksena tayttoprosentista.")]
        public TMP_Text dirtMeterLabel;
        [Tooltip("Pieni teksti palkin ylapuolella, esim. 'Kolhut 1/6'.")]
        public TMP_Text dentMeterLabel;
        [Header("Osumapulssi (KORJATTU NELJANNEN KERRAN 19.8.2026, kayttaja: 'progress bar ei nayta silta etta palkit muutu esteisiin kun osuu, toteuta tama oikeasti, tarvittaessa eri tavalla')")]
        [Tooltip("EDELLISET YRITYKSET kayttivat Image.fillAmount + Image Type=Filled -maskausta - tama RIIPPUU siita etta Image-komponentin Type/FillMethod on oikein Inspectorissa, eika sen toimivuutta voinut taata luotettavasti pelkalla koodilla. UUSI, TAYSIN ERI TOTEUTUS: palkin RectTransform ankkuroidaan koodissa VASEMPAAN REUNAAN KIINTEAKSI (ei enaa venytetty taytto) ja sen LEVEYS (sizeDelta.x, oikea pikselimaara) asetetaan suoraan - tama on KIRJAIMELLINEN koon muutos RectTransformin tasolla, ei riipu mitenkaan Image-asetuksista, joten sen NAKYMINEN ON TAATTU.")]
        public float hitFeedbackDuration = 0.45f;
        public float hitPunchScale = 1.4f;
        [Tooltip("Vilahdusvari pulssin huipulla (palautuu takaisin alkuperaiseen varin).")]
        public Color hitFlashColor = new Color(1f, 1f, 1f, 1f);

        // Yksi tila-olio per mittari (lika/kolhut) - pitaa kirjaa RectTransformista, sen
        // taydesta (100%) leveydesta pikseleina, alkuperaisesta varista ja kaynnissa
        // olevasta animaatiosta. Taytetaan lazyna EnsureWidthSetup():ssa ensimmaisella
        // kaytolla (silloin Canvas on jo ehtinyt laskea layoutin kerran, joten rt.rect.width
        // antaa oikean "taysi palkki" -leveyden talteen ENNEN kuin ankkurit vaihdetaan).
        class MeterBarState
        {
            public Image bar;
            public RectTransform rt;
            public float fullWidth = -1f;
            public Color baseColor;
            public bool colorCached;
            public Coroutine routine;
        }
        readonly MeterBarState dirtState = new MeterBarState();
        readonly MeterBarState dentState = new MeterBarState();

        void EnsureWidthSetup(MeterBarState st)
        {
            if (st.bar == null || st.rt != null) return; // ei viitetta TAI jo valmis
            st.rt = st.bar.rectTransform;
            // Talleta nykyinen (viela venytetty-taytto) leveys TAYDEKSI leveydeksi ENNEN
            // ankkurien vaihtoa - tama vastaa alkuperaista, Editorissa maaritettya palkin
            // kokoa (esim. tausta-paneelin sisalla, paddingit huomioiden).
            st.fullWidth = Mathf.Max(4f, st.rt.rect.width);
            st.rt.anchorMin = new Vector2(0f, 0f);
            st.rt.anchorMax = new Vector2(0f, 1f); // pystysuunta pysyy venytettyna
            st.rt.pivot = new Vector2(0f, 0.5f);
            var pos = st.rt.anchoredPosition;
            st.rt.anchoredPosition = new Vector2(0f, pos.y);
            // fillAmount/Type eivat ole enaa kaytossa taytto-mekanismina - varmistetaan
            // etta Image nayttaa AINA koko oman (nyt koodilla ohjatun) leveytensa, ei
            // vahingossa maskaa mitaan paalle jos Type sattuisi olemaan Filled.
            st.bar.type = Image.Type.Simple;
            st.bar.fillAmount = 1f;
        }

        static void SetBarWidth(MeterBarState st, float fraction)
        {
            if (st.rt == null) return;
            var sd = st.rt.sizeDelta;
            sd.x = st.fullWidth * Mathf.Clamp01(fraction);
            st.rt.sizeDelta = sd;
        }

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

            dirtState.bar = dirtMeterBar;
            dentState.bar = dentMeterBar;

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

            // KORJATTU 19.8.2026 (kayttaja: "nyt kun saadaan 'kolhu:...' ja 'lika:...'
            // tekstit pois, pelkat selkeat ja paremmat progress barit kertomaan
            // osumista"): lukumaaratekstit piilotetaan - pelkka palkki (+ sen sisalla
            // oleva pieni lika/kolhu-ikoni, jo olemassa ja kayttajan mielesta hyva)
            // riittaa nyt kertomaan tilanteen.
            if (dirtMeterLabel != null) dirtMeterLabel.gameObject.SetActive(false);
            if (dentMeterLabel != null) dentMeterLabel.gameObject.SetActive(false);
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
            float target = max > 0 ? (float)current / max : 0f;
            EnsureWidthSetup(dirtState);
            // Ensimmainen synkronointi (OnEnable, scenen alussa) hypataan suoraan -
            // vain OIKEAT osumat ajon aikana animoituvat.
            if (!dirtSynced)
            {
                SetBarWidth(dirtState, target);
                dirtSynced = true;
            }
            else if (dirtState.rt != null)
            {
                if (dirtState.routine != null) StopCoroutine(dirtState.routine);
                dirtState.routine = StartCoroutine(HitFeedback(dirtState, target));
            }
            // KORJATTU 19.8.2026: "Lika X/Y" -teksti poistettu kayttajan pyynnosta -
            // palkki (koon muutos + pomppu/vilahdus) riittaa nyt yksinaan.
        }

        void HandleDentMeterChanged(int current, int max)
        {
            float target = max > 0 ? (float)current / max : 0f;
            EnsureWidthSetup(dentState);
            if (!dentSynced)
            {
                SetBarWidth(dentState, target);
                dentSynced = true;
            }
            else if (dentState.rt != null)
            {
                if (dentState.routine != null) StopCoroutine(dentState.routine);
                dentState.routine = StartCoroutine(HitFeedback(dentState, target));
            }
            // KORJATTU 19.8.2026: "Kolhut X/Y" -teksti poistettu kayttajan pyynnosta -
            // palkki (koon muutos + pomppu/vilahdus) riittaa nyt yksinaan.
        }
        bool dirtSynced, dentSynced;

        // KORJATTU NELJANNEN KERRAN 19.8.2026 (ks. luokkakommentti hitFeedbackDuration-
        // kentan yllä): palkin LEVEYS (RectTransform.sizeDelta.x - kirjaimellinen koon
        // muutos, ei Image.fillAmount-maskaus) seka pomppu+vilahdus animoidaan yhdessa,
        // KIINTEAKESTOISESSA coroutinessa - AINA taman metodin koko keston (esim. 0.45s)
        // ajan, riippumatta siita kuinka pieni muutos on.
        IEnumerator HitFeedback(MeterBarState st, float targetFraction)
        {
            var bar = st.bar;
            var rt = st.rt;
            if (!st.colorCached) { st.baseColor = bar.color; st.colorCached = true; }
            Color baseColor = st.baseColor;

            Vector3 baseScale = Vector3.one;
            float startWidth = rt.sizeDelta.x;
            float targetWidth = st.fullWidth * Mathf.Clamp01(targetFraction);
            float dur = Mathf.Max(0.05f, hitFeedbackDuration);
            float e = 0f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(e / dur);
                // Koon muutos: ease-out (nopea alku, pehmea loppu) - koko ajan nakyva liike.
                float eased = 1f - (1f - k) * (1f - k);
                var sd = rt.sizeDelta;
                sd.x = Mathf.Lerp(startWidth, targetWidth, eased);
                rt.sizeDelta = sd;
                // Pomppu+vilahdus: nousee puoliväliin, laskee takaisin (huippu keskella).
                float pulse = k < 0.5f ? (k / 0.5f) : (1f - (k - 0.5f) / 0.5f);
                rt.localScale = Vector3.Lerp(baseScale, baseScale * hitPunchScale, pulse);
                bar.color = Color.Lerp(baseColor, hitFlashColor, pulse);
                yield return null;
            }
            var final = rt.sizeDelta;
            final.x = targetWidth;
            rt.sizeDelta = final;
            rt.localScale = baseScale;
            bar.color = baseColor;
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
            // KORJATTU 19.8.2026: lika-/kolhupalkkien tayttyminen animoidaan nyt
            // HitFeedback()-coroutinessa (ks. yllä), ei enaa tassa - tama Update()
            // huolehtii vain aikatekstista.
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
            SceneFader.LoadScene(washScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;
            Debug.Log("[HUD] Paavalikkoon: " + mainMenuScene);
            SceneFader.LoadScene(mainMenuScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }
    }
}