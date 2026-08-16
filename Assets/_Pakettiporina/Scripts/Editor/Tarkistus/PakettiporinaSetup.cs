#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace Pakettiporina.EditorTools
{
    // Peliscenen tarkistus- ja korjaustyokalu.
    // Sijoita kansioon: Assets/_Pakettiporina/Scripts/Editor/Tarkistus/
    // Kaytto: ylavalikko "Pakettiporina" -> Tarkistus/01 / Korjaus/01
    public static class PakettiporinaSetup
    {
        const float START_Y = 1.0f; // auto hieman maan ylapuolelle

        // Ratakohtainen data (bends/baseX/startZ/finishZ/groundScaleZ/roadBaseX) tulee
        // nyt yhdesta yhteisesta lahteesta: PakettiporinaTracks.cs (Editor/Common/).
        // Uuden radan lisays: lisaa se sinne, ei tanne.
        const float GROUND_SCALE_X = PakettiporinaTracks.GROUND_SCALE_X;

        // Scenet joissa EI KOSKAAN ole rataa (halli, valikko, pesu) - tama tyokalu on
        // vain ajopeliscenej (Game/Puisto/Satama/Huvipuisto/Maatila...) varten. LISATTY
        // 12.8.2026: ennen tama tyokalu lankesi "kaytetaan Rata 1:n oletusmittoja"
        // -varafallbackiin NAISSAKIN sceneissa ja loi vahingossa niihin ylimaaraisen
        // GroundColliderin, StartPointin ja CameraFollow-'auto puuttuu' -varoituksen
        // Main Cameraan - haitatonta mutta turhaa roskaa. Nyt tyokalu ohittaa nama
        // scenet kokonaan heti alussa.
        static readonly string[] NonRaceScenes = { "MainMenu", "Garage", "Wash" };

        [MenuItem("Pakettiporina/Tarkistus/01 - Tarkista peliscene")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/Korjaus/01 - KORJAA peliscene")]
        public static void FixAll() { Run(true); }

        static void Run(bool fix)
        {
            var log = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;

            if (System.Array.IndexOf(NonRaceScenes, sceneName) >= 0)
            {
                Debug.Log($"[PeliSceneSetup] '{sceneName}' ei ole ajorata (halli/valikko/pesu) - " +
                           "tama tyokalu on vain Game/Puisto/Satama/Huvipuisto/Maatila-tyyppisille sceneille, ohitetaan.");
                return;
            }

            var dims = PakettiporinaTracks.Find(sceneName);
            if (dims == null)
            {
                dims = PakettiporinaTracks.Find("Game");
                log.AppendLine($"(Scenea '{sceneName}' ei tunnisteta - kaytetaan Rata 1:n oletusmittoja. " +
                                "Lisaa se PakettiporinaTracks.cs:n ALL-tauluun jos tama on uusi rata, tai jos tama " +
                                "EI OLE rata, lisaa nimi NonRaceScenes-listaan taman tiedoston alkuun.)");
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
            var gameMgr   = Object.FindObjectOfType<GameManager>(true);

            // ---------- -1. GAMEMANAGER (LISATTY 16.8.2026) ----------
            // KORJATTU: puuttui tasta tyokalusta kokonaan, vaikka AudioManager/SaveManager
            // -turvaverkko oli jo olemassa. Seuraus: jos ratascene avataan ja Play painetaan
            // SUORAAN (ei MainMenu -> Halli -> Aja keikka -kautta), GameManager.Instance on
            // null koko ajon ajan - kaikki GameManager.Instance!=null -tarkistukset (Cone/
            // Laatikko/Puddle: AddDent/AddDirtHit, RaceHUD: lika-/kolhupalkkien tilaus,
            // RaceManager.FinishRace: pisteet) ohittuvat AANETTOMASTI. Tama on todennakoisin
            // syy kayttajan havaintoon "progress bar ei nayta osumia" - palkit eivat
            // yksinkertaisesti koskaan saaneet yhtaan tapahtumaa tilattavaksi.
            if (gameMgr == null)
            {
                log.AppendLine("VAROITUS: GameManager puuttuu (pisteet/lika-kolhumittarit eivat toimi " +
                                "jos testaat tasta scenesta suoraan - todennakoinen syy jos HUD:in " +
                                "lika-/kolhupalkit eivat nayta mitaan).");
                problems++;
                if (fix)
                {
                    // KORJATTU 16.8.2026: scenessa saattaa jo olla TYHJA "GameManager"-niminen
                    // GameObject (vain Transform, ei itse skriptikomponenttia - nakyy Hierarkiassa
                    // oikealta nimeltaan mutta ei oikeasti tee mitaan). Kaytetaan se uudelleen
                    // sen sijaan etta luotaisiin toinen samanniminen objekti paalekkain.
                    var existingGo = GameObject.Find("GameManager");
                    var go = existingGo != null ? existingGo : new GameObject("GameManager");
                    if (existingGo == null) Undo.RegisterCreatedObjectUndo(go, "GameManager");
                    else Undo.RegisterCompleteObjectUndo(go, "Lisaa GameManager-komponentti");
                    gameMgr = go.AddComponent<GameManager>();
                    log.AppendLine(existingGo != null
                        ? "  -> loytyi tyhja 'GameManager'-objekti, GameManager-komponentti lisatty siihen"
                        : "  -> luotu");
                }
            }
            else log.AppendLine("GameManager: OK");

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
            // HUOM 9.8.2026: mallit on kansioitu scenekohtaisiin alikansioihin - maasto
            // loytyy nyt polusta Art/Models/{sceneName}/{maastoName}.fbx (esim.
            // Art/Models/Huvipuisto/Huvipuisto.fbx), ei enaa suoraan Art/Models-juuresta.
            if (maasto == null) { log.AppendLine($"VIRHE: '{dims.maastoName}' puuttuu scenesta. Veda Art/Models/{dims.sceneName}/{dims.maastoName} sceneen."); problems++; }
            else
            {
                // HUOM 13.8.2026: odotettu kierto luetaan TrackDef.maastoRotationY:sta (ei enaa
                // aina Quaternion.identity) - joillain radoilla (esim. Satama) malli tarvitsee
                // kiinteän Y-kierron nayttaakseen oikealta. Tama esti aiemman bugin jossa
                // Sataman tarkoituksellinen 180 asteen kierto nollautui joka Korjaus/01-ajolla
                // ja koko satama-alue kaantyi paikallaan sijaitsevien koristeiden alla.
                var expectedRot = Quaternion.Euler(0f, dims.maastoRotationY, 0f);
                bool bad = maasto.transform.position != Vector3.zero
                        || Quaternion.Angle(maasto.transform.rotation, expectedRot) > 0.01f
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
                        maasto.transform.rotation = expectedRot;
                        maasto.transform.localScale = Vector3.one;
                        log.AppendLine($"  -> korjattu (0,0,0 / kierto Y={dims.maastoRotationY} / 1,1,1)");
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

                // ---------- 4c. CARENGINEAUDIO (moottorin ajoaani, kaasun/nopeuden mukaan) ----------
                var engineAudio = carGo.GetComponent<CarEngineAudio>();
                if (engineAudio == null)
                {
                    log.AppendLine("VAROITUS: CarEngineAudio puuttuu (moottorin aani ei soi ajossa).");
                    problems++;
                    if (fix)
                    {
                        engineAudio = carGo.AddComponent<CarEngineAudio>();
                        log.AppendLine("  -> lisatty (lukee AudioManager.Instance.engineLoop-klipin Startissa)");
                    }
                }
                else log.AppendLine("CarEngineAudio: OK");
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

            // ---------- 9. Maali-paneelin "Pesuun"-nappi (LISATTY 16.8.2026) ----------
            // Kayttaja kysyi "kuinka paasee autoa pesemaan ja korjaamaan radan ajon jalkeen"
            // - tutkittuna nappi ITSE ON kytketty oikein (RaceHUD.OnGarageButton vie
            // washScene-sceneen), MUTTA sen GameObjectin nimi on "OnGarageButton" (metodin
            // nimi, ei pelaajalle nakyva teksti) JA sen ikoni on pakettiporina_ikoni_halli_2
            // (HALLI-ikoni) vaikka nappi vie ENSIN Pesuun eika suoraan halliin - pelaaja ei
            // voinut tietaa etta juuri tama nappi on oikea, koska ikoni lupaa vaaraa paikkaa
            // eika tekstia ole ollenkaan. Korjataan lisaamalla selkea "Pesuun"-teksti napin
            // lapselle (Text (TMP)) ja vaihtamalla ikoni neutraalimpaan "jatka"-nuoleen.
            var garageBtnGo = GameObject.Find("OnGarageButton");
            if (garageBtnGo != null)
            {
                var label = garageBtnGo.GetComponentInChildren<TMP_Text>(true);
                if (label != null && string.IsNullOrEmpty(label.text))
                {
                    log.AppendLine("Maalin 'Pesuun'-napissa ei ole tekstia (vain ikoni, sekoittui halli-ikoniin).");
                    problems++;
                    if (fix)
                    {
                        label.text = "Pesuun";
                        EditorUtility.SetDirty(label);
                        log.AppendLine("  -> teksti asetettu ('Pesuun')");
                    }
                }

                var icon = garageBtnGo.GetComponentsInChildren<Image>(true);
                foreach (var img in icon)
                {
                    if (img.sprite != null && img.sprite.name == "pakettiporina_ikoni_halli_2")
                    {
                        log.AppendLine("Maalin 'Pesuun'-napin ikoni on Halli-ikoni (harhaanjohtava, nappi vie Pesuun ENSIN).");
                        problems++;
                        if (fix)
                        {
                            var jatkaIcon = AssetDatabase.LoadAssetAtPath<Sprite>(
                                "Assets/_Pakettiporina/Art/UI/Icons/pakettiporina_ikoni_jatka.png");
                            if (jatkaIcon != null)
                            {
                                img.sprite = jatkaIcon;
                                EditorUtility.SetDirty(img);
                                log.AppendLine("  -> ikoni vaihdettu (jatka-nuoli)");
                            }
                            else
                            {
                                log.AppendLine("  -> VAROITUS: pakettiporina_ikoni_jatka.png ei loytynyt, ikonia ei vaihdettu");
                            }
                        }
                    }
                }
            }
            else
            {
                log.AppendLine("(Maalin 'Pesuun'-nappia (OnGarageButton) ei loytynyt tasta scenesta - jos FinishPanel " +
                                "on rakennettu eri tavalla, tarkista kasin etta jokin nappi vie washScene-sceneen.)");
            }

            // ---------- 10. Lika-/kolhumittarit RaceHUD:ssa (LISATTY 16.8.2026, M5.17) ----------
            // Kayttaja: "progressbar lialle ja korjaukselle... nakyy ajossa, kasvaa osumista".
            // RaceHUD:in MUUT UI-viittaukset (rewardText/damageText/starText jne.) ovat aina
            // olleet TAYSIN KASIN kytkettyja taman tyokalun historiassa - mutta koska naille
            // kahdelle uudelle palkille ei ole viela mitaan GameObjectia missaan viidesta
            // ratascenesta, luodaan ne automaattisesti KORJAA-tilassa (sama periaate kuin
            // Pesun RepairArea/DentSpots, PakettiporinaWashSetup.cs).
            var hud = Object.FindObjectOfType<RaceHUD>(true);
            var hudCanvas = Object.FindObjectOfType<Canvas>(true);
            if (hud != null && hudCanvas != null)
            {
                // KORJATTU 16.8.2026 (kayttaja: "progress barja tulisi muokata siistimmaksi,
                // sijoittelu pause-painikkeen paalle ja samalla sita laskea tilanteen mukaan"):
                // aiempi versio arvasi kiintean (24,-40)/(24,-74) -sijainnin sokkona. Nyt
                // sijainti LASKETAAN oikeasti PauseButtonin TODELLISESTA RectTransformista -
                // jos PauseButtonia joskus siirretaan, palkit seuraavat mukana kun tyokalu
                // ajetaan uudelleen. Myos kooltaan pienempi/siistimpi (180x16 aiemman 320x28
                // sijaan) ja pieni TMP-teksti kertoo kumpi palkki on kumpikin ("Lika"/"Kolhut").
                // (Katso alta 2. KORJATTU-kommentti 16.8.2026 - ensimmainen yritys taman
                // sijainnin laskemiseksi oli itsessaan viallinen, siina oli oikea bugi.)
                var hudCanvasRt = hudCanvas.transform as RectTransform;
                var pauseBtnGo = GameObject.Find("PauseButton");
                var pauseRt = pauseBtnGo != null ? pauseBtnGo.GetComponent<RectTransform>() : null;

                // KORJATTU 16.8.2026 (kayttaja: "korjaa peliscene poistaa kokonaan progress
                // barit"): TODELLINEN BUGI loytyi - edellinen versio kopioi PauseButtonin
                // anchorMin/anchorMax/anchoredPosition SELLAISENAAN uusille palkeille, mutta
                // PauseButton EI ole suoraan Canvasin lapsi - scene-YAML paljasti etta se on
                // "Header"-nimisen valikontainerin sisalla, ja Header on ankkuroitu Canvasin
                // KESKELLE (anchorMin/Max 0.5,0.5), ei ylareunaan. PauseButtonin oma
                // anchoredPosition (0,800) on siis Headerin PAIKALLISESSA koordinaatistossa,
                // ei Canvasin. Kun uudet palkit liitettiin SUORAAN Canvasin alle mutta niiden
                // sijainti laskettiin ikaan kuin ne olisivat Headerin sisalla (ja lisaksi
                // vaarin oletettiin PauseButtonin pivotiksi keskipivotti, vaikka se on
                // oikeasti ylapivotti 0.5,1), palkit paatyivat n. 850+ yksikkoa liian ylas -
                // reilusti nakymaalueen ULKOPUOLELLE. Kayttajalle tama nayttaytyi "palkit
                // poistettu kokonaan" vaikka GameObjectit oikeasti loytyivat Hierarkiasta,
                // ne vain renderoityivat kuvaruudun ulkopuolelle.
                //
                // KORJAUS: lasketaan PauseButtonin YLAREUNAN KESKIKOHTA MAAILMAKOORDINAATEISSA
                // (GetWorldCorners - toimii oikein riippumatta valikontainereiden maarasta/
                // asettelusta valissa) ja muunnetaan se Canvasin OMAAN paikalliseen tilaan
                // (InverseTransformPoint). Uudet palkit liitetaan Canvasin alle KESKI-
                // ankkuroituna (sama konventio kuin Canvasin oma pivotti) sen sijaan etta
                // yritettaisiin matkia PauseButtonin ankkureita - talloin sijainti on aina
                // oikea riippumatta UI-hierarkian rakenteesta.
                Vector2 pauseTopCenterLocal;
                if (pauseRt != null && hudCanvasRt != null)
                {
                    var corners = new Vector3[4];
                    pauseRt.GetWorldCorners(corners); // 0=ala-vasen,1=yla-vasen,2=yla-oikea,3=ala-oikea
                    Vector3 topCenterWorld = (corners[1] + corners[2]) * 0.5f;
                    Vector3 local = hudCanvasRt.InverseTransformPoint(topCenterWorld);
                    pauseTopCenterLocal = new Vector2(local.x, local.y);
                }
                else
                {
                    pauseTopCenterLocal = new Vector2(0f, 350f); // varafallback jos PauseButtonia/Canvasia ei loydy
                }

                const float barW = 180f, barH = 16f, gap = 6f, marginAbovePause = 14f;

                Transform MakeMeterBar(string bgName, Color fillColor, string label, int stackIndex)
                {
                    var bg = GameObject.Find(bgName);
                    bool justCreated = bg == null;
                    if (bg == null)
                    {
                        bg = new GameObject(bgName, typeof(RectTransform));
                        Undo.RegisterCreatedObjectUndo(bg, "Luo " + bgName);
                        bg.transform.SetParent(hudCanvas.transform, false);
                        var bgImg0 = bg.AddComponent<Image>();
                        bgImg0.color = new Color(0f, 0f, 0f, 0.4f);
                    }

                    // Sijainti/koko paivitetaan JOKA ajolla (myos jos objekti oli jo olemassa
                    // vanhalla sijainnilla) - siksi taman ulkopuolella, ei vain justCreated-haarassa.
                    // KESKI-ankkuroitu (0.5,0.5) suoraan Canvasin alle, EI PauseButtonin
                    // ankkureita - ks. korjauskommentti ylla (pauseTopCenterLocal on jo
                    // muunnettu Canvasin paikalliseen tilaan GetWorldCorners+InverseTransformPoint
                    // -yhdistelmalla, joten se on aina oikein riippumatta UI-hierarkiasta).
                    var bgRt = (RectTransform)bg.transform;
                    bgRt.anchorMin = new Vector2(0.5f, 0.5f); bgRt.anchorMax = new Vector2(0.5f, 0.5f);
                    bgRt.pivot = new Vector2(0.5f, 0f);
                    float y = pauseTopCenterLocal.y + marginAbovePause + stackIndex * (barH + gap);
                    bgRt.anchoredPosition = new Vector2(pauseTopCenterLocal.x, y);
                    bgRt.sizeDelta = new Vector2(barW, barH);

                    var fillName = bgName + "_Fill";
                    var fillGo = bg.transform.Find(fillName)?.gameObject;
                    if (fillGo == null)
                    {
                        fillGo = new GameObject(fillName, typeof(RectTransform));
                        Undo.RegisterCreatedObjectUndo(fillGo, "Luo " + fillName);
                        fillGo.transform.SetParent(bg.transform, false);
                        var fillImg = fillGo.AddComponent<Image>();
                        fillImg.color = fillColor;
                        fillImg.type = Image.Type.Filled;
                        fillImg.fillMethod = Image.FillMethod.Horizontal;
                        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
                        fillImg.fillAmount = 0f;
                    }
                    var fillRt = (RectTransform)fillGo.transform;
                    // Pieni marginaali reunoista -> palkki nayttaa "kehyksen sisassa olevalta",
                    // siistimman naköinen kuin taysin reunasta reunaan tayttyva laatikko.
                    fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
                    fillRt.offsetMin = new Vector2(1.5f, 1.5f); fillRt.offsetMax = new Vector2(-1.5f, -1.5f);

                    // Pieni label-teksti palkin sisassa (esim. "Lika") - auttaa erottamaan
                    // kumpi palkki on kumpikin ilman erillista kuvaketta.
                    var labelGo = bg.transform.Find(bgName + "_Label")?.gameObject;
                    if (labelGo == null)
                    {
                        labelGo = new GameObject(bgName + "_Label", typeof(RectTransform));
                        Undo.RegisterCreatedObjectUndo(labelGo, "Luo label");
                        labelGo.transform.SetParent(bg.transform, false);
                        var lrt = (RectTransform)labelGo.transform;
                        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
                        var tmp = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
                        tmp.text = label;
                        tmp.fontSize = 11;
                        tmp.alignment = TMPro.TextAlignmentOptions.Center;
                        tmp.color = Color.white;
                        tmp.raycastTarget = false;
                    }

                    if (justCreated)
                        log.AppendLine($"{bgName}: luotu ({label}, siisti tayttyva palkki PauseButtonin ylapuolella).");
                    else
                        log.AppendLine($"{bgName}: sijainti/koko paivitetty (PauseButtonin ylapuolelle).");
                    return bg.transform.Find(fillName);
                }

                // HUOM: MakeMeterBar-kutsu ei ole enaa vain "jos hud.dirtMeterBar == null" -
                // haarassa, koska silloin jo KYTKETYN palkin sijaintia ei olisi koskaan
                // paivitetty PauseButtonin ylapuolelle vaikka tyokalua ajaisi uudelleen (taman
                // korjauksen koko tarkoitus). KORJAA-tilassa sijainti paivitetaan AINA.
                if (hud.dirtMeterBar == null)
                {
                    log.AppendLine("Dirt Meter Bar ei kytketty."); problems++;
                }
                if (fix)
                {
                    var t = MakeMeterBar("DirtMeterBar", new Color(0.55f, 0.4f, 0.2f, 1f), "Lika", 0);
                    hud.dirtMeterBar = t.GetComponent<Image>();
                    EditorUtility.SetDirty(hud);
                }

                if (hud.dentMeterBar == null)
                {
                    log.AppendLine("Dent Meter Bar ei kytketty."); problems++;
                }
                if (fix)
                {
                    var t = MakeMeterBar("DentMeterBar", new Color(0.6f, 0.25f, 0.2f, 1f), "Kolhut", 1);
                    hud.dentMeterBar = t.GetComponent<Image>();
                    EditorUtility.SetDirty(hud);
                }
            }
            else if (hud == null)
            {
                log.AppendLine("(RaceHUD-komponenttia ei loydy scenesta - lika-/kolhumittareita ei voitu tarkistaa.)");
            }

            log.AppendLine(problems == 0 ? "\nKaikki kunnossa!" : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty." : " Aja 'Pakettiporina/Korjaus/01 - KORJAA peliscene'."));
            if (fix) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(log.ToString());
        }
    }
}
#endif