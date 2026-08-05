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
                Debug.LogWarning("[GameManager] Toinen GameManager loytyi � tuhotaan duplikaatti.");
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
    }
}