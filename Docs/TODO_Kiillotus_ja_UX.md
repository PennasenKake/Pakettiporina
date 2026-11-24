# TO-DO: kiillotus, mehukkuus ja käyttäjäkokemus

Päivitetty 23.11.2026 (pelitestauksen jälkeen). Merkinnät: [ ] tekemättä, [~] koodi valmis, osin testattu, [x] valmis ja testattu Unityssä.
Yksityiskohtaiset testilöydökset: `Docs/Pelitestaus_23-11-2026.md`.

## 0. Väribugi (auton väri ajossa ja pesussa) - VALMIS

- [x] Ajo: `CarPainter` maalaa Car1.fbx:n paletin korin ruudun valitulla värillä (aiemmin maalasi näkymätöntä kuutiota). Testattu Satamassa ja Puistossa.
- [x] Pesu: `WashScreen` haki `Car`-kuvaa väärästä Canvasista (SceneFaderin). Nyt haetaan koko scenestä, auto on valittu väri.
- [x] Halli: esikatselu värittyy oikeasti (`CarColor.Paint`), ei kertovaa sävyä.
- [x] Auton ala-osa ei enää leikkaudu Hallissa/Pesussa (`CarColor.Recolor` luki tekstuurin väärästä kohdasta).
- [ ] Varmista tallennus/lataus (väri säilyy sovelluksen uudelleenkäynnistyksen yli).

## 1. Ennen ajoa (UX)

- [~] Halli: auto pomppaa osan vaihtuessa, mittarit animoituvat, lukittu osa näyttää "Vielä X tähteä".
- [~] Halli: esivalittu toimiva auto ("Aja" heti).
- [~] Päävalikko: elävät pilvet taivaalla, Vilkun tervehdys, pistelaskuri rullaa. Pilvet vain päävalikossa (korjattu: ne vuotivat muihin sceneihin SceneFaderin Canvasin kautta).
- [~] Raskaille sceneille "Ladataan..." (Huvipuisto, Maatila).
- [x] Kameran reikä (notch) ei peitä nappeja (`CutoutAvoider`).
- [ ] Onboarding-vihje ensimmäiselle ajolle (2-3 s kuvavihje, Vilkun puhekupla, `SaveData.tutorialSeen`).
- [ ] Lähtölaskenta: numeron pulssi (väri jo korjattu keltaiseksi), kirkas "AJA!", kameran sisäänajo.
- [x] Radan nimi ja esikatselu paketin valinnan yhteydessä (Hallin vaihe 2).
- [x] Halli jaettu kolmeen vaiheeseen (Paketti, Rata, Auto); yksi asia kerrallaan, isommat tekstit, tarina tilaus -> reitti -> auto -> ajo.
- [x] Kolmivaiheinen Halli toteutettu 23.11.2026 (`GarageWizard.cs`, rakentuu ajonaikaisesti GarageScreenin päälle; `useWizard`-valinta GarageScreenissä). Vaiheet: Paketti, Rata (esikatselukuva paketin `trackScene`:stä), Auto (osat, mittarit, VÄRI-välilehti, Aja + konfetti). Testattu Unityssä: 0 virhettä, väri (punainen) säilyy ajoon Huvipuistossa. Vanha UI on piilotettu (CanvasGroup), ei poistettu. Kuvat: Resources/WizardUI (kopiot ikoneista; yhdistä myöhemmin).
- [x] Hallin läpikäynti 23.11.: paketit (10 kpl, kuvat ok), rata-vaihe (oletusradalle kuva `rata_perus.png`, ratakohtaiset vihjeet Vilkulle, isompi ratakuva), pakettien nimet oikein (Jättijäätelö, Synttärikakku), konfettitykit "Aja!"-napissa, "!"-merkki välilehdelle jos paketti vaatii puuttuvan osan (osan reuna oranssi). Testaamatta: "!"-vihje näytöllä (paketti jonka vaatimaa osaa autossa ei ole), lukitut paketit/osat (tähtiä 245 -> kaikki auki).
- [ ] Halli, oikea laite: Vilkku/tekstien koko, kosketusalueet, Maali-tabin värivalinta (nyt leveät pillit; pyöreät swatchit olisivat selkeämmät).
- [ ] Halli: lukittujen pakettien ja osien ulkoasu (testaa pienellä tähtimäärällä), "!"-vihje puuttuvasta osasta.
- [ ] Halli: yli 9 osaa samassa kategoriassa vaatisi vierityksen (nyt enintään 3x3).
- [ ] Halli: osan vaihto ei muuta auton kuvaa (vain väri muuttuu); osakohtaiset kuvat tai muotomuutokset.
- [ ] Halli: README-kuva `halli.png` on vanha yhden näkymän halli; ota uudet kuvat vaiheista (Esittely -> Kuvakaappaus).
- [ ] Halli: ratakuvat ovat piirrettyjä; korvaa halutessasi oikeilla ruutukaappauksilla. `Resources/WizardUI` sisältää kopioita (Korjaus/10 synkkaa); harkitse yhteistä lähdettä.
- [x] Kolmivaiheisen Hallin mockup: Docs/Esittely/Halli_kolmivaiheinen_mockup.html; ratakuvat `Art/UI/Rata/rata_*.png` (Satama, Puisto, Huvipuisto, Maatila, Kylä, Perus).
- [x] Hallin vanhat pienet tekstit: ratkaistu uudella kolmivaiheisella Hallilla (vanha `GrowSmallTexts` jää varalle `useWizard = false` -tilaan).
- [~] Äänet ja efektit: `SoundSynth` (konfetti, varoitus, whoosh, arm), `Juice.Flash/FloatText/UIConfettiCannons` kytketty maaliin, kolhuun, boostiin, tauko, nollaus, tähti. Maalin konfetti tarkistettava valikosta Pakettiporina/Esittely/Maalin efektit (testi) Satama-scenessä.
- [ ] Teemapuiston kartta: merkki ajetuista radoista.
- [ ] Päävalikko: auto ajamaan tien päällä (nyt pois, peitti napit).
- [ ] Tarkista oletusasetukset (ohjaustapa, nopeus) lapselle sopiviksi.

## 2. Pelin sisäiset korjaukset (testilöydöksistä)

Valmiit:
- [x] Auto ei jää jumiin esteisiin (laatikko/kartio ovat liipaisimia).
- [x] Putoamissuoja (`CarRecovery`): auto palaa viimeiseen turvalliseen paikkaan.
- [x] Maali: auto jarruttaa pysähdyksiin, tumma himmennys paneelin taakse, tekstit luettavia.
- [x] HUD-tekstit valkoisina tummalla reunuksella; lähtölaskenta keltainen.
- [x] Lätäkkö pienennetty 0,75x, kartio suurennettu 1,4x kaikilla radoilla.
- [~] Taivaan pilvet pelisceneissä: pieni määrä ruudun ylälaidassa (taivas), HUDin alla, ei tiellä (`SkyClouds`).
- [~] Pesu: kolhumerkit näkyviin autoon (olivat scenessä kaukana autosta ja auton takana), edistymispalkki sekä pesulle että korjaukselle ("Korjaus: 2 / 10"), korjauskuplat isompia, saippuakuplat piiloon korjausvaiheessa.
- [x] Pesun korjausnapautukset toimivat ("Kolhu korjattu" 1/10...).

Auki:
- [ ] Maali-portti suuri (kirjaimet täyttävät ruudun lähellä); lippunauhat/ilmapallokaari Huvipuistossa suuria lähdössä.
- [ ] Ratakaiteet ja maailman reuna horisontissa (Satama, Maatila); Maatilan tien ja maaston kontrasti.
- [ ] Tähden koko vaihtelee radoittain (Puisto lähes auton kokoinen).
- [x] Pause: "Koti" vaatii kaksi painallusta (vihje "Paina Koti uudelleen"); testattu Satamassa: ensimmäinen näyttää vihjeen, toinen vie päävalikkoon. Vihjeen fontti kasvatettu (testaa lukukelpoisuus).
- [x] Yläpalkin lika/kolhumittareille sanaselitteet ajon alussa 5 s (testattu).
- [ ] Pesun auto pieni verrattuna kupliin: suurenna Editorissa auto ja tahrat/kolhut yhdessä.
- [x] Hallin pienet tekstit (kategoriapainikkeet, mittarien nimet, Vilkun kupla): ratkaistu kolmivaiheisella Hallilla.
- [x] "Nollaa edistyminen": kaksivaiheinen vahvistus (ks. yllä).

## 3. Mehukkuus (kohta 17)

Toteutus: `Scripts/Core/Juice.cs`. Partikkelit ja materiaali luodaan ajonaikaisesti koodissa (Sprites/Default), joten ei uusia binaaritiedostoja. Päivitys 23.11.2026: koodi KÄÄNTYY (Console 0 virhettä). Tähden kimallus nähty ajossa (partikkelit toimivat). Konfetin koko/kirkkaus kasvatettu, mutta maalissa ei vielä nähty silmin (auto ei ohjaudu itse maaliin). Editorissa pikatestinäppäimet: F8 kimallus, F9 konfetti, F10 kipinät, F11 roiske (pelinäkymä aktiivisena). Muistivaroitus: Windowsin sivutustiedosto täynnä testin aikana, editori hidas.
- [~] `Juice.cs`: pooloitu partikkelisysteemi (14 kpl), pulssi, puristus, UI-hiukkaset, nappien painuma (`JuiceButtonPress` lisätään kaikille Button-komponenteille automaattisesti).
- [~] Pulssit ja puristus: tähtiteksti pulssaa tähden kerätessä (`RaceHUD.HandleStar`), napin painuma kaikissa napeissa, auton visuaalin puristus kolhussa (`ArcadeCarController.Bump`). Pisteluvun pulssi oli jo (`PointsDisplay`).
- [~] Partikkelit radalla: tähden kimallus (`Pickup`), kolhun kipinät (`Bump`), lätäkön roiske (`Puddle`), boostin nopeusviivat (`ApplyBoost`).
- [~] Maalin konfetti: käyttäjän kuvakaappaus 23.11. osoitti, ettei se näkynyt (maailman partikkelit jäivät maaliportin taakse). Siirretty UI-tasoon (`Juice.UIConfetti`, 70 paloja putoaa maali-paneelin päälle, 4 s). Koodi valmis, EI testattu (editori oli suljettu). F9 = pikatesti.
- [x] HUD-saavutettavuus: tähti-/aikatekstin taakse tumma laatta, mittarit uudelleen asetettu (kaksi samankokoista pilleriä: ikoni, selite, palkki). Nähty Simulatorissa Huvipuistossa 23.11. (ikonit, laatat, mittarit näkyvät). Maali-himmennys 0,6 vielä katsomatta. Katso `Docs/Saavutettavuus_23-11-2026.md`.
- [x] Lika/Kolhut-palkit pidennetty 23.11. (kaksi pitkaa pillia allekkain, 62 % ruudun leveydesta, testattu Satamassa: ura ja taytto nakyvat selvasti).
- [x] Lika/Kolhut-palkit uusiksi 23.11.: teräskehys, tumma ura, täyttö, ikoni + selite + palkki (`Juice.BarFrameSprite/BarFillSprite`). Nähty Simulatorissa (Huvipuisto).
- [x] Tähti- ja kello-ikonit pisteille ja ajalle (`Juice.StarIcon/ClockIcon`, laatan sisällä). Nähty Simulatorissa.
- [x] Ohjausnapit "hävisivät" (käyttäjän raportti 23.11.): varotoimi tehty (Juice ei kosketa TouchButton-nappeja). Ohjausnapit (vasen, jarru, oikea) näkyvät Huvipuistossa 23.11. Jos ongelma toistuu laitteella, kerro mitä näkyy.
- [~] README: päävalikko, halli (vanha), ajo ja GIF (Satama) lisätty (Docs/Esittely); README-teksti päivitetty 23.11. (kolmivaiheinen halli, mehukkuus, 38 työkalua). Puuttuu maali-paneeli (käytä Esittely -> Maalin efektit (testi)), pesu ja uusi halli. Kaappaus: Unity-valikko Pakettiporina/Esittely.
- [x] BUGI: auto voi ajaa mereen / Recovery palautti samaan kohtaan (24.11.): kaiteet kaikille 5 radalle (Korjaus/11, tallennettu sceneihin) + CarRecovery käyttää historiaa (väh. 1,5 s taaksepäin, toistossa yhä vanhempaan, maa-tarkistus, suoja-aika). Maaliin pääsy testattu Game-radalla näppäimillä. Testaamatta: ajo kaiteeseen Satamassa käsin; laitetesti.
- [x] Pause-napin kosketusalue vähintään 48 dp (`PauseMenu.EnsureMinTouchSize`, 24.11.; laitteella tarkistettava)
- [~] Pesun UI-hiukkaset: kuplan poksahdus (`Bubble.TryPop`) ja kolhun korjaus (`WashScreen.RepairNextDent`).
- [ ] Laitetesti suorituskyvylle (Huvipuisto, Maatila). Tarkista erityisesti: nappien automaattiskannaus (0,6 s välein) ja partikkelien määrä.
- [~] Lika-/kolhumittarit: sanaselitteet "Lika"/"Kolhut" pysyvästi, palkin väri muuttuu vakavuuden mukaan (keltainen -> punainen, lika vaalea -> tumma ruskea). Ei vielä testattu uudella värityksellä.

## 4. Omat kuvat korjaukselle ja kolhuille (kohta 18)

- [ ] Vasara/ruuvimeisseli-ikoni korjauskuplaan (nyt ruuvikupla-sprite).
- [ ] Kolhukuvat: lommo, naarmu, halkeama (nyt yksi kolhukuva).
- [ ] `PakettiporinaWashSetup` arpoo kolhun tyypin, aja `Korjaus/03`.
- [ ] Kuvat commitoidaan omalta koneelta Git LFS:llä.

## 5. Muu peli

- [ ] Kylä-rata: maasto, koristeet, esteet, äänet, tai piilota se (ei vielä testattu).
- [ ] "Puhdas ajo" -palkkio Pesussa.
- [ ] Tarkista Tietoja-paneelin pieni epäselvä merkki (`InfoPanel > InfoScroll > TekijaHeaderRow`).

## 6. Unity-ajot ja tarkistukset

- [ ] `Korjaus/01`, `Lisäys/02`, `Lisäys/03`, `Korjaus/03`, `Korjaus/08` tarvittaessa (HUOM: `Lisäys/02` ja `Lisäys/03` voivat palauttaa lätäkön ja kartion vanhat koot).
- [ ] Build Settings: kaikki 9 scenea mukana (Tarkistus/07 tarkistaa pakettien ratascenet; aja se).
- [ ] Laitetesti: Hidas/Nopea-nopeustasot, kallistusohjaus, suorituskyky, Android-takaisin-nappi, kameran reikä oikealla laitteella.
- [ ] Testaamatta: Kylä, Tarrapaneeli, Yötila. Testattu 23.11.: Tietoja, Tietosuoja, Nollaa (Peruuta).
- [~] Sulkunappi loven alla hetken: CutoutAvoider skannaa nyt 0,1 s välein (testaa).
- [x] Nollaa-dialogi: tumma tausta (scrim) + kaksivaiheinen vahvistus ("Nollaa" -> "Varmista" 3 s sisällä); ensimmäinen avaus ei enää piiloudu (Start-bugi). Testattu Unityssä.
- [ ] Alipaneelin sulku takaisin Asetuksiin (nyt päävalikkoon).

## 7. Julkaisu (Google Play)

- [ ] Tietosuojaseloste, IARC-ikäluokitus, vanhempainportti, allekirjoitusavain ja AAB, kauppalistaus, palautesähköposti.
- [ ] README: kuvakaappaukset ja GIF.

## Suositeltu järjestys (päivitetty 23.11.2026)

1. **Laitetesti oikealla puhelimella** (uusi halli, HUD, suorituskyky, kosketusalueet): löytää eniten ongelmia nopeimmin.
2. ~~Bugi: auto ajaa mereen; maali-portti ja koristeiden koot; Pause-nappi ≥ 48 dp~~ (korjattu 24.11., ks. osio 13).
3. Maali-paneelin ja Pesun tarkistus ja README-kuvat (myös uusi halli).
4. Onboarding-vihje ja lähtölaskenta.
5. Kolhu- ja korjauskuvat; pesun auton koko.
6. Kylä ja julkaisu (Google Play -vaatimukset).

Valmiina 23.11.: väribugi, HUD-mittarit ja ikonit, Nollaa/Koti-vahvistukset, perusmehukkuus ja äänet, kolmivaiheinen halli, Editor-työkalut (Tarkistus/07, Korjaus/10, Esittely), dokumentaatio.

## 8. Suorituskyky ja akku (23.11.2026)
- [x] PerformanceManager, kuumien silmukoiden harvennus, SFX-esilataus, mittaustyökalut
- [ ] Aja Suorituskyky/03 (vain ARM64) ja tee uusi build
- [ ] HDR pois URP-Balancedista; Performant-profiili hitaille laitteille
- [x] Siirrä Docs pois Assets-kansiosta (24.11.: nyt `Docs/Arkisto_Assetsista`)
- [~] Kyla: tehdään paljon myöhemmin (oma ratamalli + FBX-elementit); tarkoituksella pois Build Settingsista
- [ ] Maatila: staattinen batching koristeille (257 piirtokutsua)
- [ ] Shader-esilämmitys (pahimmat ruudut 95-309 ms ensilatauksessa)
- [ ] Laitetesti Development Buildilla (FPS, lämpö, akku/10 min); minSdk 31 rajaa vanhat puhelimet

## 9. Tähtiporrastus (23.11.2026)
- [x] 1-3 tähteä per ajo, vapaa radan valinta, porrastetut lukitukset (Progress.cs)
- [ ] Testaa 3 tähteä ja "Uusi väri" -ilmoitus maalissa
- [ ] Näytä ajotähtien kokonaismäärä Hallin ylärivillä (nyt vain piste-pilli) ja erota sanat "pisteet" / "tähdet"
- [~] Kyläradan avaus: siirretty myöhemmäksi (ks. yllä)
- [ ] Tarrat nopeaksi palkinnoksi (ilmainen tarra joka 5. tähdestä)
- [ ] Säädä Progress-taulukon rajat pelitestin perusteella

## 10. Sanasto ja näkymät (24.11.2026)
- [x] Kolikko-ikoni pisteille, tähti ajotähdille; maalipaneelin tausta; pesun palkin sijoittelu
- [x] Lika/Kolhut-palkit piilotetaan maalissa (24.11., varmistettu kuvasta)
- [ ] Tarrapaneelin hintojen viereen kolikko-ikoni (nyt pelkkä luku)
- [ ] Pesun tauko-nappi oikeaan yläkulmaan kaikissa sceneissä yhtenäisesti

## 11. Editorityökalut (24.11.2026)
- [x] Tarkistus/08 ei varoita Kylästä (PlannedTracks)
- [~] Ratalista keskitetty osittain (`Progress.RaceScenes`); PakettiporinaTracks/Decor/Obstacles vaativat yhä omat rivinsä, Tarkistus/08 valvoo
- [x] Tarkistus/02 ajaa Tarkistus/08:n perään

## 12. Heikkousarvio ja viimeistely (24.11.2026)
- [x] ARM64 käyttöön (ARMv7 pois); HDR-tarkistus kaikista laatutasoista
- [x] Rikkinäinen tallennus kopioidaan `.rikki.bak`-tiedostoksi
- [x] Kuvakaappaukset (päävalikko, halli x3, ajo, maali, pesu) ja README päivitetty
- [ ] **Laitetesti oikealla puhelimella** (suurin tuntematon)
- [x] Automaattiset testit (RateRun, NewlyUnlocked, SaveData): 29 EditMode-testiä läpi (24.11.). Jatko: PlayMode-testit, RaceManager-integraatio
- [ ] Tallennuksen versiosiirtymä (`saveVersion`) ennen ensimmäistä julkaisua
- [ ] Lokalisointi: ~300 kovakoodattua suomenkielistä tekstiä
- [ ] Lisenssi-/lähdeinventaario (Docs/05) – este FBX-/työkalumyynnille
- [ ] HDR pois URP-Balancedista (tai todeta laitteella ok)
- [x] Suuret UI-tiedostot pilkottu partial-luokiksi (24.11.): Juice 341 + 3 osaa, GarageWizard 317 + 2, RaceHUD 384 + 1, WashScreen 375 + 1
- [ ] Wash-korjausvaiheen asettelu silmämääräisesti (kolhuteksti y=-420)
- [ ] Tarrojen hinnan viereen kolikko-ikoni; pesun tauko-napin sijainti yhtenäiseksi
- [ ] Vanhempainportti ja Google Play -vaatimukset

## 13. Bugikorjaukset (24.11.2026)
- [x] Ratakaiteet (Tarkistus/09, Korjaus/11) lisätty Game, Satama, Puisto, Huvipuisto ja Maatila -sceneihin ('Ratakaiteet'-juuri, näkymättömät BoxColliderit, kitkaton materiaali `Data/Kaide_ei_kitkaa`)
- [x] CarRecovery: historia, toistosuoja, maa-tarkistus
- [x] Maaliportti: koko 6,8 x 5,6 x 0,9 kaikilla radoilla (alle rajojen 18 x 9) -> ei enää "valtava"; ei muutettu
- [x] Koristeet: vain Huvipuisto_Maailmanpyora on iso (29 m, tarkoituksella); muut alle rajojen
- [x] Pause-nappi >= 48 dp
- [ ] Tarkista Simulatorissa/laitteella: kaiteen kohdalla ajo ei tökkää (kitkaton), Satamassa kaide ei peitä näkymää mereen
- [ ] Kyläradalle (myöhemmin) sama Korjaus/11 kun rata on valmis

## 14. Asetukset-tarkistus päävalikosta (24.11.2026, Simulator)
Käyty läpi: Musiikki, Äänet, Värähtely (kytkimet), Ohjaus (Napit/Kallistus/Molemmat), Ajonopeus (Hidas/Keski/Nopea), Tietoja pelistä, Tietosuoja, Nollaa edistyminen (vain Peruuta), Yötila. Valinnat pysyvät paneelin uudelleenavauksessa, yötila himmentää näkymän.
- [x] BUGI korjattu: Tietoja- ja Tietosuoja-paneeli piilottui heti ensimmäisellä avauksella (`Start()` komponentissa joka on paneelin päällä; sama kuin Nollaa-dialogissa 23.11.). Nyt `openRequested`-lippu (InfoManager, PrivacyManager); testattu: avautuu ensimmäisellä painalluksella.
- [x] Alipaneelien (Tietoja, Tietosuoja, Nollaa) sulkeminen palaa nyt Asetuksiin (`SettingsManager.ReopenAfterSubpanel`), ei päävalikkoon. Androidin takaisin-nappi toimii samoin.
- [ ] Sulje-/takaisin-nappi käyttää "ovi ulos" -ikonia (sama kuin Lopeta päävalikossa) ja osuu otsikon pillin päälle; vaihda nuoli-/rasti-ikoniin. Kameran reikä (S10e) peittää nappia hetken ennen CutoutAvoiderin siirtoa.
- [ ] "Aikuiselle"-otsikon lukkoikoni on pieni; tekstit (Napit/Kallistus/Molemmat) pieniä ja tummaa tekstiä vihreällä valinnalla: kontrasti heikko.
- [ ] Palautesähköposti on oletuksena `palaute@pakettiporina.fi` (InfoManager): vaihda oikeaan osoitteeseen ennen julkaisua. "Lähetä palautetta" -nappia ei painettu testissä (avaa sähköpostiohjelman).
- [ ] Nollaa-vahvistuksen varsinaista nollausta ei testattu (tuhoaisi editorin tallennuksen).
- [ ] Testaa laitteella: asetusten säilyminen sovelluksen uudelleenkäynnistyksessä (tallennus tehdään paneelia suljettaessa; jos sovellus tapetaan paneeli auki, muutokset voivat jäädä tallentamatta).

## 15. Pilvet ja kiinteät objektit (24.11.2026)
- [x] Päävalikon pilvet (`MainMenuLife`, 5 kpl) ja ajonäkymän pilvet (`SkyClouds`, 4 kpl) löytyvät ja liikkuvat; varmistettu Playssa. Ajonäkymässä taivas on vain ylin ~19 %, joten pilvet nostettu näkyvämmiksi (alpha 0,75–0,95, kaista 10–17 %).
- [ ] Päätös kiinteistä objekteista: koristeet (`PakettiporinaDecor`) ovat tarkoituksella ilman colliderta; ks. 07_DEVLOG 24.11. Ehdotus: lisää collider vain isoille koristeille lähellä tietä (työkalu), kaiteet pitävät auton radalla.
