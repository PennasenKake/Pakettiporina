#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Vie KAIKKI scenen oleelliset objektit (nimi, sijainti, kierto, koko, mallin
    // todellinen mitta) yhtena taulukkona Consoliin - kopioi tuloste ja liita chattiin
    // niin sijaintien/kokojen tarkistus/korjaus onnistuu ilman kuvakaappauksia.
    //
    // Kaytto: Pakettiporina -> 15 - Vie scenen elementit (konsoli)
    // Ratadata (bends/baseX) tulee PakettiporinaTracks.cs:sta.
    public static class PakettiporinaSceneDump
    {
        [MenuItem("Pakettiporina/Tyokalut/03 - Vie scenen elementit (konsoli)")]
        public static void DumpScene()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var preset = PakettiporinaTracks.Find(sceneName);

            s.AppendLine($"=== SCENEN ELEMENTIT: {sceneName} ===");
            s.AppendLine("(Kopioi taman koko tuloste ja liita chattiin - siina on kaikkien objektien nimi, sijainti, kierto, koko, mallin todellinen mitta ja etaisyys tien keskilinjasta)");
            s.AppendLine();
            // LISATTY 8.8.2026: "MalliMitat" oli laskettu mf.sharedMesh.bounds:sta - se on
            // MESHIN OMA paikallinen bounding box, EI ota huomioon transformin kiertoa lainkaan.
            // Se ei siis koskaan voinut kertoa seisooko objekti oikein pystyssa vai kyljellaan -
            // pelkka Scale kerrottiin paikalliseen kokoon, rotaatio jai kokonaan huomiotta.
            // Lisatty "MaailmaBounds" (Renderer.bounds, MAAILMAN-avaruudessa, huomioi kierron
            // taysin) rinnalle - jos objektin oikea korkeus on esim. 4.2 ja MaailmaBounds.y on
            // lahella sita, objekti seisoo oikein; jos korkeus nayttaytyy X:ssa tai Z:ssa
            // MaailmaBounds.y:n sijaan (ja y on pieni), objekti makaa kyljellaan -> rotX vaarin.
            s.AppendLine("Nimi\tX\tY\tZ\tRotY\tScaleX\tScaleY\tScaleZ\tMalliMitat(realX,realY,realZ)\tMaailmaBounds(x,y,z)\tRoadX(z)\tEtaisyysKeskilinjasta");

            // Kerataan kaikki Transformit, jarjestetaan Z:n mukaan jotta rata etenee jarkevassa jarjestyksessa.
            var all = new List<Transform>(Object.FindObjectsOfType<Transform>(true));
            all.Sort((a, b) => a.position.z.CompareTo(b.position.z));

            foreach (var t in all)
            {
                // Ohitetaan Canvas/UI-objektit ja kamera/valo - ei oleellisia radan sijoittelulle.
                if (t.GetComponentInParent<Canvas>() != null) continue;
                if (t.GetComponent<Camera>() != null) continue;
                if (t.GetComponent<Light>() != null) continue;
                // Ohitetaan puhtaat "kansio"-objektit joilla ei ole omaa mesh/komponenttia eika
                // fysiikkaa - nayttaisivat vain tyhjina riveina (esim. "Map"-parent itse).
                bool hasMesh = t.GetComponent<MeshFilter>() != null;
                bool hasCollider = t.GetComponent<Collider>() != null;
                bool isNamedPoint = t.name == "StartPoint" || t.name == "Finish" || t.name.StartsWith("FinishTrigger");
                if (!hasMesh && !hasCollider && !isNamedPoint) continue;

                var mf = t.GetComponent<MeshFilter>();
                string meshDims = "-";
                if (mf != null && mf.sharedMesh != null)
                {
                    var b = mf.sharedMesh.bounds.size;
                    meshDims = $"({b.x * t.lossyScale.x:F2},{b.y * t.lossyScale.y:F2},{b.z * t.lossyScale.z:F2})";
                }

                // MaailmaBounds: kerataan KAIKISTA renderoivista lapsista (esim. FBX-prefabin
                // juuri itsessaan ei valttamatta ole Rendereria, se on lapsiobjektissa) ja
                // yhdistetaan yhdeksi maailma-avaruuden AABB:ksi - taman koko kiertyy oikein
                // transformin rotaation mukana, toisin kuin sharedMesh.bounds ylla.
                string worldDims = "-";
                var renderers = t.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    var wb = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) wb.Encapsulate(renderers[i].bounds);
                    worldDims = $"({wb.size.x:F2},{wb.size.y:F2},{wb.size.z:F2})";
                }

                float roadXHere = preset != null ? PakettiporinaTracks.RoadX(t.position.z, preset) : 0f;
                float dist = t.position.x - roadXHere;

                s.AppendLine($"{t.name}\t{t.position.x:F2}\t{t.position.y:F2}\t{t.position.z:F2}\t{t.eulerAngles.y:F0}\t{t.localScale.x:F2}\t{t.localScale.y:F2}\t{t.localScale.z:F2}\t{meshDims}\t{worldDims}\t{roadXHere:F2}\t{dist:F2}");
            }

            if (preset == null)
                s.AppendLine("\n(HUOM: scenelle ei loytynyt tunnettua ratapresettia - RoadX/etaisyys-sarakkeet eivat ole luotettavia.)");

            Debug.Log(s.ToString());
        }
    }
}
#endif
