# Sisällön lisäys

Päivitetty 23.11.2026. Kuinka peliin lisätään uutta sisältöä datana.

## Uusi auton osa (PartData)

Assets → Create → Pakettiporina → Part Data. Täytä kategoria (Kori, Renkaat, Moottori, Jouset, Lisät, Maali), viisi ominaisuutta (voima, pito, keveys, kestävyys, kylmyys), ja tarvittaessa `secret`. **Avautuminen:** lisää osa `Scripts/Core/Progress.cs`:n osataulukkoon (nimi → tarvittavat ajotähdet; ei riviä = heti auki). `unlockPoints` on vain varasääntö (kolikkoraja), älä käytä sitä uusiin osiin. **Maali**-osan `color` on auton väri (näkyy Hallin Väri-välilehdellä ja ajossa). Tallenna `Data/Parts`. Aja lopuksi Pakettiporina → Korjaus/07 (kategoriat + tasapaino) ja Tarkistus/05 (analyysi).

Halli näyttää osat 3 sarakkeessa; yli 9 osaa samassa kategoriassa vaatisi vierityksen (Tarkistus/07 varoittaa).

## Uusi paketti (PackageData)

`displayName` (asset-nimi ilman ääkkösiä; kaunis nimi lisätään `GarageWizard.PrettyName`-listaan), `icon` (kuva kortille), `requiredPart`, `mass`, `rewardPoints` (kolikot), `meltTime`, **`trackScene`** (scenen nimi, esim. `Satama`; tyhjä = oletusrata). Tallenna `Data/Packages`. Jos rata on uusi, tee sille kuva (alla). Avautumisraja (ajotähdet) lisätään Progress.cs:n pakettitaulukkoon. Tarkista: Pakettiporina → Tarkistus/07 (Hallin kolmivaihe) ja Tarkistus/08 (Progress).

## Uusi tarra (StickerData)

Teema, harvinaisuus, avausehto, sprite. Tallenna `Data/Stickers`. Valmiit tarrat: Lisays/07.

## Uusi kaupunginosa / rata

1. Uusi scene (kopioi lähin: Satama/Puisto) ja lisää se Build Settingsiin.
2. Aja Lisays-työkalut (tähdet 01, elementit 02, koristeet 03, esteet 10) ja Korjaus/01.
3. Aseta pakettien `trackScene` osoittamaan sceneen.
4. **Ratakuva Halliin:** PNG nimellä `rata_<sceneNimiPienillä>.png` (esim. `rata_kyla.png`, ä→a, ö→o), suositus 480x360, kansioon `Art/UI/Rata/`. Aja sitten Korjaus/10 - Synkkaa Hallin kuvat (kopioi `Resources/WizardUI`:hin) ja lisää nimi `PakettiporinaWizardCheck.Sources()`-listaan sekä `GarageWizard.TrackInfo`-vihjeeseen. Ilman kuvaa käytetään `rata_perus`.

Nykyiset ratakuvat on piirretty skriptillä (tyylikkäät sarjakuvakuvat); ne voi korvata oikeilla ruutukaappauksilla (Pakettiporina → Esittely → Kuvakaappaus).

## Tuonti taulukoista

Ei vielä tehty.

## Lokalisointi

Ei vielä; tekstit ovat suomeksi koodissa. Hallin tekstit ovat `GarageWizard.cs`:ssä (`CatNames`, `TrackInfo`, `PrettyName`, Vilkun kuplat).

## Uusi rata (24.11.2026)
Radan lisäys koskettaa nyt: scene + Build Settings; `Progress.RaceScenes`, `Progress.tracks` ja `trackNames`; `PakettiporinaTracks.ALL` (+ tarvittaessa Decor/Obstacles-presetit); ratakuva `Resources/WizardUI/rata_<nimi>.png` (Korjaus/10). Aja lopuksi Tarkistus/08, joka kertoo puuttuvat kohdat. Kylä on merkitty suunnitelluksi (`PlannedTracks`): se tarvitsee oman ratamallin ja paljon FBX-elementtejä, joten sen scene pidetään pois Build Settingsista kunnes valmis.
