using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Lukee pelaajan syotteen ja tarjoaa sen autokontrollerille yhdessa paikassa.
    // Tukee kolmea tilaa: Keyboard (testaus editorissa), Tilt (puhelimen kallistus),
    // AutoDrive (kaasu aina pohjassa -> mobiilissa lapsi vain ohjaa).
    public class CarInput : MonoBehaviour
    {
        public enum Mode { Keyboard, Tilt, AutoDrive }

        [Header("Syotetila")]
        [Tooltip("Keyboard = editoritestaus, Tilt = puhelimen kallistus, AutoDrive = kaasu aina pohjassa")]
        public Mode mode = Mode.Keyboard;

        [Header("Kallistus (Tilt-tila)")]
        [Tooltip("Kallistuksen herkkyys")]
        public float tiltSensitivity = 2.5f;
        [Tooltip("Kuollut alue, ettei auto reagoi pieneen tarinaan")]
        public float tiltDeadzone = 0.05f;

        // Autokontrolleri lukee namaa joka ruudussa.
        public float Throttle { get; private set; } // -1..1 (kaasu / jarru)
        public float Steer { get; private set; }     // -1..1 (vasen / oikea)

        void Awake()
        {
            // Kerro consoleen heti kaynnistyksessa mika syotetila on kaytossa.
            Debug.Log($"[CarInput] Syotetila kaytossa: {mode}");
        }

        void Update()
        {
            // Lasketaan syote kerran per ruutu ja talletetaan se ominaisuuksiin.
            Throttle = ReadThrottle();
            Steer = ReadSteer();
        }

        float ReadThrottle()
        {
            if (mode == Mode.AutoDrive) return 1f;  // kaasu aina pohjassa
            return Input.GetAxis("Vertical");       // W/S tai nuolet ylos/alas
        }

        float ReadSteer()
        {
            if (mode == Mode.Tilt)
            {
                float t = Input.acceleration.x * tiltSensitivity;
                if (Mathf.Abs(t) < tiltDeadzone) t = 0f; // suodata pieni tarina pois
                return Mathf.Clamp(t, -1f, 1f);
            }
            return Input.GetAxis("Horizontal");     // A/D tai nuolet sivulle
        }
    }
}