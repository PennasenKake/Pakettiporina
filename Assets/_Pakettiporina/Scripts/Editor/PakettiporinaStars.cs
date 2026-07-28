#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Asettelee TASAN 16 TÄHTEÄ radalle
    // Käyttö: Pakettiporina -> 6 Asettele tahdet radalle
    public static class PakettiporinaStars
    {
        // Tien keskilinja (S-mutka)
        static float RoadX(float z)
        {
            if (z <= -100f || z >= 100f) return 0f;
            if (z < 0f)
            {
                float t = (z + 100f) / 100f;
                return 20f * Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            }
            else
            {
                float t = z / 100f;
                return -20f * Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f);
            }
        }

        const float FIRST_Z = -110f;   // ensimmäinen tähti
        const float LAST_Z  =  115f;   // viimeinen tähti (ennen maalia)
        const float STAR_Y  =   1.2f;  // korkeus

        const int TARGET_STAR_COUNT = 16;   // <-- TÄSSÄ MUUTOS

        [MenuItem("Pakettiporina/6 - Asettele tahdet radalle")]
        public static void Arrange()
        {
            var s = new StringBuilder();
            s.AppendLine("=== TAHTIEN ASETTELU (16 kpl) ===");

            // Etsitään kaikki Pickupit (tähdet)
            var pickups = new List<Pickup>(Object.FindObjectsOfType<Pickup>(true));
            
            if (pickups.Count == 0)
            {
                Debug.LogError("Scenessä ei ole yhtään Pickup-objektia (tähteä)!");
                return;
            }

            // Jos on enemmän kuin 16 tähteä, käytetään vain 16 ensimmäistä
            if (pickups.Count > TARGET_STAR_COUNT)
            {
                s.AppendLine($"Varoitus: Löytyi {pickups.Count} tähteä, käytetään vain {TARGET_STAR_COUNT} ensimmäistä.");
                pickups.RemoveRange(TARGET_STAR_COUNT, pickups.Count - TARGET_STAR_COUNT);
            }
            // Jos on vähemmän kuin 16, luodaan puuttuvat
            else if (pickups.Count < TARGET_STAR_COUNT)
            {
                s.AppendLine($"Luodaan {TARGET_STAR_COUNT - pickups.Count} uutta tähteä...");
                while (pickups.Count < TARGET_STAR_COUNT)
                {
                    var newStar = new GameObject($"Star_{pickups.Count + 1:00}").AddComponent<Pickup>();
                    pickups.Add(newStar);
                }
            }

            // Järjestetään nimen mukaan
            pickups.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            int n = pickups.Count; // Nyt pitäisi olla 16

            for (int i = 0; i < n; i++)
            {
                float f = (float)i / (n - 1);                    // 0.0 -> 1.0
                float z = Mathf.Lerp(FIRST_Z, LAST_Z, f);
                float x = RoadX(z);

                // Vuorotellen sivulle, jotta pelaaja joutuu ohjaamaan
                float wobble = ((i % 2 == 0) ? 3.8f : -3.8f) * Mathf.Sin(f * Mathf.PI * 1.1f);
                x += wobble;

                var t = pickups[i].transform;
                Undo.RecordObject(t, "Aseta tahti");

                t.position = new Vector3(x, STAR_Y, z);

                // Nimetään siististi
                t.name = $"Star_{i + 1:00}";

                s.AppendLine($"{t.name}: ({x:F1}, {STAR_Y}, {z:F1})");
            }

            s.AppendLine($"\nValmis! Asetettiin {n} tähteä radalle (Z {FIRST_Z} – {LAST_Z}).");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif