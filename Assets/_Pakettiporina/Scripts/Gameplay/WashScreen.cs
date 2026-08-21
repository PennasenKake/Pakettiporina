using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // M4: Pesukohtaus maalin ja hallin valissa. Kuplat kelluvat jatkuvasti ja
    // MIELLYTTAVASTI taustalla (ei rangaistusta, ei karkaamista) - jokainen
    // napautus poistaa YHDEN likatahran nakyvista autosta. Kun kaikki likatahrat
    // on poistettu, pesu on valmis ja antaa palkkion.
    //
    // M5/16.8.2026 (kayttaja: "korjaa vaihe tulisi tehda samalla tavalla kuin pesu"):
    // korjausvaihe (kolhut) toimii NYT TASMALLEEN SAMALLA PERIAATTEELLA kuin pesu -
    // ei enaa yhta "Korjaa kolhut" -nappia, vaan RepairArean sisalla kelluu
    // RepairTool-ikoneja (sama liike/napautus-logiikka kuin Bubble.cs:ssa) joita
    // napauttamalla poistetaan YKSI kolhu kerrallaan autosta (DentSpots), aivan
    // kuten kuplien napautus poistaa likatahroja. Jarjestys: PESU EKA (BubbleArea+
    // DirtSpots), sitten KORJAUS (RepairArea+DentSpots), vasta sen jalkeen Jatkoon.
    //
    // M5.17/16.8.2026 (kayttaja: "progressbar lialle ja korjaukselle... pysyy
    // seuraavaan ajoon asti... tyhjenee Wash scenessa"): dirtSpots/dentSpots-maara
    // luetaan nyt GameManager.DirtMeter/DentMeter -PYSYVISTA mittareista (ei enaa
    // vain taman yhden ajon osumista), ja jokainen napautus purkaa mittaria heti
    // yhdella (GameManager.CleanOneDirt/RepairOneDent) - ei vain kerralla lopussa.
    public class WashScreen : MonoBehaviour
    {
        public static WashScreen Instance { get; private set; }

        [Header("Kuplat (koristeellisia, ei vaikuta suoraan puhtauteen)")]
        [Tooltip("Kuplien yhteinen vanhempi. Valinnainen, kaytetaan lahinna Editor-tarkistukseen.")]
        public Transform bubbleArea;

        [Header("Likatahrat autossa")]
        [Tooltip("Nakyvat likatahra-kuvat autossa. Jos tyhja ja Dirt Spots Parent on asetettu, kerataan sen lapsista.")]
        public List<Image> dirtSpots = new List<Image>();
        [Tooltip("Valinnainen: vanhempi jonka Image-lapset kerataan automaattisesti Dirt Spots -listaan Startissa.")]
        public Transform dirtSpotsParent;
        [Tooltip("Likatahran haviamisanimaation kesto")]
        public float dirtFadeDuration = 0.25f;

        [Header("Edistyminen (pesu)")]
        [Tooltip("Esim. 'Puhtaus: 40 %'")]
        public TMP_Text progressText;
        [Tooltip("Valinnainen mittari (Image, Image Type = Filled), tayttyy puhtauden mukaan")]
        public Image cleanBar;

        [Header("Kiiltoefekti (valinnainen)")]
        [Tooltip("Nakyviin kun auto on taysin puhdas, esim. tahtia/kimalletta autossa")]
        public GameObject shineEffect;

        [Header("Valmis-paneeli")]
        public GameObject donePanel;
        public TMP_Text doneText;

        [Header("Palkkio (pesu)")]
        [Tooltip("Pistetta jokaisesta poistetusta likatahrasta")]
        public int pointsPerDirt = 5;

        // M5/16.8.2026 (kayttaja: "korjaa vaihe tulisi tehda samalla tavalla kuin pesu"):
        // RepairArea = BubbleArean vastine (RepairTool-ikonit kelluvat ja napautetaan),
        // DentSpots = DirtSpotsin vastine (nakyvat kolhu-merkit autossa, yksi haviaa per
        // napautus). RepairArea on POIS PAALTA pesun ajan ja aktivoituu vasta kun pesu
        // on valmis - DentSpots sen sijaan NAKYY heti alusta asti (kuten likatahratkin)
        // koska kolhut ovat jo autossa, vain niiden KORJAAMINEN odottaa pesun jalkeen.
        [Header("Korjaus (kolhut, kartioihin/laatikoihin osuminen)")]
        [Tooltip("RepairTool-ikonien yhteinen vanhempi (sama periaate kuin Bubble Area). Piilossa pesun ajan.")]
        public Transform repairArea;
        [Tooltip("Nakyvat kolhu-merkit autossa. Jos tyhja ja Dent Spots Parent on asetettu, kerataan sen lapsista.")]
        public List<Image> dentSpots = new List<Image>();
        [Tooltip("Valinnainen: vanhempi jonka Image-lapset kerataan automaattisesti Dent Spots -listaan Startissa.")]
        public Transform dentSpotsParent;
        [Tooltip("Kolhu-merkin haviamisanimaation kesto")]
        public float dentFadeDuration = 0.25f;
        [Tooltip("Esim. 'Korjattu: 40 %' tai 'Kolhuja: 2'")]
        public TMP_Text dentsText;
        [Tooltip("Lyhyt visuaalinen efekti joka nakyy hetken KAIKKIEN kolhujen korjauduttua (esim. kipinat).")]
        public GameObject repairEffect;
        [Tooltip("Kuinka pitkaan Repair Effect nakyy ennen Valmis-paneeliin siirtymista")]
        public float repairEffectDuration = 0.9f;
        [Tooltip("Pistetta jokaisesta korjatusta kolhusta")]
        public int pointsPerDentRepair = 3;

        [Header("Scene")]
        public string garageSceneName = "Garage";
        public string mainMenuScene = "MainMenu";

        int cleanedCount;
        int activeSpotCount;   // M5: kuinka monta likatahraa TÄLLÄ kertaa nakyy (ks. Start)
        int repairedCount;
        int activeDentCount;   // kuinka monta kolhua TALLA kertaa korjataan (ks. Start)
        bool finished;
        bool repairPhaseActive;
        int washReward;        // talteen yhdistettya Valmis-tekstia varten
        int repairReward;

        // KORJATTU 19.8.2026 (kayttaja: "wash scenessa virheellisesti menee yli 100%
        // prosentin"): FadeOutSpot() piilottaa (SetActive(false)) tahran vasta
        // dirtFadeDuration/dentFadeDuration sekunnin kuluttua, mutta CleanNextSpot()/
        // RepairNextDent() valitsee "seuraavan viela activeSelf-tahran" HETI. Jos
        // pelaaja napautti useampaa kuplaa/tyokalua nopeasti peräkkäin ENNEN kuin
        // ensimmainen tahra ehti kokonaan hava (esim. 0.25s sisalla), loydettiin
        // SAMA vielä-hipumassa-oleva tahra uudelleen ja cleanedCount/repairedCount
        // kasvoi useammin kuin tahroja oikeasti oli -> yli 100%. Nailla "varattu heti"
        // -taulukoilla tahra merkitaan varatuksi VALITTOMASTI napautushetkella (ei
        // vasta fadeen paatyttya), joten samaa tahraa ei voi valita kahdesti.
        bool[] dirtClaimed = new bool[0];
        bool[] dentClaimed = new bool[0];

        void Awake() { Instance = this; }

        void Start()
        {
            if (dirtSpots.Count == 0 && dirtSpotsParent != null)
                dirtSpots.AddRange(dirtSpotsParent.GetComponentsInChildren<Image>(true));
            if (dentSpots.Count == 0 && dentSpotsParent != null)
                dentSpots.AddRange(dentSpotsParent.GetComponentsInChildren<Image>(true));

            // M5.17/16.8.2026: likatahrojen/kolhujen maara tulee nyt PYSYVASTA mittarista
            // (GameManager.DirtMeter/DentMeter) - ei enaa vain taman yhden ajon osumista,
            // vaan kaikesta mika on kertynyt siita asti kun edellisen kerran pesit/korjasit.
            // Jos mittari on tyhja, pesu/korjaus ohitetaan aivan kuten ennenkin.
            int hits = GameManager.Instance != null ? GameManager.Instance.DirtMeter : dirtSpots.Count;
            activeSpotCount = Mathf.Clamp(hits, 0, dirtSpots.Count);
            int dents = GameManager.Instance != null ? GameManager.Instance.DentMeter : 0;
            activeDentCount = Mathf.Clamp(dents, 0, dentSpots.Count);

            for (int i = 0; i < dirtSpots.Count; i++)
            {
                var d = dirtSpots[i];
                if (d == null) continue;
                d.gameObject.SetActive(i < activeSpotCount);
                SetAlpha(d, 1f);
            }
            // Kolhu-merkit nakyvissa heti alusta (kolhut ovat jo autossa) - vain
            // RepairArean tyokalut (napautettavat) odottavat pesun valmistumista.
            for (int i = 0; i < dentSpots.Count; i++)
            {
                var d = dentSpots[i];
                if (d == null) continue;
                d.gameObject.SetActive(i < activeDentCount);
                SetAlpha(d, 1f);
            }

            if (shineEffect != null) shineEffect.SetActive(false);
            if (donePanel != null) donePanel.SetActive(false);
            if (repairEffect != null) repairEffect.SetActive(false);
            if (repairArea != null) repairArea.gameObject.SetActive(false);

            cleanedCount = 0;
            repairedCount = 0;
            finished = false;
            repairPhaseActive = false;
            washReward = 0;
            repairReward = 0;
            dirtClaimed = new bool[dirtSpots.Count];
            dentClaimed = new bool[dentSpots.Count];
            UpdateProgress();
            UpdateDentsText();

            if (activeSpotCount == 0)
            {
                Debug.Log("[Wash] Auto oli jo puhdas (ei latakko-osumia) - pesuvaihe ohitetaan.");
                StartRepairPhase();
            }
            else
            {
                Debug.Log($"[Wash] Pesu alkoi. Likatahroja: {activeSpotCount}");
            }
        }

        // Kutsutaan Bubble-skriptista, kun pelaaja napauttaa kuplan.
        public void OnBubblePopped()
        {
            if (finished) return;
            CleanNextSpot();
        }

        void CleanNextSpot()
        {
            // etsi seuraava viela nakyva JA VARAAMATON likatahra (ks. dirtClaimed-kommentti)
            for (int i = 0; i < dirtSpots.Count; i++)
            {
                var spot = dirtSpots[i];
                if (spot == null || !spot.gameObject.activeSelf) continue;
                if (i < dirtClaimed.Length && dirtClaimed[i]) continue;
                if (i < dirtClaimed.Length) dirtClaimed[i] = true;
                StartCoroutine(FadeOutSpot(spot, dirtFadeDuration));
                cleanedCount++;
                // M5.17/16.8.2026: puretaan PYSYVAA mittaria heti per napautus (ei vain
                // kerralla lopussa) - jos pelaaja poistuu Pesusta kesken, tehty tyo jaa talteen.
                GameManager.Instance?.CleanOneDirt();
                UpdateProgress();
                Debug.Log($"[Wash] Likatahra puhdistettu ({cleanedCount}/{activeSpotCount}).");
                if (cleanedCount >= activeSpotCount) Finish();
                return;
            }
        }

        IEnumerator FadeOutSpot(Image spot, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetAlpha(spot, Mathf.Lerp(1f, 0f, t / duration));
                yield return null;
            }
            SetAlpha(spot, 0f);
            spot.gameObject.SetActive(false);
        }

        static void SetAlpha(Image img, float a)
        {
            if (img == null) return;
            Color c = img.color;
            c.a = a;
            img.color = c;
        }

        void UpdateProgress()
        {
            // KORJATTU 19.8.2026 (kayttaja: "progress text kohtaa tulisi muuttaa
            // tilanteeseen paremmin sopivaksi"): jos autossa ei ole yhtaan likaa
            // (activeSpotCount == 0), teksti naytti aiemmin harhaanjohtavasti
            // "Puhtaus: 0 %" - vaikka auto on jo TAYSIN puhdas, ei 0% puhdas.
            // Sama periaate kuin UpdateDentsText():ssa jo on kolhuille ("Ei kolhuja").
            if (activeSpotCount <= 0)
            {
                if (progressText != null) progressText.text = "Ei likaa";
                if (cleanBar != null) cleanBar.fillAmount = 1f;
                return;
            }
            int total = Mathf.Max(1, activeSpotCount);
            // KORJATTU 19.8.2026: varmuuden vuoksi clampataan 0-100% talla tasolla
            // MYOS, vaikka juurisyy (dirtClaimed-kaksoislaskenta) on nyt korjattu -
            // nain prosentti ei voi enaa nayttaa esim. 113% missaan tilanteessa.
            int pct = Mathf.Clamp(Mathf.RoundToInt(100f * cleanedCount / total), 0, 100);
            if (progressText != null) progressText.text = $"Puhtaus: {pct} %";
            if (cleanBar != null) cleanBar.fillAmount = Mathf.Clamp01(cleanedCount / (float)total);
        }

        // Pesuvaihe valmis (kaikki likatahrat poistettu, tai niita ei ollut). Ei viela
        // Valmis-paneelia - siirrytaan seuraavaksi korjausvaiheeseen.
        void Finish()
        {
            if (finished) return;

            washReward = pointsPerDirt * cleanedCount;
            if (washReward > 0 && GameManager.Instance != null) GameManager.Instance.AddPoints(washReward);

            if (shineEffect != null) shineEffect.SetActive(cleanedCount > 0);

            Debug.Log($"[Wash] Pesu valmis! Palkkio +{washReward}. Siirrytaan korjausvaiheeseen.");
            StartRepairPhase();
        }

        // ---------- Korjausvaihe (kolhut) - SAMA MEKANIIKKA KUIN PESU ----------

        void StartRepairPhase()
        {
            // KORJATTU 16.8.2026 (tarkistuksessa loydetty softlock-riski): tarkistetaan
            // repairedCount >= activeDentCount EIKA vain activeDentCount <= 0 - muuten
            // jos pelaaja painaa "Uudestaan" pesuvaiheen jalkeen kun korjaus oli JO tehty
            // taman Pesu-kaynnin aikana (repairedCount == activeDentCount, kaikki
            // DentSpotit jo pois paalta), tama olisi avannut RepairArean uudestaan
            // vaikka mitaan napautettavaa ei enaa ollut - DonePanel (ja sen sisalla
            // Jatkoon-nappi) ei olisi koskaan naytetty uudelleen -> pelaaja jumissa.
            if (activeDentCount <= 0 || repairedCount >= activeDentCount)
            {
                Debug.Log("[Wash] Ei kolhuja korjattavana - korjausvaihe ohitetaan.");
                ShowDone();
                return;
            }

            repairPhaseActive = true;
            if (repairArea != null) repairArea.gameObject.SetActive(true);
            UpdateDentsText();
            Debug.Log($"[Wash] Korjausvaihe alkoi. Kolhuja: {activeDentCount}. Napauta tyokaluja korjataksesi.");
        }

        // Kutsutaan RepairTool-skriptista, kun pelaaja napauttaa tyokalua (sama periaate
        // kuin Bubble.cs -> OnBubblePopped, ks. luokkakommentti).
        public void OnRepairToolTapped()
        {
            if (finished || !repairPhaseActive) return;
            RepairNextDent();
        }

        void RepairNextDent()
        {
            // etsi seuraava viela nakyva JA VARAAMATON kolhu-merkki (ks. dentClaimed-kommentti)
            for (int i = 0; i < dentSpots.Count; i++)
            {
                var spot = dentSpots[i];
                if (spot == null || !spot.gameObject.activeSelf) continue;
                if (i < dentClaimed.Length && dentClaimed[i]) continue;
                if (i < dentClaimed.Length) dentClaimed[i] = true;
                StartCoroutine(FadeOutSpot(spot, dentFadeDuration));
                repairedCount++;
                // M5.17/16.8.2026: puretaan PYSYVAA mittaria heti per napautus, sama
                // periaate kuin CleanNextSpot()/CleanOneDirt yllä.
                GameManager.Instance?.RepairOneDent();
                UpdateDentsText();
                Debug.Log($"[Wash] Kolhu korjattu ({repairedCount}/{activeDentCount}).");
                if (repairedCount >= activeDentCount) FinishRepairAll();
                return;
            }
        }

        void UpdateDentsText()
        {
            if (dentsText == null) return;
            if (activeDentCount <= 0) { dentsText.text = "Ei kolhuja"; return; }
            if (!repairPhaseActive) { dentsText.text = $"Kolhuja: {activeDentCount}"; return; }
            int pct = Mathf.Clamp(Mathf.RoundToInt(100f * repairedCount / Mathf.Max(1, activeDentCount)), 0, 100);
            dentsText.text = $"Korjattu: {pct} %";
        }

        // Kaikki kolhut korjattu - palkkio, aani+efekti (kayttaja: "korjaamiseen tulisi
        // liittya jokin aani ja efekti"), sitten Valmis-paneeliin.
        void FinishRepairAll()
        {
            var gm = GameManager.Instance;
            repairReward = pointsPerDentRepair * repairedCount;
            // M5.17/16.8.2026: EI enaa gm.RepairDents() tassa - DentMeter puretaan jo
            // asteittain RepairNextDent()issa (gm.RepairOneDent() per napautus).
            if (gm != null && repairReward > 0)
            {
                gm.AddPoints(repairReward);
            }

            // Aani soi jo joka napautuksella (RepairTool.cs, sama periaate kuin
            // Bubble.cs/bubblePop) - tassa vain visuaalinen "kaikki valmis" -efekti,
            // aivan kuten shineEffect nakyy pesun paatteeksi ilman erillista aanta.
            repairPhaseActive = false;
            if (repairEffect != null) repairEffect.SetActive(true);

            Debug.Log($"[Wash] Kaikki kolhut korjattu! Palkkio +{repairReward}.");
            StartCoroutine(FinishRepairAfterEffect());
        }

        IEnumerator FinishRepairAfterEffect()
        {
            yield return new WaitForSeconds(repairEffectDuration);
            if (repairEffect != null) repairEffect.SetActive(false);
            if (repairArea != null) repairArea.gameObject.SetActive(false);
            ShowDone();
        }

        // Molemmat vaiheet (pesu+korjaus) ohi -> nayta yhdistetty Valmis-paneeli.
        void ShowDone()
        {
            if (finished) return;
            finished = true;

            int total = washReward + repairReward;
            if (donePanel != null) donePanel.SetActive(true);
            if (doneText != null)
            {
                string msg = "Kiiltavan puhdas ja ehja auto!";
                if (washReward > 0 && repairReward > 0) msg = "Pesty ja korjattu!";
                else if (repairReward > 0) msg = "Korjattu!";
                doneText.text = $"{msg} Palkkio: +{total}";
            }

            Debug.Log($"[Wash] Kaikki valmis! Yhteispalkkio +{total} (pesu {washReward} + korjaus {repairReward}).");
        }

        // "Jatkoon"-napin OnClick.
        public void OnContinueButton()
        {
            Debug.Log("[Wash] Jatketaan halliin: " + garageSceneName);
            SceneFader.LoadScene(garageSceneName); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        // Valinnainen "Ohita pesu" -nappi (esim. testausta varten).
        public void OnSkipButton()
        {
            Debug.Log("[Wash] Pesu ohitettu.");
            SceneFader.LoadScene(garageSceneName); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        // "Koti"-nappi (sama kuva/toiminto kuin Ajon maali-paneelissa) -> paavalikkoon.
        public void OnMainMenuButton()
        {
            Debug.Log("[Wash] Paavalikkoon: " + mainMenuScene);
            SceneFader.LoadScene(mainMenuScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        // "Uudestaan"-nappi: kaynnistaa PESUN uudestaan. HUOM: korjaa vain pesuvaiheen -
        // jos kolhut oli jo korjattu, ne pysyvat korjattuina (DentMeter on jo purettu
        // GameManagerissa per-napautus). RestartWash() lukee likamaaran TUOREENA
        // GameManager.DirtMeterista (ei vanhasta activeSpotCount-arvosta) - jos lika on
        // jo pesty, "Uudestaan" ei anna uutta pesupalkkiota tyhjasta (ks. RestartWash-
        // kommentti, 16.8.2026 tarkistuksessa loydetty ja korjattu bugi).
        public void OnRestartButton()
        {
            Debug.Log("[Wash] Pesu aloitetaan uudestaan.");
            RestartWash();
        }

        void RestartWash()
        {
            finished = false;
            cleanedCount = 0;

            // KORJATTU 16.8.2026 (tarkistuspyynnon yhteydessa loydetty bugi): activeSpotCount
            // EI enaa oteta vanhasta "mika oli taman kaynnin ALUSSA" -arvosta, vaan luetaan
            // TUOREENA GameManager.DirtMeterista - muuten "Uudestaan"-nappia pystyi
            // painamaan loputtomiin ja keraamaan pesupalkkion (pointsPerDirt * cleanedCount)
            // uudelleen ja uudelleen SAMASTA lika-annoksesta, vaikka DirtMeter oli jo oikeasti
            // purkautunut nollaan ensimmaisella pesukerralla (CleanOneDirt per napautus).
            int hits = GameManager.Instance != null ? GameManager.Instance.DirtMeter : 0;
            activeSpotCount = Mathf.Clamp(hits, 0, dirtSpots.Count);
            dirtClaimed = new bool[dirtSpots.Count];

            if (donePanel != null) donePanel.SetActive(false);
            if (shineEffect != null) shineEffect.SetActive(false);
            for (int i = 0; i < dirtSpots.Count; i++)
            {
                var d = dirtSpots[i];
                if (d == null) continue;
                d.gameObject.SetActive(i < activeSpotCount);
                SetAlpha(d, 1f);
            }
            UpdateProgress();
            Debug.Log($"[Wash] Pesu nollattu. Likatahroja jaljella: {activeSpotCount}");

            if (activeSpotCount == 0)
            {
                Debug.Log("[Wash] Ei enaa likaa (jo pesty) - ei uutta pesupalkkiota, tarkistetaan korjausvaihe.");
                StartRepairPhase();
            }
        }
    }
}
