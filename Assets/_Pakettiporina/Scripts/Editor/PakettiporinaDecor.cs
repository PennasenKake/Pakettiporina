#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Lisaa teemakoristeet (FBX-mallit, ei pelkkia primitiiveja) sen radan mukaan
    // mika scene on parhaillaan auki. Nama ovat PUHTAASTI VISUAALISIA - ei Collideria,
    // ei vaikutusta ajoon. Tarkoitus tehda jokaisesta radasta silmailla erottuva
    // omalla teemallaan sen sijaan etta ne nayttaisivat samalta vain eri varilla.
    //
    // Kaytto: Pakettiporina -> 15 - Lisaa teemakoristeet talle radalle
    // HUOM: FBX-mallit pitaa olla Unityn jo tuomia (AssetDatabase paivittynyt) ennen
    // ajoa - jos juuri loit ne Blenderista, odota etta Unity tuo ne automaattisesti
    // (tapahtuu kun Editor-ikkuna saa fokuksen) tai paina Assets -> Refresh kasin.
    //
    // Sijainnit ovat JARKEVIA OLETUKSIA, ei tarkkaa tiedetta - koska nama eivat
    // vaikuta ajoon, siirra/kierra niita vapaasti Hierarkiassa jos ne eivat osu
    // nakemaasi jarveen/tiehen tasan kohdalleen.
    public static class PakettiporinaDecor
    {
        const string MODELS_DIR = "Assets/_Pakettiporina/Art/Models";

        class DecorItem
        {
            public string modelName;   // FBX-tiedoston nimi ilman paatetta
            public Vector3 pos;
            public float rotY;
        }

        class DecorPreset
        {
            public string sceneName;
            public DecorItem[] items;
        }

        static readonly DecorPreset[] PRESETS =
        {
            new DecorPreset
            {
                sceneName = "Puisto",
                items = new[]
                {
                    new DecorItem { modelName = "Puisto_Keinu", pos = new Vector3(15f, 0f, -40f), rotY = 0f },
                    new DecorItem { modelName = "Puisto_Silta", pos = new Vector3(-24f, 0f, 70f), rotY = 90f },
                    new DecorItem { modelName = "Puisto_Penkki", pos = new Vector3(15f, 0f, -34f), rotY = 180f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", pos = new Vector3(8f, 0f, -100f), rotY = 0f },
                },
            },
            new DecorPreset
            {
                sceneName = "Satama",
                items = new[]
                {
                    new DecorItem { modelName = "Satama_Laiva", pos = new Vector3(38f, 0f, -10f), rotY = 20f },
                    new DecorItem { modelName = "Satama_Laituri", pos = new Vector3(13f, 0f, -10f), rotY = 90f },
                    new DecorItem { modelName = "Satama_Majakka", pos = new Vector3(44f, 0f, -10f), rotY = 0f },
                    new DecorItem { modelName = "Satama_Konttipino", pos = new Vector3(-20f, 0f, 30f), rotY = 0f },
                },
            },
            new DecorPreset
            {
                sceneName = "Huvipuisto",
                items = new[]
                {
                    new DecorItem { modelName = "Huvipuisto_Maailmanpyora", pos = new Vector3(28f, 0f, 0f), rotY = 0f },
                    new DecorItem { modelName = "Huvipuisto_Teltta", pos = new Vector3(-25f, 0f, -60f), rotY = 0f },
                    new DecorItem { modelName = "Huvipuisto_Karuselli", pos = new Vector3(-15f, 0f, -60f), rotY = 0f },
                    new DecorItem { modelName = "Huvipuisto_Myyntikoju", pos = new Vector3(-35f, 0f, -55f), rotY = 30f },
                },
            },
        };

        [MenuItem("Pakettiporina/15 - Lisaa teemakoristeet talle radalle")]
        public static void AddDecor()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== TEEMAKORISTEET: {sceneName} ===");

            DecorPreset preset = null;
            foreach (var p in PRESETS)
                if (p.sceneName == sceneName) { preset = p; break; }

            if (preset == null)
            {
                s.AppendLine($"Tälle scenelle ('{sceneName}') ei ole teemakoristepresettia (tuetut: Puisto, Satama, Huvipuisto).");
                Debug.LogWarning(s.ToString());
                return;
            }

            int placed = 0, missing = 0;
            foreach (var item in preset.items)
            {
                string path = $"{MODELS_DIR}/{item.modelName}.fbx";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    s.AppendLine($"VIRHE: '{path}' ei loydy - onko Unity tuonut FBX:n viela? (Assets -> Refresh)");
                    missing++;
                    continue;
                }

                // Ei duplikoida jos jo scenessa (etsitaan nimella).
                var existing = GameObject.Find(item.modelName);
                GameObject go;
                if (existing != null)
                {
                    go = existing;
                }
                else
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(model);
                    go.name = item.modelName;
                    Undo.RegisterCreatedObjectUndo(go, "Luo " + item.modelName);
                }
                go.transform.position = item.pos;
                go.transform.rotation = Quaternion.Euler(0f, item.rotY, 0f);
                s.AppendLine($"{item.modelName}: sijoitettu ({item.pos.x:F0}, {item.pos.z:F0}), kierto Y={item.rotY:F0}");
                placed++;
            }

            s.AppendLine($"\nValmis. {placed} koristetta sijoitettu, {missing} puuttui.");
            if (missing > 0)
                s.AppendLine("Aja tama uudelleen kun puuttuvat FBX:t ovat tuotu Unityyn.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif
