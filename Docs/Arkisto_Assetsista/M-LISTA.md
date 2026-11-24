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

## 17.8.2026 — Latakko näkymätön radalla, bugi löytyi ja korjattu

Käyttäjä testasi Unityssä: Latakko-objektit olivat oikeassa kohdassa Hierarkiassa
mutta eivät näkyneet radalla lainkaan. Blender-tarkastus (kaikki 4 este-FBX:ää
Kartio/Laatikko/Latakko/Boost tuotu ja mitattu) paljasti juurisyyn: **kaikki 32
Latakko.fbx:n pintaa osoittivat alaspäin (normal (0,-1,0))** — pelkkä normaalibugi,
ei sijainti- tai rotaatio-ongelma. Backface culling piilotti sen siis jokaisesta
kamerakulmasta vaikka geometria oli aivan oikeassa paikassa. Kartio/Laatikko/Boost
ovat kaksipuoleisia (yhtä paljon ylös- ja alaspäin osoittavia pintoja per kohde),
Latakolta puuttui kokonaan ylöspäin osoittava puolisko. Korjattu duplikoimalla
Latakon pinnat ja kääntämällä kopion normaalit (64 pintaa, 32+32, sama periaate
kuin sisarmalleilla) ja viety `Latakko.fbx` uudelleen. `useFileScale: 0` tarkistettu
kaikilta neljältä (kunnossa, ei siis se tunnettu bugi tällä kertaa).

**Rotaatiot muuten tarkistettu ja kunnossa**: Kartio on täydellisen
pyörähdyssymmetrinen kartio pystyssä paikallista Y-akselia pitkin (kärki suoraan
kannan keskipisteen yläpuolella), Laatikko on tavallinen laatikko oikein päin,
Boost on litteä nuolipatja — kaikki toimivat identity-rotaatiolla juuri niin kuin
koodikommentti (`PakettiporinaElements.cs`) väittää. Ei siis tarvetta lisätä
rotaatiokorjausta millekään näistä neljästä.

**EI VIELÄ UNITY-TESTATTU** — Unity lukee `Latakko.fbx`:n uudelleen kun scene
avataan/tallennetaan (tai pakota `Assets → Reimport`), minkä jälkeen lätäköiden
pitäisi näkyä radalla.

## 17.8.2026 (jatko) — Lika-/kolhupalkkien tyylipäivitys

Käyttäjä: "lika ja kolhut progress bar tyyliltään paremmin". Aiempi versio oli
litteä tumma laatikko + teksti keskellä TAYTTEEN PÄÄLLÄ (vaikea lukea). Uusi
versio (`PakettiporinaSetup.cs`, `Korjaus/01`) käyttää samaa pyöristettyä
"pilleri"-spritea kuin Asetuspaneelin kortit/napit (`pakettiporina_ui_rounded`),
pientä ikonia palkin vasemmassa reunassa (likatahra-kuva Lika-palkissa,
kestävyyskilpi-ikoni Kolhut-palkissa) ja tarkkaa "Lika 3/15" -lukumäärää
palkin YLÄPUOLELLA tekstinä sen sijaan että pelaaja arvaisi määrän pelkästä
täyttöprosentista. `RaceHUD.cs` sai kaksi uutta kenttää (`dirtMeterLabel`/
`dentMeterLabel`) joita `HandleDirtMeterChanged`/`HandleDentMeterChanged`
päivittävät livenä. **EI VIELÄ UNITY-TESTATTU** — aja `Korjaus/01` jokaisessa
ratascenessä.

## 18.8.2026 — Game-radan tehosteet/esteet väärällä puolella tietä (bends-etumerkki)

Käyttäjä: "Game scenessä tehosteet ja esteet vielä asettelut ja rotaatiot pielessä".
Game.unity:ssä ei ollut yhtään Boost_/Kartio_/Laatikko_/Latakko_-objektia tallennettuna
(käyttäjä oli testannut ajamalla `Lisäys/02` omassa avoimessa Unity-istunnossaan).
Todistettiin juurisyy suoraan `Maasto.fbx`:stä Blenderissä: mesh tuotiin, sen
`Maasto_Paletti`-tekstuurin väristä (harmaa = tie, vihreä = nurmi) tunnistettiin
oikeat tie-pinnat UV:n kautta, ja niiden todellinen X-keskikohta laskettiin jokaisella
Z:lla. Tulos verrattuna `PakettiporinaTracks.cs`:n `RoadX`-kaavaan: esim. z=-50:ssä tie
on OIKEASTI X=-20, mutta kaava (bends-amplitudi +20 tällä välillä) antoi X=+20 - TASAN
vastakkaismerkkinen JOKAISESSA näytepisteessä (magnitudit täsmäsivät täydellisesti,
pelkkä etumerkkivirhe). Sama bugiluokka kuin Huvipuistolla aiemmin löydetty ja korjattu
(ks. sen TrackDef-kommentti) - Game-radan bends ei ollut koskaan käsin Unity-testattu
tähän mennessä. **Korjattu**: `bends = new[] { (-100f, 0f, 20f), (0f, 100f, -20f) }`
→ `{ (-100f, 0f, -20f), (0f, 100f, 20f) }`. Vahvistettu visuaalisesti Blenderissä
(keltaiset merkkipallot osuvat nyt tarkasti tien päälle koko radan matkalta).
**EI VIELÄ UNITY-TESTATTU** — aja `Lisäys/02` uudelleen Game-scenessä.

## 18.8.2026 (jatko) — "TEE KAIKKIEN SCENEJEN TARKISTUS" -kokoaudit

Käyty läpi ASETTELUT/ROTAATIOT/VÄRIT/MEKANIIKAT/EFEKTIT/TOIMINNOT kaikissa 5
ratascenessä + Wash/Kyla.

**TÄRKEÄ LÖYDÖS — Blender-tekstuurivärimenetelmän luotettavuus etumerkin suhteen:**
sama menetelmä jolla Game-radan bug (yllä) todistettiin ajettiin myös Puisto/Satama/
Huvipuisto/Maatila-radoille. KAIKKI NELJÄ näyttivät saman "täydellinen magnitudi,
väärä etumerkki" -kuvion. Ennen korjausten tekemistä verrattiin tulosta Huvipuiston
OMAAN 8.8.2026-historiaan (ks. sen TrackDef-kommentti) — ja siitä löytyi ristiriita:
silloin sama Blender-menetelmä NÄYTTI täsmäävän alkuperäisiin arvoihin, mutta
käyttäjän oikeasti Unityssä käsin testaama korjaus vaati PÄINVASTAISEN etumerkin.
Eli tämä diagnostiikkamenetelmä on jo kertaalleen TODISTETUSTI antanut väärän
vastauksen — sille ei voi luottaa sokeasti ilman Unity-vahvistusta. Tämän takia:
- **Puisto ja Satama**: Blender epäili etumerkkiongelmaa, mutta KOSKA näistä ei ole
  tullut yhtään oikeaa käyttäjän Unity-havaintoa (esteet/tehosteet väärällä
  puolella), muutosta EI tehty koodiin - vain kommentti lisätty TrackDefiin joka
  kertoo tarkalleen mitä kääntää (ja miten) jos Unity-testi joskus vahvistaa bugin.
- **Game**: korjaus (18.8, ks. yllä) jätetty voimaan koska sillä ON aito käyttäjän
  Unity-havainto taustalla, mutta lisätty varoituskommentti - jos tehosteet
  näyttävät YHÄ väärin (tai nyt vastakkaisella puolella kuin ennen), etumerkki
  pitää ehkä kääntää takaisin.
- **Maatila**: sama epäily, ei käyttäjähavaintoa, ei muutosta - kommentoitu.
- **Satama**: löytyi MYÖS erillinen, varma bugi diagnostiikkakoodissani itsessään
  matkalla (Satama.fbx:llä on FBX-tuonnin 0.01-skaala, jota ensimmäinen ajo ei
  huomioinut → järjettömiä lukuja n. 100x liian isoina) - korjattu diagnostiikassa,
  mutta koska lopputulos (etumerkki) osuu samaan epävarmaan kategoriaan yllä, ei
  viety koodiin.
- **JOHTOPÄÄTÖS/SUOSITUS**: ainoa luotettava tapa vahvistaa Puisto/Satama/Maatilan
  radat lopullisesti on ajaa peliä Unityssä ja katsoa osuvatko esteet/tehosteet/
  tähdet oikeasti näkyvälle tielle (sama testi joka ratkaisi Huvipuiston 8.8.).

**Koristeiden värit/materiaalit (VÄRIT)**: käyty läpi kaikki 40 Puisto/Huvipuisto/
Satama-koristemallia Blenderissä (venemallit + Majakka/Nosturi/Varasto jo tarkistettu
aiemmassa istunnossa). Kaikki käyttävät oikeaa yhden-materiaalin+tekstuurin
("palettiatlas") -käytäntöä kuten pitääkin - EI samaa "litteä harmaa, ei tekstuuria"
-bugia joka löytyi ja korjattiin Maatilan koristeista aiemmin. Ei toimenpiteitä.

**Koristeiden asettelu/rotaatio (ASETTELUT/ROTAATIOT)**: `PakettiporinaDecor.cs`:n
140 DecorItem-sijaintia (Maatila 58, Puisto 30, Satama 25, Huvipuisto 27) käyty läpi
ohjelmallisesti: parittainen etäisyystarkistus + rotX-yhtenäisyystarkistus. Ei
todellisia päällekkäisyyksiä (kaikki <2,5 yksikön parit ovat tarkoituksellisia -
esim. suihkulähde+4 kukkapenkkiä samalla pivotilla eri rotY:llä, aitaus+eläin
tarhan sisällä). Kaikki 140 kohdetta käyttävät johdonmukaisesti rotX=-90 kuten
Blender-bmesh-pipelinen konventio vaatii - ei poikkeamia. Tämä koodipuoli on siis jo
erittäin huolellisesti käsin auditoitu (ks. kommentit tiedostossa) - ei uusia
korjauksia tarvittu.

**Mekaniikat/toiminnot (MEKANIIKAT/TOIMINNOT) - scenekohtainen synkronointitilanne**
tarkistettu suoraan tallennettujen .unity-tiedostojen YAML:sta (ei vain koodista):
- `Game.unity`: GameManager ✅, uudet tyylitellyt DirtMeterBar/DentMeterBar +
  dirtMeterLabel/dentMeterLabel ✅ (17.8. tyylipäivitys NÄKYY jo pelissä täällä).
- `Puisto.unity` / `Satama.unity` / `Huvipuisto.unity` / `Maatila.unity`:
  GameManager ✅, Laatikko-esteet ✅, MUTTA dirtMeterLabel/dentMeterLabel PUUTTUU
  kaikista neljästä - 17.8. HUD-tyylipäivitys ei ole vielä näissä. **Aja `Korjaus/01`
  uudelleen näissä neljässä scenessä** jotta uudet lika-/kolhupalkit näkyvät siellä.
- `Wash.unity`: DentSpots ✅, RepairArea ✅, DentsText ✅, EI enää RepairPanel/
  RepairButton-jäännöstä - 16.8. `Korjaus/03`-korjaus ON ajettu, korjausvaihe siis
  toimii Pesussa.
- `Kyla.unity`: sama tyhjä runko kuin 14.8./16.8. - `GroundCollinder`-kirjoitusvirhe
  YHÄ paikallaan, ei autoa/maastoa/ääniä. Vanhin ratkaisematon avoin kysymys, ei
  käsitelty tässä auditissa (vaatii oman erillisen "viedäänkö loppuun vai
  hylätäänkö" -päätöksen käyttäjältä, ks. Avoimet kysymykset -osio).

**Efektit (EFEKTIT)**: Latakko/Boost/Kartio/Laatikko-mallien rotaatiot ja
näkyvyys tarkistettu jo 17.8. istunnossa (ks. yllä) - ei uutta tässä auditissa.
Visuaalinen "mehukkuus" (partikkelit/squash-stretch, M6.4) on edelleen tekemättä,
ei osa tätä auditia (ks. Suositeltu seuraava askel -kohta 6).

## 18.8.2026 (jatko 2) — Kartio/Boost/Laatikko kyljellään + Game-lätäköt väärin + HUD-koko

Käyttäjä testasi yllä olevan auditin jälkeen Unityssa ja raportoi kolme asiaa:

- **"Lätäköt aseteltu väärin Game scenessä"**: tarkistettiin suoraan Game.unity:n
  tallennettu Latakko_02-sijainti (X=-20, Z=-50) - täsmäsi TÄSMÄLLEEN tämän session
  aiemman Game-bends-korjauksen tuottamaan arvoon. Tämä on siis SUORA todiste
  Unitysta että kyseinen korjaus oli väärä suunta - sama epäluotettavuus jonka
  Huvipuiston 8.8. historia jo osoitti (ks. yllä). **Game-radan bends PERUTTU
  takaisin alkuperäiseen etumerkkiin** (`(-100,0,20),(0,100,-20)`),
  `PakettiporinaTracks.cs`. Alkuperäinen "asettelut ja rotaatiot pielessä"
  -valitus selittyy todennäköisemmin alla olevalla kyljellään-bugilla, ei
  bends-etumerkillä.
- **"Kartiot ja boostit näyttävät olevan kyljellään"**: LÖYTYI JA KORJATTU, todellinen
  bugi. Game.unity:n tallennettu Kartio_01/Boost_01/Laatikko_01-data paljasti että
  jokaisella on `m_LocalRotation` vastaten -90° X-kiertoa - tämä tulee Kartio.fbx/
  Boost.fbx/Laatikko.fbx-prefabien OMASTA tuontitransformista (Unityn FBX-tuonti),
  ei mistään koodista. `PakettiporinaElements.cs`:n `PlaceCone`/`PlaceConeXZ`/
  `PlaceBoost`/`PlaceBoostXZ`/`PlaceLaatikko`/`PlaceLaatikkoXZ` eivät KOSKAAN
  asettaneet `transform.rotation`:ia - joten jokainen luotu objekti peri prefabin
  kyljelleen kaatuneen oletuskierron. Latakko oli aina kunnossa koska
  `PlacePuddle` asettaa oman rotaationsa eksplisiittisesti. **Korjattu**: kaikki 6
  funktiota pakottavat nyt `transform.rotation = Quaternion.identity` joka ajolla.
  **EI VIELÄ UNITY-TESTATTU** - aja `Lisäys/02` uudelleen jokaisessa ratascenessä.
- **HUD-koko**: Canvasin "Header" siirretty hieman alemmas (uusi `Korjaus/01`-
  askel, kohdearvo Y=-30, absoluuttinen jotta uudelleenajo on turvallinen).
  Lika-/kolhupalkit suurennettu ~1,4x (190x22 -> 260x34), fontti 13 -> 17.
  `PakettiporinaSetup.cs`. **EI VIELÄ UNITY-TESTATTU** - aja `Korjaus/01`
  jokaisessa ratascenessä.

## 18.8.2026 (jatko 3) — Lika-/kolhumittarin toimintaketju varmistettu + oma kolhu-sprite

Käyttäjä pyysi varmistamaan että lika-/kolhumittari toimii (esteosumat vaikuttavat
siihen, mittari toimii kertoimena pesun/korjauksen kestolle) ja antamaan kolhuille
oman kuvituksen (bubbles/pesu OK, kolhut käyttivät vielä uudelleenväritettyä
likatahra-spriteä).

- **Mittariketju tarkistettu koodista päästä päähän, EI bugeja löytynyt**:
  `Puddle.cs`/`Cone.cs`/`Laatikko.cs` kutsuvat oikein `GameManager.AddDirtHit()`/
  `AddDent()` osumasta, mittari kasvaa `HIT_MULTIPLIER=2`:lla (katto DIRT_MAX=15/
  DENT_MAX=6), `WashScreen.Start()` lukee mittarin suoraan ja asettaa sen mukaan
  kuinka monta likatahraa/kolhua pitää napauttaa pois - eli enemmän osumia = enemmän
  napautettavaa = pesu/korjaus kestää oikeasti pidempään. Ketju on siis jo täysin
  toimiva koodissa.
- **Kolhuille oma sprite**: luotu kaksi uutta ikonia (`pakettiporina_kolhu_a/b.png`,
  `Art/UI/Dirt/`) samalla tekniikalla kuin lika-blobit mutta kylmällä harmaa-
  sinisellä värillä + vaalealla kiiltoreunalla ja tummalla varjolla (metallikolhun
  näköinen, ei enää ruskea mutatahra). `PakettiporinaWashSetup.cs`: DentSpot-merkit
  käyttävät nyt näitä (vuorotellen) väripäällystyksen sijaan; lisätty myös
  päivitysaskel joka vaihtaa spriten JO OLEMASSA OLEVIIN DentSpot-objekteihin (ei
  vain uusiin) kun `Korjaus/03` ajetaan uudelleen. RepairTool (korjaustyökalu-ikonit)
  oli tässä vaiheessa vielä 16.8. vasara-siluetti koodissa - **katso alla oleva
  18.8. (jatko 4) -korjaus, jossa selvisi ettei tämäkään koskaan päivittynyt
  käyttäjän oikeaan sceneen.**
  **EI VIELÄ UNITY-TESTATTU** - aja `Korjaus/03` Pesu-scenessä.

## 18.8.2026 (jatko 4) — RepairTool "oranssi neliö" -bugi, oikea syy ja korjaus

Käyttäjä tarkensi edellistä pyyntöä: "kun saippuakuplia poksautellaan wash
scenessä niin saippuakuplia vastaavia kolhujen tilalle kun niitä kuvaa oranssi
neliö" - eli RepairTool-napautusikonit (korjausvaiheen "kuplat") näkyivät
pelaajalle pelkkänä oranssina neliönä, ei vasarana (16.8. yritys) eikä mitään
muutakaan järkevää.

**Juurisyy löytyi**: `PakettiporinaWashSetup.cs`:n RepairArea+RepairTool-luonti
ajetaan VAIN kun `wash.repairArea == null` - eli JOS RepairArea oli jo olemassa
scenessä (käyttäjän scenessä oli, paljon vanhemmasta ajosta ajalta ennen 16.8.
vasara-yritystä), koko luontikoodi (myös 16.8. vasarapäivitys) ohitettiin
kokonaan eikä KOSKAAN päivittänyt olemassa olevia RepairTool-ikoneja - ne jäivät
aivan alkuperäiseen, kaikkein ensimmäiseen placeholder-ulkoasuun (värillinen
neliö). Sama bugiluokka kuin DentSpotsilla yllä, mutta tässä paljasti ettei edes
16.8. korjaus ollut koskaan oikeasti näkynyt pelaajalle.

**Korjattu kunnolla nyt**: käyttäjän tarkennuksen mukaisesti RepairTool käyttää
nyt PYÖREÄÄ "korjauskupla"-spriteä (`pakettiporina_korjauskupla_a/b.png`,
`Art/UI/Bubbles/`) - SAMA tekniikka/rakenne kuin oikeat saippuakuplat
(`pakettiporina_kupla_a/b/c.png`, sama SVG-pohjainen radiaaligradientti +
sateenkaarireunaviiva + kiiltopiste), mutta lämpimällä kupari/oranssi paletilla
ja pienellä ruuvinpää-glyfillä keskellä erottumaan selvästi saippuakuplasta.
`PakettiporinaWashSetup.cs`: RepairTool rakennetaan nyt yhdellä Imagella (ei enää
erillisillä Handle/Head-lapsilla), ja lisätty päivitysaskel joka korvaa JO
OLEMASSA OLEVIEN RepairTool-ikonien spriten/värin (ja poistaa vanhat Handle/Head-
jäänteet jos löytyy) aina kun `Korjaus/03` ajetaan - ei enää "vain jos puuttuu
kokonaan" -rajoitusta. `RepairTool.cs`: himmennyslogiikka (`SetAllAlpha`)
päivitetty kohdistumaan suoraan omaan Imageen entisten lapsikuvien sijaan.
**EI VIELÄ UNITY-TESTATTU** - aja `Korjaus/03` Pesu-scenessä.

## 18.8.2026 (jatko 5) — Boost/Latakko-sijainti todistettu oikeaksi, näkyvyys silti rikki

Käyttäjä ajoi `Lisäys/02` Puistossa (Puisto-bendsien etumerkkikorjauksen
jälkeen) ja liitti täyden `PakettiporinaSceneDump`-taulukon: kaikilla
Boost/Latakko-objekteilla `EtaisyysKeskilinjasta` = **0,00** — eli X/Z-sijainti
on nyt todistetusti täsmälleen tien keskilinjalla, ei enää 27 yksikköä
väärässä paikassa kuten edellisessä dumpissa (joka osoittautui vanhaksi,
ennen `Lisäys/02`:n uudelleenajoa otetuksi). Sijoittelubugi on siis oikeasti
korjattu Puistossa.

**Silti käyttäjä raportoi: "EI AUTTANUT... BOOSTIT JA LÄTÄKÖT EI NÄY RADALLA
KUN PELAA"** — eli jäljellä on ERILLINEN näkyvyysbugi, ei sijaintibugi.
Käytiin läpi kaikki koodissa mahdolliset syyt: `PakettiporinaElements.cs`:n
Place-funktiot eivät koskaan aseta `gameObject.layer`:ia eivätkä poista/
disabloi Riendereriä; `Gameplay/`-skripteissä ei ole yhtään `SetActive`/
`enabled=false`-kutsua joka koskisi Boost/Latakko/Kartio/Laatikko-objekteja
(tarkistettu `WashScreen.cs`, `Pickup.cs`, `TrackBubble.cs` - kaikki koskevat
vain Pesu-scenen omia olioita). Ei siis löytynyt koodibugia joka piilottaisi
ne suoraan.

**Ongelma: vanha `PakettiporinaSceneDump` ei voinut koskaan paljastaa
näkyvyysbugia**, koska `FindObjectsOfType<Transform>(true)` listaa myös
pois päältä kytketyt objektit IDENTTISILLÄ luvuilla kuin päällä olevat -
sijainti näyttää "oikealta" konsolissa vaikka objekti olisi täysin
näkymätön pelissä (esim. `GameObject.activeSelf=false`, `Renderer.enabled=
false`, tyhjä `sharedMaterial`, tai objektin `Layer` puuttuu Kameran
`Culling Mask`ista - näistä mikään ei näy vanhassa taulukossa).

**Korjattu `PakettiporinaSceneDump.cs`:ään 4 uutta saraketta** jokaiselle
riville: `Aktiivinen` (`activeInHierarchy`, huomioi myös vanhempien
pois-päältä-kytkennän), `RendererPaalla` (`Renderer.enabled`), `Layer`
(GameObjectin kerrosnimi - voi olla FBX-prefabiin leivottu ilman että
mikään koodi on sitä koskaan asettanut), `Materiaali` (sharedMaterialin
nimi, tai "PUUTTUU" jos tyhjä = näkymätön/pinkki objekti). Lisäksi taulukon
alkuun rivi joka näyttää Main Cameran `Culling Mask`in sisällön - jos
Boost/Latakko-objektin Layer ei ole tässä listassa, se ei koskaan piirry
ruudulle vaikka kaikki muu (sijainti, aktiivisuus, Renderer, materiaali)
olisi täysin kunnossa.

**EI VIELÄ UNITY-TESTATTU** - odottaa käyttäjän uutta `Tyokalut/03 - Vie
scenen elementit`-ajoa Puistossa, jotta nähdään mikä näistä neljästä
(Aktiivinen/RendererPaalla/Layer/Materiaali/CullingMask) on todellinen
juurisyy.

## 18.8.2026 (jatko 6) — Kaikki muu poissuljettu paitsi maaston pintakorkeus

Käyttäjä ajoi päivitetyn `Tyokalut/03`-dumpin kaikissa neljässä scenessä
(Puisto, Maatila, Huvipuisto, Game). Tulos: **kaikki uudet sarakkeet olivat
kunnossa joka ikisellä Boost/Latakko/Kartio/Laatikko-objektilla** —
Aktiivinen=True, RendererPäällä=True, Layer=Default (kameran CullingMaskissa),
materiaali asetettu (ei "PUUTTUU"). Laskin myös kaikki X-sijainnit käsin
uudelleen jokaiselle objektille jokaisessa scenessä — kaikki täsmäsivät
`RoadX(z)`-kaavaan (Maatila ja Game olivat jo alunperin oikein, Huvipuiston
boostXZ-kiinnitykset toimivat kuten pitää).

Kysyin käyttäjältä tarkalleen mitä "ei näy" tarkoittaa: vastaus oli
**"PELISSÄ TAI RADALLA EI NÄY BOOSTIA TAI LÄTÄKKÖÄ MUTTA PELI REAGOI JOS
SELLAISEEN OSUU ÄÄNELLÄ JA MUULLA ASIAAN KUULUVALLA TAVALLA"** — eli
törmäys/ääni/pelilogiikka toimivat täysin normaalisti, vain visuaalinen
piirto puuttuu kokonaan. Tämä sulkee pois kaiken sijainti-, aktiivisuus-,
Layer- ja materiaalisyyn.

**Huomattu tärkeä yksityiskohta**: käyttäjä on KOKO tämän keskustelun ajan
valittanut VAIN Boost/Lätäkkö-objekteista — ei kertaakaan Kartiosta tai
Laatikosta. Boost istutetaan Y=0.05:een ja Lätäkkö Y=0.03:een (litteä
pintapäällyste-tyylinen taso), kun taas Kartio/Laatikko ovat Y=0.4:ssä
(paljon korkeampia, seisovia esteitä). Tämä sopii täydellisesti yhteen
teorian kanssa että **Maaston VISUAALINEN pinta ei olekaan tasan Y=0
kohdassa jossa Boost/Lätäkkö luullaan istutettavan** — GroundCollider
(fysiikka, auton ajopinta) on ERI, yksinkertainen tasainen laatikko joka ei
välttämättä vastaa Maaston oman meshin todellista pintakorkeutta (esim. jos
tiessä on pieni reunakoroke/kupu Blenderin puolesta) - jos Maaston pinta on
todellisuudessa esim. 0.1-0.3 yksikköä Y=0:n yläpuolella, ohuet Boost/
Lätäkkö-tasot HAUTAUTUISIVAT näkymättömiin maaston pinnan SISÄLLE samalla
kun GroundCollider (fysiikka) pysyisi silti toimivana täysin erillisenä
laatikkona - selittäisi TÄSMÄLLEEN havaitun oireen (näkymätön mutta
törmäys/ääni toimii).

**Lisätty `PakettiporinaSceneDump.cs`:ään suora testi tälle**: työkalu
etsii nyt scenen Maasto-objektin presetin `maastoName`:n perusteella,
lisää siihen väliaikaisen `MeshCollider`in (jos sillä ei jo ole Collideria;
poistetaan lopuksi ettei jää pysyvästi), ja rykäisee pystysuoraan alaspäin
jokaisen Boost_/Latakko_/Kartio_/Laatikko_-objektin kohdalta - uudet
sarakkeet `MaastonPintaTassa(Y)` ja `PystyEroMaastoon` (objektin oma Y
miinus maaston oikea pintakorkeus siinä kohtaa) kertovat suoraan onko
objekti maan alla, maan päällä oikein, vai kellumassa liian korkealla.

**EI VIELÄ UNITY-TESTATTU** - odottaa käyttäjän uutta `Tyokalut/03`-ajoa.
Jos `PystyEroMaastoon` on selvästi negatiivinen (esim. -0.1 tai pienempi)
Boost/Latakko-riveillä, teoria on vahvistettu ja korjaus on nostaa niiden
Y-sijaintia `PakettiporinaElements.cs`:ssä sen verran että ne selvästi
clearaavat maaston oikean pinnan (ei enää kiinteä 0.05/0.03, vaan maaston
pinta + pieni marginaali).

## 18.8.2026 (jatko 7) — Juurisyy VAHVISTETTU: maaston pintakorkeus vaihtelee, korjattu

Käyttäjä ajoi korjatun korkeustestin Puistossa (ensimmäinen yritys epäonnistui
koska `maastoGo`-haku käytti `GetRootGameObjects()`:ia eikä Maasto ollutkaan
scenen ylimmällä tasolla - korjattu hakemaan kaikista objekteista kuten
päädumppikin tekee). Tulos vahvisti teorian täsmälleen:

- `PystyEroMaastoon` vaihteli -0.05:sta +0.05:een Boost/Latakko-riveillä -
  osa (esim. `Boost_01/03/05`: -0.01, `Latakko_03`: -0.05) oli suoraan
  maaston pinnan ALLA, loput vain 1-5 senttiä sen yläpuolella.
- Boost/Latakko-mesh on itsessään vain ~0.03 yksikköä paksu - näin pieni
  marginaali (tai negatiivinen) riittää hautaamaan koko objektin näkymättömiin
  maaston pinnan sisään minkä tahansa pienen paikallisen korkeusvaihtelun takia.
- Kartio/Laatikko (Y=0.4) saivat aina 0.34-0.40 marginaalin - siksi niistä ei
  koskaan tullut valitusta koko keskustelun aikana.

**Juurisyy vahvistettu**: `PakettiporinaElements.cs` istutti Boost/Latakko-
objektit kiinteälle Y=0.05/0.03-tasolle olettaen Maaston pinnan olevan tasan
Y=0 kaikkialla - todellisuudessa Maaston mesh-pinta vaihtelee muutaman sentin
verran radan matkalla (peräisin Blender-generaattorista/mesh-tarkkuudesta).
`GroundCollider` (auton ajofysiikka) on kokonaan erillinen, aina tasainen
laatikko - se ei koskaan ollut sidoksissa Maaston oikeaan pintaan, minkä
vuoksi törmäys/ääni toimivat normaalisti riippumatta siitä oliko Boost/
Latakko haudattuna näkymättömiin vai ei.

**Korjattu `PakettiporinaElements.cs`**: `AddElements()` etsii nyt scenen
Maasto-objektin, lisää siihen väliaikaisen `MeshCollider`in (poistetaan
lopuksi), ja jokainen `PlaceBoost/PlaceBoostXZ/PlacePuddle/PlaceCone/
PlaceConeXZ/PlaceLaatikko/PlaceLaatikkoXZ` käyttää uutta `GroundY()`-apuria
joka rykäisee oikean maaston pintakorkeuden kyseisessä XZ-kohdassa ja lisää
siihen marginaalin (Boost/Latakko +0.08, Kartio/Laatikko +0.4 kuten ennenkin
mutta nyt oikean pinnan päälle) - ei enää kiinteää, olettamukseen perustuvaa
Y-arvoa. Jos Maastoa ei löydy, käytetään vanhaa kiinteää arvoa fallbackina
(ei riko mitään jos jokin scene poikkeaa nimikäytännöstä).

**EI VIELÄ UNITY-TESTATTU** - odottaa käyttäjän `Lisäys/02`-ajoa jokaisessa
radassa (Puisto, Maatila, Huvipuisto, Game - Satama on jo entuudestaan
toiminut, ei kiireellinen mutta kannattaa ajaa senkin varmuuden vuoksi) ja
pelitestausta että Boost/Lätäkkö nyt näkyvät.

## 19.8.2026 (jatko 8) — Terrain-korkeuskorjaus VAHVISTETTU 3/4 radalla, Puisto oma juurisyy + linear velocity -varoitusspam korjattu

Käyttäjä ajoi `Lisäys/02`:n ja pelitesti kaikki 4 aiemmin rikkinäistä rataa.
**Maatila, Huvipuisto ja Game: VAHVISTETTU KORJATUKSI** - Boost/Lätäkkö näkyvät
nyt normaalisti (jatko 7:n terrain-korkeuskorjaus toimi täsmälleen suunnitellusti).

**Puisto oli AINOA joka jäi yhä rikki**, vaikka sama korjaus ajettiin sielläkin
ja tuotti järkeviä, maaston-suhteellisia Y-arvoja (esim. Boost_01 Y=0.14, sama
luokkaa kuin muissakin radoissa). Koska korkeusmekanismi siis toimi Puistossakin,
syy ei voinut enää olla korkeus - jäljelle jäi vain yksi Puistolle UNIIKKI muutos:
18.8. samana päivänä tehty **bends-amplitudin etumerkin kääntö** (ks. jatko-osiot
ja `PakettiporinaTracks.cs`), joka tehtiin SAMAAN "ei näy" -valitukseen vedoten
mutta PELKÄLLÄ generaattori-Huvipuisto-analogialla - ei koskaan käsin
Unity-vahvistettu Puistossa itsessään (toisin kuin Huvipuiston 8.8. flippi tai
Gamen 18.8. peruutus, jotka molemmat perustuivat suoraan käyttäjän Unity-
havaintoon). Koska Maatila/Huvipuisto/Game (ei bends-flippiä) korjaantuivat
suoraan korkeuskorjauksella mutta Puisto (ainoa jolle myös bends käännettiin)
ei, päätelmä: **bends-flippi oli virheellinen** - se työnsi Boost/Latakko-
objektit tien väärälle puolelle (nyt näkyvinä muttei ajettavalla tiellä), mikä
näyttäisi edelleen "ei näy/erotu" -oireelta pelissä.

**Korjattu**: `PakettiporinaTracks.cs`, Puisto-TrackDef - `bends` palautettu
takaisin alkuperäiseen, generaattoria vastaavaan arvoon:
`{ (-120f,-30f,18f), (30f,120f,-18f) }` (oli `{ (-120f,-30f,-18f), (30f,120f,18f) }`).
**EI VIELÄ UNITY-TESTATTU** - vaatii `Lisäys/02`:n uudelleenajon Puistossa ja
pelitestin.

**Linear velocity -varoitusspam korjattu**: konsoli täyttyi kymmenillä
"Setting linear velocity of a kinematic body is not supported" -varoituksilla
jokaisen lähdön odotusajan (countdown) aikana. Syy: `RaceManager.FreezeCar(true)`
asettaa `car.isKinematic = true` odotusajaksi, mutta `ArcadeCarController.
FixedUpdate()` yritti silti kirjoittaa `rb.velocity`:a joka fysiikkaruudulla
(rivit 103 ja 125) - Unity ei salli tätä kinemaattiselle Rigidbodylle ja
kirjoittaa siitä varoituksen konsoliin joka kerta. **Korjattu**: `FixedUpdate()`
palaa heti alussa jos `rb.isKinematic`, ja `Bump()` samoin - autoa ei tarvitse
simuloida lainkaan kesken jäädytyksen. EI VIELÄ UNITY-TESTATTU.

**19.8.2026, jatko:** käyttäjä vahvisti Puiston lätäköt korjatuiksi. Uusi
valitus: "The referenced script (Unknown) on this Behaviour is missing!"
jokaiselle objektille pelin käynnistyessä - Unity ei koskaan kerro TÄSSÄ
varoituksessa mikä objekti on kyseessä. Lisätty uusi diagnostiikkatyökalu
`PakettiporinaMissingScripts.cs` (Pakettiporina/Tyokalut/05) joka käy koko
avoimen scenen läpi (myös inaktiiviset objektit) ja tulostaa jokaisen
GameObjectin tarkan hierarkiapolun jolla on puuttuva scripti-komponentti.
Odottaa käyttäjän ajoa jokaisessa scenessä jossa varoitus esiintyy, jotta
nähdään TARKALLEEN mitkä objektit (todennäköisesti vanhat/orvot jäänteet
aiemmista työkaluajoista, esim. Lisäys/10:n puhdistamat Kupla-objektit tai
vastaavat) ja voidaan joko poistaa ne tai korjata viittaus.

**19.8.2026, jatko 2 — juurisyy ja korjaus:** työkalu 06 (koko projektin
skannaus) löysi 56 rikkinäistä scripti-komponenttia, kaikki `Map/Objektit/`
-koristeissa Huvipuisto/Maatila/Satama-sceneissä (esim. Huvipuisto_
Maailmanpyora, Maatila_Navetta, Satama_Majakka - Puisto ja Game eivät kärsi).
Jäljitetty: `PakettiporinaDecor.cs`:n `AddDecor()` lisää jokaiseen koristeeseen
`PakettiporinaDecorMarker`-merkkikomponentin (kirjanpitoa Lisäys/03:n siivous-
passille ja "16 - Vie sijainnit koodiksi" -työkalulle) - luokka on YHÄ olemassa
koodissa, mutta scenen tallennettu viittaus osoittaa GUID:iin joka ei enää
täsmää mihinkään scriptiin (todennäköisesti PakettiporinaDecor.cs:n .meta
vaihtui suoran tiedostomuokkauksen seurauksena jossain kohtaa tätä sessiota,
ei Unity Editorin kautta tehty). Marker on puhdasta editor-kirjanpitoa - ei
vaikuta renderöintiin/törmäykseen/pelilogiikkaan - joten turvallisin korjaus
on poistaa pelkkä rikkinäinen komponentti koskematta itse koristeeseen.

Lisätty työkalu 07 (`Pakettiporina/Tyokalut/07 - Poista puuttuvat scriptit
KAIKISTA sceneista (KORJAA)`) joka käy kaikki scenet läpi (samalla turvallisella
additiivi-avaa/sulje-periaatteella kuin 06), poistaa rikkinäiset komponentit
`GameObjectUtility.RemoveMonoBehavioursWithMissingScript`illa ja TALLENTAA
muuttuneet scenet. HUOM: tämän jälkeen Lisäys/03:n vanhojen koristeiden
siivouspassi ei enää tunnista näitä objekteja markerin kautta, jos Lisäys/03
ajetaan joskus uudelleen näillä 3 scenellä - ei ongelma juuri nyt. EI VIELÄ
UNITY-TESTATTU - odottaa käyttäjän ajoa.

**19.8.2026, jatko 3 — työkalu 07 EI TOIMINUT, todellinen juurisyy löytyi ja
korjattu suoraan scene-tiedostoista:** kolme peräkkäistä Editor-API-yritystä
(GameObjectUtility, SerializedObject m_Component-manipulaatio, Prefab-
instanssin purku + GameObjectUtility) epäonnistuivat kaikki - joko palauttivat
0 tai raportoivat väärin onnistuneensa ilman että scene-tiedosto oikeasti
muuttui (todistettu: 06 löysi täsmälleen samat luvut uudestaan pelitestien
jälkeen, myös Puisto - jossa koristeet oli ilmeisesti lisätty vasta äskettäin,
26/9/2026 selvisi Puiston 23:n olevan sama Lisäys/03-koristeongelma).

**Oikea juurisyy löytyi suoraan .unity-tiedostojen YAML:sta**: jokaisen
rikkinäisen `PakettiporinaDecorMarker`-komponentin `m_Script`-kenttä osoitti
paljaaseen paikalliseen fileID:hen ilman guid:ia (esim.
`m_Script: {fileID: 1464767556}`) - EI ulkoiseen script-assettiin `guid:`illa
kuten pitäisi (`{fileID: 11500000, guid: <32-merkkinen-hex>, type: 3}`).
Jokaisessa sceneessä tämä paljas fileID osoitti YHTEEN tyhjään/paikalliseen
`MonoScript`-tynkäobjektiin (`!u!115`) SAMASSA tiedostossa - Unityn sisäinen
"tuntematon skripti" -sijaisobjekti, joka syntyy kun alkuperäinen guid-
viittaus lakkaa resolvoitumasta. Kaikki muut samalla tavalla (`AddComponent`
Prefab-instanssin jälkeen) lisätyt komponentit (esim. Star-tähden pickup/
pyöritys-skriptit) tarkistettiin ja niiden `m_Script`-guid:t olivat kunnossa -
ongelma rajoittuu yksinomaan `PakettiporinaDecorMarker`-instansseihin.

**Korjattu suoraan tiedostoista** (ei Unity Editorin kautta - `sed`
tekstikorvaus jokaiselle scenelle sen omalla paikallisella tynkä-fileID:llä):
`m_Script: {fileID: <scenen oma tynkä-ID>}` -> `m_Script: {fileID: 11500000,
guid: e7dde9f8ce0769e46a90e134128a9489, type: 3}` (PakettiporinaDecorMarkerin
oikea guid, `PakettiporinaDecor.cs.meta`:sta). Korjattujen määrä RIVEITTÄIN
(enemmän kuin 06:n runtime-skannaus näytti, koska osa oli menettänyt myös
`modelName`-kenttänsä eikä sen vuoksi näkynyt kaikissa ajonaikaisissa
tarkistuksissa yhtä johdonmukaisesti - kaikki oli silti aidosti liitettynä
oikeisiin GameObjekteihin, ei orpoa/käyttämätöntä dataa):
- Puisto.unity: 30 korjattu
- Huvipuisto.unity: 27 korjattu
- Maatila.unity: 57 korjattu
- Satama.unity: 25 korjattu
- Yhteensä 139 riviä korjattu neljässä tiedostossa.

Rivimäärät (`wc -l`) pysyivät identtisinä ennen/jälkeen - puhdas
yhden-rivin-korvaus, ei rakenteellista riskiä. EI VIELÄ UNITY-TESTATTU
(vaatii Unityn avaamisen/scenejen uudelleenlatauksen, koska tiedostot
muutettiin Unityn ulkopuolelta) - kerro jos varoitus vieläkin tulee.

**19.8.2026, jatko 4 — edellinen korjaus käytti VÄÄRÄÄ guidia, korjattu
uudelleen:** käyttäjä testasi Maatilassa ja sai TÄYSIN UUDEN, pahemman
virheryppään: `'Pakettiporina.EditorTools.PakettiporinaDecor' is missing the
class attribute 'ExtensionOfNativeClass'!` + `The class ... is abstract` +
Unity yritti jatkuvasti "Fixata" viittausta joka objektille toistuvasti.

**Syy**: `PakettiporinaDecor.cs`-tiedostossa oli KAKSI luokkaa -
`PakettiporinaDecorMarker` (MonoBehaviour) JA staattinen `PakettiporinaDecor`-
apuluokka (radan koristeiden lisäystyökalu). Kun tiedostossa on useampi
luokka, Unityn script-fileID 11500000 (oletus "pääluokka"-fileID) osoittaa
AINA siihen luokkaan jonka nimi täsmää TIEDOSTON nimeen - eli
`PakettiporinaDecor.cs`:ssä fileID 11500000 osoitti staattiseen
`PakettiporinaDecor`-luokkaan, EI `PakettiporinaDecorMarker`-komponenttiin.
Edellisen korjauksen `sed`-komento käytti `PakettiporinaDecor.cs.meta`:n
guidia (`e7dde9f8...`) olettaen sen osoittavan markeriin - väärin.

**Korjattu lopullisesti**: `PakettiporinaDecorMarker` siirretty omaan
tiedostoonsa `PakettiporinaDecorMarker.cs` (samassa Lisays-kansiossa, sama
namespace) - AINOA luokka tiedostossa, joten fileID 11500000 osoittaa nyt
oikein siihen. Uusi tiedosto + oma `.meta` uudella guidilla
(`da759860cf6242dfbbe5adb876e48395`, generoitu `uuid4`:lla). Kaikki neljä
scenetiedostoa (Puisto/Huvipuisto/Maatila/Satama) päivitetty uudelleen `sed`-
korvauksella osoittamaan tähän UUTEEN, oikeaan guidiin (fileID pysyy 11500000,
type 3). Vahvistettu ettei vanhaa guidia jää mihinkään näistä eikä muihin
sceneihin (Game/Garage/MainMenu/Wash/Kyla - ei koskaan viitanneet siihen).
`PakettiporinaDecor.cs`:stä poistettu luokka, jätetty kommentti ohjaamaan
uuteen tiedostoon.

EI VIELÄ UNITY-TESTATTU - odottaa käyttäjän scenejen uudelleenlatausta
(Unity täytyy tuoda uusi .cs-tiedosto ensin, tapahtuu automaattisesti kun
Editor saa fokuksen) ja pelitestiä jokaisessa neljässä scenessä.

## 19.8.2026, jatko 5 — progress-palkkien animointi, nopeustasojen ero levennetty, maskotti, Kylän perusta

**Progress-palkit liikkuvat nyt näkyvästi**: `RaceHUD.cs`:n `dirtMeterBar`/`dentMeterBar`
hyppäsivät aiemmin suoraan uuteen `fillAmount`-arvoon (kayttaja: "palkkien tulisi
liikkua mika näyttää muutoksen osumista"). Lisätty `Update()`-pohjainen
`Mathf.MoveTowards`-animointi (`meterAnimSpeed`, oletus 1.5 täyttöosaa/s) - ensimmäinen
scenen alun synkronointi hypätään suoraan (ei animoi tyhjästä), vain oikeat osumat
ajon aikana animoituvat.

**Nopeustasojen ero levennetty uudelleen**: `ArcadeCarController.SpeedTierMultiplier`
oli jo kerran levennetty 16.8. (0.72-1.32 -> 0.55-1.7), mutta käyttäjä ei YHÄ
huomannut eroa. Levennetty reilusti lisää: `{0.4f, 1f, 2.4f}` (~6x kerroinväli,
Keski pysyy 1.0:ssa).

**Maskotti**: ei ollut yhtään 3D-hahmoa/animaatiota/riggausta projektissa - vain
litteä 2D-avatar (`pakettiporina_vilkku_hahmo.png`, jo poseerattu käsi ylhäällä).
Käyttäjä valitsi kevyen version. Uusi `MascotAnimator.cs` (Scripts/UI/) animoi
tätä kuvaa koodilla (pomppu + heiluntakallistus + säännöllinen isompi "heilautus"-
pulssi) ja näyttää puhekupla-TEKSTIÄ (ei ääntä - äänen nauhoitus/generointi ei ole
mahdollista tässä työkalussa) tilanteeseen sopivilla riveillä. Uusi Editor-työkalu
`PakettiporinaMascot.cs` (Lisays/11) lisää/päivittää maskotin MainMenun pääruudulle
JA olemassa olevaan Asetukset-paneelin maskottiin, sekä Gareseen - EI VIELÄ
UNITY-TESTATTU, sijainti/koko saattaa vaatia hienosäätöä Inspectorista per scene
(MainMenu/Settings 800x600-referenssi, Garage 1080x1920).

**Kylän perusta rakennettu**: Kyla.unity oli tyhjä runko (ks. aiemmat M-LISTA-
merkinnät - "GroundCollinder"-kirjoitusvirhe, ei autoa, ei maastomallia, ei ääniä).
Lisätty `PakettiporinaTracks.cs`:ään "Kyla"-TrackDef (kuusi bend-segmenttiä,
amplitudi jopa 22 - eniten suunnanvaihtoja/jyrkin kaarto kaikista radoista, kuten
käyttäjä pyysi "selkeästi vaikeampi kuin muut"). Uusi Editor-työkalu
`PakettiporinaKylaSetup.cs` (Korjaus/09) korjaa kirjoitusvirheen, lisää näkyvän
(vihreän) MeshRendererin pohjalle, luo StartPointin, instantioi `Car.prefab`in ja
kytkee RaceManager.car/.startPoint + CameraFollow.target. EI VIELÄ: taloja, pihoja,
pihoissa autoja, puita, postilaatikoita tms. koristeita - käyttäjä valitsi
tietoisesti "aloita perustasta", koristeet/rakennukset ovat oma myöhempi vaihe
(samaan tapaan kuin Lisays/03 muille radoille). EI VIELÄ UNITY-TESTATTU.

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
