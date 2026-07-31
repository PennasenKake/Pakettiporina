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
    // Pesu-scenen (M4) tarkistus- ja korjaustyokalu.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 9 Tarkista pesu / 10 KORJAA pesu
    //
    // Odottaa scenesta naita nimia (muuta vakioita jos omat nimesi poikkeavat):
    //   BubbleArea      - tyhja Transform, jonka lapsina kuplat (Bubble-komponentti) ovat
    //   ProgressText    - TMP_Text, esim. "Poksautit 0/8 kuplaa!"
    //   DonePanel       - paneeli, joka nakyy kun kaikki kuplat on poksautettu
    //   DoneText        - TMP_Text DonePanelin sisalla
    //   ContinueButton  - nappi DonePanelin sisalla, vie halliin
    //   SkipButton      - valinnainen, ohittaa pesun
    public static class PakettiporinaWashSetup
    {
        const string BUBBLE_AREA = "BubbleArea";
        const string PROGRESS_TEXT = "ProgressText";
        const string DONE_PANEL = "DonePanel";
        const string DONE_TEXT = "DoneText";
        const string CONTINUE_BTN = "ContinueButton";
        const string SKIP_BTN = "SkipButton";
        const string MAINMENU_BTN = "MainMenuButton";  // usein kopioitu Ajon maali-paneelista
        const string RESTART_BTN = "RestartButton";    // usein kopioitu Ajon maali-paneelista
        const string GARAGE_SCENE = "Garage";
        const string MAINMENU_SCENE = "MainMenu";

        [MenuItem("Pakettiporina/9 - Tarkista pesu")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/10 - KORJAA pesu")]
        public static void FixAll() { Run(true); }

        static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == name) return t.gameObject;
            return null;
        }

        static void ClearClicks(Button b)
        {
            for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(b.onClick, i);
        }

        static void Run(bool fix)
        {
            var s = new StringBuilder();
            s.AppendLine(fix ? "=== PESUN KORJAUS ===" : "=== PESUN TARKISTUS ===");
            int problems = 0;
            void Prob(string msg) { s.AppendLine("! " + msg); problems++; }

            var wash = Object.FindObjectOfType<WashScreen>(true);
            var canvas = Object.FindObjectOfType<Canvas>(true);
            var evsys = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true);
            var gm = Object.FindObjectOfType<GameManager>(true);

            if (wash == null)
            {
                s.AppendLine("VIRHE: WashScreen puuttuu scenesta. Lisaa se jollekin objektille (esim. Canvasille).");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- 1. EventSystem ----------
            if (evsys == null)
            {
                Prob("EventSystem puuttuu (kuplien napautus ei toimi).");
                if (fix)
                {
                    var go = new GameObject("EventSystem");
                    go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    Undo.RegisterCreatedObjectUndo(go, "EventSystem");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 2. GameManager ----------
            if (gm == null)
            {
                Prob("GameManager puuttuu pesusta (palkkiopisteet eivat tallennu jos testaat tasta scenesta suoraan).");
                if (fix)
                {
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                    Undo.RegisterCreatedObjectUndo(go, "GameManager");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 2b. AudioManager ----------
            var audioMgr = Object.FindObjectOfType<AudioManager>(true);
            if (audioMgr == null)
            {
                Prob("AudioManager puuttuu pesusta (kuplan poksahdusaani ei soi jos testaat tasta scenesta suoraan).");
                if (fix)
                {
                    var go = new GameObject("AudioManager");
                    go.AddComponent<AudioManager>();
                    Undo.RegisterCreatedObjectUndo(go, "AudioManager");
                    s.AppendLine("  -> luotu (muista raahata aaniklipit sen Inspectoriin)");
                }
            }

            // ---------- 3. Canvas Scaler ----------
            if (canvas != null)
            {
                var sc = canvas.GetComponent<CanvasScaler>();
                if (sc != null)
                {
                    bool bad = sc.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                            || sc.referenceResolution != new Vector2(1080, 1920);
                    if (bad)
                    {
                        Prob($"Canvas Scaler: {sc.uiScaleMode}, {sc.referenceResolution}");
                        if (fix)
                        {
                            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                            sc.referenceResolution = new Vector2(1080, 1920);
                            sc.matchWidthOrHeight = 0.5f;
                            s.AppendLine("  -> korjattu 1080x1920 / match 0.5");
                        }
                    }
                    else s.AppendLine("Canvas Scaler: OK");
                }
            }

            // ---------- 4. BubbleArea + kuplien maara ----------
            if (wash.bubbleArea == null)
            {
                Prob("Bubble Area ei kytketty.");
                if (fix)
                {
                    var go = Find(BUBBLE_AREA);
                    if (go != null) { wash.bubbleArea = go.transform; s.AppendLine("  -> kytketty (" + BUBBLE_AREA + ")"); }
                    else s.AppendLine("  -> objektia '" + BUBBLE_AREA + "' ei loytynyt, kytke kasin");
                }
            }
            if (wash.bubbleArea != null)
            {
                int count = wash.bubbleArea.GetComponentsInChildren<Bubble>(true).Length;
                if (count == 0)
                    Prob("BubbleArean alla ei ole yhtaan Bubble-komponenttia. Lisaa muutama kupla-objekti.");
                else
                    s.AppendLine($"Kuplia BubbleArean alla: {count}");

                // Tarkista etta jokaisella kuplalla on Image ja Raycast Target paalla.
                foreach (var b in wash.bubbleArea.GetComponentsInChildren<Bubble>(true))
                {
                    var img = b.GetComponent<Image>();
                    if (img != null && !img.raycastTarget)
                    {
                        Prob($"Kupla '{b.name}': Image Raycast Target on pois paalta (ei reagoi napautukseen).");
                        if (fix) { img.raycastTarget = true; s.AppendLine("  -> korjattu"); }
                    }
                }
            }

            // ---------- 4c. BubbleArean skaala + kuplien nousukorkeus ----------
            // Tunnettu sudenkuoppa: jos BubbleArea skaalataan Scene-nakymassa (esim. yritettaessa
            // "sovittaa" alue autoa vasten), sen Transform.localScale poikkeaa 1:sta ja KAIKKI
            // kuplien liikkeet (leveys, nousumatka) kutistuvat samassa suhteessa -> kuplat
            // nakyvat pienena kasana yhdessa kohtaa eivatka nouse lahellekaan autoa.
            if (wash.bubbleArea != null)
            {
                var sc = wash.bubbleArea.localScale;
                bool badScale = Mathf.Abs(sc.x - 1f) > 0.02f || Mathf.Abs(sc.y - 1f) > 0.02f;
                if (badScale)
                {
                    Prob($"BubbleArean Scale on {sc.x:0.###}, {sc.y:0.###} (pitaisi olla 1, 1) - " +
                         "tama kutistaa kaikkien kuplien leveys- ja nousumatka-asetukset samassa suhteessa, " +
                         "joten kuplat nayttavat jaavan pieneen kasaan eivatka nouse lahelle autoa.");
                    if (fix)
                    {
                        wash.bubbleArea.localScale = Vector3.one;
                        wash.bubbleArea.localPosition = Vector3.zero;
                        s.AppendLine("  -> BubbleArean Scale asetettu (1,1,1) ja Position (0,0,0). " +
                                     "Kuplien oma Spawn X/Y ja Rise Distance maarittavat nyt sijainnin suoraan Canvasin yksikoissa.");
                    }
                }
                else s.AppendLine("BubbleArean Scale: OK (1,1,1).");

                // Kuplien nousukorkeus suhteessa Car-objektiin: yritetaan arvioida etta kuplat
                // nousevat juuri auton alapuolelle asti, eivat sen paalle/lapi.
                var carGo = Find("Car");
                var carRt = carGo != null ? carGo.GetComponent<RectTransform>() : null;
                var bubbles = wash.bubbleArea.GetComponentsInChildren<Bubble>(true);
                if (carRt != null && bubbles.Length > 0)
                {
                    float carBottom = carRt.anchoredPosition.y - (carRt.rect.height * carRt.localScale.y) / 2f;
                    const float margin = 80f;
                    float targetApex = carBottom - margin;
                    int retuned = 0;
                    foreach (var b in bubbles)
                    {
                        float wantedRise = targetApex - b.spawnY;
                        if (wantedRise > 100f && Mathf.Abs(wantedRise - b.riseDistance) > 150f)
                        {
                            if (fix) { b.riseDistance = wantedRise; EditorUtility.SetDirty(b); retuned++; }
                        }
                    }
                    if (retuned > 0)
                        s.AppendLine($"  -> {retuned} kuplan Rise Distance saadetty (~{targetApex:0}) niin etta nousu pysahtyy juuri auton alapuolelle.");
                    else if (!fix)
                        s.AppendLine($"Kuplien nousukorkeus vs. auto: OK-ish (arvioitu tavoite ~{targetApex:0}).");
                }
            }

            // ---------- 4b. DirtSpots (likatahrat autossa) ----------
            if (wash.dirtSpotsParent == null && (wash.dirtSpots == null || wash.dirtSpots.Count == 0))
            {
                var dsGo = Find("DirtSpots");
                if (dsGo != null)
                {
                    Prob("Dirt Spots Parent ei kytketty.");
                    if (fix) { wash.dirtSpotsParent = dsGo.transform; s.AppendLine("  -> kytketty (DirtSpots)"); }
                }
                else
                {
                    Prob("Likatahroja (DirtSpots) ei loydy. Lisaa autoon muutama likatahra-Image ja ryhmita ne 'DirtSpots'-objektin alle, TAI raahaa ne kasin WashScreenin Dirt Spots -listaan.");
                }
            }
            if (wash.dirtSpotsParent != null)
            {
                int dcount = wash.dirtSpotsParent.GetComponentsInChildren<Image>(true).Length;
                s.AppendLine($"Likatahroja DirtSpotsin alla: {dcount}");
            }
            else if (wash.dirtSpots != null && wash.dirtSpots.Count > 0)
            {
                s.AppendLine($"Likatahroja Dirt Spots -listassa: {wash.dirtSpots.Count}");
            }

            // ---------- 5. Edistymisteksti ----------
            if (wash.progressText == null)
            {
                Prob("Progress Text ei kytketty.");
                if (fix)
                {
                    var go = Find(PROGRESS_TEXT);
                    var t = go != null ? go.GetComponent<TMP_Text>() : null;
                    if (t != null) { wash.progressText = t; s.AppendLine("  -> kytketty"); }
                }
            }

            // ---------- 6. Valmis-paneeli ----------
            if (wash.donePanel == null)
            {
                Prob("Done Panel ei kytketty.");
                if (fix)
                {
                    var go = Find(DONE_PANEL);
                    if (go != null) { wash.donePanel = go; s.AppendLine("  -> kytketty"); }
                }
            }
            if (wash.doneText == null)
            {
                Prob("Done Text ei kytketty.");
                if (fix)
                {
                    var go = Find(DONE_TEXT);
                    var t = go != null ? go.GetComponent<TMP_Text>() : null;
                    if (t != null) { wash.doneText = t; s.AppendLine("  -> kytketty"); }
                }
            }

            // ---------- 7. Napit ----------
            // HUOM: KORJAA-tilassa OnClick asetetaan AINA uudelleen oikeaksi, ei vain kun se on tyhja.
            // Tama on tarkeaa koska napit ovat usein kopioituja Ajon maali-paneelista (samat kuvakkeet),
            // ja niissa on silloin vanha viittaus RaceHUD-komponenttiin joka ei ole tassa scenessa
            // (nayttaa "OnClick: 1" vaikka kohde on rikki, joten pelkka maaralaskenta ei riita).
            void Wire(string objName, UnityAction action, string label, bool required)
            {
                var go = Find(objName);
                var btn = go != null ? go.GetComponent<Button>() : null;
                if (btn == null)
                {
                    if (required) Prob($"Nappi '{objName}' puuttuu.");
                    return;
                }
                if (fix)
                {
                    ClearClicks(btn);
                    UnityEventTools.AddPersistentListener(btn.onClick, action);
                    s.AppendLine($"{label}: OnClick kytketty ({objName}).");
                }
                else if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    Prob($"{label}: OnClick tyhja.");
                }
                else
                {
                    s.AppendLine($"{label}: OnClick on jotain kytketty ({btn.onClick.GetPersistentEventCount()} kpl) � " +
                        "jos nappi kopioitu toisesta scenesta, kohde voi silti olla rikki. Aja '10 - KORJAA pesu' varmuudeksi.");
                }
            }
            Wire(CONTINUE_BTN, wash.OnContinueButton, "Jatkoon-nappi (halliin)", true);
            Wire(SKIP_BTN, wash.OnSkipButton, "Ohita-nappi", false);           // valinnainen
            Wire(MAINMENU_BTN, wash.OnMainMenuButton, "Koti-nappi (paavalikko)", false);
            Wire(RESTART_BTN, wash.OnRestartButton, "Uudestaan-nappi (aloita pesu uudestaan)", false);

            // ---------- 8. Scenejen nimet ----------
            if (wash.garageSceneName != GARAGE_SCENE)
            {
                Prob($"Garage Scene Name = '{wash.garageSceneName}' (odotettu '{GARAGE_SCENE}').");
                if (fix) { wash.garageSceneName = GARAGE_SCENE; s.AppendLine("  -> asetettu '" + GARAGE_SCENE + "'"); }
            }
            if (wash.mainMenuScene != MAINMENU_SCENE)
            {
                Prob($"Main Menu Scene = '{wash.mainMenuScene}' (odotettu '{MAINMENU_SCENE}').");
                if (fix) { wash.mainMenuScene = MAINMENU_SCENE; s.AppendLine("  -> asetettu '" + MAINMENU_SCENE + "'"); }
            }

            // ---------- 9. PauseMenu (tauko) ----------
            var pauseMenu = Object.FindObjectOfType<PauseMenu>(true);
            if (pauseMenu == null)
            {
                Prob("PauseMenu-komponenttia ei loydy pesusta (tauko ei toimi).");
                if (fix)
                {
                    var pmGo = Find("PausePanel");
                    var host = pmGo != null ? pmGo : new GameObject("PauseMenu");
                    if (pmGo == null) Undo.RegisterCreatedObjectUndo(host, "PauseMenu");
                    pauseMenu = host.AddComponent<PauseMenu>();
                    s.AppendLine("  -> PauseMenu lisatty (" + host.name + ")");
                }
            }
            if (pauseMenu != null)
            {
                if (pauseMenu.pausePanel == null)
                {
                    var pp = Find("PausePanel");
                    if (pp != null)
                    {
                        Prob("PauseMenu.Pause Panel ei kytketty.");
                        if (fix) { pauseMenu.pausePanel = pp; s.AppendLine("  -> kytketty (PausePanel)"); }
                    }
                }
                if (pauseMenu.garageScene != GARAGE_SCENE)
                {
                    Prob($"PauseMenu Garage Scene = '{pauseMenu.garageScene}' (odotettu '{GARAGE_SCENE}').");
                    if (fix) { pauseMenu.garageScene = GARAGE_SCENE; s.AppendLine("  -> asetettu '" + GARAGE_SCENE + "'"); }
                }
                if (pauseMenu.mainMenuScene != MAINMENU_SCENE)
                {
                    Prob($"PauseMenu Main Menu Scene = '{pauseMenu.mainMenuScene}' (odotettu '{MAINMENU_SCENE}').");
                    if (fix) { pauseMenu.mainMenuScene = MAINMENU_SCENE; s.AppendLine("  -> asetettu '" + MAINMENU_SCENE + "'"); }
                }

                // Samat "OnClick aina uudelleen fix-tilassa" -periaate kuin kohdassa 7,
                // koska nama napit ovat mys usein kopioituja Ajon pausescenesta (rikkinainen viittaus).
                void WirePause(string objName, UnityAction action, string label, bool required)
                {
                    var go = Find(objName);
                    var btn = go != null ? go.GetComponent<Button>() : null;
                    if (btn == null)
                    {
                        if (required) Prob($"Nappi '{objName}' puuttuu.");
                        return;
                    }
                    if (fix)
                    {
                        ClearClicks(btn);
                        UnityEventTools.AddPersistentListener(btn.onClick, action);
                        s.AppendLine($"{label}: OnClick kytketty ({objName}).");
                    }
                    else if (btn.onClick.GetPersistentEventCount() == 0)
                    {
                        Prob($"{label}: OnClick tyhja.");
                    }
                    else
                    {
                        s.AppendLine($"{label}: OnClick on jotain kytketty � jos nappi kopioitu toisesta scenesta, " +
                            "kohde voi silti olla rikki. Aja '10 - KORJAA pesu' varmuudeksi.");
                    }
                }
                WirePause("PauseButton", pauseMenu.Pause, "Tauko-nappi (avaa)", true);
                WirePause("ResumeButton", pauseMenu.Resume, "Jatka-nappi (tauossa)", true);
                WirePause("PauseMenuButton", pauseMenu.GoToMainMenu, "Koti-nappi (tauossa)", false);
                WirePause("PauseGarageButton", pauseMenu.GoToGarage, "Halliin-nappi (tauossa)", false); // valinnainen, lisaa itse jos haluat
            }

            // ---------- valmis ----------
            if (fix)
            {
                EditorUtility.SetDirty(wash);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            s.AppendLine(problems == 0
                ? "\nKaikki kunnossa!"
                : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty � muista Ctrl+S." : " Aja '10 - KORJAA pesu'."));
            Debug.Log(s.ToString());
        }
    }
}
#endif
