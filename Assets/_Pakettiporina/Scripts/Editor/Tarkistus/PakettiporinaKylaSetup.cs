#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // LISATTY 19.8.2026 (kayttaja: "Kylä voisi olla viimeinen tältä erää oleva scene").
    // Kyla.unity oli pitkaan tyhja runko (ks. M-LISTA.md): kaikki HUD/RaceManager/GameManager/
    // AudioManager/CarBuilder-infrastruktuuri oli jo paikoillaan (kopioitu samasta pohjasta
    // kuin muut radat), mutta KOLME asiaa puuttui/oli rikki: 1) pohjan collider-objekti oli
    // nimetty vaarin ("GroundCollinder", kirjoitusvirhe - PakettiporinaTracks.maastoName ei
    // olisi koskaan loytanyt sita), 2) ei yhtaan Car-objektia (RaceManager.car/CameraFollow.
    // target jaisivat tyhjiksi), 3) pohjalla ei ollut MeshRenderer:ia eli rata olisi
    // taysin nakymaton (vain fysiikka, ei mitaan piirrettavaa).
    //
    // Tama tyokalu korjaa/rakentaa PERUSTAN (kayttajan oma rajaus: "aloita perustasta" -
    // talot/pihat/autot/puut/postilaatikot tulevat OMANA MYOHEMPANA vaiheena, samaan tapaan
    // kuin Lisays/03 rakentaa koristeet muille radoille kun terrain/rata on jo pystyssa).
    //
    // Kaytto: avaa Kyla-scene, aja Pakettiporina -> Korjaus/09.
    public static class PakettiporinaKylaSetup
    {
        const string OLD_GROUND_NAME = "GroundCollinder"; // alkuperainen kirjoitusvirhe
        const string GROUND_NAME = "GroundCollider";
        const string CAR_PREFAB_PATH = "Assets/_Pakettiporina/Prefabs/Car.prefab";

        [MenuItem("Pakettiporina/Korjaus/09 - KORJAA Kyla (perusta: auto, maasto, tie)")]
        public static void FixKyla()
        {
            var s = new StringBuilder();
            s.AppendLine("=== KYLA: PERUSTAN KORJAUS/RAKENNUS ===");

            string sceneName = EditorSceneManager.GetActiveScene().name;
            if (sceneName != "Kyla")
                s.AppendLine($"HUOM: aktiivinen scene on '{sceneName}', ei 'Kyla' - avaa Kyla-scene jos tama ajettiin vahingossa vaarassa scenessa.");

            var preset = PakettiporinaTracks.Find("Kyla");
            if (preset == null)
            {
                s.AppendLine("VIRHE: PakettiporinaTracks.ALL:sta ei loydy 'Kyla'-TrackDefia - tarkista PakettiporinaTracks.cs.");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- 1) Nimivirheen korjaus: GroundCollinder -> GroundCollider ----------
            var groundGo = Find(GROUND_NAME);
            if (groundGo == null)
            {
                var oldGo = Find(OLD_GROUND_NAME);
                if (oldGo != null)
                {
                    Undo.RecordObject(oldGo, "Nimea GroundCollider");
                    oldGo.name = GROUND_NAME;
                    groundGo = oldGo;
                    s.AppendLine($"'{OLD_GROUND_NAME}' -> '{GROUND_NAME}' (kirjoitusvirhe korjattu).");
                }
                else
                {
                    s.AppendLine($"VAROITUS: ei loytynyt '{OLD_GROUND_NAME}' eika '{GROUND_NAME}' - pohjan collider puuttuu kokonaan, lisaa kasin.");
                }
            }
            else
            {
                s.AppendLine($"'{GROUND_NAME}' loytyi jo oikealla nimella.");
            }

            // ---------- 2) Nakyva pohja (vihrea taso) - ilman tata rata on taysin musta/tyhja ----------
            if (groundGo != null)
            {
                var mf = groundGo.GetComponent<MeshFilter>();
                var mr = groundGo.GetComponent<MeshRenderer>();
                if (mr == null) mr = Undo.AddComponent<MeshRenderer>(groundGo);
                var mat = FlatColor("Kyla_Nurmi", new Color(0.4392f, 0.6784f, 0.2784f, 1f)); // ruohonvihrea, sama sävy kuin muiden ratojen nurmialueet
                mr.sharedMaterial = mat;
                s.AppendLine(mf != null
                    ? "Lisatty/paivitetty MeshRenderer (vihrea) - rata ei ole enaa nakymaton."
                    : "VAROITUS: MeshFilter puuttuu GroundColliderilta - MeshRenderer lisatty mutta ei nay ilman meshia.");
            }

            // ---------- 3) StartPoint ----------
            var startGo = Find("StartPoint");
            if (startGo == null)
            {
                startGo = new GameObject("StartPoint");
                Undo.RegisterCreatedObjectUndo(startGo, "Luo StartPoint");
                s.AppendLine("'StartPoint' luotu.");
            }
            float startX = PakettiporinaTracks.RoadX(preset.startZ, preset.bends, preset.roadBaseX);
            startGo.transform.position = new Vector3(startX, 0.5f, preset.startZ);
            startGo.transform.rotation = Quaternion.identity;

            // ---------- 4) Car ----------
            var carGo = Find("Car");
            if (carGo == null)
            {
                var carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CAR_PREFAB_PATH);
                if (carPrefab == null)
                {
                    s.AppendLine($"VIRHE: Car-prefabia ei loytynyt polusta {CAR_PREFAB_PATH}.");
                }
                else
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(carPrefab);
                    instance.name = "Car";
                    Undo.RegisterCreatedObjectUndo(instance, "Luo Car");
                    carGo = instance;
                    s.AppendLine("'Car' instantioitu Car.prefabista.");
                }
            }
            else
            {
                s.AppendLine("'Car' loytyi jo scenessa.");
            }
            if (carGo != null)
            {
                carGo.transform.position = startGo.transform.position;
                carGo.transform.rotation = Quaternion.identity;
            }

            // ---------- 5) Kytkennat: RaceManager.car/startPoint, CameraFollow.target ----------
            var race = Object.FindObjectOfType<RaceManager>(true);
            if (race != null && carGo != null)
            {
                Undo.RecordObject(race, "Kytke Car/StartPoint");
                var rb = carGo.GetComponent<Rigidbody>();
                race.car = rb;
                race.startPoint = startGo.transform;
                s.AppendLine(rb != null
                    ? "RaceManager.car ja .startPoint kytketty."
                    : "VAROITUS: Car-objektilta puuttuu Rigidbody - RaceManager.car jai tyhjaksi.");
            }
            else if (race == null)
            {
                s.AppendLine("VAROITUS: RaceManager-komponenttia ei loydy scenesta.");
            }

            var camFollow = Object.FindObjectOfType<CameraFollow>(true);
            if (camFollow != null && carGo != null)
            {
                Undo.RecordObject(camFollow, "Kytke kameran kohde");
                camFollow.target = carGo.transform;
                s.AppendLine("CameraFollow.target kytketty Autoon.");
            }

            s.AppendLine();
            s.AppendLine("PERUSTA VALMIS - rata pitaisi olla nyt ajettavissa (vihrea tyhja tasanko, ei viela");
            s.AppendLine("taloja/pihoja/puita/postilaatikoita - ne tulevat omana vaiheena, ks. Lisays/02");
            s.AppendLine("pelielementeille [tahdet/boostit/esteet, kun niita lisataan TrackDefiin] ja");
            s.AppendLine("Lisays/03-tyylinen koristetyokalu kylan rakennuksille myohemmin).");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }

        static Material FlatColor(string name, Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            var mat = new Material(shader) { name = name };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            return mat;
        }
    }
}
#endif
