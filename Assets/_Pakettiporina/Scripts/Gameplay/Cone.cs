using UnityEngine;

namespace Pakettiporina
{
    // Kartio (tai muu pieni fyysinen este): "kolhu" jos auto osuu siihen - leikkaa
    // hetkeksi nopeudesta palan. EI vaurioita autoa eika pysayta peliä; reilu, lyhyt
    // rangaistus ilman aikarajaa tai menetysta, sopii 6-10-vuotiaille.
    //
    // Toimii KAHDELLA tavalla riippuen Collider-asetuksesta:
    //  - Is Trigger POIS (oletus, CreatePrimitive): auto tormaa fyysisesti kartioon
    //    (OnCollisionEnter). Kartio voi myos tyontya sivuun jos sille on Rigidbody.
    //  - Is Trigger PAALLA: auto ajaa suoraan lapi (OnTriggerEnter), ei fyysista tormaysta.
    // Molemmat kutsuvat saman vaikutuksen - valitse kumpi sopii radalle paremmin.
    public class Cone : MonoBehaviour
    {
        [Tooltip("Kuinka paljon nopeudesta leikataan pois osuessa (0.4 = jaa 60 % vauhdista)")]
        [Range(0.1f, 0.9f)] public float speedCutFraction = 0.4f;
        [Tooltip("Jaahdytysaika ennen kuin sama kartio voi laueta uudelleen (estaa jumitilan jos auto jaa kiinni)")]
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
            Debug.Log("[Cone] Kolhu!");
        }
    }
}
