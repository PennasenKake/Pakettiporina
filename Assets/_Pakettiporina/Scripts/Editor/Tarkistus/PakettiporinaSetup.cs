#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Peliscenen tarkistus- ja korjaustyokalu.
    // Sijoita kansioon: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: ylavalikko "Pakettiporina" -> Tarkista / Korjaa
    public static class PakettiporinaSetup
    {
        const float START_Y = 1.0f; // auto hieman maan ylapuolelle

        // Ratakohtainen data (bends/baseX/startZ/finishZ/groundScaleZ/roadBaseX) tulee
        // nyt yhdesta yhteisesta lahteesta: PakettiporinaTracks.cs. Uuden radan lisays:
        // lisaa se sinne, ei tanne.
        const float GROUND_SCALE_X = PakettiporinaTracks.GROUND_SCALE_X;

        [MenuItem("Pakettiporina/01 - Tarkista peliscene")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/02 - KORJAA peliscene")]
        public static void FixAll() { Run(true); }

        static void Run(bool fix)
        {
            var log = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var dims = PakettiporinaTracks.Find(sceneName);
            if (dims == null)
            {
                dims = PakettiporinaTracks.Find("Game");
                log.AppendLine($"(Scenea '{sceneName}' ei tunnisteta - kaytetaan Rata 1:n oletusmittoja. " +
                                "Lisaa se PakettiporinaTracks.cs:n ALL-tauluun jos tama on uusi rata.)");
            }
            float START_Z = dims.startZ, FINISH_Z = dims.finishZ;
            log.AppendLine(fix ? $"=== KORJAUS ({sceneName}) ===" : $"=== TARKISTUS ({sceneName}) ===");
            int problems = 0;

            // ---------- etsi osat ----------
            var carCtrl   = Object.FindObjectOfType<ArcadeCarController>();
            var raceMgr   = Object.FindObjectOfType<RaceManager>();
            var raceSetup = Object.FindObjectOfType<RaceSetup>();
            var camFollow = Object.FindObjectOfType<CameraFollow>();
            var finishTr  = Object.FindObjectOfType<FinishTrigger>();
            var maasto    = GameObject.Find(dims.maastoName);
            var ground    = GameObject.Find("GroundCollider");
            var startObj  = GameObject.Find("StartPoint");
            var audioMgr  = Object.FindObjectOfType<AudioManager>(true);

            // ---------- 0. AUDIOMANAGER ----------
            if (audioMgr == null)
            {
                log.AppendLine("VAROITUS: AudioManager puuttuu (aanet eivat soi jos testaat tasta scenesta suoraan).");
                problems++;
                if (fix)
                {
                    var go = new GameObject("AudioManager");
                    audioMgr = go.AddComponent<AudioManager>();
                    Undo.RegisterCreatedObjectUndo(go, "AudioManager");
                    log.AppendLine("  -> luotu (muista raahata aaniklipit sen Inspectoriin)");
                }
            }
            else log.AppendLine("AudioManager: OK");

            // ---------- 0b. SAVEMANAGER ----------
            var saveMgr = Object.FindObjectOfType<SaveManager>(true);
            if (saveMgr == null)
            {
                log.AppendLine("VAROITUS: SaveManager puuttuu (tallennus/lataus ei toimi jos testaat tasta scenesta suoraan).");
                problems++;
                if (fix)
                {
                    var go = new GameObject("SaveManager");
                    go.AddComponent<SaveManager>();
                    Undo.RegisterCreatedObjectUndo(go, "SaveManager");
                    log.AppendLine("  -> luotu (listat tayttyvat automaattisesti kun ajat 'KORJAA halli' Garage-scenessa)");
                }
            }
            else log.AppendLine("SaveManager: OK");

            // ---------- 1. MAASTO ----------
            if (maasto == null) { log.AppendLine($"VIRHE: '{dims.maastoName}' puuttuu scenesta. Veda Art/Models/{dims.maastoName} sceneen."); problems++; }
            else
            {
                bool bad = maasto.transform.position != Vector3.zero
                        || maasto.transform.rotation != Quaternion.identity
                        || maasto.transform.localScale != Vector3.one;
                if (bad)
                {
                    log.AppendLine("Maasto: vaara sijainti/kierto/skaala.");
                    problems++;
                    if (fix)
                    {
                        Undo.RecordObject(maasto.transform, "Maasto");
                        maasto.transform.SetParent(null);
                        maasto.transform.position = Vector3.zero;
                        maasto.transform.rotation = Quaternion.identity;
                        maasto.transform.localScale = Vector3.one;
                        log.AppendLine("  -> korjattu (0,0,0 / 0,0,0 / 1,1,1)");
                    }
                }
                else log.AppendLine("Maasto: OK");
            }

            // ---------- 2. GROUNDCOLLIDER ----------
            if (ground == null)
            {
                log.AppendLine("VIRHE: GroundCollider puuttuu."); problems++;
                if (fix)
                {
                    ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    ground.name = "GroundCollider";
                    Undo.RegisterCreatedObjectUndo(ground, "GroundCollider");
                    log.AppendLine("  -> luotu");
                }
            }
            if (ground != null)
            {
                var mc = ground.GetComponent<MeshCollider>();
                var mr = ground.GetComponent<MeshRenderer>();
                var mf = ground.GetComponent<MeshFilter>();
                var expectedScale = new Vector3(GROUND_SCALE_X, 1f, dims.groundScaleZ);
                bool bad = mc == null || mr != null
                        || ground.transform.position != Vector3.zero
                        || ground.transform.localScale != expectedScale;
                if (bad)
                {
                    log.AppendLine($"GroundCollider: puutteita (collider/renderer/skaala, odotettu {expectedScale}).");
                    problems++;
                    if (fix)
                    {
                        ground.transform.SetParent(null);
                        ground.transform.position = Vector3.zero;
                        ground.transform.rotation = Quaternion.identity;
                        ground.transform.localScale = expectedScale;
                        if (mr != null) Object.DestroyImmediate(mr);
                        if (mc == null)
                        {
                            mc = ground.AddComponent<MeshCollider>();
                            if (mf != null) mc.sharedMesh = mf.sharedMesh;
                        }
                        mc.convex = false;
                        log.AppendLine($"  -> korjattu (MeshCollider paalla, renderer pois, skaala {expectedScale})");
                    }
                }
                else log.AppendLine("GroundCollider: OK");
            }

            // ---------- 3. STARTPOINT ----------
            if (startObj == null)
            {
                log.AppendLine("VIRHE: StartPoint puuttuu."); problems++;
                if (fix)
                {
                    startObj = new GameObject("StartPoint");
                    Undo.RegisterCreatedObjectUndo(startObj, "StartPoint");
                    log.AppendLine("  -> luotu");
                }
            }
            if (startObj != null && fix)
            {
                startObj.transform.SetParent(null);
                startObj.transform.position = new Vector3(dims.roadBaseX, START_Y, START_Z);
                startObj.transform.rotation = Quaternion.identity;
            }
            if (startObj != null)
                log.AppendLine($"StartPoint: {startObj.transform.position}");

            // ---------- 4. AUTO ----------
            if (carCtrl == null) { log.AppendLine("VIRHE: Autoa (ArcadeCarController) ei loydy. Veda Prefabs/Car/Car sceneen."); problems++; }
            else
            {
                var carGo = carCtrl.gameObject;
                // poista ylimaaraiset kamerat ja valot auton alta
                foreach (var c in carGo.GetComponentsInChildren<Camera>(true))
                {
                    log.AppendLine("Auton alla YLIMAARAINEN KAMERA: " + c.name); problems++;
                    if (fix) { Object.DestroyImmediate(c.gameObject); log.AppendLine("  -> poistettu"); }
                }
                foreach (var l in carGo.GetComponentsInChildren<Light>(true))
                {
                    log.AppendLine("Auton alla ylimaarainen valo: " + l.name); problems++;
                    if (fix) { Object.DestroyImmediate(l.gameObject); log.AppendLine("  -> poistettu"); }
                }
                var rb = carGo.GetComponent<Rigidbody>();
                if (rb == null) { log.AppendLine("VIRHE: autolta puuttuu Rigidbody."); problems++; }
                if (fix)
                {
                    carGo.transform.SetParent(null);
                    carGo.transform.localScale = Vector3.one;
                    carGo.transform.position = new Vector3(dims.roadBaseX, START_Y, START_Z);
                    carGo.transform.rotation = Quaternion.identity;
                    if (rb != null)
                    {
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                        rb.interpolation = RigidbodyInterpolation.Interpolate;
                        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    }
                }
                // onko autolla toimiva collider?
                var col = carGo.GetComponentInChildren<Collider>();
                if (col == null) { log.AppendLine("VIRHE: autolla ei ole collideria."); problems++; }
                else if (col.isTrigger)
                {
                    log.AppendLine("VIRHE: auton collider on Trigger (ei saa olla)."); problems++;
                    if (fix) { col.isTrigger = false; log.AppendLine("  -> korjattu"); }
                }
                log.AppendLine("Auto: " + carGo.name + " @ " + carGo.transform.position);

                // ---------- 4b. CARPAINTER (M4: hallin Maali-vari nakyy ajossa) ----------
                var painter = carGo.GetComponent<CarPainter>();
                if (painter == null)
                {
                    log.AppendLine("VAROITUS: CarPainter puuttuu (auton vari ei vaihdu hallin maalivalinnan mukaan).");
                    problems++;
                    if (fix)
                    {
                        painter = carGo.AddComponent<CarPainter>();
                        log.AppendLine("  -> lisatty");
                    }
                }
                if (painter != null && painter.bodyRenderer == null)
                {
                    Renderer found = null;
                    foreach (var r in carGo.GetComponentsInChildren<Renderer>(true))
                    {
                        if (r.sharedMaterial != null && r.sharedMaterial.name.ToLower().Contains("kori"))
                        { found = r; break; }
                    }
                    if (found != null)
                    {
                        log.AppendLine("CarPainter: Body Renderer ei kytketty.");
                        problems++;
                        if (fix)
                        {
                            Undo.RecordObject(painter, "CarPainter");
                            painter.bodyRenderer = found;
                            EditorUtility.SetDirty(painter);
                            log.AppendLine("  -> kytketty (" + found.name + " / " + found.sharedMaterial.name + ")");
                        }
                    }
                    else
                    {
                        log.AppendLine("VAROITUS: autolta ei loydy materiaalia jonka nimessa on 'kori' - CarPainter ei voi maalata. " +
                            "Varmista etta auton korin Renderer kayttaa materiaalia kuten 'Auto_kori' (Art/Models/Materials).");
                        problems++;
                    }
                }
                else if (painter != null) log.AppendLine("CarPainter: OK");
            }

            // ---------- 5. MAALI ----------
            if (finishTr == null) { log.AppendLine("VIRHE: FinishTrigger puuttuu."); problems++; }
            else
            {
                var fgo = finishTr.gameObject;
                var bc = fgo.GetComponent<BoxCollider>();
                if (bc == null && fix) bc = fgo.AddComponent<BoxCollider>();
                if (fix)
                {
                    fgo.transform.SetParent(null);
                    fgo.transform.position = new Vector3(dims.roadBaseX, 0f, FINISH_Z);
                    fgo.transform.rotation = Quaternion.identity;
                    fgo.transform.localScale = Vector3.one;
                    if (bc != null)
                    {
                        bc.isTrigger = true;
                        bc.size = new Vector3(13f, 6f, 2f);
                        bc.center = new Vector3(0f, 3f, 0f);
                    }
                    // piilota mahdollinen placeholder-kuutio
                    var fmr = fgo.GetComponent<MeshRenderer>();
                    if (fmr != null && fgo.transform.childCount > 0) fmr.enabled = false;
                }
                log.AppendLine("Maali: @ " + fgo.transform.position + (bc != null && bc.isTrigger ? " (trigger OK)" : " (TRIGGER PUUTTUU)"));
            }

            // ---------- 6. KYTKENNAT ----------
            if (raceMgr != null && carCtrl != null && startObj != null)
            {
                var rb = carCtrl.GetComponent<Rigidbody>();
                if (raceMgr.car != rb || raceMgr.startPoint != startObj.transform)
                {
                    log.AppendLine("RaceManager: kytkennat puuttuvat."); problems++;
                    if (fix)
                    {
                        Undo.RecordObject(raceMgr, "RaceManager");
                        raceMgr.car = rb;
                        raceMgr.startPoint = startObj.transform;
                        raceMgr.fallY = -5f;
                        EditorUtility.SetDirty(raceMgr);
                        log.AppendLine("  -> kytketty");
                    }
                }
                else log.AppendLine("RaceManager: OK");
            }
            else if (raceMgr == null) { log.AppendLine("VIRHE: RaceManager puuttuu."); problems++; }

            if (camFollow != null && carCtrl != null)
            {
                if (camFollow.target != carCtrl.transform)
                {
                    log.AppendLine("CameraFollow: target puuttuu."); problems++;
                    if (fix)
                    {
                        Undo.RecordObject(camFollow, "CameraFollow");
                        camFollow.target = carCtrl.transform;
                        EditorUtility.SetDirty(camFollow);
                        log.AppendLine("  -> kytketty");
                    }
                }
                else log.AppendLine("CameraFollow: OK");
            }
            else if (camFollow == null)
            {
                log.AppendLine("VAROITUS: Main Cameralta puuttuu CameraFollow."); problems++;
                if (fix)
                {
                    var mainCamGo = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");
                    if (mainCamGo != null)
                    {
                        camFollow = mainCamGo.AddComponent<CameraFollow>();
                        if (carCtrl != null) camFollow.target = carCtrl.transform;
                        EditorUtility.SetDirty(mainCamGo);
                        log.AppendLine("  -> CameraFollow lisatty Main Cameraan" + (carCtrl != null ? " ja kytketty autoon" : " (auto puuttuu viela, kytke myohemmin)"));
                    }
                    else
                    {
                        log.AppendLine("  -> ei voitu lisata: 'Main Camera' -objektia ei loydy scenesta.");
                    }
                }
            }

            if (raceSetup != null && carCtrl != null)
            {
                if (raceSetup.car != carCtrl)
                {
                    log.AppendLine("RaceSetup: car puuttuu.");
                    if (fix)
                    {
                        Undo.RecordObject(raceSetup, "RaceSetup");
                        raceSetup.car = carCtrl;
                        EditorUtility.SetDirty(raceSetup);
                        log.AppendLine("  -> kytketty");
                    }
                }
                else log.AppendLine("RaceSetup: OK");
            }

            // ---------- 7. YLIMAARAISET KAMERAT SCENESSA ----------
            var cams = Object.FindObjectsOfType<Camera>();
            if (cams.Length > 1)
            {
                log.AppendLine($"VAROITUS: scenessa on {cams.Length} kameraa:");
                foreach (var c in cams) log.AppendLine("   - " + c.name);
                problems++;
            }

            // ---------- 8. PANEELIT ----------
            var fp = GameObject.Find("FinishPanel");
            if (fp != null && fp.activeSelf)
            {
                log.AppendLine("FinishPanel on paalla (nakyy heti)."); problems++;
                if (fix) { fp.SetActive(false); log.AppendLine("  -> piilotettu"); }
            }
            var pp = GameObject.Find("PausePanel");
            if (pp != null && pp.activeSelf)
            {
                log.AppendLine("PausePanel on paalla."); problems++;
                if (fix) { pp.SetActive(false); log.AppendLine("  -> piilotettu"); }
            }

            log.AppendLine(problems == 0 ? "\nKaikki kunnossa!" : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty." : " Aja 'KORJAA peliscene'."));
            if (fix) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(log.ToString());
        }
    }
}
#endif