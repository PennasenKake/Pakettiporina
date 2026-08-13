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
    // Kaytto: Pakettiporina -> Lisays/01 - Asettele tahdet radalle
    // Tukee kaikki PakettiporinaTracks.ALL:ssa listatut radat (ei tarvitse paivittaa
    // tata tiedostoa kun uusi rata lisataan sinne).
    // Ratadata (bends/baseX/startZ/finishZ) tulee PakettiporinaTracks.cs:sta - uuden
    // radan lisays tehdaan sinne, ei tanne.
    public static class PakettiporinaStars
    {
        const float STAR_Y = 1.2f;
        const float WOBBLE = 3.8f;   // sivuttaisheilahdus - pysyy tien 12 leveyden sisalla (puolikas 6)
        const int TARGET_STAR_COUNT = 16;
        const string STAR_PREFAB_PATH = "Assets/_Pakettiporina/Prefabs/Pickups/Star.prefab";

        [MenuItem("Pakettiporina/Lisays/01 - Asettele tahdet radalle")]
        public static void Arrange()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== TAHTIEN ASETTELU: {sceneName} (16 kpl) ===");

            var preset = PakettiporinaTracks.Find(sceneName);

            if (preset == null)
            {
                s.AppendLine($"VIRHE: scenelle '{sceneName}' ei ole valmista tahtipresettia.");
                s.AppendLine("Lisaa scene PakettiporinaTracks.cs:n ALL-tauluun (Editor/Common/).");
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
                // Instantioidaan valmis Star-prefab (pallo-mesh + SphereCollider(trigger) + Pickup jo
                // valmiiksi paikallaan), EI paljasta GameObject.AddComponent<Pickup>()-kutsua suoraan -
                // Pickupilla on [RequireComponent(typeof(Collider))] ja Collider on abstrakti, joten
                // Unity ei osaa paatella mita konkreettista collider-tyyppia lisata itsestaan ja
                // AddComponent<Pickup>() paljaalle objektille kaatuu virheeseen "Adding component
                // failed...". LOYTYI 11.8.2026 kun Maatilan tahdet luotiin ensimmaista kertaa
                // tyhjasta (muilla radoilla tahdet oli aina kopioitu olemassa olevista, joten tama
                // koodipolku ei ollut koskaan oikeasti ajettu ennen).
                var starPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(STAR_PREFAB_PATH);
                if (starPrefab == null)
                    s.AppendLine($"VAROITUS: '{STAR_PREFAB_PATH}' ei loytynyt - luodaan varakeinona paljas objekti " +
                                 "SphereColliderilla (ei nay visuaalisesti tahdelta).");
                while (pickups.Count < TARGET_STAR_COUNT)
                {
                    GameObject starGo;
                    if (starPrefab != null)
                    {
                        starGo = (GameObject)PrefabUtility.InstantiatePrefab(starPrefab);
                    }
                    else
                    {
                        starGo = new GameObject();
                        var sc = starGo.AddComponent<SphereCollider>();
                        sc.isTrigger = true;
                    }
                    starGo.name = $"Star_{pickups.Count + 1:00}";
                    var pickup = starGo.GetComponent<Pickup>();
                    if (pickup == null) pickup = starGo.AddComponent<Pickup>();
                    Undo.RegisterCreatedObjectUndo(starGo, "Luo tahti");
                    pickups.Add(pickup);
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
