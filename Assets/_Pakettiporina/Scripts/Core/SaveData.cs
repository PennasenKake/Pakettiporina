using System.Collections.Generic;

namespace Pakettiporina
{
    // Tallennettava data JSONina (Application.persistentDataPath). Ei ScriptableObject,
    // koska tama on pelkkaa dataa levylla, ei projektin asset.
    //
    // HUOM: PartData/PackageData ovat ScriptableObject-asseteja, joita ei voi tallentaa
    // suoraan viittauksena JSONiin - siksi tallennetaan NIMI (part.name), ja ladataan
    // takaisin etsimalla se GarageScreenin allParts/allPackages-listasta (jo olemassa
    // scenessa, toimii oikeassa buildissa - EI kayteta AssetDatabasea/Resources.Loadia).
    [System.Serializable]
    public class SaveData
    {
        public int saveVersion = 1;
        public int points;
        public List<string> selectedPartNames = new List<string>();
        public string selectedPackageName = "";
        public List<string> unlockedStickerNames = new List<string>();
    }
}
