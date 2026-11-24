using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pakettiporina
{
    // 24.11.2026: GarageWizard pilkottu partial-luokiksi (TODO: suuret UI-tiedostot). Sisalto siirretty sellaisenaan: nakymien rakennus.
    public partial class GarageWizard
    {
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
    }
}
