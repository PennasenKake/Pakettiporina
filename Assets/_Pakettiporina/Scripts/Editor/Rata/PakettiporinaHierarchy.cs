#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Pakettiporina.EditorTools
{
    // Yhteinen apuluokka Hierarkian siisteyteen (lisatty 7.8.2026 pyynnosta). Kaikki
    // radan generoimat objektit kootaan "Map"-objektin alle omiin lokeroihinsa:
    //   Map/Objektit   - teemakoristeet (PakettiporinaDecor.cs)
    //   Map/Tehosteet  - Boostit (PakettiporinaElements.cs)
    //   Map/Esteet     - Latakot + kartiot (PakettiporinaElements.cs)
    // HUOM: StartPoint/Finish/CarBuilder/GameManager/RaceSetup/RaceManager/Stars yms.
    // jarjestelma- ja pelilogiikkaobjektit EIVAT mene tanne - ne pysyvat scenen
    // juuressa kuten ennenkin, tama koskee vain visuaalisia rata-elementteja.
    public static class PakettiporinaHierarchy
    {
        public const string MAP = "Map";
        public const string OBJEKTIT = "Objektit";
        public const string TEHOSTEET = "Tehosteet";
        public const string ESTEET = "Esteet";

        static Transform FindRootChild(string name)
        {
            foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.name == name) return go.transform;
            return null;
        }

        public static Transform GetFolder(string subName)
        {
            var map = FindRootChild(MAP);
            if (map == null)
            {
                var mapGo = new GameObject(MAP);
                Undo.RegisterCreatedObjectUndo(mapGo, "Luo Map");
                map = mapGo.transform;
            }

            Transform sub = null;
            foreach (Transform child in map)
                if (child.name == subName) { sub = child; break; }

            if (sub == null)
            {
                var subGo = new GameObject(subName);
                Undo.RegisterCreatedObjectUndo(subGo, "Luo " + subName);
                subGo.transform.SetParent(map, false);
                sub = subGo.transform;
            }
            return sub;
        }
    }
}
#endif
