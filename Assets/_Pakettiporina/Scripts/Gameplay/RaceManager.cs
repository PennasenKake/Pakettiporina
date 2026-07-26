using System.Collections;
using UnityEngine;

namespace Pakettiporina
{
    // Orkesteroi yhden keikan: LAHTOLASKENTA -> ajossa -> maali.
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Viittaukset")]
        [Tooltip("Auton Rigidbody")] public Rigidbody car;
        [Tooltip("Tyhja objekti, johon auto asetetaan lahdossa")] public Transform startPoint;

        [Header("Asetukset")]
        [Tooltip("Jos auto putoaa taman Y-korkeuden alle, se palautetaan lahtoon")]
        public float fallY = -5f;
        [Tooltip("Lahtolaskennan pituus sekunteina (3 = 3,2,1 AJA!)")]
        public int countdownSeconds = 3;

        public bool IsRacing { get; private set; }
        public bool IsCountingDown { get; private set; }
        public int Stars { get; private set; }
        public float Elapsed { get; private set; }

        void Awake() { Instance = this; }

        void OnEnable() { GameEvents.OnFinish += HandleFinish; }
        void OnDisable() { GameEvents.OnFinish -= HandleFinish; }

        void Start() { StartRace(); }

        public void StartRace()
        {
            StopAllCoroutines();
            Stars = 0;
            Elapsed = 0f;
            IsRacing = false;
            ResetCar();
            StartCoroutine(CountdownRoutine());
        }

        IEnumerator CountdownRoutine()
        {
            IsCountingDown = true;
            FreezeCar(true);                 // auto pysyy paikallaan
            GameEvents.RaceStart();          // HUD nollaa mittarit ja piilottaa paneelit

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

        void HandleFinish()
        {
            if (!IsRacing) return;
            IsRacing = false;
            if (GameManager.Instance != null) GameManager.Instance.SetPhase(GameManager.Phase.Finished);
            Debug.Log($"[Race] MAALI! Aika {Elapsed:F1} s, tahdet {Stars}.");
        }

        void FreezeCar(bool freeze)
        {
            if (car == null) return;
            car.isKinematic = freeze;
        }

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

        // Kytke tama "Uudestaan"-nappiin.
        public void Restart() { StartRace(); }
    }
}