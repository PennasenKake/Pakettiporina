using System.Collections;
using UnityEngine;

namespace Pakettiporina
{
    // Orkesteroi yhden keikan: LAHTOLASKENTA -> ajossa -> maali -> palkkio.
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Viittaukset")]
        public Rigidbody car;
        [Tooltip("Kaytetaan jos TrackManageria ei loydy scenesta (vanha, kiintea rata).")]
        public Transform startPoint;

        [Header("Asetukset")]
        public float fallY = -5f;
        public int countdownSeconds = 3;

        [Header("Palkkio")]
        public int pointsPerStar = 10;

        public bool IsRacing { get; private set; }
        public bool IsCountingDown { get; private set; }
        public int Stars { get; private set; }
        public float Elapsed { get; private set; }

        public int LastReward { get; private set; }
        public int LastStars { get; private set; }
        public bool LastFit { get; private set; }

        // LISATTY 23.11.2026: ajon arvosana 1-3 tahtea (1 = maali, 2 = ehja = ei kolhuja, 3 = myos kaikki bonustahdet / nopea aika).
        public int LastRunStars { get; private set; }
        public int LastRunBestBefore { get; private set; }
        public int LastRunBonusPoints { get; private set; }
        public System.Collections.Generic.List<string> LastUnlocked { get; private set; } = new System.Collections.Generic.List<string>();
        public int BonusTotal { get; private set; }

        void Awake() { Instance = this; }

        // HUOM: EI enaa kuuntele GameEvents.OnFinish — FinishTrigger kutsuu FinishRace() suoraan,
        // jotta palkkio lasketaan ENNEN kuin HUD paivittaa maali-paneelin.
        void Start() { StartRace(); }

        public void StartRace()
        {
            StopAllCoroutines();
            Stars = 0; Elapsed = 0f; IsRacing = false;
            LastRunStars = 0; BonusTotal = 0;
            if (GameManager.Instance != null) GameManager.Instance.ResetRaceDentHits();

            // M5.17/16.8.2026: EI enaa nollata lika-/kolhumittaria ajon alussa - mittari on
            // nyt PYSYVA (kayttaja: "pysyy seuraavaan ajoon asti"), puretaan vain Pesussa.
            // (Aiemmin tassa oli GameManager.Instance.ResetRaceDamage() - poistettu koska
            // metodikin poistui GameManagerista, ks. sen kommentti.)

            // Jos scenessa on TrackManager, arvo rata ja kayta sen StartPointia.
            // Muuten kaytetaan vanhaa, sceneen kiinteasti asetettua startPointia.
            if (TrackManager.Instance != null)
            {
                var sp = TrackManager.Instance.SpawnRandomTrack();
                if (sp != null) startPoint = sp;
                else Debug.LogWarning("[Race] TrackManager loytyi mutta rataa ei saatu - kaytetaan vanhaa startPointia.");
            }

            ResetCar();
            StartCoroutine(CountdownRoutine());
        }

        IEnumerator CountdownRoutine()
        {
            IsCountingDown = true;
            FreezeCar(true);
            yield return null;   // radan esteet/tahdet ehtivat syntya ennen laskentaa
            BonusTotal = FindObjectsOfType<Pickup>().Length + FindObjectsOfType<TrackBubble>().Length;
            GameEvents.RaceStart();
            for (int i = countdownSeconds; i > 0; i--)
            {
                Debug.Log($"[Race] Lahtolaskenta: {i}");
                GameEvents.Countdown(i);
                yield return new WaitForSeconds(1f);
            }
            Debug.Log("[Race] AJA!");
            // KORJATTU 19.8.2026 (kayttaja: "edellisen muutoksen jalkeen auto ei liiku
            // startissa" - koko pelin tarkistuksessa loydetty juurisyy): PELIN YDINTILA
            // (auton vapautus jaadytyksesta + IsRacing=true) asetetaan NYT ENNEN
            // GameEvents.Go() -kutsua, ei enaa jalkeen. Aiemmin jarjestys oli painvastainen:
            // jos JOKIN OnGo-tilaajista (esim. RaceHUD.HandleGo, CarInput.CalibrateTilt)
            // heittaisi poikkeuksen (esim. StartCoroutine epaonnistuu jos GameObject on jo
            // pois paalta), koko CountdownRoutine-coroutine keskeytyisi VALITTOMASTI eika
            // FreezeCar(false)/IsRacing=true koskaan ehtisi suorittua - auto jaisi ikuisesti
            // jaadytettyna ("ei liiku startissa"), vaikka lahtolaskenta nayttaisi paattyneen.
            // Nyt auton fysiikka kaynnistyy AINA, vaikka joku UI-kuuntelija kaatuisikin.
            FreezeCar(false);
            IsCountingDown = false;
            IsRacing = true;
            if (GameManager.Instance != null) GameManager.Instance.SetPhase(GameManager.Phase.Racing);
            GameEvents.Go();
        }

        void Update()
        {
            if (!IsRacing) return;
            Elapsed += Time.deltaTime;
            if (car != null && car.position.y < fallY)
            {
                Debug.Log("[Race] Auto putosi kentalta — palautetaan lahtoon.");
                ResetCar();
            }
        }

        public void AddStar()
        {
            if (!IsRacing) return;
            Stars++;
            Debug.Log($"[Race] Tahti kerätty ({Stars}).");
            GameEvents.StarCollected(Stars);
        }

        // Kutsutaan FinishTriggerista, kun auto osuu maaliin.
        public void FinishRace()
        {
            if (!IsRacing) return;
            IsRacing = false;

            // 1) laske palkkio ENSIN
            var gm = GameManager.Instance;
            var pkg = gm != null ? gm.SelectedPackage : null;

            int packageReward = pkg != null ? pkg.rewardPoints : 0;
            int starReward = Stars * pointsPerStar;

            bool fits = true;
            if (pkg != null && pkg.requiredPart != null && gm != null && gm.SelectedParts != null)
                fits = gm.SelectedParts.Contains(pkg.requiredPart);
            if (!fits) packageReward = packageReward / 2;

            // Arvosana 1-3: maali / ehja / kaikki bonukset. Lisapisteet vain PARANNUKSESTA (ei farmaamista).
            int dents = gm != null ? gm.RaceDentHits : 0;
            int rating = Progress.RateRun(dents, Stars, BonusTotal, Elapsed);
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int bestBefore = gm != null ? gm.BestFor(sceneName) : 0;
            int bonusPts = Mathf.Max(0, rating - Mathf.Max(1, bestBefore)) * Progress.StarPointsBonus;
            LastRunStars = rating; LastRunBestBefore = bestBefore; LastRunBonusPoints = bonusPts;

            int total = packageReward + starReward + bonusPts;
            LastReward = total; LastStars = Stars; LastFit = fits;

            if (gm != null)
            {
                gm.AddPoints(total);
                LastUnlocked = gm.AwardRun(sceneName, rating);
                gm.SetPhase(GameManager.Phase.Finished);
            }

            Debug.Log($"[Race] MAALI! Aika {Elapsed:F1} s | tahdet {Stars} (+{starReward}) | paketti +{packageReward} (sopii={fits}) | yhteensa +{total}");

            // 2) VASTA nyt ilmoita HUDille (LastReward on jo oikein)
            GameEvents.Finish();

            // Play-testi 23.11.2026: auto ajoi maalin lapi ja kamera jai portin sisaan (kirjaimet
            // taytti ruudun) - jarrutetaan auto pehmeasti pysahdyksiin heti maalissa.
            if (car != null) StartCoroutine(BrakeAtFinish());
        }

        System.Collections.IEnumerator BrakeAtFinish()
        {
            float t = 0f;
            const float dur = 0.7f;
            Vector3 v0 = car.velocity;
            while (t < dur && car != null && !car.isKinematic)
            {
                t += Time.deltaTime;
                car.velocity = Vector3.Lerp(v0, Vector3.zero, t / dur);
                car.angularVelocity = Vector3.zero;
                yield return null;
            }
            if (car != null) { car.velocity = Vector3.zero; car.isKinematic = true; }
        }

        void FreezeCar(bool freeze) { if (car != null) car.isKinematic = freeze; }

        void ResetCar()
        {
            if (car == null || startPoint == null)
            {
                Debug.LogWarning("[Race] car tai startPoint puuttuu — aseta ne Inspectorissa!");
                return;
            }
            car.isKinematic = false;
            car.velocity = Vector3.zero;
            car.angularVelocity = Vector3.zero;
            car.position = startPoint.position;
            car.rotation = startPoint.rotation;
        }

        public void Restart() { StartRace(); }
    }
}