# Tiivistelmä ja tilannekatsaus — 14.8.2026

Tämä dokumentti kokoaa yhteen: (A) tämän keskustelun sisällön, ja (B) tilanteen
kaikkiin listaamiisi avoimiin asioihin — osa niistä on jo ehditty korjata
projektissa muualla, osa on yhä auki. Tarkistettu suoraan koodista/sceneistä,
ei pelkästä muistista.

---

## A) Tämä keskustelu — Maatilan 3D-mallit (Blender)

Koko tämä keskustelu käsitteli Maatila-radan objekteja Blenderin kautta:

- **Traktori**: rakennettu kokonaan uudelleen useaan otteeseen. Lopputulos on
  referenssikuvan mukainen umpihyttinen (MTZ/Belarus-tyylinen) punainen
  traktori — hytti, tuulilasi, sivuikkunat, konepelti, säleikkö, 2 ajovaloa,
  taipuva pakoputki, etuakseli, vetokoukku, merkki. Matkan varrella löytyi ja
  korjattiin useita virheitä: renkaiden pyörimisakseli oli väärinpäin (Y-Z-taso
  piti olla X-Z), kaikkien 4 renkaan navat olivat auki (aiheutti "puuttuva
  sivu" -ilmiön), konepellin ja hytin saumassa oli rako. Skaala nostettu
  1.0 → 1.3 ja sijainti siirretty kauemmas tien reunasta (X -14 → -21).
- **Navetta**: lisätty tuuliviiri, perustusreunus, heinänostopalkki+väkipyörä,
  harjalista, kattopellin korrugaatio. Löytyi ja korjattiin iso bugi: koko
  kattorakenne oli ontto kuori ilman räystäslistaa — räystään ja seinän
  väliin jäi aukko koko matkalle (todistettu vihreä tausta -testillä). Lisätty
  räystäslista koko rakennuksen ja vajan ympäri. Skaala nostettu 1.0 → 1.5.
- **Muut Maatila-elementit**: Kana, Hevonen, Kaivo, Siilo rakennettu ja
  sijoitettu (nämä olivat jo ennen tätä osiota tehtyjä, mutta osa samaa
  kokonaisuutta). Tuulimylly ja Variksenpelätti saivat vain teknisen
  ngon/self-intersecting-polygon-korjauksen — **niitä ei ole vielä tehty
  visuaalisesti paremmiksi** (ks. kohta B5 alla).
- Kaikista malleista tarkistettu `useFileScale: 0` (.meta) ja vietiin FBX
  uudelleen Blenderin kautta samalla, koko session ajan käytetyllä
  export-reseptillä.

**Muista Unityssä:** aja "Pakettiporina/12 - Lisää teemakoristeet" ja tallenna,
jotta uudet koot/korjaukset päivittyvät `Maatila.unity`-sceneen.

---

## B) Tilanne listaamiisi asioihin

### B1. Build ja äänet toimivat, valikkomusiikin melodia parempi
Ei käsitelty tässä keskustelussa. Ei tarkistettu tässä kierroksessa — jos
haluat, voin kuunnella/uudistaa `Audio/Music/`-kappaleet erikseen.

### B2. Settings-paneeli (ikonit+lyhyt teksti, toggle, info-nappi)
**Isolta osin jo tehty, mutta ei tässä keskustelussa** — löytyi tuoreita
merkkejä 13.–14.8. tehdystä uudistuksesta (`SettingsManager.cs`,
`PakettiporinaSettingsSetup.cs`):
- Musiikki/Äänet/Tärinä/Tumma tila -asetukset ovat nyt ikoni+lyhyt teksti
  -riveinä (esim. "Musiikki") on/off-kytkimillä — juuri se tyyli jota pyysit.
- **Ajonopeus**-valitsin on jo olemassa (Hidas/Keski/Nopea, 3 nappia) ja
  vaikuttaa oikeasti peliin (`ArcadeCarController.ApplySpeedTier()`).
- **Info-nappi** ("Tietoja pelistä") on olemassa ja näyttää pelin kuvauksen,
  ikäsuosituksen ja tekijätiedot.
- **Puuttuu vielä**: jokaisen asetuksen selittävä lyhyt lisälause (nyt vain
  ikoni+otsikko, ei kuvaustekstiä), ja uusin `DarkModeCard` ei ole vielä
  tallentunut itse `MainMenu.unity`-sceneen (asetustyökalu pitää ajaa kerran).

### B3. Editor-korjaustiedosto hajotti Sataman asettelut
**Löytyi ja on jo korjattu** (13.8.) — juurisyy oli, että yleinen
"KORJAA peliscene" -työkalu (Korjaus/01) pakotti KAIKKIEN ratojen maaston
rotaation nollaksi, mikä nollasi Sataman tarkoituksellisen 180° kierron joka
kerta kun työkalua ajettiin (koristeet pysyivät paikoillaan, mutta koko
ranta/vesi kääntyi alta pois — siksi näytti rikkinäiseltä). Korjattu
lisäämällä radalle oma `maastoRotationY`-kenttä. Tarkistin juuri suoraan
`Satama.unity`:sta: rotaatio on nyt oikea (180°), koristeiden sijainnit
(kartiot, nosturi) näyttävät järkeviltä eivätkä ole päällekkäin/nollattuja.
Tämä ei siis pitäisi enää toistua.

### B4. Esteitä ja tehosteita joka radalle + saippukuplien poksautus radan jälkeen
**Osittain aloitettu tänään (14.8.), mutta vasta yhdellä radalla — ja väärällä.**
Uusi työkalu (`PakettiporinaTrackObstaclesSetup.cs`, Lisays/10) sisältää juuri
sen mekaniikan mitä kaivattiin — radan jälkeisiä saippukuplia jotka pelaaja voi
poksauttaa tähdestä (`TrackBubble.cs`). Se on kuitenkin ajettu tähän mennessä
**vain Maatilalle** (4 Boost/3 Kartio/2 Lätäkkö/3 Kupla) — mikä on ristiriidassa
Maatilan oman suunnitelman kanssa (sen piti olla radoista se rauhallinen,
ilman kartioita/lätäköitä, "hengähdys"-rata). Puisto/Satama/Huvipuisto eivät
ole vielä saaneet lisäesteitä eivätkä kuplia. **Suosittelen:** harkitse
Maatilan kuplien/esteiden poistoa tai vähentämistä, ja aja sama työkalu
Puistolle/Satamalle/Huvipuistolle.

### B5. Lätäkkö/kartio vaikuttaa halliin — entä täydellinen ajo?
Tarkistettu `WashScreen.cs`: jos `GameManager.RaceDirtHits` on 0 (ei yhtään
lätäkköosumaa), pesu hyppää suoraan "Auto oli jo puhdas" -tilaan ilman
pisteitä. Eli täydellinen ajo tällä hetkellä **ei tuota mitään ylimääräistä
hyvää** — se vain ohittaa pesun. Ei vielä ratkaisua. Ehdotuksia mietittäväksi:
pieni bonuspistepotti "puhdas ajo" -saavutuksesta, tai kiillotus-minipeli
(vaikka ilman likaa) joka antaa oman pienen palkinnon.

### B6. Maatilan objektit paremmiksi (navetta, tuulimylly, traktori, variksenpelätti) + lisää elementtejä
**Traktori ja Navetta: tehty tässä keskustelussa** (ks. kohta A). **Tuulimylly
ja Variksenpelätti: vielä tekemättä** — ne saivat tähän mennessä vain
teknisen polygonikorjauksen, ei visuaalista uudistusta. Uusia elementtejä
(Kana, Hevonen, Kaivo, Siilo) on jo lisätty.

### B7. Settingsiin äänet/efektit/nopeus — mitä muuta?
Äänet, tärinä ja ajonopeus ovat jo asetuksissa (ks. B2). Muita tyypillisiä
lisäasetuksia lapsipeliin: ohjaustapa (kosketus vs. kallistus, jos tuettu),
kielivalinta (jos lokalisointia joskus tehdään), "tumma tila" (jo lisätty
mutta ei vielä tallennettu sceneen), sekä yksinkertainen
"nollaa edistyminen" -nappi (turvallisesti varmistuksen takana).

### B8. Game2 → kaupunki/kylä-teema
Iso löytö: **`Game2.unity` ei ole enää olemassa projektissa.** Sen paikalla on
uusi `Kyla.unity` (11.8.), joka näyttää tismalleen siltä mitä pyysit
(kylä-teema) — mutta se on tällä hetkellä lähes tyhjä runko: ei autoa, ei
maastomallia, ei ääniä, yksi komponentti on jopa väärin kirjoitettu
("GroundCollinder"). Koodi (mm. `AudioManager.RaceScenes`) viittaa yhä
vanhaan "Game2"-nimeen. Tämä pitää joko viedä loppuun (Korjaus/01 + teemakoristeet
Kylälle) tai selkeästi päättää mitä sille tehdään, ettei se jää roikkumaan
kahtena eri viittauksena (Game2 koodissa, Kyla levyllä).

---

## Suositeltu seuraava askel (prioriteettijärjestys)

1. **Kylä-scene loppuun** (B8) — puuttuu auto, maasto, äänet; koodin
   "Game2"-viittaukset pitää päättää säilytetäänkö vai nimetäänkö uudelleen.
2. **Esteet/kuplat oikeille radoille** (B4) — aja Lisays/10 Puistolle,
   Satamalle, Huvipuistolle; harkitse Maatilan omien vähentämistä.
3. **Tuulimylly ja Variksenpelätti** visuaalisesti paremmiksi (B6) — sama
   Blender-pipeline kuin Traktorilla/Navetalla tässä keskustelussa.
4. **Settings-paneelin viimeistely** (B2) — DarkModeCard sceneen, lyhyet
   selitystekstit riveihin.
5. **Puhdas ajo -palkinto** (B5) — vaatii suunnittelupäätöksen ennen koodausta.
6. **Valikkomusiikin melodia** (B1) — odottaa erillistä kierrosta.
