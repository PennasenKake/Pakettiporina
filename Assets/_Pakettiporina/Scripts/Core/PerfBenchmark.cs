#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: automaattinen suorituskyky- ja muistimittaus Editorissa.
    // Kaynnistys: valikko Pakettiporina -> Suorituskyky -> 01 - Aja automaattimittaus.
    // Kay lapi scenet (valikko, halli, radat, pesu), mittaa kunkin ruudunajan/GC:n/piirtokutsut/muistin,
    // ajaa muistivuototestin (scenevaihtoja edestakaisin) ja kirjoittaa raportin Docs/Suorituskyky_<pvm>_automaattimittaus.md.
    // HUOM: Editorin ruudunajat eivat ole laitteen ruudunaikoja; piirtokutsut, kolmiot, GC ja muistin kasvu ovat kuitenkin vertailukelpoisia.
    public class PerfBenchmark : MonoBehaviour
    {
        public const string PrefKey = "Pakettiporina.AutoBench";

        struct Step { public string scene; public float seconds; public Step(string s, float t) { scene = s; seconds = t; } }

        readonly List<PerfProbe.Result> results = new List<PerfProbe.Result>();
        readonly List<float> loadTimes = new List<float>();
        readonly List<string> notes = new List<string>();
        readonly List<string> leakRows = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!EditorPrefs.GetBool(PrefKey, false)) return;
            EditorPrefs.SetBool(PrefKey, false);
            var go = new GameObject("PerfBenchmark");
            DontDestroyOnLoad(go);
            go.AddComponent<PerfBenchmark>();
        }

        IEnumerator Start()
        {
            yield return null; yield return null;
            var probe = PerfProbe.Instance;
            if (probe == null) { Debug.LogError("[Perf] PerfProbe puuttuu."); Stop(); yield break; }

            var plan = new[]
            {
                new Step("MainMenu", 8f), new Step("Garage", 8f),
                new Step("Satama", 18f), new Step("Puisto", 18f), new Step("Huvipuisto", 18f), new Step("Maatila", 18f),
                new Step("Kyla", 10f), new Step("Wash", 8f)
            };

            // Mitataan ilman FPS-rajaa ja Androidin oletuslaadulla (Balanced), jotta luvut vastaavat laitetta mahdollisimman hyvin.
            int origQuality = QualitySettings.GetQualityLevel();
            PerformanceManager.DisableCap = true;
            Application.targetFrameRate = -1;
            SetQuality("Balanced");
            QualitySettings.vSyncCount = 0;

            foreach (var st in plan) yield return Measure(st.scene, st.seconds, st.scene, probe);

            // Vertailu: Performant-profiili raskaimmilla radoilla
            SetQuality("Performant");
            QualitySettings.vSyncCount = 0;
            foreach (var s in new[] { "Maatila", "Huvipuisto" }) yield return Measure(s, 12f, s + " (Performant)", probe);
            SetQuality("Balanced");
            QualitySettings.vSyncCount = 0;

            // Muistivuototesti: edestakaisin scenevaihtoja, muisti mitataan tyhjennyksen jalkeen.
            yield return LeakTest();

            WriteReport();
            QualitySettings.SetQualityLevel(origQuality, true);
            PerformanceManager.DisableCap = false;
            Stop();
        }

        static void SetQuality(string name)
        {
            int idx = System.Array.IndexOf(QualitySettings.names, name);
            if (idx >= 0) QualitySettings.SetQualityLevel(idx, true);
        }

        IEnumerator Measure(string scene, float seconds, string label, PerfProbe probe)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene)) { notes.Add("Scene '" + scene + "' ei ole Build Settingsissa: ohitettu."); yield break; }
            Time.timeScale = 1f;
            float t0 = Time.realtimeSinceStartup;
            var op = SceneManager.LoadSceneAsync(scene);
            while (!op.isDone) yield return null;
            yield return null;
            loadTimes.Add(Time.realtimeSinceStartup - t0);
            yield return new WaitForSecondsRealtime(2f);           // asettuminen (lahtolaskenta, UI:n rakennus)
            probe.BeginCapture(label);
            yield return new WaitForSecondsRealtime(seconds);
            var r = probe.EndCapture();
            results.Add(r);
            Debug.Log("[Perf] " + PerfProbe.Describe(r));
        }

        IEnumerator LeakTest()
        {
            string[] pair = { "MainMenu", "Satama", "Garage", "Huvipuisto" };
            for (int loop = 0; loop < 6; loop++)
            {
                foreach (var s in pair)
                {
                    if (!Application.CanStreamedLevelBeLoaded(s)) continue;
                    var op = SceneManager.LoadSceneAsync(s);
                    while (!op.isDone) yield return null;
                    yield return new WaitForSecondsRealtime(1.2f);
                }
                // takaisin valikkoon, tyhjennys, mittaus
                var back = SceneManager.LoadSceneAsync("MainMenu");
                while (!back.isDone) yield return null;
                yield return Resources.UnloadUnusedAssets();
                System.GC.Collect();
                yield return new WaitForSecondsRealtime(0.8f);
                leakRows.Add(string.Format("| {0} | {1:0.0} | {2:0.0} | {3:0.0} | {4} |", loop + 1,
                    PerfProbe.TotalUsedMB(), PerfProbe.TextureMB(), PerfProbe.MonoMB(), PerfProbe.TextureCountNow()));
            }
        }

        void WriteReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Suorituskyky: automaattimittaus " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            sb.AppendLine();
            sb.AppendLine("Mitattu Unity Editorissa (Play-tila). **Editorin ruudunajat eivät ole puhelimen ruudunaikoja**; piirtokutsut, kolmiot, GC-allokaatiot ja muistin kasvu ovat kuitenkin vertailukelpoisia. Laitetesti tarvitaan vielä (PerfProbe-overlay: neljän sormen kosketus 1 s Development Buildissa).");
            sb.AppendLine();
            sb.AppendLine("Ympäristö: " + SystemInfo.processorType + ", " + SystemInfo.graphicsDeviceName + ", RAM " + SystemInfo.systemMemorySize + " MB, kuva " + Screen.width + "x" + Screen.height
                + ", laatutaso mittauksessa Balanced (Androidin oletus; Performant-vertailu lopussa), FPS-raja pois mittauksen ajaksi");
            sb.AppendLine();
            sb.AppendLine("## Ruudunaika ja piirto per scene");
            sb.AppendLine();
            sb.AppendLine("| Scene | fps | ka ms | p95 ms | p99 ms | pahin ms | >33 ms | GC B/ruutu | GC max B | piirtokutsut (ka/max) | SetPass (ka/max) | kolmiot (ka/max) |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var r in results)
                sb.AppendFormat("| {0} | {1:0} | {2:0.0} | {3:0.0} | {4:0.0} | {5:0.0} | {6} | {7:0} | {8:0} | {9:0}/{10:0} | {11:0}/{12:0} | {13:0}/{14:0} |\n",
                    r.label, r.avgFps, r.avgMs, r.p95Ms, r.p99Ms, r.worstMs, r.hitches33, r.gcPerFrameBytes, r.gcMaxFrameBytes, r.drawAvg, r.drawMax, r.setPassAvg, r.setPassMax, r.trisAvg, r.trisMax);
            sb.AppendLine();
            sb.AppendLine("## Muisti per scene (kaappauksen lopussa)");
            sb.AppendLine();
            sb.AppendLine("| Scene | yhteensä MB | gfx MB | tekstuurit MB | meshit MB | ääni MB | GC-keko MB | Mono MB | tekstuureja | mesh-objekteja |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (var r in results)
                sb.AppendFormat("| {0} | {1:0.0} | {2:0.0} | {3:0.0} | {4:0.0} | {5:0.0} | {6:0.0} | {7:0.0} | {8} | {9} |\n",
                    r.label, r.totalMemMB, r.gfxMemMB, r.textureMemMB, r.meshMemMB, r.audioMemMB, r.gcMemMB, r.monoMB, r.textureCount, r.meshCount);
            sb.AppendLine();
            sb.AppendLine("## Latausajat (LoadSceneAsync, sekuntia)");
            sb.AppendLine();
            for (int i = 0; i < loadTimes.Count && i < results.Count; i++) sb.AppendFormat("- {0}: {1:0.00} s\n", results[i].label, loadTimes[i]);
            sb.AppendLine();
            sb.AppendLine("## Muistivuototesti");
            sb.AppendLine();
            sb.AppendLine("Kierros = MainMenu → Satama → Garage → Huvipuisto → MainMenu, jonka jälkeen UnloadUnusedAssets + GC. Jos luvut kasvavat tasaisesti kierroksittain, muistia vuotaa.");
            sb.AppendLine();
            sb.AppendLine("| Kierros | yhteensä MB | tekstuurit MB | Mono MB | tekstuureja |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var row in leakRows) sb.AppendLine(row);
            if (leakRows.Count >= 2)
            {
                var first = leakRows[1].Split('|'); var last = leakRows[leakRows.Count - 1].Split('|');
                float a, b;
                if (float.TryParse(first[2].Trim(), out a) && float.TryParse(last[2].Trim(), out b))
                    sb.AppendFormat("\n**Kasvu kierroksesta 2 kierrokseen {0}: {1:+0.0;-0.0;0.0} MB.**\n", leakRows.Count, b - a);
            }
            if (notes.Count > 0) { sb.AppendLine(); sb.AppendLine("## Huomiot"); foreach (var n in notes) sb.AppendLine("- " + n); }

            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs"));
            string file = Path.Combine(dir, "Suorituskyky_" + System.DateTime.Now.ToString("yyyy-MM-dd") + "_automaattimittaus.md");
            File.WriteAllText(file, sb.ToString(), new UTF8Encoding(true));
            Debug.Log("[Perf] Raportti kirjoitettu: " + file);
        }

        void Stop()
        {
            EditorApplication.isPlaying = false;
        }
    }
}
#endif
