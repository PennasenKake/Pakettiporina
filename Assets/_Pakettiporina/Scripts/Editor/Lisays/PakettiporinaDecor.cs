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

        // KANSIOITU 9.8.2026: mallit jaettu scenekohtaisiin alikansioihin (Maatila/
        // Puisto/Satama/Huvipuisto) selkeyden vuoksi - MODELS_DIR-juuressa on enaa
        // vain jaetut/yleiset mallit (Car1, Finish, Ground, Ground1, Koristeet,
        // Maasto, Star, Untitled). ResolveModelPath kokeilee ensin suoraan juuresta
        // (vanha kaytanto, jaetut mallit) ja sitten jokaisesta alikansiosta - talla
        // loytyvat myos Puisto_Penkki/Puisto_Lyhtypylvas jotka ovat Puisto-kansiossa
        // mutta joita Huvipuisto-preset kayttaa samalla modelName:lla.
        static readonly string[] MODEL_SUBFOLDERS = { "Maatila", "Puisto", "Satama", "Huvipuisto" };

        static string ResolveModelPath(string modelName)
        {
            string direct = $"{MODELS_DIR}/{modelName}.fbx";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(direct) != null) return direct;

            foreach (var sub in MODEL_SUBFOLDERS)
            {
                string p = $"{MODELS_DIR}/{sub}/{modelName}.fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(p) != null) return p;
            }
            return direct; // ei loytynyt mistaan - palauta suora polku niin virheviesti nayttaa sen
        }

        class DecorItem
        {
            public string modelName;     // FBX-tiedoston nimi ilman paatetta
            public string instanceName;  // scenen GameObjectin nimi - jos tyhja, kaytetaan modelName (mahdollistaa saman mallin useita kopioita)
            public Vector3 pos;
            public float rotX;           // korjauskierto (esim. veneet, joiden FBX-mallin oma
                                          // "ylos"-suunta ei osu Unityn Y-akselille) - useimmilla 0
            public float rotY;
            public float rotZ;           // LISATTY 8.8.2026: kayttaja tarvitsi 3-akselisen kierron
                                          // (esim. Maskotti/Rengasheitto/Pomppulinna) - useimmilla 0
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
                // Maatila: navetta+heinapaalit klusterina ensimmaisen kaarteen ulkolaidalla
                // (lansi, negat. X), aita seuraa molempia kaarteita +10 yks. tien
                // keskilinjasta, lehmat/lammas laitumella suoralla keskiosalla, variksen-
                // pelatti + tuulimylly toisen kaarteen molemmin puolin maamerkkeina.
                // Kaikki sijainnit tarkistettu RoadX(z):aa vasten - vahintaan ~10-18
                // yksikkoa piennaresta (piennar paattyy ~7,8 keskilinjasta).
                sceneName = "Maatila",
                items = new[]
                {
                    // KORJATTU 8.8.2026 (ENNAKOIVASTI, ei viela Unity-testattu): nama mallit on
                    // rakennettu SAMALLA Blender-bmesh-pipelinella ja export-reseptilla kuin
                    // Huvipuiston uudet mallit, joiden todistettiin (MaailmaBounds-dumpin
                    // avulla) makaavan kyljellaan ilman rotX=-90 -korjausta. Lisatty samalla
                    // logiikalla tanne etukateen - vahvista Unityssa "15"-dumpilla kun Maatila-
                    // scene on olemassa, ja kerro jos joku nayttaa silti vaaralta.
                    // SUURENNETTU 14.8.2026 (kayttaja: "koko voisi kasvattaa, etenkin navetan").
                    // Scale 1.0->1.5. Tarkistettu ettei uusi kylkimitta (puolisko 2,35*1,5=3,53,
                    // X-vali -21,53..-14,48 / Z-vali -73,53..-66,48) osu Heinapaali-paalien
                    // paikkoihin (lahin, Heinapaali_E, X=-12 pysyy yli 2,4 yksikon paassa uudesta
                    // reunasta). Muista tarkistaa RoadX-marginaali "15"-dumpilla kun Unity auki.
                    new DecorItem { modelName = "Maatila_Navetta", pos = new Vector3(-18f, 0f, -70f), rotX = -90f, rotY = 90f, scale = 1.5f },

                    // KORJATTU 14.8.2026 (kayttajan pyynnosta "heinapaalit asetella paremmin ja
                    // enemman"): vanhat 3 palloa olivat hajallaan yli 20 yksikon matkalla (ei
                    // nayttanyt kasalta). Tiivistetty TODELLISEKSI KASAKSI navetan oven eteen
                    // (referenssikuvan mukaan) - 6 paalia n. 1,5-2 yksikon paassa toisistaan,
                    // vaihtelevat rotaatiot/koot etta nayttaa luonnollisesti kaatuneelta pinolta
                    // eika rivilta. Aita_Aita.fbx mitattu Blenderista (2,52 x 0,12 x 1,10) -
                    // Heinapaali samaa kokoluokkaa, klusteri mahtuu piennaren ulkopuolelle.
                    // HAJAUTETTU 14.8.2026 (kayttaja: "paaleja hajauttaa ja asetella paremmin") -
                    // vanha kasa oli liian tiivis (kaikki 1,5-2 yks. paassa toisistaan, navetan
                    // ovella). Levitetty laajemmalle alueelle navetan ETELA/lansipuolelle
                    // (X -9..-16, Z -50..-72), 4-8 yksikon valein - nayttaa enemman "pellolle
                    // unohdetuilta paaleilta" kuin yhdelta rykelmalta. Kaikki X > -16 eli barn'in
                    // X-alueen (-20.35..-15.65) ULKOPUOLELLA aina, ei paallekkaisyytta riippumatta
                    // Z:sta.
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_A", pos = new Vector3(-11f, 0f, -58f), rotX = -90f, rotY = 10f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_B", pos = new Vector3(-14f, 0f, -52f), rotX = -90f, rotY = 95f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_C", pos = new Vector3(-9f, 0f, -65f), rotX = -90f, rotY = 200f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_D", pos = new Vector3(-15f, 0f, -60f), rotX = -90f, rotY = 50f, scale = 0.85f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_E", pos = new Vector3(-12f, 0f, -70f), rotX = -90f, rotY = 330f, scale = 1.05f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_F", pos = new Vector3(-10f, 0f, -54f), rotX = -90f, rotY = 160f, scale = 0.9f },
                    // LISATTY 14.8.2026 (kayttaja: "lisaa heinapaaleja lahelle tuulimyllya") -
                    // Tuulimylly on X-20,Z90 - paalit sen tuntumaan, ei kuitenkaan tornin
                    // tyveen kiinni eika tielle (roadX(90)=n. -5,8, kaikki paalit selvasti
                    // lansipuolella eli tuulimyllyn kanssa samalla puolella).
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_G", pos = new Vector3(-14f, 0f, 84f), rotX = -90f, rotY = 40f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_H", pos = new Vector3(-13f, 0f, 94f), rotX = -90f, rotY = 300f, scale = 0.9f },
                    new DecorItem { modelName = "Maatila_Heinapaali", instanceName = "Maatila_Heinapaali_I", pos = new Vector3(-26f, 0f, 87f), rotX = -90f, rotY = 120f, scale = 1.05f },

                    // KORJATTU KOLMANNEN KERRAN 14.8.2026 (kayttaja lahetti referenssikuvan
                    // oikeasta pylvas+3-poikkilauta-aitauksesta ja pyysi: "aitaukset ja elaimet
                    // lahdosta katsottuna aseteltuna oikealle seka muut vasemmalle... lampaat,
                    // kanat, hevonen ja lehmat omiin aitauksiin"). Yksi iso yhteinen laidun
                    // korvattu NELJALLA ERILLISELLA, TIIVIILLA AITAUKSELLA (yksi per elainlaji) -
                    // Aita.fbx:n materiaalit (Aita_Post x:[-1.26,1.26], Aita_Rail z:[0.33,0.82])
                    // vahvistavat etta malli JO ON pylvas+lauta-paneeli, tasan referenssikuvan
                    // tyylia - ongelma oli vain etta segmentit oli aiemmin sijoitettu 5-8 yksikon
                    // valein (rei'illa). Nyt jokainen aitaus on 5,04x5,04 nelio, 2 segmenttia/sivu
                    // (segmentin pituus 2,52), PAAT OSUVAT TASAN YHTEEN joka nurkassa - ei rakoja.
                    // Sijoitus: Lehma+Hevonen OIKEALLE (positiivinen X, sama avoin suora Z -10..24
                    // missa vanha laidun oli), Lammas+Kana VASEMMALLE (negatiivinen X, peilikuva) -
                    // kaikki reilusti (>9 yks.) tien 6 yksikon reunasta. Vanhat vapaana kavelevat
                    // Kana_A/B/C navetan pihalta POISTETTU ja korvattu omalla aitauksellaan.
                    //
                    // SUURENNETTU 14.8.2026 (kayttaja: "aitauksista isompia elaimia varten").
                    // Kaikki 4 aitausta 5,04x5,04 -> 7,56x7,56 (segmentin scale 1.0->1.5,
                    // sijainnit uudelleenlaskettu keskipisteen ympari samalla kaavalla).
                    // Tarkistettu etteivat aitaukset osu toisiinsa (Lehma/Hevonen valissa
                    // n. 4,4 yksikkoa tilaa) eika tien reunaan (sisareuna X=14,22 / 8,2
                    // yksikkoa piennaresta suoralla keskiosalla, roadX=0).
                    //
                    // --- LEHMA-AITAUS (oikea, keskus 18,-4) ---
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_N1", pos = new Vector3(16.11f, 0f, -0.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_N2", pos = new Vector3(19.89f, 0f, -0.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_S1", pos = new Vector3(16.11f, 0f, -7.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_S2", pos = new Vector3(19.89f, 0f, -7.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_W1", pos = new Vector3(14.22f, 0f, -5.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_W2", pos = new Vector3(14.22f, 0f, -2.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_E1", pos = new Vector3(21.78f, 0f, -5.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lehma_Aita_E2", pos = new Vector3(21.78f, 0f, -2.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Lehma", instanceName = "Maatila_Lehma_A", pos = new Vector3(17f, 0f, -5.5f), rotX = -90f, rotY = 200f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Lehma", instanceName = "Maatila_Lehma_B", pos = new Vector3(19f, 0f, -2.5f), rotX = -90f, rotY = 160f, scale = 1.0f },

                    // --- HEVONEN-AITAUS (oikea, keskus 18,8) ---
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_N1", pos = new Vector3(16.11f, 0f, 11.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_N2", pos = new Vector3(19.89f, 0f, 11.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_S1", pos = new Vector3(16.11f, 0f, 4.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_S2", pos = new Vector3(19.89f, 0f, 4.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_W1", pos = new Vector3(14.22f, 0f, 6.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_W2", pos = new Vector3(14.22f, 0f, 9.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_E1", pos = new Vector3(21.78f, 0f, 6.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Hevonen_Aita_E2", pos = new Vector3(21.78f, 0f, 9.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Hevonen", pos = new Vector3(18f, 0f, 8f), rotX = -90f, rotY = 300f, scale = 1.1f },

                    // --- LAMMAS-AITAUS (vasen, keskus -18,-4 - peilikuva Lehma-aitauksesta) ---
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_N1", pos = new Vector3(-19.89f, 0f, -0.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_N2", pos = new Vector3(-16.11f, 0f, -0.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_S1", pos = new Vector3(-19.89f, 0f, -7.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_S2", pos = new Vector3(-16.11f, 0f, -7.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_W1", pos = new Vector3(-21.78f, 0f, -5.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_W2", pos = new Vector3(-21.78f, 0f, -2.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_E1", pos = new Vector3(-14.22f, 0f, -5.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Lammas_Aita_E2", pos = new Vector3(-14.22f, 0f, -2.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Lammas", instanceName = "Maatila_Lammas_A", pos = new Vector3(-19f, 0f, -5.5f), rotX = -90f, rotY = 90f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Lammas", instanceName = "Maatila_Lammas_B", pos = new Vector3(-17f, 0f, -5.5f), rotX = -90f, rotY = 250f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Lammas", instanceName = "Maatila_Lammas_C", pos = new Vector3(-18f, 0f, -2.5f), rotX = -90f, rotY = 30f, scale = 0.9f },

                    // --- KANA-AITAUS (vasen, keskus -18,8 - peilikuva Hevonen-aitauksesta).
                    // Vanhat vapaana kavelevat Kana_A/B/C navetan pihalta siirretty tanne. ---
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_N1", pos = new Vector3(-19.89f, 0f, 11.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_N2", pos = new Vector3(-16.11f, 0f, 11.78f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_S1", pos = new Vector3(-19.89f, 0f, 4.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_S2", pos = new Vector3(-16.11f, 0f, 4.22f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_W1", pos = new Vector3(-21.78f, 0f, 6.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_W2", pos = new Vector3(-21.78f, 0f, 9.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_E1", pos = new Vector3(-14.22f, 0f, 6.11f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Aita", instanceName = "Kana_Aita_E2", pos = new Vector3(-14.22f, 0f, 9.89f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Maatila_Kana", instanceName = "Maatila_Kana_A", pos = new Vector3(-19f, 0f, 6.7f), rotX = -90f, rotY = 0f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Kana", instanceName = "Maatila_Kana_B", pos = new Vector3(-17f, 0f, 6.7f), rotX = -90f, rotY = 120f, scale = 1.0f },
                    new DecorItem { modelName = "Maatila_Kana", instanceName = "Maatila_Kana_C", pos = new Vector3(-18f, 0f, 9.3f), rotX = -90f, rotY = 250f, scale = 1.0f },

                    // --- Toisen kaarteen maamerkit ---
                    // SUURENNETTU 14.8.2026 (kayttaja: "pelatinta voi suurentaa"). Scale 1.2->1.6.
                    new DecorItem { modelName = "Maatila_Variksenpelatti", pos = new Vector3(10f, 0f, 45f), rotX = -90f, rotY = 0f, scale = 1.6f },
                    new DecorItem { modelName = "Maatila_Tuulimylly", pos = new Vector3(-20f, 0f, 90f), rotX = -90f, rotY = 0f, scale = 1.2f },

                    // --- LISATTY 14.8.2026 (Blenderilla uudelleenrakennettu Maatila-erä):
                    // Navetta/Tuulimylly/Variksenpelatti korvattu yksityiskohtaisemmilla
                    // malleilla (sama FBX-nimi, sama sijainti/rotaatio - vain geometria
                    // parani). Kolme UUTTA elementtia lisatty samalla bmesh-pipelinella
                    // (sama rotX=-90 -konventio): Traktori pysakoity navetan pihalle,
                    // Siilo navetan viereen, Kaivo laitumen laidalla - kaikki sijainnit
                    // valittu navetta/aita-klusterin (X -11..-24, Z -35..-95) tuntumaan
                    // ei viela Unity-testattu, tarkista "15"-dumpilla kun mahdollista. ---
                    // SUURENNETTU 14.8.2026 (kayttaja: "varmista kokojen asettelut"): SceneDump
                    // paljasti etta Traktori (pituus 2,59) oli lahes samankokoinen kuin Lehma
                    // (2,19) - oikeasti traktori on ~2x lehmaa isompi maamerkki. Scale 1.0->1.3.
                    // SIIRRETTY 14.8.2026 (kayttaja: "navettaa, paaleja, traktoria ja siiloa
                    // hajauttaa ja asetella paremmin") - Traktori/Siilo olivat molemmat aivan
                    // navetan kyljessa (5-6 yks. paassa) - koko klusteri nayttl liian ahtaalta
                    // rykelmalta. Traktori siirretty selvasti omaksi "pysakoidyksi" kohteekseen
                    // eri puolelle kaarretta (X -14, Z -40, ~30 yks. navetasta), Siilo siirretty
                    // navetan taakse kauemmas (X -23, Z -90, ~20 yks. navetasta) - molemmat
                    // pysyvat reilusti tien 6 yksikon reunan ulkopuolella koko matkalla.
                    // SKAALA KORJATTU (traktori rakennettu kokonaan uudelleen isommilla/
                    // paremmilla mittasuhteilla - pituus nyt 2,72 (oli 2,24), joten 1.3-kerroin
                    // teki siita ylisuuren. 1.0 riittaa nyt samaan visuaaliseen kokoon.
                    // SIIRRETTY (kayttajan kuvakaappaus): traktori oli X=-14, Z=-40 kohdalla
                    // roadX(-40)=-5,78 (tie -11,78..0,22) - vain 2,22 yksikon marginaali
                    // tien reunasta, minka takia se nayttai peliruudulla tunkevan autoa ja
                    // taytti puolet kuvasta liian lahelta kameraa. Siirretty X -14 -> -21
                    // (marginaali nyt n. 9,2 yksikkoa, samaa luokkaa kuin muu Maatilan
                    // koristelu) - sama Z, sama kulma, ei tarvinnut pienentaa itse mallia.
                    // SIIRRETTY 14.8.2026 (kayttaja: "sijoita traktori paremmin") - vanha
                    // yksinainen "pysakoity" paikka (X-21,Z-40) korvattu navetan/siilon
                    // pihapiirin osalla, luontevampi konteksti maatilan koristeille.
                    // roadX(-78)=n. 12,7 (bend1-kaava), traktori X=-15 -> yli 27 yksikon
                    // marginaali tiehen, ei osu navetan (X -21,5..-14,5 @ scale 1.5)
                    // laajentuneeseen kylkeen koska Z-alueet eivat mene paallekkain.
                    new DecorItem { modelName = "Maatila_Traktori", pos = new Vector3(-15f, 0f, -78f), rotX = -90f, rotY = 45f, scale = 1.3f },
                    // LISATTY 14.8.2026 (kayttaja: "voit lisata toisen traktorin") - toinen
                    // traktori peltotyossa maissipellon laidalla (Tuulimyllyn puolella,
                    // ks. Maatila_Maissipelto alempana), roadX(85)=n. -6,9 (bend2), traktori
                    // X=8 tien VASTAKKAISELLA (idan) puolella samoin kuin pelto -> ei osu tiehen.
                    new DecorItem { modelName = "Maatila_Traktori", instanceName = "Maatila_Traktori_2", pos = new Vector3(6f, 0f, 80f), rotX = -90f, rotY = 200f, scale = 1.2f },
                    // LISATTY 14.8.2026 (kayttaja: "tee reilun kokoinen maissipelto
                    // tuulimyllysta katsottuna tien toiselle puolelle") - Tuulimylly X-20,
                    // roadX(90)=n. -5,8 eli Tuulimylly on tien LANSIPUOLELLA, pelto siis
                    // ITAPUOLELLE. Uusi malli Maatila_Maissipelto.fbx (paikallismitat
                    // 15,4 x 8,2, skaalattu 1,4 -> 21,6 x 11,5). roadX(85)=n. -8,2, pellon
                    // sisareuna X=6,2 -> yli 8 yksikon marginaali piennaresta.
                    new DecorItem { modelName = "Maatila_Maissipelto", pos = new Vector3(17f, 0f, 85f), rotX = -90f, rotY = 0f, scale = 1.4f },
                    // SUURENNETTU 14.8.2026 (kayttaja: "kasvata siilon kokoa"). Scale 1.0->1.6.
                    new DecorItem { modelName = "Maatila_Siilo", pos = new Vector3(-23f, 0f, -90f), rotX = -90f, rotY = 0f, scale = 1.6f },
                    // SUURENNETTU 14.8.2026 (kayttaja: "kaivoa voi suurentaa"). Scale 1.0->1.5.
                    new DecorItem { modelName = "Maatila_Kaivo", pos = new Vector3(18f, 0f, -15f), rotX = -90f, rotY = 0f, scale = 1.5f },
                },
            },
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
                    // HIOTTU 9.8.2026 (v5): iso, paksummat rungot, kirkkaammat/isommat
                    // istuimet, lisatty mulch-varinen turva-alusta rakenteen alle jotta
                    // se lukeutuu heti "leikkikentaksi". rotX=-90 -korjaus, pos/rotY
                    // sailytetty ennallaan (paikka oikea, vain malli hiottiin).
                    // KORJATTU 9.8.2026 (v6): kayttaja totesi etta istuinosa on liian
                    // ylhaalla - ketju oli vain 64% pylvaan korkeudesta (istuin z=0.9 paikal-
                    // lista, apex=2.5). Istuin siirretty alemmas (z=0.42, ketju nyt 83%
                    // pylvaan korkeudesta) - paljon klassisempi/matalampi istuinkorkeus
                    // (maailmassa n. 0.59 maasta, oli n. 1.26).
                    new DecorItem { modelName = "Puisto_Keinu", pos = new Vector3(17f, 0f, -40f), rotX = -90f, rotY = 0f, scale = 1.4f },
                    // Silta on nyt oikea kaarisilta (uusi Blender-malli) - siirretty
                    // lammen (Puisto_Lampi) paalle, selvasti kauemmas tiesta kuin ennen.
                    new DecorItem { modelName = "Puisto_Silta", pos = new Vector3(-30f, 0f, 70f), rotX = -90f, rotY = 90f, scale = 1.4f },
                    new DecorItem { modelName = "Puisto_Lampi", pos = new Vector3(-30f, 0f, 70f), rotX = -90f, rotY = 0f, scale = 1.5f },
                    // PARANNELTU 9.8.2026: Penkki rakennettu uudelleen (puulaudat+musta
                    // metallirunko) -> tarvitsee rotX=-90 -korjauksen (jaettu malli, kaytossa
                    // myos Huvipuistossa - katso kaikki 4 esiintymaa alla).
                    // KOKOAUDIT 9.8.2026: uusi v2-malli oli scale=1.4:lla ylimitoitettu
                    // (syvyys 1.26m, korkeus 1.47m - oikea penkki ~0.5-0.6m syva, ~0.9-1m
                    // korkea). Scale pienennetty 1.4 -> 1.0 kaikissa 4 esiintymassa.
                    // KAANNETTY 9.8.2026: kayttaja totesi Unityssa etta penkit ovat vaarin
                    // pain - rotY +180 kaikissa 4 esiintymassa (myos Huvipuistossa). Scale
                    // 1.0 -> 1.2 (kayttaja: koko suhteessa muihin objekteihin, 1.0 oli
                    // ilmeisesti liian pieni - kompromissi alkuperaisen 1.4:n ja 1.0:n valilla).
                    new DecorItem { modelName = "Puisto_Penkki", pos = new Vector3(15f, 0f, -34f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    // TARKISTETTU 9.8.2026 ("tarkista Puisto objektien sijoittelu"):
                    // etaisyys tiesta oli vain 8.56 (piennar paattyy 7.8:aan) - liian
                    // pieni marginaali. Siirretty X 16 -> 18 (marginaali nyt 10.56).
                    // PARANNELTU 9.8.2026: Lyhtypylvas rakennettu uudelleen (koristeellinen
                    // jalusta+kuusikulmainen lasilyhty) -> tarvitsee rotX=-90 -korjauksen
                    // (jaettu malli, kaytossa myos Huvipuistossa - katso kaikki 5 esiintymaa).
                    // KOKOAUDIT 9.8.2026: uusi v2-malli oli scale=1.5:lla ~6m korkea (oikea
                    // puistolyhtypylvas ~4-5m). Scale pienennetty 1.5 -> 1.1 kaikissa 5
                    // esiintymassa (korkeus nyt n. 4.5m).
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", pos = new Vector3(18f, 0f, -100f), rotX = -90f, rotY = 0f, scale = 1.1f },

                    // PARANNELTU 9.8.2026: Puu rakennettu uudelleen (runko+latvusklusteri+
                    // kukka-aksentit) -> tarvitsee rotX=-90 -korjauksen.
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_A", pos = new Vector3(14f, 0f, -110f), rotX = -90f, rotY = 0f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_B", pos = new Vector3(-15f, 0f, -20f), rotX = -90f, rotY = 40f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_C", pos = new Vector3(14f, 0f, 45f), rotX = -90f, rotY = 80f, scale = 1.8f },
                    new DecorItem { modelName = "Puisto_Puu", instanceName = "Puisto_Puu_D", pos = new Vector3(6f, 0f, 95f), rotX = -90f, rotY = 160f, scale = 1.8f },
                    // Suihkulahde UUDISTETTU: monikerroksinen (allas+jalusta+allas+jalusta+
                    // huippumalja+kultasuutin), 5.2 leveä x 3.45 korkea raakamitta (oli
                    // 1.8x1.65) - selvasti isompi ja naeyttavampi. Scale 1.2 -> maailmassa
                    // ~6.2 leveä, ~4.1 korkea. Ympärille 4 kukkapenkkia sateen muotoon
                    // (referenssikuvien muodollinen ruusutarha-asettelu).
                    // PARANNELTU 8.8.2026: rakennettu uudelleen samalla bmesh-pipelinella kuin
                    // Huvipuiston mallit (kirkas paletti, mosaiikkilaatta-reunus ym.) -> tarvitsee
                    // saman rotX=-90 -korjauksen kuin kaikki muutkin talla pipelinella tehdyt.
                    new DecorItem { modelName = "Puisto_Suihkulahde", pos = new Vector3(20f, 0f, 0f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_A", pos = new Vector3(20f, 0f, 0f), rotX = -90f, rotY = 0f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_B", pos = new Vector3(20f, 0f, 0f), rotX = -90f, rotY = 90f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_C", pos = new Vector3(20f, 0f, 0f), rotX = -90f, rotY = 180f, scale = 2.2f },
                    new DecorItem { modelName = "Puisto_Kukkapenkki", instanceName = "Puisto_Kukkapenkki_D", pos = new Vector3(20f, 0f, 0f), rotX = -90f, rotY = 270f, scale = 2.2f },

                    // --- Lisää puiston elementteja: pensaita/topiareja siisteina laikkuina
                    // ympari puistoa, toinen penkki suihkulahteen aareen, toinen lyhtypylvas. ---
                    // HIOTTU 9.8.2026 (v3): enemman muotovaihtelua (5 paallekkaista palloa
                    // 3 vihrean savyssa, oli 3 palloa), 6 kukka-aksenttia 4 varissa (oli 4
                    // kukkaa 2 varissa) - sama malli kaikissa 4 instanssissa. rotX=-90 sailyi.
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_A", pos = new Vector3(22f, 0f, -8f), rotX = -90f, rotY = 0f, scale = 1.1f },
                    // TARKISTETTU 9.8.2026: Pensas_B ja Penkki_B olivat vain 2.24 yksikon
                    // paassa toisistaan (todennakoinen paallekkaisyys) - siirretty Pensas_B
                    // kauemmas (18,8) -> (13,6), etaisyys nyt n. 6.7.
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_B", pos = new Vector3(13f, 0f, 6f), rotX = -90f, rotY = 0f, scale = 1.1f },
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_C", pos = new Vector3(25f, 0f, -95f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Pensas", instanceName = "Puisto_Pensas_D", pos = new Vector3(-25f, 0f, 55f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Penkki", instanceName = "Puisto_Penkki_B", pos = new Vector3(20f, 0f, 9f), rotX = -90f, rotY = 20f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", instanceName = "Puisto_Lyhtypylvas_B", pos = new Vector3(-18f, 0f, 20f), rotX = -90f, rotY = 0f, scale = 1.1f },

                    // KORJATTU 9.8.2026 (v6, TARKEA BUGI): vanha "Y=2 on tarkoituksellinen"
                    // -kaytanto (ks. alla, historiallinen selitys sailytetty) EI huomioinut
                    // etta scale kertoo myos pivotin ja jalkojen valisen etaisyyden - malli
                    // upposi todellisuudessa n. 1,3 yksikkoa maan alle (jalat: 2,05 paikallista
                    // yksikkoa * scale 1,6 = 3,28 maailmayksikkoa pivotin alapuolella, mutta
                    // pos.y nosti vain 2). Rakennettu uudelleen niin etta pivot on NYT
                    // MAATASOSSA (Z=0), sama kaytanto kuin kaikilla muilla objekteilla - pos.y
                    // on siis normaalisti 0f eika enaa riipu scale:sta. Samalla scale
                    // pienennetty 1.6 -> 1.1 jotta koko suhteutuu jarkevammin Keinuun (uusi
                    // maailmakorkeus n. 3,9, Keinu 3,76 - lahella toisiaan, aiemmin liukumaki
                    // oli n. 5,7 eli suhteettoman iso).
                    // (Vanha historia, ei enaa relevantti: alunperin Y=2 valittiin koska
                    // liukumaen malli oli pivotoitu ylatasolta - tama konventio poistettu.)
                    // HIOTTU 9.8.2026 (v7): korvattu avoin alusta+tikkaat SULJETULLA
                    // "mokkitornilla" (2 varillista sivuseinaa + takaseina pyorealla
                    // ikkunalla) - selkeasti tunnistettavampi leikkikentan torni-rakenne.
                    // Kapea tikas -> 5 leveaa karkkiraidallista porrasaskelmaa sivukaiteilla.
                    // Liukuliuska LEVENEE (fan-out) viimeisella neljanneksella - dynaamisempi
                    // "landing zone". Pieni lippu katon huipulla. Pivot edelleen maatasossa
                    // (v6-korjaus sailytetty). Uusi maailmakorkeus n. 4,35 (oli 3,9) - hieman
                    // korkeampi lipun/katon vuoksi, edelleen jarkevassa suhteessa muihin.
                    new DecorItem { modelName = "Puisto_Liukumaki", pos = new Vector3(-12f, 0f, -90f), rotX = -90f, rotY = 0f, scale = 1.1f },

                    // --- UUDET PUISTOMAISET ELEMENTIT 9.8.2026 (kayttajan pyynnosta) ---
                    // KORJATTU 9.8.2026: kayttaja totesi etta nama 5 uusinta objektia (+Roskis/
                    // Aita) EIVAT nakyneet Unityssa - loydettiin UUSI muunnos aiemmasta
                    // useFileScale-bugista: kun FBX:lla EI OLE viela .meta-tiedostoa ollenkaan
                    // (aivan uusi malli, ei koskaan aiemmin tuotu), Unity loi ensimmaisella
                    // tuonnilla .meta:n jonka useFileScale=1 OLETUKSENA - sama ~100x kutistuma
                    // kuin aiemmin re-exportatuilla tiedostoilla. Kaikkien 7 uuden mallin (myos
                    // Roskis+Aita) .fbx.meta korjattu (useFileScale: 0). JATKOSSA: tarkista tama
                    // AINA myos aivan uusille malleille, ei vain uudelleenexportatuille.
                    // Piknikpoyta (v2, hiottu: paksummat penkit, raidallinen liina-reunus,
                    // kukkaruukku) tyhjaan kohtaan radan loppupaassa (z~120) - lansipuolella
                    // tieta, etaisyys tiesta reilusti yli piennaren.
                    // HIOTTU 9.8.2026 (v3): karkkiruudullinen (puna/valko) liina selkeaksi
                    // "piknik"-tunnukseksi (oli vain 2 sivuraitaa), isompi poyta+penkit.
                    // Scale 1.3 -> 1.5 kayttajan pyynnosta ("hieman isompi").
                    new DecorItem { modelName = "Puisto_Piknikpoyta", pos = new Vector3(-18f, 0f, 120f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    // Hiekkalaatikko (v2, hiottu: pyoristetty hiekkakasa, aurinkokatos
                    // karkkiraidalla, enemman leluja).
                    // SIIRRETTY 9.8.2026: kayttaja totesi ettei nay pelissa koska z=-130 on
                    // liian lahella lahtopistetta (startZ=-150, vain 20 yksikon paassa - auto
                    // ehtii ohi ennen kuin pelaaja huomaa sen). Siirretty z=-95:aan (55
                    // yksikon paahan lahdosta) ja X vaihdettu itapuolelta (14) lansipuolelle
                    // (-14) jotta ei osu paallekkain Lyhtypylvas_A/Puu_A:n (molemmat X=14-18,
                    // z=-100..-110) kanssa.
                    // SIIRRETTY KOLMANNEN KERRAN 9.8.2026: kayttaja pyysi sijoittamaan
                    // Hiekkalaatikon Kiipeilytelineen JALKEEN ajosuunnassa (Kiipeilyteline
                    // z=-75, ajosuunta kasvavaan Z:aan kohti maalia z=150). Siirretty valiin
                    // Kiipeilyteline (z=-75) ja Roskis (z=-60): z=-68, X sailytetty (-14) -
                    // etaisyys Kiipeilytelineesta 7, Roskikseen 8.25, molemmat riittavat
                    // koska X-linjaus ei mene paallekkain samalla tavalla kuin Liukumaen
                    // kanssa aiemmin.
                    new DecorItem { modelName = "Puisto_Hiekkalaatikko", pos = new Vector3(-14f, 0f, -68f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    // Roskis aukkokohtaan z~-60 (Pensas_C:n z=-95 ja Keinun z=-40 valissa),
                    // lansipuolella tieta.
                    new DecorItem { modelName = "Puisto_Roskis", pos = new Vector3(-16f, 0f, -60f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    // Opastaulu (v2, hiottu: kompassiruusu-aksentti kyltissa, paksumpi kehys,
                    // pieni lippu katolla) lahella maalia (z~135), itapuolella tieta.
                    // KAANNETTY 9.8.2026: kayttaja vahvisti Unityssa etta rotY=180 oli
                    // vaarin pain - kaannetty 180 astetta lisaa (180+180=0).
                    new DecorItem { modelName = "Puisto_Opastaulu", pos = new Vector3(16f, 0f, 135f), rotX = -90f, rotY = 0f, scale = 1.1f },

                    // --- LISAA PUISTOMAISIA ELEMENTTEJA 9.8.2026 (kayttajan toisesta
                    // pyynnosta) - laajennetaan leikkikentta-aluetta (Keinu z=-40,
                    // Liukumaki z=-90) kiipeilytelineella, seka lisataan lintukylpy ja
                    // matalia koristeaitoja lammen reunalle/leikkikentan laidoille. ---
                    // UUSITTU 9.8.2026 (v3): kayttajan referenssikuvien mukaan kokonaan
                    // uusiksi - kupumainen kaari-versio korvattu 2 puisella tikas-paadylla
                    // (vaakarimat, vihreat hatut, metalliset tukijalat) + keskella 2 paneelia:
                    // keltainen koettuverkko (ristikko+solmupallot) ja musta kiipeilyseina
                    // varikkailla kahvoilla. Selvasti isompi kuin ennen (kayttajan pyynnosta) -
                    // maailmanleveys nyt n. 6.3 (oli 4.68).
                    // TAYSIN UUSITTU 9.8.2026 (v6, kayttajan "paljon paljon paremmaksi"
                    // -pyynnosta): suorat tikas-paadyt korvattu ROHKEALLA punaisella
                    // "sateenkaari"-kaarirakenteella (2 rinnakkaista kaarta maasta maahan,
                    // referenssikuvan 2 mukaan) - selvasti dramaattisempi/tunnistettavampi
                    // silhuetti kuin aiempi laatikkomainen versio. Varilliset rimat kaaren
                    // sisalla, vihreat "hattu"-koristeet huipulla, diagonaaliset tukijalat
                    // molemmissa paissa. Keskella isompi verkko (harvempi 5x5-ristikko +
                    // isot solmupallot vain risteyskohdissa - selkeampi kuin tiheä versio)
                    // ja kiipeilyseina (12 isompaa kahvaa). Uusi maailmanleveys n. 7,4 (oli
                    // 6,7) - Puiston isoin leikkikentan objekti. Sijainti sailytetty,
                    // etaisyydet Hiekkalaatikkoon/Liukumaeen tarkistettu yhä riittaviksi.
                    new DecorItem { modelName = "Puisto_Kiipeilyteline", pos = new Vector3(-14f, 0f, -75f), rotX = -90f, rotY = 0f, scale = 1.15f },
                    // Lintukylpy v2 hiottu: enemman tasoja jalustassa, vesirenkaat altaassa,
                    // 2 lintua siivenavulla (oli 1).
                    new DecorItem { modelName = "Puisto_Lintukylpy", pos = new Vector3(-14f, 0f, -15f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Aita", instanceName = "Puisto_Aita_A", pos = new Vector3(-28f, 0f, 66f), rotX = -90f, rotY = 90f, scale = 1.0f },
                    new DecorItem { modelName = "Puisto_Aita", instanceName = "Puisto_Aita_B", pos = new Vector3(-18f, 0f, -85f), rotX = -90f, rotY = 0f, scale = 1.0f },
                    new DecorItem { modelName = "Puisto_Aita", instanceName = "Puisto_Aita_C", pos = new Vector3(20f, 0f, -45f), rotX = -90f, rotY = 0f, scale = 1.0f },
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
                    // PARANNELTU 8.8.2026: rakennettu uudelleen samalla bmesh-pipelinella kuin
                    // Huvipuiston mallit -> tarvitsee saman rotX=-90 -korjauksen.
                    // HIOTTU 9.8.2026 (v3): lisatty 3 ikkunaa + ovi torniin, kaide+lyhtyhuone
                    // selkeammin erottuvaksi (lasi+kaidepylvaat), pieni valo huipulla.
                    // SUURENNETTU 9.8.2026: kayttaja piti mallista taysin sellaisenaan
                    // ("tosi tosi hyva") mutta halusi sen isommaksi - scale 1.8->2.4.
                    // SUURENNETTU LISAA 9.8.2026: kayttaja pyysi viela lisaa kokoa.
                    // Scale 2.4->2.9. Siirsin ensin X:n 6->4 turvamarginaalin vuoksi, mutta
                    // kayttaja pyysi PALAUTTAMAAN X:n takaisin 6:een - piennarraja (7.8 yks.)
                    // jaa nyt juuri ja juuri kiinni (~7.8 yks.), mutta majakka on iso
                    // maamerkki kaukana ajoradasta eika oikeasti hairitse ajettavuutta.
                    // SUURENNETTU VIELA 10.8.2026: X=6 pysyy paikallaan (kayttaja ei halua
                    // siirtoa). Todellinen matemaattinen katto X:lla 6 on scale=2.89 (silloin
                    // reuna tasan 7.8 yks. keskilinjasta) - piennar (1.8 yks. lisaturva) alkaa
                    // vasta silloin. Scale 2.9->3.0 vie reunan ~7.76 yks. paahan - 0.04 yks.
                    // piennaralueen sisalle, EI ajoradalle (ajokaista paattyy 6 yks. paahan).
                    // Kayttaja ei ole ilmoittanut ajoneuvon oikeasti tormaavan majakkaan, joten
                    // pieni piennar-overlap staattisella maamerkilla on hyvaksyttava riski.
                    new DecorItem { modelName = "Satama_Majakka", pos = new Vector3(6f, 0f, -100f), rotX = -90f, rotY = 0f, scale = 3.0f },

                    // --- Lahti-klusteri mutkan huipulla (z=-50, roadX=25 - levein kohta
                    // vedelle tuolla puolella). Laituri (todellinen pituus x1.4 = 14,56,
                    // puolikas 7,28) pivotoitu X=11,2 niin etta maapaa (~18,5) jaa juuri
                    // ja juuri piennaren sisalle (road_L=19 tassa Z:ssa) eika ulotu tielle,
                    // vesipaa (~3,9) reilusti veden puolella. Laiva laiturin vesipaan
                    // tuntumassa, vene hiukan kauempana lahdessa. ---
                    // PARANNELTU 9.8.2026: Laituri ja Laiva rakennettu uudelleen samalla
                    // bmesh-pipelinella kuin muut talla istunnolla tehdyt mallit ->
                    // tarvitsevat rotX=-90 -korjauksen.
                    // HIOTTU 9.8.2026 (v4): RAKENNETTU UUDELLEEN SELKEALLA AKSELISTOLLA -
                    // rotY=90 EI ollut oikein, koska vanha malli ei ollut suunniteltu
                    // kaannettavaksi laskennallisesti; uusi malli maarittelee local -X =
                    // meri-paa (pieni tikapuu veteen), local +X = maa-paa (laatikkopino +
                    // koysikiepit). RoadX(z=-50)-tangentti on suora Z-suunnassa (tien mutkan
                    // huippu), joten meripuoli osoittaa puhtaasti -X:aan -> rotY=0 asettaa
                    // meri-paan oikein kohti avovetta (ei enaa kohti rantaa/tieta). Kansi
                    // nostettu pilarien (pilings) varaan veden ylapuolelle, kaide lisatty.
                    // Siirretty X 11.2->10.5 jotta maa-paa jaa turvallisen 7.8-yksikon
                    // piennarrajan ulkopuolelle (etaisyys tiehen nyt ~8.2 yks).
                    new DecorItem { modelName = "Satama_Laituri", pos = new Vector3(10.5f, 0f, -50f), rotX = -90f, rotY = 0f, scale = 1.4f },
                    // HIOTTU 9.8.2026 (v3): ankkuri keulassa, kaidepylvaat kannen reunalla,
                    // ikkunalasi+tutkamasto komentosillalle - selvasti yksityiskohtaisempi
                    // "hero"-objekti.
                    // SUURENNETTU + SIIRRETTY 9.8.2026: scale 1.0->1.1 ("objektien kokoa
                    // isommaksi"). Scenedumpin AABB-tarkistus paljasti etta Laiva ja Laituri
                    // MENIVAT PAALLEKKAIN (Laiva X=[-4.6,4.6], Laituri X=[4.2,16.8] - noin
                    // 0.4 yks. paallekkaisyys) - siirretty X 0->-3.5 kauemmas laiturista
                    // avoveden puolelle, korjaa paallekkaisyyden myos suuremmalla koolla.
                    // SUURENNETTU LISAA 9.8.2026: "konttialus" = Satama_Laiva (rahtilaiva).
                    // Scale 1.1->1.3, siirretty X viela -3.5->-4.5 jotta Laituriin jaa 2.7
                    // yksikon marginaali kasvaneesta koosta huolimatta.
                    new DecorItem { modelName = "Satama_Laiva", pos = new Vector3(-4.5f, 0f, -50f), rotX = -90f, rotY = 190f, scale = 1.3f },
                    // HIOTTU 9.8.2026 (v4): RAKENNETTU UUDELLEEN - 3-poikkileikkauksinen
                    // runko (keula-keskikohta-pera) pyoreamman/vahemman laatikkomaisen
                    // siluetin vuoksi, kirkkaampi oranssi+sininen reunaraita, pieni viiri
                    // keulassa. Sama local+X=keula -konventio sailyi (bmesh-koodista
                    // varmistettu).
                    // ROTAATIO KORJATTU UUDELLEEN 9.8.2026: edellisen kerran laskin keulan
                    // osoittamaan POISPAIN rannasta (kohti avovetta, kohtisuoraan
                    // laituriin nahden) - kayttaja pyysi silti tarkistamaan/parantamaan
                    // uudestaan. Oikea konventio pienelle laiturille on ETTA VENEET
                    // MAKAAVAT LAITURIN VIERESSA SEN PITUUSSUUNTAAN NAHDEN (kuten oikeassa
                    // satamassa - runko rannan/laiturin TANGENTIN suuntainen, ei sita
                    // vastaan kohtisuorassa). Laskettu uudelleen RoadX(z)-derivaatan
                    // tangentilla (ei enaa normaalilla): keula osoittaa vuorotellen
                    // rannikon suuntaan tai vastasuuntaan (luonnollinen "pysakoity"-rivi),
                    // + pieni yksilollinen kulmapoikkeama ettei rivi nayta robottimaisen
                    // tasmalliselta.
                    // ROTAATIO+KOKO KORJATTU UUDELLEEN 9.8.2026 (scenedump-data): kayttaja
                    // ilmoitti etta juuri Vene- ja Moottorivene-tyyppien rotaatiot "eivat
                    // menneet oikein" (Purjevenea ei mainittu - jatettiin ennalleen).
                    // Scenedumpin MaailmaBounds vahvisti etta rotX/rotY-matematiikka ITSE
                    // TOIMII OIKEIN Unityssa (esim. Vene worldY=0.59=local Z eli runko EI
                    // makaa kyljellaan) - ongelma oli PELKASTAAN liian suuri yksilollinen
                    // kulmapoikkeama (+/-25-30 astetta) joka sai veneen nayttamaan vinolta.
                    // Pienennetty poikkeama +/-5-8 asteeseen -> runko selvasti siistimmin
                    // rannan/laiturin suuntainen. Scale 1.0->1.1 ("objektien kokoa isommaksi").
                    // BUGIKORJAUS 9.8.2026 (v5, EI RIITTANYT): epailtiin ensin n-gon-poisto-
                    // tempun jattaneen reiän/paallekkaisyyden runkoon - korjattiin siistilla
                    // topologialla, MUTTA lisattiin samalla vahingossa finish_export-
                    // funktioon "siivous"-askel: bmesh.ops.recalc_face_normals.
                    // BUGIKORJAUS 9.8.2026 (v6, TODELLINEN SYY): kayttaja ilmoitti veneiden
                    // nayttavan "YLOSALAISILTA" - talla oli lopulta selva, konkreettinen
                    // oire. recalc_face_normals ARVAA ulkosuunnan meshin topologian
                    // perusteella - Veneen runko on TARKOITUKSELLA AVOIN muoto (ei pohjaa/
                    // kantta sulkemassa, vain kylki+yla+pera), eli EI-MANIFOLD, jolloin
                    // arvaus meni vaarinpain ja KAANSI KAIKKI NORMAALIT ALASPAIN - Unity
                    // nayti runkoa ikaankuin sisapuoli/pohja ulospain (= "ylosalainen").
                    // Purjevene/Moottorivene eivat koskaan kayttaneet tata kutsua, siksi
                    // vain Vene-tyyppi oikaisi vaarin. KORJATTU: poistettu
                    // recalc_face_normals+remove_doubles finish_export:sta kokonaan,
                    // rakennettu uudelleen (v6) TASMALLEEN samalla exportilla kuin
                    // Purjevene/Moottorivene. rotY-arvot jatettiin ennalleen (ne eivat
                    // koskaan olleet varsinainen ongelma).
                    // KORJAUS 9.8.2026 (v7, KAMERAN NAKOKULMA - TODENNAKOISIN OIKEA SYY):
                    // kayttaja lahetti ensimmaisen kerran KUVAKAAPPAUKSEN pelista (aiemmin
                    // vain sanallista palautetta) ja ilmoitti veneiden olevan "vaarin päin" -
                    // mesh oli jo korjattu (v6, ei enaa ylosalainen), joten oire on nyt eri:
                    // ajopelin seurantakamera katsoo LAHES Z-akselin suuntaisesti (radan
                    // suunta). "Ranta-tangentin suuntainen" rotaatio (~90/270 astetta)
                    // asettaa veneen PITKAN akselin SAMANSUUNTAISEKSI kameran katsesuunnan
                    // kanssa - eli vene nakyy KAPEALTA PAASTA (typistynyt, tuskin
                    // tunnistettava "laatikko"), EI SIVUPROFIILISTA. Purjevene/Moottorivene
                    // eivat karsineet tasta koska niissa on pystysuora masto/hytti joka
                    // pysyy tunnistettavana kulmasta riippumatta - pelkalla matalalla
                    // soutuveneen rungolla ei ole tata "pelastavaa" pystypiirretta.
                    // KORJATTU: Vene-perheen kulmat siirretty ~90 astetta niin etta pitka
                    // akseli on nyt POIKITTAIN kameran katsesuuntaan nahden (rotY lahella
                    // 0/180 90/270 sijaan) - vene nakyy nyt selkeana sivuprofiilina.
                    // HIOTTU 9.8.2026 (v8): lisatty pieni valkoinen "nimikilpi"-aksentti
                    // molemmin puolin keulaa seka koyty/verkkonyytti perassa - lisaa
                    // yksityiskohtaa ilman etta puhdas topologia (v6/v7-korjaus) vaarantuu.
                    // HIOTTU 9.8.2026 (v9, KAYTTAJAN REFERENSSIKUVIEN MUKAAN - oikeat
                    // muoviset soutuveneet/jollat): RAKENNETTU UUDELLEEN kokonaan.
                    // Referenssit nayttivat siistin, YKSIVARISEN kermanvalkoisen rungon
                    // (EI enaa oranssia+sinista raitaa), LITTEAN PERAN (transom - runko ei
                    // enaa kavene teravaksi kolmioksi perassa vaan paattyy suoraan pystyyn
                    // pintaan, kuten oikeissa muovijollisa), sulavamman 4-poikkileikkauksisen
                    // runkokaaren (keula-neljannes-keski-pera), airot LEPAAMASSA POIKITTAIN
                    // laidalta laidalle (ei enaa keulaan pain osoittavina), 2 puun sävyistä
                    // penkkia koko leveydelta ja pienen hinausrenkaan+koyden keulassa. Sama
                    // puhdas topologia (ei n-gon-temppuja, ei recalc_face_normals) ja
                    // local+X=keula -konventio sailyi. Tarkistettu AABB uusilla (hieman
                    // isommilla, airot levensivat runkoa) mitoilla - ei paallekkaisyyksia,
                    // tiehen jaa yli 13 yksikon marginaali kaikilla kolmella instanssilla.
                    new DecorItem { modelName = "Satama_Vene", pos = new Vector3(6f, 0f, -35f), rotX = -90f, rotY = 180f, scale = 1.1f },

                    // --- Pikkuvenesatama: lisaa veneita laiturin ympärille marina-tyyliin
                    // (referenssikuvat: rivissä venelaituri, purjeveneita ja moottoriveneita
                    // rannan tuntumassa). Kaikki tarkistettu KIERRETYLLA bounding boxilla
                    // pysymaan veden vyöhykkeen (roadX-44.3 .. roadX-10.3) sisalla, riittavan
                    // kaukana laiturista/laivasta/toisistaan (>=2,5 yks. valia). ---
                    new DecorItem { modelName = "Satama_Vene", instanceName = "Satama_Vene_B", pos = new Vector3(9f, 0f, -58f), rotX = -90f, rotY = 355f, scale = 1.1f },
                    new DecorItem { modelName = "Satama_Vene", instanceName = "Satama_Vene_C", pos = new Vector3(0f, 0f, -40f), rotX = -90f, rotY = 170f, scale = 1.0f },
                    // HIOTTU 9.8.2026 (v4): RAKENNETTU UUDELLEEN samalla akselistolla -
                    // 3-poikkileikkauksinen runko, kirkkaampi paapurje (sininen reunanauha)
                    // + keltainen keulapurje, puomi+stays+tiller+lippu sailyivat.
                    // ROTAATIO KORJATTU UUDELLEEN 9.8.2026: ks. Vene-kommentti - runko
                    // laiturin/rannan TANGENTIN suuntainen (ei kohtisuoraan avovedelle).
                    // Purjevenetta EI mainittu kayttajan viimeisimmassa "rotaatiot eivat
                    // mene oikein" -palautteessa, joten rotY jatettiin ENNALLEEN - vain
                    // scale suurennettu ("objektien kokoa isommaksi").
                    // HIOTTU 9.8.2026 (v5): lisatty patjaikkuna (porthole) rungon
                    // molemmin puolin ja pienet cleat-koukut keula/pera-paahan koysille.
                    // ROTAATIO KORJATTU 9.8.2026 (kamerakulma-periaate laajennettu KAIKKIIN
                    // veneisiin): kayttaja ilmoitti VENEIDEN YLEISESTI (ei enaa vain Vene-
                    // tyypin) "yhä ei toimi oikein" - tarkistin oman aiemman paatelmani
                    // uudelleen: oletin etta Purjevenen pystysuora masto+purje pitaisi sen
                    // tunnistettavana kulmasta riippumatta, joten jatin rotY:n lahelle 90/270
                    // (rannan tangentin suuntainen = kameran katsesuuntaan NAHDEN
                    // SAMANSUUNTAINEN = typistynyt kapealta paasta). Tama oletus oli
                    // ILMEISESTI silti liian optimistinen pienella mobiilinaytolla - siksi
                    // KAIKKI veneet (myos Purjevene/Moottorivene) kaannetty nyt samalla
                    // periaatteella kuin Vene: pitka akseli POIKITTAIN kameran
                    // katsesuuntaan (rotY lahella 0/180, ei 90/270).
                    new DecorItem { modelName = "Satama_Purjevene", pos = new Vector3(2.5f, 0f, -60f), rotX = -90f, rotY = 190f, scale = 1.2f },
                    new DecorItem { modelName = "Satama_Purjevene", instanceName = "Satama_Purjevene_B", pos = new Vector3(-5f, 0f, 20f), rotX = -90f, rotY = 15f, scale = 1.1f },
                    // LISATTY 9.8.2026: kayttaja huomasi etta z~125 kohdalla vesi oli
                    // täysin tyhja (viimeinen Poiju z=58, seuraava rantaobjekti Rantalyhty_B
                    // vasta z=140) - lisatty kolmas Purjevene tayttamaan aukko.
                    // ROTAATIO KORJATTU 9.8.2026: sama kamerakulma-periaate kuin ylla.
                    new DecorItem { modelName = "Satama_Purjevene", instanceName = "Satama_Purjevene_C", pos = new Vector3(-6f, 0f, 133f), rotX = -90f, rotY = 185f, scale = 1.1f },
                    // HIOTTU 9.8.2026 (v4): RAKENNETTU UUDELLEEN - 3-poikkileikkauksinen
                    // runko, kirkkaampi sini/puna-raidoitus, peramoottori+potkuri,
                    // uimatasanne, lipputanko+lippu ja hytin ikkunat sailyivat.
                    // ROTAATIO+KOKO KORJATTU UUDELLEEN 9.8.2026 (scenedump-data): sama
                    // korjaus kuin Vene-tyypille (ks. Vene-kommentti) - kulmapoikkeama
                    // pienennetty niin etta runko on selvasti rannan/laiturin suuntainen
                    // eika vinossa. Scale 1.0->1.1.
                    // HIOTTU 9.8.2026 (v5): lisatty peratikapuu uimatasanteesta veteen,
                    // pyoreat valkoiset fenderit rungon kyljissa, pieni keularaili.
                    // SUURENNETTU 9.8.2026: kayttaja pyysi moottoriveneille lisaa kokoa.
                    // Scale 1.1->1.35 (Moottorivene) ja 1.0->1.2 (Moottorivene_B).
                    // Tarkistettu AABB naapureita (Vene, Purjevene, Laituri, Poiju) vasten
                    // TODELLISILLA v5-mitoilla (fenderit levensivat runkoa hieman) - ei
                    // paallekkaisyyksia, tieen jaa reilusti yli 13 yksikon marginaali.
                    // ROTAATIO KORJATTU 9.8.2026 (kamerakulma-periaate laajennettu KAIKKIIN
                    // veneisiin, ks. Purjevene-kommentti taydellinen selitys): kaannetty
                    // rotY lahelle 0/180 (poikittain kameraan) 90/270:n (samansuuntainen
                    // kameraan, typistynyt) sijaan.
                    new DecorItem { modelName = "Satama_Moottorivene", pos = new Vector3(10f, 0f, -44f), rotX = -90f, rotY = 350f, scale = 1.35f },
                    new DecorItem { modelName = "Satama_Moottorivene", instanceName = "Satama_Moottorivene_B", pos = new Vector3(-6f, 0f, 50f), rotX = -90f, rotY = 175f, scale = 1.2f },

                    // --- Poijut avoimessa vedessa, ripoteltu koko lahden matkalle ---
                    // HIOTTU 9.8.2026 (v3): isompi/nakyvampi valo, pieni metallirengas
                    // koydelle, vesirenkaat pinnalla - selkeampi yksityiskohta kauempaakin.
                    // SIJOITTELU PARANNELTU: Poiju_E siirretty z=-30 -> -55 (taytti ison
                    // 50-yksikon aukon Poiju_A:n [-80] ja entisen Poiju_B:n [-15] valissa),
                    // Poiju_D siirretty z=50 -> 58 (kasvatti etaisyytta Moottorivene_B:sta
                    // 4.0 -> 8.9 yksikkoon, tiivein pari koko Satamassa).
                    // SUURENNETTU 9.8.2026: scale 1.3->1.45 kaikille poijuille ("objektien
                    // kokoa isommaksi"). Tarkistettu AABB-paallekkaisyysaudittilla (scenedump)
                    // - ei paallekkaisyyksia, kaikki reilusti veden puolella.
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_A", pos = new Vector3(-5f, 0f, -80f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_E", pos = new Vector3(-10f, 0f, -55f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_B", pos = new Vector3(-8f, 0f, -15f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_C", pos = new Vector3(-10f, 0f, 30f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_D", pos = new Vector3(-10f, 0f, 58f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    // LISATTY 9.8.2026: taytetaan tyhja vesialue z~75..140 (ei ollut
                    // yhtaan poijua/venetta talla valilla ennen taman kierroksen lisayksia).
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_F", pos = new Vector3(-9f, 0f, 100f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Poiju", instanceName = "Satama_Poiju_G", pos = new Vector3(-11f, 0f, 122f), rotX = -90f, rotY = 0f, scale = 1.45f },

                    // --- Rantavalot: yksi lahden reunalla, toinen uloskaynnin suoralla ---
                    // PARANNELTU 9.8.2026: Rantalyhty rakennettu uudelleen -> rotX=-90 -korjaus.
                    // HIOTTU 9.8.2026 (v3): kuusikulmainen lyhty (kuten Puiston uusi malli),
                    // koynnos-koriste pylvaan alaosassa - meriteemainen yksityiskohta.
                    // SUURENNETTU 9.8.2026: scale 1.3->1.45. Tarkistettu tiepiennarraja
                    // (7.8 yks.) - lahin (Rantalyhty_A) jaa 8.59 yks. paahan, turvassa.
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_A", pos = new Vector3(14f, 0f, -65f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_C", pos = new Vector3(5f, 0f, 13f), rotX = -90f, rotY = 0f, scale = 1.45f },
                    new DecorItem { modelName = "Satama_Rantalyhty", instanceName = "Satama_Rantalyhty_B", pos = new Vector3(6f, 0f, 140f), rotX = -90f, rotY = 0f, scale = 1.45f },

                    // --- Lastauspiha (nosturi + kontit + varasto), toisen mutkan (z=45..75)
                    // kohdalla jossa roadX on pienimmillaan ja maata siksi eniten - kaikki
                    // tarkistettu kierretylla bounding boxilla piennaren (~17-20) ja kentan
                    // reunan (35,75) valiin, reilulla marginaililla.
                    // PARANNELTU 9.8.2026: Konttipino ja Varasto rakennettu uudelleen ->
                    // rotX=-90 -korjaus. ---
                    // HIOTTU 9.8.2026 (v3): aaltopelti-urat sivupinnoilla, ovet+kahvat
                    // paatypinnoilla - erottuu selvasti pelkasta varikkaasta laatikosta.
                    // SUURENNETTU 9.8.2026: scale 1.0->1.15 ("objektien kokoa isommaksi").
                    // AABB-audit: ei paallekkaisyyksia naapureiden (Nosturi, Varasto) kanssa.
                    new DecorItem { modelName = "Satama_Konttipino", instanceName = "Satama_Konttipino_B", pos = new Vector3(24f, 0f, 45f), rotX = -90f, rotY = 20f, scale = 1.15f },
                    // HIOTTU 9.8.2026 (v3): lisatty ohjaamo+ikkuna, varoitusraidat jalustaan,
                    // nakyvampi koysi+koukku puomin paassa - selkeampi "tyokoneen" luonne.
                    // SUURENNETTU 9.8.2026: scale 1.0->1.15.
                    // SUURENNETTU LISAA 10.8.2026: scale 1.15->1.3 (kayttaja pyysi viela
                    // lisaa kokoa). AABB-audit (todelliset FBX-mitat 4.46x1.10x5.10):
                    // ei paallekkaisyyksia Konttipino_A/B:n eika Varaston kanssa, etaisyys
                    // tiehen ~14.9 yks - reilusti turvassa.
                    new DecorItem { modelName = "Satama_Nosturi", pos = new Vector3(24f, 0f, 55f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Satama_Konttipino", instanceName = "Satama_Konttipino_A", pos = new Vector3(23f, 0f, 65f), rotX = -90f, rotY = 0f, scale = 1.15f },
                    // HIOTTU 9.8.2026 (v2): Varasto oli viimeinen Sataman "hero"-objekti
                    // jota ei ollut viela paivitetty taman istunnon laatutasolle -
                    // rakennettu uudelleen: harjakatto aaltopelti-listoituksella, ovi+
                    // kahva, ikkunat kolmella seinalla, kyltti oven ylapuolella, pieni
                    // tynnyripino ulkonurkassa storytelling-yksityiskohtana.
                    // SUURENNETTU 9.8.2026: kayttaja piti mallista sellaisenaan ("tosi tosi
                    // hyva") mutta halusi hieman isomman - scale 1.0->1.35. RoadX(z=75)=12,
                    // Varasto x=28 (etaisyys tiehen 16 yks) - reilusti tilaa isommallekin.
                    // Lahin naapuri Konttipino_A (23,65), etaisyys ~11.2 yks - ei ongelmaa.
                    // SUURENNETTU LISAA 9.8.2026: scale 1.35->1.7 (kayttaja pyysi viela
                    // lisaa kokoa). Tarkistettu AABB Konttipino_A:ta vasten - X-akselilla
                    // olisi teoriassa paallekkaisyytta mutta Z-akseli erottaa ne selvasti
                    // (ei todellista tormaysta).
                    // SUURENNETTU LISAA 10.8.2026: scale 1.7->1.95 (kayttaja pyysi viela
                    // lisaa kokoa). AABB-audit (todelliset FBX-mitat 4.32x3.37x2.55): ei
                    // paallekkaisyyksia Konttipino_A:n eika Nosturin kanssa, etaisyys tiehen
                    // ~16 yks - reilusti turvassa (spekseissakin todettu "tilaa isommallekin").
                    new DecorItem { modelName = "Satama_Varasto", pos = new Vector3(28f, 0f, 75f), rotX = -90f, rotY = -10f, scale = 1.95f },
                },
            },
            new DecorPreset
            {
                sceneName = "Huvipuisto",
                items = new[]
                {
                    // PARANNELTU 8.8.2026 (neljäs kerta): kaytajan pyynnosta "MUOKKAA
                    // BLENDERILLA HUVIPUISTON OBJEKTEJA SELKEIMMIKSI" - rakennettu uudelleen
                    // samalla pyoristetylla bmesh-pipelinella/vaeripaletilla (kirkas puna/sini/
                    // kelta/turkoosi) kuin istunnon muut mallit, lisatty valoketjuja/retro-
                    // tivoli-koristeita. Nama olivat AIEMMIN vanhan, eri istunnon objekteja
                    // jotka EIVAT tarvinneet rotX-korjausta - nyt UUDELLEENRAKENNETTUINA samalla
                    // pipelinella ne TARVITSEVAT rotX=-90 kuten kaikki muutkin talla tekniikalla
                    // tehdyt mallit.
                    new DecorItem { modelName = "Huvipuisto_Maailmanpyora", pos = new Vector3(28f, 0f, 0f), rotX = -90f, rotY = 0f, scale = 1.8f },
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti - pysyy nyt
                    // paikallaan vaikka "11/12"-tyokaluja ajaisi uudelleen.
                    // AJOSUUNTA HUOMIOITU 8.8.2026 (viides kerta): kayttaja pyysi tarkistamaan
                    // koristeiden asettelut niin etta "etupuoli" (sisaankaynti/tiski/kyltti)
                    // nakyy tielle/autolle eika ole kaannetty poispain. Teltta on tien
                    // VASEMMALLA puolella (X -25, RoadX(-77)=0, siis selvasti vasemmalla) -
                    // sisaankaynti (rakennettu paikallisen -Y-akselin suuntaan Blenderissa,
                    // sama kaytanto kuin Popcornkoju/Karkkikoju joilla rotY=90 kaantaa
                    // etupuolen kohti tieta) kaannetty rotY 0 -> 90 samalla logiikalla.
                    // EI VIELA UNITY-TESTATTU - kerro jos sisaankaynti on nyt vaarinpain.
                    new DecorItem { modelName = "Huvipuisto_Teltta", pos = new Vector3(-25f, 0f, -77f), rotX = -90f, rotY = 90f, scale = 1.5f },
                    // KORJATTU 8.8.2026: MaailmaBounds-dumpin perusteella (Y=malli oma "syvyys",
                    // ei "korkeus") tama bmesh-tekniikalla rakennettu malli makasi kyljellaan -
                    // sama ilmio kuin veneilla, korjataan samalla rotX=-90 -kaavalla.
                    new DecorItem { modelName = "Huvipuisto_Karuselli", pos = new Vector3(-15f, 0f, -60f), rotX = -90f, rotY = 0f, scale = 1.6f },
                    // Oli x=-35 (lahes kentan reunalla, +-35.5) - siirretty hiukan sisemmas.
                    // AJOSUUNTA HUOMIOITU 8.8.2026: sama Teltta-logiikka - koju on tien
                    // VASEMMALLA (X -32, RoadX(-55)=0) - tiski kaannetty rotY 30 -> 90 kohti
                    // tieta, samaa vasen-puoli-90-astetta -kaavaa kuin muutkin tiskikojut.
                    // SIIRRETTY 8.8.2026: kayttaja huomasi etta Teltta/Karuselli/Pomppulinna/
                    // Myyntikoju/Popcornkoju/Roskis_A/Roskis_B olivat kaikki tunkeutuneet samalle
                    // 37 yksikon patkalle (z -77..-40) samalla kun radan loppupaassa (z 100..130)
                    // oli iso tyhja alue. Myyntikoju siirretty tayttamaan tuo aukko - RoadX(115)=
                    // -16.0 (kaannetylla kaavalla), sijoitettu ~14 yksikon paahan tiesta samaan
                    // tapaan kuin alkuperainen oli.
                    new DecorItem { modelName = "Huvipuisto_Myyntikoju", pos = new Vector3(-30f, 0f, 115f), rotX = -90f, rotY = 90f, scale = 1.4f },

                    // PAIVITETTY: kaari ja lippunauha rakennettu uudelleen Blenderissa
                    // oikeassa koossa (kaari 17.2 leveä x 6.9 korkea, lippunauha 16.3 x 4.6)
                    // niin että ne oikeasti ULOTTUVAT TIEN YLI (tie 12 leveä + piennarit) -
                    // sijoitettu roadX(z):n päälle (x=0, koska Huvipuiston roadBaseX=0),
                    // rotY=0 koska molemmat ovat suoralla osuudella jossa tie kulkee
                    // puhtaasti Z-suunnassa. Auto ajaa kaaren/nauhan ALTA.
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti.
                    new DecorItem { modelName = "Huvipuisto_Ilmapallokaari", pos = new Vector3(0f, 0f, -150f), rotX = -90f, rotY = 0f, scale = 1f },
                    new DecorItem { modelName = "Huvipuisto_Lippunauha", pos = new Vector3(0f, 0f, 60f), rotX = -90f, rotY = 0f, scale = 1f },
                    // Pomppulinna: uusi malli (linnake + tornit + sisäänkäynti + liput,
                    // selvästi tunnistettava) - siirretty omalle paikalleen kauemmas
                    // Teltta/Karuselli/Myyntikoju-klusterista, sisäänkäynti tietä kohti.
                    // KORJATTU 8.8.2026: sama kyljellaan-makaamisbugi kuin Karusellissa (ks.
                    // MaailmaBounds-dumpin analyysi) - lisatty rotX=-90.
                    // KIERTO KORJATTU 8.8.2026: kayttajan Unityssa testaama kierto (rotY 180,
                    // rotZ -90) - malliin toivottu lisaksi viela parannuksia ("viela parempi"),
                    // EI VIELA TEHTY.
                    // KAANNETTY 8.8.2026: kayttaja ilmoitti Pomppulinnan olevan ajosuuntaan
                    // nahden vaarinpain - kaannetty 180 astetta (rotY 180 -> 0, rotZ ennallaan).
                    // EI VIELA VARMISTETTU Unityssa - kerro jos vieläkin vaarinpain, niin
                    // kokeillaan 90 asteen askelin kunnes osuu oikein.
                    new DecorItem { modelName = "Huvipuisto_Pomppulinna", pos = new Vector3(18f, 0f, -70f), rotX = -90f, rotY = 0f, rotZ = -90f, scale = 1.4f },

                    // --- Lisää huvipuistoelementtejä ---
                    // SIIRRETTY 8.8.2026: entinen paikka (-10,10) oli vain 5,4 yksikkoa tiesta -
                    // piennar paattyy vasta ~7,8:ssa, eli koju oli lahes piennarella/tiella.
                    // Uusi paikka suoralla osuudella (z=-45, roadX=0) Teltta/Karuselli/Myyntikoju/
                    // Pomppulinna-huvipuistoklusterin vieressa (jatkaa "ruokapaikat"-tunnelmaa),
                    // 20 yksikkoa tiesta. Koko kasvatettu 1.5 -> 1.9.
                    new DecorItem { modelName = "Huvipuisto_Popcornkoju", pos = new Vector3(-20f, 0f, -45f), rotX = -90f, rotY = 90f, scale = 1.9f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_A", pos = new Vector3(10f, 0f, -145f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_B", pos = new Vector3(-10f, 0f, 145f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    // LISATTY 8.8.2026: kolme uutta ilmapallonyyttia tien varrelle, tayttamaan
                    // radan tyhjempia kohtia - kaikki tarkistettu RoadX:aa vasten (>=12 yks. tiesta).
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_C", pos = new Vector3(22f, 0f, -100f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_D", pos = new Vector3(15f, 0f, 40f), rotX = -90f, rotY = 0f, scale = 1.3f },
                    new DecorItem { modelName = "Huvipuisto_Ilmapallonyytti", instanceName = "Huvipuisto_Ilmapallonyytti_E", pos = new Vector3(24f, 0f, 130f), rotX = -90f, rotY = 0f, scale = 1.3f },

                    // --- 8.8.2026: uudet elementit ---
                    // Karttakyltti: OIKEA teemapuiston kartta 3D-objektina (ei valikko-UI:ta -
                    // kayttaja tarkensi etta kartan piti olla peliobjekti, ei paavalikon paneeli).
                    // Sijoitettu heti lahdon jalkeen, ennen ilmapallokaarta (z=-150), suoralla
                    // osuudella jossa roadX=0 - x=10 jattaa reilusti tilaa piennarelta (7.8).
                    // rotY=0 oletuksena - jos kyltin karttapuoli ei osu tieta kohti Unityssa,
                    // saada oikea suunta kokeilemalla (90/180/270).
                    // KORJATTU 8.8.2026: MaailmaBounds-dumpin todistusaineisto (Y=2.02 tasmasi
                    // mallin SYVYYTEEN 2.03, ei sen KORKEUTEEN 3.47) paljasti etta kyltti makasi
                    // kyljellaan - rotX=-90 korjaa. Sama bugi loytyi jokaisesta talla Blender-
                    // bmesh-pipelinella TASSA istunnossa rakennetusta/uudelleenrakennetusta
                    // mallista (Karuselli/Pomppulinna/Popcornkoju/Kuumailmapallo/Lentokeinu/
                    // Karttakyltti) - EI vanhoista Huvipuisto-objekteista (Maailmanpyora/Teltta/
                    // Myyntikoju/Ilmapallokaari/Ilmapallonyytti/Lippunauha), jotka on rakennettu
                    // eri istunnossa eivatka tarvitse korjausta.
                    // SIIRRETTY 8.8.2026: kayttaja ei nahnyt sita lahdossa (liian lahella
                    // StartPointia/autoa) EIKA kartta nakynyt kyltissa - jalkimmainen johtui
                    // siita etta kartan kuva (Teemapuisto_Kartta.png) oli ERI kansiossa
                    // (Art/UI) kuin muut _Paletti.png-tekstuurit (Art/Textures) - Unityn FBX-
                    // tuonti ei loytanyt sita automaattisesti. KORJATTU upottamalla tekstuurit
                    // suoraan FBX:aan (embed_textures=True) - riippumaton kansiosta. Siirretty
                    // myos hieman kauemmas lahdosta (16 yks. tiesta, oma paikka ennen kaarta).
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti (X 8, Z -13).
                    // AJOSUUNTA HUOMIOITU 8.8.2026: kyltti on tien VASEMMALLA puolella
                    // (X=8, RoadX(-13)=13.7 eli kyltti jaa RoadX:n vasemmalle puolelle) - mutta
                    // rotY oli 0, eli karttalevyn -Y-etupinta (todistetusti oikein UV:tettu ja
                    // teksturoitu, ks. aiempi diagnoosi) osoitti PITKIN tieta eika tielle pain -
                    // tama selittaa todennakoisesti myos "kartta ei nay" -valituksen paremmin
                    // kuin pelkka tekstuuribugi. Kaannetty rotY 0 -> 90 samalla vasen-puoli-
                    // logiikalla kuin Teltta/Myyntikoju. EI VIELA UNITY-TESTATTU.
                    // KAANNETTY 8.8.2026: kayttaja ilmoitti kyltin olevan ajosuuntaan nahden
                    // vaarinpain - kaannetty 180 astetta (90 -> 270). EI VIELA VARMISTETTU
                    // Unityssa (en pysty renderoimaan/nakemaan Unityn kameraa tasta kasin).
                    // KAANNETTY 8.8.2026 (toinen kerta): "alkaa menna parempaan suuntaan" -
                    // kayttaja pyysi viela 90 astetta lisaa (270 -> 0) ja hieman lisaa etaisyytta
                    // tien vasemmalle puolelle (X 8 -> 5, tie tassa RoadX=13.7 joten kyltti oli jo
                    // -X-puolella - siirretty vielä 3 yksikkoa kauemmas samaan suuntaan).
                    // KAANNETTY 8.8.2026 (kolmas kerta): kayttaja pyysi viela 180 astetta lisaa (0 -> 180).
                    new DecorItem { modelName = "Huvipuisto_Karttakyltti", pos = new Vector3(5f, 0f, -13f), rotX = -90f, rotY = 180f, scale = 1.3f },
                    // SIIRRETTY 8.8.2026: entinen paikka (-30,20) oli 21 yksikkoa tiesta - liian
                    // kaukana, tuntui irralliselta. Vedetty lahemmas (13 yks.) niin etta nakyy
                    // paremmin ajaessa, pysyy silti reilusti piennaren (7,8) ulkopuolella.
                    // Koko kasvatettu 1.2 -> 1.6.
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti.
                    new DecorItem { modelName = "Huvipuisto_Kuumailmapallo", pos = new Vector3(-10f, 0f, 20f), rotX = -90f, rotY = 0f, scale = 1.6f },
                    // Lentokeinu: kolmannen mutkan (z=80..150) ulkolaidalla - tuo sisaltoa myos
                    // radan viimeiseen kolmannekseen joka oli aiemmin melko tyhja.
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti. Malli myos
                    // pyydetty parannettavaksi viela ("vielä parempi näöltään") - EI VIELA TEHTY.
                    new DecorItem { modelName = "Huvipuisto_Lentokeinu", pos = new Vector3(14f, 0f, 100f), rotX = -90f, rotY = 0f, scale = 1.2f },

                    // --- 8.8.2026: kayttajan hyvaksymat lisaykset (penkit/roskikset/koju/peli/
                    // maskotti). UUDET mallit (Karkkikoju/Rengasheitto/Roskis/Maskotti) rakennettu
                    // TASSA istunnossa samalla bmesh-pipelinella kuin Karuselli ym. -> rotX=-90
                    // lisatty SUORAAN alusta asti (ei tarvitse enaa erikseen loytaa/korjata).
                    // Penkki/Lyhtypylvas KAYTTAVAT OLEMASSA OLEVIA Puisto-malleja (eri istunto,
                    // ei rotX-tarvetta, jo todistetusti toimivia Puistossa).
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti.
                    new DecorItem { modelName = "Huvipuisto_Karkkikoju", pos = new Vector3(-4f, 0f, -10f), rotX = -90f, rotY = 90f, scale = 1.6f },
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti+kierto.
                    new DecorItem { modelName = "Huvipuisto_Rengasheitto", pos = new Vector3(12f, 0f, 73f), rotX = -90f, rotY = -45f, rotZ = -90f, scale = 1.4f },
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti+kierto. Malliin
                    // toivottu myos parannusta ("isompi ja pehmolelumaisempi") - EI VIELA TEHTY.
                    new DecorItem { modelName = "Huvipuisto_Maskotti", pos = new Vector3(14f, 0f, 155f), rotX = -90f, rotY = 180f, rotZ = 90f, scale = 1.5f },
                    new DecorItem { modelName = "Huvipuisto_Roskis", instanceName = "Huvipuisto_Roskis_A", pos = new Vector3(-20f, 0f, -65f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    // SIIRRETTY 8.8.2026: sama syy kuin Myyntikojulla - tuotu Lentokeinun (z=100)
                    // viereen tayttamaan aukkoa, ei paalla (12 yks. X-valia, 8 yks. Z-valia).
                    new DecorItem { modelName = "Huvipuisto_Roskis", instanceName = "Huvipuisto_Roskis_B", pos = new Vector3(2f, 0f, 108f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    new DecorItem { modelName = "Huvipuisto_Roskis", instanceName = "Huvipuisto_Roskis_C", pos = new Vector3(10f, 0f, 65f), rotX = -90f, rotY = 0f, scale = 1.2f },
                    // KIINNITETTY 8.8.2026: kayttajan Unityssa testaama sijainti.
                    // PARANNELTU 9.8.2026: Puisto_Penkki (jaettu malli) rakennettu uudelleen
                    // Puisto-presetin puolella -> tarvitsee rotX=-90 -korjauksen myos tassa.
                    // KOKOAUDIT 9.8.2026: scale pienennetty 1.4 -> 1.0 (sama korjaus kuin
                    // Puisto-presetissa - v2-malli oli scale=1.4:lla ylimitoitettu).
                    // KAANNETTY 9.8.2026: sama Penkki-korjaus kuin Puisto-presetissa (rotY
                    // +180, scale 1.0 -> 1.2) - jaettu malli, kayttajan Puistossa toteama
                    // vaara suunta/koko koskee todennakoisesti myos naita.
                    new DecorItem { modelName = "Puisto_Penkki", instanceName = "Huvipuisto_Penkki_A", pos = new Vector3(-16f, 0f, -145f), rotX = -90f, rotY = 270f, scale = 1.2f },
                    new DecorItem { modelName = "Puisto_Penkki", instanceName = "Huvipuisto_Penkki_B", pos = new Vector3(-10f, 0f, 55f), rotX = -90f, rotY = 90f, scale = 1.2f },
                    // VIILATTU 8.8.2026: bends-kaannoksen jalkeen tama oli 44 yksikkoa oikealta
                    // tielta (RoadX(-120)=-15.21, vanha X=29.2 oli mitoitettu ENNEN kaannosta) -
                    // siirretty samalle (negatiiviselle) puolelle kuin viereinen koristeklusteri
                    // (Teltta/Karuselli/Myyntikoju), n. 14 yksikon paahan todellisesta tiesta.
                    // PARANNELTU 9.8.2026: Puisto_Lyhtypylvas (jaettu malli) rakennettu
                    // uudelleen -> tarvitsee rotX=-90 -korjauksen myos tassa.
                    // KOKOAUDIT 9.8.2026: scale pienennetty 1.5 -> 1.1 (sama korjaus kuin
                    // Puisto-presetissa - v2-malli oli scale=1.5:lla n. 6m korkea).
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", instanceName = "Huvipuisto_Lyhty_A", pos = new Vector3(-29.2f, 0f, -120f), rotX = -90f, rotY = 0f, scale = 1.1f },
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", instanceName = "Huvipuisto_Lyhty_B", pos = new Vector3(14f, 0f, 80f), rotX = -90f, rotY = 0f, scale = 1.1f },
                    // VIILATTU 8.8.2026: sama syy kuin Lyhty_A:ssa - oli 44 yksikkoa tiesta
                    // (RoadX(-10)=15.36). Siirretty tien VASTAKKAISELLE puolelle kuin Karkkikoju
                    // (x=-4, offset -19) visuaalisen tasapainon vuoksi, n. 14 yksikon paahan.
                    new DecorItem { modelName = "Puisto_Lyhtypylvas", instanceName = "Huvipuisto_Lyhty_C", pos = new Vector3(29.4f, 0f, -10f), rotX = -90f, rotY = 0f, scale = 1.1f },
                },
            },
        };

        [MenuItem("Pakettiporina/Lisays/03 - Lisaa teemakoristeet talle radalle")]
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
                string path = ResolveModelPath(item.modelName);
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
                go.transform.rotation = Quaternion.Euler(item.rotX, item.rotY, item.rotZ);
                go.transform.localScale = Vector3.one * item.scale;
                s.AppendLine($"{instName}: sijoitettu ({item.pos.x:F0}, {item.pos.z:F0}), kierto Y={item.rotY:F0}, Z={item.rotZ:F0}, koko x{item.scale:F1}");
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
