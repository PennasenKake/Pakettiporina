using System;
using UnityEngine;

namespace Pakettiporina
{
    // Kevyt tapahtumavayla: pelin osat ilmoittavat tapahtumista toisilleen.
    public static class GameEvents
    {
        public static event Action OnRaceStart;
        public static event Action OnFinish;
        public static event Action<int> OnStarCollected;
        public static event Action<int> OnCountdown;   // lahtolaskenta: 3, 2, 1
        public static event Action OnGo;               // "AJA!"

        public static void RaceStart()
        {
            Debug.Log("[Events] RaceStart");
            OnRaceStart?.Invoke();
        }

        public static void Finish()
        {
            Debug.Log("[Events] Finish");
            OnFinish?.Invoke();
        }

        public static void StarCollected(int total)
        {
            Debug.Log($"[Events] StarCollected ({total})");
            OnStarCollected?.Invoke(total);
        }

        public static void Countdown(int n)
        {
            OnCountdown?.Invoke(n);
        }

        public static void Go()
        {
            Debug.Log("[Events] Go");
            OnGo?.Invoke();
        }
    }
}