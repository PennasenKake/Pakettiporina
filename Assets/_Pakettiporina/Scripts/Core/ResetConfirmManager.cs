using UnityEngine;

namespace Pakettiporina
{
    // "Nollaa edistyminen" -vahvistuspaneeli (M5.11, avataan Asetukset-paneelin
    // "Nollaa edistyminen" -rivilta). Kysyy varmistuksen ENNEN kuin SaveManager.ResetProgress()
    // kutsutaan - lapsi ei saa pystya poistamaan koko etenemista yhdella vahingossa-painalluksella.
    public class ResetConfirmManager : MonoBehaviour
    {
        [Tooltip("Koko vahvistuspaneeli. Piilotetaan automaattisesti alussa.")]
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

        // "Nollaa"-vahvistusnapin OnClick: poistaa oikeasti edistymisen ja sulkee paneelin.
        public void ConfirmReset()
        {
            if (SaveManager.Instance != null) SaveManager.Instance.ResetProgress();
            Close();
        }
    }
}
