# Pakettipörinä 🚗📦

*Kaupungin hupsuin lähetti*

**Pakettipörinä** on iloinen, lapsiystävällinen 3D-mobiilipeli (Android) 6–10-vuotiaille.
Pelaaja rakentaa auton osista, valitsee toimitettavan paketin, ajaa sen radan läpi maaliin
ja pesee ja korjaa auton ajon jälkeen. Peli yhdistää **rakentelun**, **ajamisen** ja
**huoltamisen** niin, että lapsen valinnoilla on näkyvä vaikutus ajokokemukseen.

> Portfolioprojekti. Rakennettu Unitylla (URP), tavoitteena lapsiturvallinen, mainokseton
> ja offline-toimiva peli.

---

## 🎮 Pelin idea

Pelisilmukka:

1. **Halli** – kolme vaihetta, yksi asia kerrallaan: **Paketti** (mitä viedään), **Rata**
   (kuva ja nimi radasta, jolla ajetaan) ja **Auto** (rakenna autosi kuudesta osakategoriasta:
   kori, renkaat, moottori, jouset, lisät ja väri). Vilkku-maskotti opastaa.
2. **Ajo** – auto ajaa; lapsi ohjaa kallistamalla puhelinta tai napeilla ja jarruttaa
   yhdellä napilla. Matkalla kerätään tähtiä, hyödynnetään tehosteita (boost) ja väistellään
   esteitä (lätäkkö, kartio, laatikko).
3. **Maali** – ajosta saa 1–3 tähteä (perillä / paketti ehjänä / kaikki bonustähdet kerätty tai nopea aika) ja kolikkopalkkion; oikea auto oikealle paketille tuo täyden palkkion.
4. **Pesu ja korjaus** – ajossa kertynyt lika poistetaan poksauttamalla saippuakuplia ja
   kolhut korjataan napauttamalla korjauskuplia. Lika- ja kolhumittari säilyy ajojen välillä.
5. **Takaisin halliin** – auto säilyy. **Tähdet** avaavat uusia värejä, osia, paketteja ja ratoja
   kolmessa tahdissa (nopea: värit ja pienet osat, keskipitkä: osat ja paketit, pitkä: uudet radat).
   **Kolikoilla** ostetaan tarroja.

Auton **ominaisuudet vaikuttavat ajoon**: voima → kiihtyvyys, keveys → huippunopeus,
pito → pitävyys, ja paketin massa hidastaa raskasta kuormaa.

---

## ✨ Ominaisuudet

**Pelattavuus**
- **Auton rakentaminen:** 32 osaa, 10 pakettia ja salaiset kultaiset osat
- **Fysiikkapohjainen arcade-ajo** (Rigidbody, ForceMode.Acceleration)
- **Ohjaus:** kallistus (kalibroinnilla), kosketusnapit tai molemmat; kolme nopeustasoa
- **Lähtölaskenta** (3-2-1-AJA!), **1–3 tähden arvosana** joka ajosta ja kaksi valuuttaa: tähdet (avaavat sisältöä, `Progress.cs`) ja kolikot (tarrat)
- **Radan valinta selaamalla** hallissa; lukitut radat näyttävät tarvittavat tähdet
- **Pesu ja korjaus** omana minipelinään, pysyvä lika-/kolhumittari
- **36 avattavaa tarraa**

**Rakenne ja viimeistely**
- **Putoamissuoja ja kaiteet:** näkymättömät kitkattomat kaiteet radan sivuilla (Satamassa lähellä tietä, muilla radoilla maailman reunalla) ja `CarRecovery` palauttaa auton vanhempaan turvalliseen paikkaan toistuvan putoamisen jälkeen
- **Tallennus** (JSON, laitteella): kolikot, ajotähdet, radoittaiset parhaat tähdet, valinnat, asetukset ja mittarit; rikkinäinen tiedosto tallennetaan `.rikki.bak`-kopioksi
- **Asetukset:** musiikki, äänet, värähtely, ohjaustapa, ajonopeus ja **yötila**
- **Äänet:** musiikki valikolle, hallille, radalle ja pesulle sekä 12 tehostetta
- **Scenevaihdon häivytys** (fade) kaikissa näkymissä
- **Androidin takaisin-nappi:** sulkee paneelit ja palaa edelliseen näkymään
- **Vilkku-maskotti** puhekuplineen päävalikossa ja hallissa
- **Mehukkuus:** pomppivat napit, konfetti maalissa, kipinät ja pärskeet, välähdykset sekä
  koodilla syntetisoidut äänet (`Juice`, `SoundSynth`; ei erillisiä äänitiedostoja)
- **Kolmivaiheinen halli** (`GarageWizard`): isot napit, ikonit ja kuvat, auton värinvalinta
  mukana; HUD:ssa Lika- ja Kolhut-mittarit ikoneilla
- **Lapsiturva:** offline, ei mainoksia, ei datankeruuta, ei ostoja eikä chattia;
  aikuisille oma osio (tietoja pelistä, tietosuoja, edistymisen nollaus)

**Tekniikka**
- **Palettitekstuuri-työnkulku** low-poly-malleille (yksi materiaali + pieni PNG)
- **Proseduraalinen ratageneraattori** (Blender, Python): uusia ratoja vaihtamalla
  mutkat, pituuden ja teeman
- 73 omaa FBX-mallia

---

## 🗺️ Scenet ja radat

| Scene | Sisältö |
|-------|---------|
| `MainMenu` | Päävalikko, asetukset, tarrat, tietoja ja tietosuoja |
| `Garage` | Halli: auton rakennus ja paketin valinta |
| `Game` | Perusrata |
| `Puisto` | Puisto: lampi, kaarisilta, keinut |
| `Satama` | Satama: laiva, nosturi, kontit |
| `Huvipuisto` | Huvipuisto: maailmanpyörä, karuselli, teltta |
| `Maatila` | Maatila: navetta, eläimet, tuulimylly |
| `Wash` | Pesu ja korjaus |
| `Kyla` | Kylä-rata (vaikein) – **tehdään myöhemmin**: vaatii paljon FBX-elementtejä ja oman ratamallin; scene on tarkoituksella pois Build Settingsista (`Progress.PlannedTracks`) |

---

## 🛠️ Teknologiat

| Alue | Työkalut |
|------|----------|
| Moottori | Unity 2022.3 LTS (URP) |
| Kieli | C# |
| 3D-mallit | Blender (Python-skriptit, palettitekstuuritekniikka) |
| Alusta | Android (IL2CPP, ARM64) |
| Versionhallinta | Git (isot binääritiedostot Git LFS:llä) |
| Kehitystyö | Claude tekoälyavusteisena pariohjelmointikumppanina |

---

## 🧰 Editorityökalut (oma tuotanto)

Projektissa on 46 omaa Unity-editorityökalua, jotka automatisoivat scenejen rakentamisen
ja pitävät ne virheettöminä. Ne löytyvät valikosta **Pakettiporina** viidessä ryhmässä:

| Ryhmä | Työkalut |
|-------|----------|
| **Tarkistus** (10) | Peliscene, halli, pesu ja päävalikko; datan analyysi; äänten tarkistus kaikista sceneistä; **Hallin kolmivaiheen tarkistus** (kuvat, pakettien ikonit ja radat, Build Settings); **Edistyksen tarkistus** (`Progress.cs`-taulukot, ratalistat, wizard-kuvat; ajetaan myös hallin tarkistuksen perään); **Radan turvatarkistus** (kaiteet, maaliportin koko, isot koristeet); **automaattisten testien ajo** |
| **Korjaus** (11) | Peliscenen, hallin, pesun ja päävalikon korjaus ja kytkentä; kategoriat ja kosmetiikka; osien arvojen tasapainotus; "korjaa kaikki" (data); äänten kytkentä; Kylä-radan perusta; **Hallin kuvien synkkaus** (`Resources/WizardUI`); **ratakaiteet ja maaliportin rajoitus** |
| **Esittely** (3) | Kuvakaappaus, GIF-tallennus ja maalin efektien testi (kuvat `Docs/Esittely`) |
| **Lisäys** (11) | Tähdet radalle; pelielementit (boost, lätäkkö, kartio, laatikko); teemakoristeet; teemapuiston kartta; lisäosat; uudet paketit; avattavat tarrat; salaiset osat; asetuspaneeli; esteet ja tehosteet uuteen rataan; animoitu maskotti |
| **Suorituskyky** (4) | Automaattimittaus (FPS, muisti, vuodot), staattinen tarkistus (asetukset, HDR, tekstuurit, äänet, Build Settings), ARM64- ja kokosuositukset, tehosteäänten esilataus |
| **Työkalut** (7) | Tien profiili (konsoli ja Scene-näkymä); scenen elementtien ja sijaintien vienti; puuttuvien scriptien haku ja poisto |

Työkalut ovat idempotentteja eli ne voi ajaa turvallisesti uudelleen.

> Nämä työkalut ovat tietoinen valinta: ne osoittavat pyrkimystä toistettavaan,
> ylläpidettävään työnkulkuun, ei vain toimivaan lopputulokseen.

---

## 📁 Projektin rakenne

```
Assets/_Pakettiporina/
├─ Art/         (mallit, tekstuurit, UI, ikonit, ratakuvat Art/UI/Rata)
├─ Tests/       (EditMode-testit: Progress, tallennus, ajotähdet)
├─ Resources/   (WizardUI: Hallin kolmivaiheen kuvat, kopioita Art/UI-kuvista)
├─ Audio/       (musiikki ja tehosteet)
├─ Data/        (osat, paketit, tarrat ScriptableObjecteina)
├─ Prefabs/     (auto, kerättävät, esteet)
└─ Scripts/
   ├─ Core/     (GameManager, Progress (tähti-/lukitustaulukot ja ratalista), SaveManager,
   │             AudioManager, PerformanceManager, SettingsManager,
   │             SceneFader, BackButtonHandler, DarkModeOverlay, Juice, SoundSynth,
   │             CutoutAvoider, ResetConfirmManager)
   ├─ Vehicle/  (ohjaus ja ajofysiikka)
   ├─ Gameplay/ (keikka, maali, esteet, tehosteet, pesu ja korjaus)
   ├─ Builder/  (auton kokoaminen osista, sopivuus)
   ├─ UI/       (halli ja GarageWizard, HUD, valikot, tarrat, maskotti)
   ├─ Data/     (PartData, PackageData, CarStats, StickerData)
   └─ Editor/   (Tarkistus, Korjaus, Lisays, Tyokalut)
Scenes/         (MainMenu, Garage, Wash, Game, Puisto, Satama, Huvipuisto,
                 Maatila, Kyla)
Docs/           (projektidokumentaatio, Docs/Esittely kuvat, Docs/Opetussarja materiaalit)
```

---

## 🚧 Kehityksen tila

Peli on pelattava ja sisällöltään laaja: rakentelu, viisi rataa, pesu ja korjaus,
tallennus, asetukset, äänet ja yötila ovat valmiina.

**Jäljellä:**
- Kylä-rata (oma ratamalli, FBX-elementit, maasto, äänet) – myöhemmin
- Laitetestaus oikealla Android-laitteella (suorituskyky, akku, kosketus) – vielä tekemättä
- Pitkän aikavälin motivaation testaus lapsilla (tähti- ja lukitusrytmi on mitoitettu, ei vielä pelitestattu)
- Vanhempainportti ja Google Play -julkaisun vaatimukset (tietosuojaseloste, ikäluokitus, allekirjoitus)
- Lokalisointi: tekstit ovat koodissa suomeksi (noin 300 merkkijonoa, `Localization`-kansio tyhjä)

**Tunnetut heikkoudet (24.11.2026):**
- Automaattisia testejä on vasta alku: 29 EditMode-testiä (`Tests/EditMode`: tähtiarvosana, avaukset, ratalistat, tallennusdata); PlayMode-testit, UI ja ajofysiikka testaamatta
- Käyttöliittymä rakennetaan koodissa (Juice, GarageWizard, RaceHUD, WashScreen) – nopea muokata, mutta ulkoasua ei voi säätää editorissa
- Suuret UI-tiedostot on pilkottu `partial`-luokiksi (`Juice.*.cs`, `GarageWizard.*.cs`, `RaceHUD.Meters.cs`, `WashScreen.Repair.cs`); pisin tiedosto on nyt alle 400 riviä, mutta luokat ovat edelleen laajoja
- Tallennuksessa `saveVersion` on mutta siirtymälogiikkaa ei ole; vanhat tallennukset aloittavat 0 ajotähdellä
- Uuden radan lisäys vaatii muutoksia usealle listalle (Progress, `PakettiporinaTracks`, Decor, Obstacles)
- HDR päällä URP-Balanced-profiilissa (Androidin oletus) – kallis mobiilissa
- Lisenssi-/lähdetiedot kolmannen osapuolen assetteihin kirjaamatta (estää FBX-/työkalumyynnin)

---

## 📚 Esittely ja materiaalit

Kaikki esittelymateriaali on kansiossa [`Docs/Esittely`](Docs/Esittely):
[pitch-esitys](Docs/Esittely/Pakettiporina_pitch.pptx), [artikkelisarjan runko](Docs/Esittely/Artikkelisarja_Runko_10osaa_2026-08-24.md) (10-osainen "How to build your own game with Unity, Blender and Claude") ja [tutorial-idea](Docs/Esittely/Idea_TutorialArtikkelit_2026-08-20.md).

---

## 📸 Kuvia

![Ajo Satamassa](Docs/Esittely/ajo.gif)

| Päävalikko | Halli: paketti | Halli: rata |
|:---:|:---:|:---:|
| ![Päävalikko](Docs/Esittely/paavalikko.png) | ![Paketti](Docs/Esittely/halli_1_paketti.png) | ![Rata](Docs/Esittely/halli_2_rata.png) |

| Halli: auto | Ajo | Maali |
|:---:|:---:|:---:|
| ![Auto](Docs/Esittely/halli_3_auto.png) | ![Ajo](Docs/Esittely/ajo.png) | ![Maali](Docs/Esittely/maali.png) |

| Pesu |
|:---:|
| ![Pesu](Docs/Esittely/wash.png) |

*(Kuvat otettu Unityn Simulatorilla, Galaxy S10e, 24.11.2026. Ajo-GIF on aiemmalta ajolta.)*

---

## 👤 Tekijä

Eero Korhonen — [GitHub](https://github.com/PennasenKake)

Ohjelmistotekniikan insinööri (AMK). Pakettipörinä on henkilökohtainen portfolioprojekti,
jossa yhdistyvät pelisuunnittelu, C#-ohjelmointi, 3D-sisällöntuotanto ja työkalujen
kehittäminen.
