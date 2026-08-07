# Pakettiporina
# Pakettipörinä 🚗📦

**Pakettipörinä** on iloinen, lapsiystävällinen 3D-mobiilipeli (Android) 6–10-vuotiaille.
Pelaaja rakentaa itselleen auton osista, valitsee toimitettavan paketin ja ajaa sen radan
läpi maaliin. Peli yhdistää **rakentelun** ja **ajamisen** niin, että lapsen valinnoilla on
näkyvä ja tuntuva vaikutus ajokokemukseen.

> Portfolioprojekti. Rakennettu Unitylla (URP), tavoitteena lapsiturvallinen, mainokseton
> ja offline-toimiva peli.

---

## 🎮 Pelin idea

Pelisilmukka:

1. **Halli** – valitse paketti ja rakenna auto kuudesta osakategoriasta (kori, renkaat,
   moottori, jouset, lisät, maali).
2. **Ajo** – auto ajaa automaattisesti; lapsi ohjaa kallistamalla puhelinta tai napeilla,
   ja jarruttaa/peruuttaa yhdellä napilla. Matkalla kerätään tähtiä.
3. **Maali** – palkkio pisteinä; oikea auto oikealle paketille tuo täyden palkkion.
4. **Takaisin halliin** – auto säilyy, pisteet kasvavat, ja lapsi voi virittää autoa lisää.

Auton **ominaisuudet vaikuttavat ajoon**: voima → kiihtyvyys, keveys → huippunopeus,
pito → pitävyys, ja paketin massa hidastaa raskasta kuormaa. Näin "rakenna sopiva auto"
on aito, palkitseva valinta.

---

## ✨ Ominaisuudet

- **Auton rakentaminen** osista, joilla on selkeät vahvuudet ja heikkoudet
- **Fysiikkapohjainen arcade-ajo** (Rigidbody, ForceMode.Acceleration)
- **Kallistusohjaus** kalibroinnilla + vaihtoehtoiset kosketusnapit
- **Lähtölaskenta** (3-2-1-AJA!) ja selkeä palkkiosilmukka
- **Palettitekstuuri-työnkulku** low-poly-malleille (yksi materiaali + pieni PNG → värit
  toimivat luotettavasti Unityssä ja mobiili pysyy kevyenä)
- **Proseduraalinen ratageneraattori** (Blender): sama työkalu tuottaa uusia ratoja
  vaihtamalla mutkat, pituuden ja teeman
- **Lapsiturva**: offline, ei mainoksia, ei datankeruuta, vanhempainportti (suunnitteilla)

---

## 🛠️ Teknologiat

| Alue | Työkalut |
|------|----------|
| Moottori | Unity 2022.3 LTS (URP) |
| Kieli | C# |
| 3D-mallit | Blender (Python-skriptit, palettitekstuuritekniikka) |
| Alusta | Android (IL2CPP, ARM64) |
| Versionhallinta | Git |

---

## 🧰 Editorityökalut (oma tuotanto)

Projektiin on rakennettu joukko Unity-editorityökaluja, jotka nopeuttavat kehitystä ja
pitävät scenet virheettöminä. Ne löytyvät valikosta **Pakettiporina**:

- **Scenen tarkistus & korjaus** – tarkistaa ja korjaa peliscenen kytkennät (auto, kamera,
  maasto, collider, maali) yhdellä klikkauksella
- **Hallin korjaus** – lataa osat ja paketit automaattisesti ja kytkee UI:n tapahtumat
- **Data-analyysi & tasapainotus** – tarkistaa osien ja pakettien tasapainon, korjaa
  kategoriat ja generoi uutta sisältöä
- **Mittatyökalu** – tulostaa objektien todelliset mitat vianetsintää varten

> Nämä työkalut ovat tietoinen valinta: ne osoittavat pyrkimystä toistettavaan,
> ylläpidettävään työnkulkuun, ei vain toimivaan lopputulokseen.

---

## 📁 Projektin rakenne

```
Assets/_Pakettiporina/
├─ Art/         (mallit, tekstuurit, UI, ikonit)
├─ Data/        (osat ja paketit ScriptableObjecteina)
├─ Prefabs/     (auto, kerättävät, esteet)
├─ Scenes/      (MainMenu, Garage, Game)
└─ Scripts/
   ├─ Core/     (GameManager, GameEvents)
   ├─ Vehicle/  (ohjaus ja ajofysiikka)
   ├─ Gameplay/ (keikan hallinta, maali, palkkio)
   ├─ Builder/  (auton kokoaminen osista)
   ├─ UI/       (halli, HUD, valikot)
   ├─ Data/     (PartData, PackageData, CarStats)
   └─ Editor/   (kehitystyökalut)
```

---

## 🚧 Kehityksen tila

Peli on aktiivisessa kehityksessä. Ydinpelisilmukka (rakenna → aja → palkkio → halli)
on toteutettu ja pelattava. Työn alla: autonpesu-välivaihe, tallennusjärjestelmä,
äänet ja lisää ratoja.

---

## 📸 Kuvia

*(Lisää tähän kuvakaappauksia tai GIF pelistä – ks. ohje alla.)*

<!--
![Halli](docs/kuvat/halli.png)
![Ajo](docs/kuvat/ajo.png)
-->
(TULOSSA)
---

## 👤 Tekijä

Eero Korhonen — [GitHub](https://github.com/PennasenKake)

Ohjelmistotekniikan insinööri (AMK). Pakettipörinä on henkilökohtainen portfolioprojekti,
jossa yhdistyvät pelisuunnittelu, C#-ohjelmointi, 3D-sisällöntuotanto ja työkalujen
kehittäminen.
