using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Pakettiporina
{
    // Pitaa valitut osat kategorioittain ja laskee niista auton ominaisuudet (CarStats).
    // M3.4:ssa halli-UI kutsuu SelectPartia; nyt voit testata ContextMenu-napeilla.
    public class CarBuilder : MonoBehaviour
    {
        [Header("Perusarvot (auto ilman osia)")]
        public CarStats baseStats = new CarStats { voima = 40, pito = 45, keveys = 50, kestavyys = 45, kylmyys = 0 };

        readonly Dictionary<PartCategory, PartData> selected = new Dictionary<PartCategory, PartData>();

        public CarStats Current { get; private set; }
        public IReadOnlyDictionary<PartCategory, PartData> Selected => selected;

        void Awake() { Recalculate(); }

        public void SelectPart(PartData part)
        {
            if (part == null) return;
            selected[part.category] = part;
            Debug.Log($"[Builder] Valittu {part.category}: {part.displayName}");
            Recalculate();
        }

        public void ClearCategory(PartCategory category)
        {
            if (selected.Remove(category))
                Recalculate();
        }

        // Onko autossa juuri tama osa valittuna? (FitChecker kayttaa tata)
        public bool HasPart(PartData part)
        {
            if (part == null) return false;
            return selected.TryGetValue(part.category, out var sel) && sel == part;
        }

        void Recalculate()
        {
            CarStats s = baseStats;
            foreach (var part in selected.Values)
            {
                if (part == null || part.cosmeticOnly) continue;
                s.voima += part.voima;
                s.pito += part.pito;
                s.keveys += part.keveys;
                s.kestavyys += part.kestavyys;
                s.kylmyys += part.kylmyys;
            }
            s.Clamp(0, 100);
            Current = s;
            Debug.Log($"[Builder] Ominaisuudet -> {s}");
        }

        // ---- Testaus editorissa ilman UI:ta ----
        [Header("Testaus (veda tahan osat ja valinnainen paketti)")]
        public PartData[] testParts;
        public PackageData testPackage;

        [ContextMenu("Testaa: valitse testParts ja laske")]
        void TestSelect()
        {
            selected.Clear();
            foreach (var p in testParts) SelectPart(p);
        }

        [ContextMenu("Testaa sopivuus (testPackage)")]
        void TestFit()
        {
            var r = FitChecker.Check(this, testPackage);
            Debug.Log($"[Fit] {r.message} (sopii={r.fits})");
        }
    }
}