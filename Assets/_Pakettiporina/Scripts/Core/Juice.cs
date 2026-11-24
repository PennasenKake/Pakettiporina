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
    public class Juice : MonoBehaviour
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

        // ------------------------------------------------------------------ UI-hiukkaset
        // Kuplan poksahdus: pienia pisaroita lentaa ulos annetun UI-elementin kohdalta.
        public static void UIBurst(RectTransform at, Color color, int count = 10)
        {
            if (at == null || at.parent == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.UIBurstRoutine(at, color, count));
        }

        IEnumerator UIBurstRoutine(RectTransform at, Color color, int count)
        {
            var parent = at.parent;
            var dots = new List<RectTransform>();
            var vel = new List<Vector2>();
            Vector3 origin = at.localPosition;
            float size = Mathf.Clamp(Mathf.Min(at.rect.width, at.rect.height) * 0.12f, 10f, 28f);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("JuiceDot", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(parent, false);
                rt.localPosition = origin;
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(size, size);
                var im = go.GetComponent<Image>();
                im.sprite = DotSprite();
                im.raycastTarget = false;
                im.color = Color.Lerp(color, Color.white, Random.value * 0.6f);
                rt.SetAsLastSibling();
                float ang = (i / (float)count) * Mathf.PI * 2f + Random.value * 0.5f;
                vel.Add(new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * Random.Range(180f, 420f));
                dots.Add(rt);
            }
            float e = 0f, dur = 0.5f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                float k = e / dur;
                for (int i = 0; i < dots.Count; i++)
                {
                    if (dots[i] == null) continue;
                    var v = vel[i];
                    v.y -= 600f * Time.unscaledDeltaTime;
                    vel[i] = v;
                    dots[i].localPosition += (Vector3)(v * Time.unscaledDeltaTime);
                    dots[i].localScale = Vector3.one * (1f - k * 0.8f);
                    var im = dots[i].GetComponent<Image>();
                    var c = im.color; c.a = 1f - k; im.color = c;
                }
                yield return null;
            }
            foreach (var d in dots) if (d != null) Destroy(d.gameObject);
        }

        // ------------------------------------------------------------------ pilleri-sprite + UI-konfetti
        static Sprite pillSprite;

        // Pyoristetty suorakulmio (9-slice), kaytetaan HUD-tekstien taustalaattoihin.
        public static Sprite PillSprite()
        {
            if (pillSprite != null) return pillSprite;
            const int W = 64, H = 32, R = 16;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, R, W - R), cy = Mathf.Clamp(y + 0.5f, R, H - R);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(R - d + 0.5f);
                    px[y * W + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            pillSprite = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(R, R, R, R));
            return pillSprite;
        }

        // ------------------------------------------------------------------ HUD-sprite:t (proseduraaliset, ei binaaritiedostoja)
        static Sprite barFrame, barFill, starIcon, clockIcon;

        static Sprite MakeSprite(int w, int h, System.Func<int, int, Color32> f, Vector4 border)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        static Color32 C32(float r, float g, float b, float a) { return new Color32((byte)(Mathf.Clamp01(r) * 255f), (byte)(Mathf.Clamp01(g) * 255f), (byte)(Mathf.Clamp01(b) * 255f), (byte)(Mathf.Clamp01(a) * 255f)); }

        // Teraskehys, jonka paat ovat teravat (kuvan "tulppa"-palkki muunnettuna Pakettiporinaan).
        public static Sprite BarFrameSprite()
        {
            if (barFrame != null) return barFrame;
            const int W = 96, H = 32; const float Ch = 12f;
            barFrame = MakeSprite(W, H, (x, y) =>
            {
                float fy = y + 0.5f, fx = x + 0.5f;
                float xmin = Ch * Mathf.Abs(fy - H / 2f) / (H / 2f);
                float inL = fx - xmin, inR = (W - xmin) - fx;
                float inside = Mathf.Min(inL, inR);
                if (inside < 0f) return C32(0, 0, 0, 0);
                float a = Mathf.Clamp01(inside + 0.5f);
                float t = fy / H;                        // 0 alhaalla, 1 ylhaalla
                float v = Mathf.Lerp(0.30f, 0.78f, t);   // pystygradientti: teras
                if (fy > H - 4f && fy < H - 2f) v = 0.92f; // kirkas yläreuna
                if (inside < 2f || fy < 1.5f || fy > H - 1.5f) v = 0.14f; // tumma ääriviiva
                return C32(v * 0.92f, v * 0.96f, v, a);
            }, new Vector4(Ch + 3f, 7f, Ch + 3f, 7f));
            return barFrame;
        }

        // Kiiltava palkin taytto (valkoinen -> varjataan Image.colorilla).
        public static Sprite BarFillSprite()
        {
            if (barFill != null) return barFill;
            const int W = 32, H = 16, R = 6;
            barFill = MakeSprite(W, H, (x, y) =>
            {
                float cx = Mathf.Clamp(x + 0.5f, R, W - R), cy = Mathf.Clamp(y + 0.5f, R, H - R);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float a = Mathf.Clamp01(R - d + 0.5f);
                float t = (y + 0.5f) / H;
                float v = Mathf.Lerp(0.62f, 1f, t);
                if (t > 0.72f && t < 0.9f) v = Mathf.Min(1.25f, v + 0.25f);   // kiillejuova
                return C32(v, v, v, a);
            }, new Vector4(R, R, R, R));
            return barFill;
        }

        static bool InPoly(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) && p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) inside = !inside;
            return inside;
        }

        // Tahti-ikoni = pisteet/kerätyt tähdet.
        public static Sprite StarIcon()
        {
            if (starIcon != null) return starIcon;
            const int N = 64;
            Vector2[] Poly(float scale)
            {
                var v = new Vector2[10];
                for (int i = 0; i < 10; i++)
                {
                    float r = (i % 2 == 0 ? 29f : 12f) * scale;
                    float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                    v[i] = new Vector2(N / 2f + Mathf.Cos(a) * r, N / 2f + 1f + Mathf.Sin(a) * r);
                }
                return v;
            }
            var outer = Poly(1f); var inner = Poly(0.78f);
            starIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (InPoly(p, inner)) return C32(1f, Mathf.Lerp(0.78f, 0.95f, y / (float)N), 0.15f, 1f);
                if (InPoly(p, outer)) return C32(0.75f, 0.4f, 0.05f, 1f);
                return C32(0, 0, 0, 0);
            }, Vector4.zero);
            return starIcon;
        }

        // LISATTY 24.11.2026: kolikko-ikoni = POISTETTAVAT pisteet (tarrat, palkkio). Tahti on varattu ajotahdille
        // (lukitukset) ja radan bonustahdille, jotta ne eivat sekoitu pisteisiin.
        static Sprite coinIcon;
        public static Sprite CoinIcon()
        {
            if (coinIcon != null) return coinIcon;
            const int N = 64;
            var c = new Vector2(N / 2f, N / 2f);
            coinIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                if (d > 30f) return C32(0, 0, 0, 0);
                if (d > 25f) return C32(0.72f, 0.45f, 0.05f, 1f);               // tumma kultareunus
                if (d > 21f) return C32(1f, 0.9f, 0.4f, 1f);                    // vaalea rengas
                // keskella "p"-merkki: pysty + pallo (pisteet)
                bool stem = Mathf.Abs(p.x - (c.x - 5f)) < 3.2f && p.y > c.y - 14f && p.y < c.y + 14f;
                bool bowl = Vector2.Distance(p, new Vector2(c.x + 1f, c.y + 5f)) < 9.5f && Vector2.Distance(p, new Vector2(c.x + 1f, c.y + 5f)) > 4f && p.x > c.x - 5f;
                if (stem || bowl) return C32(0.72f, 0.45f, 0.05f, 1f);
                return C32(1f, Mathf.Lerp(0.72f, 0.88f, y / (float)N), 0.2f, 1f);
            }, Vector4.zero);
            return coinIcon;
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        // Kello-ikoni = jaljella oleva aika.
        public static Sprite ClockIcon()
        {
            if (clockIcon != null) return clockIcon;
            const int N = 64;
            var c = new Vector2(N / 2f, N / 2f);
            clockIcon = MakeSprite(N, N, (x, y) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                if (d > 30f) return C32(0, 0, 0, 0);
                if (SegDist(p, c, c + new Vector2(0f, 20f)) < 2.6f || SegDist(p, c, c + new Vector2(13f, -7f)) < 2.6f) return C32(0.1f, 0.15f, 0.3f, 1f);
                if (d > 24f) return C32(0.2f, 0.55f, 1f, 1f);   // sininen kehys
                return C32(0.97f, 0.97f, 1f, 1f);
            }, Vector4.zero);
            return clockIcon;
        }

        // Maalin konfetti UI-tasossa (maailman partikkelit jaivat portin taakse). Putoavia varikkaita paloja.
        public static void UIConfetti(RectTransform panel, int count = 70)
        {
            if (panel == null || !Application.isPlaying) return;
            Inst.StartCoroutine(Inst.UIConfettiRoutine(panel, count));
        }

        IEnumerator UIConfettiRoutine(RectTransform panel, int count)
        {
            var canvas = panel.GetComponentInParent<Canvas>();
            var croot = canvas != null ? (RectTransform)canvas.rootCanvas.transform : panel;
            float W = Mathf.Max(300f, croot.rect.width), H = Mathf.Max(500f, croot.rect.height);
            Color[] cs = { new Color(1f, 0.25f, 0.3f), new Color(1f, 0.85f, 0.1f), new Color(0.2f, 0.85f, 0.4f), new Color(0.3f, 0.55f, 1f), new Color(0.9f, 0.35f, 0.95f), Color.white };
            var pieces = new List<RectTransform>();
            var vel = new List<Vector3>();   // x = sway-vaihe, y = putoamisnopeus, z = pyorimisnopeus
            float size = Mathf.Max(18f, W * 0.025f);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(panel, false);
                rt.SetSiblingIndex(Mathf.Min(1, panel.childCount - 1));   // scrimin paalle, tekstien alle
                rt.sizeDelta = new Vector2(size, size * Random.Range(0.5f, 1.3f));
                rt.localPosition = new Vector3(Random.Range(-W * 0.5f, W * 0.5f), H * 0.5f + Random.Range(0f, H * 0.35f), 0f);
                var im = go.GetComponent<Image>();
                im.color = cs[Random.Range(0, cs.Length)];
                im.raycastTarget = false;
                pieces.Add(rt);
                vel.Add(new Vector3(Random.value * 6.28f, Random.Range(H * 0.35f, H * 0.7f), Random.Range(-360f, 360f)));
            }
            float e = 0f, dur = 4f;
            while (e < dur)
            {
                e += Time.unscaledDeltaTime;
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == null) continue;
                    var p = pieces[i].localPosition;
                    p.y -= vel[i].y * Time.unscaledDeltaTime;
                    p.x += Mathf.Sin(e * 3f + vel[i].x) * 60f * Time.unscaledDeltaTime;
                    pieces[i].localPosition = p;
                    pieces[i].localRotation = Quaternion.Euler(0f, 0f, vel[i].z * e);
                }
                yield return null;
            }
            foreach (var p in pieces) if (p != null) Destroy(p.gameObject);
        }

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
