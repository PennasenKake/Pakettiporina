#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

namespace Pakettiporina.EditorTools
{
    // Pesu-scenen (M4) tarkistus- ja korjaustyokalu.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/
    // Kaytto: Pakettiporina -> 9 Tarkista pesu / 10 KORJAA pesu
    //
    // Odottaa scenesta naita nimia (muuta vakioita jos omat nimesi poikkeavat):
    //   BubbleArea      - tyhja Transform, jonka lapsina kuplat (Bubble-komponentti) ovat
    //   ProgressText    - TMP_Text, esim. "Poksautit 0/8 kuplaa!"
    //   DonePanel       - paneeli, joka nakyy kun kaikki kuplat on poksautettu
    //   DoneText        - TMP_Text DonePanelin sisalla
    //   ContinueButton  - nappi DonePanelin sisalla, vie halliin
    //   SkipButton      - valinnainen, ohittaa pesun
    public static class PakettiporinaWashSetup
    {
        const string BUBBLE_AREA = "BubbleArea";
        const string PROGRESS_TEXT = "ProgressText";
        const string DONE_PANEL = "DonePanel";
        const string DONE_TEXT = "DoneText";
        const string CONTINUE_BTN = "ContinueButton";
        const string SKIP_BTN = "SkipButton";
        const string GARAGE_SCENE = "Garage";

        [MenuItem("Pakettiporina/9 - Tarkista pesu")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/10 - KORJAA pesu")]
        public static void FixAll() { Run(true); }

        static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == name) return t.gameObject;
            return null;
        }

        static void ClearClicks(Button b)
        {
            for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(b.onClick, i);
        }

        static void Run(bool fix)
        {
            var s = new StringBuilder();
            s.AppendLine(fix ? "=== PESUN KORJAUS ===" : "=== PESUN TARKISTUS ===");
            int problems = 0;
            void Prob(string msg) { s.AppendLine("! " + msg); problems++; }

            var wash = Object.FindObjectOfType<WashScreen>(true);
            var canvas = Object.FindObjectOfType<Canvas>(true);
            var evsys = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>(true);
            var gm = Object.FindObjectOfType<GameManager>(true);

            if (wash == null)
            {
                s.AppendLine("VIRHE: WashScreen puuttuu scenesta. Lisaa se jollekin objektille (esim. Canvasille).");
                Debug.LogError(s.ToString());
                return;
            }

            // ---------- 1. EventSystem ----------
            if (evsys == null)
            {
                Prob("EventSystem puuttuu (kuplien napautus ei toimi).");
                if (fix)
                {
                    var go = new GameObject("EventSystem");
                    go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    Undo.RegisterCreatedObjectUndo(go, "EventSystem");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 2. GameManager ----------
            if (gm == null)
            {
                Prob("GameManager puuttuu pesusta (palkkiopisteet eivat tallennu jos testaat tasta scenesta suoraan).");
                if (fix)
                {
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                    Undo.RegisterCreatedObjectUndo(go, "GameManager");
                    s.AppendLine("  -> luotu");
                }
            }

            // ---------- 2b. AudioManager ----------
            var audioMgr = Object.FindObjectOfType<AudioManager>(true);
            if (audioMgr == null)
            {
                Prob("AudioManager puuttuu pesusta (kuplan poksahdusaani ei soi jos testaat tasta scenesta suoraan).");
                if (fix)
                {
                    var go = new GameObject("AudioManager");
                    go.AddComponent<AudioManager>();
                    Undo.RegisterCreatedObjectUndo(go, "AudioManager");
                    s.AppendLine("  -> luotu (muista raahata aaniklipit sen Inspectoriin)");
                }
            }

            // ---------- 3. Canvas Scaler ----------
            if (canvas != null)
            {
                var sc = canvas.GetComponent<CanvasScaler>();
                if (sc != null)
                {
                    bool bad = sc.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                            || sc.referenceResolution != new Vector2(1080, 1920);
                    if (bad)
                    {
                        Prob($"Canvas Scaler: {sc.uiScaleMode}, {sc.referenceResolution}");
                        if (fix)
                        {
                            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                            sc.referenceResolution = new Vector2(1080, 1920);
                            sc.matchWidthOrHeight = 0.5f;
                            s.AppendLine("  -> korjattu 1080x1920 / match 0.5");
                        }
                    }
                    else s.AppendLine("Canvas Scaler: OK");
                }
            }

            // ---------- 4. BubbleArea + kuplien maara ----------
            if (wash.bubbleArea == null)
            {
                Prob("Bubble Area ei kytketty.");
                if (fix)
                {
                    var go = Find(BUBBLE_AREA);
                    if (go != null) { wash.bubbleArea = go.transform; s.AppendLine("  -> kytketty (" + BUBBLE_AREA + ")"); }
                    else s.AppendLine("  -> objektia '" + BUBBLE_AREA + "' ei loytynyt, kytke kasin");
                }
            }
            if (wash.bubbleArea != null)
            {
                int count = wash.bubbleArea.GetComponentsInChildren<Bubble>(true).Length;
                if (count == 0)
                    Prob("BubbleArean alla ei ole yhtaan Bubble-komponenttia. Lisaa muutama kupla-objekti.");
                else
                    s.AppendLine($"Kuplia BubbleArean alla: {count}");

                // Tarkista etta jokaisella kuplalla on Image ja Raycast Target paalla.
                foreach (var b in wash.bubbleArea.GetComponentsInChildren<Bubble>(true))
                {
                    var img = b.GetComponent<Image>();
                    if (img != null && !img.raycastTarget)
                    {
                        Prob($"Kupla '{b.name}': Image Raycast Target on pois paalta (ei reagoi napautukseen).");
                        if (fix) { img.raycastTarget = true; s.AppendLine("  -> korjattu"); }
                    }
                }
            }

            // ---------- 5. Edistymisteksti ----------
            if (wash.progressText == null)
            {
                Prob("Progress Text ei kytketty.");
                if (fix)
                {
                    var go = Find(PROGRESS_TEXT);
                    var t = go != null ? go.GetComponent<TMP_Text>() : null;
                    if (t != null) { wash.progressText = t; s.AppendLine("  -> kytketty"); }
                }
            }

            // ---------- 6. Valmis-paneeli ----------
            if (wash.donePanel == null)
            {
                Prob("Done Panel ei kytketty.");
                if (fix)
                {
                    var go = Find(DONE_PANEL);
                    if (go != null) { wash.donePanel = go; s.AppendLine("  -> kytketty"); }
                }
            }
            if (wash.doneText == null)
            {
                Prob("Done Text ei kytketty.");
                if (fix)
                {
                    var go = Find(DONE_TEXT);
                    var t = go != null ? go.GetComponent<TMP_Text>() : null;
                    if (t != null) { wash.doneText = t; s.AppendLine("  -> kytketty"); }
                }
            }

            // ---------- 7. Napit ----------
            void Wire(string objName, UnityAction action, string label, bool required)
            {
                var go = Find(objName);
                var btn = go != null ? go.GetComponent<Button>() : null;
                if (btn == null)
                {
                    if (required) Prob($"Nappi '{objName}' puuttuu.");
                    return;
                }
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    Prob($"{label}: OnClick tyhja.");
                    if (fix)
                    {
                        ClearClicks(btn);
                        UnityEventTools.AddPersistentListener(btn.onClick, action);
                        s.AppendLine("  -> kytketty");
                    }
                }
            }
            Wire(CONTINUE_BTN, wash.OnContinueButton, "Jatkoon-nappi", true);
            Wire(SKIP_BTN, wash.OnSkipButton, "Ohita-nappi", false); // valinnainen

            // ---------- 8. Scenen nimi ----------
            if (wash.garageSceneName != GARAGE_SCENE)
            {
                Prob($"Garage Scene Name = '{wash.garageSceneName}' (odotettu '{GARAGE_SCENE}').");
                if (fix) { wash.garageSceneName = GARAGE_SCENE; s.AppendLine("  -> asetettu '" + GARAGE_SCENE + "'"); }
            }

            // ---------- valmis ----------
            if (fix)
            {
                EditorUtility.SetDirty(wash);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            s.AppendLine(problems == 0
                ? "\nKaikki kunnossa!"
                : $"\nLoytyi {problems} huomiota." + (fix ? " Korjaukset tehty � muista Ctrl+S." : " Aja '10 - KORJAA pesu'."));
            Debug.Log(s.ToString());
        }
    }
}
#endif
