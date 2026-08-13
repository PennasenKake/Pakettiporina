# Projektiarvio — Pakettiporina (11.8.2026)

Kokonaisarvio tämänhetkisestä tilanteesta: koodikatselmuksen (10.8.), M-LISTA.md:n,
tiedostorakenteen ja tämän session (äänimaailma, Maatila-rata, Asetukset/Ohjeet-paneelit,
liukukytkin) pohjalta. Tarkoitus on antaa rehellinen tilannekuva — mikä on kunnossa,
mikä vaatii pientä siivousta ja mikä on isompi, tietoisesti myöhemmäksi jätetty puute.

## Yleiskuva

Pakettiporina on teknisesti hyvässä kunnossa harrastelijaprojektiksi. 63 C#-tiedostoa,
~8 400 riviä koodia, ei yhtään TODO/FIXME-merkintää, ei tunnettuja pelin kaatavia
bugeja, johdonmukainen arkkitehtuuri (singleton-manager-malli, event-pohjainen
GameManager, data-pohjaiset ScriptableObject-paketit/osat). Ydinsilmukka — aja rata,
kerää tähtiä, toimita paketti, ansaitse pisteitä, avaa tarroja/osia, muokkaa autoa
Hallissa — on kokonaan pelattavissa viidellä radalla. Tämä on selvästi enemmän kuin
"prototyyppi": se on lähellä sisällöltään valmista peliä, jolta puuttuu enää
julkaisukelpoisuuden viimeistely.

## Mikä on vahvaa

- **Arkkitehtuuri on yhtenäinen ja luettava.** GameManager/SaveManager/AudioManager
  noudattavat kaikki samaa `DontDestroyOnLoad` + duplikaattitarkistus-mallia.
  Tapahtumaväylä (`GameEvents`) pitää järjestelmät irrallaan toisistaan.
- **Data ohjaa sisältöä, ei koodi.** Radat (`PakettiporinaTracks.cs`), paketit ja
  osat ovat dataa, ei kovakoodattuja if-lauseita — uuden radan tai paketin lisääminen
  ei vaadi pelilogiikan muokkaamista, mikä näkyi konkreettisesti kun Maatila
  rakennettiin tähän session aikana huomattavasti nopeammin kuin jos geometria
  olisi ollut koodissa kiinni.
- **Kommentointi on poikkeuksellisen hyvää.** Moni kommentti selittää *miksi*, ei
  vain *mitä* (esim. SaveManagerin Script Execution Order -kommentti, RaceManagerin
  palkkiolaskennan järjestysperuste). Tämä on aliarvostettu laatumerkki — moni
  vastaavan kokoinen harrastelijaprojekti ei tee tätä.
- **Ei kertynyttä roskaa.** Nolla TODO/FIXME/HACK-merkintää koko koodikannassa on
  harvinaista tämän kokoisessa projektissa — viittaa siihen että asiat on joko
  tehty loppuun tai jätetty tietoisesti pois (ei "hoidan myöhemmin" -kulttuuria).
- **Editor-työkalut ovat toimineet suunnitellusti.** Koko "Tarkistus/Korjaus/Lisäys"
  -malli, jonka avulla tämä sessio on rakentanut äänimaailman ja UI:n ilman suoraa
  Unity-pääsyä, on osoittautunut toimivaksi ja turvalliseksi (idempotentit,
  tarkistavat tallentamattomia muutoksia, palauttavat alkuperäisen scenen).

## Mikä vaatii huomiota pian (ei kriittistä, mutta halpa korjata)

1. **M-LISTA.md on jäljessä todellisuudesta.** Se merkitsee mm. SettingsManagerin
   "tyhjäksi tynkäksi" ja äänet "ei kytketty" — molemmat ovat nyt (tämän session
   jälkeen) paikkansapitämättömiä väitteitä koodin osalta, joskin äänet eivät
   vielä ole *scenessä* kytkettyinä (ks. edellinen viesti — Korjaus/08 pitää ajaa).
   StickerGrid on jo 36/36, ei enää M-LISTAn mainitsema 24/36. Suosittelen
   päivityskierrosta dokumenttiin, koska se on ainoa paikka josta projektin
   kokonaistilanteen näkee nopeasti — jos se ei pidä paikkaansa, se menettää
   arvonsa.
2. **`ScreenManager.cs` on yhä tyhjä Unity-oletustynkä** (Start/Update, ei sisältöä).
   Navigointi hoituu suoraan `SceneManager.LoadScene()`-kutsuilla eri puolilla
   koodia, mikä toimii mutta ei ole yhdessä paikassa. Ei kiireellinen, mutta jos
   scenejä tulee lisää, kannattaa joko täyttää tämä luokka oikeasti tai poistaa
   se kokonaan — tyhjä placeholder on hämäävä sellaisenaan.
3. **Game2.unity vaikuttaa hylätyltä.** Muokkausaika 28.7., kaikki muut scenet
   11.8. Siinä ei ole `CarEngineAudio`-, `CarPainter`- eikä `AudioManager`-
   komponentteja, toisin kuin kaikissa muissa radoissa. En osaa sanoa onko se
   tarkoituksella jäänyt vanhaksi versioksi (korvattu Gamella) vai unohtunut —
   tämä kannattaa päättää eikä jättää roikkumaan, koska se on juuri sellainen
   asia joka aiheuttaa hämmennystä kuukauden päästä.
4. **10.8. katselmuksen pienet löydökset ovat yhä voimassa** (ei ole muuttunut
   tämän session aikana, koska en ole koskenut niihin tiedostoihin): kuollut
   kommentoitu koodi `CarBuilder.cs`:ssä, `Find()`/`ClearClicks()`-metodien
   kolminkertainen kopiointi eri Editor-työkaluissa, `VilkkuController` joka
   päättelee tilan UI-tekstin sisällöstä tekstivertailulla. Katselmus itse totesi
   ettei mitään näistä tarvitse tehdä ennen seuraavaa julkaisua — pätee yhä.
5. **Editor-työkalukoodia on nyt enemmän kuin ajonaikaista koodia** (n. 5 100
   riviä vs. n. 3 200 riviä). Tämä on ymmärrettävää tässä projektissa (koko
   tämän session UI-työ on tehty juuri näiden työkalujen kautta, koska minulla
   ei ole suoraa Unity-pääsyä), mutta jos projektin koko kasvaa paljon
   lisää, kannattaa pitää mielessä että osa näistä työkaluista (esim.
   `PakettiporinaSettingsSetup.cs`) on jo aika isoja yksittäisiä tiedostoja.

## Isompi, tietoisesti myöhemmäksi jätetty puute: M7 (julkaisu)

M-LISTAn mukaan koko M7-vaihe on vielä koskematta: tietosuojaseloste, ikäluokitus,
kauppalistaus, keystore/build-pipeline, **Parental Gate** ja mainos/ostologiikka.
Tämä ei ole ongelma tänään — peli ei ole vielä siinä pisteessä että näitä
tarvittaisiin. Mutta koska kohderyhmä on lapset, **Parental Gate puuttuu myös
Asetukset-paneelista**, jonka rakensimme tänään: jos "Lähetä palautetta" -nappi tai
mahdolliset tulevat osto/mainosnapit päätyvät lapsen käsiin ilman aikuisen
vahvistusta, se on tyypillinen asia jota sovelluskaupat (Google Play/App Store)
vaativat lastenkategorian sovelluksilta. Kannattaa pitää tämä listalla ennen
kuin peli lähestyy julkaisukuntoa — ei kiire, mutta ei myöskään jätettäväksi
viime tinkaan.

## Suositeltu järjestys seuraaville askeleille

1. Aja tämän session Editor-työkalut Unityssä (ks. edellinen ohje) — ilman tätä
   mikään tämän session sisällöstä ei ole vielä pelaajan nähtävissä.
2. Päätä Game2:n kohtalo (korjaa tai poista).
3. Päivitä M-LISTA.md vastaamaan nykytilaa — muuten dokumentti alkaa aktiivisesti
   harhauttaa.
4. Kun sisältö tuntuu valmiilta pelattavaksi kokonaisuudeksi: pieni siivouskierros
   10.8. katselmuksen listalta (ei kiireellinen, mutta halpa).
5. M7 vasta kun oikeasti lähestytään julkaisua — Parental Gate kannattaa kuitenkin
   pitää mielessä heti kun mikään osto/mainos/ulkoinen linkki -toiminnallisuus
   lisätään.

## Yhteenveto yhdellä lauseella

Koodi on parempaa kuin useimmissa tämän kokoluokan harrasteprojekteissa, sisältö
(viisi rataa, hallin muokkaus, tarrat, ääni, asetukset) on lähes kokonaan
pelattavissa — suurin riski juuri nyt ei ole koodin laatu vaan se, että projektin
oma tilannekuva (M-LISTA.md) ja scene-tiedostojen todellinen sisältö ovat
karanneet hieman erilleen nopean kehitystahdin takia.
