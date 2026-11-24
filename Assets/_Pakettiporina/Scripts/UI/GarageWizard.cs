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
    public class GarageWizard : MonoBehaviour
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

        // ---------------------------------------------------------------- sprite-apurit
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static Sprite circle;

        static Sprite Load(string name)
        {
            Sprite s;
            if (cache.TryGetValue(name, out s)) return s;
            var tex = Resources.Load<Texture2D>("WizardUI/" + name);
            if (tex != null) s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            cache[name] = s;
            return s;
        }

        static Sprite Circle()
        {
            if (circle != null) return circle;
            const int N = 64;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(N / 2f, N / 2f));
                    px[y * N + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(N / 2f - d + 0.5f) * 255f));
                }
            tex.SetPixels32(px); tex.Apply();
            circle = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }

        // ---------------------------------------------------------------- UI-apurit
        static RectTransform R(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static Image Img(RectTransform rt, Sprite s, Color c, bool sliced = false, float ppum = 0.3f, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = s; img.color = c; img.raycastTarget = raycast;
            if (sliced && s != null && s.border != Vector4.zero) { img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = ppum; }
            else img.preserveAspect = true;
            return img;
        }

        static Image Pill(RectTransform rt, Color c, float ppum = 0.3f, bool raycast = false)
        {
            return Img(rt, Juice.PillSprite(), c, true, ppum, raycast);
        }

        static TextMeshProUGUI Txt(Transform parent, string s, float size, Color c, TextAlignmentOptions al = TextAlignmentOptions.Center, float x0 = 0, float y0 = 0, float x1 = 1, float y1 = 1)
        {
            var rt = R("Text", parent, x0, y0, x1, y1);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = s; t.color = c; t.alignment = al; t.fontStyle = FontStyles.Bold;
            t.enableAutoSizing = true; t.fontSizeMax = size; t.fontSizeMin = Mathf.Max(24f, size * 0.55f);
            t.overflowMode = TextOverflowModes.Ellipsis; t.raycastTarget = false;
            return t;
        }

        static Button Btn(RectTransform rt, UnityAction a)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) { img = rt.gameObject.AddComponent<Image>(); img.color = new Color(1, 1, 1, 0); }
            img.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img; b.transition = Selectable.Transition.None;
            rt.gameObject.AddComponent<JuiceButtonPress>();   // heti painuma (ei odota Juicen skannausta)
            b.onClick.AddListener(a);
            return b;
        }

        // Pehmea nappi: varjo + paalaatta. Palauttaa paalaatan rectin (varjo on sisarena sen alla).
        static RectTransform Chunky(string name, Transform parent, float x0, float y0, float x1, float y1, Color face, Color shadow, out Image faceImg, out Image shadowImg)
        {
            var holder = R(name, parent, x0, y0, x1, y1);
            var sh = R("Shadow", holder, 0, 0, 1, 1);
            sh.offsetMin = new Vector2(0, -14); sh.offsetMax = new Vector2(0, -14);
            shadowImg = Pill(sh, shadow);
            var f = R("Face", holder, 0, 0, 1, 1);
            faceImg = Pill(f, face, 0.3f, true);
            return holder;
        }

        // ---------------------------------------------------------------- rakennus
        void Start()
        {
            gs = GetComponent<GarageScreen>();
            if (gs == null) { enabled = false; return; }
            Build();
            Refresh();
        }

        void Build()
        {
            Canvas canvas = (gs.carPreview != null && gs.carPreview.canvas != null) ? gs.carPreview.canvas.rootCanvas : FindObjectOfType<Canvas>();
            if (canvas == null) { enabled = false; return; }

            // 1) Auton esikatselukuva talteen ennen vanhan UI:n piilotusta.
            Image car = gs.carPreview;

            // 2) Juuri + tausta
            var rootGo = new GameObject("WizardRoot", typeof(RectTransform));
            root = (RectTransform)rootGo.transform;
            root.SetParent(canvas.transform, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            var bg = rootGo.AddComponent<Image>(); bg.color = Cream; bg.raycastTarget = true;

            var sa = Screen.safeArea;
            safe = R("Safe", root, sa.xMin / Screen.width, sa.yMin / Screen.height, sa.xMax / Screen.width, sa.yMax / Screen.height);

            BuildHeader();
            titleRt = R("Title", safe, 0.05f, 0.865f, 0.95f, 0.93f);
            titleText = Txt(titleRt, "", 84, Ink, TextAlignmentOptions.Left);
            BuildVilkku();

            for (int i = 0; i < 3; i++)
            {
                panels[i] = R("Step" + i, safe, 0f, 0.14f, 1f, 0.77f);
                var cg = panels[i].gameObject.AddComponent<CanvasGroup>();
            }
            BuildStep0(panels[0]);
            BuildStep1(panels[1]);
            BuildStep2(panels[2], car);
            BuildNav();

            // Piilota vanha UI vasta kun auton kuva on siirretty pois (muuten se perii vanhan ryhman alpha 0:n).
            // Pysyy aktiivisena, jotta GarageScreen toimii; vain nakymattomaksi + ei kosketuksia.
            foreach (Transform ch in canvas.transform)
            {
                if (ch == root) continue;
                var cg = ch.GetComponent<CanvasGroup>();
                if (cg == null) cg = ch.gameObject.AddComponent<CanvasGroup>();   // (?? ei toimi Unity-olioilla)
                cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false;
            }

            SetStep(0, 1, false);
        }

        void BuildHeader()
        {
            header = R("Header", safe, 0f, 0.935f, 1f, 1f);
            // Kaksi laskuria: KOLIKOT (pisteet, tarrat) ja TAHDET (ajotahdet, avaavat sisaltoa).
            var pill = R("Coins", header, 0.02f, 0.12f, 0.25f, 0.88f);
            Pill(pill, Color.white);
            var coin = R("Icon", pill, 0.04f, 0.1f, 0.4f, 0.9f);
            Img(coin, Juice.CoinIcon(), Color.white);
            starText = Txt(pill, "0", 56, Ink, TextAlignmentOptions.Center, 0.42f, 0f, 0.98f, 1f);

            var pill2 = R("RunStars", header, 0.265f, 0.12f, 0.49f, 0.88f);
            Pill(pill2, Color.white);
            var ring = R("StarBg", pill2, 0.03f, 0.06f, 0.41f, 0.94f);
            Img(ring, Circle(), Amber);
            var star = R("Icon", pill2, 0.07f, 0.13f, 0.37f, 0.87f);
            Img(star, Load("tahti"), Color.white);
            runStarText = Txt(pill2, "0", 56, Ink, TextAlignmentOptions.Center, 0.42f, 0f, 0.98f, 1f);

            var bar = R("Progress", header, 0.53f, 0.4f, 0.78f, 0.6f);
            for (int i = 0; i < 3; i++)
            {
                var seg = R("Seg" + i, bar, i / 3f + 0.01f, 0, (i + 1) / 3f - 0.01f, 1);
                progress[i] = Pill(seg, Sand, 0.8f);
            }

            var home = R("Home", header, 0.82f, 0.08f, 0.97f, 0.92f);
            Img(home, Circle(), Amber, false, 0.3f, true);
            var hi = R("Icon", home, 0.2f, 0.2f, 0.8f, 0.8f);
            Img(hi, Load("koti"), Color.white);
            Btn(home, () => { Juice.Play("pluck"); gs.OnMainMenuButton(); });
        }

        void BuildVilkku()
        {
            vilkkuRow = R("Vilkku", safe, 0.02f, 0.77f, 0.98f, 0.875f);
            var v = R("Char", vilkkuRow, 0f, 0f, 0.24f, 1f);
            Img(v, Load("vilkku"), Color.white);
            var b = R("Bubble", vilkkuRow, 0.25f, 0.04f, 1f, 0.96f);
            Pill(b, Color.white);
            bubbleText = Txt(b, "", 56, Ink, TextAlignmentOptions.Left, 0.05f, 0.04f, 0.95f, 0.96f);
        }

        void BuildStep0(RectTransform p)
        {
            pkgCard = R("Card", p, 0.19f, 0.1f, 0.81f, 0.98f);
            var shadow = R("Shadow", pkgCard, 0, 0, 1, 1); shadow.offsetMin = new Vector2(0, -16); shadow.offsetMax = new Vector2(0, -16);
            Pill(shadow, Sand);
            var face = R("Face", pkgCard, 0, 0, 1, 1); Pill(face, Color.white);

            var art = R("Art", face, 0.04f, 0.4f, 0.96f, 0.97f);
            pkgArtBg = Pill(art, new Color(1f, 0.91f, 0.66f));
            var ic = R("Icon", art, 0.12f, 0.08f, 0.88f, 0.92f);
            pkgImg = Img(ic, null, Color.white);
            pkgInitial = Txt(art, "", 220, Color.white, TextAlignmentOptions.Center, 0.2f, 0.2f, 0.8f, 0.8f);

            pkgName = Txt(face, "", 78, Ink, TextAlignmentOptions.Center, 0.04f, 0.22f, 0.96f, 0.38f);
            pkgInfo = Txt(face, "", 48, new Color(0.27f, 0.41f, 0.37f), TextAlignmentOptions.Center, 0.04f, 0.05f, 0.96f, 0.2f);

            pkgLock = R("Lock", face, 0, 0, 1, 1);
            Pill(pkgLock, new Color(0.14f, 0.14f, 0.19f, 0.72f));
            var li = R("Icon", pkgLock, 0.3f, 0.45f, 0.7f, 0.75f);
            Img(li, Load("lukko"), Color.white);
            pkgLockText = Txt(pkgLock, "", 64, Color.white, TextAlignmentOptions.Center, 0.05f, 0.22f, 0.95f, 0.42f);

            var left = R("Prev", p, 0f, 0.38f, 0.19f, 0.62f);
            Img(left, Load("nuoli_vasen_coral"), Color.white, false, 0.3f, true);
            Btn(left, () => { gs.PrevPackage(); Juice.Play("pluck"); Juice.Pulse(pkgCard, 1.06f, 0.25f); Refresh(); });
            var right = R("Next", p, 0.81f, 0.38f, 1f, 0.62f);
            Img(right, Load("nuoli_oikea_coral"), Color.white, false, 0.3f, true);
            Btn(right, () => { gs.NextPackage(); Juice.Play("pluck"); Juice.Pulse(pkgCard, 1.06f, 0.25f); Refresh(); });

            dotsRow = R("Dots", p, 0.2f, 0.01f, 0.8f, 0.08f);
        }

        void BuildStep1(RectTransform p)
        {
            // Rata valitaan vapaasti selaamalla (nuolet); paketti vain ehdottaa rataa.
            trackCard = R("Card", p, 0.13f, 0.3f, 0.87f, 0.99f);
            var shadow = R("Shadow", trackCard, 0, 0, 1, 1); shadow.offsetMin = new Vector2(0, -16); shadow.offsetMax = new Vector2(0, -16);
            Pill(shadow, Sand);
            var face = R("Face", trackCard, 0, 0, 1, 1); Pill(face, Color.white);
            var pic = R("Pic", face, 0.035f, 0.33f, 0.965f, 0.975f);
            trackImg = Img(pic, null, Color.white);
            trackName = Txt(face, "", 78, Ink, TextAlignmentOptions.Left, 0.06f, 0.18f, 0.62f, 0.32f);
            trackDesc = Txt(face, "", 48, new Color(0.27f, 0.41f, 0.37f), TextAlignmentOptions.Left, 0.06f, 0.03f, 0.94f, 0.18f);

            // paras arvosana tälle radalle (3 tähteä, tyhjät himmeinä)
            var bestRow = R("Best", face, 0.62f, 0.185f, 0.96f, 0.315f);
            for (int i = 0; i < 3; i++)
            {
                var c = R("S" + i, bestRow, i / 3f, 0, (i + 1) / 3f, 1);
                trackBest[i] = Img(c, Load("tahti"), Color.white);
            }

            // "Ehdotus" -tarra kun tämä rata sopii paketille
            trackSuggest = R("Suggest", face, 0.06f, 0.86f, 0.46f, 0.96f);
            Pill(trackSuggest, Orange);
            Txt(trackSuggest, "Sopii paketille", 44, Color.white, TextAlignmentOptions.Center, 0.05f, 0.05f, 0.95f, 0.95f);

            trackLock = R("Lock", face, 0, 0, 1, 1);
            Pill(trackLock, new Color(0.14f, 0.14f, 0.19f, 0.72f));
            var li = R("Icon", trackLock, 0.38f, 0.5f, 0.62f, 0.8f);
            Img(li, Load("lukko"), Color.white);
            trackLockText = Txt(trackLock, "", 64, Color.white, TextAlignmentOptions.Center, 0.05f, 0.28f, 0.95f, 0.48f);

            var left = R("Prev", p, 0f, 0.5f, 0.13f, 0.78f);
            Img(left, Load("nuoli_vasen_coral"), Color.white, false, 0.3f, true);
            Btn(left, () => { gs.PrevTrack(); Juice.Play("pluck"); Juice.Pulse(trackCard, 1.05f, 0.25f); Refresh(); });
            var right = R("Next", p, 0.87f, 0.5f, 1f, 0.78f);
            Img(right, Load("nuoli_oikea_coral"), Color.white, false, 0.3f, true);
            Btn(right, () => { gs.NextTrack(); Juice.Play("pluck"); Juice.Pulse(trackCard, 1.05f, 0.25f); Refresh(); });

            trackDots = R("Dots", p, 0.2f, 0.215f, 0.8f, 0.285f);

            var chip = R("PkgChip", p, 0.1f, 0.04f, 0.9f, 0.19f);
            Pill(chip, new Color(1f, 0.95f, 0.85f));
            trackPkg = Txt(chip, "", 52, Ink, TextAlignmentOptions.Center, 0.05f, 0.05f, 0.95f, 0.95f);
        }

        void BuildStep2(RectTransform p, Image car)
        {
            carFrame = R("CarFrame", p, 0.04f, 0.74f, 0.96f, 1f);
            Pill(carFrame, new Color(0.75f, 0.9f, 0.88f));
            if (car != null)
            {
                var rt = car.rectTransform;
                rt.SetParent(carFrame, false);
                rt.anchorMin = new Vector2(0.12f, 0.04f); rt.anchorMax = new Vector2(0.88f, 0.96f);
                rt.offsetMin = rt.offsetMax = Vector2.zero; rt.localScale = Vector3.one; rt.localRotation = Quaternion.identity;
                car.preserveAspect = true; car.raycastTarget = false;
                car.enabled = true;
            }

            tabsRow = R("Tabs", p, 0.02f, 0.6f, 0.98f, 0.73f);
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                var cell = R("Tab" + i, tabsRow, i / 6f + 0.004f, 0, (i + 1) / 6f - 0.004f, 1);
                tabImgs[i] = Pill(cell, Color.white, 0.3f, true);
                var icon = R("Icon", cell, 0.15f, 0.34f, 0.85f, 0.95f);
                Img(icon, Load(CatIcons[i]), Color.white);
                Txt(cell, CatNames[i], 36, Ink, TextAlignmentOptions.Center, 0f, 0.02f, 1f, 0.34f);
                var badge = R("Hint", cell, 0.62f, 0.62f, 1.02f, 1.08f);
                Img(badge, Circle(), Orange);
                Txt(badge, "!", 48, Color.white, TextAlignmentOptions.Center, 0f, 0f, 1f, 1f);
                badge.gameObject.SetActive(false);
                tabHints[i] = badge.gameObject;
                Btn(cell, () => { gs.SetCategory((int)Cats[idx]); Juice.Play("pluck"); Refresh(); });
            }

            chipsRow = R("Chips", p, 0.03f, 0.39f, 0.97f, 0.59f);

            var meters = R("Meters", p, 0.03f, 0.0f, 0.97f, 0.37f);
            string[] names = { "Voima", "Pito", "Keveys", "Kestää", "Kylmyys" };
            string[] icons = { "mittari_voima", "mittari_pito", "mittari_keveys", "mittari_kestavyys", "mittari_kylmyys" };
            Color[] cols = { Orange, Green, new Color(0.3f, 0.67f, 0.97f), Amber, new Color(0.5f, 0.78f, 0.95f) };
            for (int i = 0; i < 5; i++)
            {
                int col = i % 2, row = i / 2;
                float x0 = col * 0.5f + 0.01f, x1 = (col + 1) * 0.5f - 0.01f;
                float y1 = 1f - row / 3f, y0 = 1f - (row + 1) / 3f;
                var cell = R("M" + i, meters, x0, y0 + 0.01f, x1, y1 - 0.01f);
                var ic = R("Icon", cell, 0f, 0.05f, 0.28f, 0.95f);
                Img(ic, Load(icons[i]), Color.white);
                Txt(cell, names[i], 40, Ink, TextAlignmentOptions.Left, 0.3f, 0.5f, 1f, 0.98f);
                var frame = R("Bar", cell, 0.3f, 0.1f, 1f, 0.45f);
                Pill(frame, new Color(0.9f, 0.86f, 0.76f), 0.8f);
                var fill = R("Fill", frame, 0, 0, 0.06f, 1);
                meterFillRt[i] = fill;
                meterFill[i] = Pill(fill, cols[i], 0.8f);
            }
        }

        void BuildNav()
        {
            Image f, s;
            // Takaisin
            var back = Chunky("Back", safe, 0.05f, 0.03f, 0.27f, 0.115f, Amber, AmberDark, out f, out s);
            backBtnRt = back;
            var arrow = R("Arrow", back, 0.15f, 0.1f, 0.85f, 0.9f);
            var arrowImg = Img(arrow, Load("nuoli_vasen_coral"), Color.white);
            arrowImg.color = Color.white;
            arrow.GetComponent<Image>().enabled = true;
            Btn(R("Hit", back, 0, 0, 1, 1), () => { Juice.Play("whoosh"); if (step > 0) SetStep(step - 1, -1, true); });

            // Seuraava / Aja
            var next = Chunky("Next", safe, 0.31f, 0.03f, 0.95f, 0.115f, Green, GreenDark, out nextImg, out nextShadow);
            var icon = R("Icon", next, 0.06f, 0.15f, 0.22f, 0.85f);
            nextIcon = Img(icon, Load("pelaa"), Color.white);
            nextLabel = Txt(next, "Seuraava", 72, Color.white, TextAlignmentOptions.Center, 0.1f, 0.05f, 0.9f, 0.95f);
            var hit = R("Hit", next, 0, 0, 1, 1);
            nextBtn = Btn(hit, OnNext);
        }

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
