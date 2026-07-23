using UnityEngine;

namespace Pakettiporina
{
    // M3.5b: lukee hallissa valitun auton mittarit ja paketin, ja saataa ajon niiden mukaan.
    // Liita peliscenessa tyhjaan objektiin (esim. RaceSetup) ja kytke Car-kentta.
    //
    // Kartoitus:
    //   voima  -> kiihtyvyys (acceleration)
    //   pito   -> pito (grip)
    //   keveys -> huippunopeus (maxSpeed) ja kaantymisnopeus (turnSpeed)
    //   paketin massa -> vaimentaa kiihtyvyytta ja huippunopeutta (raskas = tahmea)
    //
    // Kaikki arvot rajataan turvalliselle valille: heikoinkin auto on ajettava,
    // vahvinkaan ei ole hallitsematon. Tama on tarkeaa lapsipelissa.
    public class RaceSetup : MonoBehaviour
    {
        [Header("Viittaukset")]
        [Tooltip("Auton kontrolleri. Jos tyhja, etsitaan automaattisesti.")]
        public ArcadeCarController car;

        [Header("Kiihtyvyys (voima 0-100)")]
        public float accelMin = 16f;
        public float accelMax = 30f;

        [Header("Pito (pito 0-100)")]
        public float gripMin = 4f;
        public float gripMax = 8f;

        [Header("Huippunopeus (keveys 0-100)")]
        public float speedMin = 10f;
        public float speedMax = 15f;

        [Header("Kaantyminen (keveys 0-100)")]
        public float turnMin = 100f;
        public float turnMax = 145f;

        [Header("Paketin paino")]
        [Tooltip("Massa jolla vaikutus on nolla (kevyt paketti)")]
        public float massLight = 5f;
        [Tooltip("Massa jolla vaikutus on suurin (painava paketti)")]
        public float massHeavy = 25f;
        [Tooltip("Kuinka paljon painavin paketti vaimentaa (0.25 = -25 %)")]
        [Range(0f, 0.5f)] public float massPenalty = 0.25f;

        void Start()
        {
            if (car == null) car = FindObjectOfType<ArcadeCarController>();
            if (car == null)
            {
                Debug.LogWarning("[Setup] Autoa ei loytynyt — ajoa ei saadetty.");
                return;
            }

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.Log("[Setup] Ei GameManageria (ajettiin suoraan peliscenesta) — kaytetaan oletusarvoja.");
                return;
            }

            CarStats s = gm.SelectedStats;

            // Jos hallissa ei ole kayty (kaikki nollia), ei saadeta mitaan.
            if (s.voima == 0 && s.pito == 0 && s.keveys == 0)
            {
                Debug.Log("[Setup] Ei hallivalintaa — kaytetaan oletusarvoja.");
                return;
            }

            // 0-100 -> 0..1
            float fVoima = Mathf.Clamp01(s.voima / 100f);
            float fPito = Mathf.Clamp01(s.pito / 100f);
            float fKeveys = Mathf.Clamp01(s.keveys / 100f);

            // Paketin paino: 0 (kevyt) .. 1 (painava) -> vaimennuskerroin
            float heavy = 0f;
            var pkg = gm.SelectedPackage;
            if (pkg != null)
                heavy = Mathf.Clamp01(Mathf.InverseLerp(massLight, massHeavy, pkg.mass));
            float loadFactor = 1f - heavy * massPenalty;   // esim. 1.0 ... 0.75

            // Saada arvot turvalliselle valille
            car.acceleration = Mathf.Lerp(accelMin, accelMax, fVoima) * loadFactor;
            car.grip = Mathf.Lerp(gripMin, gripMax, fPito);
            car.maxSpeed = Mathf.Lerp(speedMin, speedMax, fKeveys) * loadFactor;
            car.turnSpeed = Mathf.Lerp(turnMin, turnMax, fKeveys);

            string pn = pkg != null ? pkg.displayName : "ei pakettia";
            Debug.Log($"[Setup] Auto saadetty ({s}) | paketti={pn} massa-vaimennus={loadFactor:F2}");
            Debug.Log($"[Setup] -> kiihtyvyys {car.acceleration:F1}, pito {car.grip:F1}, nopeus {car.maxSpeed:F1}, kaanto {car.turnSpeed:F0}");
        }
    }
}