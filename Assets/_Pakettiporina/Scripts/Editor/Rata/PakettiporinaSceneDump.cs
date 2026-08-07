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
        [MenuItem("Pakettiporina/15 - Vie scenen elementit (konsoli)")]
        public static void DumpScene()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var preset = PakettiporinaTracks.Find(sceneName);

            s.AppendLine($"=== SCENEN ELEMENTIT: {sceneName} ===");
            s.AppendLine("(Kopioi taman koko tuloste ja liita chattiin - siina on kaikkien objektien nimi, sijainti, kierto, koko, mallin todellinen mitta ja etaisyys tien keskilinjasta)");
            s.AppendLine();
            s.AppendLine("Nimi\tX\tY\tZ\tRotY\tScaleX\tScaleY\tScaleZ\tMalliMitat(realX,realY,realZ)\tRoadX(z)\tEtaisyysKeskilinjasta");

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

                float roadXHere = preset != null ? PakettiporinaTracks.RoadX(t.position.z, preset) : 0f;
                float dist = t.position.x - roadXHere;

                s.AppendLine($"{t.name}\t{t.position.x:F2}\t{t.position.y:F2}\t{t.position.z:F2}\t{t.eulerAngles.y:F0}\t{t.localScale.x:F2}\t{t.localScale.y:F2}\t{t.localScale.z:F2}\t{meshDims}\t{roadXHere:F2}\t{dist:F2}");
            }

            if (preset == null)
                s.AppendLine("\n(HUOM: scenelle ei loytynyt tunnettua ratapresettia - RoadX/etaisyys-sarakkeet eivat ole luotettavia.)");

            Debug.Log(s.ToString());
        }
    }
}
#endif
