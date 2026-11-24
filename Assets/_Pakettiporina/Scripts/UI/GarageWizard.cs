using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pakettiporina
{
    // LISATTY 23.11.2026: Hallin kolmivaiheinen kayttoliittyma (Paketti -> Rata -> Auto + vari).
    // Rakentuu ajonaikaisesti GarageScreenin paalle (ei scene-muutoksia): vanha kaikki-yhdessa-nakyma
    // piilotetaan (CanvasGroup alpha 0), auton esikatselukuva siirretaan uuteen kehykseen ja kaikki
    // logiikka (osat, lukitus, FitChecker, tallennus, Aja) kulkee GarageScreenin julkisen API:n kautta.
    // Rata (vaihe 2) valitaan vapaasti selaamalla; paketin trackScene on vain ehdotus ("Sopii paketille").
    // Radat ja osat aukeavat ajoissa ansaituilla tahdilla (1-3 per ajo, ks. Progress).
    public partial class GarageWizard : MonoBehaviour
    {
        GarageScreen gs;
        RectTransform root, safe, header, titleRt, vilkkuRow;
        RectTransform[] panels = new RectTransform[3];
        int step;

        static readonly Color Ink = new Color(0.043f, 0.36f, 0.28f);
        static readonly Color Cream = new Color(0.965f, 0.918f, 0.82f);
        static readonly Color Sand = new Color(0.851f, 0.769f, 0.604f);
        static readonly Color Orange = new Color(0.894f, 0.341f, 0.18f);
        static readonly Color OrangeDark = new Color(0.65f, 0.23f, 0.11f);
        static readonly Color Green = new Color(0.114f, 0.62f, 0.459f);
        static readonly Color GreenDark = new Color(0.06f, 0.43f, 0.34f);
        static readonly Color Amber = new Color(0.957f, 0.722f, 0.376f);
        static readonly Color AmberDark = new Color(0.75f, 0.54f, 0.18f);
        static readonly Color Grey = new Color(0.72f, 0.69f, 0.63f);
        static readonly Color GreyDark = new Color(0.56f, 0.53f, 0.47f);

        // yhteiset
        TextMeshProUGUI titleText, bubbleText, starText, runStarText, nextLabel;
        Image nextImg, nextShadow, nextIcon;
        Button nextBtn;
        Image[] progress = new Image[3];
        RectTransform backBtnRt;

        // vaihe 0
        RectTransform pkgCard, pkgLock, dotsRow;
        Image pkgImg, pkgArtBg;
        TextMeshProUGUI pkgName, pkgInfo, pkgLockText, pkgInitial;

        // vaihe 1
        RectTransform trackCard, trackLock, trackSuggest, trackDots;
        Image trackImg;
        TextMeshProUGUI trackName, trackDesc, trackPkg, trackLockText;
        Image[] trackBest = new Image[3];

        // vaihe 2
        RectTransform carFrame, tabsRow, chipsRow;
        Image[] tabImgs = new Image[6];
        GameObject[] tabHints = new GameObject[6];
        Image[] meterFill = new Image[5];
        float[] meterCur = new float[5], meterTarget = new float[5];
        RectTransform[] meterFillRt = new RectTransform[5];

        static readonly PartCategory[] Cats = { PartCategory.Kori, PartCategory.Renkaat, PartCategory.Moottori, PartCategory.Jouset, PartCategory.Lisat, PartCategory.Maali };
        static readonly string[] CatNames = { "Kori", "Renkaat", "Moottori", "Jouset", "Lisät", "Väri" };
        static readonly string[] CatIcons = { "kategoria_kori", "kategoria_renkaat", "kategoria_moottori", "kategoria_jouset", "kategoria_lisat", "kategoria_maali" };

        // --- siirretty tiedostoon GarageWizard.Helpers.cs (sprite- ja UI-apurit) ---

        // --- siirretty tiedostoon GarageWizard.Build.cs (nakymien rakennus) ---

        // ---------------------------------------------------------------- toiminta
        void OnNext()
        {
            if (step == 0)
            {
                if (gs.IsCurrentPackageLocked) { Juice.Play("warning"); Juice.Pulse(pkgLock, 1.08f, 0.3f); return; }
                Juice.Play("whoosh"); SetStep(1, 1, true);
            }
            else if (step == 1)
            {
                if (gs.IsCurrentTrackLocked) { Juice.Play("warning"); Juice.Pulse(trackLock, 1.08f, 0.3f); return; }
                Juice.Play("whoosh"); SetStep(2, 1, true);
            }
            else StartCoroutine(Drive());
        }

        IEnumerator Drive()
        {
            nextBtn.interactable = false;
            Juice.Play("confetti", 0f);
            Juice.UIConfetti(root, 50);
            Juice.UIConfettiCannons(root, 35);
            float t = 0f;
            while (t < 0.55f) { t += Time.unscaledDeltaTime; yield return null; }
            nextBtn.interactable = true;
            gs.OnDrive();
        }

        void SetStep(int s, int dir, bool animate)
        {
            step = Mathf.Clamp(s, 0, 2);
            for (int i = 0; i < 3; i++) panels[i].gameObject.SetActive(i == step);
            if (animate) StartCoroutine(Slide(panels[step], dir));
            else { var cg = panels[step].GetComponent<CanvasGroup>(); cg.alpha = 1f; }
            Refresh();
        }

        IEnumerator Slide(RectTransform p, int dir)
        {
            var cg = p.GetComponent<CanvasGroup>();
            float t = 0f;
            while (t < 0.28f)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / 0.28f), 3f);
                p.anchoredPosition = new Vector2(Mathf.Lerp(140f * dir, 0f, k), 0f);
                cg.alpha = k;
                yield return null;
            }
            p.anchoredPosition = Vector2.zero; cg.alpha = 1f;
        }

        static string TrackKey(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return "";
            string s = scene.ToLowerInvariant().Replace("ä", "a").Replace("ö", "o");
            return s;
        }

        static void TrackInfo(string key, out string name, out string desc, out string tip, out string image)
        {
            image = "rata_" + key;
            switch (key)
            {
                case "satama": name = "Satama"; desc = "Laivoja, konteja ja lätäköitä."; tip = "Satamassa on lätäköitä. Väistä ne!"; break;
                case "puisto": name = "Puisto"; desc = "Puita, lampi ja kukkia."; tip = "Puistossa on mukava ajaa."; break;
                case "huvipuisto": name = "Huvipuisto"; desc = "Maailmanpyörä ja ilmapalloja."; tip = "Hui, ilmapalloja! Ajetaan maaliin."; break;
                case "maatila": name = "Maatila"; desc = "Lehmiä, lampaita ja punainen navetta."; tip = "Maatilalla on eläimiä tien vieressä."; break;
                case "kyla": name = "Kylä"; desc = "Lunta ja pieniä taloja."; tip = "Kylässä on liukasta!"; break;
                default: name = "Kaupunki"; desc = "Tavallinen kotirata."; tip = "Tuttu rata. Ajetaan varovasti!"; image = "rata_perus"; break;
            }
        }

        // Datan nimet ovat ilman ääkkösiä (asset-nimet) -> näytetään lapselle oikein kirjoitettuna.
        static string PrettyName(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            switch (s)
            {
                case "Jattijaatelo": return "Jättijäätelö";
                case "Synttarikakku": return "Synttärikakku";
                case "Kivipaasi": return "Kivipaasi";
                case "Saippuakuplat": return "Saippuakuplat";
                default: return s;
            }
        }

        // ---------------------------------------------------------------- paivitys
        void Refresh()
        {
            if (gs == null || root == null) return;
            var pkg = gs.CurrentPackageData;
            starText.text = gs.Points.ToString();
            runStarText.text = Progress.RunStars.ToString();

            for (int i = 0; i < 3; i++) progress[i].color = i <= step ? Orange : Sand;
            backBtnRt.gameObject.SetActive(step > 0);

            titleText.text = step == 0 ? "Mitä viedään?" : step == 1 ? "Tällä radalla ajetaan" : "Rakenna auto";
            bool locked = gs.IsCurrentPackageLocked;

            if (step == 0)
            {
                bubbleText.text = locked ? "Tämä avautuu myöhemmin. Kerää tähtiä!" : NextRewardLine();
                RefreshPackage(pkg, locked);
                SetNext(locked ? "Lukittu" : "Seuraava", false, locked);
            }
            else if (step == 1)
            {
                RefreshTrack(pkg);
                bool trackLocked = gs.IsCurrentTrackLocked;
                SetNext(trackLocked ? "Lukittu" : "Seuraava", false, trackLocked);
            }
            else
            {
                var fit = gs.CurrentFit;
                bubbleText.text = fit.fits ? "Hyvä valinta! Valitse väri ja aja." : (string.IsNullOrEmpty(fit.message) ? "Katso mitä paketti tarvitsee." : fit.message);
                RefreshCar();
                SetNext("Aja!", true, false);
            }
        }

        // Seuraava palkinto aina nakyvissa: "vielä N tähteä -> uusi väri" (syy pelata vielä yksi kierros).
        string NextRewardLine()
        {
            int need; string what;
            if (!Progress.NextReward(out need, out what)) return "Valitse paketti, jonka viet perille!";
            int left = need - Progress.RunStars;
            return (left == 1 ? "Vielä 1 tähti" : "Vielä " + left + " tähteä") + " ajoissa, niin avautuu: " + what.ToLowerInvariant();
        }

        void SetNext(string label, bool drive, bool disabled)
        {
            nextLabel.text = label;
            nextIcon.gameObject.SetActive(drive);
            Color face = disabled ? Grey : (drive ? Orange : Green);
            Color sh = disabled ? GreyDark : (drive ? OrangeDark : GreenDark);
            nextImg.color = face; nextShadow.color = sh;
        }

        void RefreshPackage(PackageData pkg, bool locked)
        {
            if (pkg == null) return;
            pkgName.text = PrettyName(pkg.displayName);
            pkgInfo.text = "Palkinto: " + pkg.rewardPoints + " kolikkoa";
            bool hasIcon = pkg.icon != null;
            pkgImg.sprite = pkg.icon; pkgImg.enabled = hasIcon;
            pkgInitial.text = hasIcon || string.IsNullOrEmpty(pkg.displayName) ? "" : pkg.displayName.Substring(0, 1);
            pkgLock.gameObject.SetActive(locked);
            if (locked) pkgLockText.text = GarageScreen.StarsLeftPublic(gs.PackageStarsNeeded);

            foreach (Transform c in dotsRow) Destroy(c.gameObject);
            int n = gs.PackageCount;
            for (int i = 0; i < n; i++)
            {
                var d = R("Dot", dotsRow, i / (float)n + 0.1f / n, 0.15f, (i + 1) / (float)n - 0.1f / n, 0.85f);
                Img(d, Circle(), i == gs.PackageIndex ? Orange : Sand);
            }
        }

        void RefreshTrack(PackageData pkg)
        {
            int ti = gs.TrackIndex;
            string scene = gs.CurrentTrackScene;
            string key = TrackKey(scene);
            string n, d, tip, image; TrackInfo(key, out n, out d, out tip, out image);
            var spr = Load(image);
            if (spr == null) spr = Load("rata_perus");
            trackImg.sprite = spr; trackImg.enabled = spr != null;
            trackName.text = n; trackDesc.text = d;
            trackPkg.text = pkg != null ? "Kyydissä: " + PrettyName(pkg.displayName) : "";

            bool locked = gs.IsTrackLocked(ti);
            trackLock.gameObject.SetActive(locked);
            if (locked) trackLockText.text = GarageScreen.StarsLeftPublic(gs.TrackStarsNeeded(ti));
            trackSuggest.gameObject.SetActive(!locked && gs.IsSuggestedTrack(ti));

            int best = GameManager.Instance != null ? GameManager.Instance.BestFor(scene) : 0;
            for (int i = 0; i < 3; i++) trackBest[i].color = i < best ? Color.white : new Color(0.6f, 0.58f, 0.52f, 0.35f);

            foreach (Transform c in trackDots) Destroy(c.gameObject);
            int cnt = gs.TrackCount;
            for (int i = 0; i < cnt; i++)
            {
                var dot = R("Dot", trackDots, i / (float)cnt + 0.1f / cnt, 0.15f, (i + 1) / (float)cnt - 0.1f / cnt, 0.85f);
                Img(dot, Circle(), i == ti ? Orange : (gs.IsTrackLocked(i) ? Grey : Sand));
            }
            bubbleText.text = locked ? "Tämä rata aukeaa myöhemmin. Kerää tähtiä!" : tip;
        }

        void RefreshCar()
        {
            var cur = gs.CurrentCategory;
            for (int i = 0; i < 6; i++) tabImgs[i].color = Cats[i] == cur ? new Color(1f, 0.89f, 0.78f) : Color.white;

            // Vihje: jos paketti vaatii osan jota autossa ei ole, merkitaan sen valilehti "!" ja osa oranssilla reunalla.
            var pkgNow = gs.CurrentPackageData;
            var req = (pkgNow != null && !gs.CurrentFit.fits) ? pkgNow.requiredPart : null;
            for (int i = 0; i < 6; i++) if (tabHints[i] != null) tabHints[i].SetActive(req != null && req.category == Cats[i] && cur != Cats[i]);

            foreach (Transform c in chipsRow) Destroy(c.gameObject);
            var list = gs.PartsIn(cur);
            int n = list.Count;
            if (n > 0)
            {
                int cols = Mathf.Min(n, 3), rows = (n + cols - 1) / cols;
                int sel = gs.PartIndexIn(cur);
                for (int i = 0; i < n; i++)
                {
                    int idx = i; var part = list[i];
                    int cx = i % cols, cy = i / cols;
                    float x0 = cx / (float)cols + 0.006f, x1 = (cx + 1) / (float)cols - 0.006f;
                    float y1 = 1f - cy / (float)rows, y0 = 1f - (cy + 1) / (float)rows;
                    var cell = R("Chip" + i, chipsRow, x0, y0 + 0.03f, x1, y1 - 0.03f);
                    bool isColor = cur == PartCategory.Maali;
                    bool lockd = gs.IsPartLocked(part);
                    bool picked = i == sel;
                    var rim = R("Rim", cell, 0, 0, 1, 1);
                    Pill(rim, picked ? Green : (req != null && part == req ? Orange : Color.white));
                    var inner = R("Inner", cell, 0.045f, 0.06f, 0.955f, 0.94f);
                    Pill(inner, isColor ? part.color : Color.white, 0.3f, true);
                    if (!isColor) Txt(inner, part.displayName, 40, Ink, TextAlignmentOptions.Center, 0.04f, 0.04f, 0.96f, 0.96f);
                    if (lockd)
                    {
                        var dark = R("Dim", inner, 0, 0, 1, 1); Pill(dark, new Color(0.14f, 0.14f, 0.19f, 0.6f));
                        var li = R("Lock", inner, 0.3f, 0.2f, 0.7f, 0.85f); Img(li, Load("lukko"), Color.white);
                    }
                    Btn(inner, () => { gs.SelectPartIndex(cur, idx); Juice.Play("pluck"); Refresh(); });
                }
            }

            var s = gs.builder.Current;
            meterTarget[0] = s.voima / 100f; meterTarget[1] = s.pito / 100f; meterTarget[2] = s.keveys / 100f;
            meterTarget[3] = s.kestavyys / 100f; meterTarget[4] = s.kylmyys / 100f;
        }

        void Update()
        {
            if (root == null) return;
            float dt = Time.unscaledDeltaTime * 2.2f;
            for (int i = 0; i < 5; i++)
            {
                if (meterFillRt[i] == null) continue;
                meterCur[i] = Mathf.MoveTowards(meterCur[i], Mathf.Clamp01(meterTarget[i]), dt);
                meterFillRt[i].anchorMax = new Vector2(Mathf.Max(0.06f, meterCur[i]), 1f);
            }
        }
    }
}
