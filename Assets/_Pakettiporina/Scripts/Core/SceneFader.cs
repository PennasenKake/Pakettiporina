using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // LISATTY 20.8.2026 (kayttaja: "scenevaihdot ovat kaikki akkinaisia leikkauksia...
    // pienelle lapselle akkinainen ruudun vaihtuminen voi olla hammentava"): musta kalvo
    // joka hairyttaa (fade) jokaisen scenevaihdon yli. Korvaa KAIKKI suorat
    // SceneManager.LoadScene(...)-kutsut koko pelissa - kaytetaan SceneFader.LoadScene():a
    // sen sijaan (ks. MainMenu.cs, GarageScreen.cs, PauseMenu.cs, RaceHUD.cs, WashScreen.cs).
    //
    // ARKKITEHTUURI: sama "yksi per scene, ensimmainen selviaa DontDestroyOnLoad:lla,
    // loput tuhotaan duplikaattina" -periaate kuin GameManager/SaveManager/AudioManagerilla
    // (ks. PakettiporinaAudioSetup.cs:n kommentti samasta kaavasta). TARKEA ERO: tama EI
    // kayta scenen OMAA Canvasia (joka tuhoutuu/luodaan uudelleen joka scenevaihdossa) -
    // se rakentaa OMAN, aina sailyvan Canvasin koodissa Awake()ssa, sortingOrder
    // erittain korkealla (999) jotta musta kalvo on AINA kaiken paalla, myos hetken
    // ennen kuin uuden scenen oma Canvas on ehtinyt alustua.
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [Tooltip("Kuinka kauan yksi häivytyssuunta (musta -> nakyvissa TAI nakyvissa -> musta) kestaa.")]
        public float fadeDuration = 0.35f;

        Image img;
        bool isFading;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUI();
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("SceneFaderCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGo.AddComponent<CanvasScaler>();

            var imgGo = new GameObject("Fade", typeof(RectTransform));
            imgGo.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)imgGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img = imgGo.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = false;

            // LISATTY 23.11.2026: "Ladataan..." raskaille sceneille (hidas lataus ei nayta jumilta).
            var txtGo = new GameObject("LoadingText", typeof(RectTransform));
            txtGo.transform.SetParent(canvasGo.transform, false);
            var trt = (RectTransform)txtGo.transform;
            trt.anchorMin = new Vector2(0f, 0.5f);
            trt.anchorMax = new Vector2(1f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(0f, 120f);
            loadingText = txtGo.AddComponent<Text>();
            loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            loadingText.fontSize = 56;
            loadingText.alignment = TextAnchor.MiddleCenter;
            loadingText.color = Color.white;
            loadingText.raycastTarget = false;
            loadingText.text = "Ladataan...";
            loadingText.enabled = false;
        }

        [Tooltip("Scenet joille naytetaan 'Ladataan...' (isot/raskaat scenet).")]
        public string[] heavyScenes = { "Huvipuisto", "Maatila" };
        [Tooltip("Muille sceneille teksti naytetaan vain jos lataus kestaa yli taman (sekuntia).")]
        public float showLoadingAfter = 0.6f;

        Text loadingText;

        bool IsHeavy(string sceneName)
        {
            if (heavyScenes == null) return false;
            foreach (var s in heavyScenes) if (s == sceneName) return true;
            return false;
        }

        // Kutsu tama SceneManager.LoadScene(x):n SIJAAN kaikkialla pelissa.
        public static void LoadScene(string sceneName)
        {
            if (Instance == null)
            {
                // Varafallback jos jostain syysta SceneFaderia ei loydy scenesta (esim.
                // scene jota ei viela paivitetty) - parempi ladata suoraan kuin jaada
                // jumiin mustaan ruutuun.
                Debug.LogWarning("[SceneFader] Instancea ei loydy - ladataan ilman haivytysta: " + sceneName);
                SceneManager.LoadScene(sceneName);
                return;
            }
            Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
        }

        IEnumerator FadeAndLoad(string sceneName)
        {
            if (isFading) yield break; // estaa paallekkaiset lataukset (esim. tuplaklikki)
            isFading = true;
            img.raycastTarget = true; // estaa kosketukset hairyttamisen ajaksi

            yield return Fade(0f, 1f);

            bool heavy = IsHeavy(sceneName);
            if (loadingText != null && heavy) loadingText.enabled = true;

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError("[SceneFader] SceneManager.LoadSceneAsync epaonnistui: " + sceneName);
                if (loadingText != null) loadingText.enabled = false;
                isFading = false;
                img.raycastTarget = false;
                yield break;
            }
            float waited = 0f;
            float pulse = 0f;
            while (!op.isDone)
            {
                waited += Time.unscaledDeltaTime;
                pulse += Time.unscaledDeltaTime;
                if (loadingText != null)
                {
                    if (!loadingText.enabled && waited > showLoadingAfter) loadingText.enabled = true;
                    int dots = 1 + (int)(pulse * 2f) % 3;
                    loadingText.text = "Ladataan" + new string('.', dots);
                }
                yield return null;
            }
            if (loadingText != null) loadingText.enabled = false;

            // Pieni hengähdystauko ennen paljastusta, jotta uuden scenen Awake/Start-
            // ketju (GameManager/SaveManager/AudioManager jne.) ehtii asettua ennen
            // kuin pelaaja nakee ruudun - valttaa "vilahdus vaarilla arvoilla" -efektin.
            yield return null;

            yield return Fade(1f, 0f);
            img.raycastTarget = false;
            isFading = false;
        }

        IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            var c = img.color;
            float dur = Mathf.Max(0.05f, fadeDuration);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime; // toimii myos jos Time.timeScale=0 (esim. tauko)
                float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
                c.a = a;
                img.color = c;
                yield return null;
            }
            c.a = to;
            img.color = c;
        }
    }
}
