using UnityEngine;

namespace Pakettiporina
{
    // Pelin keskussingleton: pitaa pisteet, pelin vaiheen seka valitun paketin ja auton.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum Phase { Menu, Racing, Finished }
        public Phase CurrentPhase { get; private set; } = Phase.Menu;
        public int Points { get; private set; }

        // Hallissa tehty valinta, jonka keikka lukee.
        public PackageData SelectedPackage { get; private set; }
        public CarStats SelectedStats { get; private set; }

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
        }

        // Hallin "Aja keikka" tallentaa valinnan tanne ennen scenen vaihtoa.
        public void SetSelection(PackageData pkg, CarStats stats)
        {
            SelectedPackage = pkg;
            SelectedStats = stats;
            string n = pkg != null ? pkg.displayName : "ei pakettia";
            Debug.Log($"[GameManager] Valinta tallennettu: paketti={n}, auto=({stats})");
        }
    }
}