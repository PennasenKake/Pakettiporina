#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Asettelee TASAN 16 tahtea radalle, sen radan mukaan mika scene on parhaillaan
    // auki (tunnistetaan scenen nimesta). Sama road_x-kaava/bends-arvot kuin
    // PakettiporinaElements.cs:ssa ja Blenderin ratageneraattorissa, joten tahdet
    // seuraavat aina oikeaa tien keskilinjaa riippumatta radasta.
    //
    // Kaytto: Pakettiporina -> 6 - Asettele tahdet radalle
    // Tukee: "Game" (Rata 1), "Puisto", "Satama", "Huvipuisto".
    public static class PakettiporinaStars
    {
        class TrackPreset
        {
            public string sceneName;
            public (float z0, float z1, float amp)[] bends;
            public float firstZ;   // ensimmainen tahti (Z)
            public float lastZ;    // viimeinen tahti (Z, ennen maalia)
        }

        // Bends + start/finish suoraan PakettiporinaElements.cs:sta ja
        // PakettiporinaSetup.cs:n DIMS_BY_SCENE:sta (startZ/finishZ).
        static readonly TrackPreset[] PRESETS =
        {
            new TrackPreset
            {
                sceneName = "Game",
                bends = new[] { (-100f, 0f, 20f), (0f, 100f, -20f) },
                firstZ = -110f, lastZ = 115f,   // startZ -135 / finishZ 135
            },
            new TrackPreset
            {
                sceneName = "Puisto",
                bends = new[] { (-120f, -30f, 18f), (30f, 120f, -18f) },
                firstZ = -125f, lastZ = 130f,   // startZ -150 / finishZ 150
            },
            new TrackPreset
            {
                sceneName = "Satama",
                bends = new[] { (-80f, 80f, 26f) },
                firstZ = -125f, lastZ = 130f,   // startZ -150 / finishZ 150
            },
            new TrackPreset
            {
                sceneName = "Huvipuisto",
                bends = new[] { (-150f, -80f, 16f), (-40f, 40f, -18f), (80f, 150f, 16f) },
                firstZ = -145f, lastZ = 150f,   // startZ -170 / finishZ 170
            },
        };

        const float STAR_Y = 1.2f;
        const float WOBBLE = 3.8f;   // sivuttaisheilahdus - pysyy tien 12 leveyden sisalla (puolikas 6)
        const int TARGET_STAR_COUNT = 16;

        static float RoadX(float z, (float z0, float z1, float amp)[] bends)
        {
            float x = 0f;
            foreach (var (z0, z1, amp) in bends)
                if (z >= z0 && z <= z1)
                    x += amp * Mathf.Pow(Mathf.Sin(Mathf.PI * (z - z0) / (z1 - z0)), 2f);
            return x;
        }

        [MenuItem("Pakettiporina/6 - Asettele tahdet radalle")]
        public static void Arrange()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== TAHTIEN ASETTELU: {sceneName} (16 kpl) ===");

            TrackPreset preset = null;
            foreach (var p in PRESETS)
                if (p.sceneName == sceneName) { preset = p; break; }

            if (preset == null)
            {
                s.AppendLine($"VIRHE: scenelle '{sceneName}' ei ole valmista tahtipresettia.");
                s.AppendLine("Tuetut scenet: Game, Puisto, Satama, Huvipuisto.");
                Debug.LogWarning(s.ToString());
                return;
            }

            var pickups = new List<Pickup>(Object.FindObjectsOfType<Pickup>(true));

            if (pickups.Count > TARGET_STAR_COUNT)
            {
                s.AppendLine($"Varoitus: loytyi {pickups.Count} tahtea, kaytetaan vain {TARGET_STAR_COUNT} ensimmaista.");
                pickups.RemoveRange(TARGET_STAR_COUNT, pickups.Count - TARGET_STAR_COUNT);
            }
            else if (pickups.Count < TARGET_STAR_COUNT)
            {
                s.AppendLine($"Luodaan {TARGET_STAR_COUNT - pickups.Count} uutta tahtea...");
                while (pickups.Count < TARGET_STAR_COUNT)
                {
                    var newStar = new GameObject($"Star_{pickups.Count + 1:00}").AddComponent<Pickup>();
                    Undo.RegisterCreatedObjectUndo(newStar.gameObject, "Luo tahti");
                    pickups.Add(newStar);
                }
            }

            pickups.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            int n = pickups.Count;

            for (int i = 0; i < n; i++)
            {
                float f = (float)i / (n - 1);
                float z = Mathf.Lerp(preset.firstZ, preset.lastZ, f);
                float x = RoadX(z, preset.bends);

                float wobble = ((i % 2 == 0) ? WOBBLE : -WOBBLE) * Mathf.Sin(f * Mathf.PI * 1.1f);
                x += wobble;

                var t = pickups[i].transform;
                Undo.RecordObject(t, "Aseta tahti");
                t.position = new Vector3(x, STAR_Y, z);
                t.name = $"Star_{i + 1:00}";

                s.AppendLine($"{t.name}: ({x:F1}, {STAR_Y}, {z:F1})");
            }

            s.AppendLine($"\nValmis! Asetettiin {n} tahtea radalle {sceneName} (Z {preset.firstZ:F0} .. {preset.lastZ:F0}).");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif
