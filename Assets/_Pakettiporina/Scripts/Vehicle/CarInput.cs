using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Lukee ohjauksen kahdesta lahteesta yhtaaikaa:
    //  - Nappaimisto (WSAD/nuolet) -> koneella testaus
    //  - Kosketusnapit (kaasu / vasen / oikea) -> mobiilissa (TouchButton asettaa)
    // Autokontrolleri lukee Throttle- ja Steer-arvot.
    public class CarInput : MonoBehaviour
    {
        [Header("Kallistus (valinnainen lisa)")]
        public bool useTilt = false;
        public float tiltSensitivity = 2.5f;
        public float tiltDeadzone = 0.05f;

        [Header("Automaattikaasu (valinnainen)")]
        [Tooltip("Kaasu aina pohjassa. Ei tarvita jos kaytat kaasunappia.")]
        public bool autoThrottle = false;

        // Kosketusnappien tila — TouchButton asettaa nama.
        [HideInInspector] public bool touchGas;
        [HideInInspector] public bool touchBrake;
        [HideInInspector] public bool touchLeft;
        [HideInInspector] public bool touchRight;

        public float Throttle { get; private set; } // -1..1
        public float Steer { get; private set; }     // -1..1

        void Awake()
        {
            Debug.Log($"[CarInput] Valmis. Nappaimisto: WSAD/nuolet + kosketusnapit (kaasu/vasen/oikea). Tilt={useTilt}, AutoThrottle={autoThrottle}");
        }

        void Update()
        {
            // 1) Nappaimisto (editorissa)
            float kbThrottle = Input.GetAxis("Vertical");
            float kbSteer = Input.GetAxis("Horizontal");

            // 2) Kosketusnapit (mobiilissa)
            float touchThrottle = (touchGas ? 1f : 0f) - (touchBrake ? 1f : 0f);
            float touchSteer = (touchRight ? 1f : 0f) - (touchLeft ? 1f : 0f);

            // 3) Valinnainen kallistus
            float tiltSteer = 0f;
            if (useTilt)
            {
                float t = Input.acceleration.x * tiltSensitivity;
                if (Mathf.Abs(t) < tiltDeadzone) t = 0f;
                tiltSteer = Mathf.Clamp(t, -1f, 1f);
            }

            // Yhdista lahteet (nappaimisto TAI kosketus toimii)
            float throttle = autoThrottle ? 1f : (kbThrottle + touchThrottle);
            Throttle = Mathf.Clamp(throttle, -1f, 1f);
            Steer = Mathf.Clamp(kbSteer + touchSteer + tiltSteer, -1f, 1f);
        }
    }
}