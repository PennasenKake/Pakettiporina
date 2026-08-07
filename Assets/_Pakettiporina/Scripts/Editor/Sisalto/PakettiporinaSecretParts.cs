#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Luo 6 SALAISTA osaa (yksi per kategoria). Nama eivat nay osaselaimessa
    // ollenkaan ennen kuin PartData.unlockPoints tayttyy (katso GarageScreen.
    // BuildLookups) - pelaaja ei siis edes tieda niista etukateen, ne vain
    // ilmestyvat listaan yllatyksena kun on kerannyt tarpeeksi pisteita.
    //
    // Arvot on laskettu niin etta KAIKKI 5 mittaria (voima/pito/keveys/kestavyys/
    // kylmyys) osuvat tasan 100:aan kun kaikki 6 salaista osaa on valittuna
    // samaan aikaan - eli "paras mahdollinen auto" -jekku, CarBuilderin
    // baseStats (40/45/50/45/0) + nailla lisilla:
    //   voima:     40 + 65 (moottori)                                = 105 -> 100
    //   pito:      45 + 15 (kori) + 30 (renkaat) + 30 (jouset)       = 120 -> 100
    //   keveys:    50 + 30 (kori) + 10 (renkaat) + 10 (moottori)     = 100
    //   kestavyys: 45 + 20 (kori) + 10 (renkaat) + 10 (jouset) + 15 (lisa) = 100
    //   kylmyys:    0 + 100 (lisa)                                   = 100
    // Jos baseStats tai muiden osien arvoja muutetaan myohemmin, tama pysyy
    // silti lahella 100:aa yllakirjoittavan ylimaaran (105/120) ansiosta.
    //
    // Kaytto: Pakettiporina -> 24 - Luo salaiset osat
    public static class PakettiporinaSecretParts
    {
        const string PARTS_DIR = "Assets/_Pakettiporina/Data/Parts";

        [MenuItem("Pakettiporina/24 - Luo salaiset osat")]
        public static void CreateSecretParts()
        {
            System.IO.Directory.CreateDirectory(PARTS_DIR);
            var log = new StringBuilder("=== SALAISET OSAT ===\n");

            var recipes = new[]
            {
                new SecretRecipe { fileName = "Kultainen_Kori",     displayName = "Kultainen kori",     category = PartCategory.Kori,     voima=0,  pito=15, keveys=30, kestavyys=20, kylmyys=0,   unlockPoints=200 },
                new SecretRecipe { fileName = "Kultaiset_Renkaat",  displayName = "Kultaiset renkaat",  category = PartCategory.Renkaat,  voima=0,  pito=30, keveys=10, kestavyys=10, kylmyys=0,   unlockPoints=240 },
                new SecretRecipe { fileName = "Kultainen_Moottori", displayName = "Kultainen moottori", category = PartCategory.Moottori, voima=65, pito=0,  keveys=10, kestavyys=0,  kylmyys=0,   unlockPoints=280 },
                new SecretRecipe { fileName = "Kultaiset_Jouset",   displayName = "Kultaiset jouset",   category = PartCategory.Jouset,   voima=0,  pito=30, keveys=0,  kestavyys=10, kylmyys=0,   unlockPoints=320 },
                new SecretRecipe { fileName = "Kultainen_Lisaosa",  displayName = "Kultainen lisaosa",  category = PartCategory.Lisat,    voima=0,  pito=0,  keveys=0,  kestavyys=15, kylmyys=100, unlockPoints=360 },
            };

            int created = 0, updated = 0;
            foreach (var r in recipes)
            {
                string path = $"{PARTS_DIR}/{r.fileName}.asset";
                var part = AssetDatabase.LoadAssetAtPath<PartData>(path);
                bool isNew = part == null;
                if (isNew) part = ScriptableObject.CreateInstance<PartData>();

                Undo.RecordObject(part, "Salainen osa");
                part.displayName = r.displayName;
                part.category = r.category;
                part.voima = r.voima; part.pito = r.pito; part.keveys = r.keveys;
                part.kestavyys = r.kestavyys; part.kylmyys = r.kylmyys;
                part.cosmeticOnly = false;
                part.unlockPoints = r.unlockPoints;
                part.secret = true;

                if (isNew) { AssetDatabase.CreateAsset(part, path); created++; }
                else { EditorUtility.SetDirty(part); updated++; }

                log.AppendLine($"   {r.displayName} ({r.category}): tarvitsee {r.unlockPoints} pistetta " +
                                $"[voima {r.voima:+0;-0;0} pito {r.pito:+0;-0;0} keveys {r.keveys:+0;-0;0} " +
                                $"kesto {r.kestavyys:+0;-0;0} kylmyys {r.kylmyys:+0;-0;0}]");
            }

            // Salainen maali - puhtaasti kosmeettinen palkinto, ei vaikuta mittareihin.
            {
                string path = $"{PARTS_DIR}/Sateenkaarimaali.asset";
                var part = AssetDatabase.LoadAssetAtPath<PartData>(path);
                bool isNew = part == null;
                if (isNew) part = ScriptableObject.CreateInstance<PartData>();
                Undo.RecordObject(part, "Salainen osa");
                part.displayName = "Sateenkaarimaali";
                part.category = PartCategory.Maali;
                part.voima = 0; part.pito = 0; part.keveys = 0; part.kestavyys = 0; part.kylmyys = 0;
                part.cosmeticOnly = true;
                part.color = new Color(1.0f, 0.84f, 0.25f); // kultainen - erottuu selvasti muista vareista
                part.unlockPoints = 150;
                part.secret = true;
                if (isNew) { AssetDatabase.CreateAsset(part, path); created++; }
                else { EditorUtility.SetDirty(part); updated++; }
                log.AppendLine($"   Sateenkaarimaali (Maali): tarvitsee 150 pistetta [vain kosmeettinen]");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            log.AppendLine($"\nValmis. Luotu {created}, paivitetty {updated} salaista osaa kansioon {PARTS_DIR}.");
            log.AppendLine("MUISTA: aja 'Pakettiporina/5 - KORJAA halli' jotta uudet osat tulevat mukaan " +
                            "GarageScreen.allParts / SaveManager.allParts -listoihin.");
            log.AppendLine("Kaikki 6 salaista osaa yhdessa = kaikki 5 mittaria tasan 100 (paras mahdollinen auto).");
            log.AppendLine("Osat EIVAT NAY osaselaimessa ennen kuin pelaajalla on tarpeeksi pisteita - " +
                            "eivat siis vain 'lukossa', vaan puuttuvat listasta kokonaan kunnes pisteraja tayttyy.");
            Debug.Log(log.ToString());
        }

        class SecretRecipe
        {
            public string fileName;
            public string displayName;
            public PartCategory category;
            public int voima, pito, keveys, kestavyys, kylmyys;
            public int unlockPoints;
        }
    }
}
#endif
