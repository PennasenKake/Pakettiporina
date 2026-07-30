#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Datan automaattikorjaus + tasapainotus + uudet osat.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    public static class PakettiporinaDataFix
    {
        const string PARTS_DIR = "Assets/_Pakettiporina/Data/Parts";

        static List<PartData> LoadParts() =>
            AssetDatabase.FindAssets("t:PartData")
                .Select(g => AssetDatabase.LoadAssetAtPath<PartData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();

        static PartData FindPart(string name) =>
            LoadParts().FirstOrDefault(p => p.name == name || p.displayName == name);

        // ---------- 8a: korjaa kategoriat + kosmeettisuus ----------
        [MenuItem("Pakettiporina/8a - Korjaa kategoriat ja kosmeettisuus")]
        public static void FixCategories()
        {
            var s = new StringBuilder("=== KORJAA KATEGORIAT ===\n");
            int changed = 0;

            // odotetut kategoriat nimen perusteella (avainsana -> kategoria)
            var keyword = new (string kw, PartCategory cat)[]
            {
                ("kori", PartCategory.Kori),
                ("renkaat", PartCategory.Renkaat), ("rengas", PartCategory.Renkaat),
                ("moottori", PartCategory.Moottori), ("kone", PartCategory.Moottori),
                ("jouset", PartCategory.Jouset), ("jousi", PartCategory.Jouset),
                ("laatikko", PartCategory.Lisat), ("tavaratila", PartCategory.Lisat), ("valot", PartCategory.Lisat),
                ("maali", PartCategory.Maali),
            };
            // varinimet -> Maali
            var colorNames = new[] { "punainen", "sininen", "vihrea", "vihreä", "keltainen", "musta", "valkoinen" };

            foreach (var p in LoadParts())
            {
                string low = (p.name + " " + p.displayName).ToLower();
                PartCategory? target = null;

                if (colorNames.Any(cn => low.Contains(cn))) target = PartCategory.Maali;
                else foreach (var (kw, cat) in keyword)
                    if (low.Contains(kw)) { target = cat; break; }

                if (target.HasValue && p.category != target.Value)
                {
                    Undo.RecordObject(p, "kategoria");
                    Debug.Log($"[Fix] {p.displayName}: {p.category} -> {target.Value}");
                    p.category = target.Value;
                    EditorUtility.SetDirty(p);
                    changed++;
                }

                // kosmeettisuus: jos osalla on arvoja, se EI saa olla cosmeticOnly (paitsi Maali)
                bool hasStats = p.voima != 0 || p.pito != 0 || p.keveys != 0 || p.kestavyys != 0 || p.kylmyys != 0;
                if (p.category != PartCategory.Maali && p.cosmeticOnly && hasStats)
                {
                    Undo.RecordObject(p, "cosmetic");
                    Debug.Log($"[Fix] {p.displayName}: Cosmetic Only pois (osalla on arvoja)");
                    p.cosmeticOnly = false;
                    EditorUtility.SetDirty(p);
                    changed++;
                }
                // Maali-osat: aina cosmeticOnly
                if (p.category == PartCategory.Maali && !p.cosmeticOnly)
                {
                    Undo.RecordObject(p, "cosmetic");
                    p.cosmeticOnly = true;
                    EditorUtility.SetDirty(p);
                    changed++;
                }
            }
            AssetDatabase.SaveAssets();
            s.AppendLine($"Muutoksia: {changed}. Aja '6 - Analysoi data' tarkistaaksesi.");
            Debug.Log(s.ToString());
        }

        // ---------- 8b: tasapainota arvot mielenkiintoisemmiksi ----------
        // Ideana selkeat "persoonat": jokaisella osalla vahvuus + heikkous,
        // jotta valinnoilla on tuntuva ero ajossa.
        [MenuItem("Pakettiporina/8b - Tasapainota osien arvot")]
        public static void Rebalance()
        {
            // nimi -> (voima,pito,keveys,kesto,kylmyys)
            var table = new Dictionary<string,(int v,int p,int k,int ke,int ky)>
            {
                // KORIT: kompromissi keveyden ja keston valilla
                {"Urheilukori",  (0, 5, 20,-8, 0)},   // kevyt & ketterä, hauras
                {"Maastokori",   (0, 8,-10,20, 0)},   // kestävä & pitävä, raskas
                {"Kevytkori",    (0,-4,28,-12,0)},    // erittäin kevyt, hauras
                // RENKAAT: pito vs keveys
                {"Katurenkaat",  (0, 6, 14, 0, 0)},   // nopea kadulla
                {"Maastorenkaat",(0,24,-14, 4, 0)},   // huippupito, hidas
                {"Talvirenkaat", (0,16, -6, 0,10)},   // pito + vähän kylmää
                // MOOTTORIT: voima vs keveys
                {"Peruskone",    (14, 0, 0, 4, 0)},   // tasainen
                {"Sähkömoottori",(22, 0, 8, 0, 0)},   // voimaa & kevyt
                {"Turbomoottori",(34, 0,-10,-4, 0)},  // huippuvoima, raskas
                // JOUSET: pito vs kesto
                {"Pehmeät jouset",(0,18, 0, 4, 0)},   // pehmeä pito
                {"Kovat_jouset", (0,-4, 6,16, 0)},    // kova, kestävä
                {"Kisajouset",   (0,22, 6,-6, 0)},    // kisapito, hauras
                // LISÄT
                {"Kylmalaatikko",(0, 0,-6,-4,80)},    // kylmä, painava
                {"Tavaratila",   (0, 0,-8,18, 0)},    // kestävä, raskas
            };

            int changed = 0;
            foreach (var kv in table)
            {
                var part = FindPart(kv.Key);
                if (part == null) continue;
                var (v,p,k,ke,ky) = kv.Value;
                Undo.RecordObject(part, "tasapaino");
                part.voima=v; part.pito=p; part.keveys=k; part.kestavyys=ke; part.kylmyys=ky;
                if (part.category != PartCategory.Maali) part.cosmeticOnly = false;
                EditorUtility.SetDirty(part);
                changed++;
                Debug.Log($"[Balance] {kv.Key}: voima {v}, pito {p}, keveys {k}, kesto {ke}, kylmyys {ky}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"=== TASAPAINO === Paivitetty {changed} osaa. Selkeat vahvuudet+heikkoudet.");
        }

        // ---------- 8c: luo lisää osia ----------
        [MenuItem("Pakettiporina/8c - Luo lisaosia")]
        public static void AddParts()
        {
            System.IO.Directory.CreateDirectory(PARTS_DIR);
            // (nimi, kategoria, v,p,k,ke,ky, cosmetic)
            var recipes = new (string name, PartCategory cat, int v,int p,int k,int ke,int ky, bool cos)[]
            {
                ("Kilpakori",   PartCategory.Kori,     0, 8, 24,-10, 0, false),  // kisakori
                ("Kuormakori",  PartCategory.Kori,     0, 4,-12, 24, 0, false),  // kestävä työkori
                ("Hiekkarenkaat",PartCategory.Renkaat, 0,20, -8,  2, 0, false),  // hiekka/maasto
                ("Rekkakone",   PartCategory.Moottori,26, 0,-14, 6, 0, false),   // vääntöä
                ("Kisajouset2", PartCategory.Jouset,   0,20, 8, -8, 0, false),   // (vaihtoehto)
                ("Lampolaatikko",PartCategory.Lisat,   0, 0,-6, -2,-30, false),  // pitää lämpimänä (neg kylmyys)
                ("Valot",       PartCategory.Lisat,    0, 0, 0,  4, 0, false),   // pieni kesto
                ("Keltainen",   PartCategory.Maali,    0, 0, 0,  0, 0, true),
                ("Oranssi",     PartCategory.Maali,    0, 0, 0,  0, 0, true),
            };

            int created = 0;
            var colors = new Dictionary<string,Color>{
                {"Keltainen", new Color(0.98f,0.80f,0.20f)},
                {"Oranssi",   new Color(0.94f,0.52f,0.20f)},
            };
            foreach (var r in recipes)
            {
                string path = $"{PARTS_DIR}/{r.name}.asset";
                if (AssetDatabase.LoadAssetAtPath<PartData>(path) != null)
                {
                    Debug.Log($"[AddParts] {r.name}: on jo olemassa — ohitetaan.");
                    continue;
                }
                var part = ScriptableObject.CreateInstance<PartData>();
                part.displayName = r.name;
                part.category = r.cat;
                part.voima=r.v; part.pito=r.p; part.keveys=r.k; part.kestavyys=r.ke; part.kylmyys=r.ky;
                part.cosmeticOnly = r.cos;
                if (r.cat == PartCategory.Maali)
                    part.color = colors.TryGetValue(r.name, out var c) ? c : Color.white;
                AssetDatabase.CreateAsset(part, path);
                created++;
                Debug.Log($"[AddParts] Luotu {r.name} ({r.cat})");
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log($"=== LISAOSAT === Luotu {created}. Aja '5 - KORJAA halli' ottaaksesi kayttoon.");
        }

        // ---------- 8: kaikki kerralla ----------
        [MenuItem("Pakettiporina/8 - KORJAA KAIKKI (kategoriat + tasapaino + lisaosat)")]
        public static void FixAll()
        {
            FixCategories();
            AddParts();
            Rebalance();   // tasapainota myos uudet
            Debug.Log("=== KAIKKI VALMIS === Aja lopuksi '5 - KORJAA halli' ja '6 - Analysoi data'.");
        }
    }
}
#endif