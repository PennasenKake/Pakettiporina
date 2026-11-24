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
    // LISATTY 23.11.2026: kolmivaiheisen Hallin (GarageWizard) tarkistus ja kuvien synkkaus.
    // Kaytto: Pakettiporina -> Tarkistus/07 - Tarkista Hallin kolmivaihe
    //         Pakettiporina -> Korjaus/10 - Synkkaa Hallin kuvat (Resources/WizardUI)
    // Wizard lataa kuvansa Resources/WizardUI-kansiosta (kopioita Art/UI-kuvista). Tama tyokalu varmistaa
    // etta kuvat ovat olemassa ja ajan tasalla, ja etta jokaisella paketilla on ikoni, rata ja kuva.
    public static class PakettiporinaWizardCheck
    {
        const string Root = "Assets/_Pakettiporina/";
        const string Target = Root + "Resources/WizardUI/";

        // Resurssin nimi -> lahdetiedosto (suhteessa Assets/_Pakettiporina/)
        static Dictionary<string, string> Sources()
        {
            var d = new Dictionary<string, string>();
            foreach (var n in new[] { "kori", "renkaat", "moottori", "jouset", "lisat", "maali" })
                d["kategoria_" + n] = "Art/UI/Icons/pakettiporina_ikoni_kategoria_" + n + ".png";
            foreach (var n in new[] { "voima", "pito", "keveys", "kestavyys", "kylmyys" })
                d["mittari_" + n] = "Art/UI/Icons/pakettiporina_ikoni_mittari_" + n + ".png";
            d["tahti"] = "Art/UI/Icons/pakettiporina_ikoni_tahti.png";
            d["koti"] = "Art/UI/Icons/pakettiporina_ikoni_koti.png";
            d["pelaa"] = "Art/UI/Icons/pakettiporina_ikoni_pelaa.png";
            d["nuoli_oikea_coral"] = "Art/UI/Icons/nuoli_oikea_coral.png";
            d["nuoli_vasen_coral"] = "Art/UI/Icons/nuoli_vasen_coral.png";
            d["lukko"] = "Art/UI/Icons/pakettiporina_icon_lukko.png";
            d["vilkku"] = "Art/UI/pakettiporina_vilkku_hahmo.png";
            foreach (var n in new[] { "satama", "puisto", "huvipuisto", "maatila", "kyla", "perus" })
                d["rata_" + n] = "Art/UI/Rata/rata_" + n + ".png";
            return d;
        }

        static string Abs(string assetPath) { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath)); }

        [MenuItem("Pakettiporina/Tarkistus/07 - Tarkista Hallin kolmivaihe")]
        public static void Diagnose() { Run(); }

        [MenuItem("Pakettiporina/Korjaus/10 - Synkkaa Hallin kuvat (Resources/WizardUI)")]
        public static void Sync()
        {
            int copied = 0, missing = 0;
            Directory.CreateDirectory(Abs(Target));
            foreach (var kv in Sources())
            {
                string src = Abs(Root + kv.Value), dst = Abs(Target + kv.Key + ".png");
                if (!File.Exists(src)) { Debug.LogWarning("[WizardSync] Lahde puuttuu: " + kv.Value); missing++; continue; }
                if (!File.Exists(dst) || !File.ReadAllBytes(src).SequenceEqual(File.ReadAllBytes(dst)))
                {
                    File.Copy(src, dst, true); copied++;
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("[WizardSync] Kopioitu/paivitetty " + copied + " kuvaa, lahde puuttui " + missing + " kertaa.");
        }

        static void Run()
        {
            var s = new StringBuilder("=== HALLIN KOLMIVAIHE: TARKISTUS ===\n");
            int problems = 0;
            void Prob(string m) { s.AppendLine("! " + m); problems++; }

            // 1) Kuvat: olemassa + ajan tasalla lahteen kanssa
            foreach (var kv in Sources())
            {
                string dst = Abs(Target + kv.Key + ".png"), src = Abs(Root + kv.Value);
                if (!File.Exists(dst)) { Prob("WizardUI puuttuu: " + kv.Key + ".png (aja Korjaus/10)"); continue; }
                if (File.Exists(src) && !File.ReadAllBytes(src).SequenceEqual(File.ReadAllBytes(dst)))
                    Prob("WizardUI vanhentunut: " + kv.Key + ".png (lahde on muuttunut, aja Korjaus/10)");
            }

            // 2) Paketit: ikoni, rata, scene Build Settingsissa
            var scenes = EditorBuildSettings.scenes.Select(x => Path.GetFileNameWithoutExtension(x.path)).ToHashSet();
            var trackImages = new HashSet<string>(Sources().Keys.Where(k => k.StartsWith("rata_")).Select(k => k.Substring(5)));
            var parts = LoadParts();
            foreach (var p in LoadPkgs())
            {
                if (p == null) continue;
                if (p.icon == null) Prob("Paketti '" + p.displayName + "': ikoni puuttuu (kortissa nakyy vain alkukirjain).");
                if (string.IsNullOrEmpty(p.trackScene)) s.AppendLine("i Paketti '" + p.displayName + "': ei trackScene -> oletusrata (rata_perus).");
                else
                {
                    string key = p.trackScene.ToLowerInvariant().Replace("ä", "a").Replace("ö", "o");
                    if (!trackImages.Contains(key)) Prob("Paketti '" + p.displayName + "': radalle '" + p.trackScene + "' ei ole kuvaa (kaytetaan rata_perus).");
                    if (!scenes.Contains(p.trackScene)) Prob("Paketti '" + p.displayName + "': scene '" + p.trackScene + "' puuttuu Build Settingsista.");
                }
                if (p.requiredPart != null && !parts.Contains(p.requiredPart))
                    Prob("Paketti '" + p.displayName + "': requiredPart '" + p.requiredPart.displayName + "' ei ole osalistalla.");
            }

            // 3) Osat: jokaisessa kategoriassa vahintaan yksi (Maali = varivalinta)
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                int n = parts.Count(x => x != null && x.category == c && !x.secret);
                if (n == 0) Prob("Kategoriassa " + c + " ei ole yhtaan avointa osaa" + (c == PartCategory.Maali ? " (varivalinta tyhja!)" : "") + ".");
                else if (n > 9) s.AppendLine("i Kategoriassa " + c + " on " + n + " osaa: wizard nayttaa ne 3 sarakkeessa, yli 9 vaatii vierityksen.");
            }

            // 4) Scene: GarageScreen + wizard
            var gs = Object.FindObjectOfType<GarageScreen>(true);
            if (gs == null) s.AppendLine("i Avaa Garage-scene tarkistaaksesi GarageScreenin (useWizard, carPreview).");
            else
            {
                if (!gs.useWizard) s.AppendLine("i useWizard on pois: vanha yhden nakyman halli on kaytossa.");
                if (gs.carPreview == null) Prob("GarageScreen.carPreview puuttuu: wizardin auton kuva jaisi tyhjaksi.");
            }

            s.AppendLine(problems == 0 ? "OK: ei ongelmia." : problems + " ongelmaa.");
            if (problems == 0) Debug.Log(s.ToString()); else Debug.LogWarning(s.ToString());
        }
    }
}
#endif
