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

            // LISATTY 18.8.2026 (jatko 5): jos Boost/Latakko-objektin Layer ei ole mukana
            // Main Cameran Culling Maskissa, objekti EI PIIRRY RUUDULLE VAIKKA KAIKKI MUU
            // OLISI OIKEIN (sijainti, aktiivisuus, Renderer paalla) - taysin nakymaton bugi
            // joka ei nay Scene-nakymassa jos Scene-nakymassa on "Always Refresh"/eri asetus.
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                foreach (var c in Object.FindObjectsOfType<Camera>(true)) { mainCam = c; break; }
            }
            if (mainCam != null)
            {
                var included = new List<string>();
                for (int li = 0; li < 32; li++)
                {
                    if ((mainCam.cullingMask & (1 << li)) != 0)
                    {
                        string ln = LayerMask.LayerToName(li);
                        if (!string.IsNullOrEmpty(ln)) included.Add(ln);
                    }
                }
                s.AppendLine($"Kamera: {mainCam.name}, CullingMask sisaltaa: {string.Join(", ", included)}");
            }
            else
            {
                s.AppendLine("(Kameraa ei loytynyt scenesta.)");
            }
            // LISATTY 18.8.2026 (jatko 6): kayttaja vahvisti etta Boost/Latakko-objektit ovat
            // Aktiivisia, RendererPaalla, oikealla Layerilla, materiaali+shader OK JA
            // TORMAYS/AANI TOIMII PELATESSA - mutta MITAAN EI PIIRRY RUUDULLE. Kaikki muu on
            // nyt poissuljettu paitsi yksi asia: onko objekti fyysisesti HAUDATTUNA maaston
            // PINNAN SISALLA. Boost (Y=0.05) ja Latakko (Y=0.03) ovat ainoat kentan objektit
            // jotka istutetaan LITTEANA TASOLLA aivan "Y=0"-tason paalle SIINA OLETUKSESSA
            // etta tien pinta on tasan Y=0 - mutta GroundCollider (fysiikka) on ERILLINEN,
            // yksinkertainen laatikko joka EI valttamatta vastaa Maaston OMAN mesh-pinnan
            // korkeutta (esim. jos tiessa on pieni reunakoroke/kupu tyylisyista, GroundCollider
            // pysyy silti tasaisena Y=0:ssa - auto ajaa sen mukaan, mutta VISUAALINEN Maasto
            // saattaa nayttaa pinnan hieman ylempana, jolloin Boost/Latakko jaisivat NAKYMATTA
            // pinnan ALLE vaikka fysiikka/tormays toimisi normaalisti GroundColliderin kautta).
            // Kartio/Laatikko (Y=0.4, paljon korkeampi) eivat koskaan olleet kayttajan
            // valituksen kohteena tassa keskustelussa - vain matalat Boost/Latakko ovat.
            // Testataan tama suoraan: lisataan Maastoon VALIAIKAINEN MeshCollider (poistetaan
            // lopuksi jos ei ollut alunperin), rykaistaan pystysuoraan alaspain jokaisen
            // Boost_/Latakko_/Kartio_/Laatikko_-objektin kohdalta ja verrataan osuman
            // korkeutta objektin omaan Y-sijaintiin.
            // KORJATTU 18.8.2026 (jatko 6, valittomasti): GetRootGameObjects() loytaa VAIN
            // scenen ylimman tason objektit - jos Maasto (esim. "Puisto") on jonkun toisen
            // objektin lapsi (todistettu kayttajan dumpista: "Puisto"-rivi loytyi kylla
            // paadumpista FindObjectsOfType(true):lla, mutta ei taalta), haku ei koskaan
            // loytanyt sita ja korkeustesti jai aina pois paalta. Etsitaan siis KAIKISTA
            // objekteista (myos sisakkaisista), sama tapa kuin paadumpin all-listalla.
            GameObject maastoGo = null;
            if (preset != null)
            {
                foreach (var tr in Object.FindObjectsOfType<Transform>(true))
                    if (tr.name == preset.maastoName) { maastoGo = tr.gameObject; break; }
            }
            MeshCollider tempMaastoCollider = null;
            bool maastoHadColliderAlready = false;
            if (maastoGo != null)
            {
                var existing = maastoGo.GetComponent<Collider>();
                maastoHadColliderAlready = existing != null;
                if (!maastoHadColliderAlready)
                    tempMaastoCollider = maastoGo.AddComponent<MeshCollider>();
                s.AppendLine($"(Maasto-korkeustesti: '{preset.maastoName}' loytyi, " +
                              (maastoHadColliderAlready ? "silla oli jo Collider - kaytetaan sita." : "lisattiin valiaikainen MeshCollider testia varten.") + ")");
            }
            else if (preset != null)
            {
                s.AppendLine($"(Maasto-korkeustesti: objektia '{preset.maastoName}' ei loytynyt scenen juuresta - korkeusvertailu ei ole kaytettavissa.)");
            }
            s.AppendLine();
            // LISATTY 8.8.2026: "MalliMitat" oli laskettu mf.sharedMesh.bounds:sta - se on
            // MESHIN OMA paikallinen bounding box, EI ota huomioon transformin kiertoa lainkaan.
            // Se ei siis koskaan voinut kertoa seisooko objekti oikein pystyssa vai kyljellaan -
            // pelkka Scale kerrottiin paikalliseen kokoon, rotaatio jai kokonaan huomiotta.
            // Lisatty "MaailmaBounds" (Renderer.bounds, MAAILMAN-avaruudessa, huomioi kierron
            // taysin) rinnalle - jos objektin oikea korkeus on esim. 4.2 ja MaailmaBounds.y on
            // lahella sita, objekti seisoo oikein; jos korkeus nayttaytyy X:ssa tai Z:ssa
            // MaailmaBounds.y:n sijaan (ja y on pieni), objekti makaa kyljellaan -> rotX vaarin.
            // LISATTY 18.8.2026 (jatko 5): X/Z-sijainnit on nyt todistettu oikeiksi Puistossa
            // (EtaisyysKeskilinjasta=0.00 kaikilla Boost/Latakko-objekteilla Lisays/02:n jalkeen),
            // mutta kayttaja raportoi etta ne EIVAT silti nay pelatessa. Vanha dumppi ei voinut
            // KOSKAAN paljastaa tata, koska FindObjectsOfType(true) listaa myos POIS PAALTA
            // kytketyt objektit taysin samoilla luvuilla kuin paalla olevat - sijainti nayttaa
            // "oikealta" vaikka objekti olisi nakymaton. Lisatty nakyvyyteen suoraan vaikuttavat
            // sarakkeet: onko GameObject/vanhemmat aktiivisia, onko Renderer paalla, mika Layer
            // (voi olla FBX-prefabiin leivottu ja Kameran Culling Mask voi puuttua sen), ja onko
            // materiaali edes asetettu.
            s.AppendLine("Nimi\tX\tY\tZ\tRotY\tScaleX\tScaleY\tScaleZ\tMalliMitat(realX,realY,realZ)\tMaailmaBounds(x,y,z)\tRoadX(z)\tEtaisyysKeskilinjasta\tAktiivinen\tRendererPaalla\tLayer\tMateriaali\tShaderNimi\tMaastonPintaTassa(Y)\tPystyEroMaastoon");

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

                // UUDET SARAKKEET (18.8.2026, jatko 5) - nakyvyysdiagnoosi:
                // activeInHierarchy huomioi myos vanhempien pois-paalta-kytkennan (esim. jos
                // koko "Tehosteet"-kansio olisi jostain syysta pois paalta, taalla nakyisi false
                // vaikka Boost_XX itse olisi paalla). renderers[] kerattiin jo ylla worldDims:ia
                // varten - kaytetaan samaa listaa, ei lasketa kahdesti.
                bool active = t.gameObject.activeInHierarchy;
                bool anyRendererOn = false;
                string matNames = "-";
                // LISATTY 18.8.2026 (jatko 6): kayttaja vahvisti etta Boost/Latakko OVAT
                // aktiivisia, Rendererit paalla, oikealla Layerilla JA tormays/aani toimii -
                // mutta MITAAN EI PIIRRY RUUDULLE. Ainoa jaljelle jaava selitys on itse
                // shader/materiaali - Boost/Latakko/Kartio/Laatikko ovat AINOAT scenen
                // objektit joiden materiaali luodaan ajonaikaisesti koodilla (new Material(
                // Shader.Find(...)) PakettiporinaElements.cs:ssa) sen sijaan etta se tulisi
                // FBX-tuonnin mukana (kuten KAIKKI muut nakyvat objektit: puut, penkit,
                // tahdet, auto). Jos Shader.Find epaonnistuu tai palauttaa null-shaderin,
                // sharedMaterial ei ole null (nimi nakyy) mutta EI PIIRRY MITAAN - vanha
                // dumppi ei voinut koskaan paljastaa tata, koska se tarkisti vain materiaalin
                // NIMEN, ei sen shaderia. Lisatty ShaderNimi-sarake suoraan tata varten.
                string shaderNames = "-";
                if (renderers.Length > 0)
                {
                    var names = new List<string>();
                    var shaders = new List<string>();
                    foreach (var r in renderers)
                    {
                        if (r.enabled) anyRendererOn = true;
                        if (r.sharedMaterial != null)
                        {
                            names.Add(r.sharedMaterial.name);
                            shaders.Add(r.sharedMaterial.shader != null ? r.sharedMaterial.shader.name : "SHADER PUUTTUU");
                        }
                        else
                        {
                            names.Add("PUUTTUU");
                            shaders.Add("-");
                        }
                    }
                    matNames = string.Join("+", names);
                    shaderNames = string.Join("+", shaders);
                }
                string layerName = LayerMask.LayerToName(t.gameObject.layer);
                if (string.IsNullOrEmpty(layerName)) layerName = $"(nimeton #{t.gameObject.layer})";

                // Maasto-korkeustesti vain radan omille pelielementeille (Boost/Latakko/
                // Kartio/Laatikko) - muille rivi jaisi merkityksettomaksi (esim. Star_XX
                // roikkuu ilmassa tarkoituksella, puut/penkit eivat ole tien paalla).
                string groundY = "-";
                string vertGap = "-";
                bool isFieldElement = t.name.StartsWith("Boost_") || t.name.StartsWith("Latakko_") ||
                                      t.name.StartsWith("Kartio_") || t.name.StartsWith("Laatikko_");
                if (isFieldElement && maastoGo != null)
                {
                    var col = maastoGo.GetComponent<Collider>();
                    if (col != null)
                    {
                        var rayStart = new Vector3(t.position.x, 200f, t.position.z);
                        if (col.Raycast(new Ray(rayStart, Vector3.down), out var hit, 500f))
                        {
                            groundY = hit.point.y.ToString("F3");
                            vertGap = (t.position.y - hit.point.y).ToString("F3");
                        }
                        else
                        {
                            groundY = "(ei osumaa)";
                        }
                    }
                }

                s.AppendLine($"{t.name}\t{t.position.x:F2}\t{t.position.y:F2}\t{t.position.z:F2}\t{t.eulerAngles.y:F0}\t{t.localScale.x:F2}\t{t.localScale.y:F2}\t{t.localScale.z:F2}\t{meshDims}\t{worldDims}\t{roadXHere:F2}\t{dist:F2}\t{active}\t{anyRendererOn}\t{layerName}\t{matNames}\t{shaderNames}\t{groundY}\t{vertGap}");
            }

            // Siivotaan valiaikainen MeshCollider pois jos se lisattiin vain tata testia varten -
            // ei jateta ylimaaraisia komponentteja Maastoon pysyvasti.
            if (tempMaastoCollider != null)
                Object.DestroyImmediate(tempMaastoCollider);

            if (preset == null)
                s.AppendLine("\n(HUOM: scenelle ei loytynyt tunnettua ratapresettia - RoadX/etaisyys-sarakkeet eivat ole luotettavia.)");

            Debug.Log(s.ToString());
        }
    }
}
#endif
