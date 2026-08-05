#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Luo valmiit, testattavat Boost/Lätäkkö/Kartio-objektit sen radan mukaan mika
    // scene on parhaillaan auki (tunnistetaan scenen nimesta). Sama road_x-kaava ja
    // samat "bends"-arvot kuin pakettiporina_ratageneraattori.py:ssa (Blender), joten
    // sijoittelu tasmaa aina oikeasti tiehen riippumatta radasta.
    //
    // Kaytto: Pakettiporina -> 14 - Lisaa pelielementit taman radan radalle
    // Aja-uudelleen-turvallinen: ei luo duplikaatteja, paivittaa vain sijainnin jos
    // objektit on jo olemassa (samat nimet).
    //
    // Tukee: "Game" (Rata 1, nykyinen S-mutka), "Puisto", "Satama", "Huvipuisto".
    // Uuden radan lisays: kopioi rivi PRESETS-listaan samoilla bends-arvoilla kuin
    // Blenderin ratageneraattorissa kaytit.
    public static class PakettiporinaElements
    {
        class TrackPreset
        {
            public string sceneName;
            public (float z0, float z1, float amp)[] bends;
            public float[] boostZ;
            public float[] puddleZ;
            public (float z, float offset)[] cones;
        }

        // Bends-arvot suoraan pakettiporina_ratageneraattori.py:n TRACKS-listasta
        // (Puisto/Satama/Huvipuisto) ja PakettiporinaStars.cs:sta (Game/Rata 1).
        static readonly TrackPreset[] PRESETS =
        {
            new TrackPreset
            {
                sceneName = "Game",
                bends = new[] { (-100f, 0f, 20f), (0f, 100f, -20f) },
                boostZ = new[] { -130f, 0f },
                puddleZ = new[] { -50f },
                cones = new[] { (45f, 3.5f), (60f, -3.5f) },
            },
            new TrackPreset
            {
                // Puisto: pelkkia boosteja - opettaa etta keveys kannattaa (ei muita elementteja).
                sceneName = "Puisto",
                bends = new[] { (-120f, -30f, 18f), (30f, 120f, -18f) },
                boostZ = new[] { -150f, 0f, 150f },
                puddleZ = new float[0],
                cones = new (float, float)[0],
            },
            new TrackPreset
            {
                // Satama: lätäkkö mutkan huipulla + kartiot molemmin puolin - opettaa etta pito kannattaa.
                sceneName = "Satama",
                bends = new[] { (-80f, 80f, 26f) },
                boostZ = new float[0],
                puddleZ = new[] { 0f },
                cones = new[] { (-40f, 3.5f), (40f, -3.5f) },
            },
            new TrackPreset
            {
                // Huvipuisto: kaikki elementit yhdessa - haastavin, kaikki opitut taidot tarpeen.
                sceneName = "Huvipuisto",
                bends = new[] { (-150f, -80f, 16f), (-40f, 40f, -18f), (80f, 150f, 16f) },
                boostZ = new[] { -170f, 170f },
                puddleZ = new[] { 0f },
                cones = new[] { (-115f, 3.5f), (115f, -3.5f) },
            },
        };

        static float RoadX(float z, (float z0, float z1, float amp)[] bends)
        {
            float x = 0f;
            foreach (var (z0, z1, amp) in bends)
                if (z >= z0 && z <= z1)
                    x += amp * Mathf.Pow(Mathf.Sin(Mathf.PI * (z - z0) / (z1 - z0)), 2f);
            return x;
        }

        [MenuItem("Pakettiporina/14 - Lisaa pelielementit taman radan radalle")]
        public static void AddElements()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== PELIELEMENTIT: {sceneName} ===");

            TrackPreset preset = null;
            foreach (var p in PRESETS)
                if (p.sceneName == sceneName) { preset = p; break; }

            if (preset == null)
            {
                s.AppendLine($"VIRHE: scenelle '{sceneName}' ei ole valmista elementtipresettia.");
                s.AppendLine("Tuetut scenet: Game, Puisto, Satama, Huvipuisto.");
                s.AppendLine("Uuden radan voi lisata PakettiporinaElements.cs:n PRESETS-listaan " +
                              "samoilla bends-arvoilla kuin Blenderin ratageneraattorissa.");
                Debug.LogWarning(s.ToString());
                return;
            }

            int i = 1;
            foreach (var z in preset.boostZ)
                PlaceBoost($"Boost_{i++:00}", z, preset.bends, s);

            i = 1;
            foreach (var z in preset.puddleZ)
                PlacePuddle($"Latakko_{i++:00}", z, preset.bends, s);

            i = 1;
            foreach (var (z, offset) in preset.cones)
                PlaceCone($"Kartio_{i++:00}", z, offset, preset.bends, s);

            if (preset.boostZ.Length == 0 && preset.puddleZ.Length == 0 && preset.cones.Length == 0)
                s.AppendLine("(Tälle radalle ei ole maaritelty yhtaan elementtia presetissa.)");

            s.AppendLine("\nValmis. Muokkaa/duplikoi objekteja Hierarkiassa jos haluat lisaa. " +
                         "Aani (Boost/Lätäkkö/Kartio) kytkeytyy AudioManagerin kenttiin jos niihin " +
                         "on raahattu AudioClip - muuten hiljaista, ei virhetta.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }

        static GameObject FindOrCreate(string name, PrimitiveType prim)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(prim);
                go.name = name;
                Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
            }
            return go;
        }

        static Material FlatColor(string name, Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            return new Material(shader) { name = name, color = c };
        }

        static void PlaceBoost(string name, float z, (float, float, float)[] bends, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cube);
            float x = RoadX(z, bends);
            go.transform.position = new Vector3(x, 0.05f, z);
            go.transform.localScale = new Vector3(3f, 0.1f, 3f);
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Boost>() == null) go.AddComponent<Boost>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Boost_Vihrea", new Color(0.25f, 0.85f, 0.35f));
            s.AppendLine($"{name}: ({x:F1}, 0.05, {z:F0})");
        }

        static void PlacePuddle(string name, float z, (float, float, float)[] bends, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cylinder);
            float x = RoadX(z, bends);
            go.transform.position = new Vector3(x, 0.03f, z);
            go.transform.localScale = new Vector3(4f, 0.02f, 4f);
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Puddle>() == null) go.AddComponent<Puddle>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Latakko_Sininen", new Color(0.3f, 0.55f, 0.85f, 0.85f));
            s.AppendLine($"{name}: ({x:F1}, 0.03, {z:F0})");
        }

        static void PlaceCone(string name, float z, float offsetX, (float, float, float)[] bends, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cylinder);
            float x = RoadX(z, bends) + offsetX;
            go.transform.position = new Vector3(x, 0.4f, z);
            go.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            // Kartio on kiintea este (Is Trigger POIS) - auto tormaa siihen fyysisesti.
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;
            if (go.GetComponent<Cone>() == null) go.AddComponent<Cone>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Kartio_Oranssi", new Color(0.95f, 0.5f, 0.15f));
            s.AppendLine($"{name}: ({x:F1}, 0.4, {z:F0})");
        }
    }
}
#endif
