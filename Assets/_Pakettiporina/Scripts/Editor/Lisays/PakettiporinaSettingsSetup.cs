#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

namespace Pakettiporina.EditorTools
{
    // Rakentaa/paivittaa Asetukset-napin ja koko asetuskokonaisuuden paavalikkoon (M5.2,
    // uudistettu M5.11 13.8.2026 kayttajan referenssikuvien mukaan, KORJATTU M5.12 13.8.2026
    // - ks. alla "SCAALAUSKORJAUS"). AJA MainMenu-scene AUKI.
    //
    // SCAALAUSKORJAUS (M5.12, 13.8.2026): ensimmainen versio taman tyokalun uudesta
    // ulkoasusta kayttii 900-1550 yksikon KIINTEITA, KESKITETTYJA paneeleja. Tama nayttti
    // hyvalta Editorin Scene-nakymassa mutta LEIKKAUTUI oikeasti pelin Game-nakymassa, koska:
    //   1. Canvasin CanvasScaler.referenceResolution on vain 800x600 (Scale With Screen Size,
    //      Match Width Or Height 0.5) - 900 yksikon leveys ylittaa jo referenssin, ja
    //      pystyssa (Portrait) todellinen kayttoleveys on scaler-matematiikan mukaan lahempana
    //      ~500-550 yksikkoa, ei 900.
    //   2. StickerPanel (jo olemassa oleva, TOIMIVA paneeli samassa scenessa) osoittaa oikean
    //      ratkaisun: paneelin TAUSTA on KOKO NAYTON tayttava (anchorMin 0,0 / anchorMax 1,1),
    //      ja sisalto joka ei mahdu yhdelle ruudulle (StickerPanelissa tarrarudukko) on OMAN
    //      "Scroll View" -objektin sisalla (ScrollRect + Viewport/RectMask2D + Content +
    //      VerticalLayoutGroup + ContentSizeFitter) - vain otsikko/sulkunappi ovat kiintopisteina
    //      ruudun ylä-/alareunassa scrollin ULKOPUOLELLA. Talla PALJON sisaltoa mahtuu MILLE
    //      TAHANSA nayttokoolle ilman etta mitaan jaa leikkautumaan pois nakyvista.
    // Tama tiedosto seuraa nyt TASMALLEEN samaa StickerPanel-kaavaa: paneelin tausta taytta
    // koko ruudun, otsikko/Valmis(Takaisin) ovat kiinnitettyina ylos/alas, ja rivit ovat
    // vierittyvassa Content-alueessa siltä varalta etta ne eivat mahtuisi yhdelle ruudulle.
    //
    // Rakentaa/paivittaa:
    //   SettingsButton   - avausnappi (kayttaa OLEMASSA OLEVAA "SetstButton"-objektia jos
    //                      loytyy, koska kayttaja pyysi sen uudelleenkayttoa uuden erillisen
    //                      napin sijaan - HUOM: SetstButton oli aiemmin kytketty StickerPanelin
    //                      avaamiseen, mika oli PAALLEKKAINEN Sticker_Points_Buttonin kanssa,
    //                      joten uudelleenkaytto ei riko tarrojen avaamista)
    //   SettingsPanel    - SettingsManager: koko naytto taytava tausta, kiintea otsikko+Valmis,
    //                      vierittyva sisalto valissa
    //   InfoPanel        - InfoManager
    //   PrivacyPanel     - PrivacyManager
    //   ResetConfirmPanel- ResetConfirmManager (pieni kiintokokoinen modaali, ei vierita)
    public static class PakettiporinaSettingsSetup
    {
        const string BTN_NAME = "SettingsButton";
        const string LEGACY_BTN_NAME = "SetstButton"; // kayttajan olemassa oleva nappi, uudelleenkaytetaan
        const string PANEL_NAME = "SettingsPanel";
        const string ICON_DIR = "Assets/_Pakettiporina/Art/UI/Icons";
        const string ART_UI_DIR = "Assets/_Pakettiporina/Art/UI";

        const float ROW_W = 460f; // rivien/korttien kiintea leveys - turvallinen 800x600-referenssilla myos pystyssa

        // ---------- Vari-paletti (Asetukset/Tietoja-paneilien OMA tyyli, ks. luokkakommentti) ----------
        static readonly Color SettingsBg = new Color(0.9686f, 0.898f, 0.788f, 1f);       // lampimanhiekka tausta
        static readonly Color CardWhite = new Color(1f, 1f, 1f, 0.97f);                   // valkoinen rivikortti
        static readonly Color TitlePill = new Color(0.9294f, 0.6196f, 0.1725f, 1f);       // oranssi otsikkopilleri
        static readonly Color TextDark = new Color(0.24f, 0.18f, 0.12f, 1f);              // tumma teksti valkoisella kortilla
        static readonly Color TextMuted = new Color(0.5f, 0.45f, 0.38f, 1f);              // harmaahko toissijainen teksti
        static readonly Color ToggleOn = new Color(0.298f, 0.686f, 0.314f, 1f);           // vihrea (paalla)
        static readonly Color ToggleOff = new Color(0.82f, 0.81f, 0.79f, 1f);             // vaaleanharmaa (pois)
        static readonly Color SegSelected = SettingsManager.SegSelected;                  // vihrea valittu segmentti
        static readonly Color SegUnselected = SettingsManager.SegUnselected;              // vaalea valitsematon segmentti
        static readonly Color BadgeOrange = new Color(0.9412f, 0.6196f, 0.1294f, 1f);
        static readonly Color BadgeBlue = new Color(0.2157f, 0.5412f, 0.8667f, 1f);
        static readonly Color BadgePurple = new Color(0.5804f, 0.349f, 0.851f, 1f);
        static readonly Color BadgeRed = new Color(0.8863f, 0.2941f, 0.2902f, 1f);
        static readonly Color BadgeGreen = new Color(0.2f, 0.6f, 0.4f, 1f);
        static readonly Color BadgeNavy = new Color(0.2196f, 0.2627f, 0.4157f, 1f);   // "Yotila"-kytkimen badge
        static readonly Color ChevronGray = new Color(0.6f, 0.56f, 0.5f, 1f);
        static readonly Color ValmisGreen = new Color(0.298f, 0.686f, 0.314f, 1f);
        static readonly Color DividerColor = new Color(0.85f, 0.78f, 0.66f, 1f);
        static readonly Color CoralRed = new Color(0.8863f, 0.2941f, 0.2902f, 1f);
        static readonly Color ScrollMaskTint = new Color(1f, 1f, 1f, 0.02f);

        const string PrivacyBodyText =
            "Pakettiporina ei kerää tietoja sinusta.\n\n" +
            "<b>Ei mainoksia</b> - pelissä ei näytetä yhtään mainosta.\n\n" +
            "<b>Ei datankeruuta</b> - peli ei lähetä mitään tietoja internetiin eikä seuraa pelaamista.\n\n" +
            "<b>Toimii ilman nettiä</b> - kaikki tallennukset (pisteet, autosi osat, tarrat) säilyvät " +
            "vain tässä laitteessa, ei missään palvelimella.\n\n" +
            "<b>Turvallinen lapsille</b> - ei osto- tai chat-toimintoja. Ainoa nettiyhteyttä käyttävä " +
            "toiminto on \"Tietoja\"-paneelin palautekortti, joka avaa laitteen omaa sähköpostisovellusta " +
            "vasta kun sitä erikseen napautetaan.";

        [MenuItem("Pakettiporina/Lisays/09 - Lisaa/paivita asetuspaneeli paavalikkoon")]
        public static void AddSettings()
        {
            var s = new StringBuilder();
            s.AppendLine("=== ASETUSPANEELI PAAVALIKKOON (M5.12 - scaalauskorjaus) ===");

            string sceneName = EditorSceneManager.GetActiveScene().name;
            if (sceneName != "MainMenu")
                s.AppendLine($"HUOM: aktiivinen scene on '{sceneName}', ei 'MainMenu' - avaa MainMenu-scene jos paneeli ei nay oikeassa paikassa.");

            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                s.AppendLine("VIRHE: scenesta ei loydy Canvasia.");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- Ikonit ----------
            var gearIcon = LoadIcon("pakettiporina_icon_ratas");
            var noteIcon = LoadIcon("pakettiporina_icon_nuotti");
            var speakerOnIcon = LoadIcon("pakettiporina_icon_speaker_on");
            var toggleTrackIcon = LoadIcon("pakettiporina_icon_toggle_track");
            var toggleKnobIcon = LoadIcon("pakettiporina_icon_radio_dot");
            var infoIcon = LoadIcon("pakettiporina_icon_info");
            var kirjeIcon = LoadIcon("pakettiporina_icon_kirje");
            var varahtelyIcon = LoadIcon("pakettiporina_icon_varahtely");
            var ohjausIcon = LoadIcon("pakettiporina_icon_ohjaus");
            var nopeusIcon = LoadIcon("pakettiporina_icon_nopeus");
            var kilpiIcon = LoadIcon("pakettiporina_icon_kilpi");
            var checkIcon = LoadIcon("pakettiporina_icon_check");
            var nuoliIcon = LoadIcon("pakettiporina_icon_nuoli_oikea");
            var lukkoIcon = LoadIcon("pakettiporina_icon_lukko");
            var uudestaanIcon = LoadIcon("pakettiporina_ikoni_uudestaan_1");
            var suljeIcon = LoadIcon("pakettiporina_icon_sulje");
            // M5.13, 13.8.2026 (kayttajan pyynto): paluu-ikoninapeissa (Settings/Info/Privacy)
            // kaytetaan tata "uloskirjautumis"-tyylista ikonia sulkeIconin sijaan, koko 60x60
            // eli sama kuin itse napin koko - ei erillista pienempaa ikonia napin sisalla.
            var lopetaIcon = LoadIcon("pakettiporina_ikoni_lopeta");
            // M5.15, 14.8.2026: "Yotila"-kytkimen ikoni (kuunsirppi) - ks. kayttajan pyynto
            // "pohja, mutta ei vieda viela loppuun asti" SettingsManager.darkModeToggle-kommentissa.
            var yoIcon = LoadIcon("pakettiporina_icon_yo");
            var mascotSprite = LoadSprite($"{ART_UI_DIR}/pakettiporina_vilkku_hahmo.png");
            var roundedSprite = LoadSlicedSprite("pakettiporina_ui_rounded", 30f);
            var circleSprite = LoadIcon("pakettiporina_ui_circle");

            if (gearIcon == null || noteIcon == null || speakerOnIcon == null || toggleTrackIcon == null
                || toggleKnobIcon == null || infoIcon == null || kirjeIcon == null
                || varahtelyIcon == null || ohjausIcon == null || nopeusIcon == null || kilpiIcon == null
                || checkIcon == null || nuoliIcon == null || lukkoIcon == null || uudestaanIcon == null
                || suljeIcon == null || lopetaIcon == null || yoIcon == null || mascotSprite == null || roundedSprite == null)
                s.AppendLine("VAROITUS: jokin ikoni/sprite puuttuu - tarkista Art/UI/Icons-kansio.");

            // ================= ASETUKSET-PANEELI =================
            var panelGo = Find(PANEL_NAME);
            bool panelIsNew = panelGo == null;
            if (panelIsNew)
            {
                panelGo = new GameObject(PANEL_NAME, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(panelGo, "Luo " + PANEL_NAME);
                panelGo.transform.SetParent(canvas.transform, false);
                panelGo.AddComponent<Image>();
            }
            MakeFullStretch((RectTransform)panelGo.transform);
            var panelBg = panelGo.GetComponent<Image>();
            if (panelBg == null) panelBg = panelGo.AddComponent<Image>();
            panelBg.sprite = null;
            panelBg.color = SettingsBg;

            var settings = panelGo.GetComponent<SettingsManager>();
            if (settings == null) settings = panelGo.AddComponent<SettingsManager>();
            settings.panelRoot = panelGo;

            // Otsikko - KIINTEA ylareunaan (ei vieri sisallon mukana). Kavennettu 300->270
            // (M5.14, 13.8.2026) jotta maskotti-avatarille jaa tilaa oikeaan ylakulmaan
            // paluu-ikonin vieressa, kayttajan referenssikuvan mukaisesti.
            var titleCard = MakeTopAnchoredCard(panelGo.transform, "TitleCard", 24, new Vector2(270, 84), TitlePill, roundedSprite);
            MakeLabel(titleCard.transform, "Title", "Asetukset", 44, Vector2.zero,
                new Vector2(250, 74), TextAlignmentOptions.Center, Color.white);

            // M5.13, 13.8.2026 (kayttajan pyynto): iso "Valmis"-tekstinappi alhaalta pois -
            // paluu edelliseen nakymaan hoituu pienella ikoninapilla oikeassa ylakulmassa,
            // sama tuttu kaava kuin useimmissa mobiilisovelluksissa.
            RemoveIfExists(panelGo.transform, "SettingsCloseButton");
            var backBtn = MakeBackIconButton(panelGo.transform, "SettingsBackButton", circleSprite, lopetaIcon);
            settings.closeButton = backBtn;
            ClearClicks(backBtn);
            UnityEventTools.AddPersistentListener(backBtn.onClick, settings.Close);

            // M5.14, 13.8.2026 (kayttajan pyynto): paluu-ikoni pysyy, mutta lisataan myos
            // referenssikuvan koristeellinen maskotti-avatar oikeaan ylakulmaan, paluu-ikonin
            // VASEMMALLE puolelle (ei klikattava - pelkka koristekuva).
            MakeCornerMascot(panelGo.transform, "SettingsMascot", mascotSprite, -88, -20, 44);

            // Poista M5.12:ta EDELTAVAN (ei-vierittyvan) version rivit, jotka olivat panelGo:n
            // SUORIA lapsia - uudet versiot rakennetaan alla Content:n (vierityksen) sisaan, ja
            // ilman tata siivousta vanhat jaisivat nakymattomana mutta turhana kahdennuksena
            // paneelin taakse (sama nimi, vaara vanhempi - MakeCard ei loytaisi/paivittaisi niita).
            foreach (var oldName in new[] { "MusicCard", "SfxCard", "VibrationCard", "ControlCard", "SpeedCard" })
                RemoveIfExists(panelGo.transform, oldName);

            // Vierittyva sisaltoalue Titlen ja alareunan valissa. Alareunan tila pienennetty
            // M5.13:ssa, koska iso "Valmis"-nappi ei ole enaa siella (ks. yla, paluu-ikoni).
            var content = BuildScrollArea(panelGo.transform, "SettingsScroll", 128, 32, 8);

            // ---------- Musiikki / Aanet / Varahtely (paalla/pois-liukukytkimet) ----------
            var musicToggle = BuildToggleRow(content, "Music", noteIcon, BadgeOrange, "Musiikki",
                roundedSprite, circleSprite, toggleTrackIcon, toggleKnobIcon, ToggleOn, ToggleOff, ref s);
            settings.musicToggle = musicToggle;
            WireToggleBool(musicToggle, settings.OnMusicToggled);
            musicToggle.SetIsOnWithoutNotify(true);

            var sfxToggle = BuildToggleRow(content, "Sfx", speakerOnIcon, BadgeBlue, "Äänet",
                roundedSprite, circleSprite, toggleTrackIcon, toggleKnobIcon, ToggleOn, ToggleOff, ref s);
            settings.sfxToggle = sfxToggle;
            WireToggleBool(sfxToggle, settings.OnSfxToggled);
            sfxToggle.SetIsOnWithoutNotify(true);

            var vibrationToggle = BuildToggleRow(content, "Vibration", varahtelyIcon, BadgePurple, "Värähtely",
                roundedSprite, circleSprite, toggleTrackIcon, toggleKnobIcon, ToggleOn, ToggleOff, ref s);
            settings.vibrationToggle = vibrationToggle;
            WireToggleBool(vibrationToggle, settings.OnVibrationToggled);
            vibrationToggle.SetIsOnWithoutNotify(true);

            // M5.15, 14.8.2026 (kayttajan pyynto: "pohja, mutta ei vieda viela loppuun asti"):
            // "Yotila"-kytkin tallentaa mieltymyksen (SaveManager.darkMode), mutta ei VIELA
            // muuta minkaan paneelin vareja - oikea tumma teema on oma myohempi tehtavansa.
            var darkModeToggle = BuildToggleRow(content, "DarkMode", yoIcon, BadgeNavy, "Yötila",
                roundedSprite, circleSprite, toggleTrackIcon, toggleKnobIcon, ToggleOn, ToggleOff, ref s);
            settings.darkModeToggle = darkModeToggle;
            WireToggleBool(darkModeToggle, settings.OnDarkModeToggled);
            darkModeToggle.SetIsOnWithoutNotify(false);

            // ---------- Ohjaus (segmentoitu: Napit / Kallistus / Molemmat) ----------
            var controlCard = MakeCard(content, "ControlCard", Vector2.zero, new Vector2(ROW_W, 150), CardWhite, roundedSprite);
            MakeBadgedIcon(controlCard.transform, "Icon", ohjausIcon, circleSprite, new Vector2(-188, 38), 40, BadgeRed);
            MakeLabel(controlCard.transform, "Label", "Ohjaus", 30, new Vector2(-28, 38),
                new Vector2(210, 50), TextAlignmentOptions.Left, TextDark);
            var (controlNapitBtn, controlNapitBg) = MakeSegButton(controlCard.transform, "Napit", new Vector2(-144, -34), roundedSprite);
            var (controlKallistusBtn, controlKallistusBg) = MakeSegButton(controlCard.transform, "Kallistus", new Vector2(0, -34), roundedSprite);
            var (controlMolemmatBtn, controlMolemmatBg) = MakeSegButton(controlCard.transform, "Molemmat", new Vector2(144, -34), roundedSprite);
            settings.controlNapitBg = controlNapitBg;
            settings.controlKallistusBg = controlKallistusBg;
            settings.controlMolemmatBg = controlMolemmatBg;
            WireIntButton(controlNapitBtn, settings.OnControlModeSelected, 0);
            WireIntButton(controlKallistusBtn, settings.OnControlModeSelected, 1);
            WireIntButton(controlMolemmatBtn, settings.OnControlModeSelected, 2);

            // ---------- Ajonopeus (segmentoitu: Hidas / Keski / Nopea) ----------
            var speedCard = MakeCard(content, "SpeedCard", Vector2.zero, new Vector2(ROW_W, 150), CardWhite, roundedSprite);
            MakeBadgedIcon(speedCard.transform, "Icon", nopeusIcon, circleSprite, new Vector2(-188, 38), 40, BadgeOrange);
            MakeLabel(speedCard.transform, "Label", "Ajonopeus", 30, new Vector2(-28, 38),
                new Vector2(210, 50), TextAlignmentOptions.Left, TextDark);
            var (speedHidasBtn, speedHidasBg) = MakeSegButton(speedCard.transform, "Hidas", new Vector2(-144, -34), roundedSprite);
            var (speedKeskiBtn, speedKeskiBg) = MakeSegButton(speedCard.transform, "Keski", new Vector2(0, -34), roundedSprite);
            var (speedNopeaBtn, speedNopeaBg) = MakeSegButton(speedCard.transform, "Nopea", new Vector2(144, -34), roundedSprite);
            settings.speedHidasBg = speedHidasBg;
            settings.speedKeskiBg = speedKeskiBg;
            settings.speedNopeaBg = speedNopeaBg;
            WireIntButton(speedHidasBtn, settings.OnCarSpeedSelected, 0);
            WireIntButton(speedKeskiBtn, settings.OnCarSpeedSelected, 1);
            WireIntButton(speedNopeaBtn, settings.OnCarSpeedSelected, 2);

            // ---------- Erotinviiva + "Aikuiselle" ----------
            MakeRowSpacer(content, "Divider", new Vector2(420, 3), DividerColor);

            var aikuiselleGo = MakeRowContainer(content, "AikuiselleRow", new Vector2(300, 40));
            MakeIcon(aikuiselleGo.transform, "Lock", lukkoIcon, new Vector2(-140, 0), new Vector2(20, 20), TextMuted);
            // Kayttajan pyynto M5.14: fonttikoko 32 ja lihavoitu (oli 24, normaali).
            var aikuiselleLabel = MakeLabel(aikuiselleGo.transform, "Label", "Aikuiselle", 32, new Vector2(10, 0),
                new Vector2(260, 40), TextAlignmentOptions.Left, TextMuted);
            aikuiselleLabel.fontStyle = FontStyles.Bold;

            // ---------- Tietoja pelistä / Tietosuoja / Nollaa edistyminen (chevron-rivit) ----------
            var infoRowBtn = BuildChevronRow(content, "SettingsInfoButton", infoIcon, BadgeGreen,
                "Tietoja pelistä", nuoliIcon, roundedSprite, circleSprite, ref s);
            ClearClicks(infoRowBtn);
            UnityEventTools.AddPersistentListener(infoRowBtn.onClick, settings.OpenInfo);

            var privacyRowBtn = BuildChevronRow(content, "SettingsPrivacyButton", kilpiIcon, BadgeBlue,
                "Tietosuoja", nuoliIcon, roundedSprite, circleSprite, ref s);
            ClearClicks(privacyRowBtn);
            UnityEventTools.AddPersistentListener(privacyRowBtn.onClick, settings.OpenPrivacy);

            var resetRowBtn = BuildChevronRow(content, "SettingsResetButton", uudestaanIcon, BadgeRed,
                "Nollaa edistyminen", nuoliIcon, roundedSprite, circleSprite, ref s);
            ClearClicks(resetRowBtn);
            UnityEventTools.AddPersistentListener(resetRowBtn.onClick, settings.OpenResetConfirm);

            // Poista vanhat, jo korvatut objektit jos jaljella aiemmasta ajosta.
            // M5.14, 14.8.2026 (kayttajan loytama bugi): hyvin vanhat "SettingsInfoButton" ym.
            // -chevron-rivit olivat AIKOINAAN panelGo:n SUORIA lapsia (ennen scroll-arkkitehtuuria).
            // Ne jaivat scenen ensimmaisilta ajoilta hierarkiaan, ja koska ne ovat panelGo:n
            // suoria lapsia (Content on eri paikassa), vanhaa FindChild-siivousta ei ollut viela
            // niille - "sotkivat" nakymaa/klikkauksia paallekkain uusien Content:n sisalla
            // olevien versioiden kanssa. FindChild etsii VAIN suoria lapsia, joten tama ei
            // poista Content:n sisalla olevia oikeita rivien nappeja.
            RemoveIfExists(panelGo.transform, "SettingsInfoButton");
            RemoveIfExists(panelGo.transform, "SettingsPrivacyButton");
            RemoveIfExists(panelGo.transform, "SettingsResetButton");
            RemoveIfExists(panelGo.transform, "MusicIcon"); RemoveIfExists(panelGo.transform, "MusicLabel"); RemoveIfExists(panelGo.transform, "MusicSlider");
            RemoveIfExists(panelGo.transform, "SfxIcon"); RemoveIfExists(panelGo.transform, "SfxLabel"); RemoveIfExists(panelGo.transform, "SfxSlider");
            RemoveIfExists(panelGo.transform, "SoundIcon"); RemoveIfExists(panelGo.transform, "SoundLabel"); RemoveIfExists(panelGo.transform, "SoundToggle");
            RemoveIfExists(panelGo.transform, "Divider"); // vanha versio oli suoraan panelGo:n lapsi, uusi on Content:n sisalla
            RemoveIfExists(panelGo.transform, "AikuiselleLock"); RemoveIfExists(panelGo.transform, "AikuiselleLabel");

            panelGo.SetActive(false); // piilossa oletuksena, Toggle() nayttaa
            EditorUtility.SetDirty(settings);

            // ================= TIETOJA-PANEELI =================
            BuildInfoPanel(canvas.transform, settings, mascotSprite, roundedSprite, checkIcon, circleSprite,
                lopetaIcon, kirjeIcon, infoIcon, ref s);

            // ================= TIETOSUOJA-PANEELI =================
            BuildPrivacyPanel(canvas.transform, settings, roundedSprite, kilpiIcon, circleSprite, lopetaIcon, ref s);

            // ================= NOLLAA EDISTYMINEN -VAHVISTUS =================
            BuildResetConfirmPanel(canvas.transform, settings, roundedSprite, circleSprite, uudestaanIcon, ref s);

            // ---------- Avausnappi: uudelleenkaytetaan olemassa olevaa "SetstButton"-objektia ----------
            var legacyBtnGo = Find(LEGACY_BTN_NAME);
            Button openBtn;
            GameObject btnGo;
            if (legacyBtnGo != null)
            {
                btnGo = legacyBtnGo;
                openBtn = btnGo.GetComponent<Button>();
                if (openBtn == null) { var oimg = btnGo.AddComponent<Image>(); openBtn = btnGo.AddComponent<Button>(); openBtn.targetGraphic = oimg; }
                // HUOM: tama nappi oli aiemmin kytketty StickerPanel.Togglelle (paallekkainen
                // Sticker_Points_Buttonin kanssa) - kayttaja pyysi uudelleenkayttoa Asetuksille.
                // Tarrat pysyvat silti avattavissa Sticker_Points_Buttonista, joten mitaan
                // toiminnallisuutta ei haviä.
                s.AppendLine("SetstButton uudelleenkaytetty: avaa nyt Asetukset (oli ennen StickerPanel.Toggle, paallekkainen Sticker_Points_Buttonin kanssa).");
                RemoveIfExists(btnGo.transform, "Image"); // vanha yleisnimen ikonilapsi -> korvataan omalla "Icon"-lapsella
            }
            else
            {
                btnGo = Find(BTN_NAME);
                bool btnIsNew = btnGo == null;
                if (btnIsNew)
                {
                    btnGo = new GameObject(BTN_NAME, typeof(RectTransform));
                    Undo.RegisterCreatedObjectUndo(btnGo, "Luo " + BTN_NAME);
                    btnGo.transform.SetParent(canvas.transform, false);
                    var brt = (RectTransform)btnGo.transform;
                    brt.anchorMin = brt.anchorMax = new Vector2(1f, 1f);
                    brt.pivot = new Vector2(1f, 1f);
                    brt.anchoredPosition = new Vector2(-20, -40);
                    brt.sizeDelta = new Vector2(80, 80);
                    var img = btnGo.AddComponent<Image>();
                    openBtn = btnGo.AddComponent<Button>();
                    openBtn.targetGraphic = img;
                    s.AppendLine("SetstButtonia ei loytynyt - luotiin uusi SettingsButton paavalikkoon.");
                }
                else
                {
                    openBtn = btnGo.GetComponent<Button>();
                }
            }
            var openImg = btnGo.GetComponent<Image>();
            if (openImg != null) { openImg.sprite = null; openImg.color = new Color(1f, 1f, 1f, 0f); }
            var oldLabel = FindChild(btnGo.transform, "Label");
            if (oldLabel != null && oldLabel.GetComponent<TextMeshProUGUI>() != null)
                Object.DestroyImmediate(oldLabel.gameObject);
            MakeIcon(btnGo.transform, "Icon", gearIcon, Vector2.zero, new Vector2(44, 44), Color.white);

            if (openBtn != null)
            {
                ClearClicks(openBtn);
                UnityEventTools.AddPersistentListener(openBtn.onClick, settings.Toggle);
                s.AppendLine("Asetukset-avausnapin OnClick -> SettingsManager.Toggle() kytketty.");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            s.AppendLine(panelIsNew ? "\nAsetuskokonaisuus luotu." : "\nAsetuskokonaisuuden kytkennat paivitetty.");
            Debug.Log(s.ToString());
        }

        // ================= Rakenneapufunktiot: vierityskehys (StickerPanelin "Scroll View" -kaava) =================

        // Rakentaa/paivittaa ScrollRect+Viewport(RectMask2D)+Content(VerticalLayoutGroup+
        // ContentSizeFitter) -rakenteen sailiona vieritettavalle sisallolle. topInset/bottomInset
        // jattavat tilaa kiinteille otsikko/nappi-elementeille jotka ovat TAMAN alueen ULKOPUOLELLA.
        // Palauttaa Content-transformin, johon rivit lisataan lapsiksi.
        static Transform BuildScrollArea(Transform parent, string name, float topInset, float bottomInset, float sideInset)
        {
            var existing = FindChild(parent, name);
            GameObject scrollGo = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) scrollGo.transform.SetParent(parent, false);
            var srt = (RectTransform)scrollGo.transform;
            srt.anchorMin = new Vector2(0f, 0f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.offsetMin = new Vector2(sideInset, bottomInset);
            srt.offsetMax = new Vector2(-sideInset, -topInset);

            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 22f;

            var viewportT = FindChild(scrollGo.transform, "Viewport");
            GameObject viewportGo = viewportT != null ? viewportT.gameObject : new GameObject("Viewport", typeof(RectTransform));
            if (viewportT == null) viewportGo.transform.SetParent(scrollGo.transform, false);
            var vrt = (RectTransform)viewportGo.transform;
            vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero; vrt.offsetMax = Vector2.zero;
            var mask = viewportGo.GetComponent<RectMask2D>();
            if (mask == null) mask = viewportGo.AddComponent<RectMask2D>();

            var contentT = FindChild(viewportGo.transform, "Content");
            GameObject contentGo = contentT != null ? contentT.gameObject : new GameObject("Content", typeof(RectTransform));
            if (contentT == null) contentGo.transform.SetParent(viewportGo.transform, false);
            var crt = (RectTransform)contentGo.transform;
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0f, crt.sizeDelta.y);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 16f;
            vlg.padding = new RectOffset(0, 0, 6, 6);
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vrt;
            scrollRect.content = crt;

            return contentGo.transform;
        }

        // Pieni tumma pyorea "paluu"-nappi oikeaan ylakulmaan (M5.13, 13.8.2026: kayttaja pyysi
        // etta paneelin sulkeminen/edelliseen nakymaan palaaminen hoituu ikonilla ylakulmassa
        // ison Valmis/Takaisin-tekstinapin sijaan - sama tuttu mobiilisovellusten kaava).
        static Button MakeBackIconButton(Transform parent, string name, Sprite circleSprite, Sprite xIcon)
        {
            var existing = FindChild(parent, name);
            bool isNew = existing == null;
            GameObject go = isNew ? new GameObject(name, typeof(RectTransform)) : existing.gameObject;
            if (isNew)
            {
                Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
                go.transform.SetParent(parent, false);
            }
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20, -20);
            rt.sizeDelta = new Vector2(60, 60);
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = new Color(0.15f, 0.13f, 0.11f, 0.55f);
            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            // Ikoni taytta koko 60x60-napin (kayttajan mittaama arvo Inspectorista M5.13:ssa) -
            // ei pientä ikonia ison napin sisalla.
            MakeIcon(go.transform, "Icon", xIcon, Vector2.zero, new Vector2(60, 60), Color.white);
            return btn;
        }

        // Koristeellinen, EI-klikattava maskotti-avatar oikeaan ylakulmaan (M5.14, 13.8.2026,
        // kayttajan referenssikuvan mukaan). rightOffset/topOffset ovat pivot(1,1)-ankkuroidun
        // RectTransformin anchoredPosition - eli oikean/ylareunan etaisyys ruudun reunasta.
        static void MakeCornerMascot(Transform parent, string name, Sprite mascotSprite, float rightOffset, float topOffset, float size)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(rightOffset, topOffset);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = mascotSprite;
            img.color = Color.white;
            img.raycastTarget = false; // ei klikattava - pelkka koriste
            img.preserveAspect = true;
        }

        static void MakeFullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Kortti joka on kiinnitetty paneelin YLAREUNAAN (esim. otsikko) riippumatta paneelin
        // todellisesta korkeudesta ajon aikana (paneeli taytta koko ruudun).
        static GameObject MakeTopAnchoredCard(Transform parent, string name, float topOffset, Vector2 size, Color color, Sprite roundedSprite)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -topOffset);
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return go;
        }

        // Nappi joka on kiinnitetty paneelin ALAREUNAAN (esim. "Valmis"/"Takaisin").
        static GameObject MakeBottomAnchoredButton(Transform parent, string name, float bottomOffset, Vector2 size, Color color, Sprite roundedSprite)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, bottomOffset);
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return go;
        }

        // Yksinkertainen "rivi"-sailio (ei korttitaustaa) - kayttajalle esim. Aikuiselle-otsikkorivi.
        static GameObject MakeRowContainer(Transform parent, string name, Vector2 size)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            return go;
        }

        // Ohut varillinen palkki (erotinviiva) VerticalLayoutGroupin sisalla.
        static void MakeRowSpacer(Transform parent, string name, Vector2 size, Color color)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = null;
            img.color = color;
        }

        // Rakentaa yhden Musiikki/Aanet/Varahtely-tyylisen rivin VerticalLayoutGroupin sisalla:
        // valkoinen kortti + varillinen ikonibadge + teksti + liukukytkin oikealla.
        static Toggle BuildToggleRow(Transform parent, string prefix, Sprite icon, Color badgeColor,
            string label, Sprite roundedSprite, Sprite circleSprite, Sprite trackSprite, Sprite knobSprite,
            Color onColor, Color offColor, ref StringBuilder s)
        {
            var card = MakeCard(parent, prefix + "Card", Vector2.zero, new Vector2(ROW_W, 92), CardWhite, roundedSprite);
            MakeBadgedIcon(card.transform, "Icon", icon, circleSprite, new Vector2(-188, 0), 52, badgeColor);
            MakeLabel(card.transform, "Label", label, 28, new Vector2(-28, 0),
                new Vector2(210, 60), TextAlignmentOptions.Left, TextDark);
            var toggle = BuildToggleSwitch(card.transform, prefix + "Toggle", new Vector2(150, 0),
                trackSprite, knobSprite, onColor, offColor, ref s);
            return toggle;
        }

        // Rakentaa yhden Tietoja/Tietosuoja/Nollaa-tyylisen napautettavan rivin VerticalLayoutGroupin
        // sisalla: valkoinen kortti + varillinen ikonibadge + teksti + harmaa oikea-nuoli.
        static Button BuildChevronRow(Transform parent, string name, Sprite icon, Color badgeColor,
            string label, Sprite chevronSprite, Sprite roundedSprite, Sprite circleSprite, ref StringBuilder s)
        {
            var existing = FindChild(parent, name);
            bool isNew = existing == null;
            GameObject go = isNew ? new GameObject(name, typeof(RectTransform)) : existing.gameObject;
            if (isNew)
            {
                Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
                go.transform.SetParent(parent, false);
                var img0 = go.AddComponent<Image>();
                var btn0 = go.AddComponent<Button>();
                btn0.targetGraphic = img0;
                s.AppendLine($"{name} luotu.");
            }
            else if (go.transform.parent != parent)
            {
                go.transform.SetParent(parent, false); // migroi vanha panelGo:n suora lapsi Content:n sisaan
            }
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(ROW_W, 84);
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = CardWhite;
            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            MakeBadgedIcon(go.transform, "Icon", icon, circleSprite, new Vector2(-188, 0), 44, badgeColor);
            MakeLabel(go.transform, "Label", label, 26, new Vector2(-28, 0),
                new Vector2(210, 60), TextAlignmentOptions.Left, TextDark);
            MakeIcon(go.transform, "Chevron", chevronSprite, new Vector2(200, 0), new Vector2(22, 22), ChevronGray);
            return btn;
        }

        // Rakentaa yhden segmentoidun valintanapin (esim. "Napit"/"Kallistus"/"Molemmat" -
        // kolmesta yksi rivi). Palauttaa (Button, Image) - Image on tausta jota SettingsManager
        // varittaa RefreshControlButtons/RefreshSpeedButtons-metodeilla valinnan mukaan.
        static (Button, Image) MakeSegButton(Transform parent, string label, Vector2 pos, Sprite roundedSprite)
        {
            string name = "Seg_" + label;
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(132, 52);
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = SegUnselected;
            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            MakeLabel(go.transform, "Label", label, 20, Vector2.zero,
                new Vector2(120, 44), TextAlignmentOptions.Center, TextDark);
            return (btn, img);
        }

        // Rakentaa/paivittaa "Tietoja"-paneelin: kiintea otsikko ylhaalla (+ paluu-ikoni oikeassa
        // ylakulmassa), kiintea "Laheta palautetta" -nappi alhaalla, kaikki muu (logo/kuvaus/
        // tarkistuslista/tekija) vierittyvassa Content-alueessa.
        // M5.13, 13.8.2026 (kayttajan pyynto): aiempi "Takaisin"-nappi alhaalla korvattu
        // palaute-napilla (kirjekuori-ikoni, avaa sahkopostin info.SendFeedback():lla) - paluu
        // edelliseen nakymaan hoituu nyt pelkastaan uudella ylakulman ikoninapilla, sama kaava
        // kuin Asetukset- ja Tietosuoja-paneeleissa.
        static void BuildInfoPanel(Transform canvasTransform, SettingsManager settings, Sprite mascotSprite,
            Sprite roundedSprite, Sprite checkIcon, Sprite circleSprite,
            Sprite lopetaIcon, Sprite kirjeIcon, Sprite infoIcon, ref StringBuilder s)
        {
            const string INFO_PANEL_NAME = "InfoPanel";
            var infoPanelGo = Find(INFO_PANEL_NAME);
            bool infoPanelIsNew = infoPanelGo == null;
            if (infoPanelIsNew)
            {
                infoPanelGo = new GameObject(INFO_PANEL_NAME, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(infoPanelGo, "Luo " + INFO_PANEL_NAME);
                infoPanelGo.transform.SetParent(canvasTransform, false);
                infoPanelGo.AddComponent<Image>();
            }
            MakeFullStretch((RectTransform)infoPanelGo.transform);
            var infoBg = infoPanelGo.GetComponent<Image>();
            if (infoBg == null) infoBg = infoPanelGo.AddComponent<Image>();
            infoBg.sprite = null;
            infoBg.color = SettingsBg;

            var info = infoPanelGo.GetComponent<InfoManager>();
            if (info == null) info = infoPanelGo.AddComponent<InfoManager>();
            info.panelRoot = infoPanelGo;

            var titleCard = MakeTopAnchoredCard(infoPanelGo.transform, "TitleCard", 24, new Vector2(260, 84), TitlePill, roundedSprite);
            // InfoIcon: kayttajan itse Unity-editorissa TitleCardiin lisaama pieni ikoni Title-
            // tekstin vieressa - siirretty tahan Editor-tyokaluun ettei se katoa seuraavalla ajolla.
            MakeIcon(titleCard.transform, "InfoIcon", infoIcon, new Vector2(-95, 0), new Vector2(34, 34), Color.white);
            MakeLabel(titleCard.transform, "Title", "Tietoja", 44, new Vector2(14, 0),
                new Vector2(210, 74), TextAlignmentOptions.Center, Color.white);

            // Paluu-ikoni oikeassa ylakulmassa - vie takaisin Asetuksiin.
            var infoBackIconBtn = MakeBackIconButton(infoPanelGo.transform, "InfoBackIconButton", circleSprite, lopetaIcon);
            ClearClicks(infoBackIconBtn);
            UnityEventTools.AddPersistentListener(infoBackIconBtn.onClick, info.Close);

            // Alareunan nappi on nyt PALAUTE-nappi (kirjekuori-ikoni), ei enaa "Takaisin".
            var backGo = MakeBottomAnchoredButton(infoPanelGo.transform, "InfoBackButton", 24, new Vector2(320, 84), ValmisGreen, roundedSprite);
            MakeIcon(backGo.transform, "Icon", kirjeIcon, new Vector2(-110, 0), new Vector2(34, 34), Color.white);
            MakeLabel(backGo.transform, "Label", "Lähetä palautetta", 27, new Vector2(18, 0),
                new Vector2(230, 74), TextAlignmentOptions.Center, Color.white);
            var backBtn = backGo.GetComponent<Button>();
            ClearClicks(backBtn);
            UnityEventTools.AddPersistentListener(backBtn.onClick, info.SendFeedback);

            var content = BuildScrollArea(infoPanelGo.transform, "InfoScroll", 128, 128, 8);

            MakeIcon(content, "Logo", mascotSprite, Vector2.zero, new Vector2(130, 153), Color.white);
            MakeLabel(content, "GameTitle", "Pakettipörinä", 42, Vector2.zero,
                new Vector2(ROW_W, 60), TextAlignmentOptions.Center, CoralRed);
            MakeLabel(content, "GameSubtitle", "Kaupungin hupsuin lähetti", 22, Vector2.zero,
                new Vector2(ROW_W, 40), TextAlignmentOptions.Center, TextMuted);

            var descCard = MakeCard(content, "DescCard", Vector2.zero, new Vector2(ROW_W, 180), CardWhite, roundedSprite);
            var descLabel = MakeLabel(descCard.transform, "Desc",
                "Rakenna oma auto, valitse paketti ja aja se perille radan läpi!", 25, new Vector2(0, 32),
                new Vector2(410, 100), TextAlignmentOptions.Center, TextDark);
            descLabel.enableWordWrapping = true;
            MakeLabel(descCard.transform, "AgeHint", "Ikäsuositus 6–10 vuotta", 20, new Vector2(0, -58),
                new Vector2(380, 40), TextAlignmentOptions.Center, TextMuted);

            // Tarkistuslista (4 kohtaa, vihrea checkmark + teksti) - yksi rivi per kohta.
            string[] checklist = { "Toimii ilman nettiä", "Ei mainoksia", "Ei datankeruuta", "Turvallinen lapsille" };
            for (int i = 0; i < checklist.Length; i++)
            {
                var row = MakeRowContainer(content, $"CheckRow{i}", new Vector2(ROW_W, 42));
                MakeBadgedIcon(row.transform, "Icon", checkIcon, circleSprite, new Vector2(-188, 0), 32, BadgeGreen);
                // Kayttajan mittaama korjaus M5.13: Label Pos X = 0 (ei -28) - asettuu paremmin.
                MakeLabel(row.transform, "Label", checklist[i], 24, Vector2.zero,
                    new Vector2(320, 40), TextAlignmentOptions.Left, TextDark);
            }

            MakeRowSpacer(content, "Divider", new Vector2(420, 3), DividerColor);

            var tekijaHeaderRow = MakeRowContainer(content, "TekijaHeaderRow", new Vector2(ROW_W, 40));
            MakeLabel(tekijaHeaderRow.transform, "Label", "Tekijä", 26, new Vector2(-188, 0),
                new Vector2(200, 40), TextAlignmentOptions.Left, TextDark);

            // Tekija-kortti on itse nappi: napauttamalla avautuu sahkoposti (palaute) - mockupissa
            // ei ole erillista "Laheta palautetta" -nappia, joten palaute kulkee tasta.
            var tekijaGo = Find("InfoTekijaCard");
            Button tekijaBtn;
            bool tekijaIsNew = tekijaGo == null;
            if (tekijaIsNew)
            {
                tekijaGo = new GameObject("InfoTekijaCard", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(tekijaGo, "Luo InfoTekijaCard");
                tekijaGo.transform.SetParent(content, false);
                var timg = tekijaGo.AddComponent<Image>();
                tekijaBtn = tekijaGo.AddComponent<Button>();
                tekijaBtn.targetGraphic = timg;
            }
            else
            {
                if (tekijaGo.transform.parent != content) tekijaGo.transform.SetParent(content, false);
                tekijaBtn = tekijaGo.GetComponent<Button>();
            }
            var trt = (RectTransform)tekijaGo.transform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = Vector2.zero;
            trt.sizeDelta = new Vector2(ROW_W, 160);
            var tekijaImg = tekijaGo.GetComponent<Image>();
            if (tekijaImg != null) { tekijaImg.sprite = roundedSprite; tekijaImg.type = Image.Type.Sliced; tekijaImg.color = CardWhite; }
            MakeLabel(tekijaGo.transform, "Name", "Eero Korhonen", 30, new Vector2(0, 42),
                new Vector2(420, 44), TextAlignmentOptions.Center, TextDark);
            MakeLabel(tekijaGo.transform, "Role", "Ohjelmistotekniikan insinööri (AMK)", 21, new Vector2(0, 2),
                new Vector2(420, 36), TextAlignmentOptions.Center, TextMuted);
            MakeLabel(tekijaGo.transform, "SubRole", "Portfolioprojekti · tehty Unitylla", 19, new Vector2(0, -38),
                new Vector2(420, 36), TextAlignmentOptions.Center, TextMuted);
            if (tekijaBtn != null)
            {
                ClearClicks(tekijaBtn);
                UnityEventTools.AddPersistentListener(tekijaBtn.onClick, info.SendFeedback);
            }

            MakeLabel(content, "Version", "Versio 1.0", 20, Vector2.zero,
                new Vector2(ROW_W, 34), TextAlignmentOptions.Center, TextMuted);

            // Poista vanhat suoraan paneeliin (ei Content:n sisaan) jaaneet objektit ja M5.4-ajan
            // tekstipohjainen "Ohjeet & palaute" -sisalto, joka korvattiin kuvareferenssin mukaisella
            // "Tietoja"-paneelilla.
            RemoveIfExists(infoPanelGo.transform, "Body");
            RemoveIfExists(infoPanelGo.transform, "InfoFeedbackButton");
            RemoveIfExists(infoPanelGo.transform, "InfoCloseButton");
            // Kayttaja lisasi kasin Unity-editorissa oman "CloseButton"-napin ennen kuin
            // ylakulman InfoBackIconButton oli olemassa - poistetaan nyt turhana kahdennuksena.
            RemoveIfExists(infoPanelGo.transform, "CloseButton");
            RemoveIfExists(infoPanelGo.transform, "Logo"); RemoveIfExists(infoPanelGo.transform, "GameTitle");
            RemoveIfExists(infoPanelGo.transform, "GameSubtitle"); RemoveIfExists(infoPanelGo.transform, "DescCard");
            RemoveIfExists(infoPanelGo.transform, "Divider"); RemoveIfExists(infoPanelGo.transform, "TekijaHeader");
            RemoveIfExists(infoPanelGo.transform, "Version");
            for (int i = 0; i < 4; i++) { RemoveIfExists(infoPanelGo.transform, $"CheckIcon{i}"); RemoveIfExists(infoPanelGo.transform, $"CheckLabel{i}"); }

            infoPanelGo.SetActive(false);
            EditorUtility.SetDirty(info);
            settings.infoManager = info;
            EditorUtility.SetDirty(settings);
            s.AppendLine(infoPanelIsNew ? "Tietoja-paneeli luotu." : "Tietoja-paneelin kytkennat paivitetty.");
        }

        // Rakentaa/paivittaa "Tietosuoja"-paneelin: kiintea otsikko (+ paluu-ikoni oikeassa
        // ylakulmassa) + vierittyva tekstikortti. M5.13, 13.8.2026: alareunan iso "Takaisin"-
        // nappi poistettu, sama kaava kuin Asetukset- ja Tietoja-paneeleissa.
        static void BuildPrivacyPanel(Transform canvasTransform, SettingsManager settings, Sprite roundedSprite,
            Sprite kilpiIcon, Sprite circleSprite, Sprite lopetaIcon, ref StringBuilder s)
        {
            const string NAME = "PrivacyPanel";
            var go = Find(NAME);
            bool isNew = go == null;
            if (isNew)
            {
                go = new GameObject(NAME, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, "Luo " + NAME);
                go.transform.SetParent(canvasTransform, false);
                go.AddComponent<Image>();
            }
            MakeFullStretch((RectTransform)go.transform);
            var bg = go.GetComponent<Image>();
            if (bg == null) bg = go.AddComponent<Image>();
            bg.sprite = null;
            bg.color = SettingsBg;

            var privacy = go.GetComponent<PrivacyManager>();
            if (privacy == null) privacy = go.AddComponent<PrivacyManager>();
            privacy.panelRoot = go;

            var titleCard = MakeTopAnchoredCard(go.transform, "TitleCard", 24, new Vector2(320, 84), TitlePill, roundedSprite);
            MakeIcon(titleCard.transform, "Icon", kilpiIcon, new Vector2(-105, 0), new Vector2(40, 40), Color.white);
            MakeLabel(titleCard.transform, "Title", "Tietosuoja", 34, new Vector2(24, 0),
                new Vector2(220, 74), TextAlignmentOptions.Center, Color.white);

            // Iso "Takaisin"-nappi alhaalta pois - paluu hoituu ylakulman ikoninapilla.
            RemoveIfExists(go.transform, "PrivacyBackButton");
            var privacyBackIconBtn = MakeBackIconButton(go.transform, "PrivacyBackIconButton", circleSprite, lopetaIcon);
            ClearClicks(privacyBackIconBtn);
            UnityEventTools.AddPersistentListener(privacyBackIconBtn.onClick, privacy.Close);

            RemoveIfExists(go.transform, "BodyCard"); // vanhan ei-vierittyvan version suora lapsi -> uusi rakennetaan Content:iin
            // M5.13:ssa kokeiltu Viewport top 50 - kayttajan seuraavan kierroksen simulaattori-
            // kuvasta kavi ilmi etta se oli LIIAN pieni: TitleCard (topOffset 24 + korkeus 84 =
            // ulottuu y:hyn -108) jai vierityssisallon (BodyCard, korkea valkoinen kortti) ALLE,
            // koska SettingsScroll on TitleCardin JALKEEN luotu sisarus (piirtyy paalle). M5.14,
            // 14.8.2026: nostettu 120:een, jattaa 12 yksikon marginaalin TitleCardin alareunasta.
            var content = BuildScrollArea(go.transform, "PrivacyScroll", 120, 32, 8);

            var bodyCard = MakeCard(content, "BodyCard", Vector2.zero, new Vector2(ROW_W, 620), CardWhite, roundedSprite);
            var body = MakeLabel(bodyCard.transform, "Body", PrivacyBodyText, 22, Vector2.zero,
                new Vector2(410, 570), TextAlignmentOptions.TopLeft, TextDark);
            body.enableWordWrapping = true;

            go.SetActive(false);
            EditorUtility.SetDirty(privacy);
            settings.privacyManager = privacy;
            EditorUtility.SetDirty(settings);
            s.AppendLine(isNew ? "Tietosuoja-paneeli luotu." : "Tietosuoja-paneelin kytkennat paivitetty.");
        }

        // Rakentaa/paivittaa "Nollaa edistyminen?" -vahvistuspaneelin: PIENI kiintokokoinen
        // modaali (ei vierity - sisalto on lyhyt ja mahtuu aina yhdelle ruudulle).
        static void BuildResetConfirmPanel(Transform canvasTransform, SettingsManager settings, Sprite roundedSprite,
            Sprite circleSprite, Sprite resetIcon, ref StringBuilder s)
        {
            const string NAME = "ResetConfirmPanel";
            var go = Find(NAME);
            bool isNew = go == null;
            if (isNew)
            {
                go = new GameObject(NAME, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(go, "Luo " + NAME);
                go.transform.SetParent(canvasTransform, false);
                go.AddComponent<Image>();
            }
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(460, 520);
            var bg = go.GetComponent<Image>();
            if (bg == null) bg = go.AddComponent<Image>();
            bg.sprite = roundedSprite;
            bg.type = Image.Type.Sliced;
            bg.color = CardWhite;

            var reset = go.GetComponent<ResetConfirmManager>();
            if (reset == null) reset = go.AddComponent<ResetConfirmManager>();
            reset.panelRoot = go;

            MakeBadgedIcon(go.transform, "Icon", resetIcon, circleSprite, new Vector2(0, 185), 80, BadgeRed);
            MakeLabel(go.transform, "Title", "Nollaa edistyminen?", 32, new Vector2(0, 100),
                new Vector2(400, 60), TextAlignmentOptions.Center, TextDark);
            var body = MakeLabel(go.transform, "Body",
                "Tämä poistaa kaikki pisteet, avatut osat ja tarrat pysyvästi. Tätä ei voi perua.",
                22, new Vector2(0, -5), new Vector2(400, 110), TextAlignmentOptions.Center, TextMuted);
            body.enableWordWrapping = true;

            var peruutaGo = Find("ResetPeruutaButton");
            Button peruutaBtn;
            bool peruutaIsNew = peruutaGo == null;
            if (peruutaIsNew)
            {
                peruutaGo = new GameObject("ResetPeruutaButton", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(peruutaGo, "Luo ResetPeruutaButton");
                peruutaGo.transform.SetParent(go.transform, false);
                var pimg = peruutaGo.AddComponent<Image>();
                peruutaBtn = peruutaGo.AddComponent<Button>();
                peruutaBtn.targetGraphic = pimg;
            }
            else
            {
                peruutaBtn = peruutaGo.GetComponent<Button>();
            }
            var prt2 = (RectTransform)peruutaGo.transform;
            prt2.anchorMin = prt2.anchorMax = new Vector2(0.5f, 0.5f);
            prt2.pivot = new Vector2(0.5f, 0.5f);
            prt2.anchoredPosition = new Vector2(-95, -190);
            prt2.sizeDelta = new Vector2(170, 90);
            var peruutaImg = peruutaGo.GetComponent<Image>();
            if (peruutaImg != null) { peruutaImg.sprite = roundedSprite; peruutaImg.type = Image.Type.Sliced; peruutaImg.color = SegUnselected; }
            MakeLabel(peruutaGo.transform, "Label", "Peruuta", 26, Vector2.zero,
                new Vector2(150, 80), TextAlignmentOptions.Center, TextDark);
            if (peruutaBtn != null)
            {
                ClearClicks(peruutaBtn);
                UnityEventTools.AddPersistentListener(peruutaBtn.onClick, reset.Close);
            }

            var nollaaGo = Find("ResetNollaaButton");
            Button nollaaBtn;
            bool nollaaIsNew = nollaaGo == null;
            if (nollaaIsNew)
            {
                nollaaGo = new GameObject("ResetNollaaButton", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(nollaaGo, "Luo ResetNollaaButton");
                nollaaGo.transform.SetParent(go.transform, false);
                var nimg = nollaaGo.AddComponent<Image>();
                nollaaBtn = nollaaGo.AddComponent<Button>();
                nollaaBtn.targetGraphic = nimg;
            }
            else
            {
                nollaaBtn = nollaaGo.GetComponent<Button>();
            }
            var nrt = (RectTransform)nollaaGo.transform;
            nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0.5f);
            nrt.pivot = new Vector2(0.5f, 0.5f);
            nrt.anchoredPosition = new Vector2(95, -190);
            nrt.sizeDelta = new Vector2(170, 90);
            var nollaaImg = nollaaGo.GetComponent<Image>();
            if (nollaaImg != null) { nollaaImg.sprite = roundedSprite; nollaaImg.type = Image.Type.Sliced; nollaaImg.color = BadgeRed; }
            MakeLabel(nollaaGo.transform, "Label", "Nollaa", 26, Vector2.zero,
                new Vector2(150, 80), TextAlignmentOptions.Center, Color.white);
            if (nollaaBtn != null)
            {
                ClearClicks(nollaaBtn);
                UnityEventTools.AddPersistentListener(nollaaBtn.onClick, reset.ConfirmReset);
            }

            go.SetActive(false);
            EditorUtility.SetDirty(reset);
            settings.resetConfirmManager = reset;
            EditorUtility.SetDirty(settings);
            s.AppendLine(isNew ? "Nollaa edistyminen -vahvistuspaneeli luotu." : "Nollaa edistyminen -vahvistuspaneelin kytkennat paivitetty.");
        }

        // ---------- apufunktiot ----------

        static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == name) return t.gameObject;
            return null;
        }

        static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform c in parent)
                if (c.name == name) return c;
            return null;
        }

        static void RemoveIfExists(Transform parent, string name)
        {
            var c = FindChild(parent, name);
            if (c != null) Object.DestroyImmediate(c.gameObject);
        }

        static void ClearClicks(Button b)
        {
            for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(b.onClick, i);
        }

        static Sprite LoadIcon(string name)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ICON_DIR}/{name}.png");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite LoadSprite(string fullPath)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        // Lataa Spriten 9-slice-reunalla (pyoristetyt kulmat pysyvat terävinä vaikka
        // spritea venytetaan eri kokoisiin kortteihin/nappeihin).
        static Sprite LoadSlicedSprite(string name, float border)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ICON_DIR}/{name}.png");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f,
                1, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        // Idempotentti: loytaa olemassa olevan tai luo uuden, ja paivittaa AINA sijainnin/koon/
        // varin. sprite=null -> kaytetaan pelkkaa varitettya suorakaidetta (esim. erotinviiva).
        static Image MakeIcon(Transform parent, string name, Sprite sprite, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = sprite != null;
            return img;
        }

        // Varillinen pyorea "badge" + valkoinen ikoni sen paalla (referenssikuvien tyyli:
        // esim. oranssi ympyra jonka sisalla valkoinen nuotti, ei pelkka varitetty ikonisiluetti).
        // Badge kayttaa nimea "name" (idempotentti paivitys FindChildilla), ikoni "name+Fg".
        static void MakeBadgedIcon(Transform parent, string name, Sprite icon, Sprite circleSprite,
            Vector2 pos, float size, Color badgeColor)
        {
            MakeIcon(parent, name, circleSprite, pos, new Vector2(size, size), badgeColor);
            MakeIcon(parent, name + "Fg", icon, pos, new Vector2(size * 0.56f, size * 0.56f), Color.white);
        }

        // Pyoristetty "kortti": 9-slice-tausta, palauttaa GameObjectin (lapsille).
        static GameObject MakeCard(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color, Sprite roundedSprite)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            return go;
        }

        static TextMeshProUGUI MakeLabel(Transform parent, string name, string text, float fontSize,
            Vector2 anchoredPos, Vector2 size, TextAlignmentOptions align, Color color)
        {
            var existing = FindChild(parent, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            return tmp;
        }

        // Liukukytkin (ks. Scripts/UI/ToggleSwitch.cs): pilleri-muotoinen rata + pyorea nuppi.
        static Toggle BuildToggleSwitch(Transform parent, string name, Vector2 anchoredPos,
            Sprite trackSprite, Sprite knobSprite, Color onColor, Color offColor, ref StringBuilder s)
        {
            var existing = FindChild(parent, name);
            bool isNew = existing == null;
            GameObject go = isNew ? new GameObject(name, typeof(RectTransform)) : existing.gameObject;
            if (isNew)
            {
                Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
                go.transform.SetParent(parent, false);
            }
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(120, 58);

            var trackImg = MakeIcon(go.transform, "Track", trackSprite, Vector2.zero, new Vector2(120, 58), Color.white);
            var knobImg = MakeIcon(go.transform, "Knob", knobSprite, new Vector2(-28, 0), new Vector2(46, 46), Color.white);

            var toggle = go.GetComponent<Toggle>();
            if (toggle == null) toggle = go.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = trackImg;
            toggle.graphic = null;

            var sw = go.GetComponent<ToggleSwitch>();
            if (sw == null) sw = go.AddComponent<ToggleSwitch>();
            sw.track = trackImg;
            sw.knob = (RectTransform)knobImg.transform;
            sw.offKnobX = -28f;
            sw.onKnobX = 28f;
            sw.onColor = onColor;
            sw.offColor = offColor;

            if (isNew) s.AppendLine($"{name} luotu.");
            return toggle;
        }

        static void WireToggleBool(Toggle toggle, UnityAction<bool> action)
        {
            if (toggle == null) return;
            for (int i = toggle.onValueChanged.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(toggle.onValueChanged, i);
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, action);
        }

        // Kytkee napin OnClick -> action(kiinteaArvo) PYSYVANA (serialisoituna scenetiedostoon).
        // UnityEventTools.AddIntPersistentListener "leipoo" kiintean int-arvon scenen mukana
        // tallentuvaan kutsuun - btn.onClick.AddListener(() => ...) EI kelpaisi, koska se lisaisi
        // vain ei-pysyvan runtime-kuuntelijan joka katoaa scenen uudelleenlatauksessa.
        static void WireIntButton(Button btn, UnityAction<int> action, int value)
        {
            if (btn == null) return;
            ClearClicks(btn);
            UnityEventTools.AddIntPersistentListener(btn.onClick, action, value);
        }
    }
}
#endif
