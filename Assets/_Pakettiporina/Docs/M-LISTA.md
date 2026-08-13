# Virstanpylväät ja niiden sisältö (M-viittaukset)

Tämä dokumentti selittää koodin kommenteissa ja tarkistuslistassa käytetyt
M-viittaukset (M1, M2, M3…). Jokaisella alivaiheella on tila, sisältö ja
keskeiset tiedostot. Juuri kaikille poluille: `Assets/_Pakettiporina/`

**Tilat:** ✅ valmis · 🔨 kesken · ⬜ suunniteltu

**Päivitetty:** tämä versio käytiin läpi suoraan koodia ja scenejä vasten
(ei pelkkää muistinvaraista tilaa) — tilat, tiedostonimet ja kuvaukset on
korjattu vastaamaan sitä mitä projektissa oikeasti on tällä hetkellä.
Muutama kohta on toteutettu eri tavalla/eri nimillä kuin alkuperäinen
suunnitelma povasi — nämä on merkitty erikseen.

**Päivitys 11.8.2026:** iso kierros — täysi äänimaailma (16 äänitiedostoa),
5. rata (Maatila), Asetukset-paneeli kokonaan valmiiksi rakennettuna
(äänenvoimakkuudet, äänet päällä/pois -liukukytkin) sekä uusi Ohjeet & palaute
-paneeli. Tilat ja Editor-työkalutaulukko korjattu vastaamaan tätä. **Tärkeä
huomio:** koodipuoli on valmis, mutta scene-tiedostoihin (`.unity`) muutokset
tulevat vasta kun asiaankuuluvat Editor-työkalut ajetaan Unityssä — ks. kunkin
kohdan tarkempi tila alla ja `Docs/Ohjeet_Aani_Asetukset_Maatila_2026-08-11.md`.

---

## M1 — Ajettava auto ✅
Tavoite: auto liikkuu maalla, kamera seuraa. Hauskuuden perusta.

- **M1.1 Projekti & asetukset** ✅ — Unity 2022.3 (3D URP), Active Input Handling = Both, build-perusteet (IL2CPP/ARM64).
- **M1.2 Hakemistorakenne & versionhallinta** ✅ — `_Pakettiporina`-kansiot, `.gitignore`, `Pakettiporina.asmdef`, Git-repo (GitHub: PennasenKake/Pakettiporina).
- **M1.3 Syöte** ✅ — `Scripts/Vehicle/CarInput.cs` (näppäimistö + kosketus).
- **M1.4 Autofysiikka** ✅ — `Scripts/Vehicle/ArcadeCarController.cs` (arcade: kiihdytys, kääntö, pito, huippunopeus, + boost/lätäkkö-tilapäiskerroin-kerrostus lisätty myöhemmin, ks. M2 laajennus).
- **M1.5 Kamera** ✅ — `Scripts/Core/CameraFollow.cs` (pehmeä seuraava kamera).
- **M1.6 Maa & auto-prefab** ✅ — `Prefabs/Car.prefab`. HUOM: tämä on tällä hetkellä yksinkertainen laatikkomalli (ei se yksityiskohtainen "silmät/renkaat"-auto jonka materiaalit ovat `Art/Models/Materials/Auto_*` — ne kuuluvat käyttämättömälle `Art/Models/Car1.fbx`-mallille, joka ei ole koskaan liitetty prefabiin). Korin materiaali korjattiin osoittamaan oikeaan `Auto_kori.mat`:iin (oli aiemmin rikkinäinen viittaus, minkä vuoksi hallin väriä ei koskaan piirtynyt autoon).

## M2 — Rata + maali (yksi pelattava keikka) ✅
Tavoite: keikka lähdöstä maaliin — tähdet, maali, restart, valikko, tauko.

- **M2.1 GameManager** ✅ — `Scripts/Core/GameManager.cs` (singleton: pelin vaihe + pisteet, `OnPointsChanged`-tapahtuma, DontDestroyOnLoad).
- **M2.2 GameEvents** ✅ — tapahtumaväylä (osittain sulautunut suoraan GameManagerin/RaceManagerin omiin C#-eventteihin, ei erillistä staattista luokkaa kaikkialla).
- **M2.3 RaceManager** ✅ — `Scripts/Gameplay/RaceManager.cs` (keikan kulku, kuluva aika, tähtilaskenta).
- **M2.4 FinishTrigger** ✅ — `Scripts/Gameplay/FinishTrigger.cs`.
- **M2.5 Pickup** ✅ — `Scripts/Gameplay/Pickup.cs` (tähti).
- **M2.6 HUD & Canvas** ✅ — `Scripts/UI/RaceHUD.cs`.
- **M2.7 StartPoint & minimirata** ✅.
- **M2.8 Aloitusnäkymä** ✅ — `Scripts/UI/MainMenu.cs`.
- **M2.9 Kosketusohjaus** ✅ — `Scripts/UI/TouchButton.cs`.
- **M2.10 Viimeistely** ✅ — `Scripts/UI/PauseMenu.cs`.

### M2 laajennus — Lisäradat, pelielementit ja sisältö 🔨
*(tämä on git-committisi "M5 LISÄÄ SISÄLTÖÄ JA UUDET MAPIT" — se ei oikeastaan
ollut alkuperäistä M5:tä, vaan laajensi M2:ta kolmella uudella radalla ja
lisäsi M1:n pelimekaniikkaan uusia elementtejä. Kirjattu tähän jotta M-lista
täsmää siihen mitä oikeasti tapahtui.)*

- **M2.11 Boost / Lätäkkö / Kartio -pelielementit** ✅ — `Scripts/Gameplay/Boost.cs`, `Puddle.cs`, `Cone.cs` + `ArcadeCarController`:n tilapäiskerroin-kerrostus (`boostMultiplier`/`gripPenaltyMultiplier`). Editor-työkalu `PakettiporinaElements.cs` (`Lisäys/02`) sijoittaa nämä radan `bends`-kaavan mukaan automaattisesti.
- **M2.12 Kolme uutta rataa (Puisto, Satama, Huvipuisto)** ✅ — omat scenet (`Scenes/Puisto.unity`, `Satama.unity`, `Huvipuisto.unity`), maastot generoitu Blenderillä (`pakettiporina_ratageneraattori.py` + tämän session laajennukset), noin 30 s ajomatka per rata.
  - ⚠️ **Kesken vielä**: kaikkien pitää olla lisättynä ja päällä **File → Build Settings → Scenes In Build**, muuten `SceneManager.LoadScene()` ei löydä niitä oikeassa buildissa (toimii silti Editorin Play-tilassa, mikä on hämännyt aiemmin). Tarkista `Korjaus/02 - KORJAA halli` -lokista onko vielä varoituksia.
- **M2.16 Neljäs uusi rata — Maatila (5. rata kokonaisuudessaan)** ✅ *(uusi, lisätty 10.–11.8.)* — `Scenes/Maatila.unity`, rakennettu Puiston `TrackDef`-mitoilla mutta oma rauhallinen/helppo teema (ei kartioita eikä lätäköitä — pelin ensimmäinen "hengähdys"-rata, ks. `Docs/Rata_Maatila_speksi.md`). Kytketty `Kukkaruukku`-pakettiin (`PackageData.trackScene: Maatila`). `Korjaus/01 - KORJAA peliscene` on ajettu onnistuneesti tälle radalle (CarEngineAudio, CarPainter, GroundCollider, StartPoint kunnossa).
  - ⚠️ **Kesken vielä**: `Maatila.unity` puuttuu todennäköisesti yhä **File → Build Settings → Scenes In Build** -listalta — tätä ei voi tarkistaa/korjata Editor-työkaluilla (ProjectSettings ei ole assistentin ulottuvilla), pelkkä manuaalinen Unity-askel.
- **M2.13 Teemakoristeet (FBX)** ✅ — `PakettiporinaDecor.cs` (`Lisäys/03`). Puisto: puita, penkki, lyhtypylväs, keinu, kaarisilta + lampi. Satama: kokonaan uudelleenrakennettu rantaviiva (vesi seuraa tien kaarretta koko matkan), laiva, laituri, majakka, poijut, nosturi, varasto, kontit. Huvipuisto: maailmanpyörä, teltta, karuselli, myyntikoju, ilmapallokaari, lippunauha, pomppulinna.
- **M2.14 Tähdet kaikille radoille** ✅ — `PakettiporinaStars.cs` (`Lisäys/01`) yleistetty toimimaan kaikilla radoilla (oli aiemmin vain Rata 1:lle kovakoodattu). Nyt kaikki 6 ratascenea (Game, Game2, Puisto, Satama, Huvipuisto, Maatila) tuettu — 11.8. korjattiin myös tähtien luonnin `[RequireComponent(Collider)]`-kaatumisbugi käyttämällä `Star.prefab`:in instantiointia bare-GameObjectin sijaan.
- **M2.15 Radan valinta paketin kautta (`trackScene`)** ✅ — `PackageData.trackScene` + `GarageScreen.OnDrive()` päättää minkä radan lataa valitun paketin mukaan. Dormantti `TrackManager`/`TrackData`-järjestelmä (skenen-sisäinen versio) jätettiin käyttämättä tietoisesti, koska se ei vastannut toteutunutta yksi-scene-per-rata-arkkitehtuuria.

## M3 — Rakentelu + paketit ✅
Tavoite: opettava ydin — osat, ominaisuusmittarit, paketin ja auton yhteensovitus.

- **M3.1 Data** ✅ — `Scripts/Data/PartData.cs`, `Scripts/Data/PackageData.cs` (32 osaa, 10 pakettia `Data/Parts` ja `Data/Packages` -kansioissa).
- **M3.2 Laskenta** ✅ — `Scripts/Data/CarStats.cs`, `Scripts/Builder/CarBuilder.cs`.
- **M3.3 Sopivuus** ✅ — `Scripts/Builder/FitChecker.cs`.
- **M3.4 Halli-UI** ✅ *(oli merkitty 🔨, on nyt valmis)* — `Scripts/UI/GarageScreen.cs`: osakategoriat, osavalinta, mittaripalkit, sopivuusmerkki, pakettilista, "Aja keikka", osien ja pakettien pistelukko (`unlockPoints`/`IsLocked`/`IsPackageLocked`).
- **M3.5 Kytkentä ajoon** ✅ *(oli merkitty ⬜, on itse asiassa valmis — koodissa jopa merkitty "M3.5b")* — `Scripts/Gameplay/RaceSetup.cs`: voima→kiihtyvyys, pito→pito, keveys→huippunopeus+kääntyminen, paketin massa vaimentaa suorituskykyä. Kaikki arvot rajattu turvalliselle välille lapsipeliä ajatellen. Palkkiopisteet: `RaceManager` → `GameManager.AddPoints`.
- **M3.6 Salaiset osat ("paras mahdollinen auto")** ✅ *(uusi, ei alkuperäisessä listassa)* — `PartData.secret`-lippu + `GarageScreen.BuildLookups()` piilottaa osan kokonaan osaselaimesta kunnes pisteraja täyttyy (ei vain "lukossa"-tekstiä — osa puuttuu listasta kokonaan ja ilmestyy yllätyksenä). 6 kultaista osaa (`PakettiporinaSecretParts.cs`, `Lisäys/08`), yhdessä nostavat kaikki 5 mittaria tasan sataan.

## M4 — Huoltoasema (pesu) ✅
Tavoite: maalin jälkeen pesu saippuakuplilla.

*Toteutui eri luokkanimillä kuin alkuperäinen suunnitelma ("CarCareController"
jne. eivät koskaan syntyneet), mutta tavoite on saavutettu ja pesu toimii.*

- **M4.1–M4.2 Pesun kulku** ✅ — `Scripts/Gameplay/WashScreen.cs` (ei erillistä `CarCareController`/`RepairStep`-vaihejakoa — pesu on oma suora scene `Wash.unity` maalin ja hallin välissä).
- **M4.3 Korjausvaihe** ⬜ — kolhujen erillistä korjaus-napautusta ei ole toteutettu; pesu keskittyy vain puhdistukseen.
- **M4.4 Kuplien poksautus + puhtausmittari** ✅ — `Scripts/Gameplay/Bubble.cs` + `BubbleArea`, `ProgressText` ("Poksautit X/8 kuplaa!").
- **M4.5 Pisteytys** ✅ — palkkiopisteet tulevat radalta (`RaceManager`/`GameManager`), ei erillistä `ScoreManager`-luokkaa pesussa.
- **M4.6 UI + Tauko** ✅ — `DonePanel`, `PauseMenu` pesuscenessä. *(Tässä sessiossa korjattu bugi: `DonePanel` ja `PausePanel` olivat tallentuneet scenessä oletuksena PÄÄLLÄ, jolloin "Kiiltävä auto!" -valmisteksti näkyi heti pesun alussa. `PakettiporinaWashSetup.cs` piilottaa nyt kummatkin automaattisesti KORJAA-tilassa — aja `Korjaus/03 - KORJAA pesu`.)*
- **M4.7 CarPainter — hallin väri näkyy autossa** ✅ *(uusi rivi, ei alkuperäisessä listassa mutta liittyy suoraan M4:n "maalin jälkeen"-ajatukseen)* — `Scripts/Vehicle/CarPainter.cs`. Tässä sessiossa löytyi ja korjattiin juurisyy miksi väri ei koskaan näkynyt: `Car.prefab`:n korin materiaali osoitti rikkinäiseen guidiin.

## M5 — Silmukka + meta 🔨
Tavoite: tallennus, eteneminen, tarrat, näkymäkierto, lapsiturva.

- **M5.1 SaveManager + SaveData** ✅ — `Scripts/Core/SaveManager.cs`: JSON-tallennus (`Application.persistentDataPath`), pisteet + valittu paketti + valitut osat. Singleton, lataa `Start()`:issa (tietoisesti ei `Awake()`:ssa, ks. tiedoston oma kommentti Script Execution Orderista).
- **M5.2 SettingsManager** ✅ *(11.8. — rakennettu kokonaan tyhjästä tyngästä valmiiksi)* — `Scripts/Core/SettingsManager.cs`: musiikin ja äänitehosteiden voimakkuus-liukusäätimet, yksi "Äänet"-liukukytkin (ks. M5.10a) joka mykistää/palauttaa molemmat äänet muistaen edelliset tasot, avaa myös Ohjeet & palaute -paneelin (M5.10). UI rakennetaan/päivitetään `Lisays/09`-työkalulla (`PakettiporinaSettingsSetup.cs`) — samaa koralliväripalettia ja kuvakekokoa kuin muu peli (StickerPanel), skaalattu isommaksi 11.8. luettavuuden vuoksi.
  - ⚠️ Itse `MainMenu.unity`-scenessä on tätä kirjoitettaessa yhä *vanhalla, ennen 11.8. tehtyä väriä* oleva paneeli levyllä — `Lisays/09` pitää ajaa Unityssä kerran jotta muutokset näkyvät pelissä.
- **M5.3 AudioManager** ✅ *(koodi/sisältö), ⚠️ kesken (scene-kytkentä)* — `Scripts/Core/AudioManager.cs`: 16 AudioClip-kenttää (12 tehostetta + 4 musiikkia), automaattinen musiikinvaihto `SceneManager.sceneLoaded`-tapahtumasta radan/hallin/pesun/valikon mukaan. Kaikki 16 äänitiedostoa on generoitu proseduraalisesti (`Audio/SFX/`, `Audio/Music/`) ja koodi lataa/kytkee ne `Korjaus/08 - KORJAA aanet` -työkalulla. **Tarkistettu suoraan scene-tiedostoista 11.8.: `AudioManager`-komponentti on olemassa jokaisessa scenessä (paitsi Game2, ks. alla), mutta kaikki 16 klippikenttää ovat yhä tyhjiä levyllä olevassa versiossa** — `Korjaus/08` keskeytyi aiemmin tallentamattomiin muutoksiin eikä sitä ole vielä ajettu onnistuneesti loppuun asti. Tämä on juuri nyt suurin yksittäinen ero "koodi on valmis" ja "pelaaja kuulee sen" välillä.
- **M5.4 SavedCarsManager** ⬜ *(ei erillisenä luokkana)* — auton muistaminen keikkojen välillä toteutui `SaveManager`:in kautta suoraan, joten tavoite on käytännössä saavutettu ilman erillistä luokkaa. Ei tallia/useaa suosikkiautoa.
- **M5.5 ProgressionManager + UnlockManager** ✅ *(ei erillisenä luokkana, mutta toiminnallisuus on olemassa)* — `PartData.unlockPoints`/`secret`, `PackageData.unlockPoints`, logiikka suoraan `GarageScreen`:issa (`IsLocked`, `IsPackageLocked`). Radat avautuvat paketin kautta (ks. M2.15).
- **M5.6 AchievementTracker** ✅ *(sulautunut sticker-järjestelmään, ei erillistä trackeria)* — tarrojen avautuminen = `StickerData.unlockPoints` vs. `GameManager.Points`, tarkistetaan `StickerPanel.cs`:ssä.
- **M5.7 StickerData + DistrictData** ✅ *(korjattu 11.8. — oli 🔨)* — `StickerData` valmis, **36 tarraa**, ja **StickerGrid-UI:ssa on nyt kaikki 36 nimettyä solua** (tarkistettu suoraan `MainMenu.unity`:sta: `StickerPanel.slots`-taulukossa 36 alkiota). Aiempi 24/36-vaje on siis korjattu — ei enää avointa asiaa tässä kohdassa. `DistrictData` ei ole toteutunut — kaupunginosia ei ole, radat toimivat sen sijaan alueina.
- **M5.8 Näkymät & ScreenManager** ⬜ *(arkkitehtuuri toteutui kokonaan eri tavalla)* — `ScreenManager.cs` on tyhjä käyttämätön tynkä (tarkistettu suoraan tiedostosta 11.8., tilanne ei ole muuttunut), eikä HomeScreen/MapScreen/StickerBookScreen/BottomNav-mallia ole rakennettu. Sen sijaan peli navigoi suoraan `SceneManager.LoadScene()`-kutsuilla erillisten scenejen välillä (MainMenu → Garage → Game/Puisto/Satama/Huvipuisto/Maatila → Wash → Garage). Tämä toimii, mutta on syytä tietoisesti hyväksyä arkkitehtuurivalintana eikä keskeneräisyytenä — jos yhden-scenen navigointimalli halutaan joskus, se on iso myöhempi työ.
- **M5.9 ParentalGate + Boot-scene** ⬜ — ei toteutettu. Tärkeä ennen Google Play -julkaisua (ks. M7). ⚠️ *Huomio 11.8.:* uusi Ohjeet & palaute -paneelin "Lähetä palautetta" -nappi (M5.10b) avaa laitteen sähköpostisovelluksen ilman minkäänlaista porttia — ei ongelma nyt (ei osta/ei ulkoista maksullista linkkiä), mutta jos paneeliin lisätään joskus osto- tai mainoslinkkejä, ne pitää laittaa tämän Parental Gaten taakse heti kun se rakennetaan.
- **M5.10 Ohjeet & palaute -paneeli (InfoManager)** ✅ *(uusi, ei alkuperäisessä listassa — lisätty 11.8. käyttäjän pyynnöstä)* — `Scripts/Core/InfoManager.cs` + `Scripts/Editor/Lisays/PakettiporinaSettingsSetup.cs`:n `BuildInfoPanel()`. Oma paneeli (avataan Asetukset-paneelin sisältä), sisältää pelin pikaohjeet, kuvauksen pelistä ja "Lähetä palautetta" -napin (`mailto:`-linkki, oletusosoite `InfoManager.feedbackEmail`-kentässä — **placeholder, vaihdettava oikeaksi**).
  - **M5.10a Äänet päällä/pois -liukukytkin** ✅ — `Scripts/UI/ToggleSwitch.cs`. Alun perin kaksi erillistä radiopainiketta, korvattu 11.8. yhdellä iOS-tyylisellä liukuvalla kytkimellä käyttäjän kuvaesimerkin mukaan (pilleri-rata + liukuva pyöreä nuppi, väri vaihtuu harmaa↔sininen).
  - **M5.10b Nappien klikkausäänet + moottoriääni** ✅ — `Scripts/UI/ButtonClickSound.cs` (jokainen nappi, kytketään `Korjaus/08`:lla) ja `Scripts/Vehicle/CarEngineAudio.cs` (moottorin pitch/volume kaasutuksen mukaan, kytketään `Korjaus/01`:llä kullekin radalle).

## M6 — Kiillotus 🔨
Tavoite: ulkoasu, ääni, mehukkuus, lokalisointi, suorituskyky.

- **M6.1 Ulkoasu** 🔨 — Vilkku-maskotti (`Scripts/UI/VilkkuController.cs`) on jo mukana päävalikossa ja hallissa; peruspaletti/fontit/napit ovat käytössä. Ei vielä läpikäyty systemaattisesti mockupia vasten.
- **M6.2 3D & valaistus** 🔨 — kaikki neljä rataa low-poly-mallinnettu Blenderissä paletti-atlas-tekniikalla (ei Kenney-assetteja, vaan itse generoituja). Ei erillistä URP-jälkikäsittelyä (bloom/tonemapping) vielä viritetty.
- **M6.3 Ääni** ✅ *(koodi/sisältö, korjattu 11.8. — oli ⬜), ⚠️ scene-kytkentä kesken* — ks. M5.3: musiikki (valikko/halli/rata/pesu), 12 tehostetta (lähtölaskenta, maali, tähti, kupla, nappi, boost, läträkkö, kolhu, osa-sopii, tarran-avaus, moottori) ja nappien klikkausäänet ovat kaikki koodattu ja äänitiedostot generoitu. Ei vielä Vilkun omaa ääntä eikä haptiikkaa. Pelaaja ei kuule mitään ennen kuin `Korjaus/08` on ajettu onnistuneesti (ks. M5.3).
- **M6.4 Mehukkuus** 🔨 *(pieni parannus 11.8.)* — äänipuolen "juice" (nappiklikit, osan-sopii-chime, tarran-avaus-ääni, moottoriäänen pitch kaasutuksesta) on nyt olemassa koodissa, mutta **visuaalinen** mehukkuus puuttuu yhä kokonaan: ei partikkeleita (konfetti, kimallus) eikä squash/stretch-animaatioita pisteen noususta, tähden keräyksestä tms. Suurin jäljellä oleva halpa parannus juuri nyt.
- **M6.5 Lokalisointi** ⬜ — kaikki tekstit suomeksi kovakoodattuna, ei käännösjärjestelmää.
- **M6.6 Suorituskyky & laitetestaus** ⬜ — ei object poolingia, ei mitattua testausta oikealla laitteella vielä tässä vaiheessa.

## M7 — Julkaisu (Google Play) ⬜
*(uusi osio — tästä keskusteltiin laajasti tämän projektin aiemmassa
suunnitteluvaiheessa, mutta mikään konkreettinen ei ole vielä toteutunut
projektissa itsessään, joten kaikki merkitty suunnitelluksi.)*

- **M7.1 Tietosuojaseloste & lapsille suunnattujen sovellusten säännöt** ⬜ — Google Playn "Designed for Families" -ohjelman vaatimukset, COPPA-tyyppinen huomiointi.
- **M7.2 Ikäluokitus (IARC-kysely)** ⬜.
- **M7.3 Kauppalistaus** ⬜ — kuvakuvakkeet, kuvakaappaukset, kuvaus, otsikko.
- **M7.4 Keystore & julkaisu-build** ⬜ — allekirjoitusavain, AAB-paketointi.
- **M7.5 Parental Gate** ⬜ — sama kuin M5.9, kriittinen ehto julkaisulle jos sovellus kohdistuu lapsille (esim. ostojen/linkkien takana).
- **M7.6 Mainokset/ostot (jos suunnitteilla)** ⬜ — ei vielä päätetty/toteutettu kummallakaan puolella.

---

## Kehitystyökalut (Editor-työkalut) — ei osa M-listaa, mutta oleellinen osa projektia

**Korjattu 11.8.:** alla oleva taulukko oli jäänyt jälkeen — työkalut on jo aiemmin
uudelleenjärjestetty neljäksi omaksi valikoksi (**Pakettiporina → Tarkistus/Korjaus/
Lisays/Tyokalut**), joissa kussakin on oma juokseva 01-alkuinen numerointinsa eikä
yhtä jaettua 1–16-numerointia. Tarkistettu suoraan kaikista `[MenuItem(...)]`-
attribuuteista 11.8. — tämä on nyt täsmälleen se mitä Unityn valikosta oikeasti löytyy:

| Valikko | # | Mitä tekee |
|---|---|---|
| **Tarkistus** | 01 | Tarkista peliscene |
| | 02 | Tarkista halli |
| | 03 | Tarkista pesu |
| | 04 | Tarkista päävalikko |
| | 05 | Analysoi data |
| | 06 | Tarkista äänet (kaikki scenet) |
| **Korjaus** | 01 | KORJAA peliscene *(rata-tietoinen, toimii kaikilla 6 ratascenellä; ei enää luo ylimääräisiä objekteja MainMenu/Halli/Pesu-sceneihin — korjattu 11.8.)* |
| | 02 | KORJAA halli |
| | 03 | KORJAA pesu |
| | 04 | KORJAA päävalikko |
| | 05 | Korjaa kategoriat ja kosmeettisuus |
| | 06 | Tasapainota osien arvot |
| | 07 | KORJAA KAIKKI (data: kategoriat + tasapaino) |
| | 08 | KORJAA äänet (kaikki scenet) *(uusi 11.8., ks. M5.3)* |
| **Lisäys** | 01 | Asettele tähdet radalle |
| | 02 | Lisää pelielementit tämän radan radalle |
| | 03 | Lisää teemakoristeet tälle radalle |
| | 04 | Lisää teemapuiston kartta päävalikkoon |
| | 05 | Luo lisäosia |
| | 06 | Generoi 5 uutta pakettia |
| | 07 | Luo tarrat (avattavat) |
| | 08 | Luo salaiset osat |
| | 09 | Lisää/päivitä asetuspaneeli päävalikkoon *(uusi 11.8., ks. M5.2/M5.10)* |
| **Työkalut** | 01 | Näytä tien profiili (konsoli) |
| | 02 | Näytä tien profiili Scene-näkymässä (päällä/pois) |
| | 03 | Vie scenen elementit (konsoli) |
| | 04 | Vie sijainnit koodiksi (konsoli) |

Ei aukkoja eikä duplikaatteja numeroinnissa millään neljästä valikosta — vanha
"numero 3 puuttuu / numero 6 kahdesti" -huomio ei siis enää päde nykyiseen
rakenteeseen. `PakettiporinaMitat.cs`-tiedostoa ei myöskään enää ole
projektissa (tarkistettu 11.8. — poistettu jossain vaiheessa, ei vaadi
toimenpiteitä).

---

## Avoimet kysymykset / mahdollisesti unohtuneet asiat

Näitä ei ole aiemmin kirjattu mihinkään M-kohtaan — tarkistettu 11.8. suoraan
tiedostojärjestelmästä, ei pelkkää muistia:

- **`Scenes/Game2.unity`** vaikuttaa hylätyltä: muokkausaika 28.7., kun kaikki
  muut scenet (Game, Puisto, Satama, Huvipuisto, Maatila, MainMenu, Garage,
  Wash) on muokattu 11.8. Siitä puuttuvat `CarEngineAudio`, `CarPainter` ja
  `AudioManager` — toisin kuin kaikista muista ratasceneistä. `AudioManager.
  RaceScenes`-taulukossa Game2 on silti mukana. **Ei tiedossa onko tämä
  tarkoituksella jäänyt varhainen versio (esim. korvattu Gamella) vai
  unohtunut kesken** — kannattaa päättää: joko aja `Korjaus/01` sille tai
  poista/arkistoi scene ja siivoa se `RaceScenes`-taulukosta.
- **`Scripts/CarCare/`-kansio on tyhjä** (vain `.gitkeep`). M4:n historiamerkinnän
  mukaan alkuperäinen "CarCareController"-suunnitelma korvautui `WashScreen.cs`
  :lla eikä koskaan syntynyt — tämä kansio on ilmeisesti sen jäänne. Turvallista
  poistaa Unityn kautta, ellei sinne ole suunnitteilla jotain muuta.
- **Ei automaattitestejä missään muodossa** (ei `Tests`-kansiota, ei `[Test]`/
  `[UnityTest]`-attribuutteja koko koodikannassa). Ei kriittinen tämän kokoiselle
  yhden kehittäjän projektille, mutta jos `ArcadeCarController`- tai
  `CarStats`/`FitChecker`-laskentaan aletaan tehdä isompia muutoksia jatkossa,
  edes muutama EditMode-testi laskentafunktioille (esim. `FitChecker.CheckFit()`)
  antaisi turvaverkon ilman että jokainen muutos vaatii manuaalista pelitestausta.

## Suositeltu seuraava askel

Isoimmasta vaikutuksesta pienimpään, tilanne 11.8. jälkeen:

1. **Aja `Lisays/09` ja `Korjaus/08` Unityssä** — koodi on valmis, mutta scenet
   levyllä eivät vielä sisällä kumpaakaan (ks. M5.2/M5.3). Ilman tätä askelta
   mikään tämän kierroksen työstä ei näy pelaajalle.
2. **Build Settings** — lisää Maatila (ja tarkista Puisto/Satama/Huvipuisto ovat
   yhä listalla), ks. M2.16/M2.12. Puhtaasti manuaalinen Unity-askel.
3. **Päätä Game2:n kohtalo** (ks. yllä) — ettei se jää hämäräksi jäänteeksi.
4. **Visuaalinen mehukkuus (M6.4)** — nyt kun äänipuoli on valmis, tämä on
   suurin jäljellä oleva halpa "tuntuma"-parannus (partikkelit, squash/stretch).
5. **Parental Gate (M5.9/M7.5)** — ei kiireellinen tänään, mutta pidä mielessä
   heti kun mikään osto-/mainos-/ulkoinen linkki -toiminnallisuus lisätään.

---

*Koodin kommenttien `[M1]`, `[M2]`, `[M3]` viittaavat päävirstanpylvääseen; tämä
dokumentti purkaa ne alivaiheiksi. Päivitä tilat (✅/🔨/⬜) edetessäsi.*
