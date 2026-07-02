using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Orkesteroi yhden keikan: lahto -> ajossa -> maali. Hoitaa myos tahtien
    // laskennan, kuluvan ajan ja auton palautuksen jos se putoaa kentalta.
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [Header("Viittaukset")]
        [Tooltip("Auton Rigidbody")] public Rigidbody car;
        [Tooltip("Tyhja objekti, johon auto asetetaan lahdossa")] public Transform startPoint;

        [Header("Asetukset")]
        [Tooltip("Jos auto putoaa taman Y-korkeuden alle, se palautetaan lahtoon")]
        public float fallY = -5f;

        public bool IsRacing { get; private set; }
        public int Stars { get; private set; }
        public float Elapsed { get; private set; }

        void Awake() { Instance = this; }

        void OnEnable() { GameEvents.OnFinish += HandleFinish; }
        void OnDisable() { GameEvents.OnFinish -= HandleFinish; }

        void Start() { StartRace(); }

        public void StartRace()
        {
            Stars = 0;
            Elapsed = 0f;
            ResetCar();
            IsRacing = true;
            if (GameManager.Instance != null) GameManager.Instance.SetPhase(GameManager.Phase.Racing);
            GameEvents.RaceStart();
            Debug.Log("[Race] Keikka alkoi.");
        }

        void Update()
        {
            if (!IsRacing) return;
            Elapsed += Time.deltaTime;

            // Putoamissuoja: jos auto tippuu kentalta, palauta lahtoon.
            if (car != null && car.position.y < fallY)
            {
                Debug.Log("[Race] Auto putosi kentalta — palautetaan lahtoon.");
                ResetCar();
            }
        }

        // Pickup kutsuu tata kun tahti keratan.
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

        void ResetCar()
        {
            if (car == null || startPoint == null)
            {
                Debug.LogWarning("[Race] car tai startPoint puuttuu — aseta ne Inspectorissa!");
                return;
            }
            car.velocity = Vector3.zero;
            car.angularVelocity = Vector3.zero;
            car.position = startPoint.position;
            car.rotation = startPoint.rotation;
        }

        // Kytke tama "Uudestaan"-nappiin.
        public void Restart() { StartRace(); }
    }
}