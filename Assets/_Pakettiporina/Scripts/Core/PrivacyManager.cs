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
    }
}
