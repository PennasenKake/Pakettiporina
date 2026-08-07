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
            // roadBaseX: tien X-perustaso (0 = tie kaartaa keskilinjan molemmin puolin,
            // >0 = koko tie siirretty sivuun - kaytetaan Satamassa jotta rannalle jaa
            // johdonmukaisesti tilaa toisella puolella eika vesi kaventele "joeksi").
            public float roadBaseX;
            public (float z0, float z1, float amp)[] bends;
            public float[] boostZ;
            public float[] puddleZ;
            public (float z, float offsetX)[] cones;
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
                sceneName = "Huvipuisto", maastoName = "Huvipuisto",
                startZ = -170f, finishZ = 170f, groundScaleZ = 38.0f, roadBaseX = 0f,
                bends = new[] { (-150f, -80f, 16f), (-40f, 40f, -18f), (80f, 150f, 16f) },
                boostZ = new[] { -140f, -20f, 100f },
                puddleZ = new[] { 0f },
                cones = new[] { (-115f, 3.5f), (115f, -3.5f) },
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
