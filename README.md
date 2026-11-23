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

1. **Halli** – valitse paketti ja rakenna auto kuudesta osakategoriasta (kori, renkaat,
   moottori, jouset, lisät, maali).
2. **Ajo** – auto ajaa; lapsi ohjaa kallistamalla puhelinta tai napeilla ja jarruttaa
   yhdellä napilla. Matkalla kerätään tähtiä, hyödynnetään tehosteita (boost) ja väistellään
   esteitä (lätäkkö, kartio, laatikko).
3. **Maali** – palkkio pisteinä; oikea auto oikealle paketille tuo täyden palkkion.
4. **Pesu ja korjaus** – ajossa kertynyt lika poistetaan poksauttamalla saippuakuplia ja
   kolhut korjataan napauttamalla korjauskuplia. Lika- ja kolhumittari säilyy ajojen välillä.
5. **Takaisin halliin** – auto säilyy, pisteet kasvavat, ja pisteillä avautuu uusia osia,
   paketteja ja tarroja.

Auton **ominaisuudet vaikuttavat ajoon**: voima → kiihtyvyys, keveys → huippunopeus,
pito → pitävyys, ja paketin massa hidastaa raskasta kuormaa.

---

## ✨ Ominaisuudet

**Pelattavuus**
- **Auton rakentaminen:** 32 osaa, 10 pakettia ja salaiset kultaiset osat
- **Fysiikkapohjainen arcade-ajo** (Rigidbody, ForceMode.Acceleration)
- **Ohjaus:** kallistus (kalibroinnilla), kosketusnapit tai molemmat; kolme nopeustasoa
- **Lähtölaskenta** (3-2-1-AJA!) ja tähtipohjainen palkkiosilmukka
- **Pesu ja korjaus** omana minipelinään, pysyvä lika-/kolhumittari
- **36 avattavaa tarraa**

**Rakenne ja viimeistely**
- **Tallennus** (JSON, laitteella): pisteet, valinnat, asetukset ja mittarit
- **Asetukset:** musiikki, äänet, värähtely, ohjaustapa, ajonopeus ja **yötila**
- **Äänet:** musiikki valikolle, hallille, radalle ja pesulle sekä 12 tehostetta
- **Scenevaihdon häivytys** (fade) kaikissa näkymissä
- **Androidin takaisin-nappi:** sulkee paneelit ja palaa edelliseen näkymään
- **Vilkku-maskotti** puhekuplineen päävalikossa ja hallissa
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
| `Kyla` | Kylä-rata (vaikein) – **työn alla**, vasta perusrunko |

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

Projektissa on yli 30 omaa Unity-editorityökalua, jotka automatisoivat scenejen
rakentamisen ja pitävät ne virheettöminä. Ne löytyvät valikosta **Pakettiporina**, jossa
on neljä ryhmää:

- **Tarkistus** – raportoivat scenen, hallin, pesun, päävalikon, datan ja äänten puutteet
- **Korjaus** – korjaavat ja kytkevät scenet (peliscene, halli, pesu, päävalikko, äänet)
- **Lisäys** – lisäävät sisältöä: tähdet, esteet ja tehosteet, teemakoristeet, tarrat,
  salaiset osat, asetuspaneeli ja maskotti
- **Työkalut** – apuvälineet vianetsintään (tien profiili, scenen vienti, puuttuvien
  scriptien haku)

Työkalut ovat idempotentteja eli ne voi ajaa turvallisesti uudelleen.

> Nämä työkalut ovat tietoinen valinta: ne osoittavat pyrkimystä toistettavaan,
> ylläpidettävään työnkulkuun, ei vain toimivaan lopputulokseen.

---

## 📁 Projektin rakenne

```
Assets/_Pakettiporina/
├─ Art/         (mallit, tekstuurit, UI, ikonit)
├─ Audio/       (musiikki ja tehosteet)
├─ Data/        (osat, paketit, tarrat ScriptableObjecteina)
├─ Docs/        (kehityslokit, ratojen speksit, ideat)
├─ Prefabs/     (auto, kerättävät, esteet)
└─ Scripts/
   ├─ Core/     (GameManager, SaveManager, AudioManager, SettingsManager,
   │             SceneFader, BackButtonHandler, DarkModeOverlay)
   ├─ Vehicle/  (ohjaus ja ajofysiikka)
   ├─ Gameplay/ (keikka, maali, esteet, tehosteet, pesu ja korjaus)
   ├─ Builder/  (auton kokoaminen osista, sopivuus)
   ├─ UI/       (halli, HUD, valikot, tarrat, maskotti)
   ├─ Data/     (PartData, PackageData, CarStats, StickerData)
   └─ Editor/   (Tarkistus, Korjaus, Lisays, Tyokalut)
Scenes/         (MainMenu, Garage, Wash, Game, Puisto, Satama, Huvipuisto,
                 Maatila, Kyla)
Docs/           (projektidokumentaatio)
```

---

## 🚧 Kehityksen tila

Peli on pelattava ja sisällöltään laaja: rakentelu, viisi rataa, pesu ja korjaus,
tallennus, asetukset, äänet ja yötila ovat valmiina.

**Jäljellä:**
- Kylä-rata (maasto, koristeet, äänet)
- Visuaalinen kiillotus (partikkelit, animaatiot) ja lisää mallit ja äänet
- Vanhempainportti ja Google Play -julkaisun vaatimukset (tietosuojaseloste,
  ikäluokitus, allekirjoitus)
- Laitetestaus ja suorituskyky; mahdollinen lokalisointi

---

## 📸 Kuvia

*(Kuvakaappaukset ja GIF lisätään pian.)*

---

## 👤 Tekijä

Eero Korhonen — [GitHub](https://github.com/PennasenKake)

Ohjelmistotekniikan insinööri (AMK). Pakettipörinä on henkilökohtainen portfolioprojekti,
jossa yhdistyvät pelisuunnittelu, C#-ohjelmointi, 3D-sisällöntuotanto ja työkalujen
kehittäminen.
