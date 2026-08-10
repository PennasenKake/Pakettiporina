#if UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // Generoi uusia PackageData-paketteja. Erotettu 10.8.2026 omaksi Lisays-tiedostokseen
    // PakettiporinaDataTools.cs:sta (jossa Analyze() jai puhtaaksi Tarkistus-tyokaluksi).
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Lisays/
    public static class PakettiporinaDataGenerointi
    {
        const string PKG_DIR = "Assets/_Pakettiporina/Data/Packages";

        [MenuItem("Pakettiporina/Lisays/06 - Generoi 5 uutta pakettia")]
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
            log.AppendLine("MUISTA: aja 'Pakettiporina/Korjaus/02 - KORJAA halli', niin uudet paketit tulevat mukaan hallin listaan.");
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
#endif
