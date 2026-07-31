using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // Yksi saippuakupla pesukohtauksessa. Liita UI-objektiin, jossa on Image
    // (pyorea kupla-sprite, Raycast Target paalla). Napautus/klikkaus poksauttaa.
    //
    // MIELLYTTAVA, EI-RANGAISTAVA MEKANIIKKA (samaan tapaan kuin HTML-mockupin
    // CSS-animaatio): kupla kelluu YLOSPAIN LOPUTTOMASTI, sivuttaispaikka ja
    // nousunopeus arvotaan KERRAN ja pysyvat samana koko elinkaaren (ei "karkaa"
    // eika "poksahda itsestaan" - vain jatkuva pehmea taustaliike). Jokainen kupla
    // aloittaa SATUNNAISESTA VAIHEESTA nousussaan (risen = satunnainen 0..riseDistance),
    // jolloin kaikki eivat ole heti kasassa alhaalla - vastaa CSS:n negatiivista
    // animation-delaya.
    //
    // Napautus: lyhyt "poksahdus" (kutistuu/haviaa hetkeksi), EI vaikuta liikkeeseen -
    // kupla jatkaa nousuaan taustalla koko ajan, ja ilmestyy nakyviin uudelleen
    // popCooldownin jalkeen. WashScreen saa aina +1 "siivousetenemisen" jokaisesta
    // napautuksesta (poistaa yhden likatahran autosta).
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class Bubble : MonoBehaviour, IPointerDownHandler
    {
        [Header("Sijainti ja nousu (arvotaan kerran, pysyvat samana)")]
        [Tooltip("Vasen raja, anchoredPosition.x BubbleArean sisalla")]
        public float spawnXMin = -380f;
        [Tooltip("Oikea raja, anchoredPosition.x BubbleArean sisalla")]
        public float spawnXMax = 380f;
        [Tooltip("Alareunan Y-sijainti, anchoredPosition.y BubbleArean sisalla")]
        public float spawnY = -850f;
        [Tooltip("Matka alareunasta ylareunaan, jonka jalkeen kupla kiertaa saumattomasti takaisin alkuun")]
        public float riseDistance = 1000f;
        [Tooltip("Nousunopeuden vaihteluvali, UI-pikselia sekunnissa")]
        public float riseSpeedMin = 45f;
        public float riseSpeedMax = 160f;
        [Tooltip("Sivuttaisheilunnan leveys nousun aikana")]
        public float swayAmount = 18f;
        public float swaySpeed = 1.0f;
        [Tooltip("Koon vaihtelu (jokainen kupla hieman eri kokoinen)")]
        public float scaleMin = 0.8f;
        public float scaleMax = 1.25f;

        [Header("Poksahdus (hetkellinen, EI pysayta nousua)")]
        [Tooltip("Kuinka pitkaan kupla on piilossa/ei-napautettavissa poksahduksen jalkeen")]
        public float popCooldown = 1.0f;
        [Tooltip("Kutistumis-/kasvuanimaation kesto (kumpikin suunta)")]
        public float popAnimDuration = 0.18f;

        Image img;
        RectTransform rt;

        float baseX;
        float mySpeed;
        float swaySeed;
        float myScale;
        float risen;
        bool onCooldown;

        void Awake()
        {
            img = GetComponent<Image>();
            rt = GetComponent<RectTransform>();
        }

        // Arpoo kuplalle omat pysyvat ominaisuudet. Kutsutaan kerran (Awake+OnEnable ensimmaisella
        // aktivoinnilla) ja uudestaan jos WashScreen kaynnistaa koko pesun alusta.
        void OnEnable()
        {
            onCooldown = false;
            if (img != null) img.raycastTarget = true;

            baseX = Random.Range(spawnXMin, spawnXMax);
            mySpeed = Random.Range(riseSpeedMin, riseSpeedMax);
            swaySeed = Random.Range(0f, 100f);
            myScale = Random.Range(scaleMin, scaleMax);
            risen = Random.Range(0f, riseDistance); // satunnainen alkuvaihe -> ei kasaudu alkuun

            transform.localScale = Vector3.one * myScale;
        }

        void Update()
        {
            if (rt == null) return;

            // Nousu jatkuu AINA, myos poksahdus-cooldownin aikana (kuten mockupin CSS-animaatio).
            risen += mySpeed * Time.deltaTime;
            if (risen >= riseDistance) risen -= riseDistance;

            float sway = Mathf.Sin(Time.time * swaySpeed + swaySeed) * swayAmount;
            rt.anchoredPosition = new Vector2(baseX + sway, spawnY + risen);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TryPop();
        }

        void TryPop()
        {
            if (onCooldown) return;
            onCooldown = true;

            if (img != null) img.raycastTarget = false;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayBubblePop();
            if (WashScreen.Instance != null) WashScreen.Instance.OnBubblePopped();
            Debug.Log("[Bubble] Poks!");

            StartCoroutine(PopCooldownAnim());
        }

        IEnumerator PopCooldownAnim()
        {
            Vector3 full = Vector3.one * myScale;

            // kutistu pois nakyvista
            float t = 0f;
            while (t < popAnimDuration)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(full, Vector3.zero, t / popAnimDuration);
                yield return null;
            }
            transform.localScale = Vector3.zero;

            // odota loppu cooldownista piilossa (nousu jatkuu taustalla koko ajan)
            float wait = Mathf.Max(0f, popCooldown - popAnimDuration * 2f);
            if (wait > 0f) yield return new WaitForSeconds(wait);

            // kasva takaisin nakyviin
            t = 0f;
            while (t < popAnimDuration)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.zero, full, t / popAnimDuration);
                yield return null;
            }
            transform.localScale = full;

            if (img != null) img.raycastTarget = true;
            onCooldown = false;
        }
    }
}
