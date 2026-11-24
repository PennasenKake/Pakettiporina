using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Tauko: pysayttaa ajan, nayttaa tauko-paneelin ja PIILOTTAA ohjausnapit.
    public class PauseMenu : MonoBehaviour
    {
        [Header("Viittaukset")]
        public GameObject pausePanel;
        [Tooltip("Ohjausnappien vanhempi — piilotetaan tauon ajaksi")]
        public GameObject controls;

        [Header("Tauko/jatka-napin kuva (valinnainen)")]
        public Image toggleIcon;
        public Sprite pauseSprite;
        public Sprite playSprite;

        [Header("Scenet")]
        public string mainMenuScene = "MainMenu";
        [Tooltip("Valinnainen: jos tauko-paneelissa on Halliin-nappi (esim. Pesu-scenessa)")]
        public string garageScene = "Garage";

        public bool IsPaused { get; private set; }

        void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            SetIcon(false);
            EnsureMinTouchSize();
        }

        // 24.11.2026 (TODO: "Pause-napin korkeus vahintaan 48 dp"): tauko-napin kosketusalue
        // pakotetaan vahintaan 48 x 48 dp:hen laitteen tiheyden mukaan (Canvasin skaalaus huomioiden).
        // Kuva voi pysya pienempana: lisataan vain alue, ei muuteta ulkonakoa jos nappi on jo riittava.
        public const float MinTouchDp = 48f;

        void EnsureMinTouchSize()
        {
            var go = GameObject.Find("PauseButton");
            if (go == null) return;
            var rt = go.GetComponent<RectTransform>();
            var canvas = go.GetComponentInParent<Canvas>();
            if (rt == null || canvas == null) return;
            float scale = Mathf.Max(0.01f, canvas.rootCanvas.scaleFactor);
            float density = Screen.dpi > 0f ? Screen.dpi / 160f : 2.75f;
            float minUnits = MinTouchDp * density / scale;
            // Vain pistemaiset ankkurit (ei venyva); venyva nappi hoitaa itse kokonsa.
            if (rt.anchorMin != rt.anchorMax) return;
            var size = rt.sizeDelta;
            var newSize = new Vector2(Mathf.Max(size.x, minUnits), Mathf.Max(size.y, minUnits));
            if (newSize != size)
            {
                rt.sizeDelta = newSize;
                Debug.Log($"[Pause] Tauko-nappi kasvatettu {size} -> {newSize} (48 dp).");
            }
        }

        void Update()
        {
            // Androidin takaisin-nappi = tauko/jatka
            if (Input.GetKeyDown(KeyCode.Escape))
                Toggle();
            // Piilota "Koti uudelleen" -vihje (Invoke ei toimi timeScale 0:lla)
            if (homeHint != null && homeHint.gameObject.activeSelf && Time.realtimeSinceStartup > homeArmedUntil)
                HideHomeHint();
        }

        // Kytke tauko-nappiin.
        public void Toggle()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            Juice.Play("whoosh", 0.02f);
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
            if (controls != null) controls.SetActive(false);   // napit piiloon
            SetIcon(true);
            Debug.Log("[Pause] Tauko — ajonapit piilotettu.");
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            // Napit takaisin vain jos keikka on kaynnissa (ei maalin jalkeen)
            bool racing = RaceManager.Instance != null && RaceManager.Instance.IsRacing;
            if (controls != null) controls.SetActive(racing);
            SetIcon(false);
            Debug.Log("[Pause] Jatketaan.");
        }

        float homeArmedUntil;
        Text homeHint;

        public void GoToMainMenu()
        {
            // LISATTY 23.11.2026: "Koti" ei saa poistua ajosta vahingossa -> kaksi painallusta (2,5 s sisalla).
            // Ajastus kayttaa realtimea, koska Time.timeScale on 0 tauolla.
            if (IsPaused && Time.realtimeSinceStartup > homeArmedUntil)
            {
                homeArmedUntil = Time.realtimeSinceStartup + 2.5f;
                ShowHomeHint();
                return;
            }
            Time.timeScale = 1f;
            Debug.Log("[Pause] Paavalikkoon: " + mainMenuScene);
            SceneFader.LoadScene(mainMenuScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        // Valinnainen "Halliin"-nappi tauko-paneelissa (esim. Pesu-scenessa).
        public void GoToGarage()
        {
            Time.timeScale = 1f;
            Debug.Log("[Pause] Halliin: " + garageScene);
            SceneFader.LoadScene(garageScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        void ShowHomeHint()
        {
            if (pausePanel == null) return;
            if (homeHint == null)
            {
                var go = new GameObject("HomeHint", typeof(RectTransform), typeof(Text));
                var rt = (RectTransform)go.transform;
                rt.SetParent(pausePanel.transform, false);
                rt.anchorMin = new Vector2(0.05f, 0.62f); rt.anchorMax = new Vector2(0.95f, 0.72f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                homeHint = go.GetComponent<Text>();
                homeHint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                homeHint.fontSize = 60;
                homeHint.resizeTextForBestFit = true;   // canvasin skaala on suuri: pieni fontti jai lukukelvottomaksi
                homeHint.resizeTextMinSize = 30;
                homeHint.resizeTextMaxSize = 80;
                homeHint.fontStyle = FontStyle.Bold;
                homeHint.alignment = TextAnchor.MiddleCenter;
                homeHint.color = Color.yellow;
                homeHint.raycastTarget = false;
                homeHint.horizontalOverflow = HorizontalWrapMode.Wrap;
                go.AddComponent<Outline>().effectColor = Color.black;
            }
            homeHint.text = "Poistutaanko ajosta? Paina Koti uudelleen.";
            homeHint.gameObject.SetActive(true);
            homeHint.transform.SetAsLastSibling();
        }

        void HideHomeHint()
        {
            if (homeHint != null) homeHint.gameObject.SetActive(false);
        }

        void SetIcon(bool paused)
        {
            if (toggleIcon == null) return;
            if (paused && playSprite != null) toggleIcon.sprite = playSprite;
            else if (!paused && pauseSprite != null) toggleIcon.sprite = pauseSprite;
        }
    }
}