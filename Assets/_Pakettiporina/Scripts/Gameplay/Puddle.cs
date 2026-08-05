using UnityEngine;

namespace Pakettiporina
{
    // Latakko/muta: heikentaa pitoa hetkeksi kun auto ajaa lapi (auto liukuu enemman
    // ennen kuin suuntautuu takaisin ajosuuntaan). Liita objektiin jolla on Collider
    // (Is Trigger paalla - Reset() asettaa taman automaattisesti).
    //
    // CarStats.pito (Hallissa valittu) lieventaa vaikutusta: hyva pito -> auto tuskin
    // huomaa latakkoa, huono pito -> selva liukastus. Tarkoitus opettaa etta pito
    // kannattaa pito-painotteisilla radoilla (esim. "Satama").
    [RequireComponent(typeof(Collider))]
    public class Puddle : MonoBehaviour
    {
        [Tooltip("Kuinka paljon pito heikkenee pahimmillaan (huono pito-tilasto), 0.35 = 35 % jaljella")]
        [Range(0.05f, 1f)] public float minGripMultiplier = 0.35f;
        [Tooltip("Kuinka kauan liukkaus kestaa sen jalkeen kun auto on ajanut lapi, sekuntia")]
        public float duration = 1.0f;
        [Tooltip("Jaahdytysaika ennen kuin sama latakko voi laueta uudelleen")]
        public float cooldown = 0.5f;

        float nextTriggerTime;

        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            if (Time.time < nextTriggerTime) return;
            var car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null) return;

            nextTriggerTime = Time.time + cooldown;

            // Pito-tilasto lieventaa: hyva pito (100) -> kerroin lahella 1:ta,
            // huono pito (0) -> kerroin = minGripMultiplier.
            float pito01 = 0.5f;
            var gm = GameManager.Instance;
            if (gm != null) pito01 = Mathf.Clamp01(gm.SelectedStats.pito / 100f);
            float effectiveMultiplier = Mathf.Lerp(minGripMultiplier, 1f, pito01);

            car.ApplyGripPenalty(effectiveMultiplier, duration);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySplash();
            Debug.Log($"[Puddle] Liukastus! kerroin={effectiveMultiplier:F2} (pito={pito01 * 100f:F0})");
        }
    }
}
