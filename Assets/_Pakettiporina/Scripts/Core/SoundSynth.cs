using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: proseduraaliset aanitehosteet (ei aanitiedostoja -> ei Git LFS -binaareja, ei lisensseja).
    // Kaytetaan uusille mehukkuus-efekteille, joille ei ollut valmista klippia. Soitto: Juice.Play("avain")
    // -> AudioManager.PlaySfx, joten Asetusten "Aanet"-kytkin ja aanenvoimakkuus toimivat.
    public static class SoundSynth
    {
        const int Rate = 44100;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        public static AudioClip Get(string key)
        {
            if (cache.TryGetValue(key, out var c) && c != null) return c;
            AudioClip made = null;
            switch (key)
            {
                case "confetti": made = Confetti(); break;
                case "warning": made = Warning(); break;
                case "whoosh": made = Whoosh(); break;
                case "arm": made = Arm(); break;
                case "pluck": made = Pluck(); break;
            }
            if (made != null) cache[key] = made;
            return made;
        }

        static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float[] Buffer(float seconds) { return new float[Mathf.CeilToInt(seconds * Rate)]; }

        // sinikaiku hetkesta start, kesto dur, taajuus f (voi liukua f2:een), vaimeneva verhokayra
        static void Tone(float[] d, float start, float dur, float f, float f2, float amp, float attack = 0.005f)
        {
            int s0 = Mathf.RoundToInt(start * Rate), n = Mathf.RoundToInt(dur * Rate);
            double phase = 0;
            for (int i = 0; i < n && s0 + i < d.Length; i++)
            {
                float t = i / (float)n;
                float freq = Mathf.Lerp(f, f2 <= 0f ? f : f2, t);
                phase += 2.0 * Mathf.PI * freq / Rate;
                float env = Mathf.Min(1f, i / (attack * Rate + 1f)) * Mathf.Pow(1f - t, 2f);
                d[s0 + i] += Mathf.Sin((float)phase) * env * amp;
            }
        }

        // kohinapurske yksinkertaisella alipaastosuodattimella (cutoff 0..1)
        static void Noise(float[] d, float start, float dur, float cutoffStart, float cutoffEnd, float amp, int seed)
        {
            var rnd = new System.Random(seed);
            int s0 = Mathf.RoundToInt(start * Rate), n = Mathf.RoundToInt(dur * Rate);
            float y = 0f;
            for (int i = 0; i < n && s0 + i < d.Length; i++)
            {
                float t = i / (float)n;
                float c = Mathf.Lerp(cutoffStart, cutoffEnd, t);
                float x = (float)(rnd.NextDouble() * 2.0 - 1.0);
                y += c * (x - y);
                float env = Mathf.Sin(Mathf.Min(1f, t * 1.0f) * Mathf.PI) * (1f - t * 0.3f);
                d[s0 + i] += y * env * amp;
            }
        }

        static void Normalize(float[] d, float peak)
        {
            float m = 0f;
            foreach (var v in d) m = Mathf.Max(m, Mathf.Abs(v));
            if (m < 0.0001f) return;
            float g = peak / m;
            for (int i = 0; i < d.Length; i++) d[i] *= g;
        }

        // Konfetti: popsahdus + nouseva kimallus
        static AudioClip Confetti()
        {
            var d = Buffer(0.9f);
            Noise(d, 0f, 0.12f, 0.6f, 0.15f, 1.2f, 7);
            Tone(d, 0f, 0.15f, 220f, 90f, 0.8f);
            float[] notes = { 1046.5f, 1318.5f, 1568f, 2093f, 2637f };
            for (int i = 0; i < notes.Length; i++) Tone(d, 0.08f + i * 0.07f, 0.35f, notes[i], 0f, 0.28f);
            Noise(d, 0.15f, 0.6f, 0.9f, 0.9f, 0.04f, 11);   // pehmea suhina
            Normalize(d, 0.85f);
            return Make("syn_confetti", d);
        }

        // Varoitus: kaksi laskevaa piippausta
        static AudioClip Warning()
        {
            var d = Buffer(0.4f);
            Tone(d, 0f, 0.16f, 760f, 560f, 0.7f);
            Tone(d, 0.18f, 0.18f, 620f, 420f, 0.7f);
            Normalize(d, 0.7f);
            return Make("syn_warning", d);
        }

        // Whoosh: kohina jonka suodin avautuu ja sulkeutuu
        static AudioClip Whoosh()
        {
            var d = Buffer(0.35f);
            Noise(d, 0f, 0.35f, 0.02f, 0.5f, 1f, 3);
            Normalize(d, 0.55f);
            return Make("syn_whoosh", d);
        }

        // Viritys (Varmista): kaksi lyhytta matalaa piippausta
        static AudioClip Arm()
        {
            var d = Buffer(0.3f);
            Tone(d, 0f, 0.09f, 520f, 0f, 0.8f);
            Tone(d, 0.13f, 0.09f, 520f, 0f, 0.8f);
            Normalize(d, 0.6f);
            return Make("syn_arm", d);
        }

        // Plink: pehmea yksittainen kilahdus
        static AudioClip Pluck()
        {
            var d = Buffer(0.25f);
            Tone(d, 0f, 0.22f, 880f, 0f, 0.7f, 0.002f);
            Tone(d, 0f, 0.15f, 1760f, 0f, 0.25f, 0.002f);
            Normalize(d, 0.6f);
            return Make("syn_pluck", d);
        }
    }
}
