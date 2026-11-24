#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // LISATTY 24.11.2026: edistyksen (ajotahdet, kolikot, lukitukset) eheystarkistus.
    // Kaytto: Pakettiporina -> Tarkistus/08 - Tarkista edistys (Progress)
    // Varmistaa ettei Progress.cs:n taulukko ja pelin data ole ristiriidassa:
    //  - taulukon nimet loytyvat oikeista assetteista (kirjoitusvirhe = lukitus ei toimi)
    //  - paketin vaatima osa on auki jo kun paketti aukeaa (muuten mahdoton keikka)
    //  - jokaisessa kategoriassa on vahintaan yksi osa auki nollalla tahdella (oletusauto)
    //  - radat ovat Build Settingsissa ja niilla on kuva; paketin ehdottama rata ei ole paketti ennen auki
    //  - osat/paketit joilla on vanha unlockPoints mutta ei taulukkoriviä (kayttaytyy kolikkorajana)
    public static class PakettiporinaProgressCheck
    {
        [MenuItem("Pakettiporina/Tarkistus/08 - Tarkista edistys (Progress)")]
        public static void Run()
        {
            Run(true);
        }

        // Tarkistus/02 (halli) kutsuu tata hiljaisesti omaan raporttiinsa.
        public static int RunQuiet() { return Run(false); }

        static int Run(bool log)
        {
            var s = new StringBuilder("=== EDISTYS: TARKISTUS ===\n");
            int problems = 0;
            void Prob(string m) { s.AppendLine("! " + m); problems++; }
            void Info(string m) { s.AppendLine("i " + m); }

            var parts = LoadParts();
            var pkgs = LoadPkgs();

            // 1) taulukon nimet
            var partNames = new HashSet<string>(parts.Select(p => p.name));
            var pkgNames = new HashSet<string>(pkgs.Select(p => p.name));
            foreach (var kv in Progress.PartTable)
                if (!partNames.Contains(kv.Key)) Prob("Progress.parts: '" + kv.Key + "' ei ole PartData-asset (kirjoitusvirhe?).");
            foreach (var kv in Progress.PackageTable)
                if (!pkgNames.Contains(kv.Key)) Prob("Progress.packages: '" + kv.Key + "' ei ole PackageData-asset (kirjoitusvirhe?).");

            // 2) paketin vaatima osa auki kun paketti aukeaa
            foreach (var pk in pkgs)
            {
                if (pk.requiredPart == null) continue;
                int partNeed = Mathf.Max(0, Progress.PartNeeded(pk.requiredPart));
                int pkgNeed = Mathf.Max(0, Progress.PackageNeeded(pk));
                if (pk.requiredPart.secret) Prob("Paketti '" + pk.name + "' vaatii salaisen osan '" + pk.requiredPart.name + "' -> melkein mahdoton.");
                if (partNeed > pkgNeed) Prob("Paketti '" + pk.name + "' aukeaa " + pkgNeed + " tahdella mutta vaatima osa '" + pk.requiredPart.name + "' vasta " + partNeed + " tahdella -> umpikuja.");
            }

            // 3) oletusauto: jokaisessa kategoriassa avoin osa 0 tahdella
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                var open = parts.Where(p => p.category == c && !p.secret && Progress.PartNeeded(p) <= 0 && p.unlockPoints <= 0).ToList();
                if (open.Count == 0) Prob("Kategoriassa " + c + " ei ole yhtaan osaa auki alussa (oletusauto rikki).");
            }

            // 4) radat
            var buildScenes = new HashSet<string>(EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => Path.GetFileNameWithoutExtension(x.path)));
            var allBuild = EditorBuildSettings.scenes.Select(x => Path.GetFileNameWithoutExtension(x.path)).ToList();
            foreach (var kv in Progress.TrackTable)
            {
                string scene = allBuild.FirstOrDefault(n => Progress.TrackKey(n) == kv.Key);
                bool planned = Progress.IsPlanned(kv.Key);
                if (planned && (scene == null || !buildScenes.Contains(scene)))
                    Info("Rata '" + kv.Key + "' on suunniteltu myohemmaksi (Progress.PlannedTracks): scene tarkoituksella pois Build Settingsista, Halli ei nayta sita.");
                else if (scene == null) Prob("Rata '" + kv.Key + "': scenea ei ole Build Settingsissa lainkaan.");
                else if (!buildScenes.Contains(scene)) Prob("Rata '" + scene + "' (avautuu " + kv.Value + " tahdella) on Build Settingsissa POIS PAALTA -> Hallin ratalista ei nayta sita.");
                if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "_Pakettiporina/Resources/WizardUI/rata_" + kv.Key + ".png"))))
                    Prob("Radalta '" + kv.Key + "' puuttuu kuva Resources/WizardUI/rata_" + kv.Key + ".png (aja Korjaus/10).");
            }
            // 4b) yksi ratalista: Progress.RaceScenes <-> Progress-taulukko <-> PakettiporinaTracks.ALL
            foreach (var kv in Progress.TrackTable)
            {
                bool inRace = Progress.RaceScenes.Any(r => Progress.TrackKey(r) == kv.Key);
                if (!inRace) Prob("Rata '" + kv.Key + "' puuttuu Progress.RaceScenes-listasta (musiikki/suorituskykyasetukset eivat tunne sita).");
                if (PakettiporinaTracks.ALL.All(t => Progress.TrackKey(t.sceneName) != kv.Key))
                    Prob("Rata '" + kv.Key + "' puuttuu PakettiporinaTracks.ALL-listasta (editorityokalut eivat tunne sita).");
            }
            foreach (var t in PakettiporinaTracks.ALL)
            {
                string k = Progress.TrackKey(t.sceneName);
                if (!Progress.TrackTable.Any(x => x.Key == k)) Info("PakettiporinaTracks-rata '" + t.sceneName + "' ei ole Progress.tracks-taulukossa (aina auki, ei Hallin ratalistalla).");
                if (!Progress.RaceScenes.Contains(t.sceneName)) Prob("Rata '" + t.sceneName + "' puuttuu Progress.RaceScenes-listasta.");
            }

            // 4c) Hallin (wizard) kuvat: jokainen Resources/WizardUI-kuva jota GarageWizard lataa
            string[] wizImgs = { "kategoria_kori", "kategoria_renkaat", "kategoria_moottori", "kategoria_jouset", "kategoria_lisat", "kategoria_maali",
                "tahti", "koti", "vilkku", "lukko", "nuoli_vasen_coral", "nuoli_oikea_coral", "pelaa", "rata_perus",
                "mittari_voima", "mittari_pito", "mittari_keveys", "mittari_kestavyys", "mittari_kylmyys" };
            foreach (var n in wizImgs)
                if (Resources.Load<Texture2D>("WizardUI/" + n) == null) Prob("Wizard-kuva puuttuu: Resources/WizardUI/" + n + ".png (aja Korjaus/10).");

            foreach (var pk in pkgs)
            {
                if (string.IsNullOrEmpty(pk.trackScene)) continue;
                int tn = Progress.TrackNeeded(pk.trackScene), pn = Mathf.Max(0, Progress.PackageNeeded(pk));
                if (tn > pn) Info("Paketti '" + pk.name + "' aukeaa " + pn + " tahdella, mutta sen ehdottama rata '" + pk.trackScene + "' vasta " + tn + ": Halli ehdottaa kotirataa kunnes rata aukeaa (ok, mutta lapsi voi ihmetella).");
            }

            // 5) vanha kolikkoraja ilman taulukkoriviä
            foreach (var p in parts)
                if (!p.secret && p.unlockPoints > 0 && Progress.PartNeeded(p) < 0)
                    Info("Osa '" + p.name + "': unlockPoints=" + p.unlockPoints + " mutta ei Progress-riviä -> toimii vanhana kolikkorajana (kolikot kuluvat tarroihin!). Lisaa osa Progress.cs:aan.");
            foreach (var p in pkgs)
                if (p.unlockPoints > 0 && Progress.PackageNeeded(p) < 0)
                    Info("Paketti '" + p.name + "': unlockPoints=" + p.unlockPoints + " mutta ei Progress-riviä -> vanha kolikkoraja.");
            foreach (var p in parts.Where(x => x.secret))
                if (Progress.PartNeeded(p) < 0) Prob("Salainen osa '" + p.name + "' ei ole Progress-taulukossa: aukeaa kolikkorajalla ja voi lukittua uudelleen tarraostoista.");

            // 6) tahtimaara: paljonko tarvitaan kaikkeen
            int maxNeed = Progress.PartTable.Concat(Progress.PackageTable).Concat(Progress.TrackTable).Select(x => x.Value).DefaultIfEmpty(0).Max();
            int runsAt2 = Mathf.CeilToInt(maxNeed / 2f);
            Info("Viimeinen avaus vaatii " + maxNeed + " ajotahtea = noin " + runsAt2 + " ajoa jos keskimaarin 2 tahtea/ajo (3 tahtea = " + Mathf.CeilToInt(maxNeed / 3f) + " ajoa).");
            Info("Tarroja: " + AssetDatabase.FindAssets("t:StickerData").Length + " kpl (ostetaan kolikoilla).");

            s.AppendLine(problems == 0 ? "OK: ei ongelmia." : problems + " ongelmaa.");
            try
            {
                string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Edistys_tarkistus_viimeisin.md"));
                File.WriteAllText(file, "# Edistyksen tarkistus " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n\n```\n" + s + "```\n", new UTF8Encoding(true));
            }
            catch (System.Exception e) { Debug.LogWarning("[Progress] Raportin kirjoitus epäonnistui: " + e.Message); }
            if (log) { if (problems == 0) Debug.Log(s.ToString()); else Debug.LogWarning(s.ToString()); }
            return problems;
        }
    }
}
#endif
