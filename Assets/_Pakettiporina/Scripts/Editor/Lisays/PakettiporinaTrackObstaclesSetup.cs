#if UNITY_EDITOR
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Task 4 (14.8.2026, kayttajan pyynto "Lisaa esteita ja tehosteita ratoihin +
    // saippuakupla-mekaniikka"): lisaa/paivittaa Boost/Kartio/Latakko-esteita seka UUDEN
    // TrackBubble-kuplamekaniikan avoinna olevaan ratasceneen.
    //
    // AJA se ratascene AUKI jota haluat taydentaa (Puisto/Satama/Huvipuisto/Maatila - katso
    // alla), ja suorita tama tyokalu. Tallenna scene lopuksi (Ctrl+S).
    //
    // Puisto/Satama/Huvipuisto: oli jo VALMIIKSI oikein sijoitettuja Boost/Kartio/Latakko-
    // esteita (Tehosteet/Esteet-kansioiden alla, "Map"-objektin lapsina) - naiden todellisia
    // maailma-koordinaatteja kayttaen uudet lisaykset sijoitetaan LUOTETTAVASTI radan
    // varrelle (kloonataan olemassa oleva este ja siirretaan hieman sivuun/valiin).
    //
    // Maatila (LISATTY 14.8.2026): kayttaja ajoi "Tyokalut/01 - Nayta tien profiili"
    // -tyokalun ja antoi oikean roadX(z)-taulukon konsolista. Maatilalla ei ollut yhtaan
    // valmista Boost/Kartio/Latakko-objektia kloonattavaksi (vain Star_01), joten AddMaatila()
    // rakentaa esteet TYHJASTA primitiiveista (sama kaava kuin CreateBubble) ja sijoittaa ne
    // roadX(z):n paalle taman taulukon perusteella - katso AddMaatila()-metodin kommentit.
    //
    // Game/Game2: ei viela roadX(z)-taulukkoa eika yhtaan valmista estetta - tama tyokalu
    // EI VIELA lisaa mitaan naihin kahteen sceneen. Aja "Tyokalut/01" ensin niissa (jos
    // PakettiporinaTracks.cs:ssa on niille preset) ja anna tulostus, niin lisataan samalla
    // tavalla kuin Maatilalle nyt tehtiin.
    //
    // POISTETTU KAYTOSTA 14.8.2026 (kayttaja: "poistetaan kaikilta sceneilta kaytosta,
    // vain tahdet keraittavissa" - Kupla oli sekoittanut kayttajaa, ei liittynyt pesuun
    // niinkuin alkuperainen "poksauta kuplia radan jalkeen" -idea, ja oli paallekkainen
    // konsepti tahtien kanssa). Tama tyokalu EI ENAA sijoita TrackBubble-kuplia mihinkaan
    // sceneen - RemoveExistingBubbles() sen sijaan siivoaa pois jo olemassa olevat
    // Kupla_-objektit (esim. Maatilalla oli 3 kpl) joka kerta kun tyokalua ajetaan.
    // TrackBubble.cs-luokka on jatetty koodikantaan (ei poistettu tiedostona), mutta
    // mikaan tyokalu ei enaa luo sen instansseja.
    public static class PakettiporinaTrackObstaclesSetup
    {
        [MenuItem("Pakettiporina/Lisays/10 - Lisaa esteita ja tehosteita avattuun rataan")]
        public static void AddTrackObstacles()
        {
            var s = new StringBuilder();
            string scene = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== ESTEET/TEHOSTEET ({scene}) ===");

            var mapGo = GameObject.Find("Map");
            if (mapGo == null)
            {
                s.AppendLine("VIRHE: scenesta ei loydy 'Map'-nimista objektia - avaa oikea ratascene.");
                Debug.LogError(s.ToString());
                return;
            }
            var map = mapGo.transform;

            RemoveExistingBubbles(map, ref s);

            switch (scene)
            {
                case "Puisto":
                    AddPuisto(map, ref s);
                    break;
                case "Satama":
                    AddSatama(map, ref s);
                    break;
                case "Huvipuisto":
                    AddHuvipuisto(map, ref s);
                    break;
                case "Maatila":
                    AddMaatila(map, ref s);
                    break;
                case "Game":
                case "Game2":
                    s.AppendLine($"'{scene}':lla ei ole viela yhtaan valmista estetta jonka " +
                        "sijaintia voisi kayttaa referenssina - tama tyokalu EI lisaa mitaan " +
                        "tahan sceneen (ks. luokkakommentti). Sijoitellaan kasin tai omalla erallaan.");
                    break;
                default:
                    s.AppendLine($"Tuntematon scene '{scene}' - tama tyokalu tukee Puisto/Satama/Huvipuisto/Maatila-ratoja.");
                    break;
            }

            Debug.Log(s.ToString());
        }

        // ---------- Puisto: 5 Boostia valmiina (Z -120..120, X vaihtelee 0/±13.5) ----------
        static void AddPuisto(Transform map, ref StringBuilder s)
        {
            var tehosteet = FindOrCreateContainer(map, "Tehosteet", ref s);
            CloneObstacle(tehosteet, "Boost_01", "Boost_06", new Vector3(6.75f, 0.05f, -90f), ref s);
            CloneObstacle(tehosteet, "Boost_01", "Boost_07", new Vector3(-6.75f, 0.05f, 90f), ref s);
        }

        // ---------- Satama: 7 Kartiota + 1 Latakko valmiina (Z -130..125, X 5.5..28.5) ----------
        static void AddSatama(Transform map, ref StringBuilder s)
        {
            var esteet = FindOrCreateContainer(map, "Esteet", ref s);
            CloneObstacle(esteet, "Kartio_01", "Kartio_08", new Vector3(10f, 0.4f, -20f), ref s);
            CloneObstacle(esteet, "Kartio_01", "Kartio_09", new Vector3(15f, 0.4f, 90f), ref s);
            CloneObstacle(esteet, "Latakko_01", "Latakko_02", new Vector3(20f, 0.03f, -100f), ref s);
        }

        // ---------- Huvipuisto: jo 8 Boost + 4 Kartio + 4 Latakko valmiina, ei lisattavaa ----------
        static void AddHuvipuisto(Transform map, ref StringBuilder s)
        {
            s.AppendLine("Huvipuistolla oli jo riittavasti Boost/Kartio/Latakko - ei lisatty mitaan.");
        }

        // ---------- Maatila (14.8.2026): kayttaja ajoi "Tyokalut/01 - Nayta tien profiili"
        // -tyokalun ja antoi oikean roadX(z)-taulukon (Z -150..150, tie 12 yks. levea,
        // kaksi S-kaarretta: oikealle Z -100..-20 (huippu X=13.58 kohdassa Z -70..-60),
        // vasemmalle Z 20..100 (huippu X=-13.58 kohdassa Z 60..70)). Maatilalla EI ollut
        // yhtaan valmista Boost/Kartio/Latakko-objektia kloonattavaksi (vain Star_01),
        // joten esteet rakennetaan TYHJASTA primitiiveista (sama kaava kuin CreateBubble) -
        // vaihda materiaali/mesh Inspectorista halutessasi nayttavamman ilmeen. Kaikki
        // sijainnit ovat roadX(z) +/- muutama yksikko (tien puolikas 6 yks.), eivat siis
        // koskaan yli 6 yksikon paassa keskilinjasta - pysyvat aina ajettavalla tiella.
        static void AddMaatila(Transform map, ref StringBuilder s)
        {
            var tehosteet = FindOrCreateContainer(map, "Tehosteet", ref s);
            CreateBoost(tehosteet, "Boost_01", new Vector3(3f, 0.05f, -135f), ref s);      // alkusuora, roadX=0
            CreateBoost(tehosteet, "Boost_02", new Vector3(10.58f, 0.05f, -65f), ref s);   // 1. kaarre, roadX=13.58 (sisakaarre)
            CreateBoost(tehosteet, "Boost_03", new Vector3(-10.58f, 0.05f, 65f), ref s);   // 2. kaarre, roadX=-13.58 (sisakaarre)
            CreateBoost(tehosteet, "Boost_04", new Vector3(3f, 0.05f, 135f), ref s);       // loppusuora, roadX=0

            var esteet = FindOrCreateContainer(map, "Esteet", ref s);
            CreateCone(esteet, "Kartio_01", new Vector3(5.64f, 0.4f, -100f), ref s);        // roadX=1.64, ulkoreuna
            CreateCone(esteet, "Kartio_02", new Vector3(-3f, 0.4f, 0f), ref s);             // keskisuora, roadX=0
            CreateCone(esteet, "Kartio_03", new Vector3(-5.64f, 0.4f, 100f), ref s);        // roadX=-1.64, ulkoreuna
            CreatePuddle(esteet, "Latakko_01", new Vector3(3.78f, 0.03f, -40f), ref s);     // roadX=5.78, sisareuna
            CreatePuddle(esteet, "Latakko_02", new Vector3(-3.78f, 0.03f, 40f), ref s);     // roadX=-5.78, sisareuna
        }

        // ---------- Apufunktiot ----------

        // Boost: ohut vihrea laatta (Cube, madallettu). Trigger-collider, Boost-komponentti.
        static void CreateBoost(Transform container, string name, Vector3 localPos, ref StringBuilder s)
        {
            var existing = FindChild(container, name);
            if (existing != null) { existing.localPosition = localPos; s.AppendLine($"{name}: paivitetty sijainti."); return; }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(container, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(1.8f, 0.08f, 1.8f);
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);

            var col = go.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Boost>() == null) go.AddComponent<Boost>();

            s.AppendLine($"{name}: luotu (Cube-laatta + Boost - vaihda materiaali Inspectorista " +
                "nayttavammaksi, esim. vihrea/nuolikuvio).");
        }

        // Kartio: pysty, ohut sylinteri (paikallaan seisova este). Ei trigger - fyysinen
        // tormays (OnCollisionEnter Cone.cs:ssa), tuntuu selvemmin "esteelta".
        static void CreateCone(Transform container, string name, Vector3 localPos, ref StringBuilder s)
        {
            var existing = FindChild(container, name);
            if (existing != null) { existing.localPosition = localPos; s.AppendLine($"{name}: paivitetty sijainti."); return; }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(container, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);

            if (go.GetComponent<Cone>() == null) go.AddComponent<Cone>();

            s.AppendLine($"{name}: luotu (Cylinder-tolppa + Cone - vaihda materiaali/mesh Inspectorista " +
                "oikean liikennekartion nakoiseksi jos haluat).");
        }

        // Latakko: matala, leveä sylinteri (lammikko). Trigger-collider, Puddle-komponentti.
        static void CreatePuddle(Transform container, string name, Vector3 localPos, ref StringBuilder s)
        {
            var existing = FindChild(container, name);
            if (existing != null) { existing.localPosition = localPos; s.AppendLine($"{name}: paivitetty sijainti."); return; }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(container, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(2.2f, 0.02f, 2.2f);
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);

            var col = go.GetComponent<CapsuleCollider>();
            // CreatePrimitive(Cylinder) antaa CapsuleColliderin - Puddle vaatii vain jonkin
            // Colliderin (RequireComponent(Collider)), joten CapsuleCollider kelpaa sellaisenaan.
            if (col != null) col.isTrigger = true;
            if (go.GetComponent<Puddle>() == null) go.AddComponent<Puddle>();

            s.AppendLine($"{name}: luotu (Cylinder-lammikko + Puddle - vaihda materiaali Inspectorista " +
                "tummansiniseksi/ruskeaksi lataakon nakoiseksi jos haluat).");
        }

        // Loytaa tai luo "Map":n lapsikontainerin (esim. "Kuplat") - sama kaava kuin
        // olemassa olevilla Tehosteet/Esteet-kansioilla (local position nolla).
        static Transform FindOrCreateContainer(Transform map, string name, ref StringBuilder s)
        {
            var existing = FindChild(map, name);
            if (existing != null) return existing;

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);
            go.transform.SetParent(map, false);
            go.transform.localPosition = Vector3.zero;
            s.AppendLine($"{name}-kansio luotu Mapin alle.");
            return go.transform;
        }

        // Kloonaa olemassa olevan esteen (sama scene) uuteen nimeen/sijaintiin - taataan
        // etta mesh/materiaali/collider/komponentit pysyvat identtisina alkuperaiseen,
        // ilman etta niita tarvitsee arvata tai kirjoittaa kasin.
        static void CloneObstacle(Transform container, string sourceName, string newName, Vector3 localPos, ref StringBuilder s)
        {
            var existing = FindChild(container, newName);
            if (existing != null)
            {
                existing.localPosition = localPos;
                s.AppendLine($"{newName}: paivitetty sijainti.");
                return;
            }
            var source = FindChild(container, sourceName);
            if (source == null)
            {
                s.AppendLine($"VAROITUS: lahdetta '{sourceName}' ei loytynyt - '{newName}' ohitettu.");
                return;
            }
            var clone = Object.Instantiate(source.gameObject, container);
            clone.name = newName;
            clone.transform.localPosition = localPos;
            clone.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(clone, "Luo " + newName);
            s.AppendLine($"{newName}: luotu (kloonattu '{sourceName}':sta).");
        }

        // LISATTY 14.8.2026 (kayttaja: "poistetaan kaikilta sceneilta kaytosta, vain tahdet
        // keraittavissa") - poistaa "Kuplat"-kontainerin ja kaikki sen lapset (Kupla_01..N)
        // jos niita loytyy jo avatusta scenesta. Ajetaan aina AddTrackObstacles():n alussa.
        static void RemoveExistingBubbles(Transform map, ref StringBuilder s)
        {
            var kuplat = FindChild(map, "Kuplat");
            if (kuplat == null) return;
            int removed = kuplat.childCount;
            Undo.DestroyObjectImmediate(kuplat.gameObject);
            s.AppendLine($"Poistettu 'Kuplat'-kansio ja {removed} Kupla-objektia (kaytosta poistettu 14.8.2026).");
        }

        static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform c in parent)
                if (c.name == name) return c;
            return null;
        }
    }
}
#endif
