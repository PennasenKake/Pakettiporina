# Testaus ja laadunvarmistus

Päivitetty 24.11.2026.

## Testausstrategia

- **Editor-tarkistukset** (nopea, toistettava): valikko Pakettiporina → Tarkistus/01–08 (peliscene, halli, pesu, päävalikko, data, äänet, Hallin kolmivaihe, edistys). Aja ennen jokaista committia, jossa scenejä tai dataa on muutettu.
- **Pelitestaus Unityn Simulatorissa** (Galaxy S10e): kaikki näkymät läpi, muistiinpanot [Pelitestaus_23-11-2026.md](Pelitestaus_23-11-2026.md).
- **Saavutettavuus:** [Saavutettavuus_23-11-2026.md](Saavutettavuus_23-11-2026.md).
- **Kiillotuslista:** [TODO_Kiillotus_ja_UX.md](TODO_Kiillotus_ja_UX.md) on avointen asioiden ainoa totuus.

## Esittely- ja testityökalut (Editor)

| Valikko | Käyttö |
|---|---|
| Esittely → Kuvakaappaus | tallentaa `Docs/Esittely/kuva_HHmmss.png` (Play-tilassa) |
| Esittely → GIF-tallennus päälle tai pois | ~8 kuvaa/s kansioon `gif_frames`; poista kehykset GIF:n teon jälkeen |
| Esittely → Maalin efektit (testi) | laukaisee maalin (konfetti, välähdys, ääni) ajamatta maaliin |
| Tarkistus → 07 Hallin kolmivaihe | kuvat, pakettien ikonit ja radat, Build Settings, osakategoriat |
| Tarkistus → 08 Edistys | Progress.cs:n nimet, vaadittavat osat auki, oletusauto, radat Build Settingsissa (suunnitellut radat = info), ratalistojen yhteneväisyys (Progress ↔ RaceScenes ↔ PakettiporinaTracks.ALL), Hallin wizard-kuvat, salaiset osat taulukossa. Ajetaan myös Tarkistus/02:n perään |
| Tarkistus → 09 Rata | kaiteet, maaliportin koko, isot koristeet (avoin ratascene; raportti `Docs/Rata_turva_<scene>.md`) |
| Tarkistus → 10 Testit | ajaa EditMode-testit |
| Korjaus → 11 Ratakaiteet | luo kaiteet ja rajoittaa maaliportin koon avoimeen rataan (idempotentti; tallenna scene) |
| Korjaus → 10 Synkkaa Hallin kuvat | päivittää `Resources/WizardUI` lähdekuvista |

Huomioita: F-näppäimet (F12, F7) eivät välity Simulator-ikkunaan, joten käytä Esittely-valikkoa (Kuvakaappaus). Kuvat 24.11.2026 on otettu näin ja nimetty `Docs/Esittely/{paavalikko,halli_1_paketti,halli_2_rata,halli_3_auto,ajo,maali,wash}.png`. Skriptimuutosten jälkeen Ctrl+R kääntää; odota käännös ennen Playta.

## Pelitestaus lasten kanssa

Havainnoitavat asiat: ymmärtääkö kolmen vaiheen Hallin ilman lukemista (kuvat, ikonit, Vilkku), löytääkö Väri-välilehden, huomaako "!"-vihjeen puuttuvasta osasta, onko ohjaus liian vaikea, mihin jää jumiin. Ei vielä tehty oikeilla lapsilla.

## Laitematriisi

| Laite | Android-versio | Tulos | Huom |
|---|---|---|---|
| Unity Simulator (Galaxy S10e) | — | läpikäyty 23.11. | ei oikea laite |
| Oikea laite | — | **ei vielä testattu** | tarkista tekstien koko, kosketusalueet, suorituskyky |

## Suorituskyky

Tavoite: tasainen ruudunpäivitys keskitason puhelimella. Korjattu 23.11.: `Juice` skannaa napit 0,25 s vain 3 s scenen latauksen jälkeen, muuten 1,5 s välein (vain aktiiviset); CutoutAvoider vastaavasti. Partikkelien määrä tarkistettava laitteella (Huvipuisto, Maatila). Hiukkaset ovat poolattuja. Unity-editori voi jumittua muistin loppuessa pitkissä sessioissa (käynnistä uudelleen).

## Toiminnallinen testilista

- [x] Päävalikko → Halli → kolme vaihetta → Aja → rata käynnistyy valitulla autolla ja värillä (Huvipuisto, punainen auto).
- [x] Pakettien selaus (10 kpl, kuvat), oletusrata (Kirjapino), pienet tekstit.
- [x] Pause: Koti vaatii kaksi painallusta; Nollaa vaatii kaksi painallusta.
- [x] Maali-paneeli (tähdet, vihje, palkkio, "Uusi rata" -rivi) ja konfetti Esittely-testillä 24.11.; Lika/Kolhut-palkit piilotetaan maalissa (korjattu 24.11.). Oikea maaliinajo käsin vielä testaamatta.
- [ ] Lukitut paketit/osat (tähtiä 245 → kaikki auki; testaa Nollaa-edistyminen tai pienempi tallennus).
- [ ] "!"-vihje puuttuvasta osasta.
- [ ] Huvipuisto, Maatila, Kylä, Tarrat, Yötila.

## Saavutettavuustestaus

Kontrastit ja koko: ks. Saavutettavuus-dokumentti. Uusi halli käyttää 40–84 px tekstejä 1080x1920-canvasilla ja ≥ 64 px nappeja.

## Bugien kirjaus

Malli: otsikko, toistovaiheet, odotettu vs. tapahtunut, vakavuus, tila. Avoimet bugit: [TODO_Kiillotus_ja_UX.md](TODO_Kiillotus_ja_UX.md) osio 2 (mm. auto voi ajaa mereen, CarRecovery-silmukka).

## Suorituskyky, akku ja muisti (lisätty 23.11.2026)

Työkalut (valikko Pakettiporina → Suorituskyky): 01 automaattimittaus (Play; raportti `Docs/Suorituskyky_<pvm>_automaattimittaus.md`), 02 staattinen tarkistus, 03 ARM64-suositus (kysyy ensin), 04 SFX-esilataus.
Laitteella: Development Build, neljän sormen kosketus 1 s näyttää FPS/GC/muisti/akku (Editorissa F3). Overlay on oma komponenttinsa, ettei IMGUI sotke GC-mittausta.
Tulokset: Editorin ruudunajat eivät ole laitteen lukuja; piirtokutsut, kolmiot, GC ja muistin kasvu ovat vertailukelpoisia. Muistivuotoa ei havaittu (+0,2 MB / 4 kierrosta). Raskain rata Maatila (257 piirtokutsua ka, 467 max). Performant-profiili puolittaa piirrot ja kolmiot.
Laitetesti (puhelin, 10 min ajoa, akku-% ja lämpö) on vielä tekemättä.

## Tähtiporrastus ja radan valinta (23.11.2026)
Testattu Unityssa: Halli vaihe 2 selaa 5 rataa, lukittu rata näyttää "Vielä N tähteä" ja Lukittu-napin; Satama-ajo + maali (Esittely → Maalin efekti) näytti 2 tähteä, vihjeen ja +70 palkkion. Testaamatta: 3 tähteä, "Uusi väri" -ilmoitus, tallennuksen lataus uudelleenkäynnistyksessä, Wash-korjausvaiheen asettelu (kolhuteksti siirretty y=-420, ei kolhuja testissä). Kylä on suunniteltu myöhemmäksi radaksi (ei testattava nyt).

## Automaattiset testit (24.11.2026)
`Assets/_Pakettiporina/Tests/EditMode` (asmdef `Pakettiporina.Tests.EditMode`, Unity Test Framework 1.1.33): `ProgressTests` (RateRun, ratojen avaus, NewlyUnlocked, taulukoiden terveys, RaceScenes) ja `SaveAndRunStarsTests` (SaveData JSON-kierto, vanha tallennus ilman uusia kenttiä, rikkinäinen JSON, AwardRun, ApplyLoadedProgress). **29 testiä, kaikki läpi.** Ajo: valikko Pakettiporina → Tarkistus/10 (raportti `Docs/Testit_viimeisin.md`) tai Window → General → Test Runner → EditMode. Puuttuu: PlayMode-testit (RaceManager, halli), ajofysiikka, UI.

## Asetukset (24.11.2026)
Käsin Simulatorissa läpi: kaikki kytkimet ja valinnat, Tietoja, Tietosuoja, Nollaa (Peruuta), Yötila. Ks. TODO osio 14. Automaattinen testi alipaneelien avaukselle puuttuu (vaatisi PlayMode-testin MainMenu-scenellä).
