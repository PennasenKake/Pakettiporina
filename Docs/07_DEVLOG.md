# Kehityspäiväkirja

_(Lyhyet merkinnät edistymisestä, päätöksistä ja ratkaistuista ongelmista. Täydennä jokaisen vaiheen jälkeen.)_

## M1 — Ajettava auto
_(Päivämäärä. Mitä tehtiin, mikä toimi, mitä opittiin. Esim. arcade-kontrolleri + kamera + maa.)_

## M2 — Rata + maali
_(Päivämäärä. Keikan kulku, maalitriggeri, tähdet, restart.)_

## M3 — Rakentelu + paketit
_(Täytä.)_

## M4 — Huoltoasema
_(Täytä.)_

## M5 — Silmukka + meta
_(Täytä.)_

## M6 — Kiillotus

**23.11.2026.** Pitkä kiillotus- ja testauspäivä Unityn Simulatorissa.
- *HUD:* Lika- ja Kolhut-palkit uusittu (pitkät, päällekkäin, ikonit, väri vaihtuu tason mukaan); tähti- ja aikalaskurille ikonit ja tummat laatat.
- *Mehukkuus:* `Juice` (pooled-hiukkaset, pulssit, UI-konfetti, välähdys, leijuva "+1") ja `SoundSynth` (äänet koodilla). Maalin konfetti tehtiin UI-tilassa, koska maailmanhiukkaset jäivät maaliportin taakse.
- *Turvallisuus lapselle:* Nollaa ja Koti vaativat kaksi painallusta; tumma himmennys dialogin taakse.
- *Halli:* kolmivaiheinen `GarageWizard` (Paketti → Rata → Auto + Väri). Auton värinvalinta unohtui ensimmäisestä mockupista ja lisättiin omaksi välilehdeksi.
- *Dokumentaatio ja työkalut:* Tarkistus/07, Korjaus/10, Esittely-valikko; README-kuvat ja GIF.

## M6 — Päätökset (23.11.)
- Rata ei ole erillinen valinta: jokaisella paketilla on jo `trackScene`. Vaihe 2 on esikatselu. (Mockup antoi ymmärtää toisin.)
- UI rakennetaan koodilla scene-muutosten sijaan; vanha Halli piilotetaan, ei poisteta (`useWizard`-kytkin).
- Äänet ja tehosteet syntetisoidaan: ei binäärejä, ei Git LFS -kuormaa.

## M6 — Ongelmat ja ratkaisut (23.11.)
| Ongelma | Ratkaisu |
|---|---|
| Ensimmäinen Nollaa-painallus ei tehnyt mitään; himmennys esti napit | `ResetConfirmManager`: `openRequested`, himmennys sidottu `OnEnable/OnDisable` |
| `Arial.ttf` ei kelpaa Unity 2022:ssa | `LegacyRuntime.ttf` |
| Konfetti ei näkynyt maalissa | UI-konfetti `finishPanel`:iin |
| Wizard kaatui: `??` Unity-oliolla | eksplisiittinen `== null` |
| Auto näkymätön wizardissa | siirrä auto ennen kuin vanha UI piilotetaan CanvasGroupilla |
| Ratakortti tyhjä paketilla ilman rataa | `rata_perus.png` oletukseksi |
| Unity jumissa ~25 min (muisti) | uudelleenkäynnistys; vältä ForceMeshUpdate joka kehys |

## Päätökset ja perustelut
_(Esim. miksi arcade-fysiikka eikä WheelCollider; miksi deterministinen Vilkku.)_

## Ongelmat ja ratkaisut
_(Lyhyt loki: ongelma → ratkaisu.)_

## 23.11.2026 Suorituskyky
- Lisätty PerfProbe, PerfBenchmark, PerformanceManager (60 fps radoilla, 30 fps valikoissa, akkusäästö <20 %, NeverSleep vain ajossa, lowMemory-vapautus), Suorituskyky-työkalut (4 kpl).
- Kuumien silmukoiden harvennus: CutoutAvoider, Juice-skannaus, RaceHUD-laatat. SFX esiladataan.
- Auto-mittaus: ei muistivuotoa; Maatila raskain. Avoimet: ARM64 (työkalu 03), HDR pois, Docs/Kartat pois Assetsista, Kyla Build Settingsiin, laitetesti.

## 23.11.2026 Edistys: ajon tähdet, vapaa radan valinta, porrastetut lukitukset
- Rata valitaan Hallissa vapaasti nuolilla (paketti vain ehdottaa: "Sopii paketille"); valinta tallentuu (SaveData.selectedTrack).
- Ajon arvosana 1-3 (Progress.RateRun): maali / ei kolhuja / lisäksi kaikki bonustähdet (tai alle 60 s jos radalla ei bonuksia). Lisäpisteet (+10/tähti) vain parannuksesta. Maalipaneeli: RunStarsView.
- Kertyneet ajotähdet (GameManager.RunStars) avaavat sisältöä Progress-taulukon mukaan: värit 3-12, osat 4-24, paketit 3-40, radat 5-35. Taulukko koodissa, ei assetteissa; paketin requiredPart pidetään auki.
- Halli näyttää seuraavan palkinnon Vilkun kuplassa. Vanhat tallennukset: ajotähdet alkavat nollasta -> osa sisällöstä lukittuu uudelleen.

## 24.11.2026 Sanasto ja näkymät
- Kaksi valuuttaa eroteltu: KOLIKOT (pisteet, ikoni Juice.CoinIcon, kuluvat tarroihin) ja TÄHDET (ajotähdet, tähti-ikoni, avaavat osat/paketit/radat/kultaosat). Salaiset osat aukeavat nyt tähdillä (eivät uudelleenlukitu tarraostoista). Hallin ylärivillä molemmat laskurit.
- Maalipaneelin tausta umpinainen tummanvihreä (rata ei näy läpi).
- Pesu/korjaus: edistymispalkki siirretty tauko-napin alle, pill-tyyli.

## 24.11.2026 Editor-työkalujen tarkistus pelin nykytilaa vastaan
- Uusi: Tarkistus/08 - Tarkista edistys (Progress): taulukon nimet, vaadittavat osat, oletusauto, radat Build Settingsissä, salaiset osat; raportti Docs/Edistys_tarkistus_viimeisin.md.
- Korjattu: Lisays/06 (pakettigeneraattori) valitsi vaadittavaksi osaksi salaisen kultaosan (paras tilasto) -> nyt ohittaa salaiset; asettaa trackScene. Lisays/08 ja /07 puhuvat nyt ajotähdistä/kolikoista. Suorituskyky/02: HDR-varoitus vain jos HDR päällä, varoittaa pois päältä olevista sceneistä ja Docs-kuvista Assetsissa.
- Progress: paketit eivät aukea ennen ehdotettua rataa (Munakori 5, Synttärikakku 12, Tiiliskivet 22, Kukkaruukku 24, Jättijäätelö 26).
- Peli: tallennuksesta ei ladata lukittuja osia; pesun kolhuteksti siirretty palkin alle.

## 24.11.2026 (ilta): Editor-jäännökset
- Ratalista keskitetty: `Progress.RaceScenes` (AudioManager ja PerformanceManager lukevat sen) + `Progress.PlannedTracks` (Kylä = myöhemmin, oma ratamalli ja FBX-elementit; scene tarkoituksella pois Build Settingsista).
- Tarkistus/08 laajennettu: ratalistojen yhteneväisyys (Progress ↔ RaceScenes ↔ PakettiporinaTracks.ALL) ja Hallin wizard-kuvat. Tarkistus/02 (halli) ajaa sen perään.
- Suorituskyky/02: HDR tarkistetaan kaikista laatutasoista; suunniteltu rata ei ole varoitus. Suorituskyky/03 ajettu: Android vain ARM64.
- `Assets/_Pakettiporina/Docs` siirretty pois Assetsista -> `Docs/Arkisto_Assetsista` (ei turhaa tuontia).
- Wash: kolhuteksti siirretty y=-420 (palkin alle, varmuusvälillä).
- Avoin: HDR päällä URP-Balanced ja URP-HighFidelity (Android käyttää Balancedia).

## 24.11.2026 (yö): heikkousarvio, kuvat, repo
- Heikkoudet kirjattu README:hen (ei testejä, UI koodissa, suuret tiedostot, saveVersion ilman siirtymää, ratalistat, HDR, lisenssit, lokalisointi, laitetesti puuttuu).
- Korjattu: Lika/Kolhut-palkit piiloon maalissa; rikkinäinen tallennus -> `.rikki.bak`.
- Uudet kuvakaappaukset `Docs/Esittely` (päävalikko, halli 3 vaihetta, ajo, maali, pesu); README:n kuvataulukko päivitetty.
- Dokumentaatio päivitetty: 01, 02, 03, 04, 05, 08, 09, 11_VIRSTANPYLVAAT, Docs/README, TODO osio 12; vertailu uudelleennumeroitu 12_.
- Unity sulkeutui kahdesti istunnon aikana; avattu uudelleen Hubista.

## 24.11.2026 (yö 2): testit, pilkonta, bugit
- Lisätty EditMode-testit (29 kpl, läpi) ja Tarkistus/10 testien ajoon.
- Pilkottu Juice, GarageWizard, RaceHUD ja WashScreen partial-luokiksi (sisältö muuttumaton, käännös ja testit läpi).
- CarRecovery uusittu (historia, toistosuoja, maa-tarkistus); ratakaiteet kaikille 5 radalle (Tarkistus/09, Korjaus/11); Pause-nappi >= 48 dp. Maaliportti ja koristeet mitattu: eivät olleet liian isoja nyt.
- Commit `feat: tahtiporrastus...` tehty paikallisesti; push meni GitHub Desktopissa käyntiin kirjautumisen jälkeen.

## 24.11.2026 (yö 3): Asetukset-tarkistus
- Tarkistettu Asetukset päävalikosta Simulatorissa. Korjattu: Tietoja/Tietosuoja avautuivat vasta toisella painalluksella (Start-bugi); alipaneelien sulkeminen palaa Asetuksiin. Avoimet: sulje-ikoni, kontrasti, palautesähköposti (TODO osio 14).
- Asetukset palautettu oletuksiin testin jälkeen (musiikki, äänet, värähtely päällä; Molemmat; Keski; yötila pois).
