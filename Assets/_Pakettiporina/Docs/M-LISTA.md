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

- **M2.11 Boost / Lätäkkö / Kartio / Laatikko -pelielementit** ✅ *(Laatikko lisätty 14.8.2026)* — `Scripts/Gameplay/Boost.cs`, `Puddle.cs`, `Cone.cs`, `Laatikko.cs` + `ArcadeCarController`:n tilapäiskerroin-kerrostus. Editor-työkalu `PakettiporinaElements.cs` (`Lisäys/02`) sijoittaa nämä radan `bends`-kaavan mukaan automaattisesti, ja täydellinen esterosteri on nyt `PakettiporinaTracks.cs`:ssä KAIKILLE 5 oikealle radalle (Game, Puisto, Satama, Huvipuisto, Maatila). Aiemmin kokeiltu erillinen "TrackBubble"-kuplamekaniikka (`PakettiporinaTrackObstaclesSetup.cs`) on POISTETTU käytöstä 14.8. (kayttäjä: "poistetaan käytöstä, vain tähdet kerättävissä") - vain tähdet ovat radalla kerättäviä esineitä. ⚠️ **Ei vielä ajettu Unityssä millään radalla** — aja `Lisäys/02` jokaisessa 5 ratascenessä erikseen, ja `Lisäys/10` Maatilalla/Puistolla/Satamalla siivoamaan vanhat Kupla-oliot pois.
- **M2.12 Kolme uutta rataa (Puisto, Satama, Huvipuisto)** ✅ — omat scenet (`Scenes/Puisto.unity`, `Satama.unity`, `Huvipuisto.unity`), maastot generoitu Blenderillä (`pakettiporina_ratageneraattori.py` + tämän session laajennukset), noin 30 s ajomatka per rata.
  - ⚠️ **Kesken vielä**: kaikkien pitää olla lisättynä ja päällä **File → Build Settings → Scenes In Build**, muuten `SceneManager.LoadScene()` ei löydä niitä oikeassa buildissa (toimii silti Editorin Play-tilassa, mikä on hämännyt aiemmin). Tarkista `Korjaus/02 - KORJAA halli` -lokista onko vielä varoituksia.
- **M2.16 Neljäs uusi rata — Maatila (5. rata kokonaisuudessaan)** ✅ *(uusi, lisätty 10.–11.8., laajasti päivitetty 14.–16.8.)* — `Scenes/Maatila.unity`, rakennettu Puiston `TrackDef`-mitoilla mutta oma rauhallinen/helppo teema, ks. `Docs/Rata_Maatila_speksi.md`. Kytketty `Kukkaruukku`-pakettiin. Kaikki päärakennus/eläinmallit (Navetta, Traktori x2, Tuulimylly, Hevonen, Maissipelto) rakennettu/rakennettu UUDELLEEN Blenderillä 14.–16.8. — selvästi parempi visuaalinen taso kuin alkuperäiset primitiivipohjaiset mallit. Kaikki Maatila-koristeiden sijainnit (`PakettiporinaDecor.cs`) järjestetty 16.8. tien puolen mukaan: eläinaitaukset+maissipelto+1 traktori OIKEALLE, kaikki muu (navetta, heinäpaalit, tuulimylly, variksenpelätti, 1. traktori, siilo, kaivo) VASEMMALLE.
  - **Kaikki 7 koristemallia (Navetta, Aita, Heinäpaali, Lammas, Lehmä, Tuulimylly, Variksenpelätti) väri-/materiaaliuudistettu 17.8.2026** — Blender-tarkastus paljasti että jokaisella oli tosiasiassa yksi yhtenäinen harmaa placeholder-materiaali (0.8,0.8,0.8) ilman minkäänlaista väriä, RISTIRIIDASSA 16.8.-merkinnän "muut värit kunnossa" -väitteen kanssa (se tarkasti ilmeisesti vain DEBUG_HOLE_MARKERin, ei perusväriä). Jokainen malli sai nyt aiheeseen sopivat monivärimateriaalit per-face (esim. Navetta: punainen seinä/tumma katto/ruskea ovi/ikkuna, Lehmä/Lammas: villa+mustat läiskät, Tuulimylly: valkoinen torni+punaiset lavat). Samalla korjattu Navetan katon pieni aukko harjalla ja Tuulimyllyn käännetyt normaalit navan luona. **Uusi bugilöytö**: kaikkien 7 tiedoston `.fbx.meta` `useFileScale` oli `1`, kun jokaisella muulla radan koristeella koko projektissa (Puisto/Satama/Huvipuisto) se on `0` — korjattu kaikki seitsemän. Variksenpelätti on siis nyt visuaalisesti valmis, ei enää avoin kohta.
  - ⚠️ **Kesken vielä**: `Maatila.unity` puuttuu todennäköisesti yhä **File → Build Settings → Scenes In Build** -listalta — tätä ei voi tarkistaa/korjata Editor-työkaluilla (ProjectSettings ei ole assistentin ulottuvilla), pelkkä manuaalinen Unity-askel. Myös `Lisäys/03 - Lisää teemakoristeet` pitää ajaa jotta malliuudistukset (14.–17.8.) ja sijaintien uudelleenjärjestely päivittyvät itse sceneen (koodikommentti tässä tiedostossa sanoi aiemmin virheellisesti "12" — korjattu 16.8.).
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
- **M4.3 Korjausvaihe** ✅ *(valmis 15.–16.8.2026, oli ⬜)* — kolhut korjataan NYT samassa Pesu-scenessä pesun jälkeen, TÄSMÄLLEEN samalla mekaniikalla kuin pesu: `RepairArea`+`RepairTool.cs` (kelluvat, napautettavat ikonit, sama liikelogiikka kuin `Bubble.cs`) poistavat `DentSpots`-kolhumerkit autosta yksi kerrallaan. Aani (`AudioManager.PlayRepair`) + hetken näkyvä `RepairEffect`. Siirretty pois Hallista (`GarageScreen`) kokonaan. ⚠️ **Ei vielä ajettu Unityssä** — `Korjaus/03 - KORJAA pesu` rakentaa/kytkee UI:n; `RepairTool`-ikonit ja `DentSpots`-merkit ovat vielä pelkkiä värillisiä placeholder-neliöitä (ei omaa kuvitusta).
- **M4.4 Kuplien poksautus + puhtausmittari** ✅ — `Scripts/Gameplay/Bubble.cs` + `BubbleArea`, `ProgressText` ("Poksautit X/8 kuplaa!"). ⚠️ Täydellisesti ajettu keikka (ei likaa eikä kolhuja) ei vieläkään tuota mitään ylimääräistä palkkiota — hyppää vain suoraan Valmis-paneeliin +0 pisteellä. Ei ratkaisua (ks. Avoimet kysymykset).
- **M5.17 Pysyvä lika-/kolhumittari** ✅ *(uusi 16.8.2026, käyttäjän idea)* — aiempi malli
  (kolhut/lika nollautuivat jokaisen ajon alussa) korvattiin PYSYVÄLLÄ, katollisella
  mittarilla: `GameManager.DirtMeter` (max 15) / `DentMeter` (max 6) kasvavat
  Puddle/Cone/Laatikko-osumista, näkyvät LIVE-täyttöpalkkeina `RaceHUD`:ssa ajon
  aikana (`OnDirtMeterChanged`/`OnDentMeterChanged`-tapahtumat), EIVÄT nollaudu ajon
  alussa, ja purkautuvat Pesussa asteittain per napautus (`CleanOneDirt`/
  `RepairOneDent`) — ei kertaheitolla lopussa, jotta kesken jäänyt pesu/korjaus ei
  häviä. Ei pakota pesuun/korjaukseen — täysi mittari on vain visuaalinen vihje.
  Tallentuu levylle `SaveData.dirtMeter/dentMeter` -> säilyy sovelluksen
  uudelleenkäynnistyksen yli. Latakkomäärää nostettu kaikilla 5 radalla (2-4 -> 5
  kpl/rata, `PakettiporinaTracks.cs`) jotta 15-kattoinen mittari on realistisesti
  täytettävissä muutaman ajon aikana. RepairTool sai myös vasaran-siluetti-ikonin
  (kaksi suorakulmiota, ei oikeaa kuvitusta) ja vasarointi-animaation (kierto+punch)
  pelkän häviä-ja-ilmesty-poksahduksen sijaan. ⚠️ **EI VIELÄ AJETTU Unityssä** — aja
  `Korjaus/01` jokaisessa ratascenessä (luo DirtMeterBar/DentMeterBar-palkit
  RaceHUD:iin) ja `Korjaus/03` Pesussa (vasara-ikonit), sekä `Lisäys/02` jokaisessa
  ratascenessä (uudet latakkomäärät).
  **TARKISTETTU 16.8.2026 jälkikäteen, 4 oikeaa bugia löytyi ja korjattu:**
  (1) uusien latakko-Z:ien osui 5 kohdassa täsmälleen saman Z:n päälle jonkin
  kartion/laatikon/boostin kanssa (esim. Satama latakko z=-130 = Kartio_01:n z) -
  siirretty pois päältä kaikilla radoilla; (2) RepairTool.SetAllAlpha olisi
  paljastanut näkymättömän napautusalue-Imagen valkoisena neliönä jäähdytyksen
  jälkeen - korjattu ohittamaan oma Image; (3) `WashScreen.RestartWash()` luki
  vanhentunutta `activeSpotCount`-arvoa, mikä mahdollisti loputtoman
  pesupalkkion keräämisen "Uudestaan"-nappia spämmäämällä - lukee nyt tuoreen
  `DirtMeter`-arvon; (4) sama korjaus paljasti softlock-riskin
  `StartRepairPhase()`:ssa (DonePanel/Jatkoon-nappi ei olisi voinut näkyä
  uudelleen) - korjattu; (5) `SaveManager.ResetProgress()` ("Nollaa
  edistyminen") ei nollannut DirtMeter/DentMeter-arvoja - korjattu.
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
  - **KOKONAISVALTAINEN PARANNUS 16.8.2026** (käyttäjän pyyntö): (1) löytyi ja korjattiin
    oikea aukko — `pakettiporina_aani_korjaus.wav` (korjaustyökalun ääni, ks. M5.17)
    PUUTTUI kokonaan levyltä, eli koko istunnon aikana rakennettu korjausmekaniikka olisi
    ollut TÄYSIN ÄÄNETÖN — synteesillä (numpy/scipy) generoitu puuttuva klippi samaan
    muotoon/tyyliin kuin muut SFX:t (44.1kHz mono, lyhyt kirkas "tink"-vasarointiääni,
    erottuu sekä kolhu- että sopii-äänistä). (2) `AudioManager`: lisätty pieni satunnainen
    sävelkorkeuden vaihtelu (`sfxPitchVariance`, oletus 0.06) usein toistuville äänille
    (kolhu/roiske/kupla/korjaus/tähti/nappi) - vähentää robottimaista "aina täsmälleen
    sama ääni" -toistoa. (3) Musiikin vaihto scenejen välillä (esim. Ajo->Pesu) häivyttää
    nyt pehmeästi (`musicCrossfadeDuration`, 0.6s) kovan katkaisu+aloitus-vaihdon sijaan.
    (4) Tarkistettu: moottoriääni skaalaa jo oikein pitch/volumen nopeuden mukaan
    (`CarEngineAudio.cs`, ei muutostarvetta) ja musiikkiraitojen loop-pisteet ovat jo
    näyte-tasolla jatkuvia (ei klik-ääntä silmukan kohdalla, tarkistettu waveform-analyysillä).
    **EI VIELÄ AJETTU Unityssä** — aja `Korjaus/08` jotta uusi klippi kytkeytyy.
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

**Päivitetty 16.8.2026** (jatkoa 11.8. ja 14.8. listoihin — ks. myös
`Tiivistelma_2026-08-14.md` kohta B):

- **`Scenes/Game2.unity` / `Kyla.unity`** — TÄMÄ ON YHÄ AUKI, vanhin ratkaisematon
  asia listalla. 14.8. löytyi että `Game2.unity` ei ole enää olemassa - sen
  paikalla on uusi `Kyla.unity` (kylä/kaupunkiteema, käyttäjän 14.8. pyynnöstä),
  mutta se on käytännössä tyhjä runko: ei autoa, ei maastomallia, ei ääniä, yksi
  komponentti on väärin kirjoitettu ("GroundCollinder"). Koodi (`AudioManager.
  RaceScenes` ym.) viittaa yhä vanhaan "Game2"-nimeen. EI KÄSITELTY tässä
  16.8.-sessiossa (koko sessio meni Maatilaan + pesu/korjaus-uudistukseen).
  Pitää joko viedä loppuun (maasto+auto+äänet+`Korjaus/01`+teemakoristeet) tai
  päättää tietoisesti lykätä ja siivota koodiviittaukset sen mukaisesti.
- **`Scripts/CarCare/`-kansio on tyhjä** (vain `.gitkeep`) - jäänne alkuperäisestä
  "CarCareController"-suunnitelmasta joka korvautui `WashScreen.cs`:lla. Turvallista
  poistaa Unityn kautta.
- **Ei automaattitestejä missään muodossa** (ei `Tests`-kansiota, ei `[Test]`/
  `[UnityTest]`-attribuutteja). Ei kriittinen tämän kokoiselle projektille, mutta
  `ArcadeCarController`/`CarStats`/`FitChecker`-laskennalle edes muutama
  EditMode-testi antaisi turvaverkon.
- ~~Variksenpelätti on ainoa Maatilan hahmoista/rakennuksista joka ei ole saanut
  visuaalista Blender-uudistusta~~ **KORJATTU 17.8.2026** — kaikki 7 Maatilan
  koristemallia (Variksenpelätti mukaan lukien) sai väri-/materiaaliuudistuksen,
  ks. M2.16. "Maatilan objektit paremmiksi" -kokonaisuus on nyt valmis
  koodi/malli-puolella; puuttuu vielä `Lisäys/03`-ajo Unityssä jotta näkyy
  pelissä.
- **Täydellinen ajo (ei likaa, ei kolhuja) ei anna mitään ylimääräistä palkkiota**
  Pesussa - hyppää vain suoraan Valmis-paneeliin +0 pisteellä. Tämä oli avoin jo
  14.8. (B5) eikä ratkaistu 16.8. korjaus-uudistuksen yhteydessä. Vaatii
  suunnittelupäätöksen: pieni "puhdas ajo" -bonus vai oma pieni kiillotus-minipeli.
- **RepairArea/DentSpots käyttävät vielä placeholder-grafiikkaa** —
  `PakettiporinaWashSetup.cs` rakentaa RepairTool-ikonit pelkkinä värillisinä
  neliöinä ja DentSpot-merkit harmaasävyisenä samana likatahra-spritenä eri
  värillä - ei vielä omaa kolhu-/ruuvitaltta-kuvitusta. (Toiminnallisuus on nyt
  kunnossa, ks. seuraava kohta - vain ulkoasu on placeholder.)
- **Maalin "Pesuun"-nappi korjattu 16.8.2026**: napissa ei ollut tekstiä lainkaan
  ja sen ikoni oli virheellisesti Halli-ikoni (nappi vie Pesuun, ei suoraan halliin)
  - käyttäjä kysyi "miten pääsee pesemään" mikä paljasti tämän. Korjattu
  `Korjaus/01`:een (teksti "Pesuun" + jatka-nuoli-ikoni).
- **KRIITTINEN LÖYDÖS JA KORJAUS 16.8.2026: korjausvaihetta ei voinut oikeasti
  tehdä loppuun asti.** Suoralla `Wash.unity`-scenen YAML-tarkastuksella (ei
  Unityn kautta) löytyi kaksi bugia jotka yhdessä tekivät korjausvaiheesta
  käytännössä toimimattoman, vaikka koodi (`WashScreen.cs`) oli oikein:
  1. `DentSpots`-objektia (kolhu-merkit autossa) ei ollut scenessä OLLENKAAN ->
     `WashScreen.Start()` laski `activeDentCount`:n aina nollaksi -> korjausvaihe
     ohitettiin AINA, vaikka radalla olisi tullut kolhuja.
  2. `DentsText` oli vahingossa vanhan, aina-pois-päältä-olevan `RepairPanel`
     -jäänteen (ks. alla) lapsena -> teksti ei olisi näkynyt pelaajalle KOSKAAN
     vaikka korjausvaihe olisi toiminutkin.
  Näiden lisäksi scenessä oli vanhan yhden-napin korjaussuunnitelman kuollutta
  jäännöstä: `RepairPanel`+`RepairButton`, `OnClick` viittasi metodiin
  `OnRepairButton` jota `WashScreen.cs`:ssä ei enää ole (korvattu RepairArea/
  RepairTool-mekaniikalla). Korjattu `PakettiporinaWashSetup.cs`:ään
  (`Korjaus/03`): DentSpots + 6 kolhu-merkkiä luodaan nyt automaattisesti jos
  puuttuvat, DentsText siirretään suoraan Canvasin alle (ei minkään paneelin
  sisään), ja vanha RepairPanel/RepairButton poistetaan kokonaan.
  **EI VIELÄ AJETTU Unityssä** — aja `Korjaus/03` Pesu-scenessä ja `Korjaus/01`
  jokaisessa ratascenessä.

## 16.8.2026 — Testauslistan bugit + jono-backlog (Task #22-#32)

Käyty läpi käyttäjän 10-kohtainen priorisoitu backlog järjestyksessä. Kaikki
vaativat vielä Unity/Blender-uudelleenajon (ks. "Suositeltu seuraava askel"):

- **Testauslista.md luotu** (`Docs/Testauslista.md`) — 9 osiota kattaen koko
  pelattavuuden.
- **Nopeustaso ei erottunut**: `ArcadeCarController.SpeedTierMultiplier`
  0.72-1.32 -> 0.55-1.7 (n. 3x kerroinväli, oli n. 1.8x).
- **Progress bar ei näyttänyt osumia**: juurisyy — `PakettiporinaSetup.cs`
  (ratascenen tarkistustyökalu) ei luonut `GameManageria` jos se puuttui,
  toisin kuin Pesu-scenen työkalu. Lisätty sama automaattinen
  luonti/uudelleenkäyttö kuin Pesussa.
- **Progress barit siistitty**: sijoitus lasketaan nyt live PauseButtonin
  RectTransformista (ei kovakoodattuja koordinaatteja), pienemmät
  palkit + Lika/Kolhut-tekstit, pinottu Pause-napin yläpuolelle.
- **Objektien värit tarkistettu**: Blender-materiaaliaudit kaikille
  Maatila-malleille — löytyi ja poistettiin käyttämätön `DEBUG_HOLE_MARKER`
  (magenta) -jäänne `Maatila_Navetta`:sta, muut värit kunnossa.
- **Lätäkkö ei erottunut tiestä**: `PakettiporinaElements.PlacePuddle`
  uusittu — epäsäännöllinen soikea muoto (deterministinen per lätäkkö),
  tummempi/kylläisempi väri + kiiltävä (korkea Smoothness) materiaali,
  tumma "märkä reuna" -halo taustalla. Ks. `Lisäys/02`.
- **Pesu/korjaus loppuu liian nopeasti**: `GameManager.HIT_MULTIPLIER = 2` —
  yksi osuma radalla kasvattaa lika-/kolhumittaria nyt 2 yksiköllä (katto
  DIRT_MAX/DENT_MAX ennallaan) -> enemmän siivottavaa/korjattavaa Pesussa.
- **Hevonen ei ollut valmis**: Blenderissä renderöity kuva paljasti että
  malli näytti oikeasti laamalta/kameelilta (liian pitkä ohut kaula,
  irtonaiset harjapiikit ilmassa, paksut jalat, litteä hännänmuotoinen
  levy). Rakennettu kokonaan uudelleen bmesh-koodilla: lyhyempi/paksumpi
  porrastettu kaula, kiinteä selkään/kaulaan upotettu harjaharjanne,
  riippuva kapeneva häntä, ohuemmat jalat, tarkistettu ettei jää rakoja
  (jalka-runko ja pää-kuono -liitokset). Uudelleenvienti
  `Maatila_Hevonen.fbx`, guid/useFileScale säilyi.
- **Tuulimylly 180°**: `PakettiporinaDecor.cs`, rotY 0 -> 180.
- **Eläinaitaukset jonoon**: Lehma/Hevonen/Lammas/Kana -aitaukset olivat
  2x2-ruudukossa (kaksi saraketta), järjestetty yhdeksi riviksi samalle
  X-kaistalle (14,22..21,78), Z-keskukset -4/8/20/32 (12 yksikön väli,
  sama rako kuin alkuperäinen Lehma-Hevonen-pari).
- **Maissipelto 90°**: rotY 0 -> 90 — tarkistettu että tämä itse asiassa
  KASVATTAA marginaalia tiehen (lyhyempi sivu osoittaa nyt tiehen päin).

## 16.8.2026 (jatko) — Progress bar -bugi + oikeat FBX-mallit esteille/tehosteille

- **KRIITTINEN BUGI LOYDETTY JA KORJATTU**: kayttaja raportoi "korjaa peliscene
  poistaa kokonaan progress barit". Todellinen syy: `PakettiporinaSetup.cs`:n
  meter-bar-sijoittelu kopioi PauseButtonin anchorMin/anchorMax/anchoredPosition
  sellaisenaan, mutta PauseButton ON sisakkaisen "Header"-container-GameObjectin
  lapsi (ankkuroitu Canvasin KESKELLE, ei ylareunaan) - palkit paatyivat n. 850+
  yksikkoa vaaraan suuntaan, reilusti nakymaalueen ulkopuolelle. Korjattu
  laskemaan PauseButtonin ylareunan keskikohta MAAILMAKOORDINAATEISSA
  (`GetWorldCorners`) ja muuntamaan se Canvasin paikalliseen tilaan
  (`InverseTransformPoint`) - toimii nyt oikein riippumatta UI-hierarkiasta.
- **Kartio/Laatikko/Latakko/Boost ovat nyt oikeita Blender-FBX-malleja**
  (ei enaa paljaita Unity-primitiiveja). Uudet mallit: `Art/Models/Esteet/
  {Kartio,Laatikko,Latakko,Boost}.fbx`. Kartio = liikennekartio valkoisella
  raidalla, Laatikko = puulaatikko lautajaolla+kulmalistoilla, Latakko =
  epasaannollinen blobi + tumma reunahalo SAMASSA meshissa (korvaa vanhan
  kaksi-primitiivia-ratkaisun), Boost = vihrea nuolipad. `PakettiporinaElements.
  cs` lataa nama `FindOrCreateModel`-funktiolla (sama periaate kuin
  PakettiporinaDecor.cs) ja lisaa colliderin kasin (FBX ei tuo sita automaattisesti
  toisin kuin CreatePrimitive) - primitiivi-fallback sailyy jos FBX puuttuu.
  **HUOM UUSILLE FBX:ILLE**: ensimmaisella Unity-tuonnilla .meta voi saada
  `useFileScale: 1` (tunnettu bugi, ks. Decor-kommentit) - tarkista/korjaa
  0:ksi jos mallit nayttavat n. 100x liian pienilta.
- **EI VIELA UNITY-TESTATTU** - aja `Lisays/02` jokaisessa 5 ratascenessa JA
  `Korjaus/01` jokaisessa 5 ratascenessa.

## Suositeltu seuraava askel

**Päivitetty 16.8.2026.** Isoimmasta vaikutuksesta pienimpään:

0. **Aja koko 16.8. backlogin muutokset** (ks. yllä oleva osio) — erityisesti
   `Lisäys/02` (uusi lätäkkömuoto/väri + HIT_MULTIPLIER näkyy vasta pelissä),
   `Lisäys/03` Maatilalla (uusi Hevonen-malli, Tuulimylly-kierto, aitausten
   uudelleenjärjestely, Maissipelto-kierto), `Korjaus/01` (GameManager-korjaus
   + progress bar -sijoittelu).

1. **Aja kaikki tämän session (14.–16.8.) Unity-työkalut** — MIKÄÄN tässä ja
   edellisessä sessiossa tehdyistä koodimuutoksista ei näy pelissä ennen tätä:
   - `Lisäys/02` jokaisessa 5 ratascenessä (Laatikko-este + koko esterosteri +
     UUDET latakkomäärät pysyvää likamittaria varten, 16.8.)
   - `Lisäys/10` Maatilalla/Puistolla/Satamalla (siivoaa vanhat Kupla-oliot)
   - `Lisäys/03` Maatilalla (uudet mallit + sijaintien uudelleenjärjestely)
   - `Korjaus/03` Pesussa (RepairArea/DentSpots/vasara-ikonit/koko korjausvaihe,
     siivoaa myös vanhan RepairPanel/RepairButton-jäänteen, 16.8.)
   - `Korjaus/01` jokaisessa ratascenessä (maalin "Pesuun"-nappi + UUDET
     DirtMeterBar/DentMeterBar-palkit RaceHUD:iin, 16.8.)
   - `Korjaus/08` (äänet, jos `repairSound` tms. ei vielä soi)
2. **Build Settings** — tarkista Maatila + kaikki muut scenet ovat listalla
   (puhtaasti manuaalinen Unity-askel, ei tarkistettavissa Editor-työkalulla).
3. **Päätä Game2/Kyla:n kohtalo** (ks. Avoimet kysymykset) — vanhin auki oleva
   asia, ei käsitelty 14.–16.8.
4. ~~Variksenpelätti viimeisenä Maatilan visuaalisena kohteena~~ **VALMIS 17.8.2026**
   — kaikki 7 Maatilan koristemallia väri-/materiaaliuudistettu, ks. M2.16.
5. **"Puhdas ajo" -palkinto** Pesussa — vaatii suunnittelupäätöksen.
6. **Visuaalinen mehukkuus (M6.4)** — partikkelit, squash/stretch, edelleen
   tekemättä.
7. **Parental Gate (M5.9/M7.5)** — ei kiireellinen, mutta pidä mielessä.

---

*Koodin kommenttien `[M1]`, `[M2]`, `[M3]` viittaavat päävirstanpylvääseen; tämä
dokumentti purkaa ne alivaiheiksi. Päivitä tilat (✅/🔨/⬜) edetessäsi.*
