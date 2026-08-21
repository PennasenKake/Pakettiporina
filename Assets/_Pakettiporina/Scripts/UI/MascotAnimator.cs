using UnityEngine;
using TMPro;

namespace Pakettiporina
{
    // LISATTY 19.8.2026 (kayttaja: "Pakettipörinä maskotti luoda niin että se liikkuu ja
    // heiluttaa esim kättä ... main menussa, settings ja garagessa, voisiko antaa jotain
    // tilanteeseen sopivaa kommenttia ääneen"). Projektissa ei ollut valmiina yhtaan 3D-
    // hahmoa, riggausta tai animaatioklippia mihinkaan - vain yksi litteä 2D-kuva
    // (pakettiporina_vilkku_hahmo.png, jo poseerattu kasi ylhaalla "heiluttaa"-asennossa).
    // Kayttaja valitsi kevyen version: animoidaan TATA samaa 2D-kuvaa koodilla (ei uutta
    // 3D-mallia/riggausta), ja aanikommentit korvataan puhekupla-TEKSTILLA koska
    // aaninaytteiden nauhoitus/generointi ei ole tassa tyokalussa mahdollista.
    //
    // Kaksi liikettä yhdessä antavat elavan "heiluttaa iloisesti" -tunnun litteasta
    // still-kuvasta: 1) pehmea pystysuuntainen pomppu (hengitys/elavyys), 2) rytminen
    // kallistus-heilunta joka jaljittelee kadenheilutusta, voimistuen hetkeksi
    // saannollisin valein isommaksi "TASSA MINÄ OLEN" -heilautukseksi.
    public class MascotAnimator : MonoBehaviour
    {
        [Header("Pomppu (pystyliike)")]
        public float bobAmplitude = 6f;      // pikselia
        public float bobSpeed = 1.6f;        // jaksoa/sekunti-tyylinen kerroin

        [Header("Heiluntakallistus")]
        public float idleTiltDegrees = 4f;   // jatkuva kevyt heilunta
        public float waveTiltDegrees = 16f;  // isompi heilautus pulssin aikana
        public float waveInterval = 3.5f;    // sekuntia idle-heilunnan valissa ennen isompaa heilautusta
        public float wavePulseDuration = 1.0f;

        [Header("Puhekupla (valinnainen - korvaa aanikommentin)")]
        [Tooltip("Jos asetettu, nailta riveilta valitaan satunnaisesti/jarjestyksessa teksti nakyviin puhekuplaan.")]
        public TMP_Text speechBubbleText;
        public GameObject speechBubbleRoot; // koko kupla piilotetaan/nayetaan tallä (jos eri kuin teksti itse)
        [TextArea(1, 2)]
        public string[] speechLines;
        public float speechVisibleSeconds = 3.2f;
        public float speechIntervalSeconds = 6f;

        [Header("Piilotus koko naytolla olevien paneelien paalla (19.8.2026)")]
        [Tooltip("Kayttaja: 'maskotin tulisi olla piilossa kun settings ja infopanel ja privacypanel on paalla'. Jos jokin naista on activeInHierarchy, maskotti (ja puhekupla) piilotetaan CanvasGroup.alpha=0:lla - GameObject pysyy itse aktiivisena jotta Update() jatkaa pyorimista ja huomaa kun paneeli sulkeutuu.")]
        public GameObject[] hideWhilePanelsActive;

        RectTransform rt;
        Vector2 basePos;
        float t;
        float waveTimer;
        int speechIndex;
        float speechTimer;
        bool speechShowing;
        CanvasGroup cg;

        void Awake()
        {
            rt = transform as RectTransform;
            if (rt != null) basePos = rt.anchoredPosition;
            waveTimer = waveInterval; // aloita ensimmainen "iso heilautus" kohtuu pian, ei heti
            speechTimer = 1.2f; // pieni viive ennen ensimmaista puhekuplaa, ei heti scenen alussa
            if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);

            cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        }

        void Update()
        {
            // Piilotus paneelien paalla - tarkistetaan ensin, ennen animaatiota (ei tarvetta
            // animoida nakymatonta maskottia, mutta ei myoskaan haittaa - alpha=0 riittaa).
            bool hide = false;
            if (hideWhilePanelsActive != null)
            {
                for (int i = 0; i < hideWhilePanelsActive.Length; i++)
                {
                    if (hideWhilePanelsActive[i] != null && hideWhilePanelsActive[i].activeInHierarchy)
                    {
                        hide = true;
                        break;
                    }
                }
            }
            if (cg != null)
            {
                cg.alpha = hide ? 0f : 1f;
                cg.blocksRaycasts = !hide;
                cg.interactable = !hide;
            }

            t += Time.unscaledDeltaTime;

            // 1) Pehmea pomppu - toimii aina, taustalla.
            if (rt != null)
            {
                float bob = Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmplitude;
                rt.anchoredPosition = basePos + new Vector2(0f, bob);
            }

            // 2) Kallistusheilunta: jatkuva pieni + saannollinen isompi pulssi.
            waveTimer -= Time.unscaledDeltaTime;
            float tilt;
            if (waveTimer <= 0f)
            {
                // Isomman heilautuksen aikana - nopeampi, laajempi kallistus muutaman sekunnin ajan.
                float pulseT = Mathf.Clamp01(1f - (waveTimer + wavePulseDuration) / wavePulseDuration);
                tilt = Mathf.Sin(t * 9f) * waveTiltDegrees * (1f - pulseT);
                if (waveTimer <= -wavePulseDuration)
                    waveTimer = waveInterval; // palaa idle-heilluntaan, odota seuraavaan pulssiin
            }
            else
            {
                tilt = Mathf.Sin(t * 2.2f) * idleTiltDegrees;
            }
            transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

            // 3) Puhekupla-teksti (aanikommentin korvike) - vaihtuu tasaisin valein.
            if (speechLines != null && speechLines.Length > 0 && speechBubbleText != null)
            {
                speechTimer -= Time.unscaledDeltaTime;
                if (speechTimer <= 0f)
                {
                    if (!speechShowing)
                    {
                        speechBubbleText.text = speechLines[speechIndex % speechLines.Length];
                        speechIndex++;
                        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(true);
                        speechShowing = true;
                        speechTimer = speechVisibleSeconds;
                    }
                    else
                    {
                        if (speechBubbleRoot != null) speechBubbleRoot.SetActive(false);
                        speechShowing = false;
                        speechTimer = speechIntervalSeconds;
                    }
                }
            }
        }
    }
}
