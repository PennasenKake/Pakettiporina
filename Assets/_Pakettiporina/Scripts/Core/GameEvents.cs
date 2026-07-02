using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Pakettiporina
{
    // Kevyt tapahtumavayla: pelin osat ilmoittavat tapahtumista toisilleen
    // tuntematta toisiaan suoraan. Tilaa OnEnablessa, peru OnDisablessa.
    public static class GameEvents
    {
        public static event Action OnRaceStart;
        public static event Action OnFinish;
        public static event Action<int> OnStarCollected; // parametri = tahtien kokonaismaara

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
    }
}