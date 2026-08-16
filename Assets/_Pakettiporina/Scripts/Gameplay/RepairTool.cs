using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 16.8.2026 (kayttaja: "korjaa vaihe tulisi tehda samalla tavalla kuin
    // pesu"). Yksi korjaustyokalu korjausvaiheessa - SAMA nousu/heilunta-liike kuin
    // Bubble.cs:ssa (kelluu jatkuvasti ylospain), mutta kutsuu WashScreen.
    // OnRepairToolTapped() OnBubblePopped():n sijaan ja poistaa yhden KOLHUN (ei
    // likatahran) autosta joka napautuksella.
    //
    // MUUTETTU 16.8.2026 (kayttaja: "pelaajalle voisi korjauksessa nakya jokin vasaran
    // tai siihen liittyva tyokalu mika klikatessa liikkuu mika tarkoittaa korjausta"):
    // napautus EI enaa kutistu-hajoa-palaa (Bubble-tyylinen "poksahdus"), vaan tyokalu
    // TEKEE VASARANISKU-LIIKKEEN (nopea kierto taakse ja isku eteen, ks. RepairSwingAnim)
    // ja himmenee hetkeksi jaahdytyksen ajaksi - visuaalisesti selvasti "korjaamista",
    // ei "poksauttamista". Ikoni itse rakennetaan PakettiporinaWashSetup.cs:ssa
    // karkeana vasaran siluettina (varsi+paa, ei oikeaa kuvitusta) - vaihda Unityssa
    // oikeaan sprite-kuvitukseen jos/kun sellainen tehdaan.
    //
    // Liita UI-objektiin, jossa on Image (Raycast Target paalla). RepairArean
    // (WashScreen.repairArea) alle, sama tapa kuin kuplat BubbleArean alla.
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class RepairTool : MonoBehaviour, IPointerDownHandler
    {
        [Header("Sijainti ja nousu (arvotaan kerran, pysyvat samana)")]
        public float spawnXMin = -380f;
        public float spawnXMax = 380f;
        public float spawnY = -850f;
        public float riseDistance = 1000f;
        public float riseSpeedMin = 45f;
        public float riseSpeedMax = 160f;
        public float swayAmount = 18f;
        public float swaySpeed = 1.0f;
        public float scaleMin = 0.8f;
        public float scaleMax = 1.25f;

        [Header("Vasarointi (hetkellinen, EI pysayta nousua)")]
        public float popCooldown = 1.0f;
        public float popAnimDuration = 0.18f;
        [Tooltip("Vasarankierron laajuus asteina (0 = ei kiertoa, vain himmennys).")]
        public float swingAngle = 32f;
        [Tooltip("Himmeys jaahdytyksen ajan (1 = ei himmenna ollenkaan).")]
        [Range(0.1f, 1f)] public float cooldownAlpha = 0.45f;

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

        void OnEnable()
        {
            onCooldown = false;
            if (img != null) img.raycastTarget = true;

            baseX = Random.Range(spawnXMin, spawnXMax);
            mySpeed = Random.Range(riseSpeedMin, riseSpeedMax);
            swaySeed = Random.Range(0f, 100f);
            myScale = Random.Range(scaleMin, scaleMax);
            risen = Random.Range(0f, riseDistance);

            transform.localScale = Vector3.one * myScale;
        }

        void Update()
        {
            if (rt == null) return;

            risen += mySpeed * Time.deltaTime;
            if (risen >= riseDistance) risen -= riseDistance;

            float sway = Mathf.Sin(Time.time * swaySpeed + swaySeed) * swayAmount;
            rt.anchoredPosition = new Vector2(baseX + sway, spawnY + risen);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TryUse();
        }

        void TryUse()
        {
            if (onCooldown) return;
            onCooldown = true;

            if (img != null) img.raycastTarget = false;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayRepair();
            if (WashScreen.Instance != null) WashScreen.Instance.OnRepairToolTapped();
            Debug.Log("[RepairTool] Vasarointi!");

            StartCoroutine(RepairSwingAnim());
        }

        // Vasariniskun liike: nopea kierto taakse ja "isku" eteen (yksi sini-jakso,
        // vaimenee loppua kohti), samalla pieni skaalapunch myyman iskun voimaa.
        // Ei enaa katoa kokonaan (toisin kuin Bubble-poksahdus) - jaahdytyksen ajan
        // tyokalu vain himmenee, mika lukee selvemmin "juuri kaytetty tyokalu" kuin
        // "hajosi ja ilmestyy takaisin".
        IEnumerator RepairSwingAnim()
        {
            Vector3 full = Vector3.one * myScale;
            float t = 0f;
            while (t < popAnimDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / popAnimDuration);
                float angle = -swingAngle * Mathf.Sin(p * Mathf.PI * 2f) * (1f - p);
                float punch = 0.18f * Mathf.Sin(p * Mathf.PI);
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                transform.localScale = full * (1f + punch);
                yield return null;
            }
            transform.localRotation = Quaternion.identity;
            transform.localScale = full;

            // HUOM: himmennys kohdistuu KAIKKIIN lapsi-Image-komponentteihin, ei vain
            // taman GameObjectin omaan - PakettiporinaWashSetup.cs rakentaa vasaran
            // kahdesta osasta (varsi+paa, molemmat omia Image-lapsiaan), joten pelkka
            // "img" (RequireComponent) ei riittaisi himmentamaan koko ikonia.
            SetAllAlpha(cooldownAlpha);
            float wait = Mathf.Max(0f, popCooldown - popAnimDuration);
            if (wait > 0f) yield return new WaitForSeconds(wait);
            SetAllAlpha(1f);

            if (img != null) img.raycastTarget = true;
            onCooldown = false;
        }

        // HUOM: ohittaa taman GameObjectin OMAN Imagen ("img") - se on tarkoituksella
        // aina lapinakyva (alpha 0, pelkka napautusalue) eika sita saa vahingossa
        // paljastaa nakyvaksi valkoiseksi nelioksi himmennyksen jalkeen.
        void SetAllAlpha(float a)
        {
            foreach (var i in GetComponentsInChildren<Image>(true))
            {
                if (i == img) continue;
                var c = i.color;
                c.a = a;
                i.color = c;
            }
        }
    }
}
