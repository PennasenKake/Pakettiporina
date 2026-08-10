#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Lisaa/paivittaa teemapuiston yleiskartan (kaikki neljä rataa: Puisto, Satama,
    // Maatila, Huvipuisto) paavalikon (MainMenu-scene) Canvasille niin etta se nakyy
    // heti kun peli aloitetaan. AJA MainMenu-scene AUKI.
    //
    // Kayttaa Assets/_Pakettiporina/Art/UI/Teemapuisto_Kartta.png -kuvaa. Kuva ladataan
    // Texture2D:na ja siita tehdaan Sprite ajonaikaisesti (Sprite.Create) - tama toimii
    // vaikka kuvan Texture Type ei olisikaan viela asetettu "Sprite (2D and UI)":ksi
    // Unityn Inspectorissa (kuva on tuotu tuoreena, oletus-importti ei valttamatta ole
    // Sprite - jos haluat sen "oikeana" Sprite-assettina jatkossa, vaihda Texture Type
    // kasin ja aja tama uudelleen, se kayttaa silloin sita suoraan).
    //
    // Sijoitus: oikea ylakulma, kiinteassa 340x255 koossa (kuva on 1024x768 eli 4:3) -
    // TARKOITUKSELLA nurkassa eika koko ruudun peittavana, jotta se ei mene PlayButton/
    // QuitButton-nappien paalle (jotka ovat yleensa keskella). Jos haluat sen isommaksi
    // tai toiseen kohtaan, siirra "TeemapuistoKarttaPaneeli"-objektia Hierarkiassa kasin -
    // tama tyokalu ei ylikirjoita sijaintia jos objekti on jo olemassa (paivittaa vain
    // kuvan/koon jos ne puuttuvat).
    public static class PakettiporinaMainMenuMap
    {
        const string PANEL_NAME = "TeemapuistoKarttaPaneeli";
        const string TEX_PATH = "Assets/_Pakettiporina/Art/UI/Teemapuisto_Kartta.png";

        [MenuItem("Pakettiporina/Lisays/04 - Lisaa teemapuiston kartta paavalikkoon")]
        public static void AddMap()
        {
            var s = new StringBuilder();
            s.AppendLine("=== TEEMAPUISTON KARTTA PAAVALIKKOON ===");

            string sceneName = EditorSceneManager.GetActiveScene().name;
            if (sceneName != "MainMenu")
                s.AppendLine($"HUOM: aktiivinen scene on '{sceneName}', ei 'MainMenu' - avaa MainMenu-scene ja aja uudelleen jos kartta ei nay oikeassa scenessa.");

            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                s.AppendLine("VIRHE: scenesta ei loydy Canvasia. Lisaa Canvas ensin (paavalikon UI:n pitaisi jo olla jonkin Canvasin alla).");
                Debug.LogError(s.ToString());
                return;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_PATH);
            if (tex == null)
            {
                s.AppendLine($"VIRHE: '{TEX_PATH}' ei loydy - onko Unity tuonut kuvan viela? (Assets -> Refresh)");
                Debug.LogError(s.ToString());
                return;
            }

            // Etsi olemassa oleva paneeli (myos piilotettu) - jos loytyy, vain paivita
            // kuva/koko, ala siirra sita jos kayttaja on jo asetellut sen kasin.
            Transform existing = null;
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == PANEL_NAME) { existing = t; break; }

            GameObject go;
            RectTransform rt;
            Image img;
            bool isNew = existing == null;

            if (isNew)
            {
                go = new GameObject(PANEL_NAME, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(go, "Luo " + PANEL_NAME);
                go.transform.SetParent(canvas.transform, false);
                rt = go.GetComponent<RectTransform>();
                // Oikea ylakulma, kiintea koko (4:3, kuva 1024x768).
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-24f, -24f);
                rt.sizeDelta = new Vector2(340f, 255f);
                img = go.GetComponent<Image>();
            }
            else
            {
                go = existing.gameObject;
                rt = go.GetComponent<RectTransform>();
                img = go.GetComponent<Image>();
                if (img == null) img = go.AddComponent<Image>();
                s.AppendLine("Paneeli on jo olemassa - sijaintia ei muutettu, paivitetaan vain kuva.");
            }

            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            img.sprite = sprite;
            img.preserveAspect = true;
            go.transform.SetAsLastSibling(); // pysyy nappien PAALLA - siirra Hierarkiassa jos nappi jaa alle

            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            s.AppendLine(isNew
                ? "Kartta lisatty paavalikkoon (oikea ylakulma). Nakyy heti kun peli aloitetaan."
                : "Kartan kuva paivitetty.");
            Debug.Log(s.ToString());
        }
    }
}
#endif
