/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Hallin UI karusellina: valitse kategoria, selaa osia isoilla nuolilla.
    // Jokainen selaus sovittaa osan heti -> mittarit liikkuvat. Lapsiystavallinen.
    public class GarageScreen : MonoBehaviour
    {
        [Header("Logiikka")]
        public CarBuilder builder;
        [Tooltip("Kaikki osatiedostot (Data/Parts)")]
        public List<PartData> allParts = new List<PartData>();
        [Tooltip("Kaikki pakettitiedostot (Data/Packages)")]
        public List<PackageData> allPackages = new List<PackageData>();

        [Header("Kategoria")]
        public TMP_Text categoryText;   // nykyisen kategorian nimi

        [Header("Osaselain")]
        public TMP_Text partNameText;   // nykyisen osan nimi (nuolten valissa)

        [Header("Mittarit (Image, Image Type = Filled)")]
        public Image barVoima, barPito, barKeveys, barKestavyys, barKylmyys;

        [Header("Pakettiselain")]
        public TMP_Text packageNameText; // nykyisen paketin nimi
        public TMP_Text fitText;         // "Sopii keikkaan!" / "Tarvitsee: ..."
        public TMP_Text vilkkuText;      // valinnainen opasteksti

        [Header("Ajo")]
        public string gameSceneName = "SampleScene";

        // sisainen tila
        readonly Dictionary<PartCategory, List<PartData>> byCat = new Dictionary<PartCategory, List<PartData>>();
        readonly Dictionary<PartCategory, int> indexByCat = new Dictionary<PartCategory, int>();
        PartCategory currentCat = PartCategory.Kori;
        int packageIndex = 0;

        void Start()
        {
            if (builder == null) builder = FindObjectOfType<CarBuilder>();
            BuildLookups();
            ApplyAllDefaults();               // oletusauto: jokaisen kategorian eka osa
            SetCategory((int)PartCategory.Kori);
            if (allPackages.Count > 0) ShowPackage();
            UpdateStats();
            Debug.Log("[Garage] Halli valmis.");
        }

        void BuildLookups()
        {
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                byCat[c] = new List<PartData>();
                indexByCat[c] = 0;
            }
            foreach (var p in allParts)
                if (p != null) byCat[p.category].Add(p);
        }

        void ApplyAllDefaults()
        {
            foreach (var kv in byCat)
                if (kv.Value.Count > 0) builder.SelectPart(kv.Value[0]);
        }

        // ---- Kategorianapit kutsuvat tata: 0=Kori,1=Renkaat,2=Moottori,3=Jouset,4=Lisat,5=Maali ----
        public void SetCategory(int catIndex)
        {
            currentCat = (PartCategory)catIndex;
            if (categoryText != null) categoryText.text = CategoryName(currentCat);
            ShowPart();
            Debug.Log($"[Garage] Kategoria: {currentCat}");
        }

        // ---- Osaselain (nuolinapit) ----
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
            builder.SelectPart(list[i]);   // sovita heti -> mittarit liikkuvat
            ShowPart();
            UpdateStats();
            UpdateFit();
        }

        void ShowPart()
        {
            var list = byCat[currentCat];
            if (partNameText != null)
                partNameText.text = list.Count == 0 ? "(ei osia)" : list[indexByCat[currentCat]].displayName;
        }

        // ---- Pakettiselain (nuolinapit) ----
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
            UpdateFit();
            Debug.Log($"[Garage] Paketti: {pkg.displayName}");
        }

        PackageData CurrentPackage => allPackages.Count > 0 ? allPackages[packageIndex] : null;

        // ---- Mittarit ja sopivuus ----
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
                vilkkuText.text = r.fits ? "Hyvä valinta!" : "Katso mitä paketti tarvitsee.";
        }

        // ---- "Aja keikka" -napin OnClick ----
        public void OnDrive()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetSelection(CurrentPackage, builder.Current);
            Debug.Log("[Garage] Aja keikka -> " + gameSceneName);
            SceneManager.LoadScene(gameSceneName);
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

*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Pakettiporina
{
    // Hallin UI karusellina + aktiivisen tabin korostus + auton varin valinta (Maali).
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
        public Color tabNormalColor = new Color(0.945f, 0.937f, 0.909f); // #F1EFE8
        public Color tabActiveColor = new Color(0.114f, 0.62f, 0.459f);  // #1D9E75

        [Header("Osaselain")]
        public TMP_Text partNameText;

        [Header("Auton esikatselu")]
        [Tooltip("Auton kuva (Image), jonka väri vaihtuu Maali-osasta")]
        public Image carPreview;

        [Header("Mittarit (Image, Image Type = Filled)")]
        public Image barVoima, barPito, barKeveys, barKestavyys, barKylmyys;

        [Header("Pakettiselain")]
        public TMP_Text packageNameText;
        public TMP_Text fitText;
        public TMP_Text vilkkuText;

        [Header("Ajo")]
        public string gameSceneName = "SampleScene";

        readonly Dictionary<PartCategory, List<PartData>> byCat = new Dictionary<PartCategory, List<PartData>>();
        readonly Dictionary<PartCategory, int> indexByCat = new Dictionary<PartCategory, int>();
        PartCategory currentCat = PartCategory.Kori;
        int packageIndex = 0;

        void Start()
        {
            if (builder == null) builder = FindObjectOfType<CarBuilder>();
            BuildLookups();
            ApplyAllDefaults();
            SetCategory((int)PartCategory.Kori);
            if (allPackages.Count > 0) ShowPackage();
            UpdateStats();
            Debug.Log("[Garage] Halli valmis.");
        }

        void BuildLookups()
        {
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                byCat[c] = new List<PartData>();
                indexByCat[c] = 0;
            }
            foreach (var p in allParts)
                if (p != null) byCat[p.category].Add(p);
        }

        void ApplyAllDefaults()
        {
            foreach (var kv in byCat)
                if (kv.Value.Count > 0) ApplyPart(kv.Value[0]);
        }

        // Sovita osa autoon + hoida varin vaihto Maali-osalle.
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
            ApplyPart(list[i]);
            ShowPart();
            UpdateStats();
            UpdateFit();
        }

        void ShowPart()
        {
            var list = byCat[currentCat];
            if (partNameText != null)
                partNameText.text = list.Count == 0 ? "(ei osia)" : list[indexByCat[currentCat]].displayName;
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
            UpdateFit();
            Debug.Log($"[Garage] Paketti: {pkg.displayName}");
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
                vilkkuText.text = r.fits ? "Hyvä valinta!" : "Katso mitä paketti tarvitsee.";
        }

        public void OnDrive()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetSelection(CurrentPackage, builder.Current);
            Debug.Log("[Garage] Aja keikka -> " + gameSceneName);
            SceneManager.LoadScene(gameSceneName);
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