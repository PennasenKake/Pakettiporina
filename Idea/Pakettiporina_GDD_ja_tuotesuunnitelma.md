# Pakettipörinä — Pelisuunnittelu- ja tuotedokumentaatio

*3D-mobiilipeli alakoululaisille · ideasta tuotteeksi*
Versio 1.0 · suunnitteludokumentti (GDD + tuotesuunnitelma)

---

## 1. Tiivistelmä

**Pakettipörinä** on iloinen 3D-mobiilipeli 6–10-vuotiaille. Pelaaja on kaupungin hupsuin lähetti, joka rakentaa ja virittää oman autonsa, valitsee toimitettavan paketin ja ajaa sen värikkään kaupungin halki. Pelin sydän on yksi opettava ajatus: **paketin ominaisuudet määräävät, millainen auton pitää olla** — painava paketti vaatii tehoa, särkyvä vaatii pehmeät jouset, kylmä vaatii kylmälaatikon. Maaliin päästyä autoa huolletaan: kolhu korjataan ja auto pestään saippuakuplilla (poksauttelu), minkä jälkeen ansaitut pisteet lisätään kertymään ja voi valita uuden paketin.

| | |
|---|---|
| **Alusta** | Android (ensin), mahdollisesti iOS myöhemmin |
| **Kohderyhmä** | 6–10-vuotiaat lapset; vanhemmat portinvartijoina |
| **Genre** | Kevyt ajo + rakentelu + keräily |
| **Pelisessio** | 1–3 min / keikka, lyhyet sessiot |
| **Moottori** | Godot 4 (suositus) tai Unity |
| **Kieli** | Suomi ensin, englanti lokalisointina |
| **Ansaintamalli** | Kertaosto tai täysin ilmainen; ei mainoksia, ei sisäisiä ostoksia lapsille |

---

## 2. Visio ja suunnittelupilarit

> Lapsi rakentaa hassun autonsa, oppii huomaamattaan että teot vaikuttavat, ja palaa peliin keräilyn ja rauhoittavan hoivahetken vuoksi.

Kaikki suunnittelupäätökset peilataan viiteen pilariin:

1. **Ilo** — kirkkaat värit, leikkisät hahmot, palkitsevat hetket (ilmapallot/kuplat, maalijuhla).
2. **Opettavaisuus** — syy-seuraus, ennakointi ja yhteensovittaminen; oppiminen tekemisen kautta, ei tietovisaa.
3. **Luovuus** — loputtomasti tapoja rakentaa oma auto; oma ilmaisu palkitaan.
4. **Hoiva** — auton korjaaminen ja peseminen rauhoittavana, vastuuta opettavana lopetuksena.
5. **Turvallisuus** — ei pelottavaa sisältöä, ei painostusta, ei tiedonkeruuta; lapselle ja vanhemmalle turvallinen tila.

**Ei-tavoitteet:** ei realistista vaurioitumista tai väkivaltaa, ei aikapainetta joka turhauttaa, ei kilpailullista verkkomoninpeliä, ei "game overia".

---

## 3. Kohderyhmä

- **Ensisijainen:** 6–10-vuotiaat. Osa ei vielä lue sujuvasti → opastus kuvin, äänin ja hahmon (Vilkku) avulla.
- **Toissijainen:** vanhemmat ja opettajat, jotka päättävät asennuksesta ja arvioivat turvallisuuden ja opettavuuden.
- **Suunnittelun reunaehdot:** lyhyet sessiot, suuret kosketuspinnat, minimaalinen luettava teksti, anteeksiantava vaikeustaso, ei FOMO-mekaniikkaa.

---

## 4. Pelikokemus ja ydinsilmukka

**Ydinsilmukka (yksi keikka, ~1–3 min):**

```
Valitse paketti  →  Rakenna / lataa auto  →  Katso reitti kartalta
        →  Aja kaupungin läpi  →  Huoltoasema (korjaa + pese)
        →  Pisteet kertymään  →  (takaisin alkuun)
```

**Metasilmukka (istuntojen yli):** pisteet ja saavutukset → avaa osia, tarroja ja kaupunginosia → uusia ja hauskempia keikkoja → korkeampi arvonimi.

Pelaaja kokee olevansa pätevä lähetti: oma auto, omat valinnat, ja näkyvä eteneminen.

---

## 5. Pelimekaniikat

### 5.1 Auton rakentaminen
Kuusi kategoriaa: **kori, renkaat, moottori, jouset, maali, lisät**. Jokaisessa useita osia (yht. 30, ks. sisältötaulukot). Jokainen osa muuttaa viittä **ominaisuusmittaria**:

- **Voima** — kiihtyvyys ja mäkien nousu
- **Pito** — mutkat, liukkaat pinnat, ohjattavuus
- **Keveys** — ketteryys, hyppyjen pituus
- **Kestävyys** — töyssyjen ja kuoppien sieto, särkyvän paketin suoja
- **Kylmyys** — kylmälaatikon teho (jäätelö)

Osavalinta on kompromisseja: lisää tehoa → lisää painoa → vähemmän keveyttä. Tämä on pelin pohja opettavalle ajattelulle.

### 5.2 Paketit ja yhteensovittaminen
Jokaisella paketilla on **ominaisuustageja**, jotka kertovat vaatimuksen:

| Paketin ominaisuus | Vaatimus autolle |
|---|---|
| Särkyvä (munat, kakku) | Pehmeät jouset, maltillinen vauhti |
| Kylmä (jäätelö) | Kylmälaatikko + nopeus ennen sulamista |
| Läikkyvä (akvaario) | Tasainen ajo, hyvä pito |
| Painava (jättikivi) | Tehokas moottori, isot renkaat |
| Iso & kevyt (ilmapallot) | Tuulisuojaus (spoileri), painoa |

Halli näyttää reaaliaikaisen "**Sopii keikkaan**" -palautteen, joka opettaa syy-seurauksen heti valinnan yhteydessä.

### 5.3 Ajo ja ohjaus
- **Ohjaus:** puhelimen kallistus (kiihtyvyysanturi). Vaihtoehtona kosketusnapit asetuksissa.
- **Kaasu:** automaattinen tai yksi iso **boost-nappi**.
- **Anteeksiantava:** maaliin pääsee aina; ajon laatu vaikuttaa vain tähtiin, pisteisiin ja huoltotarpeeseen — ei epäonnistumista.

### 5.4 Rata ja kaupunginosat
Radat kootaan **uudelleenkäytettävistä palikoista** (lähtösuora, kartiokuja, tori, mäki, lätäkkö, tunneli, silta, satama, töyssytie, ramppi, maali). Jokainen palikka testaa tiettyä ominaisuutta ja voi jättää autoon jäljen (lika/kolhu). Ks. tarkka taulukko liitteenä olevassa Excel-tiedostossa (välilehti *Radan palikat*).

Kaupunginosat avautuvat etenemällä ja nostavat vaikeustasoa:
- **Keskusta** — lempeä, tasainen, paljon väistelyä (tutoriaali-osa)
- **Puisto** — mutkia, lätäköitä
- **Satama** — liukas, konttilabyrintti
- **Huvipuisto** — ramppeja ja temppuja
- **Vuoristo** — jyrkät nousut, tuuli (vaativin)

### 5.5 Huoltoasema (maalin korvaava lopetus)
Radalla auto kerää tilan: lätäköt → lika, törmäykset → kolhut.
1. **Korjaus** — napauta kolhua, se korjautuu.
2. **Pesu** — saippuakuplat nousevat; poksauttelu poistaa likaa ja täyttää **puhtausmittarin**, kunnes auto kiiltää.
3. **Valmis** — pisteet lasketaan kertymään, uusi tarra voi paljastua, ja voi valita uuden paketin.

Siisti ajo = vähemmän huollettavaa (ei rangaistus, vaan palkinto nopeammasta vuorosta). Hoivaava, rauhoittava lopetus joka kerta.

### 5.6 Pisteet, palkinnot ja eteneminen
**Keikan pisteet** = paketin palkkio + kerätyt tähdet + poksautetut kuplat + siisteysbonus. Erittely näytetään lapselle läpinäkyvästi.

**Arvonimet** (avaavat sisältöä):

| Taso | Arvonimi | Pisteraja | Avaa |
|---|---|---|---|
| 1 | Aloittelija | 0 | Keskusta |
| 2 | Lähetti | 500 | Puisto, 10 tarraa |
| 3 | Taitolähetti | 2 000 | Satama, kylmälaatikko |
| 4 | Mestarilähetti | 5 000 | Huvipuisto, hehkurenkaat |
| 5 | Legenda | 12 000 | Vuoristo, lentosiivet |

**Tarrat** (60 kpl) — koristeellisia kokoelmapalkintoja saavutuksista. **Osat** (30 kpl) — toiminnallisia ja kosmeettisia, ostetaan pisteillä. Täydet listat: ks. Excel-tiedosto.

### 5.7 Tallennetut autot (muistitoiminto)
Pelaaja tallentaa suosikkiautonsa nimellä ja lataa sen uudelleen yhdellä napilla — ei tarvitse rakentaa alusta joka keikalle. Talli säilyttää useita autoja eri tarpeisiin.

### 5.8 Opashahmo Vilkku
Ystävällinen hahmo, joka ohjaa joka ruudussa lyhyellä puhekuplalla ja ääneen luettuna (vähän luettavaa). Reagoi tilanteeseen ("Tämä on särkyvä — laita pehmeät jouset!") ja tekee pelistä lämpimän ja saavutettavan.

---

## 6. Sisältö

Täysi sisältö on koottu erilliseen Excel-tiedostoon **`pakettiporina_sisaltotaulukot.xlsx`**, kolmella välilehdellä:

- **Radan palikat** — palikka → este/tilanne → testattava ominaisuus → jälki autoon → bonus.
- **Osat (30)** — osa, kategoria ja vaikutus viiteen mittariin sekä avautumisehto.
- **Tarrat (60)** — tarra, teema, avausehto ja harvinaisuus.

Sisältöpacing: aloituksessa tarjolla ~5 osaa ja muutama paketti; loput avautuvat etenemällä, jotta valinnanvapaus kasvaa pelaajan taidon mukana.

---

## 7. Eteneminen ja talous

- **Pistelähteet:** keikan palkkio, tähdet, kuplat, siisteysbonus, päivän keikka, putkibonus.
- **Pistenielut:** uusien osien ja koristeiden avaaminen.
- **Ei oikeaa rahaa:** kaikki ansaitaan pelaamalla. Talous viritetään niin, että uusi osa tai kaupunginosa avautuu tasaisin väliajoin (esim. pieni avaus joka 2.–3. keikka alussa).
- **Tasapaino:** vältetään grindausta — etenemisen pitää tuntua palkitsevalta lyhyilläkin sessioilla.

---

## 8. UX/UI

**Lasten UX-periaatteet:**
- Suuret kosketuspinnat (≥ 48 px), yksi selkeä päätoiminto per ruutu.
- Minimaalinen teksti; kuvat, ikonit ja Vilkun puhe kantavat ohjeistuksen.
- Välitön visuaalinen ja äänipalaute (väri vaihtuu, "Sopii!", poksahdus).
- Vaiheindikaattorit ja selkeä alanavigaatio (Koti / Keikat / Halli / Kartta / Tarrat).
- Anteeksiantavuus: ei rangaisevia virhetiloja, helppo perua valinta.

**Päänäkymät:** Koti, Keikkapörssi (kuvallinen pakettilista), Halli (rakentelu + mittarit + tallennetut autot), Kartta (reitti), Ajo (HUD), Huoltoasema (korjaus + pesu), Tarrakirja (tarrat + ominaisuudet).

(Interaktiiviset mockupit on tuotettu erikseen ja toimivat tämän dokumentin visuaalisena liitteenä.)

---

## 9. Taidesuunta

- **Tyyli:** matalapolygoninen (low-poly), pyöreät muodot, kirkkaat tasaiset värit, "lelumainen" tunnelma.
- **Realismi:** vain sen verran että fysiikka tuntuu uskottavalta; mittasuhteet ja törmäykset liioiteltuja ja vaarattomia.
- **Hahmot:** Vilkku-maskotti johdonmukaisena tunnistettavana hahmona kaikkialla.
- **Paletti:** lämpimät korostukset (oranssi/keltainen), iloiset perusvärit; riittävä kontrasti luettavuudelle.
- **Animaatio:** pomppivat painikkeet, hilpeät reaktiot, kuplien ja konfettien partikkelit.

---

## 10. Ääni

- **Musiikki:** kevyt, iloinen taustamusiikki, joka ei väsytä toistossa; rauhallisempi raita huoltoasemalle.
- **SFX:** moottori, renkaat, tähtien keräys, kuplien poksahdus, "sopii"-kilahdus.
- **Vilkun ääni:** nauhoitettu puhe tai laadukas puhesynteesi (TTS), jotta lukutaidoton pärjää.
- **Haptiikka:** kevyt värähdys palautteena (kupla, palkinto).

---

## 11. Saavutettavuus ja lapsiturvallisuus

**Saavutettavuus:**
- Värisokeusystävälliset ikonit ja muodot (ei pelkkä väri viestin kantajana).
- Ohjeet sekä kuvina että ääneen; säädettävä äänenvoimakkuus ja musiikin sammutus.
- Vaihtoehtoinen ohjaus (kosketus kallistuksen sijaan), herkkyyden säätö.

**Lapsiturvallisuus ja yksityisyys (tärkeä tuotteen reunaehto):**
- **Ei mainoksia** eikä kolmannen osapuolen seurantaa.
- **Ei henkilötietojen keräystä**; peli toimii offline-tilassa, edistyminen tallentuu vain laitteelle.
- **Vanhempainportti** (parental gate) kaikkien ulkoisten linkkien ja asetusten edessä.
- **Ei sisäisiä ostoksia** lapsille suunnatussa kokemuksessa.
- Tavoitteena täyttää Google Playn **Families**-ohjelman ja vastaavat lapsia koskevat vaatimukset (sekä iOS:n Kids-kategoria, jos julkaistaan). EU:ssa huomioitava GDPR ja lapsia koskeva sääntely, USA:ssa COPPA.

> Huom: alustojen ja lainsäädännön vaatimukset muuttuvat — tarkista voimassa olevat Google Play / App Store -käytännöt ja lapsia koskeva sääntely ennen julkaisua, ja konsultoi tarvittaessa asiantuntijaa.

---

## 12. Tekninen arkkitehtuuri

**Moottori:** Godot 4 (suositus tähän projektiin: ilmainen, avoin, kevyt mobiili-3D-vienti, `VehicleBody3D` antaa autofysiikan valmiina, hyvä portfolioon). Vaihtoehto: Unity (C#, laaja ekosysteemi).

**Kerrokset (alhaalta ylös):**

1. **Build & julkaisu** — Android SDK (Android Studion kautta), AAB-paketointi, allekirjoitus, emulaattori/laitetestaus.
2. **Moottori & renderöinti** — Godot 4, mobiilirenderöijä (Vulkan/GLES3), low-poly, tavoite 60 fps keskitason laitteilla.
3. **Sisältö & kentät** — uudelleenkäytettävät palikat (scenet/prefabit), joista radat kootaan.
4. **Datakerros** — osat, paketit, tarrat ja kentät datana (Godot `Resource` / JSON); tallennus paikallisesti.
5. **Auto & fysiikka** — ajoneuvokontrolleri, jonka parametrit (teho, pito, jousto, massa) tulevat valituista osista + paketin painosta.
6. **Pelilogiikka & tila** — tilakone: Koti → Keikat → Halli → Kartta → Ajo → Huolto → (alkuun); pisteet, tähdet, voittoehto, huoltotila.
7. **Käyttöliittymä & opas** — Godotin Control-nodet, isot napit, Vilkun puhekuplat, lokalisointi käännösavaimilla.

**Suorituskyky:** low-poly-mallit, esilaskettu valaistus, object pooling (esteet, kuplat, partikkelit), rajattu draw call -määrä.

**"Paketti vaikuttaa autoon" toteutuksena:** paketin massa lisätään auton kokonaismassaan, ja osat säätävät fysiikkaparametreja — fysiikka hoitaa loput (raskas hidastaa kiihtyvyyttä ja pidentää jarrutusta). Realismi syntyy luonnostaan ilman erikoiskoodausta.

---

## 13. Datamallit (esimerkkiskeemat)

**Osa (Part):**
```json
{
  "id": "pehmeat_jouset",
  "name": "Pehmeät jouset",
  "category": "jouset",
  "effects": { "voima": 0, "pito": 15, "keveys": 0, "kestavyys": 5, "kylmyys": 0 },
  "cosmetic_only": false,
  "unlock": { "type": "points", "value": 0 }
}
```

**Paketti (Package):**
```json
{
  "id": "munakori",
  "name": "Munakori",
  "icon": "egg",
  "tags": ["sarkyva"],
  "requires_part": "pehmeat_jouset",
  "mass": 8,
  "reward_points": 30,
  "melt_timer": null
}
```

**Tarra (Sticker / Achievement):**
```json
{
  "id": "hellavarainen",
  "name": "Hellävarainen",
  "theme": "paketit",
  "rarity": "harvinainen",
  "condition": { "type": "deliver_fragile_intact", "count": 5 }
}
```

**Radan palikka (TrackBlock):**
```json
{
  "id": "maki",
  "tests": "voima",
  "effect_on_car": "none",
  "obstacle": null,
  "bonus": "none"
}
```

**Tallennus (SaveGame):**
```json
{
  "points_total": 1375,
  "rank": 2,
  "unlocked_parts": ["pikkukori", "sileat_renkaat", "pehmeat_jouset"],
  "unlocked_stickers": ["ensilahetti", "mutakylpy"],
  "unlocked_districts": ["keskusta", "puisto"],
  "saved_cars": [
    { "name": "Punapommi", "color": "#E24B4A", "parts": { "kori": "sport", "renkaat": "sileat", "moottori": "pieni", "jouset": "pehmeat", "lisat": null } }
  ],
  "settings": { "tilt_steering": true, "music": true, "sfx": true, "language": "fi" }
}
```

---

## 14. Projektirakenne (Godot, ehdotus)

```
pakettiporina/
├─ project.godot
├─ scenes/
│  ├─ main.tscn               # tilakone / reititys
│  ├─ ui/                     # koti, keikat, halli, kartta, tarrat
│  ├─ game/
│  │  ├─ car.tscn             # VehicleBody3D + ohjaus
│  │  ├─ track_block_*.tscn   # uudelleenkäytettävät palikat
│  │  └─ huoltoasema.tscn     # korjaus + pesu
│  └─ vilkku.tscn             # opashahmo + puhekupla
├─ scripts/
│  ├─ game_state.gd
│  ├─ car_builder.gd
│  ├─ track_generator.gd      # kokoaa radan palikoista
│  ├─ scoring.gd
│  └─ save_manager.gd
├─ data/
│  ├─ parts/*.tres            # tai parts.json
│  ├─ packages/*.tres
│  ├─ stickers.json
│  └─ districts.json
├─ assets/ (models, textures, audio, fonts)
└─ localization/ (fi.csv, en.csv)
```

**Ratageneraattori:** valitsee kaupunginosan palikkapoolista sopivan jonon, instantioi ne peräkkäin ja kytkee tähtien ja esteiden sijainnit — uudet kentät syntyvät paloja yhdistelemällä ilman käsityötä per kenttä.

---

## 15. Laajuus ja MVP

**MVP (pelattava pystysiivu) sisältää:**
- Yksi kaupunginosa (keskusta), 1 valmis rata palikoista.
- Auton rakentelu vähintään 2 vaihtoehdolla per kategoria + maali.
- 3–4 pakettia eri vaatimuksin + "sopii keikkaan" -palaute.
- Ajo (kallistusohjaus + boost), tähtien keräys.
- Huoltoasema (korjaus + pesu kuplilla), pisteet ja kertymä.
- Tallennus laitteelle + 1 tallennettu auto.
- Vilkun perusohjeistus.

**MVP:n ulkopuolelle (myöhemmin):** kaikki 5 kaupunginosaa, kaikki 30 osaa ja 60 tarraa, ääninäyttely, lokalisointi, päivän keikka, lisäpalikat.

---

## 16. Tiekartta

Vaiheittain, kukin itsenäisesti esiteltävä (sopii portfolioon):

| Vaihe | Tavoite | Keskeiset tulokset |
|---|---|---|
| **0 – Prototyyppi** | Todista että ajaminen on hauskaa | 1 auto, 1 rata, kallistusohjaus, boost |
| **1 – MVP / pystysiivu** | Koko silmukka kerran läpi | Rakentelu, paketti-matchaus, huoltoasema, pisteet, tallennus |
| **2 – Sisältö** | Riittävästi valinnanvaraa | Kaikki osat, paketit, tarrat, 3 kaupunginosaa, ratageneraattori |
| **3 – Kiillotus** | Tunnelma ja saavutettavuus | Ääni, animaatiot, Vilkun puhe, EN-lokalisointi, asetukset |
| **4 – Julkaisu** | Kauppaan | Families-vaatimukset, ikonit/kuvankaappaukset, laitetestaus, beta, julkaisu |

---

## 17. Testaus ja laadunvarmistus

- **Lasten pelitestaus** kohderyhmän kanssa mahdollisimman aikaisin: ymmärtävätkö ohjeet ilman lukemista, onko ohjaus liian vaikea, mikä tuntuu hauskalta.
- **Laitematriisi:** muutama keskitason ja edullinen Android-puhelin; varmista 60 fps tai vakaa ruudunpäivitys ja kohtuullinen akunkulutus.
- **Toiminnallinen testaus:** paketti-matchaus, pisteiden laskenta, tallennuksen eheys, huoltotilan nollautuminen.
- **Saavutettavuustestaus:** värikontrastit, ääniohjeet, vaihtoehtoinen ohjaus.

---

## 18. Julkaisu

- **Kauppa:** Google Play, **Families**-ohjelmaan haku; vältetään mainoksia ja seurantaa.
- **Kauppasivu:** ikoni, kuvakaappaukset eri näkymistä, lyhyt video, selkeä kuvaus vanhemmille (mitä peli opettaa, ei mainoksia/ostoksia).
- **Lokalisointi:** suomi ja englanti; käännösavaimet alusta asti, jotta lisäkielet ovat vain dataa.
- **Ikäluokitus:** hae sopiva ikäluokitus (esim. PEGI 3 / ESRB Everyone), kun sisältö on vakaa.

---

## 19. Mittarit (eettisesti)

Mittareita käytetään pelin laadun, ei käyttöajan maksimoinnin arviointiin:
- Pääseekö uusi pelaaja ensimmäisen keikan läpi ilman jumitusta (tutoriaalin läpäisy).
- Ymmärrettävyys: kuinka moni valitsee oikean osan paketille ilman apua.
- Palaako pelaaja seuraavana päivänä (maltillinen retentio, ei pakottava).
- Vanhempien tyytyväisyys ja arviot turvallisuudesta.
- Kaatumis- ja suorituskykyraportit.

Vältetään tarkoituksella koukuttavia, käyttöaikaa venyttäviä kuviota lapsille.

---

## 20. Riskit ja niiden hallinta

| Riski | Hallinta |
|---|---|
| Laajuus paisuu | Pidä MVP tiukkana; lisää sisältöä vaiheittain |
| Suorituskyky halvoilla laitteilla | Low-poly, pooling, testaa aikaisin oikealla laitteella |
| Kallistusohjaus liian vaikea lapselle | Tarjoa kosketusohjaus ja herkkyyden säätö |
| Taiteen tuotantoaika | Yhtenäinen low-poly-tyyli ja uudelleenkäytettävät palaset |
| Retentio ilman pimeitä kuvioita | Palkitse luovuus ja keräily, ei pakkopelaaminen |
| Lapsiturvavaatimukset | Suunnittele yksityisyys sisään alusta (offline, ei mainoksia/keräystä) |

---

## 21. Liitteet

- **`pakettiporina_sisaltotaulukot.xlsx`** — radan palikat, 30 osaa ja 60 tarraa.
- Interaktiiviset mockupit (koti, keikat, halli, kartta, ajo, huoltoasema, tarrakirja) keskustelun visuaalisina liitteinä.

---

*Tämä dokumentti on elävä suunnitelma — päivitä sitä kun pelitestaus ja toteutus tuovat uutta tietoa.*
