#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Lisaa teemakoristeet (FBX-mallit, ei pelkkia primitiiveja) sen radan mukaan
    // mika scene on parhaillaan auki. Nama ovat PUHTAASTI VISUAALISIA - ei Collideria,
    // ei vaikutusta ajoon. Tarkoitus tehda jokaisesta radasta silmailla erottuva
    // omalla teemallaan sen sijaan etta ne nayttaisivat samalta vain eri varilla.
    //
    // Kaytto: Pakettiporina -> 12 - Lisaa teemakoristeet talle radalle
    // HUOM: FBX-mallit pitaa olla Unityn jo tuomia (AssetDatabase paivittynyt) ennen
    // ajoa - jos juuri loit ne Blenderista, odota etta Unity tuo ne automaattisesti
    // (tapahtuu kun Editor-ikkuna saa fokuksen) tai paina Assets -> Refresh kasin.
    //
    // Sijainnit on tarkistettu radan road_x(z)-kaavaa vasten (sama kaava kuin
    // PakettiporinaElements.cs:ssa) niin etta jokainen koriste on vahintaan ~8-12
    // yksikkoa tien keskilinjasta (tie on 12 yksikkoa levea) JA sisalla kentan
    // 71-levyisella alueella (+-35.5). Jos siirrat naita Hierarkiassa kasin, pida
    // molemmat rajat mielessa etteivat koristeet putoa tielle tai kentan reunan yli.
    //
    // Sataman maasto (Satama.fbx) generoitiin uudelleen niin etta meri seuraa tien
    // kaarretta jatkuvasti tien VASEMMALLA puolella (negatiivinen x roadX(z):sta) -
    // siksi laituri/laiva/majakka/poijut ovat negatiivisella puolella ja nosturi/
    // varasto/kontit positiivisella (maapuoli, lastausalue).
    // Merkkikomponentti jokaisessa AddDecor():n luomassa koristeessa. Talla loydetaan
    // ja siivotaan KAIKKI vanhat koristeet (myos ne joiden nimi/sijainti on peraisin
    // AIEMMASTA koodiversiosta eika enaa esiinny nykyisessa PRESETS-listassa) ennen
    // uudelleensijoitusta - muuten esim. suihkulahteen/kukkapenkkien vanhat, jo
    // korvatut kopiot jaisivat scenneen nakymattomina paallekkain uusien kanssa
    // (SceneDump ei naytä niita koska se lukee vain nykyista PRESETS-listaa).
    public class PakettiporinaDecorMarker : MonoBehaviour
    {
        // FBX-tiedoston nimi (ilman paatetta) josta tama koriste on luotu - talteen
        // jotta "16 - Vie sijainnit koodiksi" -tyokalu voi tunnistaa oikean mallin
        // vaikka instanceName poikkeaisi siita (esim. Puisto_Kukkapenkki_A).
        public string modelName;
    }

    public static class PakettiporinaDecor
    {
        const string MODELS_DIR = "Assets/_Pakettiporina/Art/Models";

        class DecorItem
        {
            public string modelName;     // FBX-tiedoston nimi ilman paatetta
            public string instanceName;  // scenen GameObjectin nimi - jos tyhja, kaytetaan modelName (mahdollistaa saman mallin useita kopioita)
            public Vector3 pos;
            public float rotX;           // korjauskierto (esim. veneet, joiden FBX-mallin oma
                                          // "ylos"-suunta ei osu Unityn Y-akselille) - useimmilla 0
            public float rotY;
            public float scale = 1f;     // koristeen kokokerroin - isompi = naeyttavampi/helpompi huomata ajaessa
        }

        class DecorPreset
        {
            public string sceneName;
            public DecorItem[] items;
        }

        static readonly DecorPreset[] PRESETS =
        {
            new DecorPreset
            {
                sceneName = "Puisto",
                items = new[]
                {
                    // Keinu UUDISTETTU 7.8.2026: oikea A-kehikko (vinot jalat + tukipalkki,
                    // entinen malli oli vain 2 pystytolppaa) + 3 keinua (ei 2), kirkkaat
                    // vaihtelevat istuinvarit (puna/kelta/sininen) - selvasti nayttavampi.
                    // Uusi raakamalli isompi (2.34 korkea, oli 0.9) - scale pienennetty
                    // 1.6 -> 1.4 jotta lopputulos ei ole ylisuuri (tarkistettu ettei
                    // ulotu piennarelle: etaisyys tiesta jaa n. 2.5 yksikkoa marginaalia).
                    new DecorItem { modelName = "Puisto_Keinu", pos = new Vector3(17f, 0f, -40f), rotY = 0f, scale = 1.4f },
                    // Silta on nyt oikea kaarisilta (uusi Blender-malli) - siirretty
                    // lammen (Puisto_Lampi) paalle, selvasti kauemmas tiesta kuin ennen.
                    new DecorItem { modelName = "Puisto_Silta", pos = new Vector3(-30f, 0f, 70f), rotY = 90f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lampi", pos = new Vector3(-30f, 0f, 70f), rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Puisto_Penkki", pos = new Vector3(15f, 0f, -34f), rotY = 180f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", pos = new Vector3(16f, 0f, -100f), rotY = 0f, scale = 1.5f },

                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_A", pos = new Vector3(14f, 0f, -110f), rotY = 0f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_B", pos = new Vector3(-15f, 0f, -20f), rotY = 40f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_C", pos = new Vector3(14f, 0f, 45f), rotY = 80f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_D", pos = new Vector3(6f, 0f, 95f), rotY = 160f, scale = 1.8f },
                    // Suihkulahde UUDISTETTU: monikerroksinen (allas+jalusta+allas+jalusta+
                    // huippumalja+kultasuutin), 5.2 leveä x 3.45 korkea raakamitta (oli
                    // 1.8x1.65) - selvasti isompi ja naeyttavampi. Scale 1.2 -> maailmassa
                    // ~6.2 leveä, ~4.1 korkea. Ympärille 4 kukkapenkkia sateen muotoon
                    // (referenssikuvien muodollinen ruusutarha-asettelu).
                    new DecorItem { modelName = "Puisto_Suihkulahde", pos = new Vector3(20f, 0f, 0f), rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_A", pos = new Vector3(20f, 0f, 0f), rotY = 0f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_B", pos = new Vector3(20f, 0f, 0f), rotY = 90f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_C", pos = new Vector3(20f, 0f, 0f), rotY = 180f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_D", pos = new Vector3(20f, 0f, 0f), rotY = 270f, scale = 2.2f },

                    // --- Lisää puiston elementteja: pensaita/topiareja siisteina laikkuina
                    // ympari puistoa, toinen penkki suihkulahteen aareen, toinen lyhtypylvas. ---
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_A", pos = new Vector3(22f, 0f, -8f), rotY = 0f, scale = 1.1f },
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_B", pos = new Vector3(18f, 0f, 8f), rotY = 0f, scale = 1.1f },
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_C", pos = new Vector3(25f, 0f, -95f), rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_D", pos = new Vector3(-25f, 0f, 55f), rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Penkki", instanceName = "Puisto_Penkki_B", pos = new Vector3(20f, 0f, 9f), rotY = 200f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", instanceName = "Puisto_Lyhtypylvas_B", pos = new Vector3(-18f, 0f, 20f), rotY = 0f, scale = 1.5f },

                    // HUOM 7.8.2026: Y=2 on TARKOITUKSELLINEN (liukumaen malli on pivotoitu
                    // yläalustan tasolta, ei maasta - Y=0 upottaisi sen maahan). Tama unohtui
                    // kun sijainti vietiin koodiin "16"-tyokalulla edellisella kerralla (Y
                    // pudotettiin vahingossa nollaan) - korjattu myos vientityokalu alla.
                    new DecorItem { modelName = "Puisto_Liukumaki", pos = new Vector3(-12f, 2f, -90f), rotY = 0f, scale = 1.6f },
                },
            },
            new DecorPreset
            {
                sceneName = "Satama",
                // SATAMA v2 - KOKO RATA JA ASETTELU UUDELLEENSUUNNITELTU. Tie on nyt
                // EPASYMMETRINEN: iso loiva mutka oikealle z=-100..0 (huippu roadX=25
                // kohdassa z=-50), sen jalkeen loivempi mutka vasemmalle z=0..100 (pohja
                // roadX=9 kohdassa z=50) - ei enaa yksi symmetrinen kupera lahti vaan
                // vaihteleva rantaviiva jolla on oma "leveampi lahti" - "kapeampi salmi"
                // -rytmi. Ks. roadX(z) PakettiporinaElements.cs/Stars.cs:ssa. Koko tie
                // edelleen +15 sivuun (baseX) - meri pysyy ~34 yksikkoa leveana
                // (~48% 71-levyisesta maastosta) KOKO matkan, koska vesikaistale on aina
                // roadX(z):sta laskettu vakioetaisyys, ei riipu mutkan suunnasta.
                //
                // Meri on tien VASEMMALLA (pienemman X:n) puolella koko matkan - laiva/
                // laituri/vene/poijut/majakka/rantalyhdyt sielia. Nosturi/kontit/varasto
                // maapuolella (oikea/suurempi X) - sijoitettu TOISEN mutkan (z=45..75)
                // kohdalle, jossa roadX on pienimmillaan (~9-12) ja maata on siksi
                // EITEN eniten (18-19 yksikkoa piennarelta kentan reunaan, ei enaa vain
                // ~3 kuten alkuperaisessa suunnitelmassa). Jokainen sijainti tarkistettu
                // ohjelmallisesti KIERRETTYA bounding boxia vasten (ei vain pivot-etaisyytta).
                //
                // Tarina radan varrella: satama "suu" (majakka) -> lahti (laituri+laiva+
                // vene, poijuja vedessa) -> lastauspiha (nosturi+kontit+varasto) -> uloskaynti
                // (toinen rantalyhty).
                items = new[]
                {
                    // --- Majakka: sataman "suulla" (mutka alkaa, z=-100), keskitetty
                    // hiekkakaistaleen (X 4,7-7,2) paalle - ensimmainen nakyva maamerkki. ---
                    new DecorItem { modelName = "Satama_Majakka", pos = new Vector3(6f, 0f, -100f), rotY = 0f, scale = 1.8f },

                    // --- Lahti-klusteri mutkan huipulla (z=-50, roadX=25 - levein kohta
                    // vedelle tuolla puolella). Laituri (todellinen pituus x1.4 = 14,56,
                    // puolikas 7,28) pivotoitu X=11,2 niin etta maapaa (~18,5) jaa juuri
                    // ja juuri piennaren sisalle (road_L=19 tassa Z:ssa) eika ulotu tielle,
                    // vesipaa (~3,9) reilusti veden puolella. Laiva laiturin vesipaan
                    // tuntumassa, vene hiukan kauempana lahdessa. ---
                    new DecorItem { modelName = "Satama_Laituri", pos = new Vector3(11.2f, 0f, -50f), rotY = 90f, scale = 1.4f },
                    new DecorItem { modelName = "Satama_Laiva", pos = new Vector3(0f, 0f, -50f), rotY = 190f, scale = 1.0f },
                    // KORJATTU 8.8.2026: Satama_Vene-mallikin (uudelleenrakennettu) tarvitsee
                    // rotX=-90 seisoakseen oikein, kayttajan Unityssa testaama - eli oletukseni
                    // etta uusi hull-rakennustekniikka ei sita tarvitsisi oli vaara.
                    new DecorItem { modelName = "Satama_Vene", pos = new Vector3(6f, 0f, -35f), rotX = -90f, rotY = 45f, scale = 1.0f },

                    // --- Pikkuvenesatama: lisaa veneita laiturin ympärille marina-tyyliin
                    // (referenssikuvat: rivissä venelaituri, purjeveneita ja moottoriveneita
                    // rannan tuntumassa). Kaikki tarkistettu KIERRETYLLA bounding boxilla
                    // pysymaan veden vyöhykkeen (roadX-44.3 .. roadX-10.3) sisalla, riittavan
                    // kaukana laiturista/laivasta/toisistaan (>=2,5 yks. valia). ---
                    new DecorItem { modelName = "Satama_Vene", instanceName = "Satama_Vene_B", pos = new Vector3(9f, 0f, -58f), rotX = -90f, rotY = -25f, scale = 1.0f },
                    new DecorItem { modelName = "Satama_Vene", instanceName = "Satama_Vene_C", pos = new Vector3(0f, 0f, -40f), rotX = -90f, rotY = 150f, scale = 0.9f },
                    // KORJATTU 7.8.2026: Purjeveneen (vanha, ei-uudelleenrakennettu FBX)
                    // akselisto ei osu Unityn Y-ylos-suuntaan (vene makasi kyljellaan) -
                    // kayttaja testasi Unityssa oikeat arvot: rotX=-90, rotY Purjevenelle
                    // paivitetty 110->280.
                    new DecorItem { modelName = "Satama_Purjevene", pos = new Vector3(2.5f, 0f, -60f), rotX = -90f, rotY = 280f, scale = 1.1f },
                    new DecorItem { modelName = "Satama_Purjevene", instanceName = "Satama_Purjevene_B", pos = new Vector3(-5f, 0f, 20f), rotX = -90f, rotY = 200f, scale = 1.0f },
                    // HUOM 8.8.2026: Moottorivene RAKENNETTIIN UUDELLEEN (uusi runko+
                    // konsoli+tuulilasi+moottori, sama akselisto kuin Suihkulahde/
                    // Kukkapenkki/Pensas/Keinu jotka EIVAT tarvitse rotX-korjausta) - siksi
                    // vanha rotX=-90 -kiertokorjaus poistettu, uusi malli seisoo oikein
                    // pelkalla rotY:lla. TARKISTA Unityssa - jos vene nyt makaa kyljellaan,
                    // rotX=-90 pitaa palauttaa (kerro niin lisaan sen takaisin).
                    new DecorItem { modelName = "Satama_Moottorivene", pos = new Vector3(10f, 0f, -44f), rotY = 255f, scale = 1.0f },
                    new DecorItem { modelName = "Satama_Moottorivene", instanceName = "Satama_Moottorivene_B", pos = new Vector3(-6f, 0f, 50f), rotY = 60f, scale = 0.9f },

                    // --- Poijut avoimessa vedessa, ripoteltu koko lahden matkalle ---
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_A", pos = new Vector3(-5f, 0f, -80f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_E", pos = new Vector3(-10f, 0f, -30f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_B", pos = new Vector3(-8f, 0f, -15f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_C", pos = new Vector3(-10f, 0f, 30f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_D", pos = new Vector3(-10f, 0f, 50f), rotY = 0f, scale = 1.3f },

                    // --- Rantavalot: yksi lahden reunalla, toinen uloskaynnin suoralla ---
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_A", pos = new Vector3(14f, 0f, -65f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_C", pos = new Vector3(5f, 0f, 13f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_B", pos = new Vector3(6f, 0f, 140f), rotY = 0f, scale = 1.3f },

                    // --- Lastauspiha (nosturi + kontit + varasto), toisen mutkan (z=45..75)
                    // kohdalla jossa roadX on pienimmillaan ja maata siksi eniten - kaikki
                    // tarkistettu kierretylla bounding boxilla piennaren (~17-20) ja kentan
                    // reunan (35,75) valiin, reilulla marginaililla. ---
                    new DecorItem { modelName = "Satama_Konttipino", instanceName = "Satama_Konttipino_B", pos = new Vector3(24f, 0f, 45f), rotY = 20f, scale = 1.0f },
                    new DecorItem { modelName = "Satama_Nosturi", pos = new Vector3(24f, 0f, 55f), rotY = 0f, scale = 1.0f },
                    new DecorItem { modelName = "Satama_Konttipino", instanceName = "Satama_Konttipino_A", pos = new Vector3(23f, 0f, 65f), rotY = 0f, scale = 1.0f },
                    new DecorItem { modelName = "Satama_Varasto", pos = new Vector3(28f, 0f, 75f), rotY = -10f, scale = 1.0f },
                },
            },
            new DecorPreset
            {
                sceneName = "Huvipuisto",
                items = new[]
                {
                    new DecorItem { modelName = "Huvipuisto_Maailmanpyora", pos = new Vector3(28f, 0f, 0f), rotY = 0f, scale = 1.8f },
                    new DecorItem { modelName = "Huvipuisto_Teltta", pos = new Vector3(-25f, 0f, -60f), rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Huvipuisto_Karuselli", pos = new Vector3(-15f, 0f, -60f), rotY = 0f, scale = 1.6f },
                    // Oli x=-35 (lahes kentan reunalla, +-35.5) - siirretty hiukan sisemmas.
                    new DecorItem { modelName = "Huvipuisto_Myyntikoju", pos = new Vector3(-32f, 0f, -55f), rotY = 30f, scale = 1.4f },

                    // PAIVITETTY: kaari ja lippunauha rakennettu uudelleen Blenderissa
                    // oikeassa koossa (kaari 17.2 leveä x 6.9 korkea, lippunauha 16.3 x 4.6)
                    // niin että ne oikeasti ULOTTUVAT TIEN YLI (tie 12 leveä + piennarit) -
                    // sijoitettu roadX(z):n päälle (x=0, koska Huvipuiston roadBaseX=0),
                    // rotY=0 koska molemmat ovat suoralla osuudella jossa tie kulkee
                    // puhtaasti Z-suunnassa. Auto ajaa kaaren/nauhan ALTA.
                    new DecorItem { modelName = "Huvipuisto_Ilmapallokaari", pos = new Vector3(0f, 0f, -160f), rotY = 0f, scale = 1f },
                    new DecorItem { modelName = "Huvipuisto_Lippunauha", pos = new Vector3(0f, 0f, 60f), rotY = 0f, scale = 1f },
                    // Pomppulinna: uusi malli (linnake + tornit + sisäänkäynti + liput,
                    // selvästi tunnistettava) - siirretty omalle paikalleen kauemmas
                    // Teltta/Karuselli/Myyntikoju-klusterista, sisäänkäynti tietä kohti.
                    new DecorItem { modelName = "Huvipuisto_Pomppulinna", pos = new Vector3(18f, 0f, -70f), rotY = -90f, scale = 1.4f },

                    // --- Lisää huvipuistoelementtejä ---
                    new DecorItem { modelName = "Huvipuisto_Popcornkoju", pos = new Vector3(-14f, 0f, 55f), rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_A", pos = new Vector3(10f, 0f, -145f), rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_B", pos = new Vector3(-10f, 0f, 145f), rotY = 0f, scale = 1.3f },
                },
            },
        };

        [MenuItem("Pakettiporina/12 - Lisaa teemakoristeet talle radalle")]
        public static void AddDecor()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            s.AppendLine($"=== TEEMAKORISTEET: {sceneName} ===");

            DecorPreset preset = null;
            foreach (var p in PRESETS)
                if (p.sceneName == sceneName) { preset = p; break; }

            if (preset == null)
            {
                s.AppendLine($"Tälle scenelle ('{sceneName}') ei ole teemakoristepresettia (tuetut: Puisto, Satama, Huvipuisto).");
                Debug.LogWarning(s.ToString());
                return;
            }

            // TAYSI SIIVOUS ENSIN: tuhoa KAIKKI aiemmin taman komennon luomat koristeet
            // (merkitty PakettiporinaDecorMarker-komponentilla), riippumatta siita
            // esiintyyko niiden nimi enaa nykyisessa PRESETS-listassa. Tama poistaa
            // myos "orpo"-objektit jotka jaivat scenneen aiemmasta koodiversiosta kun
            // koristeen nimi/maara muuttui (esim. suihkulahteen/kukkapenkkien vanhat
            // asettelut) - naita vanha pelkka-nimivertailu ei koskaan loytanyt eika
            // SceneDump nayttanyt, koska molemmat lukevat vain nykyista listaa.
            int cleared = 0;
            foreach (var marker in Object.FindObjectsOfType<PakettiporinaDecorMarker>(true))
            {
                Undo.DestroyObjectImmediate(marker.gameObject);
                cleared++;
            }
            s.AppendLine($"Siivottu {cleared} vanhaa koristetta (myos nykyisessa listassa enaa esiintymattomat).");

            int placed = 0, missing = 0;
            foreach (var item in preset.items)
            {
                string instName = string.IsNullOrEmpty(item.instanceName) ? item.modelName : item.instanceName;
                string path = $"{MODELS_DIR}/{item.modelName}.fbx";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    s.AppendLine($"VIRHE: '{path}' ei loydy - onko Unity tuonut FBX:n viela? (Assets -> Refresh)");
                    missing++;
                    continue;
                }

                // Varmuuden vuoksi: tuhoa myos mahdollinen samanniminen objekti joka EI
                // ole markkeroitu (esim. kasin Hierarkiaan raahattu testiobjekti).
                foreach (var t in Object.FindObjectsOfType<Transform>(true))
                {
                    if (t.name == instName) { Undo.DestroyObjectImmediate(t.gameObject); break; }
                }
                var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
                go.name = instName;
                go.AddComponent<PakettiporinaDecorMarker>().modelName = item.modelName;
                go.transform.SetParent(PakettiporinaHierarchy.GetFolder(PakettiporinaHierarchy.OBJEKTIT), false);
                Undo.RegisterCreatedObjectUndo(go, "Luo " + instName);
                go.transform.position = item.pos;
                go.transform.rotation = Quaternion.Euler(item.rotX, item.rotY, 0f);
                go.transform.localScale = Vector3.one * item.scale;
                s.AppendLine($"{instName}: sijoitettu ({item.pos.x:F0}, {item.pos.z:F0}), kierto Y={item.rotY:F0}, koko x{item.scale:F1}");
                placed++;
            }

            s.AppendLine($"\nValmis. {placed} koristetta sijoitettu, {missing} puuttui.");
            if (missing > 0)
                s.AppendLine("Aja tama uudelleen kun puuttuvat FBX:t ovat tuotu Unityyn.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(s.ToString());
        }
    }
}
#endif
