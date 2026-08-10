#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // Datan korjaus + tasapainotus (osien LISAAMINEN on omassa tiedostossaan,
    // Editor/Lisays/PakettiporinaDataLisaosat.cs - tama tiedosto koskee vain
    // OLEMASSA OLEVIEN osien korjaamista).
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Korjaus/
    public static class PakettiporinaDataFix
    {
        static PartData FindPart(List<PartData> all, string name) =>
            all.FirstOrDefault(p => p.name == name || p.displayName == name);

        // ---------- Korjaa kategoriat + kosmeettisuus ----------
        [MenuItem("Pakettiporina/Korjaus/05 - Korjaa kategoriat ja kosmeettisuus")]
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
            s.AppendLine($"Muutoksia: {changed}. Aja 'Pakettiporina/Tarkistus/05 - Analysoi data' tarkistaaksesi.");
            Debug.Log(s.ToString());
        }

        // ---------- Tasapainota arvot mielenkiintoisemmiksi ----------
        // Ideana selkeat "persoonat": jokaisella osalla vahvuus + heikkous,
        // jotta valinnoilla on tuntuva ero ajossa.
        [MenuItem("Pakettiporina/Korjaus/06 - Tasapainota osien arvot")]
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

            var all = LoadParts();
            int changed = 0;
            foreach (var kv in table)
            {
                var part = FindPart(all, kv.Key);
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

        // ---------- Kaikki data-korjaukset kerralla (EI lisaa uusia osia - ks. Lisays/) ----------
        [MenuItem("Pakettiporina/Korjaus/07 - KORJAA KAIKKI (data: kategoriat + tasapaino)")]
        public static void FixAll()
        {
            FixCategories();
            Rebalance();
            Debug.Log("=== KAIKKI VALMIS === Aja lopuksi 'Pakettiporina/Korjaus/02 - KORJAA halli' ja " +
                      "'Pakettiporina/Tarkistus/05 - Analysoi data'. Uusia osia varten: " +
                      "'Pakettiporina/Lisays/05 - Luo lisaosia'.");
        }
    }
}
#endif
