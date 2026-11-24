# Pelitestaus 23.11.2026 (Unity Play, Galaxy S10e -simulaattori)

Testattu: Päävalikko, Asetukset, Halli, Ajo (Satama), Pesu. Korjattu = koodi valmis, ei vielä uudelleentestattu laitteella.

## Korjattu testin aikana
- [x] **Auton väri ajossa**: CarPainter maalasi väärää (näkymätöntä) mallia ja väärää paletin ruutua. Nyt kori on valittu väri.
- [x] **Kameran reikä peitti napit** (Halli: Koti, Asetukset: sulje). Uusi `CutoutAvoider` siirtää napit pois loven alta.
- [x] **Halli: auton esikatselun väri** oli väärä (punainen x keltainen = oranssi). `CarColor.Paint` värittää korin oikeasti, myös Pesun 2D-auton.
- [x] **Päävalikon auto** peitti napit ja tuplasi taustakuvan auton: pois päältä (`showCar`). Pilvet jäävät.
- [x] **Päävalikon pilvet** piirtyivät paneelien päälle: piilotetaan kun paneeli on auki.
- [x] **Puhekupla** katkaisi sanan ("Pakettipörinää/n!"): teksti sovittuu kuplaan.
- [x] **HUD-tekstit** (Tähdet, aika) olivat tummat taivasta vasten, lähtölaskenta magenta: valkoinen teksti + tumma reunus, laskenta keltainen.
- [x] **Auto putoaa maailmasta** (Satama: laiturilta mereen, kamera tuijotti vettä): `CarRecovery` palauttaa viimeiseen turvalliseen paikkaan.
- [x] **Pesu, ei likaa**: kuplat jäivät "Valmis"-tekstin päälle ja näkyi "Palkkio: +0": kuplat piiloon, palkkio vain jos > 0.

## Avoimet löydökset (ei vielä korjattu)
- [ ] Ajo: ei ratakaiteita; auto voi ajaa rannalle/laiturille. Horisontissa näkyy maailman reuna (tumma kaista): lisää taivas/sumu tai laajenna maasto.
- [x] Ajo: yläpalkin kaksi mittaria (lika/kolhut) ovat ilman selitettä; ensimmäisellä kerralla epäselvää mitä ne ovat. (korjattu myöhemmissä kierroksissa)
- [x] Ajo: pausevalikossa vain kaksi ikonia, "Koti" ilman vahvistusta. (korjattu myöhemmissä kierroksissa)
- [ ] Halli: osan vaihto ei muuta auton kuvaa (vain mittarit), joten valinta ei tunnu. Harkitse pientä ikonia/vilahdusta.
- [ ] Halli: mittarien nimet (Kylmyys, Keveys) koskettavat ikoneja; Vilkun puhekuplan teksti pieni.
- [ ] Halli: kategoriapainikkeiden teksti pieni (10-11 px simulaattorissa).
- [x] Asetukset: "Nollaa edistyminen" on listassa ilman vanhempainporttia. (korjattu myöhemmissä kierroksissa)
- [x] Pesu: 2D-auto on haalea/vaalea perusvärissä; lähtötilassa kuplia ja korjauskuvakkeita ilman ohjetta. (korjattu myöhemmissä kierroksissa)
- [ ] Ei testattu: maali- ja pesu-/korjausvaihe oikealla ajolla, Puisto/Huvipuisto/Maatila/Kylä, Tarrat, Tietoja/Tietosuoja-paneelit, kallistusohjaus laitteella.

## Huomio
Testin aikana Pesu-scene käynnistettiin suoraan, jolloin pelin tallennus kirjoittui hetkeksi ilman valittuja osia (0 osaa). Pisteet säilyivät (6117); valittu auto palautuu oletukseen ja valitaan hallissa uudelleen.

---

# Jatkotestaus: radat, maali, pesu ja koot (23.11.2026, 2. kierros)

Ajettu: Satama, Huvipuisto, Game (perusrata), Maatila ja Puisto (alku), Maali-paneeli, Pesu. Kylää ja Tarrapaneelia ei saatu auki.

## Korjattu tässä kierroksessa
- [x] Pesu: "Puhtaus: 100 %" jakautui kahdelle riville (kovavälilyönti, ei rivitystä).
- [x] Maali-paneeli: "Perillä!" oli magenta ruutulippukuvion päällä: keltainen teksti + tumma reunus, palkkio valkoinen reunuksella. HUD-reunus paksummaksi.

## Koot ja sijainnit suhteessa rataan (auto 1,2 x 2,4 m)
| Elementti | Havainto | Suositus |
|-----------|----------|----------|
| Lätäkkö (Latakko, mittakaava ~4) | Satamassa peittää ~2/3 tien leveydestä, Game-radalla ~2x auton leveys | Pienennä n. 0,7x tai sijoita sivuun; nyt väistäminen on vaikeaa lapselle |
| Kartio (0,6 x 0,4) | Kaukaa vain ~8 px, hädin tuskin näkyy | Suurenna ~1,4x tai lisää väriä/valkoinen raita |
| Laatikko (0,8) | Puistossa lähes auton korkuinen ja jäi auton viereen/eteen: auto näytti jäävän jumiin (testattava) | Tarkista törmäys ja sijoitus lähtöpisteen vierestä |
| Tähti | Puistossa lähes auton korkuinen; muualla pieni | Yhtenäistä kokoa; sijoita tielle, ei reunaan |
| Boost-levy (3 x 3) | Hyvä koko | Ei muutosta |
| Maali-portti ("MAALI"-kirjaimet) | Valtava; kamera jää portin sisään ja kirjaimet täyttävät ruudun | Pienennä porttia tai siirrä kameraa/pysäytä auto portin eteen |
| Koristeet (Huvipuisto: lippunauha/ilmapallokaari) | Valtavia ruudun yläreunassa lähdössä | Pienennä tai nosta pois kameran näkökentästä |
| Pesun auto | Pieni (~25 % leveydestä) verrattuna ~130 px kupliin | Suurenna autoa ~1,5x |
| Pesun korjauskuvakkeet | Pienet (~35 px), liikkuvat, jäävät suurten kuplien alle; kosketus ei osunut | Suurenna, nosta kuplien päälle, hidasta; piilota saippuakuplat kun Puhtaus 100 % |

## Muut löydökset
- [x] Pesun korjausvaihe ei kerro mitään (ei ohjetta, ei korjauslaskuria); "Puhtaus: 100 %" jää näkyviin. (korjattu myöhemmissä kierroksissa)
- [x] Ajon HUD: valkoinen aika/tähtiteksti hukkuu vaaleaan taivaaseen (Game, Satama); "Tähdet:" jää puun/koristeen päälle (Puisto). (korjattu myöhemmissä kierroksissa)
- [ ] Maatilan tie ja maasto lähes samaa harmaanbeigeä: tien reuna erottuu huonosti.
- [ ] Horisontissa tumma kaista (maailman reuna) Satamassa/Maatilassa.
- [x] Puisto: auto ei näyttänyt liikkuvan alun jälkeen (testaa laatikkoa vasten). (korjattu myöhemmissä kierroksissa)
- [x] Maali-paneelilla ei ole taustaa; teksti ja napit kelluvat suoraan kuvan päällä. (korjattu myöhemmissä kierroksissa)
- [ ] Pause-nappi (keltainen soikio) on pieni ja lähellä HUD:ia; mittarit (lika/kolhut) ilman selitettä.

---

# 3. kierros: korjaukset löydöksiin (23.11.2026)

Uudelleentestattu Playssa (Puisto -> maali -> Pesu): 0 virhettä Consolessa.

- [x] **Lätäkkö** pienennetty 0,75x (x, z) ja **kartio** suurennettu 1,4x kaikissa radoissa (Game, Puisto, Satama, Huvipuisto, Maatila). Huom: jos ajat editorityökalut `Lisäys/02` tai `Lisäys/03` uudelleen, ne voivat palauttaa vanhat koot.
- [x] **Laatikko ja kartio ovat liipaisimia** (`isTrigger`): auto hidastuu ja kolhiintuu, mutta ei jää jumiin (Puisto: auto jäi laatikon taakse ikuisesti).
- [x] **Maali**: auto jarruttuu pehmeästi pysähdyksiin maalissa (kamera ei enää jää portin sisään). Maali-paneelille tumma himmennys, tekstit keltainen/valkoinen.
- [x] **Pesun auton väri**: `FindObjectOfType<Canvas>` löysi SceneFaderin Canvasin, joten `Car`-kuvaa ei koskaan väritetty. Nyt haetaan koko scenestä: auto on valittu väri.
- [x] **Korjausvaihe**: saippuakuplat piiloon (viiveellä), "Puhtaus"-teksti piiloon, "Napauta korjauskuplia!" -ohje, korjauskuplat 1,3-1,8x isompia ja hitaampia, ylimpänä. Toiminta varmistettu (Kolhu korjattu 1/10 ... 4/10).
- [x] TMP-reunuksen NullReference estetty; kuplan coroutine-virhe (inactive) korjattu.
- [ ] **Peruttu**: Pesun auton suurennus 1,4x/1,2x. Likatahrat ja kolhumerkit ovat kiinteissä paikoissa, joten auto erkani niistä. Suurenna Editorissa auto ja tahrat yhdessä.

## Edelleen auki
- [ ] Maali-portti on edelleen suuri (kirjaimet); kamera ok jarrutuksen jälkeen.
- [ ] Huvipuiston lippunauhat/ilmapallokaari suuria lähdössä; Maatilan tien kontrasti; maailman reuna horisontissa; ratakaiteet.
- [ ] Pause: "Koti" ilman vahvistusta; mittarit (lika/kolhut) ilman selitettä; Pesun mittari-/tähtitekstit pieniä.
- [ ] Tähti: koko vaihtelee radoittain (Puisto lähes auton kokoinen).
- [ ] Ei testattu: Kylä, Tarrapaneeli, Tietoja/Tietosuoja, laitteen kallistusohjaus, Hidas/Nopea-nopeustasot.

---

# 4. kierros: Asetukset-alipaneelit (23.11.2026)

Testattu Playssa (Galaxy S10e): Tietoja, Tietosuoja, Nollaa edistyminen (vain Peruuta), Yötila-rivi nähty. Consolessa 0 virhettä.

- [x] **Tietosuoja**: teksti luettava ja mahtuu, ei katkea.
- [x] **Nollaa edistyminen**: vahvistusdialogi ("Tätä ei voi perua"), Peruuta toimii, pisteet säilyvät (6822).
- [ ] Sulkunappi (Tietoja/Tietosuoja) piirtyy ensin kameran reiän päälle ja siirtyy vasta sitten sivuun (`CutoutAvoider` skannaa 0,4 s välein): siirrä heti paneelin auetessa. Sulkunappi voi myös osua otsikkopillin päälle.
- [ ] Alipaneelin sulkeminen vie suoraan päävalikkoon, ei takaisin Asetuksiin.
- [ ] Nollaa-dialogin tausta on läpikuultava ja päävalikko näkyy sen läpi; lisää tumma himmennys. Nollaus on vain yksi vahvistus: lisää vanhempainportti (esim. pidä painettuna 3 s tai lasku).
- [ ] Ei vielä testattu: Yötilan kytkentä, Tarrapaneeli, Kylä.

## 5. kierros: korjaukset (23.11.2026)
- [x] **Nollaa edistyminen**: ensimmäinen avaus ei näyttänyt dialogia (`Start()` piilotti paneelin heti). Korjattu. Lisätty tumma himmennys (sidottu paneelin aktivointiin, ei jää päälle) ja kaksi painallusta ("Varmista"). Testattu: Peruuta sulkee, himmennys poistuu.
- [x] CutoutAvoider skannaa 0,1 s välein: sulkunappi on heti loven vieressä.
- [~] Pause "Koti": vaatii kaksi painallusta. Ei vielä testattu ajossa.
- HUOM: testauksessa Nollaa-polku suoritettiin vahingossa kerran, jolloin testitallenne palasi 6822 -> 45 tähteen. Ei vaikutusta koodiin.
- [x] Pause Koti testattu. Löytyi virhe: `Arial.ttf` ei ole enää sisäänrakennettu fontti (Unity 2022) -> vaihdettu `LegacyRuntime.ttf`. Vihjeteksti oli liian pieni -> best fit 30-80.
- [x] Lika-/kolhupalkeille sanaselitteet ("Lika", "Kolhut") ajon alussa 5 s (RaceHUD.ShowMeterHints), testattu Satamassa.
- [ ] Ratakaiteet ja maailman reuna: vaativat Editor-työtä (näkymättömät seinät/kaiteet ja taivas/sumu kunkin radan reunalle). Ei tehty koodilla.
