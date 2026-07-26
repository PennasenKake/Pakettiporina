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
    // Hallin (Garage) tarkistus- ja korjaustyokalu.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 4 Tarkista halli / 5 KORJAA halli
    public static class PakettiporinaGarageSetup
    {
        // Odotetut objektinimet (muuta jos omat nimesi poikkeavat)
        static readonly string[] TAB_NAMES = {
            "TabKori", "TabRenkaat", "TabMoottori", "TabJouset", "TabLisat", "TabMaali"
        };
        const string PREV_PART = "PrevPartButton";
        const string NEXT_PART = "NextPartButton";
        const string PREV_PKG  = "PrevPackageButton";
        const string NEXT_PKG  = "NextPackageButton";
        const string DRIVE     = "DriveButton";
        const string GAME_SCENE = "Game";

        [MenuItem("Pakettiporina/4 - Tarkista halli")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/5 - KORJAA halli")]
        public static void FixAll() { Run(true); }

        // --- apuri: etsi objekti nimella, myos piilotetut ---
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
            s.AppendLine(fix ? "=== HALLIN KORJAUS ===" : "=== HALLIN TARKISTUS ===");
            int problems = 0;
            void Prob(string msg) { s.AppendLine("! " + msg); problems++; }

            var garage  = Object.FindObjectOfType<GarageScreen>(true);
            var builder = Object.FindObjectOfType<CarBuilder>(true);
            var canvas  = Object.FindObjectOfType<Canvas>(true);
            var evsys   = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true);
            var gm      = Object.FindObjectOfType<GameManager>(true);

            if (garage == null)
            {
                s.AppendLine("VIRHE: GarageScreen puuttuu scenesta. Lisaa se Canvasiin.");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- 1. CarBuilder ----------
            if (builder == null)
            {
                Prob("CarBuilder puuttuu.");
                if (fix)
                {
                    var go = new GameObject("CarBuilder");
                    builder = go.AddComponent<CarBuilder>();
                    Undo.RegisterCreatedObjectUndo(go, "CarBuilder");
                    s.AppendLine("  -> luotu");
                }
            }
            if (builder != null && garage.builder != builder)
            {
                Prob("GarageScreen.builder ei kytketty.");
                if (fix) { garage.builder = builder; s.AppendLine("  -> kytketty"); }
            }

            // ---------- 2. EventSystem ----------
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

            // ---------- 3. GameManager ----------
            if (gm == null)
            {
                Prob("GameManager puuttuu hallista (valinta ei tallennu jos aloitat tasta).");
                if (fix)
                {
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                    Undo.RegisterCreatedObjectUndo(go, "GameManager");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 4. Canvas Scaler ----------
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

            // ---------- 5. Osat ja paketit AUTOMAATTISESTI ----------
            var partGuids = AssetDatabase.FindAssets("t:PartData");
            var pkgGuids  = AssetDatabase.FindAssets("t:PackageData");
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
            allParts.Sort((a, b) => a.category != b.category
                ? a.category.CompareTo(b.category)
                : string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            s.AppendLine($"Projektissa: {allParts.Count} osaa, {allPkgs.Count} pakettia.");
            if (garage.allParts.Count != allParts.Count || garage.allPackages.Count != allPkgs.Count)
            {
                Prob($"Listat eivat tasmaa (hallissa {garage.allParts.Count} osaa / {garage.allPackages.Count} pakettia).");
                if (fix)
                {
                    garage.allParts = allParts;
                    garage.allPackages = allPkgs;
                    s.AppendLine("  -> ladattu automaattisesti projektista");
                }
            }
            else s.AppendLine("Osa- ja pakettilistat: OK");

            // datan laatutarkistus
            foreach (var p in allParts)
            {
                if (string.IsNullOrWhiteSpace(p.displayName) || p.displayName == "Uusi osa")
                    Prob($"Osalta '{p.name}' puuttuu Display Name.");
                if (!p.cosmeticOnly && p.voima == 0 && p.pito == 0 && p.keveys == 0 && p.kestavyys == 0 && p.kylmyys == 0)
                    Prob($"Osalla '{p.name}' kaikki arvot ovat 0 (ei vaikuta mihinkaan).");
            }
            foreach (var pk in allPkgs)
            {
                if (pk.requiredPart != null && !allParts.Contains(pk.requiredPart))
                    Prob($"Paketti '{pk.name}' vaatii osan jota ei ole listassa -> mahdoton keikka!");
            }

            // ---------- 6. Tekstikentat ----------
            void NeedText(ref TMP_Text field, string objName, string label)
            {
                if (field != null) return;
                Prob($"{label} ei kytketty.");
                if (!fix) return;
                var go = Find(objName);
                var t = go != null ? go.GetComponent<TMP_Text>() : null;
                if (t != null) { field = t; s.AppendLine($"  -> kytketty ({objName})"); }
                else s.AppendLine($"  -> objektia '{objName}' ei loytynyt, kytke kasin");
            }
            var cat = garage.categoryText;   NeedText(ref cat, "CategoryText", "Category Text");    garage.categoryText = cat;
            var pn  = garage.partNameText;   NeedText(ref pn,  "PartNameText", "Part Name Text");   garage.partNameText = pn;
            var pkn = garage.packageNameText;NeedText(ref pkn, "PackageNameText","Package Name Text");garage.packageNameText = pkn;
            var ft  = garage.fitText;        NeedText(ref ft,  "FitText", "Fit Text");              garage.fitText = ft;

            // ---------- 7. Mittarit ----------
            void NeedBar(ref Image field, string objName, string label)
            {
                if (field == null)
                {
                    Prob($"{label} ei kytketty.");
                    if (fix)
                    {
                        var go = Find(objName);
                        var im = go != null ? go.GetComponent<Image>() : null;
                        if (im != null) { field = im; s.AppendLine($"  -> kytketty ({objName})"); }
                    }
                }
                if (field != null && field.type != Image.Type.Filled)
                {
                    Prob($"{label}: Image Type ei ole Filled (mittari ei liiku).");
                    if (fix)
                    {
                        field.type = Image.Type.Filled;
                        field.fillMethod = Image.FillMethod.Horizontal;
                        field.fillOrigin = (int)Image.OriginHorizontal.Left;
                        s.AppendLine("  -> korjattu Filled / Horizontal / Left");
                    }
                }
            }
            var b1 = garage.barVoima;     NeedBar(ref b1, "BarVoima", "Bar Voima");         garage.barVoima = b1;
            var b2 = garage.barPito;      NeedBar(ref b2, "BarPito", "Bar Pito");           garage.barPito = b2;
            var b3 = garage.barKeveys;    NeedBar(ref b3, "BarKeveys", "Bar Keveys");       garage.barKeveys = b3;
            var b4 = garage.barKestavyys; NeedBar(ref b4, "BarKestavyys", "Bar Kestavyys"); garage.barKestavyys = b4;
            var b5 = garage.barKylmyys;   NeedBar(ref b5, "BarKylmyys", "Bar Kylmyys");     garage.barKylmyys = b5;

            // ---------- 8. Auton esikatselu ----------
            if (garage.carPreview == null)
            {
                Prob("Car Preview ei kytketty (varinvalinta ei nay).");
                if (fix)
                {
                    var go = Find("Car") ?? Find("CarPreview");
                    var im = go != null ? go.GetComponent<Image>() : null;
                    if (im != null) { garage.carPreview = im; s.AppendLine("  -> kytketty"); }
                }
            }

            // ---------- 9. Valilehdet + korostus ----------
            var tabs = new Button[TAB_NAMES.Length];
            bool tabsOk = true;
            for (int i = 0; i < TAB_NAMES.Length; i++)
            {
                var go = Find(TAB_NAMES[i]);
                tabs[i] = go != null ? go.GetComponent<Button>() : null;
                if (tabs[i] == null) { Prob($"Valilehti '{TAB_NAMES[i]}' puuttuu."); tabsOk = false; }
            }
            if (tabsOk)
            {
                bool same = garage.categoryTabs != null && garage.categoryTabs.Length == tabs.Length;
                if (same)
                    for (int i = 0; i < tabs.Length; i++)
                        if (garage.categoryTabs[i] != tabs[i]) { same = false; break; }
                if (!same)
                {
                    Prob("Category Tabs -lista puuttuu tai on vaarassa jarjestyksessa.");
                    if (fix) { garage.categoryTabs = tabs; s.AppendLine("  -> kytketty oikeassa jarjestyksessa"); }
                }
                else s.AppendLine("Category Tabs: OK");
            }

            // ---------- 10. Nappien OnClick ----------
            void Wire(string objName, UnityAction action, string label)
            {
                var go = Find(objName);
                var btn = go != null ? go.GetComponent<Button>() : null;
                if (btn == null) { Prob($"Nappi '{objName}' puuttuu."); return; }
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    Prob($"{label}: OnClick tyhja.");
                    if (fix)
                    {
                        ClearClicks(btn);
                        UnityEventTools.AddPersistentListener(btn.onClick, action);
                        s.AppendLine("  -> kytketty");
                    }
                }
            }
            Wire(PREV_PART, garage.PrevPart,    "Edellinen osa");
            Wire(NEXT_PART, garage.NextPart,    "Seuraava osa");
            Wire(PREV_PKG,  garage.PrevPackage, "Edellinen paketti");
            Wire(NEXT_PKG,  garage.NextPackage, "Seuraava paketti");
            Wire(DRIVE,     garage.OnDrive,     "Aja keikka");

            if (fix && tabsOk)
            {
                var call = new UnityAction<int>(garage.SetCategory);
                for (int i = 0; i < tabs.Length; i++)
                {
                    ClearClicks(tabs[i]);
                    UnityEventTools.AddIntPersistentListener(tabs[i].onClick, call, i);
                }
                s.AppendLine("Valilehtien OnClick -> SetCategory(0..5) kytketty.");
            }

            // ---------- 11. Scenen nimi ----------
            if (garage.gameSceneName != GAME_SCENE)
            {
                Prob($"Game Scene Name = '{garage.gameSceneName}' (odotettu '{GAME_SCENE}').");
                if (fix) { garage.gameSceneName = GAME_SCENE; s.AppendLine("  -> asetettu '" + GAME_SCENE + "'"); }
            }

            // ---------- valmis ----------
            if (fix)
            {
                EditorUtility.SetDirty(garage);
                if (builder != null) EditorUtility.SetDirty(builder);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            s.AppendLine(problems == 0
                ? "\nKaikki kunnossa!"
                : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty — muista Ctrl+S." : " Aja '5 - KORJAA halli'."));
            Debug.Log(s.ToString());
        }
    }
}
#endif