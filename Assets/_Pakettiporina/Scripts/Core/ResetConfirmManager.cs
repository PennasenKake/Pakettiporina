using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // "Nollaa edistyminen" -vahvistuspaneeli (M5.11, avataan Asetukset-paneelin
    // "Nollaa edistyminen" -rivilta). Kysyy varmistuksen ENNEN kuin SaveManager.ResetProgress()
    // kutsutaan - lapsi ei saa pystya poistamaan koko etenemista yhdella vahingossa-painalluksella.
    // PAIVITETTY 23.11.2026: tumma himmennys paneelin taakse (tausta oli lapinakyva) ja
    // "vanhempainportti": ensimmainen Nollaa-painallus vain virittaa napin ("Varmista"),
    // toinen painallus 3 s sisalla nollaa.
    public class ResetConfirmManager : MonoBehaviour
    {
        [Tooltip("Koko vahvistuspaneeli. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        const float ArmSeconds = 3f;
        GameObject scrim;
        float armedUntil;
        TMP_Text confirmLabel;
        string originalLabel;

        bool openRequested;

        void Start()
        {
            // KORJATTU 23.11.2026: Start ajetaan vasta ensimmaisella aktivoinnilla, joten se piilotti
            // dialogin heti ensimmaisen Open()-kutsun jalkeen (ensimmainen Nollaa-painallus "ei tehnyt mitaan").
            if (panelRoot != null && !openRequested) panelRoot.SetActive(false);
        }

        // Scrim sidotaan paneelin aktivointiin (tama komponentti on itse paneelin paalla), jotta
        // himmennys ei koskaan jaa paalle ilman dialogia (korjaus: scrim esti kaikki napit).
        void OnEnable()
        {
            EnsureScrim();
            if (scrim != null) scrim.SetActive(true);
            transform.SetAsLastSibling(); // paneeli scrimin ylapuolelle
        }

        void OnDisable()
        {
            if (scrim != null) scrim.SetActive(false);
        }

        void OnDestroy()
        {
            if (scrim != null) Destroy(scrim);
        }

        public void Open()
        {
            openRequested = true;
            if (panelRoot != null) panelRoot.SetActive(true);
            Disarm();
        }

        public void Close()
        {
            Disarm();
            bool wasOpen = panelRoot != null && panelRoot.activeSelf;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (wasOpen) SettingsManager.ReopenAfterSubpanel();   // takaisin Asetuksiin
        }

        void Update()
        {
            if (armedUntil > 0f && Time.unscaledTime > armedUntil) Disarm();
        }

        // "Nollaa"-vahvistusnapin OnClick: ensin viritys, sitten oikea nollaus.
        public void ConfirmReset()
        {
            if (armedUntil <= 0f)
            {
                Arm();
                return;
            }
            if (SaveManager.Instance != null) SaveManager.Instance.ResetProgress();
            Close();
        }

        void Arm()
        {
            armedUntil = Time.unscaledTime + ArmSeconds;
            Juice.Play("arm", 0f);
            FindConfirmLabel();
            if (confirmLabel != null) confirmLabel.text = "Varmista";
        }

        void Disarm()
        {
            armedUntil = 0f;
            if (confirmLabel != null && originalLabel != null) confirmLabel.text = originalLabel;
        }

        void FindConfirmLabel()
        {
            if (confirmLabel != null || panelRoot == null) return;
            foreach (var t in panelRoot.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.text == "Nollaa") { confirmLabel = t; originalLabel = t.text; break; }
            }
        }

        void EnsureScrim()
        {
            if (scrim != null) return;
            var parent = transform.parent as RectTransform;
            if (parent == null) return;
            scrim = new GameObject("ResetScrim", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)scrim.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-3000, -3000); rt.offsetMax = new Vector2(3000, 3000);
            var img = scrim.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);
            img.raycastTarget = true; // estaa taustan napit
            scrim.transform.SetSiblingIndex(transform.GetSiblingIndex());
        }
    }
}
