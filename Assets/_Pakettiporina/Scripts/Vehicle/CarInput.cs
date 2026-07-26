using UnityEngine;

namespace Pakettiporina
{
    // OHJAUS:
    //  - Auto ajaa ITSESTAAN eteenpain (nopeus tulee auton ominaisuuksista).
    //  - Keskinappi = JARRU. Pidettaessa auto hidastaa ja alkaa pakittaa.
    //  - Ohjaus: napit, puhelimen kallistus, tai molemmat (valittavissa).
    //  - Kallistus KALIBROIDAAN lahtolaskennan lopussa: se asento, jossa lasta
    //    pitaa puhelinta AJA!-hetkella, on "suoraan".
    //  - Nappaimisto (A/D ohjaa, S jarruttaa) toimii aina editoritestausta varten.
    public class CarInput : MonoBehaviour
    {
        public enum ControlMode { Napit, Kallistus, Molemmat }

        [Header("Ohjaustapa")]
        public ControlMode controlMode = ControlMode.Molemmat;

        [Header("Automaattiajo")]
        [Tooltip("Auto liikkuu itsestaan eteenpain")]
        public bool autoDrive = true;

        [Header("Kallistus")]
        [Tooltip("Kuinka paljon puhelinta pitaa kallistaa taysille ohjaukselle (pienempi = herkempi)")]
        [Range(0.1f, 0.8f)] public float maxTiltAngle = 0.35f;
        [Tooltip("Pieni kuollut alue, ettei auto reagoi tarinaan")]
        [Range(0f, 0.15f)] public float tiltDeadzone = 0.04f;
        [Tooltip("Pehmennys: isompi = nopeampi reagointi")]
        public float tiltSmoothing = 10f;

        // Kosketusnapit — TouchButton asettaa nama.
        // HUOM: touchGas toimii JARRUNA (vanha nappikytkenta kelpaa sellaisenaan).
        [HideInInspector] public bool touchGas;
        [HideInInspector] public bool touchBrake;
        [HideInInspector] public bool touchLeft;
        [HideInInspector] public bool touchRight;

        public float Throttle { get; private set; }  // -1 (pakki) .. 1 (eteen)
        public float Steer { get; private set; }     // -1 .. 1
        public bool IsBraking { get; private set; }
        public bool TiltAvailable { get; private set; }

        // Kalibroitu "suoraan"-asento. Oletus: puhelin kadessa n. 45 asteen kulmassa.
        Vector3 tiltNeutral = new Vector3(0f, -0.7f, -0.7f);
        float smoothedTilt;

        void Awake()
        {
            TiltAvailable = SystemInfo.supportsAccelerometer;
            Debug.Log($"[CarInput] Valmis. Tila={controlMode}, automaattiajo={autoDrive}, kiihtyvyysanturi={TiltAvailable}");
        }

        void OnEnable() { GameEvents.OnGo += CalibrateTilt; }
        void OnDisable() { GameEvents.OnGo -= CalibrateTilt; }

        void Start() { CalibrateTilt(); }

        // Ottaa nykyisen puhelimen asennon "suoraksi". Kutsutaan AJA!-hetkella.
        public void CalibrateTilt()
        {
            if (!TiltAvailable) return;
            Vector3 a = Input.acceleration;
            if (a.sqrMagnitude > 0.01f)
            {
                tiltNeutral = a;
                smoothedTilt = 0f;
                Debug.Log($"[CarInput] Kallistus kalibroitu (neutraali x={a.x:F2}).");
            }
        }

        void Update()
        {
            // Lahtolaskennan aikana ei ohjata eika liikuta.
            bool racing = RaceManager.Instance == null || RaceManager.Instance.IsRacing;
            if (!racing)
            {
                Throttle = 0f; Steer = 0f; IsBraking = false;
                smoothedTilt = 0f;
                return;
            }

            float kbV = Input.GetAxis("Vertical");
            float kbH = Input.GetAxis("Horizontal");

            // --- Jarru ---
            IsBraking = touchGas || touchBrake || kbV < -0.1f;
            float forward = autoDrive ? 1f : Mathf.Max(0f, kbV);
            Throttle = IsBraking ? -1f : forward;

            // --- Ohjaus ---
            float steer = kbH;   // nappaimisto aina mukana (editoritestaus)

            if (controlMode == ControlMode.Napit || controlMode == ControlMode.Molemmat)
                steer += (touchRight ? 1f : 0f) - (touchLeft ? 1f : 0f);

            if ((controlMode == ControlMode.Kallistus || controlMode == ControlMode.Molemmat) && TiltAvailable)
                steer += ReadTilt();

            Steer = Mathf.Clamp(steer, -1f, 1f);
        }

        float ReadTilt()
        {
            // Ero kalibroituun neutraaliin — nain puhelinta saa pitaa missa asennossa tahansa.
            float raw = Input.acceleration.x - tiltNeutral.x;

            // Kuollut alue, ja sen jalkeen pehmea aloitus (ei hyppaysta)
            if (Mathf.Abs(raw) < tiltDeadzone) raw = 0f;
            else raw -= Mathf.Sign(raw) * tiltDeadzone;

            float target = Mathf.Clamp(raw / Mathf.Max(0.05f, maxTiltAngle), -1f, 1f);
            smoothedTilt = Mathf.Lerp(smoothedTilt, target, Time.deltaTime * tiltSmoothing);
            return smoothedTilt;
        }

        // Kytke halutessasi asetusnappeihin (0=Napit, 1=Kallistus, 2=Molemmat)
        public void SetControlMode(int mode)
        {
            controlMode = (ControlMode)Mathf.Clamp(mode, 0, 2);
            CalibrateTilt();
            Debug.Log("[CarInput] Ohjaustapa: " + controlMode);
        }
    }
}