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
    // AJA se ratascene AUKI jota haluat taydentaa (Puisto/Satama/Huvipuisto - katso alla
    // miksi juuri nama kolme), ja suorita tama tyokalu. Tallenna scene lopuksi (Ctrl+S).
    //
    // MIKSI VAIN Puisto/Satama/Huvipuisto TASSA ERASSA (EI Game/Game2/Maatila):
    // Nailla kolmella radalla oli jo VALMIIKSI oikein sijoitettuja Boost/Kartio/Latakko-
    // esteita (Tehosteet/Esteet-kansioiden alla, "Map"-objektin lapsina) - naiden todellisia
    // maailma-koordinaatteja kayttaen uudet lisaykset voidaan sijoittaa LUOTETTAVASTI radan
    // varrelle (kloonataan olemassa oleva este ja siirretaan hieman sivuun/valiin). Game.unity,
    // Game2.unity ja Maatila.unity eivat sisaltaneet YHTAAN valmista estetta - ilman mitaan
    // koordinaattireferenssia radan muodosta uusien esteiden sijoittelu olisi arvausta ja
    // saattaisi paatya ilmaan tai radan ulkopuolelle. Nailla kolmella tehdaan oma erillinen
    // kierros myohemmin (esim. Scene-nakymasta otetun kuvakaappauksen tai manuaalisesti
    // podetettujen koordinaattien avulla).
    //
    // TrackBubble (Gameplay/TrackBubble.cs, UUSI M5.16): pallo (Sphere-primitiivi, oletus-
    // materiaali - vaihda halutessasi soapy/lapikuultava materiaali Inspectorista), Is Trigger
    // paalla, TrackBubble-komponentti (poksahtaa + antaa tahden, RaceManager.AddStar()) ja
    // Spinner-komponentti (pyorii + kelluu pehmeasti, sama komponentti kuin Tahti-keraily).
    public static class PakettiporinaTrackObstaclesSetup
    {
        [MenuItem("Pakettiporina/Lisays/10 - Lisaa esteita, tehosteita ja kuplia avattuun rataan")]
        public static void AddTrackObstacles()
        {
            var s = new StringBuilder();
            string scene = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== ESTEET/TEHOSTEET/KUPLAT ({scene}) ===");

            var mapGo = GameObject.Find("Map");
            if (mapGo == null)
            {
                s.AppendLine("VIRHE: scenesta ei loydy 'Map'-nimista objektia - avaa oikea ratascene.");
                Debug.LogError(s.ToString());
                return;
            }
            var map = mapGo.transform;

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
                case "Game":
                case "Game2":
                case "Maatila":
                    s.AppendLine($"'{scene}':lla ei ole viela yhtaan valmista estetta jonka " +
                        "sijaintia voisi kayttaa referenssina - tama tyokalu EI lisaa mitaan " +
                        "tahan sceneen (ks. luokkakommentti). Sijoitellaan kasin tai omalla erallaan.");
                    break;
                default:
                    s.AppendLine($"Tuntematon scene '{scene}' - tama tyokalu tukee Puisto/Satama/Huvipuisto-ratoja.");
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

            var kuplat = FindOrCreateContainer(map, "Kuplat", ref s);
            CreateBubble(kuplat, "Kupla_01", new Vector3(4f, 0.5f, -120f), ref s);
            CreateBubble(kuplat, "Kupla_02", new Vector3(-4f, 0.5f, 0f), ref s);
            CreateBubble(kuplat, "Kupla_03", new Vector3(4f, 0.5f, 120f), ref s);
        }

        // ---------- Satama: 7 Kartiota + 1 Latakko valmiina (Z -130..125, X 5.5..28.5) ----------
        static void AddSatama(Transform map, ref StringBuilder s)
        {
            var esteet = FindOrCreateContainer(map, "Esteet", ref s);
            CloneObstacle(esteet, "Kartio_01", "Kartio_08", new Vector3(10f, 0.4f, -20f), ref s);
            CloneObstacle(esteet, "Kartio_01", "Kartio_09", new Vector3(15f, 0.4f, 90f), ref s);
            CloneObstacle(esteet, "Latakko_01", "Latakko_02", new Vector3(20f, 0.03f, -100f), ref s);

            var kuplat = FindOrCreateContainer(map, "Kuplat", ref s);
            CreateBubble(kuplat, "Kupla_01", new Vector3(25f, 0.5f, -65f), ref s);
            CreateBubble(kuplat, "Kupla_02", new Vector3(10f, 0.5f, 25f), ref s);
            CreateBubble(kuplat, "Kupla_03", new Vector3(18f, 0.5f, 117f), ref s);
        }

        // ---------- Huvipuisto: jo 8 Boost + 4 Kartio + 4 Latakko - lisataan vain kuplat ----------
        static void AddHuvipuisto(Transform map, ref StringBuilder s)
        {
            var kuplat = FindOrCreateContainer(map, "Kuplat", ref s);
            CreateBubble(kuplat, "Kupla_01", new Vector3(-8f, 0.5f, -140f), ref s);
            CreateBubble(kuplat, "Kupla_02", new Vector3(12f, 0.5f, -40f), ref s);
            CreateBubble(kuplat, "Kupla_03", new Vector3(-12f, 0.5f, 40f), ref s);
            CreateBubble(kuplat, "Kupla_04", new Vector3(5f, 0.5f, 130f), ref s);
        }

        // ---------- Apufunktiot ----------

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

        // Rakentaa uuden TrackBubblen tyhjasta (Sphere-primitiivi - ei tarvitse ulkoista
        // mesh-referenssia, joten talle ei ole olemassa olevaa objektia kloonattavaksi).
        static void CreateBubble(Transform container, string name, Vector3 localPos, ref StringBuilder s)
        {
            var existing = FindChild(container, name);
            if (existing != null)
            {
                existing.localPosition = localPos;
                s.AppendLine($"{name}: paivitetty sijainti.");
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(container, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 1.1f;
            Undo.RegisterCreatedObjectUndo(go, "Luo " + name);

            var col = go.GetComponent<SphereCollider>();
            if (col != null) col.isTrigger = true;

            if (go.GetComponent<TrackBubble>() == null) go.AddComponent<TrackBubble>();

            var spin = go.GetComponent<Spinner>();
            if (spin == null) spin = go.AddComponent<Spinner>();
            spin.spinSpeed = 40f;
            spin.bobHeight = 0.3f;
            spin.bobSpeed = 1.5f;

            s.AppendLine($"{name}: luotu (Sphere + TrackBubble + Spinner - vaihda materiaali " +
                "Inspectorista jos haluat lapikuultavamman saippuakupla-ilmeen).");
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
