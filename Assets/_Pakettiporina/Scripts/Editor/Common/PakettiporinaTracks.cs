#if UNITY_EDITOR
using UnityEngine;

namespace Pakettiporina.EditorTools
{
    // YKSI YHTEINEN ratatietolahde. Ennen tama data (bends/baseX/startZ/finishZ/
    // groundScaleZ/boostZ/puddleZ/cones) oli kasin kopioitu kuuteen eri tiedostoon
    // (Setup/Elements/Stars/RoadProfile/SceneDump/Decor-kommentit) - se aiheutti
    // 6.8.2026 Satama-bugin (auton spawn jai vahingossa X=0:aan koska roadBaseX oli
    // paivitetty vain viiteen kuudesta kopiosta). Nyt KAIKKI nailla tiedoilla
    // toimivat tyokalut (PakettiporinaSetup, -Elements, -Stars, -RoadProfile,
    // -SceneDump) lukevat saman TrackDef-taulukon tasta yhdesta paikasta.
    //
    // UUDEN RADAN LISAAMINEN: lisaa yksi TrackDef ALL-taulukkoon alla. Se riittaa -
    // mitaan muuta tiedostoa ei tarvitse koskea (paitsi PakettiporinaDecor.cs:n oma
    // koristelista, joka on radan SISALTOA eika geometriaa, silla pysyy erikseen).
    public static class PakettiporinaTracks
    {
        public class TrackDef
        {
            public string sceneName;
            public string maastoName;
            public float startZ, finishZ;
            public float groundScaleZ;
            // maastoRotationY: maaston (FBX-mallin) odotettu Y-kierto asteina. Useimmilla
            // radoilla 0, mutta jotkin mallit on tuotu/aseteltu niin etta ne tarvitsevat
            // kiinteän kierron nayttaakseen oikealta (esim. Satama, ks. alla). LISATTY
            // 13.8.2026: PakettiporinaSetupin "Maasto: vaara sijainti/kierto/skaala"
            // -korjaus pakotti KAIKKI maastot Quaternion.identityyn riippumatta radasta,
            // mika hiljaa nollasi Sataman tarkoituksellisen 180 asteen kierron joka
            // kerta kun Korjaus/01 ajettiin - koko satama-alue (laituri/vesi) kaantyi
            // ymparinsa mutta erikseen sijoitetut koristeet (kartiot, laatikot,
            // nosturi/majakka/laiva jne.) jaivat paikoilleen, mika rikkoi koko asettelun
            // nakymatta itse koodissa virheelta. Nyt jokainen rata voi maaritella oman
            // odotetun kiertonsa tassa.
            public float maastoRotationY;
            // roadBaseX: tien X-perustaso (0 = tie kaartaa keskilinjan molemmin puolin,
            // >0 = koko tie siirretty sivuun - kaytetaan Satamassa jotta rannalle jaa
            // johdonmukaisesti tilaa toisella puolella eika vesi kaventele "joeksi").
            public float roadBaseX;
            public (float z0, float z1, float amp)[] bends;
            public float[] boostZ;
            public float[] puddleZ;
            public (float z, float offsetX)[] cones;

            // KAYTTAJAN UNITYSSA KASIN TESTAAMAT (x,z) -ylikirjoitukset - jos annettu (ei-tyhja),
            // PakettiporinaElements kayttaa naita SUORAAN eika laske X:aa road_x(z)-kaavasta.
            // Lisatty 8.8.2026 Huvipuistoa varten: kayttaja totesi etaisyystestilla etta
            // formulasta laskettu X ei osunut nakyvalle tielle radan alkupaassa - eika
            // formulaa haluttu (viela) muuttaa kaikkien elementtien osalta, joten yksittaiset,
            // Unityssa oikeaksi todetut sijainnit voidaan kiinnittaa tassa suoraan.
            public (float z, float x)[] boostXZ;
            public (float z, float x)[] conesXZ;
        }

        // Radan leveys on aina 71 -> GroundColliderin X-skaala sama kaikilla radoilla.
        public const float GROUND_SCALE_X = 7.2f;

        // Tahtien ensimmainen/viimeinen Z lasketaan aina samalla marginaalilla
        // startZ/finishZ:sta (+25 alussa, -20 lopussa) - jattaa tilaa ennen
        // ensimmaista/viimeisen jalkeen ilman etta tahti on aivan lahdon/maalin paalla.
        public const float STAR_START_MARGIN = 25f;
        public const float STAR_FINISH_MARGIN = 20f;

        public static readonly TrackDef[] ALL =
        {
            new TrackDef
            {
                // Maatila: ensimmainen "hengahdys"-rata, kaksi hyvin loivaa peltotien
                // kaarretta. Ei kartioita/lataakoita - helppo, rauhallinen. Tahdet
                // toimivat vaikeasuunnitelman "kolikoiden" sijasta kunnes oma
                // kolikkosysteemi rakennetaan.
                sceneName = "Maatila", maastoName = "Maatila",
                startZ = -150f, finishZ = 150f, groundScaleZ = 34.0f, roadBaseX = 0f,
                bends = new[] { (-110f, -20f, 14f), (20f, 110f, -14f) },
                boostZ = new float[0],
                puddleZ = new float[0],
                cones = new (float, float)[0],
            },
            new TrackDef
            {
                sceneName = "Game", maastoName = "Maasto",
                startZ = -135f, finishZ = 135f, groundScaleZ = 30.2f, roadBaseX = 0f,
                bends = new[] { (-100f, 0f, 20f), (0f, 100f, -20f) },
                boostZ = new[] { -130f, 0f },
                puddleZ = new[] { -50f },
                cones = new[] { (45f, 3.5f), (60f, -3.5f) },
            },
            new TrackDef
            {
                // Puisto: pelkkia boosteja - opettaa etta keveys kannattaa (ei muita elementteja).
                // KORJATTU 7.8.2026: oli vain 3 boostia ja kaksi niista osui tasan
                // start/finish-linjalle (z=-150/150) - jarjeton sijainti (ennen lahtoa/
                // jalkeen maalin). Nyt 5 boostia tasavalein z=-120..120, 30 yksikkoa
                // marginaalia molemmissa paissa.
                sceneName = "Puisto", maastoName = "Puisto",
                startZ = -150f, finishZ = 150f, groundScaleZ = 34.0f, roadBaseX = 0f,
                bends = new[] { (-120f, -30f, 18f), (30f, 120f, -18f) },
                boostZ = new[] { -120f, -60f, 0f, 60f, 120f },
                puddleZ = new float[0],
                cones = new (float, float)[0],
            },
            new TrackDef
            {
                // Satama v2: epasymmetrinen lahti - iso loiva mutka oikealle z=-100..0,
                // loivempi vasemmalle z=0..100. Koko tie +15 sivuun (roadBaseX) jotta
                // rannalle jaa johdonmukaisesti tilaa koko radan matkalta.
                sceneName = "Satama", maastoName = "Satama",
                startZ = -150f, finishZ = 150f, groundScaleZ = 34.0f, roadBaseX = 15f,
                // Sataman FBX-malli on tuotu/aseteltu 180 astetta kierrettyna (nakyy myos
                // scenen m_LocalEulerAnglesHint.y=180-arvosta) - laituri/vesi ovat oikealla
                // puolella VAIN tassa kierrossa. AlA muuta 0:ksi, ks. maastoRotationY-kommentti.
                maastoRotationY = 180f,
                bends = new[] { (-100f, 0f, 10f), (0f, 100f, -6f) },
                boostZ = new float[0],
                puddleZ = new[] { 0f },
                // KORJATTU 7.8.2026: scenessa oli jo 7 kartiota (Kartio_01..07) vaikka
                // koodissa oli vain 2 - loput 5 olivat jaaneet "orpoina" vanhasta
                // presetista (sama ilmio kuin Puiston Boost_06). Vietiin talteen "16 -
                // Vie sijainnit koodiksi" -tyokalulla. HUOM Kartio_07 (z=-80, offset
                // -10.5): etaisyys tiesta on piennartakin (7.8) suurempi eli se on
                // nurmikolla eika oikeasti tuki tieta - tarkista/siirra jos halutaan
                // sen oikeasti hidastavan.
                cones = new[] { (-130f, 3.0f), (-110f, -3.0f), (-80f, -10.5f), (-50f, 3.5f), (50f, -3.5f), (110f, -3.0f), (125f, -3.0f) },
            },
            new TrackDef
            {
                // Huvipuisto: kaikki elementit yhdessa - haastavin, kaikki opitut taidot tarpeen.
                // TAYDENNETTY 8.8.2026: elementit nyt jokaisessa kolmesta osiosta (bend1/
                // s-mutka/bend3) - boost/lätäkkö sijoittuvat AINA road_x(z):n paalle (siis
                // suoraan tielle, PlaceBoost/PlacePuddle eivat kayta offsetX:aa), kartiot
                // road_x(z)+offsetX. Lisatty lätäkkö bend1:een ja kaksi kartiota s-mutkan
                // suunnanvaihtoon (pakottaa vaistelemaan juuri kun tie kaantyy toiseen suuntaan).
                //
                // KORJATTU 8.8.2026 (sama paiva, myohemmin): edellinen muokkaus poisti
                // vahingossa sceneName/maastoName/startZ/finishZ/groundScaleZ/roadBaseX/bends-
                // rivit talta TrackDefilta editoidessa vain boostZ/puddleZ/cones-osaa. Tama
                // aiheutti "ei tunnettua ratapresettia" -virheen KAIKISSA tyokaluissa
                // (SceneDump/RoadProfile/Elements) Huvipuisto-scenessa, vaikka rata oli listalla -
                // koska sceneName oli hiljaa null (C#:n string-oletusarvo) eika koskaan tasmannyt
                // "Huvipuisto":n kanssa Find()-vertailussa. Palautettu.
                //
                // KORJATTU 8.8.2026 (kolmas kerta): kayttaja totesi Unityssa etta tehosteet/
                // esteet/tahdet EIVAT osuneet nakyvalle tielle, erityisesti radan alkupaassa -
                // kaikki kaytannon testauksesta saadut korjatut pisteet (Boost_01/02/03/05,
                // Kartio_01/02/04) osoittautuivat systemaattisesti VASTAKKAISMERKKISIKSI kuin
                // mita bends-kaava antoi (esim. Boost_02 z=-100: kaava antoi X=+9.78, oikea
                // paikka oli X=-4 - siis negatiivinen). Kaikkien kolmen mutkan amplitudin
                // etumerkki oli vaarinpain (peilikuva sivuttain nakyvaan tiehen nahden) -
                // kaannetty. HUOM tama poikkeaa Blenderista tehdysta FBX-tarkistuksesta joka
                // NAYTTI tasmaavan alkuperaisiin (positiivisiin) arvoihin - todennakoinen syy:
                // Blenderin oma X-akseli kaantyy FBX-vienti/tuontiketjussa (kasisyys/handedness-
                // muunnos) eika suoraan vastaa Unityn X:aa - Unity-testi on tassa auktoriteetti.
                // TASAPAINOTETTU 8.8.2026: kayttaja pyysi tarkistamaan/tasapainottamaan
                // tahtien/boostien/esteiden maaran ja sijoittelun. Alkuperaisessa listassa
                // (5 boostia, 2 latakkoa) oli kaksi 80 yksikon "tyhjaa" valia (z -100..-20 ja
                // z -20..60) JA koko viimeinen 71 yksikon pätka (z 99..170, koko kolmas mutka +
                // maaliin johtava suora) ilman YHTAAN boostia. Lisatty 3 boostia (z=-65,20,130)
                // ja 2 latakkoa (z=-130,110) tayttamaan aukot - uudet valit nyt tasaisesti
                // 30-45 yksikkoa (oli 30-80). Uusien kohteiden X laskettu SUORAAN alla olevalla
                // (jo Unityssa vahvistetulla, kaannetylla) bends-kaavalla road_x(z):lla - ei
                // kasin testattu Unityssa viela kuten muut boostXZ-pisteet, mutta kaava on nyt
                // todistetusti tarkka (~5 yks. virhemarginaali kayttajan hand-testatuista
                // pisteista) joten pitaisi osua tielle oikein.
                sceneName = "Huvipuisto", maastoName = "Huvipuisto",
                startZ = -170f, finishZ = 170f, groundScaleZ = 38.0f, roadBaseX = 0f,
                bends = new[] { (-150f, -80f, -16f), (-40f, 40f, 18f), (80f, 150f, -16f) },
                boostZ = new[] { -140f, -100f, -65f, -20f, 20f, 60f, 99f, 130f },
                puddleZ = new[] { -130f, -60f, 0f, 110f },
                cones = new[] { (-115f, 3.5f), (-15f, -3f), (15f, 3f), (115f, -3.5f) },

                // Kayttajan Unityssa kasin testaamat, tarkat sijainnit - kaytetaan naita
                // SUORAAN boostZ/cones+kaavan sijasta (ks. TrackDef.boostXZ-kommentti).
                // Kolme uutta tayteboostia (-65,20,130) lisatty perään, X laskettu kaavasta
                // (ks. yllaoleva TASAPAINOTETTU-kommentti) - ei viela kasin Unity-testattu.
                boostXZ = new[] { (-140f, -4f), (-100f, -4f), (-65f, 0f), (-20f, 5f), (20f, 9f), (60f, 0f), (99f, -5f), (130f, -9.78f) },
                // KORJATTU 8.8.2026 ("tarkista kaikki huolellisesti" -pyynnon yhteydessa loydetty):
                // Kartio_03:n arvo (15,-9.44) oli peraisin AIVAN ALKUPERAISESTA (ennen bends-
                // kaannosta otetusta) scene-dumpista - kayttaja ei koskaan pyytanyt sille
                // korjausta erikseen (vain Kartio_01/02/04 mainittiin), joten se jai vahingossa
                // vanhaksi. Nyt kaannetyn kaavan mukaan RoadX(15)=12.44, joten -9.44 oli 21.9
                // yksikkoa oikealta tielta - kiintea este joka ei enaa oikeasti tukkinut tieta.
                // Korjattu samalla n. +5 offset-periaatteella kuin Kartio_01/02/04 (mutta
                // vastakkainen etumerkki Kartio_02:een nahden, koska ne ovat s-mutkan
                // vastakkaisilla puolilla) - EI VIELA KASIN UNITY-TESTATTU.
                conesXZ = new[] { (-115f, -11f), (-15f, 7f), (15f, 17.44f), (114f, -11f) },
            },
        };

        public static TrackDef Find(string sceneName)
        {
            foreach (var t in ALL)
                if (t.sceneName == sceneName) return t;
            return null;
        }

        // Sama road_x(z)-kaava kaikkialla: x = roadBaseX + sum(amp * sin(pi*(z-z0)/(z1-z0))^2)
        // jokaiselle bend-segmentille jonka [z0,z1] sisaltaa z:n.
        public static float RoadX(float z, TrackDef t)
        {
            float x = t.roadBaseX;
            foreach (var (z0, z1, amp) in t.bends)
                if (z >= z0 && z <= z1)
                    x += amp * Mathf.Pow(Mathf.Sin(Mathf.PI * (z - z0) / (z1 - z0)), 2f);
            return x;
        }

        // Ylikuormattu versio kun kaytossa on vain irralliset bends+baseX (ei koko TrackDef) -
        // esim. jos joku tyokalu haluaa laskea RoadX:n ilman TrackDef-viittausta.
        public static float RoadX(float z, (float z0, float z1, float amp)[] bends, float baseX = 0f)
        {
            float x = baseX;
            foreach (var (z0, z1, amp) in bends)
                if (z >= z0 && z <= z1)
                    x += amp * Mathf.Pow(Mathf.Sin(Mathf.PI * (z - z0) / (z1 - z0)), 2f);
            return x;
        }

        public static float StarFirstZ(TrackDef t) => t.startZ + STAR_START_MARGIN;
        public static float StarLastZ(TrackDef t) => t.finishZ - STAR_FINISH_MARGIN;
    }
}
#endif
