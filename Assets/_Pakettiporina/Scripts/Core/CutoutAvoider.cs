using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026 (Play-testi: Koti-nappi ja asetusten sulkunappi jaavat kameran reiän
    // alle - Galaxy S10e simulaattorissa). Siirtaa painikkeet (Selectable) sivuun, jos ne
    // osuvat laitteen kameraloveen/lovialueeseen (Screen.cutouts). Ei muuta layoutia
    // muuten - vain vaikuttaville napeille pieni vaakasiirto. Luo itsensa automaattisesti.
    public class CutoutAvoider : MonoBehaviour
    {
        const float Margin = 12f; // pikselia

        static CutoutAvoider instance;
        readonly HashSet<int> handled = new HashSet<int>();
        float nextScan, burstUntil;
        static readonly Vector3[] corners = new Vector3[4];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (instance != null) return;
            var go = new GameObject("CutoutAvoider");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CutoutAvoider>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (s, m) => { instance.handled.Clear(); instance.burstUntil = Time.unscaledTime + 3f; };
            instance.burstUntil = Time.unscaledTime + 3f;
        }

        void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            // SUORITUSKYKY 23.11.2026: 0,1 s vain scenen latauksen jalkeen (3 s) ja kun jokin napeista on juuri avautunut;
            // muulloin 0,25 s (aiemmin aina 0,1 s = FindObjectsOfType 10 kertaa sekunnissa koko pelin ajan).
            nextScan = Time.unscaledTime + (Time.unscaledTime < burstUntil ? 0.1f : 0.25f);

            var cutouts = Screen.cutouts;
            if (cutouts == null || cutouts.Length == 0) return;

            foreach (var sel in FindObjectsOfType<Selectable>())
            {
                if (sel == null || !sel.isActiveAndEnabled) continue;
                int id = sel.GetInstanceID();
                if (handled.Contains(id)) continue;

                var rt = sel.transform as RectTransform;
                var canvas = sel.GetComponentInParent<Canvas>();
                if (rt == null || canvas == null) continue;
                canvas = canvas.rootCanvas;
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera == null) continue;

                handled.Add(id); // kasitellaan kerran per painike

                var r = ScreenRect(rt, canvas);
                foreach (var cut in cutouts)
                {
                    if (!r.Overlaps(cut)) continue;
                    // siirrytaan vaakasuunnassa pois: poispain lovesta (lahemmalle ruudun keskelle)
                    float cutCenter = cut.center.x;
                    float shift;
                    if (r.center.x >= cutCenter) shift = cut.xMax - r.xMin + Margin;       // oikealle
                    else shift = -(r.xMax - cut.xMin + Margin);                              // vasemmalle
                    // jos oikealle siirto veisi ruudun ulkopuolelle, siirrytaan vasemmalle
                    if (r.xMax + shift > Screen.width) shift = -(r.xMax - cut.xMin + Margin);
                    if (r.xMin + shift < 0) shift = cut.xMax - r.xMin + Margin;
                    float scale = Mathf.Max(0.0001f, canvas.scaleFactor);
                    var p = rt.anchoredPosition;
                    p.x += shift / scale;
                    rt.anchoredPosition = p;
                    Debug.Log("[CutoutAvoider] Siirretty '" + sel.name + "' pois kameralovesta (" + shift.ToString("0") + " px).");
                    break;
                }
            }
        }

        static Rect ScreenRect(RectTransform rt, Canvas canvas)
        {
            rt.GetWorldCorners(corners);
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 b = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
    }
}
