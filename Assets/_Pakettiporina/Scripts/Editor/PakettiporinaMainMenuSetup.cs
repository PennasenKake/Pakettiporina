#if UNITY_EDITOR
using System.Collections.Generic;
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
    // Paavalikon (MainMenu) tarkistus- ja korjaustyokalu.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 11 Tarkista paavalikko / 12 KORJAA paavalikko
    //
    // Odottaa scenesta naita nimia (muuta vakioita jos omat nimesi poikkeavat):
    //   PlayButton, QuitButton      - MainMenun perusnapit
    //   PointsText                  - TMP_Text, nayttaa pisteet (PointsDisplay-komponentti)
    //   Sticker_Points_Button       - avaa/sulkee tarrapaneelin
    //   StickerPanel                - StickerPanel-komponentti, sis. "StickerGrid"-lapsen
    //                                 jonka lapset nimetaan StickerData-assetin nimen mukaan
    public static class PakettiporinaMainMenuSetup
    {
        const string PLAY_BTN = "PlayButton";
        const string QUIT_BTN = "QuitButton";
        const string POINTS_TEXT = "PointsText";
        const string STICKER_BTN = "Sticker_Points_Button";
        const string STICKER_PANEL = "StickerPanel";
        const string STICKER_GRID = "StickerGrid";
        const string GARAGE_SCENE = "Garage";

        [MenuItem("Pakettiporina/11 - Tarkista paavalikko")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/12 - KORJAA paavalikko")]
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

        // Etsii SUORAN lapsen nimella, kirjainkoosta valittamatta (esim. "lock" == "Lock").
        static Transform FindChildCI(Transform parent, string name)
        {
            foreach (Transform t in parent)
                if (string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase))
                    return t;
            return null;
        }

        // Etsii lapsiobjektin nimella rekursiivisesti (kaikista jalkelaisista, ei vain
        // suorista lapsista), kirjainkoosta valittamatta - kaytetaan Lock/LockText-lapsien
        // tunnistukseen silla varalta etta ne on vahingossa sijoitettu esim. "Lock"-objektin
        // ALLE eika sen viereen, tai nimetty hieman eri kirjainkoolla.
        static Transform FindDeepCI(Transform root, string name)
        {
            foreach (Transform t in root)
            {
                if (string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase)) return t;
                var found = FindDeepCI(t, name);
                if (found != null) return found;
            }
            return null;
        }

        // Etsii TMP_Text-objektin root:in lapsista (rekursiivisesti) jonka nimi tasmaa
        // johonkin candidateNamesLower-listan nimeen (pienina kirjaimina). Kaytetaan
        // StickerPanelin sisaisen pistetekstin tunnistamiseen, koska kasin tehty objekti
        // paatyy helposti hieman eri nimelle (esim. "pointsText" "StickerPointsText":n
        // sijaan) - tarkka nimihaku jattaisi sen silloin huomaamatta ja pisteet jaisivat
        // paneelissa aina lukemaan placeholder-tekstin.
        static GameObject FindTmpByNames(Transform root, params string[] candidateNamesLower)
        {
            foreach (Transform t in root)
            {
                if (t.GetComponent<TMP_Text>() != null)
                {
                    string n = t.name.ToLowerInvariant();
                    foreach (var cand in candidateNamesLower)
                        if (n == cand) return t.gameObject;
                }
                var found = FindTmpByNames(t, candidateNamesLower);
                if (found != null) return found;
            }
            return null;
        }

        static void Run(bool fix)
        {
            var s = new StringBuilder();
            s.AppendLine(fix ? "=== PAAVALIKON KORJAUS ===" : "=== PAAVALIKON TARKISTUS ===");
            int problems = 0;
            void Prob(string msg) { s.AppendLine("! " + msg); problems++; }

            var menu = Object.FindObjectOfType<MainMenu>(true);
            var evsys = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true);

            if (menu == null)
            {
                s.AppendLine("VIRHE: MainMenu-komponenttia ei loydy scenesta. Lisaa se jollekin objektille.");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- 1. EventSystem ----------
            if (evsys == null)
            {
                Prob("EventSystem puuttuu (napit eivat toimi).");
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
            var gm = Object.FindObjectOfType<GameManager>(true);
            if (gm == null)
            {
                Prob("GameManager puuttuu paavalikosta (peli ei toimi ilman sita).");
                if (fix)
                {
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                    Undo.RegisterCreatedObjectUndo(go, "GameManager");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 3. AudioManager ----------
            var audioMgr = Object.FindObjectOfType<AudioManager>(true);
            if (audioMgr == null)
            {
                Prob("AudioManager puuttuu paavalikosta (aanet eivat soi).");
                if (fix)
                {
                    var go = new GameObject("AudioManager");
                    go.AddComponent<AudioManager>();
                    Undo.RegisterCreatedObjectUndo(go, "AudioManager");
                    s.AppendLine("  -> luotu (muista raahata aaniklipit sen Inspectoriin)");
                }
            }

            // ---------- 4. SaveManager + listat ----------
            var saveMgr = Object.FindObjectOfType<SaveManager>(true);
            if (saveMgr == null)
            {
                Prob("SaveManager puuttuu paavalikosta (tallennus/lataus ei toimi).");
                if (fix)
                {
                    var go = new GameObject("SaveManager");
                    saveMgr = go.AddComponent<SaveManager>();
                    Undo.RegisterCreatedObjectUndo(go, "SaveManager");
                    s.AppendLine("  -> luotu");
                }
            }
            if (saveMgr != null)
            {
                var partGuids = AssetDatabase.FindAssets("t:PartData");
                var pkgGuids = AssetDatabase.FindAssets("t:PackageData");
                var allParts = new List<PartData>();
                foreach (var g in partGuids)
                {
                    var p = AssetDatabase.LoadAssetAtPath<PartData>(AssetDatabase.GUIDToAssetPath(g));
                    if (p != null) allParts.Add(p);
                }
                var allPkgs = new List<PackageData>();
                foreach (var g in pkgGuids)
                {
                    var p = AssetDatabase.LoadAssetAtPath<PackageData>(AssetDatabase.GUIDToAssetPath(g));
                    if (p != null) allPkgs.Add(p);
                }
                if (saveMgr.allParts.Count != allParts.Count || saveMgr.allPackages.Count != allPkgs.Count)
                {
                    Prob($"SaveManagerin listat eivat tasmaa ({saveMgr.allParts.Count} osaa / {saveMgr.allPackages.Count} pakettia).");
                    if (fix)
                    {
                        saveMgr.allParts = allParts;
                        saveMgr.allPackages = allPkgs;
                        EditorUtility.SetDirty(saveMgr);
                        s.AppendLine("  -> SaveManagerin listat ladattu");
                    }
                }
                else s.AppendLine("SaveManagerin listat: OK");
            }

            // ---------- 5. Pisteteksti (PointsDisplay) ----------
            var ptGo = Find(POINTS_TEXT);
            if (ptGo == null)
            {
                Prob($"'{POINTS_TEXT}'-objektia ei loydy (pisteet eivat nay paavalikossa).");
            }
            else
            {
                var pd = ptGo.GetComponent<PointsDisplay>();
                var tmp = ptGo.GetComponent<TMP_Text>();
                if (tmp == null)
                {
                    Prob($"'{POINTS_TEXT}'-objektilta puuttuu TMP_Text-komponentti.");
                }
                else if (pd == null)
                {
                    Prob("PointsDisplay-komponentti puuttuu.");
                    if (fix)
                    {
                        pd = ptGo.AddComponent<PointsDisplay>();
                        pd.pointsText = tmp;
                        s.AppendLine("  -> lisatty ja kytketty");
                    }
                }
                else s.AppendLine("PointsDisplay: OK");
            }

            // ---------- 6. Tarrapaneeli (valinnainen) ----------
            var panelGo = Find(STICKER_PANEL);
            StickerPanel stickerPanel = null;
            if (panelGo == null)
            {
                s.AppendLine($"Tarrapaneeli ('{STICKER_PANEL}') ei loydy - ei pakollinen, mutta '{STICKER_BTN}' " +
                             "ei tee mitaan ennen kuin se on luotu. Ks. suunnitelmadokumentti.");
            }
            else
            {
                stickerPanel = panelGo.GetComponent<StickerPanel>();
                if (stickerPanel == null)
                {
                    Prob("StickerPanel-komponentti puuttuu Sticker Panel -objektilta.");
                    if (fix) { stickerPanel = panelGo.AddComponent<StickerPanel>(); s.AppendLine("  -> lisatty"); }
                }
                if (stickerPanel != null && stickerPanel.panelRoot == null)
                {
                    Prob("StickerPanel.Panel Root ei kytketty.");
                    if (fix) { stickerPanel.panelRoot = panelGo; EditorUtility.SetDirty(stickerPanel); s.AppendLine("  -> kytketty (itseensa)"); }
                }

                // Pisteteksti paneelin sisalla (valinnainen). Haetaan STICKER_PANELin OMASTA
                // hierarkiasta (ei koko scenesta) jotta ei vahingossa napata paavalikon
                // Sticker_Points_Buttonin omaa "PointsText"-objektia. Hyvaksytaan useampi
                // nimivariantti, koska kasin nimeaminen menee helposti hieman eri lailla.
                var spTextGo = FindTmpByNames(panelGo.transform, "stickerpointstext", "pointstext");
                if (spTextGo != null && stickerPanel != null)
                {
                    var tmp = spTextGo.GetComponent<TMP_Text>();
                    if (tmp != null && stickerPanel.pointsText != tmp)
                    {
                        Prob($"StickerPanel.Points Text ei kytketty (loytyi '{spTextGo.name}').");
                        if (fix) { stickerPanel.pointsText = tmp; EditorUtility.SetDirty(stickerPanel); s.AppendLine($"  -> kytketty ('{spTextGo.name}')"); }
                    }
                    else if (tmp != null) s.AppendLine($"StickerPanel.Points Text: OK ('{spTextGo.name}').");
                }
                else
                {
                    s.AppendLine($"StickerPanelin sisalta ei loytynyt pistetekstia (valinnainen - nimea 'StickerPointsText' jos haluat sen).");
                }

                // Slottien automaattinen tunnistus: StickerGrid-lapsen jokainen lapsi
                // nimetaan StickerData-assetin nimen mukaan (esim. "Vilkku.asset" <-> "Vilkku"-objekti).
                // HUOM: rakennetaan JOKA fix-ajolla uudestaan tyhjasta (ei vain jos maara
                // muuttuu) - nain esim. myohemmin lisatyt Lock-lapset kytkeytyvat aina, eika
                // vanha, osittain tyhja Slots-lista jaa "piiloon" pelkan lukumaaran takana.
                var gridGo = Find(STICKER_GRID);
                if (gridGo == null)
                {
                    s.AppendLine($"'{STICKER_GRID}' ei loydy - tarraslotteja ei voida tunnistaa automaattisesti.");
                }
                else if (stickerPanel != null)
                {
                    var stickerGuids = AssetDatabase.FindAssets("t:StickerData");
                    var stickerByName = new Dictionary<string, StickerData>();
                    foreach (var g in stickerGuids)
                    {
                        var sd = AssetDatabase.LoadAssetAtPath<StickerData>(AssetDatabase.GUIDToAssetPath(g));
                        if (sd != null) stickerByName[sd.name.ToLowerInvariant()] = sd;
                    }
                    if (stickerGuids.Length == 0)
                        s.AppendLine("Projektista ei loytynyt yhtaan StickerData-assetia (Assets > Create > Pakettiporina > Sticker).");

                    var newSlots = new List<StickerPanel.Slot>();
                    var newButtons = new List<Button>();
                    var missingLock = new List<string>();
                    var missingLockText = new List<string>();
                    int matched = 0, unmatched = 0, withLock = 0, withLockText = 0, withButton = 0;
                    foreach (Transform child in gridGo.transform)
                    {
                        var img = child.GetComponent<Image>();
                        if (img == null) continue;
                        string key = child.name.ToLowerInvariant();
                        if (!stickerByName.TryGetValue(key, out var sticker))
                        {
                            unmatched++;
                            continue;
                        }
                        // Suora lapsi ensin (odotettu sijainti, kirjainkoosta valittamatta),
                        // sitten rekursiivinen varahaku silla varalta etta LockText paatyi
                        // vahingossa Lock-objektin ALLE eika sen viereen.
                        var lockChild = FindChildCI(child, "Lock");
                        var lockTextChild = FindChildCI(child, "LockText") ?? FindDeepCI(child, "LockText");
                        if (lockChild != null) withLock++; else missingLock.Add(child.name);
                        if (lockTextChild != null) withLockText++; else missingLockText.Add(child.name);

                        // Slotin nappi: kaytetaan objektilla jo olevaa Buttonia, tai lisataan yksi
                        // (koko slotti napautettavaksi -> ostaa lukossa olevan tarran).
                        var btn = child.GetComponent<Button>();
                        if (btn == null && fix)
                        {
                            btn = child.gameObject.AddComponent<Button>();
                            btn.transition = Selectable.Transition.None;
                        }
                        if (btn != null && btn.targetGraphic == null) btn.targetGraphic = img;
                        if (btn != null) withButton++;

                        newSlots.Add(new StickerPanel.Slot
                        {
                            sticker = sticker,
                            image = img,
                            lockOverlay = lockChild != null ? lockChild.gameObject : null,
                            lockText = lockTextChild != null ? lockTextChild.GetComponent<TMP_Text>() : null,
                            button = btn
                        });
                        newButtons.Add(btn);
                        matched++;
                    }

                    bool needsRebuild = fix; // aina uudelleen fix-ajolla, katso HUOM yllä
                    if (!fix && matched != stickerPanel.slots.Count)
                        Prob($"StickerPanel.Slots ei tasmaa (loytyi {matched} kytkettavaa, oli {stickerPanel.slots.Count}).");

                    if (needsRebuild)
                    {
                        stickerPanel.slots = newSlots;
                        EditorUtility.SetDirty(stickerPanel);
                        s.AppendLine($"  -> {matched} slottia kytketty (Lock-peite: {withLock}/{matched}, LockText: {withLockText}/{matched}, Nappi: {withButton}/{matched}).");

                        // Nappien OnClick -> StickerPanel.OnSlotClicked(index), indeksipohjaisesti
                        // (sama kaava kuin Garagen kategoriavalilehdilla).
                        var call = new UnityAction<int>(stickerPanel.OnSlotClicked);
                        for (int i = 0; i < newButtons.Count; i++)
                        {
                            if (newButtons[i] == null) continue;
                            ClearClicks(newButtons[i]);
                            UnityEventTools.AddIntPersistentListener(newButtons[i].onClick, call, i);
                        }
                        s.AppendLine($"  -> {withButton} napin OnClick -> OnSlotClicked(index) kytketty.");
                    }
                    else
                    {
                        s.AppendLine($"StickerPanel.Slots: OK ({matched} kpl, Lock-peite: {withLock}/{matched}, LockText: {withLockText}/{matched}, Nappi: {withButton}/{matched}).");
                    }

                    if (missingLock.Count > 0)
                        s.AppendLine($"VAROITUS: {missingLock.Count} tarralta puuttuu 'Lock'-lapsi (lukko ei nay koskaan niilla): " +
                                     string.Join(", ", missingLock));
                    if (missingLockText.Count > 0)
                        s.AppendLine($"VAROITUS: {missingLockText.Count} tarralta puuttuu 'LockText'-lapsi (pistemaara ei nay lukossa): " +
                                     string.Join(", ", missingLockText));
                    if (unmatched > 0)
                        s.AppendLine($"VAROITUS: {unmatched} StickerGridin lasta ei tasmaa minkaan StickerData-assetin nimeen.");
                }
            }

            // ---------- 7. Napit ----------
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
            }
            Wire(PLAY_BTN, menu.PlayGame, "Pelaa-nappi", true);
            Wire(QUIT_BTN, menu.QuitGame, "Lopeta-nappi", false);
            if (stickerPanel != null)
                Wire(STICKER_BTN, stickerPanel.Toggle, "Tarrat/Pisteet-nappi", false);

            // ---------- 8. Scenen nimi ----------
            if (menu.gameSceneName != GARAGE_SCENE)
            {
                Prob($"Game Scene Name = '{menu.gameSceneName}' (odotettu '{GARAGE_SCENE}').");
                if (fix) { menu.gameSceneName = GARAGE_SCENE; s.AppendLine("  -> asetettu '" + GARAGE_SCENE + "'"); }
            }
            else s.AppendLine("Game Scene Name: OK (" + GARAGE_SCENE + ").");

            // ---------- valmis ----------
            if (fix)
            {
                EditorUtility.SetDirty(menu);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            s.AppendLine(problems == 0
                ? "\nKaikki kunnossa!"
                : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty - muista Ctrl+S." : " Aja '12 - KORJAA paavalikko'."));
            Debug.Log(s.ToString());
        }
    }
}
#endif
