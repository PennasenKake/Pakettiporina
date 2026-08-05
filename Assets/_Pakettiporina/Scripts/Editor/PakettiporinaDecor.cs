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
    // Sijainnit on tarkistettu radan road_x(z)-kaavaa vasten (sama kaava kuin
    // PakettiporinaElements.cs:ssa) niin etta jokainen koriste on vahintaan ~8-12
    // yksikkoa tien keskilinjasta (tie on 12 yksikkoa levea) JA sisalla kentan
    // 71-levyisella alueella (+-35.5). Jos siirrat naita Hierarkiassa kasin, pida
    // molemmat rajat mielessa etteivat koristeet putoa tielle tai kentan reunan yli.
    //
    // Sataman maasto (Satama.fbx) generoitiin uudelleen niin etta meri seuraa tien
    // kaarretta jatkuvasti tien VASEMMALLA puolella (negatiivinen x roadX(z):sta) -
    // siksi laituri/laiva/majakka/poijut ovat negatiivisella puolella ja nosturi/
    // varasto/kontit positiivisella (maapuoli, lastausalue).
    public static class PakettiporinaDecor
    {
        const string MODELS_DIR = "Assets/_Pakettiporina/Art/Models";

        class DecorItem
        {
            public string modelName;     // FBX-tiedoston nimi ilman paatetta
            public string instanceName;  // scenen GameObjectin nimi - jos tyhja, kaytetaan modelName (mahdollistaa saman mallin useita kopioita)
            public Vector3 pos;
            public float rotY;
            public float scale = 1f;     // koristeen kokokerroin - isompi = naeyttavampi/helpompi huomata ajaessa
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
                    new DecorItem { modelName = "Puisto_Keinu", pos = new Vector3(15f, 0f, -40f), rotY = 0f, scale = 1.6f },
                    // Silta on nyt oikea kaarisilta (uusi Blender-malli) - siirretty
                    // lammen (Puisto_Lampi) paalle, selvasti kauemmas tiesta kuin ennen.
                    new DecorItem { modelName = "Puisto_Silta", pos = new Vector3(-30f, 0f, 70f), rotY = 90f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lampi", pos = new Vector3(-30f, 0f, 70f), rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Puisto_Penkki", pos = new Vector3(15f, 0f, -34f), rotY = 180f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", pos = new Vector3(16f, 0f, -100f), rotY = 0f, scale = 1.5f },

                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_A", pos = new Vector3(14f, 0f, -110f), rotY = 0f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_B", pos = new Vector3(-15f, 0f, -20f), rotY = 40f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_C", pos = new Vector3(14f, 0f, 45f), rotY = 80f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_D", pos = new Vector3(6f, 0f, 95f), rotY = 160f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Suihkulahde", pos = new Vector3(20f, 0f, 0f), rotY = 0f, scale = 1.6f },
                    new DecorItem { modelName = "Puisto_Liukumaki", pos = new Vector3(-16f, 0f, -115f), rotY = 90f, scale = 1.6f },
                },
            },
            new DecorPreset
            {
                sceneName = "Satama",
                items = new[]
                {
                    // Meri on nyt tien VASEMMALLA puolella (negat. x) koko radan matkalta -
                    // laiva/laituri/majakka/poijut siirretty sinne. Nosturi/varasto/kontit
                    // ovat maapuolella (positiivinen x) - satama-alueen lastauspiha.
                    new DecorItem { modelName = "Satama_Laiva", pos = new Vector3(-1f, 0f, -10f), rotY = 200f, scale = 1.8f },
                    new DecorItem { modelName = "Satama_Laituri", pos = new Vector3(9f, 0f, -10f), rotY = 90f, scale = 1.4f },
                    new DecorItem { modelName = "Satama_Majakka", pos = new Vector3(-6f, 0f, 50f), rotY = 0f, scale = 1.7f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_A", pos = new Vector3(-8f, 0f, -40f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_B", pos = new Vector3(-10f, 0f, 40f), rotY = 0f, scale = 1.3f },

                    new DecorItem { modelName = "Satama_Nosturi", pos = new Vector3(32f, 0f, -30f), rotY = 160f, scale = 1.6f },
                    new DecorItem { modelName = "Satama_Varasto", pos = new Vector3(30f, 0f, 45f), rotY = -10f, scale = 1.6f },
                    new DecorItem { modelName = "Satama_Konttipino", pos = new Vector3(32f, 0f, -55f), rotY = 0f, scale = 1.5f },
                },
            },
            new DecorPreset
            {
                sceneName = "Huvipuisto",
                items = new[]
                {
                    new DecorItem { modelName = "Huvipuisto_Maailmanpyora", pos = new Vector3(28f, 0f, 0f), rotY = 0f, scale = 1.8f },
                    new DecorItem { modelName = "Huvipuisto_Teltta", pos = new Vector3(-25f, 0f, -60f), rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Huvipuisto_Karuselli", pos = new Vector3(-15f, 0f, -60f), rotY = 0f, scale = 1.6f },
                    // Oli x=-35 (lahes kentan reunalla, +-35.5) - siirretty hiukan sisemmas.
                    new DecorItem { modelName = "Huvipuisto_Myyntikoju", pos = new Vector3(-32f, 0f, -55f), rotY = 30f, scale = 1.4f },

                    new DecorItem { modelName = "Huvipuisto_Ilmapallokaari", pos = new Vector3(14f, 0f, -160f), rotY = 90f, scale = 1.6f },
                    new DecorItem { modelName = "Huvipuisto_Lippunauha", pos = new Vector3(-16f, 0f, 60f), rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Huvipuisto_Pomppulinna", pos = new Vector3(20f, 0f, -45f), rotY = -20f, scale = 1.7f },
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
                string instName = string.IsNullOrEmpty(item.instanceName) ? item.modelName : item.instanceName;
                string path = $"{MODELS_DIR}/{item.modelName}.fbx";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    s.AppendLine($"VIRHE: '{path}' ei loydy - onko Unity tuonut FBX:n viela? (Assets -> Refresh)");
                    missing++;
                    continue;
                }

                // Ei duplikoida jos jo scenessa (etsitaan instanssin nimella, myos piilotetut).
                GameObject existing = null;
                foreach (var t in Object.FindObjectsOfType<Transform>(true))
                {
                    if (t.name == instName) { existing = t.gameObject; break; }
                }
                GameObject go;
                if (existing != null)
                {
                    go = existing;
                }
                else
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(model);
                    go.name = instName;
                    Undo.RegisterCreatedObjectUndo(go, "Luo " + instName);
                }
                go.transform.position = item.pos;
                go.transform.rotation = Quaternion.Euler(0f, item.rotY, 0f);
                go.transform.localScale = Vector3.one * item.scale;
                s.AppendLine($"{instName}: sijoitettu ({item.pos.x:F0}, {item.pos.z:F0}), kierto Y={item.rotY:F0}, koko x{item.scale:F1}");
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
