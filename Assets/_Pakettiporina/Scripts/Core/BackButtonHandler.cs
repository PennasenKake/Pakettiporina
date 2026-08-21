using UnityEngine;

namespace Pakettiporina
{
    // LISATTY 20.8.2026 (kayttaja: "Android-takaisin-nappi toimii vain radalla ajon
    // aikana... tama on todellinen kaytettavyysaukko juuri Android-julkaisua
    // ajatellen"): Androidin fyysinen takaisin-nappi (= KeyCode.Escape Unityssa) ei
    // reagoinut mihinkaan MainMenu-, Garage- tai Wash-scenessa.
    //
    // Ratascenet EIVAT tarvitse tata - PauseMenu.cs jo kuuntelee Escapea siella (avaa
    // tauon). Tama komponentti on VAIN MainMenu/Garage/Wash-sceneissa, jottei sama
    // Escape-painallus kasittelisi kahta eri asiaa samalla kertaa.
    //
    // Toiminta: etsii scenesta tunnetut paneelit (jotka kaikki noudattavat samaa
    // "panelRoot"-kaavaa - ks. SettingsManager/InfoManager/PrivacyManager/
    // ResetConfirmManager/StickerPanel) ja sulkee ENSIMMAISEN avoinna olevan,
    // SISALTA ULOSPAIN - esim. jos Tietoja-paneeli on auki Asetusten SISALLA, se
    // sulkeutuu ENSIN (palataan Asetuksiin), ei koko Asetuspaneelia kerralla samalla
    // painalluksella. Jos mikaan paneeli ei ole auki, siirrytaan valinnaiseen
    // fallbackScene-sceneen SceneFaderin kautta (esim. Garagesta MainMenuun) - tyhja
    // merkkijono = ei tee mitaan (esim. MainMenun juuresta tai Pesun kesken-tilasta ei
    // haluta paastaa pois vahingossa yhdella painalluksella).
    public class BackButtonHandler : MonoBehaviour
    {
        [Tooltip("Scene johon SceneFaderilla siirrytaan jos mikaan paneeli ei ollut auki. " +
                 "Tyhja = ei tee mitaan (esim. MainMenu, Wash).")]
        public string fallbackScene = "";

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (TryCloseTopPanel()) return;

            if (!string.IsNullOrEmpty(fallbackScene))
                SceneFader.LoadScene(fallbackScene);
        }

        bool TryCloseTopPanel()
        {
            // Jarjestys sisalta ulospain: Info/Privacy/ResetConfirm avataan Asetusten
            // SISALTA, joten ne tarkistetaan ENNEN itse Asetuspaneelia.
            var info = FindObjectOfType<InfoManager>(true);
            if (info != null && info.panelRoot != null && info.panelRoot.activeSelf)
            {
                info.Close();
                return true;
            }
            var privacy = FindObjectOfType<PrivacyManager>(true);
            if (privacy != null && privacy.panelRoot != null && privacy.panelRoot.activeSelf)
            {
                privacy.Close();
                return true;
            }
            var reset = FindObjectOfType<ResetConfirmManager>(true);
            if (reset != null && reset.panelRoot != null && reset.panelRoot.activeSelf)
            {
                reset.Close();
                return true;
            }
            var settings = FindObjectOfType<SettingsManager>(true);
            if (settings != null && settings.panelRoot != null && settings.panelRoot.activeSelf)
            {
                settings.Close();
                return true;
            }
            var sticker = FindObjectOfType<StickerPanel>(true);
            if (sticker != null && sticker.panelRoot != null && sticker.panelRoot.activeSelf)
            {
                // StickerPanelilla ei ole omaa Close()-metodia, vain Toggle() - kutsuttaisiin
                // sita nyt UUDESTAAN AUKI koska se togglaisi - siksi suoraan SetActive(false).
                sticker.panelRoot.SetActive(false);
                return true;
            }
            var garage = FindObjectOfType<GarageScreen>(true);
            if (garage != null && garage.statsPanel != null && garage.statsPanel.activeSelf)
            {
                garage.statsPanel.SetActive(false);
                return true;
            }
            return false;
        }
    }
}
