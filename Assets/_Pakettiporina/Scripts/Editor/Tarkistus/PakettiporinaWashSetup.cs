#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // Pesu-scenen (M4) tarkistus- ja korjaustyokalu.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Tarkistus/
    // Kaytto: Pakettiporina -> Tarkistus/03 Tarkista pesu, Korjaus/03 KORJAA pesu
    //
    // Odottaa scenesta naita nimia (muuta vakioita jos omat nimesi poikkeavat):
    //   BubbleArea      - tyhja Transform, jonka lapsina kuplat (Bubble-komponentti) ovat
    //   ProgressText    - TMP_Text, esim. "Poksautit 0/8 kuplaa!"
    //   DonePanel       - paneeli, joka nakyy kun kaikki kuplat on poksautettu
    //   DoneText        - TMP_Text DonePanelin sisalla
    //   ContinueButton  - nappi DonePanelin sisalla, vie halliin
    //   SkipButton      - valinnainen, ohittaa pesun
    //   RepairArea      - (16.8.2026) tyhja Transform, jonka lapsina RepairTool-ikonit ovat -
    //                      TASMALLEEN SAMA periaate kuin BubbleArea/Bubble, mutta korjaa
    //                      kolhun likatahran sijaan. Piilossa pesun ajan.
    //   DentSpots       - (16.8.2026) autossa nakyvat kolhu-merkit, sama periaate kuin DirtSpots
    //   DentsText       - TMP_Text, esim. "Korjattu: 40 %"
    //   RepairEffect    - hetken nakyva kuittausefekti kun kaikki kolhut on korjattu
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

        // LISATTY 15.8.2026 (kayttaja: "korjaa tulisi olla wash scenen kanssa samassa,
        // ensin pestaan ja sen jalkeen korjataan"): korjausvaihe (kolhut) rakennetaan
        // nyt tahan sceneen. Siirretty pois Halli-scenesta (PakettiporinaGarageSetup.cs)
        // kokonaan.
        // MUUTETTU 16.8.2026 (kayttaja: "korjaa vaihe tulisi tehda samalla tavalla kuin
        // pesu"): yhden RepairButton-napin sijaan RepairArea (RepairTool-ikonit, sama
        // periaate kuin BubbleArea/Bubble) + DentSpots (kolhu-merkit autossa, sama
        // periaate kuin DirtSpots) - napauta tyokaluja korjataksesi kolhut YKSI kerrallaan.
        const string REPAIR_AREA = "RepairArea";
        const string DENT_SPOTS = "DentSpots";
        const string DENTS_TEXT = "DentsText";
        const string REPAIR_EFFECT = "RepairEffect";
        const string DENT_SPRITE_PATH = "Assets/_Pakettiporina/Art/UI/Dirt/pakettiporina_lika_b.png";

        // KORJATTU 16.8.2026 (kayttaja: "riittavan kauan pesua ja korjausta paasee tekemaan"):
        // scenesta loytyi VANHAN yhden-napin korjaussuunnitelman jaanteet (RepairPanel +
        // sen sisalla RepairButton, OnClick viittaa metodiin OnRepairButton jota WashScreenissa
        // ei enaa ole). DentsText oli vahingossa taman VANHAN, aina pois paalta olevan
        // RepairPanelin lapsena -> DentsText ei NAKYNYT KOSKAAN pelaajalle, koska Unityssa
        // lapsen aktiivisuus riippuu koko esi-isaketjusta. Lisaksi DentSpots (kolhu-merkit
        // autossa) puuttui scenesta kokonaan -> WashScreen.Start() laski activeDentCount:n
        // aina nollaksi, eli korjausvaihe ohittui AINA vaikka ajossa olisi tullut kolhuja.
        // Nailla kahdella bugilla korjausvaihetta ei siis voinut oikeasti paasta tekemaan.
        const string LEGACY_REPAIR_PANEL = "RepairPanel"; // sisaltaa myos vanhan RepairButtonin lapsenaan

        [MenuItem("Pakettiporina/Tarkistus/03 - Tarkista pesu")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/Korjaus/03 - KORJAA pesu")]
        public static void FixAll() { Run(true); }

        // Find()/ClearClicks() ovat PakettiporinaEditorUtils:ssa (Editor/Common/),
        // tuotu tahan "using static"-direktiivilla yllä.

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

            // ---------- 2c. SaveManager ----------
            var saveMgr = Object.FindObjectOfType<SaveManager>(true);
            if (saveMgr == null)
            {
                Prob("SaveManager puuttuu pesusta (tallennus/lataus ei toimi jos testaat tasta scenesta suoraan).");
                if (fix)
                {
                    var go = new GameObject("SaveManager");
                    go.AddComponent<SaveManager>();
                    Undo.RegisterCreatedObjectUndo(go, "SaveManager");
                    s.AppendLine("  -> luotu (listat tayttyvat automaattisesti kun ajat 'KORJAA halli' Garage-scenessa)");
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
            // DonePanel jaa helposti paalle Hierarkiaan kun sita on editoitu (esim. tekstien
            // asettelu) ja unohtuu piilottaa - silloin se nakyy heti pesun alussa "Kiiltava auto!"
            // yhta aikaa ProgressTextin "0/8"-lukeman kanssa. Sama tarkistus kuin peliscenen
            // FinishPanelille (PakettiporinaSetup.cs).
            var donePanelGo = Find(DONE_PANEL);
            if (donePanelGo != null && donePanelGo.activeSelf)
            {
                Prob("Done Panel on paalla (nakyy heti pesun alussa, vaikka kuplia ei ole viela poksautettu).");
                if (fix) { donePanelGo.SetActive(false); s.AppendLine("  -> piilotettu"); }
            }
            // Sama tarkistus RepairArea/RepairEffect:lle (LISATTY 15.8.2026, MUUTETTU
            // 16.8.2026) - RepairArean tyokalut eivat saa nakya/olla napautettavissa
            // ennen kuin pesuvaihe on valmis (WashScreen.Start() piilottaa sen koodissa,
            // mutta jos scenea on editoitu Play-tilan ulkopuolella se voi jaada paalle).
            var repairAreaGo = Find(REPAIR_AREA);
            if (repairAreaGo != null && repairAreaGo.activeSelf)
            {
                Prob("Repair Area on paalla (tyokalut nakyisivat heti, vaikka pesuvaihe ei ole viela valmis).");
                if (fix) { repairAreaGo.SetActive(false); s.AppendLine("  -> piilotettu"); }
            }
            var repairEffectGoCheck = Find(REPAIR_EFFECT);
            if (repairEffectGoCheck != null && repairEffectGoCheck.activeSelf)
            {
                Prob("Repair Effect on paalla (pitaisi nakya vain hetken korjauksen yhteydessa).");
                if (fix) { repairEffectGoCheck.SetActive(false); s.AppendLine("  -> piilotettu"); }
            }

            // ---------- 6b. Korjausvaihe (kolhut) - RepairArea/RepairTool/DentSpots/
            // DentsText/RepairEffect (LISATTY 15.8.2026, siirretty Hallista tanne;
            // MUUTETTU 16.8.2026 - "korjaa vaihe tulisi tehda samalla tavalla kuin
            // pesu": RepairArea+RepairTool mirroraa BubbleArea+Bubblea, DentSpots
            // mirroraa DirtSpotsia) ----------
            if (canvas != null)
            {
                var canvasT = canvas.transform;

                // --- RepairArea + muutama RepairTool-ikoni (sama periaate kuin BubbleArea) ---
                if (wash.repairArea == null)
                {
                    Prob("Repair Area ei kytketty.");
                    if (fix)
                    {
                        var raGo = Find(REPAIR_AREA);
                        if (raGo == null)
                        {
                            raGo = new GameObject(REPAIR_AREA, typeof(RectTransform));
                            Undo.RegisterCreatedObjectUndo(raGo, "Luo RepairArea");
                            raGo.transform.SetParent(canvasT, false);
                            var art = (RectTransform)raGo.transform;
                            art.anchorMin = Vector2.zero; art.anchorMax = Vector2.one;
                            art.offsetMin = Vector2.zero; art.offsetMax = Vector2.zero;

                            // Muutama RepairTool-ikoni valmiiksi (samat liikeoletusarvot kuin
                            // Bubble.cs:lla). MUUTETTU 16.8.2026 (kayttaja: "pelaajalle voisi
                            // korjauksessa nakya jokin vasaran tai siihen liittyva tyokalu") -
                            // pelkan varillisen nelion sijaan rakennetaan karkea VASARAN
                            // SILUETTI kahdesta suorakulmiosta (varsi+paa) - ei oikeaa
                            // sprite-kuvitusta (ei kuvituspipelinea 2D-ikoneille tassa
                            // projektissa), mutta lukee heti "tyokalu" eika "pallo/nappula".
                            // Vaihda Unityssa oikeaan sprite-kuvitukseen jos/kun sellainen tehdaan.
                            for (int i = 0; i < 5; i++)
                            {
                                var toolGo = new GameObject($"RepairTool_{i + 1:00}", typeof(RectTransform));
                                toolGo.transform.SetParent(raGo.transform, false);
                                var trt2 = (RectTransform)toolGo.transform;
                                trt2.sizeDelta = new Vector2(140, 140);

                                // Nakymaton koko-osan Image: itse napautusalue (RequireComponent
                                // RepairTool.cs:ssa vaatii Imagen taalla) - alpha 0, mutta
                                // raycastTarget toimii silti lapinakymattomasta huolimatta.
                                var timg = toolGo.AddComponent<Image>();
                                timg.color = new Color(1f, 1f, 1f, 0f);
                                timg.raycastTarget = true;
                                toolGo.AddComponent<RepairTool>();

                                // Varsi: ohut, vino suorakulmio.
                                var handle = new GameObject("Handle", typeof(RectTransform));
                                handle.transform.SetParent(toolGo.transform, false);
                                var handleRt = (RectTransform)handle.transform;
                                handleRt.sizeDelta = new Vector2(16, 90);
                                handleRt.anchoredPosition = new Vector2(0, -10);
                                handleRt.localRotation = Quaternion.Euler(0, 0, -35);
                                var handleImg = handle.AddComponent<Image>();
                                handleImg.color = new Color(0.36f, 0.24f, 0.14f, 1f); // puuvarsi
                                handleImg.raycastTarget = false;

                                // Paa: leveampi suorakulmio varren ylapaassa, sama kierto.
                                var head = new GameObject("Head", typeof(RectTransform));
                                head.transform.SetParent(toolGo.transform, false);
                                var headRt = (RectTransform)head.transform;
                                headRt.sizeDelta = new Vector2(54, 26);
                                headRt.anchoredPosition = new Vector2(26, 27);
                                headRt.localRotation = Quaternion.Euler(0, 0, -35);
                                var headImg = head.AddComponent<Image>();
                                headImg.color = new Color(0.55f, 0.55f, 0.58f, 1f); // metallipaa
                                headImg.raycastTarget = false;
                            }
                            s.AppendLine("  -> RepairArea + 5 vasara-muotoista RepairTool-ikonia luotu " +
                                         "(koko Canvasin kokoinen, sama periaate kuin BubbleArea) - " +
                                         "placeholder-siluetti, vaihda Unityssa oikeaan sprite-kuvitukseen jos haluat.");
                        }
                        wash.repairArea = raGo.transform;
                        s.AppendLine("  -> kytketty (" + REPAIR_AREA + ")");
                    }
                }
                if (wash.repairArea != null)
                {
                    int toolCount = wash.repairArea.GetComponentsInChildren<RepairTool>(true).Length;
                    if (toolCount == 0)
                        Prob("RepairArean alla ei ole yhtaan RepairTool-komponenttia. Lisaa muutama tyokalu-objekti.");
                    else
                        s.AppendLine($"RepairTooleja RepairArean alla: {toolCount}");

                    foreach (var rtComp in wash.repairArea.GetComponentsInChildren<RepairTool>(true))
                    {
                        var img = rtComp.GetComponent<Image>();
                        if (img != null && !img.raycastTarget)
                        {
                            Prob($"RepairTool '{rtComp.name}': Image Raycast Target on pois paalta (ei reagoi napautukseen).");
                            if (fix) { img.raycastTarget = true; s.AppendLine("  -> korjattu"); }
                        }
                    }
                }

                // --- DentSpots (kolhu-merkit autossa, sama periaate kuin DirtSpots) ---
                // KORJATTU 16.8.2026: aiemmin tama vain valitti puuttuvasta DentSpotsista
                // eika luonut sita - koska DentSpots puuttui scenesta KOKONAAN,
                // WashScreen.Start()in activeDentCount laski aina nollaksi ja korjausvaihe
                // ohittui AINA, vaikka radalla olisi tullut kolhuja. Nyt DentSpots + muutama
                // kolhu-Image luodaan automaattisesti KORJAA-tilassa, samaan tapaan kuin
                // RepairArea+RepairTool -ikonit ylla.
                if (wash.dentSpotsParent == null && (wash.dentSpots == null || wash.dentSpots.Count == 0))
                {
                    var dentGo = Find(DENT_SPOTS);
                    bool justCreatedDentSpots = false;
                    if (dentGo == null)
                    {
                        Prob("Kolhu-merkkeja (DentSpots) ei loydy scenesta ollenkaan - korjausvaihe EI VOI koskaan " +
                             "loytaa yhtaan korjattavaa, vaikka pelaaja ajaisi kolhuja radalla.");
                        if (fix)
                        {
                            justCreatedDentSpots = true;
                            dentGo = new GameObject(DENT_SPOTS, typeof(RectTransform));
                            Undo.RegisterCreatedObjectUndo(dentGo, "Luo DentSpots");
                            dentGo.transform.SetParent(canvasT, false);
                            var dsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DENT_SPRITE_PATH);

                            // Klusteri auton alaosassa, erillaan DirtSpotsin klusterista
                            // (joka on ylempana) - ei mene paallekkain, harmaa/metallinen
                            // sävy erottaa kolhun likatahrasta vaikka sprite on sama placeholder.
                            (float x, float y)[] spotPos = {
                                (350f, 1250f), (700f, 1250f), (400f, 1170f),
                                (650f, 1170f), (520f, 1290f), (520f, 1130f),
                            };
                            for (int i = 0; i < spotPos.Length; i++)
                            {
                                var dGo = new GameObject($"DentSpot_{i + 1:00}", typeof(RectTransform));
                                dGo.transform.SetParent(dentGo.transform, false);
                                var drt = (RectTransform)dGo.transform;
                                drt.anchorMin = new Vector2(0, 1); drt.anchorMax = new Vector2(0, 1);
                                drt.sizeDelta = new Vector2(90, 90);
                                drt.anchoredPosition = new Vector2(spotPos[i].x, -spotPos[i].y);
                                var dImg = dGo.AddComponent<Image>();
                                if (dsSprite != null) dImg.sprite = dsSprite;
                                dImg.color = new Color(0.55f, 0.53f, 0.52f, 1f); // harmaa "kolhu", ei ruskea "lika"
                            }
                            s.AppendLine($"  -> DentSpots + {spotPos.Length} kolhu-merkkia luotu (placeholder-grafiikka, " +
                                         "vaihda Unityssa oikeaan kolhu-sprite jos/kun sellainen tehdaan).");
                        }
                    }
                    if (dentGo != null)
                    {
                        if (!justCreatedDentSpots) Prob("Dent Spots Parent ei kytketty.");
                        if (fix) { wash.dentSpotsParent = dentGo.transform; s.AppendLine("  -> kytketty (" + DENT_SPOTS + ")"); }
                    }
                }
                if (wash.dentSpotsParent != null)
                {
                    int dentCount = wash.dentSpotsParent.GetComponentsInChildren<Image>(true).Length;
                    s.AppendLine($"Kolhu-merkkeja DentSpotsin alla: {dentCount}");
                }
                else if (wash.dentSpots != null && wash.dentSpots.Count > 0)
                {
                    s.AppendLine($"Kolhu-merkkeja Dent Spots -listassa: {wash.dentSpots.Count}");
                }

                // --- DentsText: PITAA olla suoraan Canvasin lapsi (kuten ProgressText),
                // EI minkaan paneelin sisalla joka voi olla pois paalta. KORJATTU 16.8.2026:
                // loytyi vahingossa vanhan RepairPanelin (pois paalta koko ajan) lapsena,
                // jolloin se ei nakynyt pelaajalle KOSKAAN, vaikka WashScreen paivitti sen
                // tekstia oikein koko ajan taustalla.
                var dentsTextGo = Find(DENTS_TEXT);
                if (dentsTextGo != null && dentsTextGo.transform.parent != canvasT)
                {
                    Prob("Dents Text on jonkin paneelin sisalla (ei suoraan Canvasin lapsi) - " +
                         "jos se paneeli on pois paalta, teksti ei nay pelaajalle KOSKAAN.");
                    if (fix)
                    {
                        Undo.SetTransformParent(dentsTextGo.transform, canvasT, "Siirra DentsText Canvasin alle");
                        var dtrt = (RectTransform)dentsTextGo.transform;
                        dtrt.anchorMin = new Vector2(0, 1); dtrt.anchorMax = new Vector2(0, 1);
                        dtrt.anchoredPosition = new Vector2(22, -280);
                        dtrt.sizeDelta = new Vector2(600, 100);
                        s.AppendLine("  -> siirretty suoraan Canvasin alle, ProgressTextin alapuolelle");
                    }
                }
                if (wash.dentsText == null)
                {
                    Prob("Dents Text ei kytketty.");
                    if (fix)
                    {
                        var dtGo = Find(DENTS_TEXT);
                        var dt = dtGo != null ? dtGo.GetComponent<TMP_Text>() : null;
                        if (dt != null) { wash.dentsText = dt; s.AppendLine("  -> kytketty (" + DENTS_TEXT + ")"); }
                        else s.AppendLine("  -> objektia '" + DENTS_TEXT + "' ei loytynyt, luo TMP_Text ja kytke kasin");
                    }
                }

                // --- Vanhan yhden-napin korjaussuunnitelman jaanteet: RepairPanel+RepairButton
                // (LISATTY 16.8.2026). Nama korvattiin RepairArea+RepairTool-mekaniikalla
                // 16.8., mutta vanhat objektit jaivat sceneen. RepairButtonin OnClick viittaa
                // metodiin OnRepairButton jota WashScreenissa ei enaa ole (rikki, mutta ei
                // aiheuta kaannosvirhetta koska UnityEvent tallentaa metodin nimen tekstina).
                var legacyPanelGo = Find(LEGACY_REPAIR_PANEL);
                if (legacyPanelGo != null)
                {
                    Prob("Vanhan korjaus-napin jaanteet loytyivat scenesta (RepairPanel/RepairButton) - " +
                         "korvattu RepairArea+RepairTool-mekaniikalla, nama ovat rikkinaista kuollutta koodia.");
                    if (fix)
                    {
                        Undo.DestroyObjectImmediate(legacyPanelGo); // vie RepairButton-lapsen mukana
                        s.AppendLine("  -> RepairPanel (ja sen sisalla RepairButton) poistettu.");
                    }
                }

                // RepairEffect: pieni, hetken nakyva kuittausefekti korjauksen yhteydessa
                // (kayttaja: "korjaamiseen tulisi liittya jokin aani ja efekti"). Yksinkertainen
                // keskitetty kuittausteksti - vaihda Unityssa naeyttavampaan jos haluat
                // (esim. partikkelijarjestelma tai kipina-sprite Bubble-tyylin sijaan).
                var effectGo = Find(REPAIR_EFFECT);
                if (effectGo == null)
                {
                    Prob("RepairEffect puuttuu Pesusta - rakennetaan.");
                    if (fix)
                    {
                        effectGo = new GameObject(REPAIR_EFFECT, typeof(RectTransform));
                        Undo.RegisterCreatedObjectUndo(effectGo, "Luo RepairEffect");
                        effectGo.transform.SetParent(canvasT, false);
                        var ert = (RectTransform)effectGo.transform;
                        ert.anchorMin = new Vector2(0.5f, 0.5f);
                        ert.anchorMax = new Vector2(0.5f, 0.5f);
                        ert.pivot = new Vector2(0.5f, 0.5f);
                        ert.anchoredPosition = new Vector2(0, 120);
                        ert.sizeDelta = new Vector2(420, 100);

                        var etmp = effectGo.AddComponent<TextMeshProUGUI>();
                        etmp.text = "Korjattu!";
                        etmp.fontSize = 52;
                        etmp.fontStyle = FontStyles.Bold;
                        etmp.color = new Color(0.95f, 0.55f, 0.25f, 1f);
                        etmp.alignment = TextAlignmentOptions.Center;

                        effectGo.SetActive(false);
                        s.AppendLine("  -> RepairEffect luotu (yksinkertainen kuittausteksti, hetken nakyvissa " +
                                     "korjauksen yhteydessa) - vaihda Unityssa naeyttavampaan jos haluat.");
                    }
                }
                else s.AppendLine("RepairEffect: OK (loytyi jo scenesta).");
                if (wash.repairEffect == null && effectGo != null)
                {
                    Prob("Repair Effect ei kytketty.");
                    if (fix) { wash.repairEffect = effectGo; s.AppendLine("  -> kytketty (RepairEffect)"); }
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
                    s.AppendLine($"{label}: OnClick on jotain kytketty ({btn.onClick.GetPersistentEventCount()} kpl) — " +
                        "jos nappi kopioitu toisesta scenesta, kohde voi silti olla rikki. Aja 'Pakettiporina/Korjaus/03 - KORJAA pesu' varmuudeksi.");
                }
            }
            Wire(CONTINUE_BTN, wash.OnContinueButton, "Jatkoon-nappi (halliin)", true);
            Wire(SKIP_BTN, wash.OnSkipButton, "Ohita-nappi", false);           // valinnainen
            Wire(MAINMENU_BTN, wash.OnMainMenuButton, "Koti-nappi (paavalikko)", false);
            Wire(RESTART_BTN, wash.OnRestartButton, "Uudestaan-nappi (aloita pesu uudestaan)", false);
            // Korjaus EI enaa kaytä yhta nappia (16.8.2026) - ks. RepairArea/RepairTool ylla.

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
                var pausePanelGo = Find("PausePanel");
                if (pausePanelGo != null && pausePanelGo.activeSelf)
                {
                    Prob("Pause Panel on paalla (nakyy heti, vaikka peli ei ole viela pysaytetty).");
                    if (fix) { pausePanelGo.SetActive(false); s.AppendLine("  -> piilotettu"); }
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
                        s.AppendLine($"{label}: OnClick on jotain kytketty — jos nappi kopioitu toisesta scenesta, " +
                            "kohde voi silti olla rikki. Aja 'Pakettiporina/Korjaus/03 - KORJAA pesu' varmuudeksi.");
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
                : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty — muista Ctrl+S." : " Aja 'Pakettiporina/Korjaus/03 - KORJAA pesu'."));
            Debug.Log(s.ToString());
        }
    }
}
#endif
