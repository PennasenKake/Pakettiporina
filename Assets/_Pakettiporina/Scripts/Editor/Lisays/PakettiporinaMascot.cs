#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // LISATTY 19.8.2026 (kayttaja: "Pakettipörinä maskotti luoda niin että se liikkuu ja
    // heiluttaa esim kättä ... main menussa, settings ja garagessa, voisiko antaa jotain
    // tilanteeseen sopivaa kommenttia ääneen"). Ei ollut valmiina 3D-hahmoa/riggausta/
    // animaatiota - kayttaja valitsi kevyen reitin: animoidaan olemassa oleva 2D-kuva
    // (pakettiporina_vilkku_hahmo.png, jo poseerattu "kasi ylhaalla") koodilla
    // (ks. MascotAnimator.cs) ja korvataan aanikommentti puhekupla-TEKSTILLA (ei
    // aaninaytteita - nauhoitus/generointi ei ole tassa tyokalussa mahdollista).
    //
    // Kaytto: avaa MainMenu- TAI Garage-scene, aja tama. MainMenussa paivittaa/luo KAKSI
    // maskottia: yksi aina nakyvissa paavalikon paaruudulla, toinen (jo olemassa oleva
    // "SettingsMascot") Asetukset-paneelin sisalla - molemmat saavat MascotAnimator-
    // komponentin + puhekuplan jos niilta puuttuu. Garagessa luo/paivittaa yhden.
    //
    // HUOM sijainti/koko: MainMenu/Settings kayttaa 800x600 CanvasScaler-referenssia,
    // Garage 1080x1920 (portrait) - siksi eri oletusarvot per scene. Jos maskotti nakyy
    // vaarassa kohdassa/paallekkain jonkin toisen UI-elementin kanssa, saada Inspectorista
    // (RectTransform anchoredPosition) - tyokalu on idempotentti, turvallinen ajaa uudelleen.
    public static class PakettiporinaMascot
    {
        const string ART_UI_DIR = "Assets/_Pakettiporina/Art/UI";
        const string ICON_DIR = "Assets/_Pakettiporina/Art/UI/Icons";

        static readonly string[] MainMenuLines =
        {
            "Hei! Valitse rata ja aja!",
            "Tervetuloa Pakettipörinään!",
            "Kerää tähtiä ja tienaa pisteitä!",
            "Muista käydä pesulla ajon jälkeen!",
        };
        static readonly string[] GarageLines =
        {
            "Rakenna unelmiesi paketti-auto!",
            "Kokeile eri osia - löydä oma tyylisi!",
            "Kerätyillä pisteillä saat avattua uusia tarroja!",
        };
        static readonly string[] SettingsLines =
        {
            "Säädä äänet ja nopeus sopiviksi!",
            "Voit vaihtaa ohjaustavan täältä!",
        };

        [MenuItem("Pakettiporina/Lisays/11 - Lisaa animoitu maskotti (MainMenu/Settings/Garage)")]
        public static void AddMascot()
        {
            var s = new StringBuilder();
            s.AppendLine("=== ANIMOITU MASKOTTI ===");
            string sceneName = EditorSceneManager.GetActiveScene().name;

            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                s.AppendLine("VIRHE: scenesta ei loydy Canvasia.");
                Debug.LogError(s.ToString());
                return;
            }

            var mascotSprite = LoadSprite($"{ART_UI_DIR}/pakettiporina_vilkku_hahmo.png");
            var roundedSprite = LoadSlicedSprite("pakettiporina_ui_rounded", 30f);
            if (mascotSprite == null)
            {
                s.AppendLine($"VIRHE: {ART_UI_DIR}/pakettiporina_vilkku_hahmo.png ei loytynyt.");
                Debug.LogError(s.ToString());
                return;
            }

            if (sceneName == "MainMenu")
            {
                // 1) Paavalikon paaruudulla aina nakyva maskotti.
                // KORJATTU 19.8.2026 (kayttaja: "voisi kooltaan hieman kasvattaa ja
                // sijoittaa keskemmaksi ruutua, nyt se peittaa settings painikkeen"):
                // alkuperainen sijainti (rightOffset -30, size 90) osui paallekkain
                // "SetstButton"-napin kanssa (napin RectTransform: anchor top-right,
                // anchoredPosition -20/-40, koko 80x80 - eli varaa nurkasta alueen
                // x:[-100,-20] y:[-120,-40]). Uusi sijainti (rightOffset -140, size 115)
                // siirtaa maskotin selvasti vasemmalle Settings-napista - tyhjaan
                // valiin logon/otsikon (keskella) ja Settings-napin (oikea ylanurkka)
                // valissa - ja kasvattaa hieman kokoa.
                //
                // PERUTTU 20.8.2026 (kayttaja: "peru kaksi viimeisinta muutosta
                // mainsceneen, ei menty oikeaan suuntaan"): kokeilin 20.8. kasvattaa
                // tata (rightOffset -50, topOffset -420, size 260) samassa erassa kuin
                // MainMenun sisallon uudelleenskaalaava LegacyLayoutScale-kaare - kayttaja
                // perui MOLEMMAT, joten palautettu tahan alkuperaiseen (19.8.) arvoon.
                SetupMascot(canvas.transform, "MainMenuMascot", mascotSprite, roundedSprite,
                    rightOffset: -140, topOffset: -30, size: 115, speechLeft: true, lines: MainMenuLines, s);

                // KORJATTU 19.8.2026 (kayttaja: "maskottin tulisi olla piilossa kun settings ja
                // infopanel ja privacypanel on paalla, muuten toimii jo maskotti hyvin") - naiden
                // kolmen koko naytolla olevan paneelin ollessa auki maskotti (ja sen puhekupla)
                // piilotetaan MascotAnimator.hideWhilePanelsActive:lla (ks. MascotAnimator.cs).
                // KORJATTU UUDELLEEN 19.8.2026 (kayttaja: "mainmenussa maskotti tulee
                // nollataanko peli paneelin paalle, ei tarkoitus, seka maskotti nakyy myos
                // tarrapaneelissa, ei tarkoitus") - lista oli vajaa, unohtui kaksi muutakin
                // koko naytolla olevaa paneelia: ResetConfirmPanel ("Nollataanko peli?") ja
                // StickerPanel (tarrat). Lisatty molemmat samaan listaan.
                var mainMenuMascotGo = Find("MainMenuMascot");
                if (mainMenuMascotGo != null)
                {
                    var mainAnim = mainMenuMascotGo.GetComponent<MascotAnimator>();
                    if (mainAnim != null)
                    {
                        var settingsPanel = Find("SettingsPanel");
                        var infoPanel = Find("InfoPanel");
                        var privacyPanel = Find("PrivacyPanel");
                        var resetConfirmPanel = Find("ResetConfirmPanel");
                        var stickerPanel = Find("StickerPanel");
                        mainAnim.hideWhilePanelsActive = new[] { settingsPanel, infoPanel, privacyPanel, resetConfirmPanel, stickerPanel };
                        s.AppendLine($"MainMenuMascot: piilotus paneelien paalla kytketty (Settings={settingsPanel != null}, Info={infoPanel != null}, Privacy={privacyPanel != null}, ResetConfirm={resetConfirmPanel != null}, Sticker={stickerPanel != null}).");
                    }
                }

                // 2) Olemassa oleva "SettingsMascot" (Asetukset-paneelin sisalla, ks.
                // PakettiporinaSettingsSetup.MakeCornerMascot) - animoidaan sama, EI luoda
                // uutta, koska paneeli/koko/sijainti on jo oikein muun asetus-UI:n kanssa.
                var settingsMascotGo = Find("SettingsMascot");
                if (settingsMascotGo != null)
                {
                    AttachAnimatorAndSpeech(settingsMascotGo.transform, roundedSprite, speechLeft: true,
                        lines: SettingsLines, s, label: "SettingsMascot (olemassa oleva)");
                }
                else
                {
                    s.AppendLine("HUOM: 'SettingsMascot' ei loytynyt - aja ensin Lisays/09 (asetuspaneeli) jos haluat maskotin myos sinne.");
                }
            }
            else if (sceneName == "Garage")
            {
                SetupMascot(canvas.transform, "GarageMascot", mascotSprite, roundedSprite,
                    rightOffset: -40, topOffset: -40, size: 130, speechLeft: true, lines: GarageLines, s);
            }
            else
            {
                s.AppendLine($"HUOM: aktiivinen scene on '{sceneName}' - tama tyokalu tukee vain MainMenu ja Garage -sceneja. Avaa jompikumpi ja aja uudelleen.");
            }

            Debug.Log(s.ToString());
        }

        static void SetupMascot(Transform canvasTransform, string name, Sprite mascotSprite, Sprite roundedSprite,
            float rightOffset, float topOffset, float size, bool speechLeft, string[] lines, StringBuilder s)
        {
            var existing = FindChild(canvasTransform, name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null) go.transform.SetParent(canvasTransform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(rightOffset, topOffset);
            rt.sizeDelta = new Vector2(size, size);
            rt.localScale = Vector3.one; // varmuuden vuoksi - ei koskaan peilikuvaa/vaarää kokoa uudelleenkaytetylla objektilla
            var img = go.GetComponent<Image>();
            if (img == null) img = go.AddComponent<Image>();
            img.sprite = mascotSprite;
            img.color = Color.white;
            img.raycastTarget = false;
            img.preserveAspect = true;

            AttachAnimatorAndSpeech(go.transform, roundedSprite, speechLeft, lines, s, name);
        }

        // Lisaa MascotAnimator-komponentin (jos puuttuu) + pieni puhekupla vasemmalle
        // (tai oikealle) maskotista - AANIKOMMENTIN korvike (ks. luokkakommentti).
        static void AttachAnimatorAndSpeech(Transform mascotTransform, Sprite roundedSprite, bool speechLeft,
            string[] lines, StringBuilder s, string label)
        {
            var anim = mascotTransform.GetComponent<MascotAnimator>();
            if (anim == null) anim = mascotTransform.gameObject.AddComponent<MascotAnimator>();
            anim.speechLines = lines;

            var bubbleGo = FindChild(mascotTransform, "SpeechBubble");
            GameObject bubble = bubbleGo != null ? bubbleGo.gameObject : new GameObject("SpeechBubble", typeof(RectTransform));
            if (bubbleGo == null) bubble.transform.SetParent(mascotTransform, false);
            var brt = (RectTransform)bubble.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(speechLeft ? 0f : 1f, 0.5f);
            brt.pivot = new Vector2(speechLeft ? 1f : 0f, 0.5f);
            brt.anchoredPosition = new Vector2(speechLeft ? -10f : 10f, 0f);
            brt.sizeDelta = new Vector2(170f, 60f);
            var bimg = bubble.GetComponent<Image>();
            if (bimg == null) bimg = bubble.AddComponent<Image>();
            if (roundedSprite != null) { bimg.sprite = roundedSprite; bimg.type = Image.Type.Sliced; }
            bimg.color = new Color(1f, 1f, 1f, 0.95f);
            bimg.raycastTarget = false;

            var textGo = FindChild(bubble.transform, "Text");
            GameObject textObj = textGo != null ? textGo.gameObject : new GameObject("Text", typeof(RectTransform));
            if (textGo == null) textObj.transform.SetParent(bubble.transform, false);
            var trt = (RectTransform)textObj.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(8f, 4f); trt.offsetMax = new Vector2(-8f, -4f);
            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.24f, 0.18f, 0.12f, 1f);
            tmp.text = "";
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            // KORJATTU 19.8.2026 (kayttaja: "garage scenessä maskotin tekstit vaarin pain,
            // peilikuvana mika ei hyva"): Garagessa taman tekstikomponentin isRightToLeftText
            // oli jaanyt paalle (m_isRightToLeft: 1 scenen YAML:ssa) - todennakoisesti
            // jonkin uudelleenkaytetyn "Text"-objektin jaanneasetus, jota koodi ei aiemmin
            // koskaan nollannut (asetettiin vain fontSize/alignment/color/text/wordwrap, ei
            // tata). Tekstit nakyivat siksi peilikuvana. Pakotetaan nyt AINA vasemmalta-
            // oikealle, riippumatta mahdollisesta vanhasta jaanneasetuksesta.
            tmp.isRightToLeftText = false;

            anim.speechBubbleText = tmp;
            anim.speechBubbleRoot = bubble;
            bubble.SetActive(false);

            s.AppendLine($"{label}: MascotAnimator + puhekupla ({lines.Length} riviä) kunnossa.");
        }

        static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform t in parent)
                if (t.name == name) return t;
            return null;
        }

        static Sprite LoadSprite(string fullPath)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite LoadSlicedSprite(string name, float border)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ICON_DIR}/{name}.png");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f,
                1, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }
    }
}
#endif
