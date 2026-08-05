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

        [Header("Kategoriav�lilehdet (korostus)")]
        [Tooltip("J�rjestys: 0=Kori,1=Renkaat,2=Moottori,3=Jouset,4=Lisat,5=Maali")]
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
            SetCategory((int)PartCategory.Kori);
            UpdateStats();
            Debug.Log("[Garage] Halli valmis.");
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
                if (p.secret && pts < p.unlockPoints) continue;
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
                    if (idx >= 0)
                    {
                        indexByCat[part.category] = idx;
                        ApplyPart(part);
                        loaded.Add(part.category);
                    }
                }
                Debug.Log($"[Garage] Ladattu tallennettu auto ({loaded.Count} osaa).");
            }

            // Kategoriat joita ei ladattu -> oletusosa (index 0)
            foreach (var kv in byCat)
                if (kv.Value.Count > 0 && !loaded.Contains(kv.Key))
                {
                    indexByCat[kv.Key] = 0;
                    ApplyPart(kv.Value[0]);
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
                carPreview.color = part.color;
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
                ApplyPart(part);
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
                lockText.text = IsLocked(part) ? $"Lukossa - tarvitset {part.unlockPoints} pistetta" : "";
        }

        // Onko osa viela lukossa? unlockPoints=0 (oletus) tarkoittaa aina auki.
        bool IsLocked(PartData part)
        {
            if (part == null || part.unlockPoints <= 0) return false;
            int pts = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            return pts < part.unlockPoints;
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
                packageLockText.text = IsPackageLocked(pkg) ? $"Lukossa - tarvitset {pkg.unlockPoints} pistetta" : "";
            UpdateFit();
            Debug.Log($"[Garage] Paketti: {pkg.displayName}");
        }

        // Onko paketti (ja sita kautta sen rata) viela lukossa? unlockPoints=0 (oletus) = aina auki.
        bool IsPackageLocked(PackageData pkg)
        {
            if (pkg == null || pkg.unlockPoints <= 0) return false;
            int pts = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            return pts < pkg.unlockPoints;
        }

        PackageData CurrentPackage => allPackages.Count > 0 ? allPackages[packageIndex] : null;

        void UpdateStats()
        {
            CarStats s = builder.Current;
            if (barVoima) barVoima.fillAmount = s.voima / 100f;
            if (barPito) barPito.fillAmount = s.pito / 100f;
            if (barKeveys) barKeveys.fillAmount = s.keveys / 100f;
            if (barKestavyys) barKestavyys.fillAmount = s.kestavyys / 100f;
            if (barKylmyys) barKylmyys.fillAmount = s.kylmyys / 100f;
        }

        void UpdateFit()
        {
            FitChecker.Result r = FitChecker.Check(builder, CurrentPackage);
            if (fitText != null)
            {
                fitText.text = r.message;
                fitText.color = r.fits ? new Color(0.23f, 0.55f, 0.07f) : new Color(0.52f, 0.31f, 0.04f);
            }
            if (vilkkuText != null)
                vilkkuText.text = r.fits ? "Hyv� valinta!" : "Katso mit� paketti tarvitsee.";
        }

        // "Aja keikka": tallenna paketti, mittarit JA valitut osat -> auto sailyy.
        // Jos paketilla on oma trackScene (esim. "Game2"), ajetaan silla - muuten oletusradalla.
        public void OnDrive()
        {
            var pkgToCheck = CurrentPackage;
            if (IsPackageLocked(pkgToCheck))
            {
                Debug.Log($"[Garage] '{pkgToCheck.displayName}' on viela lukossa (tarvitset {pkgToCheck.unlockPoints} pistetta) - ei lahdeta ajoon.");
                if (packageLockText != null) StartCoroutine(FlashLock(packageLockText));
                return;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.SetSelection(CurrentPackage, builder.Current, builder.GetSelectedList());
            SaveManager.Instance?.Save();   // tallenna auto+paketti heti kun keikka lahtee

            string scene = gameSceneName;
            var pkg = CurrentPackage;
            if (pkg != null && !string.IsNullOrEmpty(pkg.trackScene))
                scene = pkg.trackScene;

            Debug.Log("[Garage] Aja keikka -> " + scene);
            SceneManager.LoadScene(scene);
        }

        IEnumerator FlashLock(TMP_Text t)
        {
            var original = t.color;
            t.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            if (t != null) t.color = original;
        }

        // "Koti"-napin OnClick -> paavalikkoon.
        public void OnMainMenuButton()
        {
            Debug.Log("[Garage] Paavalikkoon: " + mainMenuScene);
            SceneManager.LoadScene(mainMenuScene);
        }

        string CategoryName(PartCategory c)
        {
            switch (c)
            {
                case PartCategory.Kori: return "Kori";
                case PartCategory.Renkaat: return "Renkaat";
                case PartCategory.Moottori: return "Moottori";
                case PartCategory.Jouset: return "Jouset";
                case PartCategory.Lisat: return "Lis�t";
                case PartCategory.Maali: return "Maali";
                default: return c.ToString();
            }
        }
    }
}
