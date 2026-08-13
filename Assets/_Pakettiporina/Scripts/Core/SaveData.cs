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

        // Asetukset (M5.2). Oletusarvot samat kuin AudioManagerin omat Inspector-oletukset,
        // joten vanhat tallennukset joissa naita kenttia ei viela ole eivat hiljenna peliä.
        public float musicVolume = 0.6f;
        public float sfxVolume = 1f;

        // Asetukset (M5.11, 13.8.2026): varahtely, ohjaustapa ja auton nopeustaso. Oletusarvot
        // valittu niin etta vanhat (naita kenttia edeltavat) tallennukset kayttaytyvat jarkevasti:
        // varahtely paalla, ohjaus "Molemmat" (sama oletus kuin CarInput.controlMode:lla), nopeus
        // "Keski" (ei muuta autoa nopeammaksi/hitaammaksi kuin ennen tata ominaisuutta).
        public bool vibrationOn = true;
        public int controlMode = 2;   // 0=Napit, 1=Kallistus, 2=Molemmat (ks. CarInput.ControlMode)
        public int carSpeedTier = 1;  // 0=Hidas, 1=Keski, 2=Nopea

        // Asetukset (M5.15, 14.8.2026): "Yotila" - TOISTAISEKSI VAIN tallennettu mieltymys,
        // ei viela oikeaa visuaalista tummaa teemaa (kayttajan pyynto: "pohja, mutta ei
        // vieda viela loppuun asti"). Oletus false, jotta vanhat tallennukset eivat muuta
        // mitaan. Kun teema toteutetaan oikeasti, tama arvo luetaan silloin.
        public bool darkMode = false;
    }
}
