# Artikkelisarjan runko: "How to build your own game with Unity, Blender and Claude"

M-listan (virstanpylväsdokumentin) pohjalta muokattu 10-osainen, askel askeleelta
etenevä tutorial-sarja. Jatkaa/tarkentaa `Idea_TutorialArtikkelit_2026-08-20.md`:ssä
sovittua mallia (ilmainen artikkeli + julkinen repo, maksullinen asset-paketti).
Ei aloitettu — tuotantovalmis runko sitä varten kun kirjoittamiseen palataan.

## Periaatteet joita jokainen osa noudattaa

- **Yksi osa = yksi toistettava lopputulos.** Lukija pystyy seuraamaan osan
  itsenäisesti ja saa aina jotain oikeasti pelattavaa/näkyvää valmiiksi lopussa,
  ei pelkkää teoriaa.
- **Koukku → ongelma → ratkaisu vaiheittain → repo-linkki.** Sama kaava kuin
  alkuperäisessä ideassa.
- **"Claude tässä" -laatikko jokaisessa osassa** — lyhyt, rehellinen kuvaus mihin
  tekoälyavustaja tässä nimenomaisessa vaiheessa auttoi ja mihin ei (päätökset,
  debuggaus ja integrointi olivat aina käyttäjän — ks. osa 9 tarkemmin).
- **Repo-merkintä per osa**: mitä koodia/assetteja kyseinen osa julkaisee
  ilmaiseksi repoon, ja mikä kuuluu erilliseen maksulliseen pakettiin (FBX-mallit,
  valmiit palettitekstuurit, Editor-työkalut kokonaisuudessaan).

---

## Osa 1 — Projektin pystytys: Unity + Blender + Claude yhdessä

**Tavoite:** tyhjästä toimivaan Unity 2022.3 URP -projektiin, oikea
kansiorakenne, versionhallinta, ja lyhyt selitys MIKSI juuri tämä työkaluyhdistelmä
(ei pelkkää "asenna Unity" -tylsyyttä).

- Unity-projektin luonti (3D URP -template), Active Input Handling = Both.
- `_Pakettiporina`-kansiorakenne, `.gitignore`, `.asmdef`, Git-repo pystyyn.
- Lyhyt "miksi Blender + Unity + Claude" -osio: mihin kukin työkalu sopii tässä
  projektissa (Blender = matalapoly-mallit + palettitekstuurit, Unity = pelimoottori
  + Editor-automaatio, Claude = pariohjelmointikumppani scenetiedostojen ja
  koodin kanssa — ei "taikaa", vaan työkalu jota käyttäjä ohjaa).
- **Repo tähän osaan**: projektin runkorakenne + `.gitignore`/`.asmdef`-pohjat.

## Osa 2 — Blenderistä Unityyn: palettitekstuuri-tekniikka

**Tavoite:** ensimmäinen matalapoly-malli Blenderistä Unityyn niin että väri
säilyy oikein ja skaala ei ole 100x pielessä.

- Palettitekstuuri-tekniikka: yksi materiaali + pieni PNG per malli-perhe (ei
  kymmeniä erillisiä materiaaleja).
- FBX-vientiasetukset Blenderistä.
- Unity-tuontiasetukset joihin kannattaa kiinnittää huomiota:
  `materialImportMode` (External vs. InPrefab), `useSRGBMaterialColor`, ja
  tunnettu `useFileScale`-ansa (uusi FBX voi tuoda `1`:llä vaikka pitäisi olla
  `0` → malli ~100x liian pieni — konkreettinen, projektissa oikeasti osuttu bugi).
- **Repo tähän osaan**: 2-3 esimerkki-FBX (yksinkertainen laatikkoauto + yksi
  koristemalli) + palettitekstuuripohja.

## Osa 3 — Ajettava auto

**Tavoite:** auto liikkuu maalla, kamera seuraa pehmeästi — ensimmäinen "hauska"
hetki.

- Syöte (näppäimistö + kosketus), arcade-tyylinen autofysiikka (kiihdytys,
  kääntö, pito, huippunopeus — ei realistista fysiikkaa, tarkoituksella).
- Kameran seuranta.
- `RigidbodyConstraints`-bittimaski autolle jotta se ei koskaan käänny kyljelleen
  (lapsipeli — turvallisuus/ennustettavuus ennen realismia).
- **Repo tähän osaan**: `CarInput.cs`, `ArcadeCarController.cs`, `CameraFollow.cs`,
  auto-prefab.

## Osa 4 — Ensimmäinen pelattava rata

**Tavoite:** lähtö → maali, tähtien keräys, HUD, pause, restart — yksi kokonainen
pelattava keikka.

- RaceManager (kulku, aika, tähtilaskenta), FinishTrigger, Pickup.
- HUD ja Canvas-perusteet, kosketusohjauspainikkeet.
- Lyhyt maininta proseduraalisesta ratageneraattorista Blenderissä (syvemmin ei
  vielä tässä osassa — vain "tämän rata tehtiin skriptillä, ei käsin").
- **Repo tähän osaan**: `RaceManager.cs`, `FinishTrigger.cs`, `Pickup.cs`,
  `RaceHUD.cs`, yksi minimirata-scene.

## Osa 5 — Rakenna omat Unity-Editor-työkalut

**Vahvin yksittäinen aihe koko sarjassa** (ks. `Idea_TutorialArtikkelit`-muistio) —
harva aloittelija osaa tämän, eikä sitä opeteta tyypillisissä tutoriaaleissa.

**Tavoite:** oma `Tarkistus/Korjaus/Lisäys`-mallinen Editor-työkalusetti joka
automatisoi scenen kytkennät ja datan tarkistuksen sen sijaan että kaikki
tehdään käsin Inspectorista joka kerta.

- `[MenuItem]`-attribuutin perusteet, idempotentin (turvallisesti uudelleen
  ajettavan) työkalun suunnitteluperiaatteet.
- Esimerkki: yksi "Tarkistus"-työkalu (raportoi puutteet) + yksi "Korjaus"-työkalu
  (korjaa ne) samasta scenestä.
- Miksi tämä malli skaalautuu paremmin kuin käsin klikkailu kun ratoja/sceneja
  on monta.
- **Repo tähän osaan**: koko `Editor/`-kansion rakenne + 2 yksinkertaistettua
  esimerkkityökalua (täysi 20+ työkalun setti kuuluu maksulliseen pakettiin).

## Osa 6 — Auton rakentelu ja paketit

**Tavoite:** oppimisen ydinmekaniikka — osat, ominaisuusmittarit, paketin ja
auton yhteensovitus, halli-UI.

- Datamallit (`PartData`, `PackageData`), laskenta (`CarStats`, `CarBuilder`),
  sopivuustarkistus (`FitChecker`).
- Halli-UI: osakategoriat, mittaripalkit, sopivuusmerkki.
- Pisteillä avattavat osat/paketit — yksinkertainen etenemisjärjestelmä ilman
  erillistä "UnlockManager"-luokkaa (näytetään että se ei aina ole tarpeen).
- **Repo tähän osaan**: `PartData.cs`, `PackageData.cs`, `CarStats.cs`,
  `CarBuilder.cs`, `FitChecker.cs`, `GarageScreen.cs` (tiivistetty versio).

## Osa 7 — Toinen pelisilmukka: pesu ja korjaus

**Tavoite:** näyttää miten rakentaa TOINEN, erilainen minipeli saman projektin
sisään (kuplien poksautus + kolhujen korjaus) ilman että se tuntuu irralliselta.

- Pysyvä, kattoinen lika-/kolhumittari joka kasvaa radalla ja purkautuu Pesussa
  asteittain — miksi "nollaudu joka ajolla" -malli hylättiin.
- Kosketuksella poksautettava kupla-mekaniikka, sama periaate laajennettuna
  korjaustyökaluksi.
- **Repo tähän osaan**: `Bubble.cs`, `RepairTool.cs`, `WashScreen.cs`,
  `GameManager`:n mittarilogiikka (tiivistetty).

## Osa 8 — Tallennus, asetukset, äänet ja eteneminen

**Tavoite:** pelistä "demosta" oikeaksi peliksi — mikään ei häviä kun sovellus
suljetaan.

- `SaveManager` (JSON-tallennus `persistentDataPath`:iin), miksi ladataan
  `Start()`:ssa eikä `Awake()`:ssa (Script Execution Order -selitys).
- `SettingsManager` + `AudioManager`: äänet, ohjaustapa, nopeustaso.
- Tarrat/avaukset kevyenä palkitsemisjärjestelmänä.
- **Repo tähän osaan**: `SaveManager.cs`, `SaveData.cs`, `SettingsManager.cs`,
  `AudioManager.cs` (tiivistetty, ilman projektin omia äänitiedostoja).

## Osa 9 — Debug-saagat ja rehellinen katsaus AI-avusteiseen kehitykseen

**Toiseksi vahvin aihe sarjassa** — yhdistää kaksi ajankohtaista, harvoin yhdessä
käsiteltyä teemaa: oikeat Unity-debuggaustarinat JA rehellinen kuvaus mihin
tekoälyavusteista työtä käytettiin.

- **Tapaus A**: "Miksi lätäköt eivät näkyneet radalla" — monivaiheinen etsintä
  (normaalit → sijainti/rotaatio, väärä johtopäätös → lopulta maaston
  pintakorkeuden vaihtelu). Opettaa systemaattista poissulkemista.
- **Tapaus B**: CanvasScaler-referenssiresoluution matematiikka ja miksi sen
  "yhtenäistäminen" saattaa rikkoa olemassa olevan asettelun sen sijaan että
  korjaisi mitään — projektissa jopa kolmeen kertaan yritetty samaa korjausta.
- **Rehellinen AI-osio**: mihin Claudea käytettiin (scenetiedostojen suora YAML-
  editointi, toistuvien Editor-työkalujen kirjoitus, debug-skriptien ajaminen),
  mihin ei (pelisuunnittelupäätökset, lopulliset hyväksynnät, käyttäjätestaus) —
  ja tapaus jossa AI:n oma diagnostiikka johti harhaan kahdesti (bends-
  etumerkkisaaga) ennen kuin vain oikea pelitesti paljasti totuuden. Tämä on
  osan tärkein opetus: AI on työkalu jota pitää epäillä, ei oraakkeli.
- **Repo tähän osaan**: ei koodia — sen sijaan lyhyt "debug-päiväkirja" -ote
  projektin oikeasta kehityslokista esimerkkinä.

## Osa 10 — Julkaisuvalmiiksi: kiillotus ja Google Play

**Tavoite:** mitä jää vielä tehtäväksi ennen kuin peli on oikeasti julkaisuvalmis
— rehellinen, ei-glamouroitu loppukatsaus.

- Visuaalinen ja äänellinen "mehukkuus" (partikkelit, squash/stretch, pieni
  sävelkorkeusvaihtelu toistuville äänille).
- Google Play -lastenpelivaatimukset: tietosuojaseloste, IARC-ikäluokitus,
  Parental Gate, allekirjoitusavain/AAB-build.
- Loppuyhteenveto koko sarjasta + linkki maksulliseen asset-pakettiin
  (täydet FBX-mallit, palettitekstuurit, koko Editor-työkalusetti, PDF-opas).
- **Repo tähän osaan**: julkaisu-tarkistuslista (checklist) markdown-muodossa.

---

## Repo-jako yhteenvetona (ilmainen vs. maksullinen)

Sama malli kuin `Idea_TutorialArtikkelit`-muistiossa, tarkennettuna per osa:

- **Ilmainen repo** (GitHub): jokaisen osan koodiesimerkit tiivistettynä/
  yksinkertaistettuna (ei koko projektin kaikkia 40+ scriptiä sellaisenaan),
  2-3 esimerkki-FBX osaan 2, minimirata-scene osaan 4.
- **Maksullinen paketti** (itch.io/Gumroad): kaikki oikeat FBX-mallit
  (n. 70 kpl), valmiit palettitekstuurit, koko Editor-työkalusetti
  (Tarkistus/Korjaus/Lisäys/Työkalut, 20+ työkalua), Blenderin
  ratageneraattoriskripti, PDF-koontiopas.

## Käytännön muistilista

- Aloita osasta 1 tai 5 — osa 5 (Editor-työkalut) on erillinen, itsenäinen koukku
  jos haluaa testata kiinnostusta ilman koko sarjan sitoutumista.
- Kerää kuvakaappaukset/GIFit jokaisesta osasta MATKALLA, ei jälkikäteen —
  erityisesti osan 9 debug-saagoista (ennen/jälkeen-kuvat ratkaisevat).
- Ei pakko julkaista kaikkia 10 kerralla tai edes järjestyksessä — osa 5 ja 9
  toimivat myös itsenäisinä, sarjasta irrallisina artikkeleina jos into hiipuu
  kesken.
