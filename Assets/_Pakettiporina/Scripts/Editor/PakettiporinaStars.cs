#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Asettelee kaikki Star-objektit tasaisesti radalle, tien mutkan mukaan.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 6 Asettele tahdet radalle
    public static class PakettiporinaStars
    {
        // Sama tien keskilinja kuin Blenderin maastossa (S-mutka, suora paissa)
        static float RoadX(float z)
        {
            if (z <= -100f || z >= 100f) return 0f;
            if (z < 0f) { float t = (z + 100f) / 100f; return  20f * Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f); }
            else        { float t = z / 100f;          return -20f * Mathf.Pow(Mathf.Sin(Mathf.PI * t), 2f); }
        }

        const float FIRST_Z = -110f;   // ensimmainen tahti
        const float LAST_Z  =  115f;   // viimeinen tahti (ennen maalia Z=135)
        const float STAR_Y  =   1.2f;  // korkeus (auto ajaa lapi)

        [MenuItem("Pakettiporina/6 - Asettele tahdet radalle")]
        public static void Arrange()
        {
            var s = new StringBuilder();
            s.AppendLine("=== TAHTIEN ASETTELU ===");

            // etsi kaikki Pickup-objektit (tahdet)
            var pickups = new List<Pickup>(Object.FindObjectsOfType<Pickup>(true));
            if (pickups.Count == 0)
            {
                s.AppendLine("Ei yhtaan Pickup-objektia (tahtea) scenessa.");
                Debug.LogWarning(s.ToString());
                return;
            }

            // jarjesta nimen mukaan (siisti jako)
            pickups.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            int n = pickups.Count;

            // hae paketti pois tielta hieman vuorotellen (kaarien ulkoreunalle)
            for (int i = 0; i < n; i++)
            {
                float f = (n == 1) ? 0.5f : (float)i / (n - 1);
                float z = Mathf.Lerp(FIRST_Z, LAST_Z, f);
                float x = RoadX(z);

                // pieni sivuheitto vuorotellen, jotta lapsi ohjaa aktiivisesti (pysyy tiella +-4)
                float wobble = ((i % 2 == 0) ? 3.5f : -3.5f) * Mathf.Sin(f * Mathf.PI);
                x += wobble;

                var t = pickups[i].transform;
                Undo.RecordObject(t, "Aseta tahti");
                t.position = new Vector3(x, STAR_Y, z);
                s.AppendLine($"{pickups[i].name}: ({x:F1}, {STAR_Y}, {z:F1})");
            }

            s.AppendLine($"\nAseteltu {n} tahtea radalle (Z {FIRST_Z}..{LAST_Z}).");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif