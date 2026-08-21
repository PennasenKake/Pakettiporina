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

        [Header("Nopeustaso (Asetukset: Hidas/Keski/Nopea, ks. SaveManager.carSpeedTier)")]
        [Tooltip("0=Hidas, 1=Keski, 2=Nopea. SaveManager.ApplyToScene() asettaa taman scenen latautuessa - ei tarvitse kasin muokata.")]
        public int speedTier = 1;
        // Kerroin kiihtyvyydelle JA huippunopeudelle - molemmat skaalautuvat yhdessa, jotta
        // "Hidas" tuntuu oikeasti rauhallisemmalta autolta eika vain matalammalta katolta
        // (pelkka maxSpeed-rajoitus ilman kiihtyvyyden pienennysta tuntuisi vain "tukkoiselta").
        // KORJATTU 16.8.2026: 0.72-1.32 ei riittanyt erottumaan.
        // KORJATTU UUDELLEEN 19.8.2026: levennetty 0.4-2.4:aan - EI VIELA RIITTANYT/MENI
        // LIIAN PITKALLE (kayttaja: "Hidas ei liikuta autoa... Nopea liian nopea" - eli
        // molemmat aarilaidat menivat rikki eri suuntiin samaan aikaan).
        // KORJATTU KOLMANNEN KERRAN 19.8.2026 (kayttaja: "missa Hidas Keski ja Nopea
        // maaritetaan"): MUUTETTU STATIC readonly -taulukosta Inspectorista suoraan
        // muokattavaksi kentaksi - loydat/saadat naita NYT TASTA (ArcadeCarController-
        // komponentti, Car-prefab tai scenen Car-objekti, "Nopeustaso"-otsikon alla,
        // "Speed Tier Multiplier" -taulukko: [0]=Hidas, [1]=Keski, [2]=Nopea). Ei tarvitse
        // enaa minun muokata koodia joka kerta - voit itse hienosaataa suoraan Play-tilan
        // AIKANA Inspectorista ja nahda vaikutuksen HETI (ApplySpeedTier lokittaa nyt myos
        // konsoliin joka soveltokerralla tarkat lopulliset arvot, ks. alla).
        [Tooltip("[0]=Hidas, [1]=Keski, [2]=Nopea. Kerroin kiihtyvyydelle JA huippunopeudelle. Keski pidetaan 1.0:ssa jotta olemassaoleva ratasuunnittelu ei muutu. Voit saataa naita suoraan tasta - ei tarvitse koodimuutosta.")]
        public float[] speedTierMultiplier = { 0.65f, 1f, 1.6f };
        float baseAcceleration = -1f, baseMaxSpeed = -1f;

        [Header("Debug")]
        [Tooltip("Tulosta nopeus consoleen (EI joka ruutu, vaan valein)")]
        public bool logSpeed = false;
        [Tooltip("Kuinka usein nopeus tulostetaan, sekuntia")]
        public float logInterval = 0.5f;

        [Header("Radan tilapaiset vaikutukset (EI muokata kasin - Boost/Puddle asettavat nama)")]
        float boostMultiplier = 1f;
        float boostEndTime = -1f;
        float gripPenaltyMultiplier = 1f;
        float gripPenaltyEndTime = -1f;

        Rigidbody rb;
        CarInput input;
        float nextLogTime;
        bool wasMoving;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            input = GetComponent<CarInput>();
            rb.centerOfMass = new Vector3(0f, centerOfMassY, 0f); // matala painopiste
            // Talleta Inspectorin alkuperaiset arvot "perustasoksi" ENNEN kuin ApplySpeedTier
            // mahdollisesti skaalaa niita - nain SaveManager.ApplyToScene() (joka voi kutsua
            // taman useamman kerran, esim. Asetuksista kesken pelin) ei koskaan kasaannu
            // (0.72 * 0.72 * ...) vaan laskee aina samasta perustasosta.
            baseAcceleration = acceleration;
            baseMaxSpeed = maxSpeed;
            ApplySpeedTier(speedTier);
            Debug.Log("[Car] Autokontrolleri valmis. Paina Play ja aja WASD/nuolilla.");
        }

        // Asetukset-paneelin "Ajonopeus"-valitsin (Hidas/Keski/Nopea) kutsuu tata SaveManagerin
        // kautta (ks. SaveManager.ApplyToScene). Turvallinen kutsua ennen Awakea EI ole -
        // jos base-arvoja ei viela ole otettu talteen, otetaan ne talteen nyt.
        // KORJATTU 19.8.2026 (kayttaja: "nopeuden valitsin Hidas, Keski ja Nopea, niin
        // Hidas ei liikuta autoa... tama korjata"): Hidas (0.4x) yhdistettyna hallissa
        // valitun paketin HEIKOIMPAAN mahdolliseen kiihtyvyys/nopeus-yhdistelmaan
        // (RaceSetup.cs, accelMin/speedMin * raskaan paketin massavaimennus) saattoi
        // paatya niin pieneksi etta auto ei kaytannossa tuntunut liikkuvan ollenkaan.
        // Kova VAHIMMAISTASO nyt varmistaa etta Hidas-taso on aina selvasti hitaampi
        // kuin Keski MUTTA EI KOSKAAN kaytannossa pysahdyksissa, riippumatta siita mika
        // paketti/osat on valittu.
        const float MinAcceleration = 5f;
        const float MinMaxSpeed = 3f;

        public void ApplySpeedTier(int tier)
        {
            if (baseAcceleration < 0f) baseAcceleration = acceleration;
            if (baseMaxSpeed < 0f) baseMaxSpeed = maxSpeed;
            speedTier = Mathf.Clamp(tier, 0, 2);
            float m = (speedTierMultiplier != null && speedTierMultiplier.Length == 3) ? speedTierMultiplier[speedTier] : 1f;
            acceleration = Mathf.Max(baseAcceleration * m, MinAcceleration);
            maxSpeed = Mathf.Max(baseMaxSpeed * m, MinMaxSpeed);
            // KORJATTU KOLMANNEN KERRAN 19.8.2026: lokitetaan AINA lopulliset arvot, jotta
            // seuraavalla testauskerralla konsolista nakee suoraan mika tier/kerroin/
            // perusarvo/lopputulos oikeasti oli - ei tarvitse enaa arvailla staattisesti.
            Debug.Log($"[Car] Nopeustaso sovellettu: tier={speedTier} ({(new[]{"Hidas","Keski","Nopea"})[speedTier]}), kerroin={m:F2}, base(accel/maxSpeed)={baseAcceleration:F1}/{baseMaxSpeed:F1} -> LOPULLINEN accel={acceleration:F1} maxSpeed={maxSpeed:F1}");
        }

        // KORJATTU 19.8.2026 (koko pelin tarkistuksessa loydetty bugi, selittaa kayttajan
        // toistuvan valituksen "en huomaa vaihtelu eroa hitaan ja Keski seka nopean
        // valilla" - Nopeustaso-kerrointa levennettiin kahdesti turhaan): RaceSetup.cs
        // asetti AIEMMIN car.acceleration/car.maxSpeed SUORAAN hallissa valitun paketin
        // perusteella Start():ssa - TAMA TAPAHTUU AINA ArcadeCarController.Awake():n
        // JALKEEN (Unity: kaikki Awaket ennen yhtaan Start():a), eli se ylikirjoitti
        // Nopeustaso-kertoimen vaikutuksen KOKONAAN aina kun pelaaja oli kaynyt Hallissa
        // (eli normaalilla pelireitilla AINA) - Asetusten Hidas/Keski/Nopea-valinnalla ei
        // siis ollut mitaan vaikutusta oikeaan ajoon, vain suoraan scenetestaukseen ilman
        // hallivalintaa. RaceSetup kutsuu nyt tata PERUSARVOJEN (base-, ennen Nopeustaso-
        // kerrointa) asettamiseen suoran acceleration/maxSpeed-asetuksen sijaan - kerroin
        // sovelletaan aina PERAAN, jotta pakettivalinta JA nopeustaso vaikuttavat yhdessa.
        public void SetBaseStats(float accel, float maxSp, float turn, float gr)
        {
            baseAcceleration = accel;
            baseMaxSpeed = maxSp;
            turnSpeed = turn;
            grip = gr;
            ApplySpeedTier(speedTier);
        }

        void FixedUpdate()
        {
            // KORJATTU 19.8.2026: RaceManager.FreezeCar(true) asettaa rb.isKinematic = true
            // lahdon odotusaikana (countdown), jotta auto ei paase liikkumaan ennen "AJA!".
            // Kinemaattiselle Rigidbodylle EI SAA asettaa .velocitya - Unity kirjoittaa siita
            // konsoliin varoituksen JOKA FixedUpdate-ruudulla ("Setting linear velocity of a
            // kinematic body is not supported"), mika tayttaa konsolin turhaan. Auton fysiikkaa
            // ei muutenkaan tarvitse laskea kesken jaadytyksen, joten koko paivitys ohitetaan.
            if (rb.isKinematic) return;

            // Nollaa tilapaiset radan vaikutukset (Boost/Puddle) kun niiden aika loppuu.
            if (Time.time > boostEndTime) boostMultiplier = 1f;
            if (Time.time > gripPenaltyEndTime) gripPenaltyMultiplier = 1f;
            float effectiveMaxSpeed = maxSpeed * boostMultiplier;
            float effectiveGrip = grip * gripPenaltyMultiplier;

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
            // Boost nostaa effectiveMaxSpeedia hetkeksi, joten kattoa ei tarvitse muuttaa pysyvasti.
            Vector3 flat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flat.magnitude > effectiveMaxSpeed)
            {
                flat = flat.normalized * effectiveMaxSpeed;
                rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
            }

            // 4) Kaantyminen — vain kun autolla on vauhtia (paikallaan ei kaanny).
            float speedFactor = Mathf.Clamp01(Mathf.Abs(fwdSpeed) / 3f);
            float yaw = steer * turnSpeed * speedFactor * Time.fixedDeltaTime;
            if (fwdSpeed < -0.1f) yaw = -yaw; // peruutuksessa ohjaus toisinpain
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yaw, 0f));

            // 5) Pito — ohjaa sivuttaisvauhti auton eteenpain-suuntaan (arcade-pito).
            // Lätäkkö pienentaa effectiveGripia hetkeksi, jolloin auto liukuu enemman sivuttain.
            Vector3 v = rb.velocity;
            Vector3 forwardVel = transform.forward * fwdSpeed;
            rb.velocity = Vector3.Lerp(v, new Vector3(forwardVel.x, v.y, forwardVel.z),
                effectiveGrip * Time.fixedDeltaTime);

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
                Debug.Log($"[Car] Nopeus {flat.magnitude:F1}/{effectiveMaxSpeed:F1} | kaasu {throttle:F2} | ohjaus {steer:F2}");
            }
        }

        // --- Radan elementtien kutsumat julkiset metodit (Boost/Puddle/Cone) ---

        // Kutsutaan Boost-objektista: nostaa nopeuskattoa hetkeksi ja antaa valittoman
        // tyonnon eteenpain. Jos useampi boost paalekkain, kaytetaan vahvinta/pisinta.
        public void ApplyBoost(float multiplier, float duration, float impulse = 0f)
        {
            boostMultiplier = Mathf.Max(boostMultiplier, multiplier);
            boostEndTime = Mathf.Max(boostEndTime, Time.time + duration);
            if (impulse > 0f) rb.AddForce(transform.forward * impulse, ForceMode.VelocityChange);
        }

        // Kutsutaan Puddle-objektista: heikentaa pitoa hetkeksi (auto liukuu enemman
        // sivuttain ennen kuin suuntautuu takaisin ajosuuntaan).
        public void ApplyGripPenalty(float multiplier, float duration)
        {
            gripPenaltyMultiplier = Mathf.Min(gripPenaltyMultiplier, multiplier);
            gripPenaltyEndTime = Mathf.Max(gripPenaltyEndTime, Time.time + duration);
        }

        // Kutsutaan Cone-objektista: leikkaa nopeudesta hetkellisesti palan. Ei pysyvaa
        // rangaistusta eika vauriota - lapselle reilu "kolhu".
        public void Bump(float speedCutFraction)
        {
            if (rb.isKinematic) return; // ei osumia kesken lahdon jaadytyksen
            rb.velocity *= Mathf.Clamp01(1f - speedCutFraction);
            VibrationManager.Vibrate(); // Asetukset: "Varahtely" - tuntuva kuittaus kolhusta
        }
    }
}