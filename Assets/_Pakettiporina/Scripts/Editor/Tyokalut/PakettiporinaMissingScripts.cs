#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // LISATTY 19.8.2026 (kayttaja: "kun pelin kaynnistaa niin antaa seuraavan
    // varoituksen konsolissa jokaiselle objektille: The referenced script
    // (Unknown) on this Behaviour is missing!"). Unity ei koskaan kerro TASSA
    // varoituksessa MIKA objekti on kyseessa - vain etta jollain GameObjectilla
    // on komponentti jonka scripti-viittaus on rikki (poistettu/uudelleennimetty
    // .cs-tiedosto, tai vanha prefab-instanssi jonka GUID ei enaa tasmaa mihinkaan
    // scriptiin). Tama tyokalu kayy koko avoimen scenen lapi (myos piilotetut/
    // inaktiiviset objektit) ja tulostaa jokaisen GameObjectin TASMALLISEN
    // hierarkiapolun jolla on >=1 puuttuva scripti-komponentti, jotta ne loytaa
    // ja poistaa/korjaa kasin Unityssa (valitse rivi Hierarkiassa, poista
    // "Missing (Mono Script)" -komponentti Inspectorista).
    //
    // Kaytto: Pakettiporina -> Tyokalut -> 05 - Etsi puuttuvat scriptit (konsoli)
    public static class PakettiporinaMissingScripts
    {
        [MenuItem("Pakettiporina/Tyokalut/05 - Etsi puuttuvat scriptit (konsoli)")]
        public static void FindMissingScripts()
        {
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var s = new StringBuilder();
            s.AppendLine($"=== PUUTTUVAT SCRIPTIT: {sceneName} ===");

            int objectCount = 0;
            int componentCount = 0;

            // FindObjectsOfType(true) loytaa myos inaktiiviset objektit - GetRootGameObjects
            // + rekursio tarvitaan reitin (esim. "Rata/Esteet/Kartio_03") tulostamiseen.
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                Walk(root.transform, s, ref objectCount, ref componentCount);
            }

            if (objectCount == 0)
                s.AppendLine("Ei loytynyt puuttuvia scripteja itse scenesta.");
            else
                s.AppendLine($"--- Scenessa yhteensa {componentCount} puuttuvaa scripti-komponenttia {objectCount} objektissa. ---");

            // LISATTY 19.8.2026: kayttaja ajoi taman Puistossa scenen ollessa auki, tulos oli
            // tyhja, MUTTA varoitus tulee silti kun PELI KAYNNISTETAAN. Tama tarkoittaa etta
            // rikkinaninen komponentti EI ole itse scenessa vaan jossain mika INSTANTIOIDAAN
            // ajon aikana - eli Prefab-ASSETissa (esim. UI-ikoni/partikkeli/HUD-elementti jota
            // koodi luo Instantiate()lla). Tallainen ei nay pelkassa scene-hierarkiassa. Kaydaan
            // siksi lapi KAIKKI projektin Prefab-assetit ja niiden koko hierarkia.
            s.AppendLine();
            s.AppendLine("=== PUUTTUVAT SCRIPTIT: kaikki projektin Prefab-assetit ===");
            int prefabObjCount = 0, prefabCompCount = 0, prefabsScanned = 0;
            // LAAJENNETTU 19.8.2026: "t:Prefab" loytaa VAIN oikeat Prefab-assetit (sininen
            // kuutio-ikoni) - se EI loyda raakoja FBX/Model-tiedostoja, vaikka niitakin
            // instantioidaan PrefabUtility.InstantiatePrefab():lla (AddDecor() kayttaa
            // molempia). "t:GameObject" kattaa molemmat tyypit samalla haulla.
            foreach (var guid in AssetDatabase.FindAssets("t:GameObject"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabRoot == null) continue;
                prefabsScanned++;
                WalkPrefab(prefabRoot.transform, path, s, ref prefabObjCount, ref prefabCompCount);
            }
            s.AppendLine(prefabObjCount == 0
                ? $"Ei loytynyt puuttuvia scripteja {prefabsScanned} tarkistetusta Prefabista."
                : $"--- Prefabeissa yhteensa {prefabCompCount} puuttuvaa scripti-komponenttia {prefabObjCount} objektissa (tarkistettu {prefabsScanned} Prefabia). ---");

            if (objectCount == 0 && prefabObjCount == 0)
                s.AppendLine("EI LOYTYNYT MITAAN - jos varoitus silti tulee pelia kaynnistaessa, tarkista onko jokin 'Bootstrap'/pysyva scene (DontDestroyOnLoad) latautumassa Puiston lisaksi, tai onko StreamingAssets/Resources-kansiossa suoraan ladattavia .prefab-tiedostoja joita AssetDatabase ei nayta ajonaikaisessa buildissa (Editorissa pitaisi silti loytya).");
            else
                s.AppendLine("Korjaus: Scenen riveille - valitse Hierarkiasta, poista 'Missing (Mono Script)' -komponentti Inspectorista (tai koko objekti jos se on vanha/orpo jaanne). Prefab-riveille - avaa Prefabin polku Project-ikkunasta (kaksoisklikkaa), poista sama tapa Prefab-editorissa, tallenna.");

            Debug.Log(s.ToString());
        }

        // LISATTY 19.8.2026: kayttaja ajoi ylla olevan Puistossa - seka scene etta kaikki
        // Prefabit puhtaita, MUTTA varoitus tulee edelleen "kun pelin kaynnistaa" (todennakoisesti
        // pelaajan oikea aloitusreitti MainMenusta, ei Puisto suoraan Editorissa auki). Tama
        // tarkoittaa etta rikkinainen komponentti on jossain TOISESSA scenessa (esim. MainMenu/
        // Garage/Wash tai muu joka ei ole talla hetkella auki Editorissa) - esim. pysyva
        // DontDestroyOnLoad-manageri joka luodaan MainMenussa ja kulkeutuu mukana Puistoon.
        // Tama tyokalu kay lapi KAIKKI Assets/Scenes-kansion .unity-tiedostot (paitsi TextMesh
        // Pro:n omat esimerkkiscenet) LATAAMALLA JOKAISEN ADDITIIVISESTI, skannaamalla sen,
        // ja SULKEMALLA sen taas (tallentamatta) - talla hetkella auki oleva scene ei koskaan
        // muutu eika mitaan tallenneta, joten tama on turvallinen ajaa milloin tahansa.
        [MenuItem("Pakettiporina/Tyokalut/06 - Etsi puuttuvat scriptit KAIKISTA sceneista (konsoli)")]
        public static void FindMissingScriptsAllScenes()
        {
            var s = new StringBuilder();
            s.AppendLine("=== PUUTTUVAT SCRIPTIT: KAIKKI SCENET ===");
            s.AppendLine("(Kayttaja jaljelle jaanyt scene pysyy koskemattomana - jokainen tarkistettava scene ladataan ja suljetaan taustalla.)");

            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            // Jo auki olevat scenet (esim. talla hetkella editoitava Puisto) skannataan
            // suoraan paikoillaan - EI avata toista kertaa additiivisesti (loisi kaksoiskappaleen).
            var alreadyOpenPaths = new HashSet<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
                alreadyOpenPaths.Add(SceneManager.GetSceneAt(i).path);

            int totalObj = 0, totalComp = 0;

            foreach (var path in scenePaths)
            {
                Scene scene;
                bool openedHere = false;
                if (alreadyOpenPaths.Contains(path))
                {
                    scene = SceneManager.GetSceneByPath(path);
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    openedHere = true;
                }

                int objCount = 0, compCount = 0;
                var sceneSb = new StringBuilder();
                foreach (var root in scene.GetRootGameObjects())
                    Walk(root.transform, sceneSb, ref objCount, ref compCount);

                if (objCount > 0)
                {
                    s.AppendLine($"--- {scene.name} ({path}): {compCount} puuttuvaa scripti-komponenttia {objCount} objektissa ---");
                    s.Append(sceneSb);
                    totalObj += objCount;
                    totalComp += compCount;
                }

                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true); // true = poista muistista, EI tallenneta
            }

            if (totalObj == 0)
                s.AppendLine("Ei loytynyt puuttuvia scripteja YHDESSAKAAN scenessa. Jos varoitus silti tulee, kyseessa on todennakoisesti ScriptableObject-asset (esim. Asetukset/SaveData/RaceConfig-tyyppinen data-asset) - kerro missa kohtaa pelia varoitus TARKALLEEN ilmestyy (heti alussa MainMenussa? Vasta radalle siirryttaessa? Wash/Garage-vaiheessa?) niin kohdistetaan haku sinne.");
            else
                s.AppendLine($"=== YHTEENSA: {totalComp} puuttuvaa scripti-komponenttia {totalObj} objektissa koko projektissa. ===");

            Debug.Log(s.ToString());
        }

        // LISATTY 19.8.2026: kayttaja ajoi 06:n - loytyi 56 puuttuvaa scriptia Huvipuisto/
        // Maatila/Satama-sceneissa, KAIKKI "Map/Objektit/..." -koristeissa (esim.
        // Huvipuisto_Maailmanpyora, Maatila_Navetta, Satama_Majakka). Syy jaljitetty:
        // PakettiporinaDecor.cs:n AddDecor() lisaa jokaiseen luomaansa koristeeseen
        // `PakettiporinaDecorMarker`-merkkikomponentin (rivi 1005) - luokka ITSE on
        // edelleen olemassa samassa tiedostossa (ei poistettu), mutta scenen tallennettu
        // viittaus osoittaa GUID:iin joka ei enaa tasmaa mihinkaan scriptiin (todennakoisesti
        // .meta-tiedoston GUID vaihtui jossain vaiheessa tata sessiota tehtyjen suorien
        // tiedostomuokkausten seurauksena, ei Unity Editorin kautta). Puisto ja Game eivat
        // kayta AddDecor()-koristeita samalla tavalla / niiden koristeet on lisatty eri
        // reittia, joten vain nama 3 scenea kärsivät.
        //
        // PakettiporinaDecorMarker on PUHTAASTI editor-tyokalun kirjanpitoa (tallettaa
        // modelName-merkkijonon, jotta Lisays/03:n siivouspassi ja "16 - Vie sijainnit
        // koodiksi" loytavat/tunnistavat koristeet) - EI vaikuta renderointiin, tormaykseen
        // eika pelilogiikkaan millaan tavalla. Koska viittaus on jo rikki, sen sisaltama
        // data (modelName) on joka tapauksessa lukukelvoton - turvallisin korjaus on
        // POISTAA pelkka rikkinainen komponentti (`GameObjectUtility.
        // RemoveMonoBehavioursWithMissingScript`), joka jattaa itse koristeobjektin
        // (malli/Transform/muut komponentit) koskemattomaksi. HUOM: taman jalkeen Lisays/03:n
        // "vanhojen koristeiden siivous" (FindObjectsOfType<PakettiporinaDecorMarker>) ei
        // enaa loyda/tunnista NAITA tiettyja objekteja merkin kautta (koska merkki poistuu) -
        // jos Lisays/03 ajetaan uudelleen jommassakummassa naista 3 scenesta, se saattaa
        // lisata UUDET koristeet vanhojen PAALLE eika korvaa niita. Ei ongelma nyt (ei
        // suunnitteilla uutta Lisays/03-ajoa nailla sceneilla), mutta pida mielessa.
        [MenuItem("Pakettiporina/Tyokalut/07 - Poista puuttuvat scriptit KAIKISTA sceneista (KORJAA)")]
        public static void RemoveMissingScriptsAllScenes()
        {
            var s = new StringBuilder();
            s.AppendLine("=== PUUTTUVIEN SCRIPTIEN POISTO: KAIKKI SCENET ===");

            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            var alreadyOpenPaths = new HashSet<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
                alreadyOpenPaths.Add(SceneManager.GetSceneAt(i).path);

            int totalRemoved = 0;
            int scenesChanged = 0;

            foreach (var path in scenePaths)
            {
                Scene scene;
                bool openedHere = false;
                if (alreadyOpenPaths.Contains(path))
                {
                    scene = SceneManager.GetSceneByPath(path);
                }
                else
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    openedHere = true;
                }

                int removedHere = 0;
                var failedHere = new List<string>();
                foreach (var root in scene.GetRootGameObjects())
                    removedHere += RemoveMissingRecursive(root, failedHere);

                if (removedHere > 0)
                {
                    totalRemoved += removedHere;
                    scenesChanged++;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    s.AppendLine($"{scene.name} ({path}): poistettu {removedHere} rikkinaista scripti-komponenttia, tallennettu.");
                }
                if (failedHere.Count > 0)
                {
                    s.AppendLine($"{scene.name}: {failedHere.Count} kohdetta EI voitu poistaa (suojattu data - todennakoisesti Transform-slotti, ei koskettu):");
                    foreach (var f in failedHere) s.AppendLine("  " + f);
                }

                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }

            s.AppendLine(totalRemoved == 0
                ? "Ei loytynyt/poistettu mitaan."
                : $"=== YHTEENSA poistettu {totalRemoved} rikkinaista komponenttia {scenesChanged} scenesta. Konsolivaroitusten pitaisi loppua. ===");

            Debug.Log(s.ToString());
        }

        // KORJATTU 19.8.2026: `GameObjectUtility.RemoveMonoBehavioursWithMissingScript`
        // palautti 0 Prefab-instansseille (koristeet ovat PrefabUtility.InstantiatePrefab-
        // instansseja). KORJATTU (jatko): vaihdettu suoraan SerializedObject-manipulaatioon
        // m_Component-taulukkoon - TAMA OSOITTAUTUI MYOS RIKKINAISEKSI: Unity kirjoitti
        // "It is not allowed to modify the data property" konsoliin JOKAISESTA poistosta,
        // EIKA se ollut kosmeettinen - poisto ei OIKEASTI tapahtunut, vaikka koodi ei
        // heittanyt .NET-poikkeusta (native-tason esto, ei C#-exception) ja siksi "removed++"
        // suoritui virheellisesti joka kerta. Kayttaja pelitesti uudelleen 19.8. ja saman
        // 6/26/24 maaran varoituksia tuli TAKAISIN Huvipuisto/Maatila/Satama-sceneissa -
        // todiste etta scenetiedostot eivat oikeasti muuttuneet, vaikka tyokalu vaitti
        // tallentaneensa. KORJATTU LOPULLISESTI: puretaan Prefab-instanssi TAYSIN irti
        // (`PrefabUtility.UnpackPrefabInstance(..., Completely, ...)`) ENNEN poistoa - tama
        // muuttaa koristeen tavalliseksi (ei-Prefab-linkitetyksi) GameObjectiksi, jolloin
        // `GameObjectUtility.RemoveMonoBehavioursWithMissingScript` (Unityn oma, dokumentoitu
        // API tahan tarkoitukseen) toimii normaalisti ilman Prefab-override-rajoitusta.
        // Koristeet eivat tarvitse pysya Prefab-linkitettyina (staattista koristelua, ei
        // koskaan "Revert to Prefab" -tarvetta), joten irrottaminen on turvallinen.
        static int RemoveMissingRecursive(GameObject go, List<string> failed)
        {
            int removed = 0;

            if (PrefabUtility.IsPartOfAnyPrefab(go))
            {
                var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (outermost != null)
                    PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            try
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            }
            catch (System.Exception e)
            {
                failed.Add($"{GetPath(go.transform)}: {e.Message}");
            }

            for (int i = 0; i < go.transform.childCount; i++)
                removed += RemoveMissingRecursive(go.transform.GetChild(i).gameObject, failed);
            return removed;
        }

        static void Walk(Transform t, StringBuilder s, ref int objectCount, ref int componentCount)
        {
            var comps = t.GetComponents<Component>();
            int missing = 0;
            foreach (var c in comps)
            {
                if (c == null) missing++; // "null mutta ei oikeasti null" = puuttuva scripti-viittaus
            }
            if (missing > 0)
            {
                objectCount++;
                componentCount += missing;
                s.AppendLine($"{GetPath(t)}  (puuttuvia scripteja: {missing})");
            }
            for (int i = 0; i < t.childCount; i++)
                Walk(t.GetChild(i), s, ref objectCount, ref componentCount);
        }

        static void WalkPrefab(Transform t, string assetPath, StringBuilder s, ref int objectCount, ref int componentCount)
        {
            var comps = t.GetComponents<Component>();
            int missing = 0;
            foreach (var c in comps)
            {
                if (c == null) missing++;
            }
            if (missing > 0)
            {
                objectCount++;
                componentCount += missing;
                s.AppendLine($"{assetPath} :: {GetPath(t)}  (puuttuvia scripteja: {missing})");
            }
            for (int i = 0; i < t.childCount; i++)
                WalkPrefab(t.GetChild(i), assetPath, s, ref objectCount, ref componentCount);
        }

        static string GetPath(Transform t)
        {
            string path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }
}
#endif
