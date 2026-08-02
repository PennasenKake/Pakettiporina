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
    }
}