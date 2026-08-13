using UnityEngine;

namespace Pakettiporina
{
    // Ohjeet & palaute -paneeli (avataan Asetukset-paneelin sisalta). Sama Toggle/Open/Close
    // -kaava kuin SettingsManagerissa ja StickerPanelissa - paneeli piilotetaan alussa.
    public class InfoManager : MonoBehaviour
    {
        [Tooltip("Koko Ohjeet & palaute -paneeli. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        [Tooltip("Osoite johon 'Laheta palautetta' -nappi avaa sahkopostisovelluksen (mailto:...). " +
                 "VAIHDA TAMA OMAAN OSOITTEESEESI Inspectorissa tai alla olevaan oletusarvoon.")]
        public string feedbackEmail = "palaute@pakettiporina.fi";

        void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Open()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(!panelRoot.activeSelf);
        }

        // "Laheta palautetta" -napin OnClick. Avaa laitteen oman sahkopostisovelluksen
        // valmiiksi taytetylla vastaanottajalla ja aiheella.
        public void SendFeedback()
        {
            if (string.IsNullOrEmpty(feedbackEmail)) return;
            Application.OpenURL($"mailto:{feedbackEmail}?subject=Pakettiporina%20-%20palaute");
        }
    }
}
