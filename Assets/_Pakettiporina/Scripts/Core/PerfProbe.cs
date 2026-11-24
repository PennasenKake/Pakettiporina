using System;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: suorituskykymittari. Toimii Editorissa ja Development Buildissa (laitteella).
    //  - Tallennus: BeginCapture(nimi) ... EndCapture() -> Result (fps, ruudunaika, GC, piirtokutsut, muisti).
    //  - Naytolle FPS-overlay: Editorissa F3, laitteella (Development Build) neljan sormen kosketus 1 s.
    // Release-buildissa ei tee mitaan (ei luoda).
    public class PerfProbe : MonoBehaviour
    {
        public static PerfProbe Instance { get; private set; }

        public struct Result
        {
            public string label;
            public int frames;
            public float seconds, avgFps, avgMs, medianMs, p95Ms, p99Ms, worstMs;
            public int hitches33, hitches50;        // ruutuja yli 33 ms / 50 ms
            public float gcPerFrameBytes, gcMaxFrameBytes;
            public int gcCollections;               // GC.Collect-kierrokset (gen0) kaappauksen aikana
            public float drawAvg, drawMax, setPassAvg, setPassMax, trisAvg, trisMax;
            public float totalMemMB, gfxMemMB, textureMemMB, meshMemMB, audioMemMB, gcMemMB, monoMB;
            public int textureCount, meshCount;
            public float batteryStart, batteryEnd;
        }

        ProfilerRecorder rDraw, rSetPass, rTris, rGcFrame, rTotal, rGfx, rTex, rMesh, rAudio, rGc, rTexCount, rMeshCount;
        readonly List<float> frames = new List<float>(4096);
        readonly List<float> gcSamples = new List<float>(4096);
        bool capturing;
        string label;
        float capStart;
        int gcStart;
        float batStart;
        double drawSum, setSum, trisSum;
        float drawMax, setMax, trisMax;

        // overlay
        bool overlay;
        float fpsSmoothed, ms, gcLast;
        float touchHold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            if (!Application.isEditor && !Debug.isDebugBuild) return;   // release: ei lainkaan
            var go = new GameObject("PerfProbe");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PerfProbe>();
        }

        void OnEnable()
        {
            rDraw = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            rSetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            rTris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            rGcFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            rTotal = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            rGfx = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            rTex = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            rMesh = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            rAudio = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Audio Used Memory");
            rGc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            rTexCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Count");
            rMeshCount = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Count");
        }

        void OnDisable()
        {
            rDraw.Dispose(); rSetPass.Dispose(); rTris.Dispose(); rGcFrame.Dispose(); rTotal.Dispose(); rGfx.Dispose();
            rTex.Dispose(); rMesh.Dispose(); rAudio.Dispose(); rGc.Dispose(); rTexCount.Dispose(); rMeshCount.Dispose();
        }

        static float Last(ProfilerRecorder r) { return r.Valid ? (float)r.LastValue : 0f; }
        const float MB = 1024f * 1024f;

        public void BeginCapture(string name)
        {
            label = name; capturing = true;
            frames.Clear(); gcSamples.Clear();
            drawSum = setSum = trisSum = 0; drawMax = setMax = trisMax = 0;
            capStart = Time.realtimeSinceStartup;
            gcStart = GC.CollectionCount(0);
            batStart = SystemInfo.batteryLevel;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
            {
                ms = dt * 1000f;
                fpsSmoothed = Mathf.Lerp(fpsSmoothed <= 0 ? 1f / dt : fpsSmoothed, 1f / dt, 0.1f);
            }
            gcLast = Last(rGcFrame);

            if (capturing)
            {
                frames.Add(dt * 1000f);
                gcSamples.Add(gcLast);
                float d = Last(rDraw), s = Last(rSetPass), t = Last(rTris);
                drawSum += d; setSum += s; trisSum += t;
                if (d > drawMax) drawMax = d; if (s > setMax) setMax = s; if (t > trisMax) trisMax = t;
            }

            // overlay-kytkin
            if (Application.isEditor) { if (Input.GetKeyDown(KeyCode.F3)) SetOverlay(!overlay); }
            else
            {
                if (Input.touchCount >= 4) { touchHold += dt; if (touchHold > 1f) { SetOverlay(!overlay); touchHold = -2f; } }
                else touchHold = 0f;
            }
        }

        public Result EndCapture()
        {
            capturing = false;
            var r = new Result { label = label, frames = frames.Count };
            r.seconds = Time.realtimeSinceStartup - capStart;
            if (frames.Count == 0) return r;

            var sorted = new List<float>(frames); sorted.Sort();
            float sum = 0f; foreach (var f in frames) sum += f;
            r.avgMs = sum / frames.Count;
            r.avgFps = 1000f / Mathf.Max(0.001f, r.avgMs);
            r.medianMs = sorted[sorted.Count / 2];
            r.p95Ms = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.95f))];
            r.p99Ms = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.99f))];
            r.worstMs = sorted[sorted.Count - 1];
            foreach (var f in frames) { if (f > 33.4f) r.hitches33++; if (f > 50f) r.hitches50++; }

            float gsum = 0f, gmax = 0f; foreach (var g in gcSamples) { gsum += g; if (g > gmax) gmax = g; }
            r.gcPerFrameBytes = gsum / gcSamples.Count; r.gcMaxFrameBytes = gmax;
            r.gcCollections = GC.CollectionCount(0) - gcStart;

            int n = frames.Count;
            r.drawAvg = (float)(drawSum / n); r.drawMax = drawMax;
            r.setPassAvg = (float)(setSum / n); r.setPassMax = setMax;
            r.trisAvg = (float)(trisSum / n); r.trisMax = trisMax;

            r.totalMemMB = Last(rTotal) / MB; r.gfxMemMB = Last(rGfx) / MB; r.textureMemMB = Last(rTex) / MB;
            r.meshMemMB = Last(rMesh) / MB; r.audioMemMB = Last(rAudio) / MB; r.gcMemMB = Last(rGc) / MB;
            r.monoMB = Profiler.GetMonoUsedSizeLong() / MB;
            r.textureCount = (int)Last(rTexCount); r.meshCount = (int)Last(rMeshCount);
            r.batteryStart = batStart; r.batteryEnd = SystemInfo.batteryLevel;
            return r;
        }

        // Muistin tilannekuva (esim. vuototestiin) ilman kaappausta.
        public static float TotalUsedMB() { return Instance != null ? Last(Instance.rTotal) / MB : 0f; }
        public static float TextureMB() { return Instance != null ? Last(Instance.rTex) / MB : 0f; }
        public static float MonoMB() { return Profiler.GetMonoUsedSizeLong() / MB; }
        public static int TextureCountNow() { return Instance != null ? (int)Last(Instance.rTexCount) : 0; }

        public static string Describe(Result r)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}: {1:0.0} fps (keskiarvo {2:0.0} ms, p95 {3:0.0}, p99 {4:0.0}, pahin {5:0.0} ms), ruutuja >33 ms: {6}, GC/ruutu {7:0} B, piirtokutsut {8:0} (max {9:0}), kolmioita {10:0}, muisti {11:0.0} MB",
                r.label, r.avgFps, r.avgMs, r.p95Ms, r.p99Ms, r.worstMs, r.hitches33, r.gcPerFrameBytes, r.drawAvg, r.drawMax, r.trisAvg, r.totalMemMB);
            return sb.ToString();
        }

        // Overlay on OMA komponenttinsa, joka luodaan vain kun overlay on paalla: OnGUI allokoi muistia joka ruutu
        // (IMGUI-tapahtumat), joten se ei saa olla PerfProbessa itsessaan - muuten mittari sotkisi oman GC-mittauksensa.
        void SetOverlay(bool on)
        {
            overlay = on;
            var o = GetComponent<PerfOverlay>();
            if (on && o == null) gameObject.AddComponent<PerfOverlay>();
            else if (!on && o != null) Destroy(o);
        }

        public string OverlayText()
        {
            return string.Format("{0:0} fps  {1:0.0} ms\nGC/ruutu {2:0} B\nmuisti {3:0} MB (tex {4:0})\npiirto {5:0}  kolmiot {6:0}\nakku {7:0}%",
                fpsSmoothed, ms, gcLast, Last(rTotal) / MB, Last(rTex) / MB, Last(rDraw), Last(rTris), SystemInfo.batteryLevel * 100f);
        }
    }

    public class PerfOverlay : MonoBehaviour
    {
        GUIStyle style;
        void OnGUI()
        {
            if (PerfProbe.Instance == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(18, Screen.height / 45), fontStyle = FontStyle.Bold };
                style.normal.textColor = Color.yellow;
            }
            GUI.Label(new Rect(20, 20, Screen.width, Screen.height / 3f), PerfProbe.Instance.OverlayText(), style);
        }
    }
}
