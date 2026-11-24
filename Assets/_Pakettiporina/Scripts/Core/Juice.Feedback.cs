using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // 24.11.2026: Juice pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: aanet, valahdys, leijuva teksti, konfettitykit.
    public partial class Juice
    {
        // ------------------------------------------------------------------ aanet, valahdys, leijuva teksti, konfettitykit
        // Soittaa proseduraalisen aanen AudioManagerin kautta (Asetusten Aanet-kytkin huomioidaan).
        public static void Play(string key, float pitchVariance = 0.04f)
        {
            if (!Application.isPlaying || AudioManager.Instance == null) return;
            var clip = SoundSynth.Get(key);
            if (clip != null) AudioManager.Instance.PlaySfx(clip, pitchVariance);
        }

        static RectTransform overlay;

        // Oma ylimman tason Canvas (sorting 90, SceneFaderin 999 alapuolella) valahdyksille ja leijuville teksteille.
        static RectTransform Overlay()
        {
            if (overlay != null) return overlay;
            var go = new GameObject("JuiceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Object.DontDestroyOnLoad(go);
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 90;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080f, 1920f);
            cs.matchWidthOrHeight = 0.5f;
            overlay = (RectTransform)go.transform;
            return overlay;
        }

        // Koko ruudun valahdys (esim. kolhu = punainen, maali = valkoinen, boost = vaalea sininen).
        public static void Flash(Color color, float alpha = 0.3f, float duration = 0.25f)
        {
            if (!Application.isPlaying) return;
            Inst.StartCoroutine(Inst.FlashRoutine(color, alpha, duration));
        }

        IEnumerator FlashRoutine(Color color, float alpha, float duration)
        {
            var go = new GameObject("Flash", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(Overlay(), false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var im = go.GetComponent<Image>();
            im.raycastTarget = false;
            float e = 0f;
            while (e < duration)
            {
                e += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(e / duration);
                im.color = new Color(color.r, color.g, color.b, alpha * k * k);
                yield return null;
            }
            Destroy(go);
        }

        // Leijuva teksti (esim. "+1" tahden kohdalla): nousee ja haipuu.
        public static void FloatText(RectTransform near, string text, Color color)
        {
            if (near == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.FloatTextRoutine(near, text, color));
        }

        IEnumerator FloatTextRoutine(RectTransform near, string text, Color color)
        {
            var go = new GameObject("FloatText", typeof(RectTransform), typeof(Text), typeof(Outline));
            var rt = (RectTransform)go.transform;
            rt.SetParent(Overlay(), false);
            rt.sizeDelta = new Vector2(300f, 100f);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = text; t.fontSize = 64; t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter; t.color = color; t.raycastTarget = false;
            go.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
            Vector3 start = near.position;
            float e = 0f, dur = 0.8f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                float k = e / dur;
                rt.position = start + new Vector3(0f, Screen.height * 0.08f * k, 0f);
                rt.localScale = Vector3.one * (1f + 0.4f * Mathf.Sin(Mathf.Min(1f, k * 3f) * Mathf.PI * 0.5f));
                var c = t.color; c.a = 1f - k * k; t.color = c;
                yield return null;
            }
            Destroy(go);
        }

        // Konfettitykit ruudun alakulmista: pallot ammutaan ylos-sisaan ja ne putoavat kaarena.
        public static void UIConfettiCannons(RectTransform panel, int perSide = 45)
        {
            if (panel == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.CannonRoutine(panel, perSide));
        }

        IEnumerator CannonRoutine(RectTransform panel, int perSide)
        {
            var canvas = panel.GetComponentInParent<Canvas>();
            var croot = canvas != null ? (RectTransform)canvas.rootCanvas.transform : panel;
            float W = Mathf.Max(300f, croot.rect.width), H = Mathf.Max(500f, croot.rect.height);
            Color[] cs = { new Color(1f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.1f), new Color(0.2f, 0.85f, 0.4f), new Color(0.3f, 0.55f, 1f), new Color(0.9f, 0.35f, 0.95f), Color.white };
            var pcs = new List<RectTransform>(); var vel = new List<Vector2>(); var spin = new List<float>();
            float size = Mathf.Max(18f, W * 0.024f);
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < perSide; i++)
                {
                    var go = new GameObject("Cannon", typeof(RectTransform), typeof(Image));
                    var rt = (RectTransform)go.transform;
                    rt.SetParent(panel, false);
                    rt.SetSiblingIndex(Mathf.Min(1, panel.childCount - 1));
                    rt.sizeDelta = new Vector2(size, size * Random.Range(0.5f, 1.2f));
                    rt.localPosition = new Vector3(side * W * 0.5f, -H * 0.42f, 0f);
                    go.GetComponent<Image>().color = cs[Random.Range(0, cs.Length)];
                    go.GetComponent<Image>().raycastTarget = false;
                    float ang = Random.Range(55f, 80f) * Mathf.Deg2Rad;           // ylos, hieman sisaan
                    float sp = Random.Range(0.9f, 1.5f) * H;
                    vel.Add(new Vector2(-side * Mathf.Cos(ang) * sp * 0.75f, Mathf.Sin(ang) * sp));
                    spin.Add(Random.Range(-540f, 540f));
                    pcs.Add(rt);
                }
            float e = 0f, dur = 3.2f, g = H * 1.5f;
            while (e < dur)
            {
                float dt = Time.unscaledDeltaTime; e += dt;
                for (int i = 0; i < pcs.Count; i++)
                {
                    if (pcs[i] == null) continue;
                    var v = vel[i]; v.y -= g * dt; v.x *= (1f - 0.6f * dt); vel[i] = v;
                    pcs[i].localPosition += (Vector3)(v * dt);
                    pcs[i].localRotation = Quaternion.Euler(0f, 0f, spin[i] * e);
                }
                yield return null;
            }
            foreach (var p in pcs) if (p != null) Destroy(p.gameObject);
        }
    }
}
