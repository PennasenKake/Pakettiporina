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

- **M2.11 Boost / Lätäkkö / Kartio -pelielementit** ✅ — `Scripts/Gameplay/Boost.cs`, `Puddle.cs`, `Cone.cs` + `ArcadeCarController`:n tilapäiskerroin-kerrostus (`boostMultiplier`/`gripPenaltyMultiplier`). Editor-työkalu `PakettiporinaElements.cs` (valikko 14) sijoittaa nämä radan `bends`-kaavan mukaan automaattisesti.
- **M2.12 Kolme uutta rataa (Puisto, Satama, Huvipuisto)** ✅ — omat scenet (`Scenes/Puisto.unity`, `Satama.unity`, `Huvipuisto.unity`), maastot generoitu Blenderillä (`pakettiporina_ratageneraattori.py` + tämän session laajennukset), noin 30 s ajomatka per rata.
  - ⚠️ **Kesken vielä**: kaikkien kolmen pitää olla lisättynä ja päällä **File → Build Settings → Scenes In Build**, muuten `SceneManager.LoadScene()` ei löydä niitä oikeassa buildissa (toimii silti Editorin Play-tilassa, mikä on hämännyt aiemmin). Tarkista `5 - KORJAA halli` -lokista onko vielä varoituksia.
- **M2.13 Teemakoristeet (FBX)** ✅ — `PakettiporinaDecor.cs` (valikko 15). Puisto: puita, penkki, lyhtypylväs, keinu, kaarisilta + lampi. Satama: kokonaan uudelleenrakennettu rantaviiva (vesi seuraa tien kaarretta koko matkan), laiva, laituri, majakka, poijut, nosturi, varasto, kontit. Huvipuisto: maailmanpyörä, teltta, karuselli, myyntikoju, ilmapallokaari, lippunauha, pomppulinna.
- **M2.14 Tähdet kaikille radoille** ✅ — `PakettiporinaStars.cs` (valikko 6) yleistetty toimimaan kaikilla neljällä radalla (oli aiemmin vain Rata 1:lle kovakoodattu).
- **M2.15 Radan valinta paketin kautta (`trackScene`)** ✅ — `PackageData.trackScene` + `GarageScreen.OnDrive()` päättää minkä radan lataa valitun paketin mukaan. Dormantti `TrackManager`/`TrackData`-järjestelmä (skenen-sisäinen versio) jätettiin käyttämättä tietoisesti, koska se ei vastannut toteutunutta yksi-scene-per-rata-arkkitehtuuria.

## M3 — Rakentelu + paketit ✅
Tavoite: opettava ydin — osat, ominaisuusmittarit, paketin ja auton yhteensovitus.

- **M3.1 Data** ✅ — `Scripts/Data/PartData.cs`, `Scripts/Data/PackageData.cs` (32 osaa, 10 pakettia `Data/Parts` ja `Data/Packages` -kansioissa).
- **M3.2 Laskenta** ✅ — `Scripts/Data/CarStats.cs`, `Scripts/Builder/CarBuilder.cs`.
- **M3.3 Sopivuus** ✅ — `Scripts/Builder/FitChecker.cs`.
- **M3.4 Halli-UI** ✅ *(oli merkitty 🔨, on nyt valmis)* — `Scripts/UI/GarageScreen.cs`: osakategoriat, osavalinta, mittaripalkit, sopivuusmerkki, pakettilista, "Aja keikka", osien ja pakettien pistelukko (`unlockPoints`/`IsLocked`/`IsPackageLocked`).
- **M3.5 Kytkentä ajoon** ✅ *(oli merkitty ⬜, on itse asiassa valmis — koodissa jopa merkitty "M3.5b")* — `Scripts/Gameplay/RaceSetup.cs`: voima→kiihtyvyys, pito→pito, keveys→huippunopeus+kääntyminen, paketin massa vaimentaa suorituskykyä. Kaikki arvot rajattu turvalliselle välille lapsipeliä ajatellen. Palkkiopisteet: `RaceManager` → `GameManager.AddPoints`.
- **M3.6 Salaiset osat ("paras mahdollinen auto")** ✅ *(uusi, ei alkuperäisessä listassa)* — `PartData.secret`-lippu + `GarageScreen.BuildLookups()` piilottaa osan kokonaan osaselaimesta kunnes pisteraja täyttyy (ei vain "lukossa"-tekstiä — osa puuttuu listasta kokonaan ja ilmestyy yllätyksenä). 6 kultaista osaa (`PakettiporinaSecretParts.cs`, valikko 16), yhdessä nostavat kaikki 5 mittaria tasan sataan.

## M4 — Huoltoasema (pesu) ✅
Tavoite: maalin jälkeen pesu saippuakuplilla.

*Toteutui eri luokkanimillä kuin alkuperäinen suunnitelma ("CarCareController"
jne. eivät koskaan syntyneet), mutta tavoite on saavutettu ja pesu toimii.*

- **M4.1–M4.2 Pesun kulku** ✅ — `Scripts/Gameplay/WashScreen.cs` (ei erillistä `CarCareController`/`RepairStep`-vaihejakoa — pesu on oma suora scene `Wash.unity` maalin ja hallin välissä).
- **M4.3 Korjausvaihe** ⬜ — kolhujen erillistä korjaus-napautusta ei ole toteutettu; pesu keskittyy vain puhdistukseen.
- **M4.4 Kuplien poksautus + puhtausmittari** ✅ — `Scripts/Gameplay/Bubble.cs` + `BubbleArea`, `ProgressText` ("Poksautit X/8 kuplaa!").
- **M4.5 Pisteytys** ✅ — palkkiopisteet tulevat radalta (`RaceManager`/`GameManager`), ei erillistä `ScoreManager`-luokkaa pesussa.
- **M4.6 UI + Tauko** ✅ — `DonePanel`, `PauseMenu` pesuscenessä. *(Tässä sessiossa korjattu bugi: `DonePanel` ja `PausePanel` olivat tallentuneet scenessä oletuksena PÄÄLLÄ, jolloin "Kiiltävä auto!" -valmisteksti näkyi heti pesun alussa. `PakettiporinaWashSetup.cs` piilottaa nyt kummatkin automaattisesti KORJAA-tilassa — aja `10 - KORJAA pesu`.)*
- **M4.7 CarPainter — hallin väri näkyy autossa** ✅ *(uusi rivi, ei alkuperäisessä listassa mutta liittyy suoraan M4:n "maalin jälkeen"-ajatukseen)* — `Scripts/Vehicle/CarPainter.cs`. Tässä sessiossa löytyi ja korjattiin juurisyy miksi väri ei koskaan näkynyt: `Car.prefab`:n korin materiaali osoitti rikkinäiseen guidiin.

## M5 — Silmukka + meta 🔨
Tavoite: tallennus, eteneminen, tarrat, näkymäkierto, lapsiturva.

- **M5.1 SaveManager + SaveData** ✅ — `Scripts/Core/SaveManager.cs`: JSON-tallennus (`Application.persistentDataPath`), pisteet + valittu paketti + valitut osat. Singleton, lataa `Start()`:issa (tietoisesti ei `Awake()`:ssa, ks. tiedoston oma kommentti Script Execution Orderista).
- **M5.2 SettingsManager** ⬜ *(ei toteutettu — tarkistettu suoraan: tiedosto on tyhjä `Start()`/`Update()`-tynkä, sama tilanne kuin `PakettiporinaMitat.cs`:llä oli)* — ei ohjaustapa/kieli/äänivalintoja vielä.
- **M5.3 AudioManager** 🔨 — `Scripts/Core/AudioManager.cs` on olemassa ja siinä on kentät (esim. `bubblePop`, Boost/Lätäkkö/Kartio-äänet), MUTTA yhtään AudioClipiä ei ole raahattu niihin — peli on tällä hetkellä käytännössä äänetön keskeisissä hetkissä. **Tämä on todennäköisesti suurin yksittäinen viimeistelyaukko juuri nyt** (ks. keskustelu pelin "juicesta" — ääni + pieni visuaalinen palkitseminen on halvin iso parannus).
- **M5.4 SavedCarsManager** ⬜ *(ei erillisenä luokkana)* — auton muistaminen keikkojen välillä toteutui `SaveManager`:in kautta suoraan, joten tavoite on käytännössä saavutettu ilman erillistä luokkaa. Ei tallia/useaa suosikkiautoa.
- **M5.5 ProgressionManager + UnlockManager** ✅ *(ei erillisenä luokkana, mutta toiminnallisuus on olemassa)* — `PartData.unlockPoints`/`secret`, `PackageData.unlockPoints`, logiikka suoraan `GarageScreen`:issa (`IsLocked`, `IsPackageLocked`). Radat avautuvat paketin kautta (ks. M2.15).
- **M5.6 AchievementTracker** ✅ *(sulautunut sticker-järjestelmään, ei erillistä trackeria)* — tarrojen avautuminen = `StickerData.unlockPoints` vs. `GameManager.Points`, tarkistetaan `StickerPanel.cs`:ssä.
- **M5.7 StickerData + DistrictData** 🔨 — `StickerData` ✅ valmis, **36 tarraa** (oli 24, tässä sessiossa lisättiin 12 uutta: pöllö, perhonen, kissa, kukka, jalkapallo, lumihiutale, pilvi, leijona, kitara, avain, komeetta, kello). `DistrictData` ei ole toteutunut — kaupunginosia ei ole, radat toimivat sen sijaan alueina.
  - ⚠️ StickerGrid-UI:ssa on tällä hetkellä vasta **24 nimettyä solua** — 12 uusinta tarraa eivät näy pelissä ennen kuin gridiin lisätään loput solut.
- **M5.8 Näkymät & ScreenManager** ⬜ *(arkkitehtuuri toteutui kokonaan eri tavalla)* — `ScreenManager.cs` on tyhjä käyttämätön tynkä (tarkistettu suoraan tiedostosta), eikä HomeScreen/MapScreen/StickerBookScreen/BottomNav-mallia ole rakennettu. Sen sijaan peli navigoi suoraan `SceneManager.LoadScene()`-kutsuilla erillisten scenejen välillä (MainMenu → Garage → Game/Puisto/Satama/Huvipuisto → Wash → Garage). Tämä toimii, mutta on syytä tietoisesti hyväksyä arkkitehtuurivalintana eikä keskeneräisyytenä — jos yhden-scenen navigointimalli halutaan joskus, se on iso myöhempi työ.
- **M5.9 ParentalGate + Boot-scene** ⬜ — ei toteutettu. Tärkeä ennen Google Play -julkaisua (ks. M7).

## M6 — Kiillotus 🔨
Tavoite: ulkoasu, ääni, mehukkuus, lokalisointi, suorituskyky.

- **M6.1 Ulkoasu** 🔨 — Vilkku-maskotti (`Scripts/UI/VilkkuController.cs`) on jo mukana päävalikossa ja hallissa; peruspaletti/fontit/napit ovat käytössä. Ei vielä läpikäyty systemaattisesti mockupia vasten.
- **M6.2 3D & valaistus** 🔨 — kaikki neljä rataa low-poly-mallinnettu Blenderissä paletti-atlas-tekniikalla (ei Kenney-assetteja, vaan itse generoituja). Ei erillistä URP-jälkikäsittelyä (bloom/tonemapping) vielä viritetty.
- **M6.3 Ääni** ⬜ — ks. M5.3, sama aukko. Ei musiikkia, ei SFX:ää, ei Vilkun ääntä, ei haptiikkaa.
- **M6.4 Mehukkuus** ⬜ — ei partikkeleita (konfetti, kimallus) eikä squash/stretch-animaatioita pisteen noususta, tähden keräyksestä tms. Toinen iso, suhteellisen halpa parannus juuri nyt.
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

`Assets/_Pakettiporina/Scripts/Editor/`-kansiossa on 16 työkalua, jotka kaikki
noudattavat samaa "Tarkista X / KORJAA X" -kaavaa (valikko **Pakettiporina**):

| # | Työkalu | Mitä tekee |
|---|---|---|
| 1–2 | `PakettiporinaSetup.cs` | Peliscenen kytkentöjen tarkistus/korjaus, nyt rata-tietoinen (toimii kaikilla 4 radalla). |
| 4–5 | `PakettiporinaGarageSetup.cs` | Hallin kytkennät, osa/pakettilistojen automaattinen täyttö, trackScene vs. Build Settings -tarkistus. |
| 6 | `PakettiporinaStars.cs` | Tähtien asettelu radan kaarteen mukaan (kaikki 4 rataa). |
| 6 | `PakettiporinaDataTools.cs` | Datan analyysi + 5 uuden paketin generointi. *(HUOM: numero 6 on käytössä kahdesti, kosmeettinen epäjohdonmukaisuus.)* |
| 7 | `PakettiporinaDataTools.cs` | Pakettigenerointi. |
| 8/8a/8b/8c | `PakettiporinaDataFix.cs` | Osien kategoriakorjaus, tasapainotus, lisäosien luonti. |
| 9–10 | `PakettiporinaWashSetup.cs` | Pesuscenen kytkennät + Done/PausePanel-piilotus (korjattu tässä sessiossa). |
| 11–12 | `PakettiporinaMainMenuSetup.cs` | Päävalikon kytkennät. |
| 13 | `PakettiporinaStickerTools.cs` | Tarrojen StickerData-assetit kuvista (36 tarraa). |
| 14 | `PakettiporinaElements.cs` | Boost/Lätäkkö/Kartio-sijoittelu radan mukaan. |
| 15 | `PakettiporinaDecor.cs` | Teemakoristeiden (FBX) sijoittelu radan mukaan, tukee saman mallin useita instansseja. |
| 16 | `PakettiporinaSecretParts.cs` | Salaisten osien luonti ("paras mahdollinen auto"). |

**Tunnetut pienet siivousasiat:** valikkonumero 3 puuttuu kokonaan; numero 6
on kahdessa eri työkalussa; `PakettiporinaMitat.cs` on tyhjä käyttämätön
tiedosto joka kannattaa poistaa Unityn kautta (ei koodillisesti kriittinen).

---

## Suositeltu seuraava askel

Kolme suurinta yksittäistä parannusta juuri nyt, isoimmasta vaikutuksesta pienimpään:

1. **Build Settings** — lisää Puisto/Satama/Huvipuisto (M2.12, jo aloitettu, jäljellä puhtaasti manuaalinen Unity-askel).
2. **Ääni (M5.3/M6.3)** — suurin puuttuva "juice"-elementti, vaikuttaa koko pelin tuntumaan.
3. **StickerGrid 24→36 solua (M5.7)** — 12 juuri lisättyä tarraa ovat tällä hetkellä keräämättömiä pelissä.

---

*Koodin kommenttien `[M1]`, `[M2]`, `[M3]` viittaavat päävirstanpylvääseen; tämä
dokumentti purkaa ne alivaiheiksi. Päivitä tilat (✅/🔨/⬜) edetessäsi.*
