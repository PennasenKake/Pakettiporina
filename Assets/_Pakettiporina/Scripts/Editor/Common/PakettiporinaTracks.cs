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
            // LISATTY 14.8.2026 (kayttaja: "sailytetaan esteina kartiot, laatikot ja
            // latakot... niita tulee olla joka radassa riittavasti ja tasaisesti, ja
            // ehdottomasti jokaisella radalla"). Sama (z, offsetX)-formaatti kuin cones.
            public (float z, float offsetX)[] laatikot;

            // KAYTTAJAN UNITYSSA KASIN TESTAAMAT (x,z) -ylikirjoitukset - jos annettu (ei-tyhja),
            // PakettiporinaElements kayttaa naita SUORAAN eika laske X:aa road_x(z)-kaavasta.
            // Lisatty 8.8.2026 Huvipuistoa varten: kayttaja totesi etaisyystestilla etta
            // formulasta laskettu X ei osunut nakyvalle tielle radan alkupaassa - eika
            // formulaa haluttu (viela) muuttaa kaikkien elementtien osalta, joten yksittaiset,
            // Unityssa oikeaksi todetut sijainnit voidaan kiinnittaa tassa suoraan.
            public (float z, float x)[] boostXZ;
            public (float z, float x)[] conesXZ;
            public (float z, float x)[] laatikotXZ;
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
                // Maatila: alunperin "ei esteita" -helppo hengahdysrata. LISATTY 14.8.2026
                // (kayttaja: "esteita/tehosteita joka radalle, ehdottomasti jokaiselle") -
                // kevyt mutta olemassaoleva esterosteri, edelleen radoista helpoin
                // (vahiten esteita: 3 kartiota+2 laatikkoa+2 latakkoa vs. esim. Huvipuiston
                // 4+4+4). HUOM: Lisays/10-tyokalu (PakettiporinaTrackObstaclesSetup) on jo
                // aiemmin lisannyt taman TrackDefin OHI muutaman Boost/Kartio/Latakko/Kupla-
                // objektin suoraan Maatila.uniin - tama TrackDef-lista on eri, "virallinen"
                // jarjestelma (PakettiporinaElements/Lisays 02); aja Lisays/02 uudelleen
                // TAMAN listan mukaisesti ja tarkista ettei vanhoja Lisays/10-objekteja jaa
                // paallekkain (nimet eroavat, Kupla_ ei kuulu tahan listaan lainkaan).
                sceneName = "Maatila", maastoName = "Maatila",
                startZ = -150f, finishZ = 150f, groundScaleZ = 34.0f, roadBaseX = 0f,
                bends = new[] { (-110f, -20f, 14f), (20f, 110f, -14f) },
                boostZ = new[] { -130f, -20f, 100f },
                // LAAJENNETTU 16.8.2026 (kayttaja: "pysyvalle likamittarille (max 15) pitaa
                // olla riittavasti lätäköitä tiellä" - 2->5 latakkoa jokaisella radalla,
                // ks. myos GameManager.DirtMeter/DentMeter). TARKISTETTU ettei osu samaan
                // Z:aan minkaan kartion/laatikon/boostin kanssa (0 -> 10, valttaa Kartio_02:n z=0).
                puddleZ = new[] { -120f, -65f, 10f, 55f, 120f },
                cones = new[] { (-90f, 3.5f), (0f, -3f), (75f, 3f) },
                laatikot = new[] { (-45f, -3.5f), (35f, 3.5f) },
            },
            new TrackDef
            {
                // TAYDENNETTY 14.8.2026 (kayttaja: "esteita/tehosteita joka radalle,
                // ehdottomasti jokaiselle") - lisatty laatikko + tasapainotettu maara.
                sceneName = "Game", maastoName = "Maasto",
                startZ = -135f, finishZ = 135f, groundScaleZ = 30.2f, roadBaseX = 0f,
                bends = new[] { (-100f, 0f, 20f), (0f, 100f, -20f) },
                boostZ = new[] { -130f, -30f, 90f },
                // LAAJENNETTU 16.8.2026 (pysyva likamittari, ks. Maatila-kommentti ylla).
                // TARKISTETTU: 100 -> 105, valttaa Kartio_03:n z=100.
                puddleZ = new[] { -100f, -50f, 20f, 60f, 105f },
                cones = new[] { (-70f, -3.5f), (45f, 3.5f), (100f, -3.5f) },
                laatikot = new[] { (-20f, 3.5f), (70f, -3.5f) },
            },
            new TrackDef
            {
                // Puisto: alunperin pelkkia boosteja (opettaa etta keveys kannattaa).
                // TAYDENNETTY 14.8.2026 (kayttaja: "esteita/tehosteita joka radalle,
                // ehdottomasti jokaiselle") - lisatty kevyt esterosteri boostien rinnalle,
                // silti toiseksi helpoin Maatilan jalkeen (vahemman kartioita/laatikoita
                // kuin Satamalla/Huvipuistolla).
                sceneName = "Puisto", maastoName = "Puisto",
                startZ = -150f, finishZ = 150f, groundScaleZ = 34.0f, roadBaseX = 0f,
                bends = new[] { (-120f, -30f, 18f), (30f, 120f, -18f) },
                boostZ = new[] { -120f, -60f, 0f, 60f, 120f },
                // LAAJENNETTU 16.8.2026 (pysyva likamittari, ks. Maatila-kommentti ylla).
                // TARKISTETTU: 0 -> 15, valttaa Boost_03:n z=0.
                puddleZ = new[] { -90f, -50f, 15f, 50f, 90f },
                cones = new[] { (-45f, 3.5f), (45f, -3.5f) },
                laatikot = new[] { (-105f, -3.5f), (105f, 3.5f) },
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
                // TAYDENNETTY 14.8.2026 (kayttaja: "esteita/tehosteita joka radalle,
                // ehdottomasti jokaiselle") - Satamalla ei ollut YHTAAN boostia, lisatty 3.
                boostZ = new[] { -140f, -30f, 90f },
                // LAAJENNETTU 16.8.2026 (pysyva likamittari, ks. Maatila-kommentti ylempana).
                // TARKISTETTU: -130 -> -125, valttaa Kartio_01:n z=-130.
                puddleZ = new[] { -125f, -70f, 0f, 60f, 100f },
                // KORJATTU 7.8.2026: scenessa oli jo 7 kartiota (Kartio_01..07) vaikka
                // koodissa oli vain 2 - loput 5 olivat jaaneet "orpoina" vanhasta
                // presetista (sama ilmio kuin Puiston Boost_06). Vietiin talteen "16 -
                // Vie sijainnit koodiksi" -tyokalulla. HUOM Kartio_07 (z=-80, offset
                // -10.5): etaisyys tiesta on piennartakin (7.8) suurempi eli se on
                // nurmikolla eika oikeasti tuki tieta - tarkista/siirra jos halutaan
                // sen oikeasti hidastavan.
                cones = new[] { (-130f, 3.0f), (-110f, -3.0f), (-80f, -10.5f), (-50f, 3.5f), (50f, -3.5f), (110f, -3.0f), (125f, -3.0f) },
                laatikot = new[] { (-95f, 3.5f), (20f, -3.5f), (140f, 3.0f) },
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
                // LAAJENNETTU 16.8.2026 (pysyva likamittari, ks. Maatila-kommentti ylempana).
                // TARKISTETTU: 60 -> 50, valttaa Boost-taytteen z=60.
                puddleZ = new[] { -130f, -60f, 0f, 50f, 110f },
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
                // LISATTY 14.8.2026 (kayttaja: "esteita/tehosteita joka radalle, ehdottomasti
                // jokaiselle") - lasketu suoraan (kaannetylla) bends-kaavalla, kuten uusimmat
                // boostZ-tayteet ylla - EI VIELA KASIN UNITY-TESTATTU.
                laatikot = new[] { (-150f, 4f), (-95f, -6f), (45f, -6f), (145f, 4f) },
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
