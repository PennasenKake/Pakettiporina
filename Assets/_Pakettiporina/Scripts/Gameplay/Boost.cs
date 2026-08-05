using UnityEngine;

namespace Pakettiporina
{
    // Boost-tapa (vihrea nuolilaatta): antaa hetkellisen vauhtipiikin kun auto ajaa lapi.
    // Liita objektiin jolla on Collider (Is Trigger paalla - Reset() asettaa taman
    // automaattisesti kun lisaat komponentin).
    //
    // Kevyempi auto (CarStats.keveys, Hallissa valittu) saa vahvemman tehostuksen -
    // taman tarkoitus on opettaa etta keveys kannattaa boost-painotteisilla radoilla
    // (esim. "Puisto"), samalla tavalla kuin Lätäkkö opettaa etta pito kannattaa.
    [RequireComponent(typeof(Collider))]
    public class Boost : MonoBehaviour
    {
        [Tooltip("Kuinka paljon nopeuskattoa nostetaan hetkeksi (1.4 = +40 %)")]
        public float speedMultiplier = 1.4f;
        [Tooltip("Kuinka kauan tehostus kestaa, sekuntia")]
        public float duration = 1.2f;
        [Tooltip("Valitton tyontovoima ajosuuntaan kun auto ajaa lapi (lisaksi nopeuskattoa nostetaan)")]
        public float impulse = 6f;
        [Tooltip("Jaahdytysaika ennen kuin sama boost voi laueta uudelleen (estaa jumitilanteen tayspysahduksissa)")]
        public float cooldown = 0.5f;
        [Tooltip("Kuinka paljon keveys-tilasto voi parhaimmillaan lisata tehostusta (0.5 = +50 % lisaa taydella keveydella)")]
        [Range(0f, 1f)] public float keveysBonusMax = 0.5f;

        float nextTriggerTime;

        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (Time.time < nextTriggerTime) return;
            var car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null) return;

            nextTriggerTime = Time.time + cooldown;

            float bonus = 1f;
            var gm = GameManager.Instance;
            if (gm != null) bonus = 1f + Mathf.Clamp01(gm.SelectedStats.keveys / 100f) * keveysBonusMax;

            car.ApplyBoost(speedMultiplier * bonus, duration, impulse * bonus);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBoost();
            Debug.Log($"[Boost] Tehostus! peruskerroin={speedMultiplier:F2}, keveysbonus={bonus:F2}");
        }
    }
}
