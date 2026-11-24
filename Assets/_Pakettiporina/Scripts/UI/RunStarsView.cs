using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: maalipaneelin 1-3 tahden nayttö. Rakentuu ajonaikaisesti maalipaneelin paalle:
    // kolme tahtea ponnahtaa esiin yksi kerrallaan (ansaitut kullalla, puuttuvat harmaana + vihje mika puuttui),
    // alla mahdollinen "Uusi: ..." -palkintorivi kun jokin avautui.
    public class RunStarsView : MonoBehaviour
    {
        static readonly Color Gold = new Color(1f, 0.82f, 0.15f);
        static readonly Color Empty = new Color(1f, 1f, 1f, 0.28f);

        public static void Show(RectTransform panel, int stars, string hint, List<string> unlocked)
        {
            if (panel == null) return;
            var old = panel.Find("RunStars");
            if (old != null) Destroy(old.gameObject);

            var rootGo = new GameObject("RunStars", typeof(RectTransform));
            var rt = (RectTransform)rootGo.transform;
            rt.SetParent(panel, false);
            rt.anchorMin = new Vector2(0.08f, 0.60f); rt.anchorMax = new Vector2(0.92f, 0.83f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rootGo.AddComponent<RunStarsView>().StartCoroutine(Run(rt, stars, hint, unlocked));
        }

        static RectTransform Cell(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static TextMeshProUGUI Label(Transform parent, string s, float size, Color c, float x0, float y0, float x1, float y1)
        {
            var rt = Cell("Text", parent, x0, y0, x1, y1);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = s; t.color = c; t.alignment = TextAlignmentOptions.Center; t.fontStyle = FontStyles.Bold;
            t.enableAutoSizing = true; t.fontSizeMax = size; t.fontSizeMin = 24f; t.raycastTarget = false;
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        static IEnumerator Run(RectTransform rt, int stars, string hint, List<string> unlocked)
        {
            var icons = new Image[3];
            var sprite = Juice.StarIcon();
            for (int i = 0; i < 3; i++)
            {
                var c = Cell("Star" + i, rt, i / 3f + 0.02f, 0.38f, (i + 1) / 3f - 0.02f, 1f);
                var img = c.gameObject.AddComponent<Image>();
                img.sprite = sprite; img.preserveAspect = true; img.raycastTarget = false;
                img.color = Empty;
                icons[i] = img;
            }
            var hintText = Label(rt, hint ?? "", 44, Color.white, 0f, 0.16f, 1f, 0.38f);
            var newText = Label(rt, "", 48, Gold, 0f, 0f, 1f, 0.16f);
            hintText.alpha = 0f;

            yield return new WaitForSecondsRealtime(0.6f);

            for (int i = 0; i < 3; i++)
            {
                if (i < stars)
                {
                    icons[i].color = Gold;
                    Juice.Pulse(icons[i].transform, 1.5f, 0.35f);
                    Juice.Play("pluck", 0f);
                    Juice.UIBurst((RectTransform)icons[i].transform, Gold, 12);
                    yield return new WaitForSecondsRealtime(0.38f);
                }
            }
            if (!string.IsNullOrEmpty(hint)) hintText.alpha = 1f;

            if (unlocked != null && unlocked.Count > 0)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                Juice.Play("confetti", 0f);
                for (int i = 0; i < unlocked.Count; i++)
                {
                    newText.text = unlocked[i];
                    Juice.Pulse(newText.transform, 1.25f, 0.3f);
                    yield return new WaitForSecondsRealtime(1.6f);
                }
            }
        }

        // Vihje mika tahti puuttui: lapselle ymmarrettava.
        public static string HintFor(int stars, int bonusTotal)
        {
            if (stars >= 3) return "Täydellinen ajo!";
            if (stars == 2) return bonusTotal > 0 ? "Kerää kaikki tähdet saadaksesi kolmannen!" : "Aja vielä nopeammin kolmanteen tähteen!";
            return "Väistä esteet, niin paketti pysyy ehjänä!";
        }
    }
}
