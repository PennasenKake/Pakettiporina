using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // Asetuspaneeli (M5.2, uudistettu M5.11 13.8.2026 kayttajan referenssikuvien mukaan):
    // Musiikki/Aanet/Varahtely ovat nyt yksinkertaisia paalla/pois-liukukytkimia (ei liuku-
    // saatimia - referenssikuvassa ei ole voimakkuussaatoa, vain on/off). Ohjaus ja Ajonopeus
    // ovat kolmen napin segmentoituja valitsimia. Alaosassa kolme "Aikuiselle"-riviä jotka
    // avaavat omat alipaneelinsa (Tietoja pelista / Tietosuoja / Nollaa edistyminen).
    //
    // Sama "Toggle/Refresh"-kaava kuin StickerPanelissa: paneeli piilotetaan alussa ja
    // kontrollit paivitetaan nykyisista arvoista vasta kun paneeli avataan (ei Start()issa,
    // koska SaveManager.Load() saattaa asettaa taman hetkiset arvot vasta hieman myohemmin
    // samalla framella - Script Execution Order ei takaa jarjestysta kahden eri Start()in valilla).
    public class SettingsManager : MonoBehaviour
    {
        [Tooltip("Koko paneeli, joka avataan/suljetaan Asetukset-napista. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        [Tooltip("Valinnainen: sulkee paneelin (\"Valmis\")")]
        public Button closeButton;

        [Header("Musiikki/Aanet/Varahtely (paalla/pois-liukukytkimet, ks. Scripts/UI/ToggleSwitch.cs)")]
        public Toggle musicToggle;
        public Toggle sfxToggle;
        public Toggle vibrationToggle;

        [Tooltip("M5.15, 14.8.2026: 'Yotila'-kytkin. TOISTAISEKSI VAIN tallentaa mieltymyksen " +
            "(SaveManager.darkMode) - ei viela vaikuta mihinkaan visuaalisesti. Kayttajan pyynto: " +
            "pohja nyt, oikea tumma teema myohemmassa erassa.")]
        public Toggle darkModeToggle;

        [Header("Ohjaus (segmentoitu valitsin: Napit / Kallistus / Molemmat)")]
        public Image controlNapitBg;
        public Image controlKallistusBg;
        public Image controlMolemmatBg;

        [Header("Ajonopeus (segmentoitu valitsin: Hidas / Keski / Nopea)")]
        public Image speedHidasBg;
        public Image speedKeskiBg;
        public Image speedNopeaBg;

        [Header("Aikuiselle-osio")]
        [Tooltip("Tietoja pelista -paneeli, jonka \"Tietoja pelista\" -rivi avaa.")]
        public InfoManager infoManager;
        [Tooltip("Tietosuoja-paneeli, jonka \"Tietosuoja\" -rivi avaa.")]
        public PrivacyManager privacyManager;
        [Tooltip("\"Nollaa edistyminen\" -vahvistuspaneeli.")]
        public ResetConfirmManager resetConfirmManager;

        // Vari-vakiot segmentoitujen valitsimien tilan nakymiseen - kytketaan Editor-tyokalulla
        // (PakettiporinaSettingsSetup.cs) mutta pidetaan tassakin, jos joku haluaa kutsua
        // Refresh-metodeja suoraan koodista/testeista ilman Editor-tyokalua.
        public static readonly Color SegSelected = new Color(0.1607843f, 0.6f, 0.4392157f, 1f);   // vihrea, sama sävy kuin referenssikuvan "Molemmat"
        public static readonly Color SegUnselected = new Color(0.93f, 0.9f, 0.85f, 1f);            // vaalea, referenssikuvan valitsemattomat pillerit

        // Muistetaan viimeisimmat ei-nolla-arvot, jotta "paalla" palauttaa saman tason
        // mykistyksen jalkeen eika vain hyppaa oletusarvoon.
        float rememberedMusicVolume = 0.6f;
        float rememberedSfxVolume = 1f;

        void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // "Asetukset"-napin OnClick.
        public void Toggle()
        {
            if (panelRoot == null) return;
            bool show = !panelRoot.activeSelf;
            if (show) Refresh();
            else Close();
            panelRoot.SetActive(show);
        }

        public void Close()
        {
            // Tallennetaan levylle vasta kun paneeli suljetaan, ei jokaisella kytkimen
            // painalluksella - saastaa levy-IO:ta.
            if (SaveManager.Instance != null) SaveManager.Instance.Save();
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // HUOM: jos infoManager/privacyManager/resetConfirmManager -kytkenta on jostain syysta
        // jaanyt tyhjaksi (esim. Editor-tyokalu ajettu ennen paneelin luontia, tai scenea ei
        // tallennettu ajon jalkeen), etsitaan viittaus ajonaikaisesti FindObjectOfTypella sen
        // sijaan etta pudotaan hiljaa pois - ilman tata "Tietoja pelistä" nappi vain sulkee
        // Asetukset (Close() toimii) eika avaa mitaan, mika nayttaa kayttajalle silta etta
        // nappi "hyppaa paavalikkoon" vaikka oikeasti kyse on tyhjasta viittauksesta.
        public void OpenInfo()
        {
            Close();
            if (infoManager == null) infoManager = FindObjectOfType<InfoManager>(true);
            if (infoManager != null) infoManager.Open();
            else Debug.LogWarning("[Settings] InfoManageria ei loydy scenesta - aja Pakettiporina/Lisays/09 uudelleen ja tallenna scene.");
        }

        public void OpenPrivacy()
        {
            Close();
            if (privacyManager == null) privacyManager = FindObjectOfType<PrivacyManager>(true);
            if (privacyManager != null) privacyManager.Open();
            else Debug.LogWarning("[Settings] PrivacyManageria ei loydy scenesta - aja Pakettiporina/Lisays/09 uudelleen ja tallenna scene.");
        }

        public void OpenResetConfirm()
        {
            Close();
            if (resetConfirmManager == null) resetConfirmManager = FindObjectOfType<ResetConfirmManager>(true);
            if (resetConfirmManager != null) resetConfirmManager.Open();
            else Debug.LogWarning("[Settings] ResetConfirmManageria ei loydy scenesta - aja Pakettiporina/Lisays/09 uudelleen ja tallenna scene.");
        }

        void Refresh()
        {
            var audio = AudioManager.Instance;
            if (audio != null)
            {
                if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(audio.musicVolume > 0.001f);
                if (sfxToggle != null) sfxToggle.SetIsOnWithoutNotify(audio.sfxVolume > 0.001f);
                if (audio.musicVolume > 0.001f) rememberedMusicVolume = audio.musicVolume;
                if (audio.sfxVolume > 0.001f) rememberedSfxVolume = audio.sfxVolume;
            }

            var save = SaveManager.Instance;
            if (save != null)
            {
                if (vibrationToggle != null) vibrationToggle.SetIsOnWithoutNotify(save.vibrationOn);
                if (darkModeToggle != null) darkModeToggle.SetIsOnWithoutNotify(save.darkMode);
                RefreshControlButtons(save.controlMode);
                RefreshSpeedButtons(save.carSpeedTier);
            }
        }

        // --- Musiikki/Aanet/Varahtely (liukukytkinten OnValueChanged, kytketaan Editor-tyokalulla) ---

        public void OnMusicToggled(bool isOn)
        {
            var audio = AudioManager.Instance;
            if (audio == null) return;
            if (isOn)
            {
                audio.SetMusicVolume(rememberedMusicVolume);
            }
            else
            {
                if (audio.musicVolume > 0.001f) rememberedMusicVolume = audio.musicVolume;
                audio.SetMusicVolume(0f);
            }
        }

        public void OnSfxToggled(bool isOn)
        {
            var audio = AudioManager.Instance;
            if (audio == null) return;
            if (isOn)
            {
                audio.SetSfxVolume(rememberedSfxVolume);
                audio.PlayButtonClick(); // pieni valitön esikuuntelu, jotta lapsi kuulee heti etta aanet ovat paalla
            }
            else
            {
                if (audio.sfxVolume > 0.001f) rememberedSfxVolume = audio.sfxVolume;
                audio.SetSfxVolume(0f);
            }
        }

        public void OnVibrationToggled(bool isOn)
        {
            if (SaveManager.Instance == null) return;
            SaveManager.Instance.vibrationOn = isOn;
            if (isOn) VibrationManager.Vibrate(); // heti tuntuva kuittaus etta paalla
        }

        // M5.15, 14.8.2026: tallentaa VAIN mieltymyksen - ei viela mitaan visuaalista
        // vaikutusta (ks. kommentti darkModeToggle-kentan yllä).
        public void OnDarkModeToggled(bool isOn)
        {
            if (SaveManager.Instance == null) return;
            SaveManager.Instance.darkMode = isOn;
        }

        // --- Ohjaus (0=Napit, 1=Kallistus, 2=Molemmat) ---

        public void OnControlModeSelected(int mode)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.controlMode = mode;
                SaveManager.Instance.ApplyToScene();
            }
            RefreshControlButtons(mode);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }

        void RefreshControlButtons(int mode)
        {
            SetSeg(controlNapitBg, mode == 0);
            SetSeg(controlKallistusBg, mode == 1);
            SetSeg(controlMolemmatBg, mode == 2);
        }

        // --- Ajonopeus (0=Hidas, 1=Keski, 2=Nopea) ---

        public void OnCarSpeedSelected(int tier)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.carSpeedTier = tier;
                SaveManager.Instance.ApplyToScene();
            }
            RefreshSpeedButtons(tier);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }

        void RefreshSpeedButtons(int tier)
        {
            SetSeg(speedHidasBg, tier == 0);
            SetSeg(speedKeskiBg, tier == 1);
            SetSeg(speedNopeaBg, tier == 2);
        }

        static void SetSeg(Image img, bool on)
        {
            if (img != null) img.color = on ? SegSelected : SegUnselected;
        }
    }
}
