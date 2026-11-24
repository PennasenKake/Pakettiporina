using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Hallin UI: karuselli, tab-korostus, varinvalinta JA auton sailyminen keikkojen valilla.
    public class GarageScreen : MonoBehaviour
    {
        [Header("Logiikka")]
        public CarBuilder builder;
        public List<PartData> allParts = new List<PartData>();
        public List<PackageData> allPackages = new List<PackageData>();

        [Header("Kategoria")]
        public TMP_Text categoryText;

        [Header("Kategoriavälilehdet (korostus)")]
        [Tooltip("Järjestys: 0=Kori,1=Renkaat,2=Moottori,3=Jouset,4=Lisat,5=Maali")]
        public Button[] categoryTabs;
        public Color tabNormalColor = new Color(0.945f, 0.937f, 0.909f);
        public Color tabActiveColor = new Color(0.114f, 0.62f, 0.459f);

        [Header("Osaselain")]
        public TMP_Text partNameText;

        [Header("Auton esikatselu")]
        public Image carPreview;

        [Header("Mittarit (Image, Image Type = Filled)")]
        public Image barVoima, barPito, barKeveys, barKestavyys, barKylmyys;

        [Header("Pakettiselain")]
        public TMP_Text packageNameText;
        public TMP_Text fitText;
        public TMP_Text vilkkuText;
        [Tooltip("Pieni kuva nykyisesta paketista (PackageData.icon)")]
        public Image packageImage;

        [Header("Ajo")]
        public string gameSceneName = "SampleScene";

        [Header("Paavalikko")]
        [Tooltip("Nimi scenelle johon 'Koti'-nappi vie. Lisaa scene Build Settingsiin!")]
        public string mainMenuScene = "MainMenu";

        [Header("Tilastopaneeli (napin takana)")]
        [Tooltip("Paneeli joka sisaltaa mittaripalkit + sopivuustekstin. Piilotetaan automaattisesti alussa.")]
        public GameObject statsPanel;

        [Header("Osien lukitus (pisteilla)")]
        [Tooltip("Nakyy kun selattu osa on viela lukossa (PartData.unlockPoints > pisteet). Valinnainen.")]
        public TMP_Text lockText;

        [Header("Ratojen lukitus (pisteilla, PackageData.unlockPoints)")]
        [Tooltip("Nakyy kun selattu paketti (ja sen rata) on viela lukossa. Valinnainen.")]
        public TMP_Text packageLockText;

        // M5/15.8.2026: Kolhu-UI ("Kolhuja: N" + "Korjaa"-nappi) SIIRRETTY Pesu-sceneen
        // (WashScreen.cs) - kayttaja: "korjaa tulisi olla wash scenen kanssa samassa,
        // ensin pestaan ja sen jalkeen korjataan". Halli ei enaa nayta/korjaa kolhuja
        // ollenkaan - katso WashScreen.dentsText/repairArea + RepairTool.cs.

        readonly Dictionary<PartCategory, List<PartData>> byCat = new Dictionary<PartCategory, List<PartData>>();
        readonly Dictionary<PartCategory, int> indexByCat = new Dictionary<PartCategory, int>();
        PartCategory currentCat = PartCategory.Kori;
        int packageIndex = 0;

        void Start()
        {
            if (builder == null) builder = FindObjectOfType<CarBuilder>();
            if (statsPanel != null) statsPanel.SetActive(false);   // piilossa oletuksena, avataan napista
            BuildLookups();
            LoadSavedOrDefaults();                 // <- lataa tallennettu auto TAI oletusosat
            RestorePackage();                      // <- lataa tallennettu paketti jos on
            EnsureWorkingCar();                    // <- varmista etta "Aja" toimii heti
            BuildTracks();                         // <- radat (vapaa valinta) + tallennettu/ehdotettu rata
            SetCategory((int)PartCategory.Kori);
            UpdateStats();
            GrowSmallTexts();
            if (useWizard && GetComponent<GarageWizard>() == null) gameObject.AddComponent<GarageWizard>();
            Debug.Log("[Garage] Halli valmis.");
        }

        // LISATTY 23.11.2026: pienet tekstit (kategoriapainikkeet, mittarien nimet, Vilkun kupla) olivat
        // liian pienia (~20-30 px 1080x1920-canvasilla). Sallitaan kasvu tekstilaatikon sisalla:
        // alaraja = nykyinen koko, ylaraja = 1.7x (vahintaan 44). Autosize sovittaa tekstin laatikkoon.
        void GrowSmallTexts()
        {
            foreach (var t in FindObjectsOfType<TMP_Text>(true))
            {
                if (t == null || t.enableAutoSizing || t.fontSize >= 44f) continue;
                float baseSize = t.fontSize;
                t.enableAutoSizing = true;
                t.fontSizeMin = baseSize;
                t.fontSizeMax = Mathf.Max(44f, baseSize * 1.7f);
                t.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        // ---- LISATTY 23.11.2026: halli elavammaksi + valmis auto ----------------------------

        // Esivalinta: jos nykyinen auto ei sovi nykyiselle paketille (tai paketti on lukossa),
        // valitaan avoin paketti jolle auto sopii, tai lisataan puuttuva vaadittu osa.
        // Pelaaja voi siis painaa heti "Aja".
        void EnsureWorkingCar()
        {
            if (allPackages.Count == 0 || builder == null) return;

            var cur = CurrentPackage;
            if (!IsPackageLocked(cur) && FitChecker.Check(builder, cur).fits) return;

            // 1) Etsi avoin paketti jolle nykyinen auto jo sopii.
            for (int i = 0; i < allPackages.Count; i++)
            {
                var p = allPackages[i];
                if (p == null || IsPackageLocked(p)) continue;
                if (FitChecker.Check(builder, p).fits) { packageIndex = i; ShowPackage(); return; }
            }

            // 2) Muuten lisaa avoimen paketin vaatima osa autoon.
            for (int i = 0; i < allPackages.Count; i++)
            {
                var p = allPackages[i];
                if (p == null || IsPackageLocked(p) || p.requiredPart == null) continue;
                var req = p.requiredPart;
                if (IsLocked(req) || !byCat.ContainsKey(req.category)) continue;
                int idx = byCat[req.category].IndexOf(req);
                if (idx < 0) continue;
                indexByCat[req.category] = idx;
                ApplyPart(req);
                packageIndex = i;
                ShowPackage();
                Debug.Log($"[Garage] Esivalittu toimiva auto: {req.displayName} -> {p.displayName}");
                return;
            }
        }

        // Pomppu/puristus auton esikatselulle osan vaihtuessa.
        Vector3 previewBaseScale = Vector3.one;
        bool previewScaleKnown;
        Coroutine bumpRoutine;

        void BumpPreview()
        {
            if (carPreview == null || !isActiveAndEnabled) return;
            var rt = carPreview.rectTransform;
            if (!previewScaleKnown) { previewBaseScale = rt.localScale; previewScaleKnown = true; }
            if (bumpRoutine != null) StopCoroutine(bumpRoutine);
            bumpRoutine = StartCoroutine(BumpRoutine(rt));
        }

        IEnumerator BumpRoutine(RectTransform rt)
        {
            const float dur = 0.32f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                // vaimeneva heilahdus: puristuu leveaksi, venyy korkeaksi, asettuu
                float w = Mathf.Sin(k * Mathf.PI * 3f) * (1f - k) * 0.12f;
                rt.localScale = new Vector3(previewBaseScale.x * (1f + w), previewBaseScale.y * (1f - w), previewBaseScale.z);
                yield return null;
            }
            rt.localScale = previewBaseScale;
            bumpRoutine = null;
        }

        // Mittarit liukuvat kohdearvoon.
        Image[] barImages;
        float[] barTargets;

        void Update()
        {
            if (barImages == null) return;
            float step = Time.unscaledDeltaTime * 2.2f;
            for (int i = 0; i < barImages.Length; i++)
            {
                var b = barImages[i];
                if (b != null) b.fillAmount = Mathf.MoveTowards(b.fillAmount, barTargets[i], step);
            }
        }

        static string StarsLeft(int need)
        {
            return need == 1 ? "Vielä 1 tähti" : $"Vielä {need} tähteä";
        }

        // "Tiedot"/"i"-napin OnClick: avaa/sulkee tilastopaneelin.
        public void ToggleStatsPanel()
        {
            if (statsPanel == null) return;
            statsPanel.SetActive(!statsPanel.activeSelf);
        }

        void BuildLookups()
        {
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                byCat[c] = new List<PartData>();
                indexByCat[c] = 0;
            }
            int pts = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            foreach (var p in allParts)
            {
                if (p == null) continue;
                // Salainen osa ei nay osaselaimessa OLLENKAAN ennen kuin pisteraja tayttyy -
                // ei siis pelkkaa "lukossa"-tekstia, vaan osa puuttuu listasta kokonaan.
                // Kun pelaaja on kerannyt tarpeeksi pisteita, osa ilmestyy yllatyksena seuraavalla
                // hallikaynnilla (BuildLookups ajetaan Start():ssa).
                // 24.11.2026: salaisuus aukeaa ajotahdilla (Progress), ei kolikoilla: kolikot kuluvat tarroihin eivatka saa lukita osaa uudelleen.
                if (p.secret && Progress.PartLocked(p)) continue;
                byCat[p.category].Add(p);
            }
        }

        // Lataa GameManagerin tallennetut osat (jos on) oikeilla indekseilla, muuten oletukset.
        void LoadSavedOrDefaults()
        {
            var gm = GameManager.Instance;
            var saved = (gm != null) ? gm.SelectedParts : null;
            var loaded = new HashSet<PartCategory>();

            if (saved != null && saved.Count > 0)
            {
                foreach (var part in saved)
                {
                    if (part == null) continue;
                    var list = byCat[part.category];
                    int idx = list.IndexOf(part);
                    // 24.11.2026: lukittua osaa ei oteta tallennuksesta (vanha tallennus ennen ajotahtilukituksia)
                    if (idx >= 0 && !IsLocked(part))
                    {
                        indexByCat[part.category] = idx;
                        ApplyPart(part);
                        loaded.Add(part.category);
                    }
                }
                Debug.Log($"[Garage] Ladattu tallennettu auto ({loaded.Count} osaa).");
            }

            // Kategoriat joita ei ladattu -> ensimmainen AVOIN osa (lukittu osa ei saa olla oletus)
            foreach (var kv in byCat)
                if (kv.Value.Count > 0 && !loaded.Contains(kv.Key))
                {
                    int di = 0;
                    for (int k = 0; k < kv.Value.Count; k++) if (!IsLocked(kv.Value[k])) { di = k; break; }
                    indexByCat[kv.Key] = di;
                    ApplyPart(kv.Value[di]);
                }
        }

        void RestorePackage()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.SelectedPackage != null)
            {
                int idx = allPackages.IndexOf(gm.SelectedPackage);
                if (idx >= 0) packageIndex = idx;
            }
            if (allPackages.Count > 0) ShowPackage();
        }

        void ApplyPart(PartData part)
        {
            builder.SelectPart(part);
            if (part.category == PartCategory.Maali && carPreview != null)
                CarColor.Paint(carPreview, part.color); // oikea uudelleenvarjays (ei kertovaa sointua)
        }

        public void SetCategory(int catIndex)
        {
            currentCat = (PartCategory)catIndex;
            if (categoryText != null) categoryText.text = CategoryName(currentCat);
            HighlightTabs(catIndex);
            ShowPart();
            Debug.Log($"[Garage] Kategoria: {currentCat}");
        }

        void HighlightTabs(int active)
        {
            if (categoryTabs == null) return;
            for (int i = 0; i < categoryTabs.Length; i++)
            {
                if (categoryTabs[i] == null) continue;
                Image img = categoryTabs[i].GetComponent<Image>();
                if (img != null) img.color = (i == active) ? tabActiveColor : tabNormalColor;
            }
        }

        public void NextPart() { StepPart(+1); }
        public void PrevPart() { StepPart(-1); }

        void StepPart(int dir)
        {
            var list = byCat[currentCat];
            if (list.Count == 0) return;
            int i = indexByCat[currentCat] + dir;
            if (i < 0) i = list.Count - 1;
            if (i >= list.Count) i = 0;
            indexByCat[currentCat] = i;

            var part = list[i];
            if (!IsLocked(part))
            {
                ApplyPart(part);
                BumpPreview();
            }
            // Jos osa on lukossa: autoon jaa edellinen valittu osa, mutta selain nayttaa
            // silti lukitun osan nimen + lockTextin ("tarvitset X pistetta").

            ShowPart();
            UpdateStats();
            UpdateFit();
        }

        void ShowPart()
        {
            var list = byCat[currentCat];
            if (list.Count == 0)
            {
                if (partNameText != null) partNameText.text = "(ei osia)";
                if (lockText != null) lockText.text = "";
                return;
            }
            var part = list[indexByCat[currentCat]];
            if (partNameText != null) partNameText.text = part.displayName;
            if (lockText != null)
                lockText.text = IsLocked(part) ? StarsLeft(Progress.PartLeft(part)) : "";
        }

        // Onko osa viela lukossa? Ensisijaisesti ajon tahdet (Progress-taulukko), muuten vanha unlockPoints-saanto.
        bool IsLocked(PartData part)
        {
            return Progress.PartLocked(part);
        }

        public void NextPackage() { StepPackage(+1); }
        public void PrevPackage() { StepPackage(-1); }

        void StepPackage(int dir)
        {
            if (allPackages.Count == 0) return;
            packageIndex += dir;
            if (packageIndex < 0) packageIndex = allPackages.Count - 1;
            if (packageIndex >= allPackages.Count) packageIndex = 0;
            ShowPackage();
            trackIndex = SuggestedTrackIndex();   // uusi paketti -> rata palaa ehdotukseen
        }

        void ShowPackage()
        {
            var pkg = allPackages[packageIndex];
            if (packageNameText != null) packageNameText.text = pkg.displayName;
            if (packageImage != null)
            {
                packageImage.sprite = pkg.icon;
                packageImage.enabled = pkg.icon != null; // piilota jos kuva puuttuu, ei nayta harmaata laatikkoa
            }
            if (packageLockText != null)
                packageLockText.text = IsPackageLocked(pkg) ? StarsLeft(Progress.PackageLeft(pkg)) : "";
            UpdateFit();
            Debug.Log($"[Garage] Paketti: {pkg.displayName}");
        }

        // Onko paketti (ja sita kautta sen rata) viela lukossa? unlockPoints=0 (oletus) = aina auki.
        bool IsPackageLocked(PackageData pkg)
        {
            return Progress.PackageLocked(pkg);
        }

        PackageData CurrentPackage => allPackages.Count > 0 ? allPackages[packageIndex] : null;

        // Uusin FitChecker-tulos - VilkkuController lukee TAMAN suoraan sen sijaan
        // etta paattelisi tilan fitText:n nakyvasta merkkijonosta (hauras, hajoaisi
        // hiljaa jos viestien sanamuotoa joskus muutetaan).
        public FitChecker.Result CurrentFit { get; private set; }

        // Soitetaan kuittausaani vain kun sopivuus VAIHTUU ei-sopivasta sopivaksi - ei joka
        // kerta kun UpdateFit ajetaan (esim. joka osa-askeleella), muuten aani spammaisi.
        bool wasFitting;

        static int CurrentPoints() { return GameManager.Instance != null ? GameManager.Instance.Points : 0; }

        void UpdateStats()
        {
            CarStats s = builder.Current;
            bool first = barImages == null;
            if (first)
            {
                barImages = new[] { barVoima, barPito, barKeveys, barKestavyys, barKylmyys };
                barTargets = new float[5];
            }
            barTargets[0] = s.voima / 100f;
            barTargets[1] = s.pito / 100f;
            barTargets[2] = s.keveys / 100f;
            barTargets[3] = s.kestavyys / 100f;
            barTargets[4] = s.kylmyys / 100f;
            // Ensimmainen paivitys asetetaan heti (ei animoida nollasta), myohemmat liukuvat Update():ssa.
            if (first)
                for (int i = 0; i < barImages.Length; i++)
                    if (barImages[i] != null) barImages[i].fillAmount = barTargets[i];
        }

        void UpdateFit()
        {
            FitChecker.Result r = FitChecker.Check(builder, CurrentPackage);
            CurrentFit = r;
            if (fitText != null)
            {
                fitText.text = r.message;
                fitText.color = r.fits ? new Color(0.23f, 0.55f, 0.07f) : new Color(0.52f, 0.31f, 0.04f);
            }
            if (vilkkuText != null)
                vilkkuText.text = r.fits ? "Hyvä valinta!" : "Katso mitä paketti tarvitsee.";

            if (r.fits && !wasFitting && AudioManager.Instance != null)
                AudioManager.Instance.PlayFitGood();
            wasFitting = r.fits;
        }

        // "Aja keikka": tallenna paketti, mittarit JA valitut osat -> auto sailyy.
        // Jos paketilla on oma trackScene (esim. "Game2"), ajetaan silla - muuten oletusradalla.
        public void OnDrive()
        {
            var pkgToCheck = CurrentPackage;
            if (IsPackageLocked(pkgToCheck))
            {
                Debug.Log($"[Garage] '{pkgToCheck.displayName}' on viela lukossa (puuttuu {Progress.PackageLeft(pkgToCheck)} tahtea) - ei lahdeta ajoon.");
                if (packageLockText != null) StartCoroutine(FlashLock(packageLockText));
                return;
            }
            if (IsTrackLocked(trackIndex))
            {
                Debug.Log($"[Garage] Rata '{CurrentTrackScene}' on viela lukossa - ei lahdeta ajoon.");
                return;
            }

            // Rata: Hallissa vapaasti valittu (oletus = paketin ehdotus).
            string scene = CurrentTrackScene;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSelection(CurrentPackage, builder.Current, builder.GetSelectedList());
                GameManager.Instance.SelectedTrack = scene;
            }
            SaveManager.Instance?.Save();   // tallenna auto+paketti+rata heti kun keikka lahtee

            Debug.Log("[Garage] Aja keikka -> " + scene);
            SceneFader.LoadScene(scene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        IEnumerator FlashLock(TMP_Text t)
        {
            var original = t.color;
            t.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            if (t != null) t.color = original;
        }

        // ---- LISATTY 23.11.2026: julkinen API kolmivaiheiselle Hallille (GarageWizard) ----------
        [Header("Kolmivaiheinen halli")]
        [Tooltip("Rakentaa Paketti -> Rata -> Auto -nakymat koodilla vanhan kaikki-yhdessa-nakyman paalle.")]
        public bool useWizard = true;

        public static string StarsLeftPublic(int need) { return StarsLeft(need); }
        public int PackageCount => allPackages.Count;
        public PackageData CurrentPackageData => CurrentPackage;
        public bool IsCurrentPackageLocked => IsPackageLocked(CurrentPackage);
        public int PackageStarsNeeded => CurrentPackage == null ? 0 : Progress.PackageLeft(CurrentPackage);

        // ---- LISATTY 23.11.2026: radan vapaa valinta (selaus). Paketti ehdottaa rataa (trackScene), mutta pelaaja voi vaihtaa. ----
        readonly List<string> trackScenes = new List<string>();
        int trackIndex;

        void BuildTracks()
        {
            trackScenes.Clear();
            trackScenes.Add(gameSceneName);
            foreach (var n in new[] { "Satama", "Puisto", "Huvipuisto", "Maatila", "Kyla" })
                if (Application.CanStreamedLevelBeLoaded(n)) trackScenes.Add(n);

            // tallennettu valinta jos se on edelleen avoin, muuten paketin ehdotus
            var gm = GameManager.Instance;
            trackIndex = SuggestedTrackIndex();
            if (gm != null && !string.IsNullOrEmpty(gm.SelectedTrack))
            {
                int i = trackScenes.IndexOf(gm.SelectedTrack);
                if (i >= 0 && !IsTrackLocked(i)) trackIndex = i;
            }
        }

        public int TrackCount => trackScenes.Count;
        public int TrackIndex => trackIndex;
        public string CurrentTrackScene => trackScenes.Count > 0 ? trackScenes[Mathf.Clamp(trackIndex, 0, trackScenes.Count - 1)] : gameSceneName;
        public string TrackSceneAt(int i) { return i >= 0 && i < trackScenes.Count ? trackScenes[i] : gameSceneName; }
        public bool IsTrackLocked(int i) { return Progress.TrackLocked(TrackSceneAt(i)); }
        public int TrackStarsNeeded(int i) { return Progress.TrackLeft(TrackSceneAt(i)); }
        public bool IsCurrentTrackLocked => IsTrackLocked(trackIndex);

        // Paketin ehdottama rata; jos se on lukossa tai puuttuu, ensimmainen avoin rata (kotirata on aina auki).
        public int SuggestedTrackIndex()
        {
            var pkg = CurrentPackage;
            string want = pkg != null && !string.IsNullOrEmpty(pkg.trackScene) ? pkg.trackScene : gameSceneName;
            int i = trackScenes.IndexOf(want);
            if (i < 0 || IsTrackLocked(i)) return 0;
            return i;
        }
        public bool IsSuggestedTrack(int i) { return pkgSuggestsTrack(i); }
        bool pkgSuggestsTrack(int i)
        {
            var pkg = CurrentPackage;
            string want = pkg != null && !string.IsNullOrEmpty(pkg.trackScene) ? pkg.trackScene : gameSceneName;
            return TrackSceneAt(i) == want;
        }

        public void NextTrack() { if (trackScenes.Count > 0) trackIndex = (trackIndex + 1) % trackScenes.Count; }
        public void PrevTrack() { if (trackScenes.Count > 0) trackIndex = (trackIndex - 1 + trackScenes.Count) % trackScenes.Count; }
        public int PackageIndex => packageIndex;
        public int Points => CurrentPoints();
        public PartCategory CurrentCategory => currentCat;

        public IList<PartData> PartsIn(PartCategory c) { return byCat.ContainsKey(c) ? byCat[c] : new List<PartData>(); }
        public int PartIndexIn(PartCategory c) { return indexByCat.ContainsKey(c) ? indexByCat[c] : 0; }
        public bool IsPartLocked(PartData p) { return IsLocked(p); }
        public int PartStarsNeeded(PartData p) { return p == null ? 0 : Progress.PartLeft(p); }

        // Valitsee osan suoraan (swatch/chip-napit). Lukittu osa vain selataan -> nayttaa lukon.
        public void SelectPartIndex(PartCategory c, int idx)
        {
            var list = PartsIn(c);
            if (idx < 0 || idx >= list.Count) return;
            currentCat = c;
            indexByCat[c] = idx;
            var part = list[idx];
            if (!IsLocked(part)) { ApplyPart(part); BumpPreview(); }
            ShowPart();
            UpdateStats();
            UpdateFit();
        }

        // Valitun osan lukitustila: onko selattu osa lukossa (autossa on silloin edellinen osa).
        public bool BrowsedPartLocked(PartCategory c)
        {
            var list = PartsIn(c);
            return list.Count > 0 && IsLocked(list[Mathf.Clamp(indexByCat[c], 0, list.Count - 1)]);
        }

        public bool CanDrive => !IsPackageLocked(CurrentPackage) && !IsCurrentTrackLocked;

        // "Koti"-napin OnClick -> paavalikkoon.
        public void OnMainMenuButton()
        {
            Debug.Log("[Garage] Paavalikkoon: " + mainMenuScene);
            SceneFader.LoadScene(mainMenuScene); // KORJATTU 20.8.2026: haivytetty scenevaihto
        }

        string CategoryName(PartCategory c)
        {
            switch (c)
            {
                case PartCategory.Kori: return "Kori";
                case PartCategory.Renkaat: return "Renkaat";
                case PartCategory.Moottori: return "Moottori";
                case PartCategory.Jouset: return "Jouset";
                case PartCategory.Lisat: return "Lisät";
                case PartCategory.Maali: return "Maali";
                default: return c.ToString();
            }
        }
    }
}
