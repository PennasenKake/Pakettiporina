# Koodikatselmus — koko projekti (10.8.2026)

Käytiin läpi kaikki 44 C#-tiedostoa (n. 7270 riviä): Core, Data, Gameplay, Vehicle, UI,
Builder sekä kaikki Editor-työkalut (Rata/Sisalto/Tarkistus). Yleisarvio: koodikanta on
poikkeuksellisen siistissä kunnossa harrastelijaprojektiksi — johdonmukainen nimeäminen,
kattavat suomenkieliset kommentit, null-tarkistukset lähes kaikkialla, eikä yhtään
peliä kaatavaa tai pelilogiikkaa rikkovaa bugia löytynyt. Suurin osa alla olevista
huomioista on siivousta ja pieniä tehostuksia, ei hätäkorjauksia.

## 1. Ajonaikainen koodi (Core/Gameplay/Vehicle/UI/Builder)

**Vahvuudet.** GameManager/SaveManager/AudioManager noudattavat kaikki samaa, hyvää
singleton-mallia (`DontDestroyOnLoad` + duplikaattitarkistus Awake:ssa). Tapahtumavayla
(`GameEvents`) pitää järjestelmät irrallaan toisistaan. `SaveManager` lataa datan
`Start()`:ssa eikä `Awake()`:ssa nimenomaan Script Execution Orderin takia — tämä on
kommentoitu auki, hyvä käytäntö. `CarPainter` käyttää `MaterialPropertyBlock`ia materiaalin
instansioinnin sijaan, mikä on juuri oikea tapa värjätä paljon toistuvia objekteja ilman
material-vuotoja. `ArcadeCarController`/`CarInput` ovat siistiä, allokaatiotonta
`FixedUpdate`/`Update`-koodia — ei GC-painetta ajon aikana.

**Löydetyt kohdat:**

- **`CarBuilder.cs` sisältää n. 60 riviä kommentoitua, vanhaa versiota koko luokasta**
  (`/* ... */`-lohko tiedoston alussa) ennen nykyistä, aktiivista versiota. Tämä on
  puhdasta kuollutta koodia — turvallista poistaa kokonaan, git-historia säilyttää
  vanhan version joka tapauksessa.

- **Singleton-kaavan epäjohdonmukaisuus.** `RaceManager`, `TrackManager` ja `WashScreen`
  asettavat `Instance = this` Awake:ssa ilman duplikaattitarkistusta (toisin kuin
  GameManager/SaveManager/AudioManager). Käytännössä nämä ovat scene-kohtaisia eikä
  ongelmia ole havaittu, mutta yhtenäisyyden vuoksi sama `if (Instance != null &&
  Instance != this) Destroy(...)`-suoja kannattaisi lisätä kaikkialle — suojaa esim.
  tilanteelta jossa scene ladataan vahingossa additiivisesti kahteen kertaan.

- **`RaceHUD.Update()`** kirjoittaa `timeText.text`-merkkijonon joka ruutu
  (`race.Elapsed.ToString("F1")`), mikä allokoi roskaa jatkuvasti ajon aikana. Ei
  vaikuta pelattavuuteen mobiilissakaan käytännössä, mutta halvalla tehostettavissa:
  päivitä esim. 0.1 s välein (`if (Time.frameCount % 6 == 0)` tms.) tai pyöristä arvo
  ja vertaa edelliseen ennen tekstin asettamista.

- **`VilkkuController.BuildLine()`** päättelee pelitilan tekstisisällön perusteella
  (`fitText.text.Contains("Sopii")` / `"Tarvitsee"`). Toimii, mutta on hauras: jos joku
  myöhemmin muuttaa `FitChecker`-viestien sanamuotoa, Vilkun logiikka hajoaa hiljaa.
  Parempi olisi lukea suoraan `FitChecker.Result.fits`-totuusarvo (esim. tallentamalla
  se `GarageScreen`iin ja välittämällä `VilkkuController`ille), ei UI-tekstiä.

- **Tiedostokoodaus.** Osa tiedostoista (mm. `CameraFollow.cs`, `CarInput.cs`,
  `TouchButton.cs`, `VilkkuController.cs`, `RaceSetup.cs`, `FitChecker.cs`,
  `Pickup.cs`) on tallennettu Windows-1252/ISO-8859-koodauksella UTF-8:n sijaan.
  Tämä näkyy mm. em-tavuviivojen (–) vääristymisenä kommenteissa työkaluissa jotka
  odottavat UTF-8:aa (kuten tämä katselmus). Ei vaikuta pelin toimintaan Unity/Windows-
  ympäristössä, mutta kannattaa normalisoida kaikki `.cs`-tiedostot UTF-8:ksi
  (esim. Rider/VS Code: "Save with encoding") välttääksesi ongelmia versionhallinnassa
  tai jos projektia joskus muokataan muulla käyttöjärjestelmällä.

- **Pieni tyylihuomio:** `ScreenManager.cs` ja osa `SettingsManager.cs`:stä ovat
  tyhjiä Unity-mallipohjia (pelkkä `Start()`/`Update()` ilman sisältöä).
  `SettingsManager` on jo tiedossa keskeneräiseksi (Settings-valikon sisältö on
  suunnitteilla), mutta `ScreenManager.cs` vaikuttaa käyttämättömältä placeholderilta —
  kannattaa joko täyttää tarkoituksella tai poistaa, jos mikään ei viittaa siihen.

## 2. Editor-työkalut (Rata/Sisalto/Tarkistus)

**Vahvuus, syytä nostaa esiin nimenomaan hyvänä esimerkkinä:** `PakettiporinaTracks.cs`
on jo refaktoroitu "yhdeksi yhteiseksi ratatietolähteeksi" — `RoadX()`-kaava ja jokaisen
radan `bends`/`roadBaseX`/`startZ`/`finishZ` on määritelty vain YHDESSÄ paikassa ja kaikki
muut työkalut (`PakettiporinaElements`, `RoadProfile`, `SceneDump`, `Stars`,
`ExportPlacement`, `Setup`) hakevat sen sieltä. Tiedoston oma kommentti kertoo tämän
syntyneen aiemman bugin korjauksesta (duplikoitu ratadata aiheutti virheellisen
spawn-sijainnin). Tämä on juuri oikea rakenne — ei toimenpiteitä.

**Löydetyt kohdat:**

- **`Find(string name)` ja `ClearClicks(Button b)` on kopioitu identtisinä kolmeen
  tiedostoon**: `PakettiporinaGarageSetup.cs`, `PakettiporinaMainMenuSetup.cs` ja
  `PakettiporinaWashSetup.cs` (rivi riviltä sama koodi kaikissa kolmessa). Selkein
  yksittäinen tehostusmahdollisuus koko katselmuksessa: siirrä nämä kahteen metodiin
  yhteiseen staattiseen apuluokkaan (esim. uusi `Editor/PakettiporinaEditorUtils.cs`),
  ja korvaa kolme kopiota kutsuilla sinne. Vähentää ylläpitopintaa kolmasosaan, eikä
  muuta mitään toiminnallisesti.

- **`LoadParts()`/`LoadPkgs()` on toteutettu erikseen sekä `PakettiporinaDataFix.cs`:ssä
  että `PakettiporinaDataTools.cs`:ssä** (sama `AssetDatabase.FindAssets("t:PartData")`
  + lataus-logiikka). Sama korjaus kuin yllä: yksi yhteinen apumetodi.

- **`PakettiporinaDataFix.FindPart(name)` kutsuu `LoadParts()`:ia joka kerta**, eli
  jokainen haku skannaa koko `Data/Parts`-kansion `AssetDatabase`:sta uudelleen ja lataa
  kaikki osat levyltä. `Rebalance()` kutsuu tätä n. 14 kertaa peräkkäin (kerran per
  taulukon rivi) — eli koko osakansio luetaan levyltä 14 kertaa yhden ajon aikana.
  Tämä on Editor-työkalu (ei vaikuta pelin suorituskykyyn buildissa), mutta silti
  turhaa: hae `var all = LoadParts()` kerran funktion alussa ja tee haku
  `all.FirstOrDefault(...)`-listasta muistista.

- **`PakettiporinaMainMenuMap.AddMap()` luo joka ajokerralla uuden `Sprite.Create(...)`
  -objektin** eikä koskaan vapauta edellistä. Koska tätä työkalua ajetaan usein
  toistuvasti kehityksen aikana (kuten tämän projektin historiasta näkyy), jää
  Editor-sessioon roskaa kunnes domain reload tyhjentää sen. Ei riski buildissa, mutta
  jos sprite halutaan "oikeaksi" assetiksi tallennuksen sijaan ajonaikaiseksi, voisi
  harkita `AssetDatabase.LoadAssetAtPath<Sprite>`-käyttöä jos kuva on jo tuotu
  Sprite-tyyppisenä (ks. tiedoston oma kommentti — tämä on jo tiedostettu, ei uusi
  löytö, mutta nostettu tähän kokonaiskuvan vuoksi).

## 3. Priorisoitu suositus

1. **Poista `CarBuilder.cs`:n kommentoitu vanha luokka** (60 riviä kuollutta koodia) — 
   nopein, riskitön siivous.
2. **Yhdistä `Find`/`ClearClicks`/`LoadParts`/`LoadPkgs`-apumetodit yhteen paikkaan** —
   suurin ylläpidettävyyshyöty suhteessa vaivaan, ei riskiä koska logiikka on jo
   identtinen joka kopiossa.
3. **`RaceHUD`:n aikapäivitys harvemmaksi** — pieni, ei-kriittinen suorituskykyparannus.
4. **`VilkkuController` lukemaan tila suoraan `FitChecker.Result`ista tekstin sijaan** —
   ehkäisee tulevan hiljaisen bugin, ei kiireellinen.
5. **Tiedostokoodaukset UTF-8:ksi** — kosmeettinen, tee kun sopii.
6. **`ScreenManager.cs`:n tarkoitus selvitettävä** (poista tai täytä).

Mitään näistä ei tarvitse tehdä ennen seuraavaa julkaisua — pelilogiikka, tallennus ja
autofysiikka ovat kunnossa. Kerro jos haluat, että teen jonkin yllä olevista suoraan.
