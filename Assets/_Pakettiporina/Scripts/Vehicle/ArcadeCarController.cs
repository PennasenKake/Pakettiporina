using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Arcade-tyylinen autofysiikka ensimmaiseen pelattavaan demoon.
    // Lukee syotteen CarInputilta. Vaatii Rigidbodyn ja CarInputin samassa objektissa.
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CarInput))]
    public class ArcadeCarController : MonoBehaviour
    {
        [Header("Ajo")]
        [Tooltip("Kiihtyvyysvoima")] public float acceleration = 22f;
        [Tooltip("Suurin nopeus")] public float maxSpeed = 12f;
        [Tooltip("Kaantymisnopeus (astetta sekunnissa)")] public float turnSpeed = 120f;
        [Tooltip("Pito: kuinka nopeasti vauhti kaantyy auton suuntaan")] public float grip = 6f;
        [Tooltip("Hidastus kun ei kaasua")] public float brake = 1.5f;

        [Header("Vakaus")]
        [Tooltip("Painopisteen korkeus; matalampi = vakaampi, vahemman kaatuilua")]
        public float centerOfMassY = -0.5f;

        [Header("Debug")]
        [Tooltip("Tulosta nopeus consoleen (EI joka ruutu, vaan valein)")]
        public bool logSpeed = false;
        [Tooltip("Kuinka usein nopeus tulostetaan, sekuntia")]
        public float logInterval = 0.5f;

        Rigidbody rb;
        CarInput input;
        float nextLogTime;
        bool wasMoving;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<CarInput>();
            rb.centerOfMass = new Vector3(0f, centerOfMassY, 0f); // matala painopiste
            Debug.Log("[Car] Autokontrolleri valmis. Paina Play ja aja WASD/nuolilla.");
        }

        void FixedUpdate()
        {
            float throttle = input.Throttle;
            float steer = input.Steer;

            // Auton eteenpain-nopeus: positiivinen = eteen, negatiivinen = taakse.
            float fwdSpeed = Vector3.Dot(rb.velocity, transform.forward);

            // 1) Kiihdytys auton eteenpain-suuntaan.
            rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);

            // 2) Kevyt hidastus kun kaasua ei anneta.
            if (Mathf.Approximately(throttle, 0f))
                rb.velocity = Vector3.MoveTowards(rb.velocity,
                    new Vector3(0f, rb.velocity.y, 0f), brake * Time.fixedDeltaTime);

            // 3) Rajoita huippunopeus (vain vaakataso, ettei putoamisnopeus rajoitu).
            Vector3 flat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flat.magnitude > maxSpeed)
            {
                flat = flat.normalized * maxSpeed;
                rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
            }

            // 4) Kaantyminen — vain kun autolla on vauhtia (paikallaan ei kaanny).
            float speedFactor = Mathf.Clamp01(Mathf.Abs(fwdSpeed) / 3f);
            float yaw = steer * turnSpeed * speedFactor * Time.fixedDeltaTime;
            if (fwdSpeed < -0.1f) yaw = -yaw; // peruutuksessa ohjaus toisinpain
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));

            // 5) Pito — ohjaa sivuttaisvauhti auton eteenpain-suuntaan (arcade-pito).
            Vector3 v = rb.velocity;
            Vector3 forwardVel = transform.forward * fwdSpeed;
            rb.velocity = Vector3.Lerp(v, new Vector3(forwardVel.x, v.y, forwardVel.z),
                grip * Time.fixedDeltaTime);

            // --- Konsolilokit hallitusti (ei joka ruutu) ---
            // Tapahtumaloki: kerro kun auto lahtee liikkeelle tai pysahtyy.
            bool moving = flat.magnitude > 0.3f;
            if (moving != wasMoving)
            {
                Debug.Log(moving ? "[Car] Auto lahti liikkeelle." : "[Car] Auto pysahtyi.");
                wasMoving = moving;
            }
            // Valinnainen nopeusloki, vain logInterval-valein ettei konsoli tukkeudu.
            if (logSpeed && Time.time >= nextLogTime)
            {
                nextLogTime = Time.time + logInterval;
                Debug.Log($"[Car] Nopeus {flat.magnitude:F1}/{maxSpeed} | kaasu {throttle:F2} | ohjaus {steer:F2}");
            }
        }
    }
}