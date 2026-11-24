using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // Ajon HUD: tahdet, aika, lahtolaskenta, ajonapit, maali-paneeli (palkkio) ja paluu halliin.
    public partial class RaceHUD : MonoBehaviour
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
            float leftInset = st.rt.offsetMin.x;   // 23.11.2026: vasen reunus sailyy (selite/ikoni palkin vasemmalla)
            st.rt.anchorMin = new Vector2(0f, 0f);
            st.rt.anchorMax = new Vector2(0f, 1f); // pystysuunta pysyy venytettyna
            st.rt.pivot = new Vector2(0f, 0.5f);
            var pos = st.rt.anchoredPosition;
            st.rt.anchoredPosition = new Vector2(leftInset, pos.y);
            // fillAmount/Type eivat ole enaa kaytossa taytto-mekanismina - varmistetaan
            // etta Image nayttaa AINA koko oman (nyt koodilla ohjatun) leveytensa, ei
            // vahingossa maskaa mitaan paalle jos Type sattuisi olemaan Filled.
            // 23.11.2026: kiiltava 9-slice-sprite sailyy sliced-tyyppisena (ei venyta kulmia)
            st.bar.type = (st.bar.sprite != null && st.bar.sprite.border != Vector4.zero) ? Image.Type.Sliced : Image.Type.Simple;
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
            }
            // 23.11.2026: alkusynkronointi viivastetaan yhteen kuvaan, jotta mittarien asettelu (ArrangeMeters)
            // ehtii kayttaa Canvasin oikeaa kokoa ennen kuin palkin taysi leveys talletetaan.
            StartCoroutine(InitMeters());
            if (starText != null) AddPlate(starText, Juice.StarIcon());
            if (timeText != null) AddPlate(timeText, Juice.ClockIcon());

            // KORJATTU 19.8.2026 (kayttaja: "nyt kun saadaan 'kolhu:...' ja 'lika:...'
            // tekstit pois, pelkat selkeat ja paremmat progress barit kertomaan
            // osumista"): lukumaaratekstit piilotetaan - pelkka palkki (+ sen sisalla
            // oleva pieni lika/kolhu-ikoni, jo olemassa ja kayttajan mielesta hyva)
            // riittaa nyt kertomaan tilanteen.
            // (23.11.2026: selitteet ovat nyt pysyvasti nakyvissa, ks. ArrangeMeters)

            // LISATTY 23.11.2026 (Play-testi: Tahdet/aika-tekstit tummaa harmaata taivasta ja
            // majakkaa vasten, huono luettavuus; lahtolaskenta magenta): valkoinen teksti + tumma ulkoreuna.
            StyleHudText(starText, Color.white);
            StyleHudText(timeText, Color.white);
            StyleHudText(countdownText, new Color(1f, 0.85f, 0.1f));
            StyleHudText(messageText, new Color(1f, 0.85f, 0.1f));   // Perilla!-teksti (oli magenta, hukkui ruutulippukuvioon)
            StyleHudText(rewardText, Color.white);
            // Maali-paneelin KAIKKI tekstit (kentat voivat osoittaa muualle kuin nakyviin tekstiobjekteihin).
            if (finishPanel != null)
            {
                bool first = true;
                foreach (var tt in finishPanel.GetComponentsInChildren<TMP_Text>(true))
                {
                    StyleHudText(tt, first ? new Color(1f, 0.85f, 0.1f) : Color.white);
                    first = false;
                }
            }

            // Maali-paneelille tumma himmennys taustalle (Play-testi: teksti ja napit kelluivat
            // suoraan ruutulippukuvion paalla). Luodaan kerran, paneelin alimmaiseksi lapseksi.
            if (finishPanel != null && finishPanel.transform.Find("Scrim") == null)
            {
                var go = new GameObject("Scrim", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(finishPanel.transform, false);
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(-3000f, -3000f); rt.offsetMax = new Vector2(3000f, 3000f);
                var im = go.GetComponent<Image>();
                // 24.11.2026: UMPINAINEN tumma vihrea tausta (rata ja HUD taustalla nayttivat sekavalta, kayttajan kuvakaappaus).
                im.color = new Color(0.04f, 0.2f, 0.19f, 1f);
                im.raycastTarget = false;
                rt.SetAsFirstSibling();
            }
        }

        static void StyleHudText(TMP_Text t, Color c)
        {
            if (t == null || t.font == null) return;
            t.enableVertexGradient = false;
            t.color = c;
            t.fontStyle |= FontStyles.Bold;
            try
            {
                t.outlineWidth = 0.4f;
                t.outlineColor = new Color32(25, 25, 25, 255);
            }
            catch (System.NullReferenceException) { /* TMP-materiaali ei viela valmis - reunus ohitetaan */ }
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

        // --- siirretty tiedostoon RaceHUD.Meters.cs (lika/kolhumittarit ja HUD-asettelu) ---

        void LateUpdate()
        {
            foreach (var p in plates)
            {
                if (p.plate == null || p.text == null) continue;
                bool show = p.text.gameObject.activeInHierarchy && !string.IsNullOrEmpty(p.text.text);
                if (p.plate.gameObject.activeSelf != show) p.plate.gameObject.SetActive(show);
                if (!show) continue;
                // SUORITUSKYKY 23.11.2026: laatan koko/paikka lasketaan vain kun teksti muuttuu tai korkeintaan 2 kertaa/s
                // (aiemmin joka ruutu -> sizeDelta-muutos likasi Canvasin joka ruutu = turhaa CPU- ja akkukuormaa).
                bool changed = p.lastText != p.text.text;
                if (!changed && Time.unscaledTime < p.nextLayout) continue;
                p.nextLayout = Time.unscaledTime + 0.5f;
                // Kevennys: mesh-paivitys vain kun teksti muuttui (aiemmin joka kuva -> raskas, editori hidastui)
                if (changed) { p.text.ForceMeshUpdate(); p.lastText = p.text.text; }
                var b = p.text.textBounds;
                float padX = Mathf.Max(14f, b.size.y * 0.5f), padY = Mathf.Max(6f, b.size.y * 0.25f);
                float iconSize = p.icon != null ? b.size.y * 1.15f : 0f;
                float iconSpace = p.icon != null ? iconSize + padX * 0.4f : 0f;
                // laatta kasvaa vasemmalle ikonin verran; teksti pysyy paikallaan
                p.plate.position = p.text.transform.TransformPoint(b.center - new Vector3(iconSpace * 0.5f, 0f, 0f));
                float pw = b.size.x + padX * 2f + iconSpace;
                p.plate.sizeDelta = new Vector2(pw, b.size.y + padY * 2f + (p.icon != null ? b.size.y * 0.15f : 0f));
                if (p.icon != null)
                {
                    p.icon.sizeDelta = new Vector2(iconSize, iconSize);
                    p.icon.anchoredPosition = new Vector2(-pw * 0.5f + padX * 0.7f + iconSize * 0.5f, 0f);
                }
            }
        }

        // LISATTY 23.11.2026 (Play-testi: lika-/kolhupalkit ilman selitetta). Numerot pidetaan pois (kayttajan
        // toive 19.8.), mutta ajon alussa naytetaan 5 s sanaselitteet "Lika" ja "Kolhut".
        System.Collections.IEnumerator ShowMeterHints()
        {
            var pairs = new[] { (dirtMeterLabel, "Lika"), (dentMeterLabel, "Kolhut") };
            foreach (var (label, text) in pairs)
            {
                if (label == null) continue;
                label.text = text;
                label.gameObject.SetActive(true);
                StyleHudText(label, Color.white);
                label.enableAutoSizing = false;
                label.fontSize = 34;                       // oletusfontti oli ~5 px ruudulla
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
            }
            // PAIVITETTY 23.11.2026 (kayttaja: "selkeammat ja paremmat"): selitteet jäävät pysyvästi näkyviin.
            yield break;
        }

        // Palkin vari kertoo vakavuuden: matala = vaalea/keltainen, korkea = punainen/tumma (hyva -> paha).
        static readonly Color DirtLow = new Color(0.85f, 0.7f, 0.4f), DirtHigh = new Color(0.35f, 0.2f, 0.08f);
        static readonly Color DentLow = new Color(1f, 0.85f, 0.2f), DentHigh = new Color(0.9f, 0.12f, 0.1f);

        void ApplyLevelColor(MeterBarState st, float fraction, bool isDirt)
        {
            if (st.bar == null) return;
            var c = isDirt ? Color.Lerp(DirtLow, DirtHigh, fraction) : Color.Lerp(DentLow, DentHigh, fraction);
            c.a = 1f;
            st.baseColor = c; st.colorCached = true;
            st.bar.color = c;
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
            if (starText != null)
            {
                starText.text = total.ToString();
                Juice.Pulse(starText.transform, 1.4f, 0.3f);
                Juice.FloatText(starText.rectTransform, "+1", new Color(1f, 0.9f, 0.2f));
            }
        }

        void HandleFinish()
        {
            // Maalin konfetti: UI-tasossa (maailman partikkelit jaivat maaliportin taakse, kayttajan kuvakaappaus 23.11.)
            if (finishPanel != null)
            {
                var fr = finishPanel.transform as RectTransform;
                Juice.UIConfetti(fr, 80);          // sade ylhaalta
                Juice.UIConfettiCannons(fr, 45);   // tykit alakulmista
            }
            Juice.Flash(Color.white, 0.45f, 0.35f);
            Juice.Play("confetti", 0f);
            if (finishPanel != null) finishPanel.SetActive(true);
            if (controls != null) controls.SetActive(false);
            // 24.11.2026: Lika/Kolhut-palkit piiloon maalissa (jaivat tahtinaytön paalle, kayttajan palaute).
            if (dirtMeterBar != null && dirtMeterBar.transform.parent != null) dirtMeterBar.transform.parent.gameObject.SetActive(false);
            if (dentMeterBar != null && dentMeterBar.transform.parent != null) dentMeterBar.transform.parent.gameObject.SetActive(false);
            if (messageText != null) messageText.text = race != null && race.LastFit ? "Hienoa, perillä!" : "Perillä!";
            if (rewardText != null && race != null)
                rewardText.text = "Palkkio: +" + race.LastReward + " kolikkoa";
            if (finishPanel != null && race != null)
                RunStarsView.Show(finishPanel.transform as RectTransform, race.LastRunStars,
                    RunStarsView.HintFor(race.LastRunStars, race.BonusTotal), race.LastUnlocked);
            // Tyylit uudelleen heti paneelin nayttohetkella (OnEnable-tyyli ei pysynyt teksteissa).
            if (finishPanel != null)
            {
                bool firstTxt = true;
                foreach (var tt in finishPanel.GetComponentsInChildren<TMP_Text>(true))
                {
                    StyleHudText(tt, firstTxt ? new Color(1f, 0.85f, 0.1f) : Color.white);
                    tt.ForceMeshUpdate();
                    firstTxt = false;
                }
            }
            Debug.Log("[HUD] Maali-paneelin tekstit tyylitetty (" + (finishPanel != null ? finishPanel.GetComponentsInChildren<TMP_Text>(true).Length : 0) + ").");

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