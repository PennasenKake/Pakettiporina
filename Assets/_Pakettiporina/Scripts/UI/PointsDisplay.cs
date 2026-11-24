using System.Collections;
using UnityEngine;
using TMPro;

namespace Pakettiporina
{
    // Nayttaa GameManagerin pisteet TMP-tekstissa (esim. MainMenun tai hallin tahti-napissa).
    // Liita objektiin, jossa on TMP_Text, tai kytke pointsText kasin.
    // LISATTY 23.11.2026: luku "rullaa" ylos edellisesta nakyvasta arvosta uuteen, ja tekstia pulssataan.
    public class PointsDisplay : MonoBehaviour
    {
        public TMP_Text pointsText;

        [Tooltip("Rullaa luku ylos (ja pulssi) kun pisteet muuttuvat tai scene avautuu.")]
        public bool countUp = true;
        public float countSeconds = 0.8f;

        // Viimeksi nakytetty arvo yli scenevaihtojen -> rullaus jatkuu siita mihin jai.
        static int lastShown = 0;

        int shown;
        Coroutine routine;

        void Reset() { pointsText = GetComponent<TMP_Text>(); }

        void OnEnable()
        {
            if (pointsText == null) pointsText = GetComponent<TMP_Text>();
            SwapStarForCoin();
            int target = CurrentPoints();
            shown = countUp ? Mathf.Min(lastShown, target) : target;
            SetText(shown);
            if (countUp && shown != target) AnimateTo(target);
            else lastShown = target;
            // HUOM: pelkka OnEnable-paivitys ei riita, koska pisteet voivat muuttua
            // SAMAN scenen sisalla (esim. tarran ostaminen paavalikossa) ilman etta
            // tama objekti koskaan disabloituu/enabloituu uudelleen - siksi tilataan
            // myos GameManagerin OnPointsChanged-tapahtuma, joka paivittaa tekstin
            // valittomasti aina kun pisteet oikeasti muuttuvat.
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged += HandlePointsChanged;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged -= HandlePointsChanged;
            if (routine != null) { StopCoroutine(routine); routine = null; }
            lastShown = CurrentPoints();
            if (pointsText != null) pointsText.transform.localScale = Vector3.one;
        }

        // 24.11.2026: pisteet ovat KOLIKOITA (tarrat); tahti-ikoni on ajotahdille (lukitukset).
        // Scenen vanha tahti-kuva pisteluvun vierella vaihdetaan kolikkoon ajonaikaisesti (ei scene-muutosta).
        void SwapStarForCoin()
        {
            if (pointsText == null || pointsText.transform.parent == null) return;
            foreach (var img in pointsText.transform.parent.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (img.sprite == null) continue;
                string n = (img.sprite.name + " " + img.gameObject.name).ToLowerInvariant();
                if (n.Contains("tahti") || n.Contains("star")) { img.sprite = Juice.CoinIcon(); img.type = UnityEngine.UI.Image.Type.Simple; img.preserveAspect = true; img.color = Color.white; return; }
            }
        }

        static int CurrentPoints() { return GameManager.Instance != null ? GameManager.Instance.Points : 0; }

        void HandlePointsChanged(int p)
        {
            if (pointsText == null) pointsText = GetComponent<TMP_Text>();
            if (pointsText == null) return;
            if (countUp && isActiveAndEnabled) AnimateTo(p);
            else { shown = p; SetText(p); lastShown = p; }
        }

        void AnimateTo(int target)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Roll(target));
        }

        IEnumerator Roll(int target)
        {
            int from = shown;
            float t = 0f;
            float dur = Mathf.Max(0.1f, countSeconds);
            bool up = target > from;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / dur), 3f); // ease-out
                shown = Mathf.RoundToInt(Mathf.Lerp(from, target, k));
                SetText(shown);
                if (up && pointsText != null)
                {
                    float s = 1f + 0.18f * Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
                    pointsText.transform.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }
            shown = target;
            lastShown = target;
            SetText(target);
            if (pointsText != null) pointsText.transform.localScale = Vector3.one;
            routine = null;
        }

        void SetText(int v)
        {
            if (pointsText != null) pointsText.text = v.ToString();
        }
    }
}
