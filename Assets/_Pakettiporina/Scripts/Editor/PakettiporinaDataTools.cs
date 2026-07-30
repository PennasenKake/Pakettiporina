using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Datatyokalu: analysoi osat ja paketit, tarkistaa tasapainon ja luo uusia paketteja.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: valikko "Pakettiporina"
    public static class PakettiporinaDataTools
    {
        const string PARTS_DIR = "Assets/_Pakettiporina/Data/Parts";
        const string PKG_DIR = "Assets/_Pakettiporina/Data/Packages";

        static List<PartData> LoadParts()
        {
            return AssetDatabase.FindAssets("t:PartData")
                .Select(g => AssetDatabase.LoadAssetAtPath<PartData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();
        }
        static List<PackageData> LoadPkgs()
        {
            return AssetDatabase.FindAssets("t:PackageData")
                .Select(g => AssetDatabase.LoadAssetAtPath<PackageData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();
        }

        // ---------- 6. ANALYYSI ----------
        [MenuItem("Pakettiporina/6 - Analysoi data")]
        public static void Analyze()
        {
            var parts = LoadParts();
            var pkgs = LoadPkgs();
            var s = new StringBuilder();
            s.AppendLine("=== DATA-ANALYYSI ===");
            s.AppendLine($"Osia: {parts.Count} | Paketteja: {pkgs.Count}\n");

            // osat kategorioittain
            s.AppendLine("--- OSAT KATEGORIOITTAIN ---");
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                var inCat = parts.Where(p => p.category == c).ToList();
                s.AppendLine($"\n[{c}] ({inCat.Count} kpl)");
                if (inCat.Count == 0) { s.AppendLine("   (tyhja — kategoriassa ei ole osia!)"); continue; }
                foreach (var p in inCat)
                {
                    string effects = Effects(p);
                    string flags = "";
                    if (string.IsNullOrWhiteSpace(p.displayName)) flags += " [EI NIMEA]";
                    if (!p.cosmeticOnly && effects == "ei vaikutusta") flags += " [KAIKKI 0]";
                    if (p.cosmeticOnly) flags += " (kosmeettinen)";
                    s.AppendLine($"   {p.displayName,-16} -> {effects}{flags}");
                }
            }

            // paketit
            s.AppendLine("\n--- PAKETIT ---");
            foreach (var pk in pkgs.OrderBy(p => p.rewardPoints))
            {
                string req = pk.requiredPart != null ? pk.requiredPart.displayName : "(ei vaatimusta)";
                bool bad = pk.requiredPart != null && !parts.Contains(pk.requiredPart);
                string flags = "";
                if (bad) flags += " [VAADITTU OSA PUUTTUU -> MAHDOTON!]";
                if (pk.rewardPoints == 0) flags += " [PALKKIO 0]";
                if (string.IsNullOrWhiteSpace(pk.displayName)) flags += " [EI NIMEA]";
                s.AppendLine($"   {pk.displayName,-16} massa={pk.mass,-4} palkkio={pk.rewardPoints,-4} vaatii={req}{flags}");
            }

            // tasapaino
            s.AppendLine("\n--- TASAPAINO ---");
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                var inCat = parts.Where(p => p.category == c && !p.cosmeticOnly).ToList();
                if (inCat.Count == 0) continue;
                s.AppendLine($"   {c}: voima {Range(inCat, p => p.voima)}  pito {Range(inCat, p => p.pito)}  keveys {Range(inCat, p => p.keveys)}  kesto {Range(inCat, p => p.kestavyys)}  kylmyys {Range(inCat, p => p.kylmyys)}");
            }

            // kattavuus: onko jokaiselle vaaditulle osalle paketti?
            s.AppendLine("\n--- KATTAVUUS ---");
            var required = pkgs.Where(p => p.requiredPart != null).Select(p => p.requiredPart).Distinct().ToList();
            s.AppendLine($"   Paketit vaativat {required.Count} eri osaa.");
            var neverRequired = parts.Where(p => !p.cosmeticOnly && !required.Contains(p)).ToList();
            if (neverRequired.Count > 0)
                s.AppendLine($"   Osia joita mikaan paketti ei vaadi: {string.Join(", ", neverRequired.Select(p => p.displayName))}");

            Debug.Log(s.ToString());
        }

        static string Effects(PartData p)
        {
            var parts = new List<string>();
            if (p.voima != 0) parts.Add($"voima {p.voima:+0;-0}");
            if (p.pito != 0) parts.Add($"pito {p.pito:+0;-0}");
            if (p.keveys != 0) parts.Add($"keveys {p.keveys:+0;-0}");
            if (p.kestavyys != 0) parts.Add($"kesto {p.kestavyys:+0;-0}");
            if (p.kylmyys != 0) parts.Add($"kylmyys {p.kylmyys:+0;-0}");
            return parts.Count == 0 ? "ei vaikutusta" : string.Join(", ", parts);
        }
        static string Range(List<PartData> list, System.Func<PartData, int> sel)
        {
            var vals = list.Select(sel).ToList();
            return $"{vals.Min()}..{vals.Max()}";
        }

        // ---------- 7. GENEROI PAKETTEJA ----------
        // Luo joukon uusia paketteja, joista jokainen vaatii jonkin olemassa olevan osan.
        [MenuItem("Pakettiporina/7 - Generoi 5 uutta pakettia")]
        public static void GeneratePackages()
        {
            var parts = LoadParts().Where(p => !p.cosmeticOnly).ToList();
            if (parts.Count == 0) { Debug.LogWarning("Ei osia — luo osia ensin."); return; }

            System.IO.Directory.CreateDirectory(PKG_DIR);

            // (nimi, teema-tagi, massa, palkkio, vaadittu ominaisuus)
            var recipes = new (string name, string tag, int mass, int reward, PartCategory needCat, string needStat)[]
            {
                ("Kukkaruukku",  "herkka",  12, 45, PartCategory.Jouset,  "pito"),
                ("Tiiliskivet",  "painava", 24, 70, PartCategory.Moottori,"voima"),
                ("Lumiukko",     "kylma",   14, 55, PartCategory.Lisat,   "kylmyys"),
                ("Kirjapino",    "painava", 20, 60, PartCategory.Kori,    "kesto"),
                ("Saippuakuplat","herkka",   6, 40, PartCategory.Renkaat, "pito"),
            };

            int created = 0;
            var log = new StringBuilder("=== GENEROI PAKETTEJA ===\n");
            foreach (var r in recipes)
            {
                string path = $"{PKG_DIR}/{r.name}.asset";
                if (AssetDatabase.LoadAssetAtPath<PackageData>(path) != null)
                {
                    log.AppendLine($"   {r.name}: on jo olemassa — ohitetaan.");
                    continue;
                }
                // etsi sopivin vaadittu osa: kategoriasta se, jolla paras haluttu ominaisuus
                var candidates = parts.Where(p => p.category == r.needCat).ToList();
                PartData req = candidates
                    .OrderByDescending(p => StatOf(p, r.needStat))
                    .FirstOrDefault();

                var pkg = ScriptableObject.CreateInstance<PackageData>();
                pkg.displayName = r.name;
                pkg.tags = new string[] { r.tag };
                pkg.mass = r.mass;
                pkg.rewardPoints = r.reward;
                pkg.requiredPart = req;
                if (r.tag == "kylma") pkg.meltTime = 40f;

                AssetDatabase.CreateAsset(pkg, path);
                created++;
                log.AppendLine($"   Luotu {r.name}: massa={r.mass}, palkkio={r.reward}, vaatii={(req != null ? req.displayName : "?")}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            log.AppendLine($"\nValmis. Luotu {created} uutta pakettia kansioon {PKG_DIR}.");
            log.AppendLine("MUISTA: aja 'Pakettiporina/5 - KORJAA halli', niin uudet paketit tulevat mukaan hallin listaan.");
            Debug.Log(log.ToString());
        }

        static int StatOf(PartData p, string stat)
        {
            switch (stat)
            {
                case "voima": return p.voima;
                case "pito": return p.pito;
                case "keveys": return p.keveys;
                case "kesto": return p.kestavyys;
                case "kylmyys": return p.kylmyys;
                default: return 0;
            }
        }
    }
}
