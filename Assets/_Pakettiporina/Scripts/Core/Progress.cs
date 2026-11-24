using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: edistyksen porrastus. Ajon arvosana on 1-3 tahtea (ks. RaceManager.FinishRace),
    // ja kertyneet tahdet (GameManager.RunStars) avaavat sisaltoa kolmessa tahdissa:
    //   NOPEA   (3-12 tahtea): varit ja pienet lisaosat - jokaisella pelikerralla jotain uutta
    //   KESKI   (7-24 tahtea): uudet osat ja paketit
    //   PITKA   (5-35 tahtea): uudet radat / alueet (juhlahetki)
    // Taulukko on koodissa (ei assetteja): helppo saataa yhdesta paikasta. Avain = asset-nimi (PartData.name /
    // PackageData.name) tai radan scene-nimi. Jos osaa/pakettia ei ole taulukossa, kaytetaan vanhaa
    // unlockPoints-saantoa (pisteet), joten salaiset Kultaosat toimivat ennallaan.
    // HUOM: paketin vaatima osa (requiredPart) pidetaan aina auki (0), ettei synny umpikujaa.
    public static class Progress
    {
        public const int StarPointsBonus = 10;      // lisapisteet jokaisesta 2. ja 3. tahdesta

        static readonly Dictionary<string, int> parts = new Dictionary<string, int>
        {
            // nopeat: varit
            { "Oranssi", 3 }, { "Keltainen", 6 }, { "Sininen_maali", 9 }, { "Vihreä_maali", 12 }, { "Sateenkaarimaali", 30 },
            // nopeat: pienet lisaosat
            { "Valot", 4 }, { "Lampolaatikko", 5 }, { "Tavaratila", 9 },
            // keskipitkat: renkaat, korit, moottorit, jouset
            { "Hiekkarenkaat", 7 }, { "Maastorenkaat", 11 },
            { "Kilpakori", 10 }, { "Kisajouset", 14 }, { "Urheilukori", 16 }, { "Rekkakone", 18 },
            { "Kisajouset2", 20 }, { "Sähkömoottori", 24 },
            // pitkat: kultaosat (salaisia, ilmestyvat listaan kun tahdet riittavat)
            { "Kultainen_Kori", 45 }, { "Kultaiset_Renkaat", 50 }, { "Kultainen_Moottori", 55 }, { "Kultaiset_Jouset", 60 }, { "Kultainen_Lisaosa", 70 },
        };

        static readonly Dictionary<string, int> packages = new Dictionary<string, int>
        {
            // 24.11.2026: paketti ei aukea ennen kuin sen ehdottama rata on auki (Tarkistus/08 varoitti ristiriidasta):
            // Puisto 5 -> Munakori 5; Huvipuisto 12 -> Synttarikakku 12; Maatila 22 -> Tiiliskivet 22.
            { "Kirjapino", 3 }, { "Munakori", 5 }, { "Synttarikakku", 12 }, { "Tiiliskivet", 22 }, { "Kukkaruukku", 24 },
            { "Jattijaatelo", 26 }, { "Saippuakuplat", 32 }, { "Lumiukko", 40 },
        };

        // avain pienilla kirjaimilla, ilman aakkosia
        static readonly Dictionary<string, int> tracks = new Dictionary<string, int>
        {
            { "satama", 0 }, { "puisto", 5 }, { "huvipuisto", 12 }, { "maatila", 22 }, { "kyla", 35 },
        };

        static readonly Dictionary<string, string> trackNames = new Dictionary<string, string>
        {
            { "satama", "Satama" }, { "puisto", "Puisto" }, { "huvipuisto", "Huvipuisto" }, { "maatila", "Maatila" }, { "kyla", "Kylä" },
        };

        // YKSI PAIKKA ratalistalle (24.11.2026). Ajoscenet: musiikki (AudioManager) ja suorituskykyasetukset
        // (PerformanceManager) lukevat taman. Uusi rata: lisaa tahan, tracks-taulukkoon ja trackNames-taulukkoon,
        // seka Editor/Common/PakettiporinaTracks.cs:n ALL-listaan (Tarkistus/08 varoittaa jos jokin puuttuu).
        public static readonly string[] RaceScenes = { "Game", "Game2", "Satama", "Puisto", "Huvipuisto", "Maatila", "Kyla" };

        // Suunnitellut radat joiden scene on tarkoituksella pois Build Settingsista (valmistuu myohemmin).
        // Garage ei nayta niita; tarkistukset eivat pidä sita virheena.
        public static readonly string[] PlannedTracks = { "kyla" };
        public static bool IsPlanned(string key) { return System.Array.IndexOf(PlannedTracks, key) >= 0; }

        // Vain luku: Editor-tarkistus (Tarkistus/08) kay taulukot lapi.
        public static IEnumerable<KeyValuePair<string, int>> PartTable { get { return parts; } }
        public static IEnumerable<KeyValuePair<string, int>> PackageTable { get { return packages; } }
        public static IEnumerable<KeyValuePair<string, int>> TrackTable { get { return tracks; } }

        public static string TrackKey(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return "";
            return scene.ToLowerInvariant().Replace("ä", "a").Replace("ö", "o");
        }

        public static int RunStars { get { return GameManager.Instance != null ? GameManager.Instance.RunStars : 0; } }

        // Palauttaa tarvittavat tahdet, tai -1 jos taulukossa ei ole (silloin kaytetaan unlockPoints-saantoa).
        public static int PartNeeded(PartData p) { int n; return p != null && parts.TryGetValue(p.name, out n) ? n : -1; }
        public static int PackageNeeded(PackageData p) { int n; return p != null && packages.TryGetValue(p.name, out n) ? n : -1; }
        public static int TrackNeeded(string scene) { int n; return tracks.TryGetValue(TrackKey(scene), out n) ? n : 0; }

        public static bool PartLocked(PartData p)
        {
            int need = PartNeeded(p);
            if (need >= 0) return RunStars < need;
            if (p == null || p.unlockPoints <= 0) return false;
            return (GameManager.Instance != null ? GameManager.Instance.Points : 0) < p.unlockPoints;
        }

        public static int PartLeft(PartData p)
        {
            int need = PartNeeded(p);
            if (need >= 0) return Mathf.Max(0, need - RunStars);
            return p == null ? 0 : Mathf.Max(0, p.unlockPoints - (GameManager.Instance != null ? GameManager.Instance.Points : 0));
        }

        public static bool PackageLocked(PackageData p)
        {
            int need = PackageNeeded(p);
            if (need >= 0) return RunStars < need;
            if (p == null || p.unlockPoints <= 0) return false;
            return (GameManager.Instance != null ? GameManager.Instance.Points : 0) < p.unlockPoints;
        }

        public static int PackageLeft(PackageData p)
        {
            int need = PackageNeeded(p);
            if (need >= 0) return Mathf.Max(0, need - RunStars);
            return p == null ? 0 : Mathf.Max(0, p.unlockPoints - (GameManager.Instance != null ? GameManager.Instance.Points : 0));
        }

        public static bool TrackLocked(string scene) { return RunStars < TrackNeeded(scene); }
        public static int TrackLeft(string scene) { return Mathf.Max(0, TrackNeeded(scene) - RunStars); }

        // Mita avautui kun tahtimaara kasvoi from -> to. Teksti kerrotaan lapselle maalissa.
        public static List<string> NewlyUnlocked(int from, int to)
        {
            var list = new List<string>();
            if (to <= from) return list;
            var sm = SaveManager.Instance;
            if (sm != null)
            {
                foreach (var p in sm.allParts)
                {
                    int n = PartNeeded(p);
                    if (n > from && n <= to) list.Add((p.category == PartCategory.Maali ? "Uusi väri: " : "Uusi osa: ") + p.displayName);
                }
                foreach (var p in sm.allPackages)
                {
                    int n = PackageNeeded(p);
                    if (n > from && n <= to) list.Add("Uusi paketti: " + p.displayName);
                }
            }
            foreach (var kv in tracks)
                if (kv.Value > from && kv.Value <= to) list.Add("Uusi rata: " + trackNames[kv.Key]);
            return list;
        }

        // Seuraava palkinto (pienin tahtiraja joka on viela edessa) - nayttoa varten. null jos kaikki auki.
        public static bool NextReward(out int need, out string text)
        {
            need = int.MaxValue; text = null;
            int cur = RunStars;
            var list = NewlyUnlocked(cur, cur + 1);
            for (int t = cur + 1; t <= cur + 60; t++)
            {
                list = NewlyUnlocked(t - 1, t);
                if (list.Count > 0) { need = t; text = list[0]; return true; }
            }
            return false;
        }

        // Ajon arvosana 1-3. Puhdas funktio, helppo testata.
        public static int RateRun(int dentHits, int bonusCollected, int bonusTotal, float seconds)
        {
            int stars = 1;                                  // maaliin paaseminen
            if (dentHits == 0) stars = 2;                   // paketti ehjana
            bool allBonus = bonusTotal > 0 ? bonusCollected >= bonusTotal : seconds <= 60f;
            if (stars == 2 && allBonus) stars = 3;          // lisaksi kaikki bonustahdet (tai nopea aika jos bonuksia ei ole)
            return stars;
        }
    }
}
