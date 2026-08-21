using System;
using UnityEngine;

namespace Pakettiporina
{
    // Kevyt tapahtumavayla: pelin osat ilmoittavat tapahtumista toisilleen.
    //
    // KORJATTU 19.8.2026 (koko pelin tarkistuksessa loydetty rakenteellinen riski,
    // kayttaja: "auto ei liiku startissa"): normaali C#-tapa kutsua monikuuntelijaista
    // eventtia (`OnGo?.Invoke()`) heittaa POIKKEUKSEN VALITTOMASTI JOS YKSIKIN tilaaja
    // (esim. jokin HUD/UI-skripti) heittaa poikkeuksen - eika muita, sen JALKEEN
    // listattuja tilaajia (tai eventin KUTSUJAA, esim. RaceManager.CountdownRoutine)
    // enaa suoriteta ollenkaan. Yhden UI-kuuntelijan bugi olisi nain voinut hiljaisesti
    // pysayttaa koko pelin ydinlogiikan (esim. auton vapautuksen). Jokainen tilaaja
    // kutsutaan nyt ERIKSEEN try/catchissa - yhden kuuntelijan virhe lokitetaan
    // konsoliin muttei enaa voi estaa muita kuuntelijoita tai itse tapahtuman
    // kutsujaa jatkamasta normaalisti.
    public static class GameEvents
    {
        public static event Action OnRaceStart;
        public static event Action OnFinish;
        public static event Action<int> OnStarCollected;
        public static event Action<int> OnCountdown;   // lahtolaskenta: 3, 2, 1
        public static event Action OnGo;               // "AJA!"

        static void SafeInvoke(Action ev, string name)
        {
            if (ev == null) return;
            foreach (var d in ev.GetInvocationList())
            {
                try { ((Action)d)(); }
                catch (Exception e) { Debug.LogError($"[Events] Kuuntelija epaonnistui ({name}): {e}"); }
            }
        }

        static void SafeInvoke(Action<int> ev, int arg, string name)
        {
            if (ev == null) return;
            foreach (var d in ev.GetInvocationList())
            {
                try { ((Action<int>)d)(arg); }
                catch (Exception e) { Debug.LogError($"[Events] Kuuntelija epaonnistui ({name}): {e}"); }
            }
        }

        public static void RaceStart()
        {
            Debug.Log("[Events] RaceStart");
            SafeInvoke(OnRaceStart, nameof(OnRaceStart));
        }

        public static void Finish()
        {
            Debug.Log("[Events] Finish");
            SafeInvoke(OnFinish, nameof(OnFinish));
        }

        public static void StarCollected(int total)
        {
            Debug.Log($"[Events] StarCollected ({total})");
            SafeInvoke(OnStarCollected, total, nameof(OnStarCollected));
        }

        public static void Countdown(int n)
        {
            SafeInvoke(OnCountdown, n, nameof(OnCountdown));
        }

        public static void Go()
        {
            Debug.Log("[Events] Go");
            SafeInvoke(OnGo, nameof(OnGo));
        }
    }
}