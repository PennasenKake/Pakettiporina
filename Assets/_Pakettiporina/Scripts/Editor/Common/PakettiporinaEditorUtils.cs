#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;

namespace Pakettiporina.EditorTools
{
    // Jaetut apufunktiot Editor-tyokaluille. Nama olivat aiemmin kopioitu identtisina
    // useaan tiedostoon (PakettiporinaGarageSetup/MainMenuSetup/WashSetup: Find+ClearClicks,
    // PakettiporinaDataFix/DataTools: LoadParts/LoadPkgs) - koottu 10.8.2026 tanne YHTEEN
    // paikkaan koodikatselmuksen suosituksesta. Jos tarvitset jonkin naista, kutsu tata
    // luokkaa ala kopioi metodia uudelleen.
    public static class PakettiporinaEditorUtils
    {
        // Etsii minka tahansa (myos piilotetun) GameObjectin nimella koko scenesta.
        public static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name == name) return t.gameObject;
            return null;
        }

        // Poistaa napin kaikki Inspectorissa kytketyt OnClick-kuuntelijat (kaytetaan
        // ennen uudelleenkytkentaa, ettei sama toiminto tuplaannu ajettaessa korjaus
        // useaan kertaan).
        public static void ClearClicks(Button b)
        {
            for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(b.onClick, i);
        }

        // Lataa kaikki projektin PartData-assetit AssetDatabasesta. HUOM: skannaa koko
        // projektin - jos kutsut tata monta kertaa peräkkäin (esim. silmukassa), hae
        // tulos KERRAN muuttujaan äläkä kutsu tätä joka kierroksella.
        public static List<PartData> LoadParts() =>
            AssetDatabase.FindAssets("t:PartData")
                .Select(g => AssetDatabase.LoadAssetAtPath<PartData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();

        // Sama PackageData-assettelle.
        public static List<PackageData> LoadPkgs() =>
            AssetDatabase.FindAssets("t:PackageData")
                .Select(g => AssetDatabase.LoadAssetAtPath<PackageData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();
    }
}
#endif
