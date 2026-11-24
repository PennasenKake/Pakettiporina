# Ohjeet: äänet, Asetukset/Ohjeet-paneeli ja Maatila-rata käyttöön (11.8.2026)

Tämä dokumentti kertoo, mitä pitää tehdä **Unity Editorissa** jotta tämän session
kaikki muutokset (äänimaailma, Asetukset-paneelin uudistus, Ohjeet & palaute
-paneeli, Maatila-rata) tulevat oikeasti käyttöön pelissä. Kaikki koodi- ja
asset-tiedostot on tarkistettu automaattisesti levyltä (ks. "Tarkistuksen
tulos" alla) — mikään näistä ei ole arvailua.

## Tarkistuksen tulos (tehty suoraan levyllä olevista tiedostoista)

**Kunnossa:**
- Kaikki 13 tällä session muokattua/luotua C#-tiedostoa ovat sulkumerkeiltään
  tasapainossa (aaltosulkeet ja kaarisulkeet täsmäävät) — ei kääntövirheiden
  riskiä niiden osalta.
- Kaikki 16 äänitiedostoa (12 SFX + 4 musiikkia) ja kaikki 9 uutta/muokattua
  ikonia löytyvät oikeista kansioista, oikeilla nimillä, ja jokaisella on meta-tiedosto.
- Koko projektissa (781 meta-tiedostoa) ei ole yhtään duplikaatti-guidia.
- `Maatila.unity` + meta ovat kunnossa, `Kukkaruukku`-paketti osoittaa siihen,
  `Star.prefab` löytyy.
- `MainMenu.unity`/`Garage.unity`/`Wash.unity` ovat puhtaita vanhoista
  GroundCollider/StartPoint/CameraFollow-jäänteistä.

**Tärkeä havainto — tästä kannattaa lähteä liikkeelle:**

Tarkistin scene-tiedostot suoraan levyltä (en Consolen lokiviesteistä, jotka
eivät aina tarkoita että muutos on myös *tallennettu*). Tulos:

| Asia | Tila levyllä juuri nyt |
|---|---|
| `SettingsPanel` MainMenussa | On olemassa, mutta **vanhalla tumman sinisellä värillä** — tämän session värit/skaalaus/Ohjeet-nappi eivät ole vielä scenessä |
| `InfoPanel` ("Ohjeet & palaute") | **Ei ole vielä olemassa yhdessäkään scenessä** |
| `AudioManager`-komponentti | On olemassa jokaisessa scenessä paitsi Game2 |
| `AudioManager`:in äänikentät (countdownBeep, menuMusic, jne.) | **Kaikki 16 kenttää ovat tyhjiä (null) kaikissa scenessä** — yhtään ääntä ei siis vielä oikeasti soi |
| `ButtonClickSound` napeissa | **Puuttuu kaikista napeista kaikissa scenessä** |
| `CarEngineAudio` + `CarPainter` | Kunnossa radoilla Game, Puisto, Satama, Huvipuisto, Maatila |
| Game2-rata | **Ei CarEngineAudio, ei CarPainter, ei AudioManageria — tätä sceneä ei ole koskettu koko tämän session aikana** (tiedoston muokkausaika 28.7., kaikki muut 11.8.) |

Selitys: `Korjaus/08 – KORJAA äänet` -ajo keskeytyi viime kerralla
tallentamattomiin muutoksiin, eikä sitä ole sen jälkeen ajettu loppuun asti.
Siksi äänet eivät vielä toimi peliss vaikka koodi on täysin valmis. Tämä on
**helppo korjata** — ks. vaihe 3 alla.

---

## Tee nämä Unityssä, TÄSSÄ JÄRJESTYKSESSÄ

### 1. Avaa MainMenu-scene ja tallenna kaikki auki olevat muutokset

`File > Save` (Ctrl+S / Cmd+S) ennen kuin ajat mitään työkalua. Useampi alla
oleva työkalu vaihtaa avointa sceneä automaattisesti ja **keskeyttää itsensä**
jos nykyisessä scenessä on tallentamattomia muutoksia — juuri tästä syystä
äänien kytkentä ei mennyt läpi viime kerralla.

### 2. Rakenna Asetukset- ja Ohjeet-paneelit uudelleen

MainMenu auki → **Pakettiporina > Lisäys > 09 – Lisää/päivitä asetuspaneeli
päävalikkoon**

Tämä yksi ajo tekee kaiken tämän session UI-työn kerralla:
- Vaihtaa Asetukset-paneelin värin tumman sinisestä koralliksi (sama kuin Tarrat-paneeli)
- Suurentaa kaikki ikonit/fontit/liukusäätimet
- Rakentaa uuden **Ohjeet & palaute** -napin ja koko sen paneelin (ohjeet,
  tietoa pelistä, "Lähetä palautetta" -nappi)
- Lisää X-ikonin Sulje-nappeihin ja kirjekuori-ikonin palautenappiin
- Kytkee "Äänet päällä/pois" -radiopainikkeet

**Tarkista Console-ikkunasta:** lokin pitäisi päättyä riviin `Asetuspaneelin
kytkennät päivitetty.` eikä yhtään `VIRHE:`-riviä. Jos näet `VAROITUS: jokin
ikoni puuttuu` — kerro minulle, jokin ikonitiedosto ei ole latautunut oikein.

**Tallenna scene** (Ctrl+S) heti tämän jälkeen.

**Testaa Play-tilassa:** paina Asetukset-rataskuvaketta → paneelin pitäisi
näkyä korallinvärisenä (ei enää tumman sinisenä), isommilla teksteillä/ikoneilla.
Paina "Ohjeet & palaute" → oma paneeli avautuu ohjeteksteineen.

### 3. Kytke äänet kaikkiin sceneihin

MainMenu (tai mikä tahansa scene) auki, **ei tallentamattomia muutoksia** →
**Pakettiporina > Korjaus > 08 – KORJAA äänet (kaikki scenet)**

Tämä käy läpi kaikki 9 sceneä (Game, Game2, Puisto, Satama, Huvipuisto,
Maatila, MainMenu, Garage, Wash), kytkee kaikki 16 äänikenttää jokaiseen
AudioManageriin ja lisää `ButtonClickSound`-komponentin jokaiseen nappiin.
Työkalu tallentaa jokaisen scenen itse lopuksi — sinun ei tarvitse tallentaa
manuaalisesti välissä, **mutta älä koske Unityyn ajon aikana** (se vaihtaa
avointa sceneä useita kertoja peräkkäin).

**Tarkista Console-ikkunasta:** rivi `Käsitelty 9 sceneä. AudioManagereita
luotu: 0` *(koska ne on jo luotu aiemmin)* `klippejä kytketty: 144, nappeihin
lisätty ääni: <jokin luku>.` Jos scenemäärä on pienempi kuin 9 tai joku scene
puuttuu listalta, jokin meni pieleen kesken ajon.

**Testaa Play-tilassa:** MainMenussa pitäisi kuulua taustamusiikki heti, ja
napin painallus pitäisi antaa klik-äänen.

### 4. Korjaa Game2-rata (ei ole koskettu koko session aikana)

Avaa **Game2**-scene → **Pakettiporina > Korjaus > 01 – KORJAA peliscene**.
Tämä lisää puuttuvat `CarEngineAudio`/`CarPainter`-komponentit (muut radat
saivat nämä jo aiemmin, Game2 ei syystä tai toisesta). Tallenna scene.

Jos Game2 ei ole enää käytössä (esim. korvattu Gamella) — kerro minulle, niin
voin jättää tämän välistä eikä sitä tarvitse korjata.

### 5. Testaa Maatila-rata

Avaa **Maatila**-scene Play-tilassa, tai valitse Kukkaruukku-paketti
päävalikosta ja aja rata läpi. Tarkista: auto pysyy tiellä, tähdet toimivat,
maali laukeaa, moottoriääni kuuluu ja muuttuu kaasutuksen mukaan.

### 6. Lisää Maatila Build Settingsiin (ainoa asia jota en pysty tekemään puolestasi)

`File > Build Settings > Scenes In Build` → raahaa `Maatila.unity` mukaan jos
se ei jo ole listalla. En pääse käsiksi `ProjectSettings`-kansioon tästä
ympäristöstä, joten tämä yksi klikkaus jää sinulle.

---

## Jos jokin menee pieleen

- **Työkalu keskeytyy "tallentamattomia muutoksia" -viestiin:** paina Ctrl+S
  ja aja työkalu uudelleen.
- **Console näyttää `VAROITUS: jokin ikoni puuttuu`:** tarkista että
  `Assets/_Pakettiporina/Art/UI/Icons/`-kansiossa on kaikki `pakettiporina_icon_*.png`-
  tiedostot (info, sulje, kirje, ratas, nuotti, speaker_on/off, radio_ring/dot).
- **Äänet eivät kuulu Play-tilassa vaikka Korjaus/08 sanoi onnistuneen:**
  tarkista Asetukset-paneelista etteivät liukusäätimet ole nollassa ja ettei
  "Äänet pois" ole valittuna.
- **Palautesähköposti:** `InfoManager`-komponentin `Feedback Email`-kenttä
  (Inspectorissa, InfoPanel-objektissa) on tällä hetkellä placeholder
  `palaute@pakettiporina.fi` — vaihda se oikeaksi.

## Yhteenveto tämän session koodimuutoksista

| Tiedosto | Mitä |
|---|---|
| `AudioManager.cs` | 16 äänikenttää, per-scene automaattisoitto |
| `SettingsManager.cs` | Äänenvoimakkuudet, mykistys-radiopainikkeet, `OpenInfo()` |
| `InfoManager.cs` (uusi) | Ohjeet & palaute -paneelin logiikka |
| `ButtonClickSound.cs` (uusi) | Klik-ääni jokaiselle napille |
| `CarEngineAudio.cs` (uusi) | Moottoriäänen pitch/volume kaasutuksen mukaan |
| `PakettiporinaAudioSetup.cs` | Editor-työkalu: Korjaus/08 |
| `PakettiporinaSettingsSetup.cs` | Editor-työkalu: Lisäys/09 (paneelit, värit, skaalaus) |
| `PakettiporinaSetup.cs` | Ei-ratascenejen suojaus + CarEngineAudio-lisäys |
| `PakettiporinaStars.cs` | Tähtien luonti prefabista (RequireComponent-bugi korjattu) |
| `Maatila.unity` (uusi) | 5. rata |
| 9 uutta/muokattua ikonia | Settings/Ohjeet-paneelien kuvakkeet |
| 16 äänitiedostoa | Synteettiset SFX + musiikki |
