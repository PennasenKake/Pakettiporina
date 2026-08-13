using System.Collections;
using UnityEngine;

namespace Pakettiporina
{
    // Saippuakupla RADALLA (M5.16, 14.8.2026 - Task 4, kayttajan pyynto "esteita ja
    // tehosteita ratoihin + saippuakupla-mekaniikka"). ERI luokka kuin Gameplay/Bubble.cs
    // (se on Pesu-kohtauksen 2D UI-kupla, tama on 3D-maailmassa radalla kelluva kupla).
    //
    // FILOSOFIA sama kuin Boostilla/Pesun kuplilla: MIELLYTTAVA, EI-RANGAISTAVA mekaniikka.
    // Auto ajaa kuplan lapi -> kupla poksahtaa (pieni visuaalinen "pop" + poksahdusaani,
    // AudioManager.bubblePop - SAMA aani kuin Pesussa, jo olemassa) ja pelaaja saa yhden
    // tahden (RaceManager.AddStar(), sama palkkiojarjestelma kuin Tahti-keraily/Pickup.cs
    // - RaceHUD paivittyy automaattisesti, ei tarvitse uutta UI-koodia). Kupla EI katoa
    // pysyvasti - se "kasvaa takaisin" respawnDelay:n jalkeen, joten samaa kuplaa voi
    // poksauttaa uudelleen jos ajaa toisen kierroksen tai palaa takaisin.
    //
    // Liita objektiin jolla on Collider (Is Trigger paalla - Reset() asettaa taman
    // automaattisesti). Visuaaliksi sopii esim. pallo (Sphere) + lapikuultava/saippuakupla-
    // tyylinen materiaali. Voit lisata samaan objektiin (tai lapseen) my os Spinner-
    // komponentin (ks. Gameplay/Spinner.cs) pehmeaan kellunta-animaatioon - ei pakollinen.
    [RequireComponent(typeof(Collider))]
    public class TrackBubble : MonoBehaviour
    {
        [Tooltip("Kuinka kauan kupla on piilossa/koskematon poksahduksen jalkeen ennen kuin se kasvaa takaisin nakyviin, sekuntia")]
        public float respawnDelay = 4f;
        [Tooltip("Kutistumis-/kasvuanimaation kesto (kumpikin suunta)")]
        public float popAnimDuration = 0.15f;

        [Tooltip("Visuaalinen lapsiobjekti joka skaalataan poksahtaessa (jos tyhja, kaytetaan tata objektia itseaan)")]
        public Transform visual;

        Collider col;
        Vector3 fullScale;
        bool popped;

        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void Awake()
        {
            col = GetComponent<Collider>();
            if (visual == null) visual = transform;
            fullScale = visual.localScale;
        }

        void OnTriggerEnter(Collider other)
        {
            if (popped) return;
            var car = other.GetComponentInParent<ArcadeCarController>();
            if (car == null) return;

            TryPop();
        }

        void TryPop()
        {
            popped = true;
            if (col != null) col.enabled = false;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayBubblePop();
            if (RaceManager.Instance != null) RaceManager.Instance.AddStar();
            Debug.Log("[TrackBubble] Poks! +1 tahti.");

            StartCoroutine(PopAndRespawn());
        }

        IEnumerator PopAndRespawn()
        {
            // kutistu pois nakyvista
            float t = 0f;
            while (t < popAnimDuration)
            {
                t += Time.deltaTime;
                visual.localScale = Vector3.Lerp(fullScale, Vector3.zero, t / popAnimDuration);
                yield return null;
            }
            visual.localScale = Vector3.zero;

            float wait = Mathf.Max(0f, respawnDelay - popAnimDuration * 2f);
            if (wait > 0f) yield return new WaitForSeconds(wait);

            // kasva takaisin nakyviin
            t = 0f;
            while (t < popAnimDuration)
            {
                t += Time.deltaTime;
                visual.localScale = Vector3.Lerp(Vector3.zero, fullScale, t / popAnimDuration);
                yield return null;
            }
            visual.localScale = fullScale;

            if (col != null) col.enabled = true;
            popped = false;
        }
    }
}
