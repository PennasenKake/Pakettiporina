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
        [MenuItem("Pakettiporina/Lisays/02 - Lisaa pelielementit taman radan radalle")]
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
                if (t.name.StartsWith("Boost_") || t.name.StartsWith("Latakko_") || t.name.StartsWith("Kartio_") || t.name.StartsWith("Laatikko_"))
                {
                    Undo.DestroyObjectImmediate(t.gameObject);
                    cleared++;
                }
            }
            if (cleared > 0)
                s.AppendLine($"Siivottu {cleared} vanhaa pelielementtia (myos nykyisessa listassa enaa esiintymattomat).");

            // KAYTTAJAN TESTAAMAT (x,z) -YLIKIRJOITUKSET: jos preset.boostXZ/conesXZ on
            // annettu (ei-tyhja), kaytetaan naita SUORAAN eika lasketa X:aa road_x(z):sta.
            // Lisatty 8.8.2026 Huvipuistolle, koska kayttaja totesi Unityssa etta kaavasta
            // laskettu X ei osunut nakyvalle tielle radan alkupaassa - ks. TrackDef-kommentti.
            int i = 1;
            if (preset.boostXZ != null && preset.boostXZ.Length > 0)
            {
                foreach (var (z, x) in preset.boostXZ)
                    PlaceBoostXZ($"Boost_{i++:00}", x, z, s);
            }
            else
            {
                foreach (var z in preset.boostZ)
                    PlaceBoost($"Boost_{i++:00}", z, preset.bends, preset.roadBaseX, s);
            }

            i = 1;
            foreach (var z in preset.puddleZ)
                PlacePuddle($"Latakko_{i++:00}", z, preset.bends, preset.roadBaseX, s);

            i = 1;
            if (preset.conesXZ != null && preset.conesXZ.Length > 0)
            {
                foreach (var (z, x) in preset.conesXZ)
                    PlaceConeXZ($"Kartio_{i++:00}", x, z, s);
            }
            else
            {
                foreach (var (z, offset) in preset.cones)
                    PlaceCone($"Kartio_{i++:00}", z, offset, preset.bends, preset.roadBaseX, s);
            }

            // LISATTY 14.8.2026 (kayttaja: "sailytetaan esteina kartiot, laatikot ja
            // latakot... niita tulee olla joka radassa riittavasti ja tasaisesti").
            i = 1;
            if (preset.laatikotXZ != null && preset.laatikotXZ.Length > 0)
            {
                foreach (var (z, x) in preset.laatikotXZ)
                    PlaceLaatikkoXZ($"Laatikko_{i++:00}", x, z, s);
            }
            else if (preset.laatikot != null)
            {
                foreach (var (z, offset) in preset.laatikot)
                    PlaceLaatikko($"Laatikko_{i++:00}", z, offset, preset.bends, preset.roadBaseX, s);
            }

            if (preset.boostZ.Length == 0 && preset.puddleZ.Length == 0 && preset.cones.Length == 0
                && (preset.laatikot == null || preset.laatikot.Length == 0))
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

        // LISATTY 16.8.2026 (kayttaja: "voisiko niista samalla tavalla toimivia .fbx
        // malleja luoda nykyisten esteiden ja tehosteiden tilalle") - Kartio/Laatikko/
        // Latakko/Boost olivat pelkkia Unity-primitiiveja (Cylinder/Cube). Nyt oikeat
        // Blenderissa mallinnetut FBX:t (liikennekartio valkoisella raidalla, puulaatikko
        // lautajaolla, epasaannollinen tumma-reunainen lätäkkö, vihrea nuolipad) samassa
        // kansiossa/tyylilla kuin muut projektin mallit. Sama PrefabUtility.InstantiatePrefab
        // -periaate kuin PakettiporinaDecor.cs:ssa. Mallit on rakennettu Blenderissa
        // Y-YLOS-paikallisesti (POIKKEAA Decor-jarjestelman rotX=-90-konventiosta!) juuri
        // jotta identity-rotaatio (ei rotX-korjausta) riittaa - sama kaytanto kuin vanhoilla
        // primitiiveilla, joten mikaan olemassaoleva sijainti/rotaatiokoodi ei tarvinnut
        // muutoksia. Jos FBX:aa ei loydy (esim. Unity ei ole viela tuonut sita), palataan
        // VANHAAN primitiivi-fallbackiin ettei tyokalu jaa kokonaan toimimattomaksi.
        const string OBSTACLE_MODELS_DIR = "Assets/_Pakettiporina/Art/Models/Esteet";

        static GameObject FindOrCreateModel(string name, string fbxName, PrimitiveType fallbackPrim, out bool wasFbx)
        {
            wasFbx = false;
            GameObject go = null;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
            {
                if (t.name == name) { go = t.gameObject; break; }
            }
            if (go != null) return go;

            string path = $"{OBSTACLE_MODELS_DIR}/{fbxName}.fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(model);
                go.name = name;
                wasFbx = true;
            }
            else
            {
                go = GameObject.CreatePrimitive(fallbackPrim);
                go.name = name;
            }
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
            return go;
        }

        // FBX-instantioinnilla EI tule automaattista Collideria (toisin kuin
        // GameObject.CreatePrimitive, joka lisaa sen itse) - lisataan tarvittaessa kasin.
        static Collider EnsureCollider(GameObject go, bool isTrigger, bool useCapsule)
        {
            var col = go.GetComponent<Collider>();
            if (col == null)
                col = useCapsule ? (Collider)go.AddComponent<CapsuleCollider>() : go.AddComponent<BoxCollider>();
            col.isTrigger = isTrigger;
            return col;
        }

        static Material FlatColor(string name, Color c)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            return new Material(shader) { name = name, color = c };
        }

        static void PlaceBoost(string name, float z, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Boost", PrimitiveType.Cube, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.TEHOSTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX);
            go.transform.position = new Vector3(x, 0.05f, z);
            go.transform.localScale = new Vector3(3f, 0.1f, 3f);
            EnsureCollider(go, isTrigger: true, useCapsule: false);
            if (go.GetComponent<Boost>() == null) go.AddComponent<Boost>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Boost_Vihrea", new Color(0.25f, 0.85f, 0.35f));
            s.AppendLine($"{name}: ({x:F1}, 0.05, {z:F0})");
        }

        // Sama kuin PlaceBoost, mutta X annetaan suoraan (kayttajan Unityssa testaama
        // sijainti) - ei lasketa road_x(z):sta. Ks. TrackDef.boostXZ-kommentti.
        static void PlaceBoostXZ(string name, float x, float z, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Boost", PrimitiveType.Cube, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.TEHOSTEET), false);
            go.transform.position = new Vector3(x, 0.05f, z);
            go.transform.localScale = new Vector3(3f, 0.1f, 3f);
            EnsureCollider(go, isTrigger: true, useCapsule: false);
            if (go.GetComponent<Boost>() == null) go.AddComponent<Boost>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Boost_Vihrea", new Color(0.25f, 0.85f, 0.35f));
            s.AppendLine($"{name}: ({x:F1}, 0.05, {z:F0}) [kiinnitetty]");
        }

        // Sama kuin PlaceCone, mutta X annetaan suoraan. Ks. TrackDef.conesXZ-kommentti.
        static void PlaceConeXZ(string name, float x, float z, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Kartio", PrimitiveType.Cylinder, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            go.transform.position = new Vector3(x, 0.4f, z);
            go.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            EnsureCollider(go, isTrigger: false, useCapsule: true);
            if (go.GetComponent<Cone>() == null) go.AddComponent<Cone>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Kartio_Oranssi", new Color(0.95f, 0.5f, 0.15f));
            s.AppendLine($"{name}: ({x:F1}, 0.4, {z:F0}) [kiinnitetty]");
        }

        // LISATTY 14.8.2026 (kayttaja: "sailytetaan esteina kartiot, laatikot ja
        // latakot") - sama rakenne kuin PlaceCone/PlaceConeXZ, mutta ruskea laatikko +
        // Laatikko-komponentti Cone.n sijaan (oma esteluokka, ks. Laatikko.cs).
        static void PlaceLaatikko(string name, float z, float offsetX, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Laatikko", PrimitiveType.Cube, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX) + offsetX;
            go.transform.position = new Vector3(x, 0.4f, z);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            EnsureCollider(go, isTrigger: false, useCapsule: false);
            if (go.GetComponent<Laatikko>() == null) go.AddComponent<Laatikko>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Laatikko_Ruskea", new Color(0.55f, 0.36f, 0.18f));
            s.AppendLine($"{name}: ({x:F1}, 0.4, {z:F0})");
        }

        static void PlaceLaatikkoXZ(string name, float x, float z, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Laatikko", PrimitiveType.Cube, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            go.transform.position = new Vector3(x, 0.4f, z);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            EnsureCollider(go, isTrigger: false, useCapsule: false);
            if (go.GetComponent<Laatikko>() == null) go.AddComponent<Laatikko>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Laatikko_Ruskea", new Color(0.55f, 0.36f, 0.18f));
            s.AppendLine($"{name}: ({x:F1}, 0.4, {z:F0}) [kiinnitetty]");
        }

        // KORJATTU 16.8.2026 (kayttaja: "Latakon tulisi muistuttaa enemman latakkoa ja
        // erottua tiesta", myohemmin: "voisiko niista samalla tavalla toimivia .fbx
        // malleja luoda"). Vanha versio oli taydellisen pyorea, vaalea, puolilapinakyva
        // kiekko joka hukkui harmaaseen tiehen; valivaiheessa parannettiin kahdella
        // erillisella runtime-primitiivilla (paablobi+halo). NYT: yksi Blenderissa
        // mallinnettu FBX ("Latakko.fbx") jossa epasaannollinen blobimuoto JA tumma
        // reunahalo ovat jo valmiiksi samassa meshissa (2 materiaalislottia) - ei enaa
        // tarvita erillista "_Reuna"-apuobjektia. Per-latakko-vaihtelu (koko/kiertokulma)
        // pysyy silti deterministisena z-koordinaatista, sovelletaan FBX:n paalle scale/
        // rotation-arvoina jolloin jokainen latakko on silti hieman erilainen.
        static void PlacePuddle(string name, float z, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            float x = PakettiporinaTracks.RoadX(z, bends, baseX);

            // Deterministinen "satunnaisuus" z-koordinaatista + nimesta - sama tulos joka ajolla.
            int seed = Mathf.Abs(Mathf.RoundToInt(z * 17f)) + name.GetHashCode();
            var rnd = new System.Random(seed);
            float scaleX = 3.6f + (float)rnd.NextDouble() * 1.4f;  // 3.6 - 5.0
            float scaleZ = 3.2f + (float)rnd.NextDouble() * 1.4f;  // 3.2 - 4.6 (eri kuin X -> soikea, ei ympyra)
            float rotY = (float)rnd.NextDouble() * 360f;

            // Siivoa mahdollinen vanha erillinen "_Reuna"-apuobjekti edellisesta
            // (kaksi-primitiivia) -versiosta - FBX:ssa reuna on jo mukana meshissa.
            var legacyHalo = GameObject.Find(name + "_Reuna");
            if (legacyHalo != null) Undo.DestroyObjectImmediate(legacyHalo);

            var go = FindOrCreateModel(name, "Latakko", PrimitiveType.Cylinder, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            go.transform.position = new Vector3(x, 0.03f, z);
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            // FBX-malli on jo litteä (paikallinen Y-korkeus ~0.01) - ei tarvitse enaa
            // pakottaa Y-skaalaa lahes nollaan kuten vanhalla Cylinder-primitiivilla.
            go.transform.localScale = new Vector3(scaleX, isFbx ? 1f : 0.02f, scaleZ);
            EnsureCollider(go, isTrigger: true, useCapsule: false);
            if (go.GetComponent<Puddle>() == null) go.AddComponent<Puddle>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null)
                rend.sharedMaterial = WetMaterial("Latakko_Sininen", new Color(0.08f, 0.24f, 0.44f, 0.92f), 0.9f);

            s.AppendLine($"{name}: ({x:F1}, 0.03, {z:F0}) [soikea {scaleX:F1}x{scaleZ:F1}, rot {rotY:F0} deg]");
        }

        // Sama kuin FlatColor, mutta lisaa lapinakyvyyden (URP Transparent-tila) ja
        // korkean Smoothness/Metallic-arvon jotta pinta heijastaa valoa kuten oikea vesi.
        static Material WetMaterial(string name, Color c, float smoothness)
        {
            var mat = FlatColor(name, c);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness); // Standard-shaderin nimi
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // URP/Lit: 0=Opaque, 1=Transparent
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return mat;
        }

        static void PlaceCone(string name, float z, float offsetX, (float, float, float)[] bends, float baseX, StringBuilder s)
        {
            var go = FindOrCreateModel(name, "Kartio", PrimitiveType.Cylinder, out bool isFbx);
            go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.ESTEET), false);
            float x = PakettiporinaTracks.RoadX(z, bends, baseX) + offsetX;
            go.transform.position = new Vector3(x, 0.4f, z);
            go.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
            // Kartio on kiintea este (Is Trigger POIS) - auto tormaa siihen fyysisesti.
            EnsureCollider(go, isTrigger: false, useCapsule: true);
            if (go.GetComponent<Cone>() == null) go.AddComponent<Cone>();
            var rend = go.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial == null) rend.sharedMaterial = FlatColor("Kartio_Oranssi", new Color(0.95f, 0.5f, 0.15f));
            s.AppendLine($"{name}: ({x:F1}, 0.4, {z:F0})");
        }
    }
}
#endif
