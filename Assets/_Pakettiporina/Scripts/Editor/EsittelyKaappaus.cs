#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pakettiporina.EditorTools
{
    // LISATTY 23.11.2026: README-materiaalin kaappaus valikosta (nappaimet eivat paassyt Simulator-ikkunaan).
    //  Pakettiporina/Esittely/Kuvakaappaus      -> Docs/Esittely/kuva_HHmmss.png (Play-tilassa)
    //  Pakettiporina/Esittely/GIF-tallennus     -> kytkee ~8 kuvaa/s tallennuksen: Docs/Esittely/gif_frames/f_0000.png ...
    public static class EsittelyKaappaus
    {
        static bool recording;
        static int index;
        static double nextFrame;
        static string dir;

        static string BaseDir()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Esittely"));
        }

        [MenuItem("Pakettiporina/Esittely/Kuvakaappaus")]
        static void Shot()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Esittely] Aloita Play-tila ensin."); return; }
            Directory.CreateDirectory(BaseDir());
            string file = Path.Combine(BaseDir(), "kuva_" + System.DateTime.Now.ToString("HHmmss") + ".png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log("[Esittely] Kuvakaappaus: " + file);
        }

        // Testaa maalin efektit (konfetti, valahdys, aani) ajon aikana ilman etta tarvitsee ajaa maaliin.
        [MenuItem("Pakettiporina/Esittely/Maalin efektit (testi)")]
        static void TestFinish()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Esittely] Aloita Play-tila ensin."); return; }
            // Oikea polku (palkkio + 1-3 tahden arvosana + tallennus); jos rata ei ole kaynnissa, pelkka efekti.
            if (RaceManager.Instance != null && RaceManager.Instance.IsRacing) RaceManager.Instance.FinishRace();
            else GameEvents.Finish();
        }

        [MenuItem("Pakettiporina/Esittely/GIF-tallennus paalle tai pois")]
        static void ToggleRecording()
        {
            recording = !recording;
            if (recording)
            {
                if (!Application.isPlaying) { Debug.LogWarning("[Esittely] Aloita Play-tila ensin."); recording = false; return; }
                dir = Path.Combine(BaseDir(), "gif_frames");
                Directory.CreateDirectory(dir);
                index = 0;
                nextFrame = 0;
                EditorApplication.update += Tick;
            }
            else EditorApplication.update -= Tick;
            Debug.Log("[Esittely] GIF-tallennus " + (recording ? "PAALLA" : "POIS (" + index + " kehysta)"));
        }

        static void Tick()
        {
            if (!recording || !Application.isPlaying) { recording = false; EditorApplication.update -= Tick; return; }
            if (EditorApplication.timeSinceStartup < nextFrame) return;
            nextFrame = EditorApplication.timeSinceStartup + 0.125;
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, "f_" + (index++).ToString("0000") + ".png"));
        }
    }
}
#endif
