using UnityEngine;

namespace Pakettiporina
{
    // Laatikko (puinen kuormalaatikko tms.): sama periaate kuin Cone.cs - fyysinen
    // este joka leikkaa hetkeksi nopeudesta palan jos auto tormaa siihen. Oma
    // luokka Konen sijaan jotta radan editointityokalut/nimet/tilastot pysyvat
    // selkeina omana kategorianaan ("kartiot JA laatikot JA lätäköt" - kayttajan
    // 14.8.2026 maarittelema esteroosteri), vaikka toiminta on identtinen.
    //
    // LISATTY 14.8.2026 (kayttaja: "sailytetaan esteina kartiot, laatikot ja
    // latakot"). Kolhu kasvattaa samaa PYSYVAA GameManager.DentMeter-mittaria kuin
    // Kartio (M5.17/16.8.2026, max 6, ei nollaudu ajon alussa) - puretaan Pesu-scenessa
    // napauttamalla RepairTool-tyokaluja (ks. RepairTool.cs).
    public class Laatikko : MonoBehaviour
    {
        [Tooltip("Kuinka paljon nopeudesta leikataan pois osuessa (0.4 = jaa 60 % vauhdista)")]
        [Range(0.1f, 0.9f)] public float speedCutFraction = 0.45f;
        [Tooltip("Jaahdytysaika ennen kuin sama laatikko voi laueta uudelleen")]
        public float cooldown = 0.8f;

        float nextHitTime;

        void OnCollisionEnter(Collision collision) { TryHit(collision.collider); }
        void OnTriggerEnter(Collider other) { TryHit(other); }

        void TryHit(Collider other)
        {
            if (Time.time < nextHitTime) return;
            var car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null) return;

            nextHitTime = Time.time + cooldown;
            car.Bump(speedCutFraction);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBump();
            if (GameManager.Instance != null) GameManager.Instance.AddDent();
            Debug.Log("[Laatikko] Kolhu!");
        }
    }
}
