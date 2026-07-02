using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Pelin keskussingleton: pitaa pisteet ja pelin vaiheen. Sailyy scenejen yli.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum Phase { Menu, Racing, Finished }
        public Phase CurrentPhase { get; private set; } = Phase.Menu;
        public int Points { get; private set; }

        void Awake()
        {
            // Singleton: vain yksi saa olla olemassa.
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameManager] Toinen GameManager loytyi — tuhotaan duplikaatti.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // sailyy kun scenet vaihtuvat
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
    }
}