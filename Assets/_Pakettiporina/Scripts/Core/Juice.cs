using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: "mehukkuus" (TODO kohta 3). Yksi paikka pienille palautteille:
    //  - poolattu partikkelisysteemi (kimallus, kipinat, roiske, konfetti, nopeusviivat)
    //  - UI-pulssit (Pulse), auton puristus (Squash), napin painuma (JuiceButtonPress, lisataan automaattisesti)
    //  - UI-hiukkaset (UIBurst) pesun kuplan poksahdukseen
    // Kaikki rakennetaan koodissa; partikkelimateriaali luodaan ajonaikaisesti (Sprites/Default, mukana aina buildissa),
    // joten ei uusia binaaritiedostoja (Git LFS) eika scene-muutoksia.
    public partial class Juice : MonoBehaviour
    {
        static Juice inst;
        static Material particleMat;
        static Texture2D dotTex;
        static Sprite dotSprite;
        const int PoolSize = 14;
        readonly List<ParticleSystem> pool = new List<ParticleSystem>();
        int nextReuse;
        readonly Dictionary<Transform, Coroutine> pulses = new Dictionary<Transform, Coroutine>();
        readonly Dictionary<Transform, Vector3> baseScales = new Dictionary<Transform, Vector3>();
        float nextButtonScan;
#if UNITY_EDITOR
        bool recording; string recDir; int recIndex; float nextFrame;
#endif

        static Juice Inst
        {
            get
            {
                if (inst == null)
                {
                    var go = new GameObject("Juice");
                    DontDestroyOnLoad(go);
                    inst = go.AddComponent<Juice>();
                }
                return inst;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var _ = Inst;
            // SUORITUSKYKY 23.11.2026: nappien skannaus tiheaan vain scenen latauksen jalkeen (3 s), sen jalkeen harvaan.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (s, m) => { if (inst != null) { inst.scanBurstUntil = Time.unscaledTime + 3f; inst.nextButtonScan = 0f; } };
            if (inst != null) inst.scanBurstUntil = Time.unscaledTime + 3f;
        }
        float scanBurstUntil;

        // ------------------------------------------------------------------ materiaali / sprite
        static Texture2D DotTexture()
        {
            if (dotTex != null) return dotTex;
            const int N = 32;
            dotTex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(N / 2f, N / 2f)) / (N / 2f);
                    float a = 1f - Mathf.SmoothStep(0.55f, 1f, d);
                    px[y * N + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            dotTex.SetPixels32(px);
            dotTex.Apply();
            return dotTex;
        }

        static Sprite DotSprite()
        {
            if (dotSprite == null) dotSprite = Sprite.Create(DotTexture(), new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);
            return dotSprite;
        }

        static Material ParticleMaterial()
        {
            if (particleMat != null) return particleMat;
            var sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            particleMat = new Material(sh) { name = "JuiceParticle", mainTexture = DotTexture() };
            return particleMat;
        }

        // ------------------------------------------------------------------ partikkelit
        ParticleSystem GetSystem()
        {
            // vapaa (ei elossa) ensin
            foreach (var p in pool) if (p != null && !p.IsAlive(true)) return p;
            if (pool.Count < PoolSize) return Create();
            // pooli taynna: kierratetaan vanhin
            nextReuse = (nextReuse + 1) % pool.Count;
            var reuse = pool[nextReuse];
            reuse.Clear();
            return reuse;
        }

        ParticleSystem Create()
        {
            var go = new GameObject("JuiceParticles");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true;
            main.maxParticles = 200;
            var em = ps.emission; em.enabled = false;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.15f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.2f)));

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.material = ParticleMaterial();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sortingOrder = 500;
            pool.Add(ps);
            return ps;
        }

        static void Emit(Vector3 pos, Color a, Color b, int count, float speed, float size, float life, float gravity, float radius = 0.15f)
        {
            if (!Application.isPlaying) return;
            var ps = Inst.GetSystem();
            if (ps == null) return;
            ps.transform.position = pos;
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(a, b);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.7f, life);
            main.gravityModifier = gravity;
            var shape = ps.shape; shape.radius = radius;
            ps.Emit(count);
            ps.Play();
        }

        // Tahden kimallus (keltainen/valkoinen, leijuu ylos).
        public static void Sparkle(Vector3 pos)
        {
            Emit(pos + Vector3.up * 0.5f, new Color(1f, 0.9f, 0.2f), Color.white, 18, 4.5f, 0.35f, 0.7f, -0.2f, 0.25f);
        }

        // Kolhun kipinat (oranssi/keltainen, putoavat).
        public static void Sparks(Vector3 pos)
        {
            Emit(pos + Vector3.up * 0.4f, new Color(1f, 0.55f, 0.1f), new Color(1f, 0.9f, 0.3f), 16, 6f, 0.22f, 0.5f, 1.2f, 0.2f);
        }

        // Latakon roiske (sinisia pisaroita).
        public static void Splash(Vector3 pos)
        {
            Emit(pos + Vector3.up * 0.2f, new Color(0.35f, 0.65f, 0.95f), new Color(0.75f, 0.9f, 1f), 24, 5f, 0.3f, 0.7f, 1.6f, 0.4f);
        }

        // Maalin konfetti: monivarinen sade kameran edessa.
        public static void Confetti(Camera cam)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            var t = cam.transform;
            // PAIVITETTY: isompi, kirkkaampi ja leveampi (ensimmainen versio oli ~4 px himmea pilkku).
            Vector3 center = t.position + t.forward * 9f + t.up * 5f;
            Color[] cs = { new Color(1f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.1f), new Color(0.2f, 0.85f, 0.4f), new Color(0.3f, 0.55f, 1f), new Color(0.9f, 0.35f, 0.95f) };
            for (int i = 0; i < cs.Length; i++)
                Emit(center, cs[i], cs[(i + 1) % cs.Length], 40, 8f, 0.75f, 2.6f, 0.9f, 4.5f);
        }

        // Boostin nopeusviivat: valkoisia viiruja auton takana kestoajan.
        public static void BoostTrail(Transform car, float duration)
        {
            if (car == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.BoostTrailRoutine(car, duration));
        }

        IEnumerator BoostTrailRoutine(Transform car, float duration)
        {
            float t = 0f;
            while (t < duration && car != null)
            {
                t += 0.06f;
                Vector3 back = car.position - car.forward * 1.6f + Vector3.up * 0.5f;
                Emit(back, new Color(1f, 1f, 1f, 0.9f), new Color(0.7f, 0.9f, 1f, 0.8f), 3, 1.5f, 0.18f, 0.35f, 0f, 0.5f);
                yield return new WaitForSeconds(0.06f);
            }
        }

        // ------------------------------------------------------------------ pulssit / puristus
        // Kasvattaa Transformin hetkeksi (peak x) ja palauttaa. Turvallinen kutsua peräkkäin.
        public static void Pulse(Transform t, float peak = 1.25f, float duration = 0.25f)
        {
            if (t == null || !Application.isPlaying) return;
            var j = Inst;
            if (!j.baseScales.ContainsKey(t)) j.baseScales[t] = t.localScale;
            if (j.pulses.TryGetValue(t, out var running) && running != null) j.StopCoroutine(running);
            j.pulses[t] = j.StartCoroutine(j.PulseRoutine(t, j.baseScales[t], peak, duration));
        }

        IEnumerator PulseRoutine(Transform t, Vector3 baseScale, float peak, float duration)
        {
            float e = 0f;
            while (e < duration && t != null)
            {
                e += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(e / duration);
                // nopea nousu, pehmea lasku pienella yliheitolla
                float s = k < 0.3f ? Mathf.Lerp(1f, peak, k / 0.3f) : Mathf.Lerp(peak, 1f, (k - 0.3f) / 0.7f);
                t.localScale = baseScale * s;
                yield return null;
            }
            if (t != null) { t.localScale = baseScale; pulses.Remove(t); baseScales.Remove(t); }
        }

        // Puristus: litistyy pystysuunnassa ja levenee sivuille, palautuu. Kayta auton visuaaliin (ei fysiikkaan).
        public static void Squash(Transform visual, float amount = 0.18f, float duration = 0.3f)
        {
            if (visual == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.SquashRoutine(visual, amount, duration));
        }

        IEnumerator SquashRoutine(Transform t, float amount, float duration)
        {
            Vector3 b = t.localScale;
            float e = 0f;
            while (e < duration && t != null)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / duration);
                float w = Mathf.Sin(k * Mathf.PI) * Mathf.Exp(-2f * k); // vaimeneva heilahdus
                t.localScale = new Vector3(b.x * (1f + amount * w), b.y * (1f - amount * 1.2f * w), b.z * (1f + amount * w));
                yield return null;
            }
            if (t != null) t.localScale = b;
        }

        // --- siirretty tiedostoon Juice.UIEffects.cs (UI-hiukkaset, pilleri-sprite, UI-konfetti) ---
        // --- siirretty tiedostoon Juice.HudSprites.cs (proseduraaliset HUD-spritet) ---

        // --- siirretty tiedostoon Juice.Feedback.cs (aanet, valahdys, leijuva teksti, konfettitykit) ---

        // ------------------------------------------------------------------ nappien painuma
        void Update()
        {
#if UNITY_EDITOR
            // Vain editorissa: README-materiaalin kaappaus. F12 = kuvakaappaus (Docs/Esittely/kuva_*.png),
            // F7 = aloita/lopeta GIF-kehysten tallennus (Docs/Esittely/gif_frames/f_*.png, ~8 kuvaa/s).
            if (Input.GetKeyDown(KeyCode.F12))
            {
                string dir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Docs/Esittely"));
                System.IO.Directory.CreateDirectory(dir);
                string file = System.IO.Path.Combine(dir, "kuva_" + System.DateTime.Now.ToString("HHmmss") + ".png");
                ScreenCapture.CaptureScreenshot(file);
                Debug.Log("[Juice] Kuvakaappaus: " + file);
            }
            if (Input.GetKeyDown(KeyCode.F7))
            {
                recording = !recording;
                if (recording)
                {
                    recDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Docs/Esittely/gif_frames"));
                    System.IO.Directory.CreateDirectory(recDir);
                    recIndex = 0; nextFrame = 0f;
                }
                Debug.Log("[Juice] GIF-tallennus " + (recording ? "PAALLA" : "POIS") + " (" + recIndex + " kehysta)");
            }
            if (recording && Time.unscaledTime >= nextFrame)
            {
                nextFrame = Time.unscaledTime + 0.125f;
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(recDir, "f_" + (recIndex++).ToString("0000") + ".png"));
            }
#endif
#if UNITY_EDITOR
            // Vain editorissa: efektien pikatesti ajon aikana (F8 kimallus, F9 konfetti, F10 kipinat, F11 roiske).
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 p = cam.transform.position + cam.transform.forward * 8f - Vector3.up * 2f;
                if (Input.GetKeyDown(KeyCode.F8)) Sparkle(p);
                if (Input.GetKeyDown(KeyCode.F9)) { var fp = FindObjectOfType<RaceHUD>(); if (fp != null && fp.finishPanel != null) UIConfetti(fp.finishPanel.transform as RectTransform); }
                if (Input.GetKeyDown(KeyCode.F10)) Sparks(p);
                if (Input.GetKeyDown(KeyCode.F11)) Splash(p);
            }
#endif
            if (Time.unscaledTime < nextButtonScan) return;
            bool burst = Time.unscaledTime < scanBurstUntil;
            nextButtonScan = Time.unscaledTime + (burst ? 0.25f : 1.5f);
            // includeInactive vain purskeessa: piilossa olevien paneelien napit saavat painumisen heti kun ne aukeavat
            foreach (var b in FindObjectsOfType<Button>(burst))
            {
                if (b == null || b.GetComponent<JuiceButtonPress>() != null) continue;
                // kuplat/korjaustyokalu animoivat itse mittakaavaansa
                if (b.GetComponent<Bubble>() != null || b.GetComponent<RepairTool>() != null) continue;
                // ajon ohjausnapit (pidettavat) jatetaan rauhaan: ei mittakaavamuutoksia (kayttaja: ohjausnapit havisivat)
                if (b.GetComponent<TouchButton>() != null || b.GetComponentInParent<TouchButton>() != null) continue;
                b.gameObject.AddComponent<JuiceButtonPress>();
            }
        }
    }

    // Lisataan automaattisesti kaikille Button-komponenteille: painaessa nappi puristuu, vapautuksessa pomppaa takaisin.
    public class JuiceButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        Vector3 baseScale;
        bool down;

        public void OnPointerDown(PointerEventData e)
        {
            if (transform.localScale.sqrMagnitude < 0.01f) return; // suojaus: ei kosketa piilotettuihin/nollaskaalattuihin
            if (!down) baseScale = transform.localScale;
            down = true;
            transform.localScale = baseScale * 0.9f;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!down) return;
            down = false;
            transform.localScale = baseScale;
            Juice.Pulse(transform, 1.12f, 0.2f);
        }

        void OnDisable()
        {
            if (down) { transform.localScale = baseScale; down = false; }
        }
    }
}
