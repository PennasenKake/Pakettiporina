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
    // Kaytto: Pakettiporina -> 10 - Asettele tahdet radalle
    // Tukee: "Game" (Rata 1), "Puisto", "Satama", "Huvipuisto".
    // Ratadata (bends/baseX/startZ/finishZ) tulee PakettiporinaTracks.cs:sta - uuden
    // radan lisays tehdaan sinne, ei tanne.
    public static class PakettiporinaStars
    {
        const float STAR_Y = 1.2f;
        const float WOBBLE = 3.8f;   // sivuttaisheilahdus - pysyy tien 12 leveyden sisalla (puolikas 6)
        const int TARGET_STAR_COUNT = 16;

        [MenuItem("Pakettiporina/10 - Asettele tahdet radalle")]
        public static void Arrange()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== TAHTIEN ASETTELU: {sceneName} (16 kpl) ===");

            var preset = PakettiporinaTracks.Find(sceneName);

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

            float firstZ = PakettiporinaTracks.StarFirstZ(preset);
            float lastZ = PakettiporinaTracks.StarLastZ(preset);

            for (int i = 0; i < n; i++)
            {
                float f = (float)i / (n - 1);
                float z = Mathf.Lerp(firstZ, lastZ, f);
                float x = PakettiporinaTracks.RoadX(z, preset);

                float wobble = ((i % 2 == 0) ? WOBBLE : -WOBBLE) * Mathf.Sin(f * Mathf.PI * 1.1f);
                x += wobble;

                var t = pickups[i].transform;
                Undo.RecordObject(t, "Aseta tahti");
                t.position = new Vector3(x, STAR_Y, z);
                t.name = $"Star_{i + 1:00}";

                s.AppendLine($"{t.name}: ({x:F1}, {STAR_Y}, {z:F1})");
            }

            s.AppendLine($"\nValmis! Asetettiin {n} tahtea radalle {sceneName} (Z {firstZ:F0} .. {lastZ:F0}).");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif
