#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Luo uusia PartData-osia (perusosat, ei salaiset - salaiset osat ovat omassa
    // tiedostossaan, PakettiporinaSecretParts.cs). Erotettu 10.8.2026 omaksi
    // Lisays-tiedostokseen PakettiporinaDataFix.cs:sta, jotta "korjaa olemassa
    // olevaa dataa" ja "lisaa uutta dataa" pysyvat selkeasti erillaan.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Lisays/
    public static class PakettiporinaDataLisaosat
    {
        const string PARTS_DIR = "Assets/_Pakettiporina/Data/Parts";

        [MenuItem("Pakettiporina/Lisays/05 - Luo lisaosia")]
        public static void AddParts()
        {
            System.IO.Directory.CreateDirectory(PARTS_DIR);
            // (nimi, kategoria, v,p,k,ke,ky, cosmetic)
            var recipes = new (string name, PartCategory cat, int v,int p,int k,int ke,int ky, bool cos)[]
            {
                ("Kilpakori",   PartCategory.Kori,     0, 8, 24,-10, 0, false),  // kisakori
                ("Kuormakori",  PartCategory.Kori,     0, 4,-12, 24, 0, false),  // kestävä työkori
                ("Hiekkarenkaat",PartCategory.Renkaat, 0,20, -8,  2, 0, false),  // hiekka/maasto
                ("Rekkakone",   PartCategory.Moottori,26, 0,-14, 6, 0, false),   // vääntää
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
            Debug.Log($"=== LISAOSAT === Luotu {created}. Aja 'Pakettiporina/Korjaus/02 - KORJAA halli' ottaaksesi kayttoon.");
        }
    }
}
#endif
