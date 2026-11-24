using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: akku- ja suorituskykyvarotoimet. Luo itsensa automaattisesti.
    //  - Ruudunpaivitysraja: valikot ja halli 30 fps (ei animaatiota joka vaatisi 60), ajo ja pesu 60 fps.
    //    Ilman rajaa moderni 90/120 Hz -puhelin piirtaisi 90-120 fps -> turhaa lampoa ja akunkulutusta.
    //  - Virransaasto: jos akku < 20 % eika lataa, kaikki 30 fps.
    //  - Naytto: ajossa NeverSleep (kallistusohjauksessa ei kosketuksia, naytto sammuisi kesken ajon); muualla jarjestelman asetus.
    //    Tauolla (timeScale 0) jarjestelman asetus, jotta unohtunut tauko ei pidä näyttöä päällä.
    //  - Application.lowMemory: vapauttaa käyttämättömät resurssit.
    public class PerformanceManager : MonoBehaviour
    {
        public static bool DisableCap;                      // PerfBenchmark mittaa ilman rajaa

        static readonly HashSet<string> RaceScenes = new HashSet<string>(Progress.RaceScenes);   // yksi ratalista: Progress.cs
        static readonly HashSet<string> SmoothScenes = new HashSet<string> { "Wash" };

        static PerformanceManager inst;
        bool race;
        int wantedFps = 30;
        float nextPoll;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (inst != null) return;
            var go = new GameObject("PerformanceManager");
            DontDestroyOnLoad(go);
            inst = go.AddComponent<PerformanceManager>();
            SceneManager.sceneLoaded += (s, m) => { if (inst != null) inst.Apply(s.name); };
            Application.lowMemory += OnLowMemory;
            inst.Apply(SceneManager.GetActiveScene().name);
        }

        static void OnLowMemory()
        {
            Debug.LogWarning("[Perf] Muisti vähissä: vapautetaan käyttämättömät resurssit.");
            Resources.UnloadUnusedAssets();
        }

        void Apply(string scene)
        {
            race = RaceScenes.Contains(scene);
            wantedFps = (race || SmoothScenes.Contains(scene)) ? 60 : 30;
            Refresh();
        }

        void Refresh()
        {
            if (DisableCap) { Application.targetFrameRate = -1; return; }
            int fps = wantedFps;
            if (BatterySaving()) fps = Mathf.Min(fps, 30);
            QualitySettings.vSyncCount = 0;                 // targetFrameRate vaikuttaa vain kun vSync on pois paalta
            Application.targetFrameRate = fps;
            bool keepAwake = race && Time.timeScale > 0f;
            Screen.sleepTimeout = keepAwake ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }

        static bool BatterySaving()
        {
            float b = SystemInfo.batteryLevel;              // -1 jos ei tiedossa (editori, tietokone)
            return b >= 0f && b < 0.2f && SystemInfo.batteryStatus == BatteryStatus.Discharging;
        }

        void Update()
        {
            if (Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + 2f;              // tauko/akku muuttuu harvoin: 2 s riittaa
            Refresh();
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused) Refresh();                         // palatessa taustalta: palauta rajat
        }
    }
}
