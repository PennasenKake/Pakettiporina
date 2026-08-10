#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using static Pakettiporina.EditorTools.PakettiporinaEditorUtils;

namespace Pakettiporina.EditorTools
{
    // Analysoi osat ja paketit, tarkistaa tasapainon - puhtaasti lukeva tarkistustyokalu,
    // ei muokkaa mitaan. Pakettien GENEROINTI on omassa tiedostossaan,
    // Editor/Lisays/PakettiporinaDataGenerointi.cs.
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Tarkistus/
    public static class PakettiporinaDataAnalyysi
    {
        [MenuItem("Pakettiporina/Tarkistus/05 - Analysoi data")]
        public static void Analyze()
        {
            var parts = LoadParts();
            var pkgs = LoadPkgs();
            var s = new StringBuilder();
            s.AppendLine("=== DATA-ANALYYSI ===");
            s.AppendLine($"Osia: {parts.Count} | Paketteja: {pkgs.Count}\n");

            // osat kategorioittain
            s.AppendLine("--- OSAT KATEGORIOITTAIN ---");
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                var inCat = parts.Where(p => p.category == c).ToList();
                s.AppendLine($"\n[{c}] ({inCat.Count} kpl)");
                if (inCat.Count == 0) { s.AppendLine("   (tyhja — kategoriassa ei ole osia!)"); continue; }
                foreach (var p in inCat)
                {
                    string effects = Effects(p);
                    string flags = "";
                    if (string.IsNullOrWhiteSpace(p.displayName)) flags += " [EI NIMEA]";
                    if (!p.cosmeticOnly && effects == "ei vaikutusta") flags += " [KAIKKI 0]";
                    if (p.cosmeticOnly) flags += " (kosmeettinen)";
                    s.AppendLine($"   {p.displayName,-16} -> {effects}{flags}");
                }
            }

            // paketit
            s.AppendLine("\n--- PAKETIT ---");
            foreach (var pk in pkgs.OrderBy(p => p.rewardPoints))
            {
                string req = pk.requiredPart != null ? pk.requiredPart.displayName : "(ei vaatimusta)";
                bool bad = pk.requiredPart != null && !parts.Contains(pk.requiredPart);
                string flags = "";
                if (bad) flags += " [VAADITTU OSA PUUTTUU -> MAHDOTON!]";
                if (pk.rewardPoints == 0) flags += " [PALKKIO 0]";
                if (string.IsNullOrWhiteSpace(pk.displayName)) flags += " [EI NIMEA]";
                s.AppendLine($"   {pk.displayName,-16} massa={pk.mass,-4} palkkio={pk.rewardPoints,-4} vaatii={req}{flags}");
            }

            // tasapaino
            s.AppendLine("\n--- TASAPAINO ---");
            foreach (PartCategory c in System.Enum.GetValues(typeof(PartCategory)))
            {
                var inCat = parts.Where(p => p.category == c && !p.cosmeticOnly).ToList();
                if (inCat.Count == 0) continue;
                s.AppendLine($"   {c}: voima {Range(inCat, p => p.voima)}  pito {Range(inCat, p => p.pito)}  keveys {Range(inCat, p => p.keveys)}  kesto {Range(inCat, p => p.kestavyys)}  kylmyys {Range(inCat, p => p.kylmyys)}");
            }

            // kattavuus: onko jokaiselle vaaditulle osalle paketti?
            s.AppendLine("\n--- KATTAVUUS ---");
            var required = pkgs.Where(p => p.requiredPart != null).Select(p => p.requiredPart).Distinct().ToList();
            s.AppendLine($"   Paketit vaativat {required.Count} eri osaa.");
            var neverRequired = parts.Where(p => !p.cosmeticOnly && !required.Contains(p)).ToList();
            if (neverRequired.Count > 0)
                s.AppendLine($"   Osia joita mikaan paketti ei vaadi: {string.Join(", ", neverRequired.Select(p => p.displayName))}");

            Debug.Log(s.ToString());
        }

        static string Effects(PartData p)
        {
            var parts = new List<string>();
            if (p.voima != 0) parts.Add($"voima {p.voima:+0;-0}");
            if (p.pito != 0) parts.Add($"pito {p.pito:+0;-0}");
            if (p.keveys != 0) parts.Add($"keveys {p.keveys:+0;-0}");
            if (p.kestavyys != 0) parts.Add($"kesto {p.kestavyys:+0;-0}");
            if (p.kylmyys != 0) parts.Add($"kylmyys {p.kylmyys:+0;-0}");
            return parts.Count == 0 ? "ei vaikutusta" : string.Join(", ", parts);
        }
        static string Range(List<PartData> list, System.Func<PartData, int> sel)
        {
            var vals = list.Select(sel).ToList();
            return $"{vals.Min()}..{vals.Max()}";
        }
    }
}
#endif
