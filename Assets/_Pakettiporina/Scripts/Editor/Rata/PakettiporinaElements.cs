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
    // Kaytto: Pakettiporina -> 11 - Lisaa pelielementit taman radan radalle
    // Aja-uudelleen-turvallinen: ei luo duplikaatteja, paivittaa vain sijainnin jos
    // objektit on jo olemassa (samat nimet).
    //
    // Tukee: "Game" (Rata 1, nykyinen S-mutka), "Puisto", "Satama", "Huvipuisto".
    // Ratadata (bends/baseX/boostZ/puddleZ/cones) tulee PakettiporinaTracks.cs:sta -
    // uuden radan lisays tehdaan sinne, ei tanne.
    public static class PakettiporinaElements
    {
        [MenuItem("Pakettiporina/11 - Lisaa pelielementit taman radan radalle")]
        public static void AddElements()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== PELIELEMENTIT: {sceneName} ===");

            var preset = PakettiporinaTracks.Find(sceneName);

            if (preset == null)
            {
                s.AppendLine($"VIRHE: scenelle '{sceneName}' ei ole valmista elementtipresettia.");
                s.AppendLine("Tuetut scenet: Game, Puisto, Satama, Huvipuisto.");
                s.AppendLine("Uuden radan voi lisata PakettiporinaTracks.cs:n ALL-tauluun " +
                              "samoilla bends-arvoilla kuin Blenderin ratageneraattorissa.");
                Debug.LogWarning(s.ToString());
                return;
            }

            // TAYSI SIIVOUS ENSIN: tuhoa KAIKKI aiemmin luodut Boost_/Latakko_/Kartio_
            // -nimiset objektit ennen uudelleensijoitusta - sama korjaus kuin Decorissa
            // 7.8.2026. Ilman tata vanhan presetin ylimaaraiset elementit (esim. jos
            // boostZ-taulukossa oli aiemmin enemman arvoja) jaavat scenneen orpoina,
            // koska FindOrCreate paivittaa vain NYKYISEN listan nimet - todettiin
            // kaytannossa Puistossa (Boost_06 jai roikkumaan vaikka boostZ:ssa on vain
            // 5 arvoa).
            int cleared = 0;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (t.name.StartsWith("Boost_") || t.name.StartsWith("Latakko_") || t.name.StartsWith("Kartio_"))
                {
                    Undo.DestroyObjectImmediate(t.gameObject);
                    cleared++;
                }
            }
            if (cleared > 0)
                s.AppendLine($"Siivottu {cleared} vanhaa pelielementtia (myos nykyisessa listassa enaa esiintymattomat).");

            int i = 1;
            foreach (var z in preset.boostZ)
                PlaceBoost($"Boost_{i++:00}", z, preset.bends, preset.roadBaseX, s);

            i = 1;
            foreach (var z in preset.puddleZ)
                PlacePuddle($"Latakko_{i++:00}", z, preset.bends, preset.roadBaseX, s);

            i = 1;
            foreach (var (z, offset) in preset.cones)
                PlaceCone($"Kartio_{i++:00}", z, offset, preset.bends, preset.roadBaseX, s);

            if (preset.boostZ.Length == 0 && preset.puddleZ.Length == 0 && preset.cones.Length == 0)
                s.AppendLine("(Tälle radalle ei ole maaritelty yhtaan elementtia presetissa.)");

            s.AppendLine("\nValmis. Muokkaa/duplikoi objekteja Hierarkiassa jos haluat lisaa. " +
                         "Aani (Boost/Lätäkkö/Kartio) kytkeytyy AudioManagerin kenttiin jos niihin " +
                         "on raahattu AudioClip - muuten hiljaista, ei virhetta.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }

        // GameObject.Find loytaa vain AKTIIVISET objektit - jos Boost/Latakko/Kartio oli
        // valilla piilotettuna (esim. testauksen aikana), Find ei loytaisi sita ja tama
        // loisi vahingossa toisen samannimisen kopion. Etsitaan siksi kaikista objekteista
        // (myos piilotetut) nimen perusteella.
        static GameObject FindOrCreate(string name, PrimitiveType prim)
        {
            GameObject go = null;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (t.name == name) { go = t.gameObject; break; }
            }
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

        static void PlaceBoost(string name, float z, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cube);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.TEHOSTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX);
            go.transform.position = new Vector3(x, 0.05f, z);
            go.transform.localScale = new Vector3(3f, 0.1f, 3f);
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Boost>() == null) go.AddComponent<Boost>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Boost_Vihrea", new Color(0.25f, 0.85f, 0.35f));
            s.AppendLine($"{name}: ({x:F1}, 0.05, {z:F0})");
        }

        static void PlacePuddle(string name, float z, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cylinder);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX);
            go.transform.position = new Vector3(x, 0.03f, z);
            go.transform.localScale = new Vector3(4f, 0.02f, 4f);
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Puddle>() == null) go.AddComponent<Puddle>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Latakko_Sininen", new Color(0.3f, 0.55f, 0.85f, 0.85f));
            s.AppendLine($"{name}: ({x:F1}, 0.03, {z:F0})");
        }

        static void PlaceCone(string name, float z, float offsetX, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreate(name, PrimitiveType.Cylinder);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX) + offsetX;
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
