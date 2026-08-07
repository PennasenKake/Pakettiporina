#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Luo avattavat tarrat (StickerData) valmiiksi generoiduista kuvista.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 23 - Luo tarrat (avattavat)
    public static class PakettiporinaStickerTools
    {
        const string ART_DIR = "Assets/_Pakettiporina/Art/UI/Tarrat";
        const string DATA_DIR = "Assets/_Pakettiporina/Data/Stickers";
        const string PREFIX = "pakettiporina_tarra_";

        // (tiedostonimen loppuosa, nayttonimi, pisteraja)
        static readonly (string key, string name, int points)[] RECIPES =
        {
            ("raketti",     "Raketti",      20),
            ("salama",      "Salama",       40),
            ("kultatahti",  "Kultatahti",   60),
            ("jaatelo",     "Jaatelo",      90),
            ("aurinko",     "Aurinko",     110),
            ("ilmapallo",   "Ilmapallo",   140),
            ("aarrearkku",  "Aarrearkku",  180),
            ("timantti",    "Timantti",    220),
            ("sateenkaari", "Sateenkaari", 260),
            ("kruunu",      "Kruunu",      320),
            ("robotti",     "Robotti",     400),
            ("tikkari",     "Tikkari",     500),

            // Toinen erä
            ("kirje",       "Kirje",        30),
            ("noppa",       "Noppa",        50),
            ("sydan",       "Sydan",        75),
            ("purjevene",   "Purjevene",   100),
            ("mitali",      "Mitali",      130),
            ("kompassi",    "Kompassi",    170),
            ("kuu",         "Kuu",         210),
            ("lahjapaketti","Lahjapaketti",240),
            ("ufo",         "Ufo",         280),
            ("pokaali",     "Pokaali",     350),
            ("planeetta",   "Planeetta",   450),
            ("kilpaauto",   "Kilpa-auto",  600),

            // Kolmas era
            ("pollo",       "Pollo",        15),
            ("perhonen",    "Perhonen",     35),
            ("kissa",       "Kissa",        65),
            ("kukka",       "Kukka",        85),
            ("jalkapallo",  "Jalkapallo",  120),
            ("lumihiutale", "Lumihiutale", 150),
            ("pilvi",       "Pilvi",       190),
            ("leijona",     "Leijona",     230),
            ("kitara",      "Kitara",      270),
            ("avain",       "Avain",       310),
            ("komeetta",    "Komeetta",    380),
            ("kello",       "Kello",       550),
        };

        [MenuItem("Pakettiporina/23 - Luo tarrat (avattavat)")]
        public static void GenerateStickers()
        {
            var log = new StringBuilder("=== LUO TARRAT ===\n");

            // ---------- 1. Varmista etta kuvat ovat Sprite-tyyppisia ----------
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { ART_DIR });
            var spriteByKey = new Dictionary<string, Sprite>();
            int fixedImports = 0;
            foreach (var g in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                    fixedImports++;
                }
                string fname = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                string key = fname.StartsWith(PREFIX) ? fname.Substring(PREFIX.Length) : fname;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) spriteByKey[key] = sprite;
            }
            if (textureGuids.Length == 0)
            {
                log.AppendLine($"VIRHE: kuvia ei loytynyt kansiosta {ART_DIR}.");
                Debug.LogError(log.ToString());
                return;
            }
            if (fixedImports > 0) log.AppendLine($"Korjattu Texture Type = Sprite: {fixedImports} kpl.");

            // ---------- 2. Luo StickerData-assetit ----------
            System.IO.Directory.CreateDirectory(DATA_DIR);
            int created = 0, updated = 0, missingImage = 0;
            foreach (var r in RECIPES)
            {
                string path = $"{DATA_DIR}/{r.name}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<StickerData>(path);
                bool hasSprite = spriteByKey.TryGetValue(r.key, out var sprite);
                if (!hasSprite) missingImage++;

                if (existing != null)
                {
                    bool changed = false;
                    if (existing.image == null && hasSprite) { existing.image = sprite; changed = true; }
                    if (existing.unlockPoints != r.points) { existing.unlockPoints = r.points; changed = true; }
                    if (changed)
                    {
                        EditorUtility.SetDirty(existing);
                        updated++;
                        log.AppendLine($"   Paivitetty {r.name}: pisteraja={r.points}, kuva={(hasSprite ? "kytketty" : "PUUTTUU")}");
                    }
                    continue;
                }

                var sd = ScriptableObject.CreateInstance<StickerData>();
                sd.displayName = r.name;
                sd.image = sprite;
                sd.unlockPoints = r.points;
                AssetDatabase.CreateAsset(sd, path);
                created++;
                log.AppendLine($"   Luotu {r.name}: pisteraja={r.points}, kuva={(hasSprite ? "kytketty" : "PUUTTUU")}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (missingImage > 0)
                log.AppendLine($"VAROITUS: {missingImage} tarralta puuttuu kuva - tarkista tiedostonimet kansiossa {ART_DIR}.");
            log.AppendLine($"\nValmis. Luotu {created}, paivitetty {updated} tarraa kansioon {DATA_DIR}.");
            log.AppendLine("MUISTA: rakenna StickerGrid MainMenuun (objektien nimet = tarrojen nimet, esim. 'Raketti'), " +
                            "aja sitten 'Pakettiporina/12 - KORJAA paavalikko'.");
            Debug.Log(log.ToString());
        }
    }
}
#endif
