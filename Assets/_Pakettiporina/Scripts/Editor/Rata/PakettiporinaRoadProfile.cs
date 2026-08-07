#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Nayttaa kunkin radan tien profiilin (keskilinjan X jokaisella Z:lla) - joko
    // konsoliin taulukkona tai suoraan Scene-nakymaan piirrettyna elavana linjana.
    // Tarkoitus: helpottaa Boost/Latakko/Kartio/Star-objektien kasin-sailytoa niin
    // etta ne osuvat oikeasti tielle, ei sen viereen.
    //
    // Ratadata (bends/baseX/startZ/finishZ) tulee PakettiporinaTracks.cs:sta - uuden
    // radan lisays tehdaan sinne, ei tanne.
    //
    // Kaytto:
    //   Pakettiporina -> 13 - Nayta tien profiili (konsoli)   -> taulukko Consoliin
    //   Pakettiporina -> 14 - Nayta tien profiili Scene-nakymassa (paalla/pois) -> jattaa
    //     keltaisen linjan (keskilinja) + harmaat reunaviivat (tien 12 yks. leveys) nakyviin
    //     Scene-ikkunaan pysyvasti, myos kun mikaan objekti ei ole valittuna. Piirtaa myos
    //     scenen nykyiset Boost/Latakko/Kartio/Star-objektit: VIHREA = tiella, PUNAINEN = tien
    //     vieressa (virhe).
    public static class PakettiporinaRoadProfile
    {
        const float ROAD_HALF = 6f; // tien puolikas leveys (tie = 12 yks.)

        // ================= 1) Konsolitaulukko =================
        [MenuItem("Pakettiporina/13 - Nayta tien profiili (konsoli)")]
        public static void PrintProfile()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var p = PakettiporinaTracks.Find(sceneName);
            if (p == null)
            {
                Debug.LogWarning($"'{sceneName}': ei tunnettua ratapresettia. Tuetut: Game, Puisto, Satama, Huvipuisto.");
                return;
            }

            s.AppendLine($"=== TIEN PROFIILI: {sceneName} (Z {p.startZ:F0}..{p.finishZ:F0}, tie {ROAD_HALF*2:F0} yks. levea) ===");
            s.AppendLine("Z\tX (keskilinja)\tVasen reuna\tOikea reuna");
            const float STEP = 10f;
            float z = p.startZ;
            while (z <= p.finishZ + 0.01f)
            {
                float x = PakettiporinaTracks.RoadX(z, p);
                s.AppendLine($"{z,6:F0}\t{x,8:F2}\t{x - ROAD_HALF,8:F2}\t{x + ROAD_HALF,8:F2}");
                z += STEP;
            }
            // varmista etta finishZ tulee mukaan vaikka ei osuisi STEP-valiin
            if ((p.finishZ - p.startZ) % STEP > 0.01f)
            {
                float xf = PakettiporinaTracks.RoadX(p.finishZ, p);
                s.AppendLine($"{p.finishZ,6:F0}\t{xf,8:F2}\t{xf - ROAD_HALF,8:F2}\t{xf + ROAD_HALF,8:F2}");
            }

            s.AppendLine("\n--- Scenen nykyiset elementit tata profiilia vasten ---");
            int offCount = 0;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                bool relevant = t.GetComponent<Boost>() != null || t.GetComponent<Puddle>() != null ||
                                t.GetComponent<Cone>() != null || t.GetComponent<Pickup>() != null;
                if (!relevant) continue;
                float roadXHere = PakettiporinaTracks.RoadX(t.position.z, p);
                float delta = t.position.x - roadXHere;
                bool onRoad = Mathf.Abs(delta) <= ROAD_HALF + 0.5f; // pieni marginaali
                if (!onRoad) offCount++;
                s.AppendLine($"{t.name,-14} pos=({t.position.x,6:F1},{t.position.z,6:F1})  roadX={roadXHere,6:F2}  poikkeama={delta,6:F2}  {(onRoad ? "OK - tiella" : "!!! EI TIELLA !!!")}");
            }
            if (offCount > 0)
                s.AppendLine($"\nHUOM: {offCount} kpl Boost/Latakko/Kartio/Star-objekteja on tien ULKOPUOLELLA - siirra ne roadX(z):n paalle (+/- {ROAD_HALF:F0}).");
            else
                s.AppendLine("\nKaikki Boost/Latakko/Kartio/Star-objektit ovat tiella.");

            Debug.Log(s.ToString());
        }

        // ================= 2) Elava Scene-nakyman overlay =================
        const string PREF_KEY = "Pakettiporina_ShowRoadProfileOverlay";

        [InitializeOnLoad]
        static class Overlay
        {
            static Overlay()
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }

            static void OnSceneGUI(SceneView view)
            {
                if (!EditorPrefs.GetBool(PREF_KEY, false)) return;

                string sceneName = EditorSceneManager.GetActiveScene().name;
                var p = PakettiporinaTracks.Find(sceneName);
                if (p == null) return;

                const float STEP = 2f;
                var center = new List<Vector3>();
                var left = new List<Vector3>();
                var right = new List<Vector3>();
                for (float z = p.startZ; z <= p.finishZ + 0.01f; z += STEP)
                {
                    float x = PakettiporinaTracks.RoadX(z, p);
                    center.Add(new Vector3(x, 0.3f, z));
                    left.Add(new Vector3(x - ROAD_HALF, 0.15f, z));
                    right.Add(new Vector3(x + ROAD_HALF, 0.15f, z));
                }

                Handles.color = new Color(1f, 0.9f, 0.1f, 0.95f);
                Handles.DrawAAPolyLine(6f, center.ToArray());
                Handles.color = new Color(0.75f, 0.75f, 0.8f, 0.7f);
                Handles.DrawAAPolyLine(3f, left.ToArray());
                Handles.DrawAAPolyLine(3f, right.ToArray());

                // Z-tikkamerkit 20 yksikon valein, X-arvo tekstina - helpottaa kasin sijoittelua
                Handles.color = Color.white;
                for (float z = p.startZ; z <= p.finishZ + 0.01f; z += 20f)
                {
                    float x = PakettiporinaTracks.RoadX(z, p);
                    var pos = new Vector3(x, 0.3f, z);
                    Handles.Label(pos + Vector3.up * 1.2f, $"Z={z:F0}\nX={x:F1}");
                    Handles.DrawLine(new Vector3(x - ROAD_HALF - 1f, 0.15f, z), new Vector3(x + ROAD_HALF + 1f, 0.15f, z));
                }

                // Boost/Latakko/Kartio/Star: vihrea jos tiella, punainen jos ei
                foreach (var t in Object.FindObjectsOfType<Transform>(true))
                {
                    bool relevant = t.GetComponent<Boost>() != null || t.GetComponent<Puddle>() != null ||
                                    t.GetComponent<Cone>() != null || t.GetComponent<Pickup>() != null;
                    if (!relevant) continue;
                    float roadXHere = PakettiporinaTracks.RoadX(t.position.z, p);
                    bool onRoad = Mathf.Abs(t.position.x - roadXHere) <= ROAD_HALF + 0.5f;
                    Handles.color = onRoad ? new Color(0.2f, 1f, 0.2f, 0.9f) : new Color(1f, 0.15f, 0.15f, 0.95f);
                    Handles.SphereHandleCap(0, t.position, Quaternion.identity, 1.2f, EventType.Repaint);
                }

                view.Repaint();
            }
        }

        [MenuItem("Pakettiporina/14 - Nayta tien profiili Scene-nakymassa (paalla-pois)")]
        public static void ToggleOverlay()
        {
            bool current = EditorPrefs.GetBool(PREF_KEY, false);
            EditorPrefs.SetBool(PREF_KEY, !current);
            Debug.Log(!current
                ? "Tien profiili -overlay PAALLA: keltainen viiva = keskilinja, harmaat = tien reunat (6 yks. molemmin puolin). Vihrea pallo = elementti tiella, punainen = elementti tien ulkopuolella."
                : "Tien profiili -overlay POIS PAALTA.");
            SceneView.RepaintAll();
        }

        [MenuItem("Pakettiporina/14 - Nayta tien profiili Scene-nakymassa (paalla-pois)", true)]
        public static bool ToggleOverlayValidate()
        {
            Menu.SetChecked("Pakettiporina/14 - Nayta tien profiili Scene-nakymassa (paalla-pois)", EditorPrefs.GetBool(PREF_KEY, false));
            return true;
        }
    }
}
#endif
