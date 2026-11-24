using UnityEngine;

namespace Pakettiporina
{
    // Tietosuoja-paneeli (M5.11, avataan Asetukset-paneelin "Tietosuoja"-rivilta). Sama
    // Open/Close/Toggle-kaava kuin InfoManagerissa - paneeli piilotetaan alussa. Ei omaa
    // dynaamista sisaltoa (teksti kytketaan Editor-tyokalulla PakettiporinaSettingsSetup.cs:ssa),
    // koska tietosuojaseloste ei muutu ajonaikaisesti.
    public class PrivacyManager : MonoBehaviour
    {
        [Tooltip("Koko Tietosuoja-paneeli. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        bool openRequested;

        void Start()
        {
            // KORJATTU 24.11.2026: komponentti on itse paneelin paalla, joten Start ajetaan vasta ensimmaisella
            // aktivoinnilla ja piilotti paneelin heti ensimmaisen Open()-kutsun jalkeen (ensimmainen avaus "ei tehnyt mitaan").
            if (panelRoot != null && !openRequested) panelRoot.SetActive(false);
        }

        public void Open()
        {
            openRequested = true;
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        public void Close()
        {
            bool wasOpen = panelRoot != null && panelRoot.activeSelf;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (wasOpen) SettingsManager.ReopenAfterSubpanel();   // takaisin Asetuksiin
        }

        public void Toggle()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(!panelRoot.activeSelf);
        }
    }
}
