using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Pelin keskussingleton: pisteet, vaihe, valittu paketti, valitut osat ja lasketut mittarit.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum Phase { Menu, Racing, Finished }
        public Phase CurrentPhase { get; private set; } = Phase.Menu;
        public int Points { get; private set; }

        // Hallin valinta, joka sailyy scenejen yli (DontDestroyOnLoad).
        public PackageData SelectedPackage { get; private set; }
        public CarStats SelectedStats { get; private set; }
        // Valitut osat: nailla halli rakentaa saman auton uudelleen.
        public List<PartData> SelectedParts { get; private set; } = new List<PartData>();

        // Pysyvasti avatut tarrat (ostettu pisteilla). Sailyy tallennuksen yli.
        public HashSet<string> UnlockedStickerNames { get; private set; } = new HashSet<string>();

        // M5.17/16.8.2026 (kayttaja: "progressbar lialle ja korjaukselle, joka kasvaa
        // osumista ja pysyy seuraavaan ajoon asti, tyhjenee Pesussa poksauttelemalla"):
        // PYSYVA lika-/kolhumittari. AIEMMIN (14.-15.8.) RaceDents/RaceDirtHits nollautuivat
        // JOKAISEN ajon alussa (RaceManager.StartRace -> ResetRaceDamage) - tietoinen
        // paatos silloin ("edellisen keikan korjaamattomat kolhut eivat kasaannu
        // loputtomiin"), mutta kayttaja halusi nyt PAINVASTAISEN kayttaytymisen: mittari
        // kasvaa Cone/Laatikko/Puddle-osumista AJON AIKANA (nakyy myos live-palkkina
        // RaceHUD:ssa), EI nollaudu ajon alussa, ja purkautuu vain Pesussa asteittain
        // (yksi napautus = yksi yksikko pois, ks. CleanOneDirt/RepairOneDent). Ei pakota
        // pesuun - taynna oleva mittari on vain visuaalinen vihje, ei este.
        // Katto (DIRT_MAX/DENT_MAX) tasmaa WashScreenin dirtSpots(15)/dentSpots(6)
        // -kuvakemaariin, joten yksi mittariyksikko = yksi nakyva merkki autossa, ei
        // tarvitse erillista skaalausta.
        public const int DIRT_MAX = 15;
        public const int DENT_MAX = 6;

        // LISATTY 16.8.2026 (kayttaja: "osumat voisivat toimia kertoimina (esimerkiksi 2x)
        // pesulle ja korjaukselle, huomaan etta loppuu liian nopeasti pesu ja korjaus"):
        // yksi oikea osuma radalla kasvattaa mittaria HIT_MULTIPLIER-verran (katto pitaa
        // silti kiinni DIRT_MAX/DENT_MAX:sta). Nain mittari tayttyy nopeammin ajon aikana,
        // eli pelaaja paatyy Pesuun useammalla likatahralla/kolhulla siivottavana - Pesu-
        // ja korjausvaihe kestaa pidempaan yhdella ajolla ansaituista osumista.
        public const int HIT_MULTIPLIER = 2;

        public int DirtMeter { get; private set; }
        public int DentMeter { get; private set; }

        // RaceHUD tilaa nama paivittaakseen palkit reaaliaikaisesti heti kun osuma
        // tapahtuu, ilman pollausta joka ruudulla. (nykyinen, max).
        public event System.Action<int, int> OnDirtMeterChanged;
        public event System.Action<int, int> OnDentMeterChanged;

        // Ilmoittaa AINA kun Points-arvo muuttuu (lisays, lataus tallennuksesta, tarraosto).
        // UI-komponentit (esim. PointsDisplay) voivat tilata taman sen sijaan etta luottavat
        // vain OnEnableen - talla vaietaan tilanne jossa pisteet muuttuvat SAMAN scenen
        // sisalla (esim. tarran ostaminen paavalikossa) eika mikaan UI-elementti ehdi
        // aktivoitua/deaktivoitua uudelleen, mika muuten jattaisi vanhan lukeman nakyviin.
        public event System.Action<int> OnPointsChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] Toinen GameManager loytyi — tuhotaan duplikaatti.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[GameManager] Valmis. Pisteet: {Points}");
        }

        public void SetPhase(Phase p)
        {
            CurrentPhase = p;
            Debug.Log($"[GameManager] Vaihe: {p}");
        }

        public void AddPoints(int amount)
        {
            Points += amount;
            Debug.Log($"[GameManager] +{amount} pistetta. Yhteensa: {Points}");
            SaveManager.Instance?.Save();
            OnPointsChanged?.Invoke(Points);
        }

        // Hallin "Aja keikka" tallentaa taalle: paketti, mittarit JA valitut osat.
        public void SetSelection(PackageData pkg, CarStats stats, List<PartData> parts)
        {
            SelectedPackage = pkg;
            SelectedStats = stats;
            SelectedParts = new List<PartData>(parts); // kopio talteen
            string n = pkg != null ? pkg.displayName : "ei pakettia";
            Debug.Log($"[GameManager] Valinta tallennettu: paketti={n}, osia={SelectedParts.Count}, auto=({stats})");
        }

        // Kaytetaan SaveManagerista pisteiden palauttamiseen tallennuksesta (ei laske yhteen, asettaa suoraan).
        public void SetPoints(int amount)
        {
            Points = amount;
            Debug.Log($"[GameManager] Pisteet ladattu tallennuksesta: {Points}");
            OnPointsChanged?.Invoke(Points);
        }

        // Kaytetaan SaveManagerista: palauttaa paketin + osat ilman mittareita
        // (mittarit lasketaan uudelleen kun pelaaja kaynnistaa Hallin, CarBuilderin kautta).
        public void ApplyLoadedSelection(PackageData pkg, List<PartData> parts)
        {
            SelectedPackage = pkg;
            SelectedParts = parts != null ? new List<PartData>(parts) : new List<PartData>();
            SelectedStats = default;
            Debug.Log($"[GameManager] Ladattu auto tallennuksesta: osia={SelectedParts.Count}, paketti={(pkg != null ? pkg.displayName : "ei mitaan")}.");
        }

        // Onko tarra jo ostettu/avattu pysyvasti (ei riipu nykyisista pisteista).
        public bool IsStickerUnlocked(StickerData sticker)
        {
            return sticker != null && UnlockedStickerNames.Contains(sticker.name);
        }

        // Yrittaa ostaa tarran nykyisilla pisteilla. Jos jo avattu, palauttaa true heti
        // (ei veloita uudelleen). Jos pisteet eivat riita, palauttaa false eika tee mitaan.
        public bool TryUnlockSticker(StickerData sticker)
        {
            if (sticker == null) return false;
            if (UnlockedStickerNames.Contains(sticker.name)) return true;
            if (Points < sticker.unlockPoints) return false;

            Points -= sticker.unlockPoints;
            UnlockedStickerNames.Add(sticker.name);
            Debug.Log($"[GameManager] Tarra ostettu: {sticker.displayName} (-{sticker.unlockPoints} p). Pisteet jaljella: {Points}");
            SaveManager.Instance?.Save();
            OnPointsChanged?.Invoke(Points);
            return true;
        }

        // Kaytetaan SaveManagerista: palauttaa avattujen tarrojen listan tallennuksesta.
        public void ApplyLoadedStickers(List<string> names)
        {
            UnlockedStickerNames = names != null ? new HashSet<string>(names) : new HashSet<string>();
            Debug.Log($"[GameManager] Ladattu {UnlockedStickerNames.Count} avattua tarraa tallennuksesta.");
        }

        // --- M5.17: pysyva lika-/kolhumittari (Cone/Laatikko/Puddle taytta, Pesu tyhjentaa) ---
        // HUOM: ei enaa ResetRaceDamage()-metodia/kutsua RaceManager.StartRace():ssa -
        // mittari EI nollaudu ajon alussa (kayttajan 16.8.2026 paatos, ks. yllaoleva
        // luokkakommentti). Jos joskus halutaan palata vanhaan per-ajo-kayttaytymiseen,
        // katso Git-historiaa taman kommentin kohdalta.

        // HUOM tallennuksesta: AddDent/AddDirtHit/CleanOneDirt/RepairOneDent EIVAT kutsu
        // SaveManager.Save():a suoraan (toisin kuin AddPoints) - osumia voi tulla useita
        // sekunnissa huonolla ajolla, eika levylle kirjoittamista haluta joka osumalla.
        // Mittarit tallentuvat silti luotettavasti: (1) AddPoints() (tahdet/maalipalkkio/
        // pesu-/korjauspalkkio) kutsuu Save():n JA lukee talloin GameManagerin senhetkiset
        // DirtMeter/DentMeter-arvot mukaan, (2) SaveManager.OnApplicationPause/Quit
        // tallentaa aina taustalle siirryttaessa/suljettaessa.
        // Kutsutaan Cone.cs/Laatikko.cs:sta kun auto osuu esteeseen.
        public void AddDent()
        {
            DentMeter = Mathf.Min(DentMeter + HIT_MULTIPLIER, DENT_MAX);
            OnDentMeterChanged?.Invoke(DentMeter, DENT_MAX);
            Debug.Log($"[GameManager] Kolhu! Mittari: {DentMeter}/{DENT_MAX}");
        }

        // Kutsutaan Puddle.cs:sta kun auto ajaa latakon lapi.
        public void AddDirtHit()
        {
            DirtMeter = Mathf.Min(DirtMeter + HIT_MULTIPLIER, DIRT_MAX);
            OnDirtMeterChanged?.Invoke(DirtMeter, DIRT_MAX);
            Debug.Log($"[GameManager] Roiske! Mittari: {DirtMeter}/{DIRT_MAX}");
        }

        // Kutsutaan WashScreen.CleanNextSpot():sta JOKAISEN poksautetun kuplan jalkeen
        // (ei enaa kerralla lopussa) - pysyva mittari puretaan asteittain, jotta osittainen
        // pesu (esim. jos pelaaja poistuu Pesusta kesken kaiken) ei haihdu tyhjiin.
        public void CleanOneDirt()
        {
            DirtMeter = Mathf.Max(0, DirtMeter - 1);
            OnDirtMeterChanged?.Invoke(DirtMeter, DIRT_MAX);
        }

        // Kutsutaan WashScreen.RepairNextDent():sta jokaisen RepairTool-napautuksen
        // jalkeen. Sama periaate kuin CleanOneDirt.
        public void RepairOneDent()
        {
            DentMeter = Mathf.Max(0, DentMeter - 1);
            OnDentMeterChanged?.Invoke(DentMeter, DENT_MAX);
        }

        // SaveManagerin kayttoon: palauttaa mittarit levylta sovelluksen
        // uudelleenkaynnistyksen jalkeen (kayttaja: "kylla mielellaan jos mahdollista").
        public void ApplyLoadedDamage(int dirt, int dents)
        {
            DirtMeter = Mathf.Clamp(dirt, 0, DIRT_MAX);
            DentMeter = Mathf.Clamp(dents, 0, DENT_MAX);
            OnDirtMeterChanged?.Invoke(DirtMeter, DIRT_MAX);
            OnDentMeterChanged?.Invoke(DentMeter, DENT_MAX);
            Debug.Log($"[GameManager] Lika-/kolhumittarit ladattu tallennuksesta: {DirtMeter}/{DIRT_MAX}, {DentMeter}/{DENT_MAX}.");
        }
    }
}