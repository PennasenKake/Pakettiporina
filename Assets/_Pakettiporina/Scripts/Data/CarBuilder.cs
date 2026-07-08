using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Pitaa valitut osat kategorioittain ja laskee niista auton ominaisuudet (CarStats).
    // M3.4:ssa halli-UI kutsuu SelectPartia; nyt voit testata ContextMenu-napilla.
    public class CarBuilder : MonoBehaviour
    {
        [Header("Perusarvot (auto ilman osia)")]
        public CarStats baseStats = new CarStats { voima = 40, pito = 45, keveys = 50, kestavyys = 45, kylmyys = 0 };

        // Valitut osat: yksi per kategoria.
        readonly Dictionary<PartCategory, PartData> selected = new Dictionary<PartCategory, PartData>();

        // Lasketut ominaisuudet (paivittyy kun osa vaihtuu).
        public CarStats Current { get; private set; }
        public IReadOnlyDictionary<PartCategory, PartData> Selected => selected;

        void Awake() { Recalculate(); }

        // Valitse osa (korvaa saman kategorian aiemman valinnan).
        public void SelectPart(PartData part)
        {
            if (part == null) return;
            selected[part.category] = part;
            Debug.Log($"[Builder] Valittu {part.category}: {part.displayName}");
            Recalculate();
        }

        // Poista kategorian valinta (esim. Lisat pois).
        public void ClearCategory(PartCategory category)
        {
            if (selected.Remove(category))
                Recalculate();
        }

        void Recalculate()
        {
            CarStats s = baseStats; // struct kopioituu -> baseStats ei muutu
            foreach (var part in selected.Values)
            {
                if (part == null || part.cosmeticOnly) continue; // koriste ei muuta ominaisuuksia
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
        [Header("Testaus (veda tahan muutama osa)")]
        public PartData[] testParts;

        [ContextMenu("Testaa: valitse testParts ja laske")]
        void TestSelect()
        {
            selected.Clear();
            foreach (var p in testParts) SelectPart(p);
        }
    }
}