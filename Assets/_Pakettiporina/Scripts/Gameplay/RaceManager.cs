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

        void Awake() { Instance = this; }

        // HUOM: EI enaa kuuntele GameEvents.OnFinish — FinishTrigger kutsuu FinishRace() suoraan,
        // jotta palkkio lasketaan ENNEN kuin HUD paivittaa maali-paneelin.
        void Start() { StartRace(); }

        public void StartRace()
        {
            StopAllCoroutines();
            Stars = 0; Elapsed = 0f; IsRacing = false;
            ResetCar();
            StartCoroutine(CountdownRoutine());
        }

        IEnumerator CountdownRoutine()
        {
            IsCountingDown = true;
            FreezeCar(true);
            GameEvents.RaceStart();
            for (int i = countdownSeconds; i > 0; i--)
            {
                Debug.Log($"[Race] Lahtolaskenta: {i}");
                GameEvents.Countdown(i);
                yield return new WaitForSeconds(1f);
            }
            Debug.Log("[Race] AJA!");
            GameEvents.Go();
            FreezeCar(false);
            IsCountingDown = false;
            IsRacing = true;
            if (GameManager.Instance != null) GameManager.Instance.SetPhase(GameManager.Phase.Racing);
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

            int total = packageReward + starReward;
            LastReward = total; LastStars = Stars; LastFit = fits;

            if (gm != null) { gm.AddPoints(total); gm.SetPhase(GameManager.Phase.Finished); }

            Debug.Log($"[Race] MAALI! Aika {Elapsed:F1} s | tahdet {Stars} (+{starReward}) | paketti +{packageReward} (sopii={fits}) | yhteensa +{total}");

            // 2) VASTA nyt ilmoita HUDille (LastReward on jo oikein)
            GameEvents.Finish();
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