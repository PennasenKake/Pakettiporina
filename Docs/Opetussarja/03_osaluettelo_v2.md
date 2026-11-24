# How to code your own game: artikkelisarjan osaluettelo (versio 2, 24.11.2026)

Pohjana Pakettipörinä (Unity 2022.3 LTS, URP, Android). Sarja rakentaa pelin alusta Google Playhin pienissä osissa.

Versio 2 perustuu vertailuun Unity Learnin Create with Codeen, Catlike Codingiin, Kodecoon, Udemy-kursseihin ja GameDev.tv:hen (ks. Docs/12_ARTIKKELISARJA_vertailu.md). Muutokset versioon 1 on listattu lopussa.

## Periaatteet

1. **Nopea voitto:** lukija pelaa jo ensimmäisen osan lopussa (alle 15 min).
2. **Yksi osa = yksi käsite = 20-40 min = alle 100 riviä uutta koodia.**
3. **Jokaisessa osassa:** tavoite ensimmäisessä lauseessa, alku- ja loppuprojekti, kuva tai GIF lopputuloksesta, yksi "kokeile itse" -tehtävä ja ratkaisu.
4. **Opetuskoodi toimii ilman maksullista pakettia** (harmaat laatikot, koodista generoidut spritet ja äänet).
5. **Lopussa lukija tekee oman variaation**, ei vain kopioi.
6. **Lähtötaso kerrotaan:** tiedät mitä muuttuja ja funktio ovat; Unityä ei tarvitse osata.
7. **Jokaisen osan alussa:** Unity-versio, URP, arvioitu aika, edellytykset.

## Aikataulu lukijalle

| Jakso | Osat | Aika | Lopputulos |
|---|---|---|---|
| Perusta | 1-6 | n. 3-4 h (viikonloppu) | pelattava minipeli |
| Peli | 7-15 | n. 6-8 h | kokonainen peli, jossa edistys |
| Julkaisu | 16-20 | n. 3-4 h | peli Google Playssa |
| Oma peli | 21 | n. 2-4 h | oma variaatio |

## Ilmainen ja maksullinen

| | Ilmainen | Maksullinen paketti |
|---|---|---|
| Artikkelit ja koodi | kaikki | - |
| Mallit | harmaat laatikot + **yksi ilmainen näyte-FBX** | FBX: auto, osat, paketit, radat |
| Editorityökalut | opetettavat pienet työkalut + **yksi ilmainen näytetyökalu** | 43 työkalun kokoelma, tarkistukset, suorituskykymittari |
| UI ja äänet | koodista generoidut | valmiit spritet, ikonit, äänet |
| Scenet | aloitusscene | valmiit scenet ja loppuprojektit |

Näytteet antavat ostajalle nähdä laadun ennen ostoa.

---

## Jakso 1: Perusta (ilmainen)

| # | Otsikko | Tavoite (lukija osaa lopuksi) | Sisältö | Tehtävä | Pakettipörinän vastine |
|---|---|---|---|---|---|
| 1 | **Auto ajaa 15 minuutissa** | Ajaa laatikkoautoa oikealla ohjaimella | Valmis aloitusprojekti, Rigidbody, arcade-fysiikka (miksi ei WheelCollider), nappi- ja kallistusohjaus | Muuta nopeutta ja kääntymistä | ArcadeCarController, CarInput |
| 2 | Oma projekti ja Android | Pystyttää oma projekti ja ajaa puhelimessa | Unity Hub, URP, Android-asetukset, kansiorakenne, Git (ei LFS-binäärejä), Unity Remote | Aja peli omalla puhelimella | projektin rakenne |
| 3 | Rata ja maali | Rakentaa rata ja maaliportti | Laatikot tai ProBuilder, trigger, FinishTrigger | Tee oma mutka | FinishTrigger |
| 4 | Kamera | Seurantakamera ilman nykimistä | LateUpdate, pehmennys, kamera ja loveen (cutout) | Vaihda kamerakulmaa | kamera, CutoutAvoider |
| 5 | Lähtölaskenta ja ajastin | Kolmen sekunnin lähtölaskenta | Coroutine, tapahtumat (GameEvents) | Lisää "Go!"-teksti | RaceManager, GameEvents |
| 6 | Ensimmäinen valikko | Päävalikko ja scenenvaihto | Canvas, TextMeshPro, SceneManager, häivytys | Lisää toinen nappi | MainMenu, SceneFader |

Lopputulos: pelattava minipeli (aja maaliin ajassa). **Julkaise osat 1-3 ilmaisena ensin ja mittaa kiinnostus** (ks. Validointi).

## Jakso 2: Peli

| # | Otsikko | Tavoite | Sisältö | Tehtävä | Pakettipörinän vastine |
|---|---|---|---|---|---|
| 7 | Esteet ja kolhut | Osumat vaikuttavat peliin | Cone/Laatikko/Puddle, mittarit, pysyvä lika ja kolhut | Tee uusi este | GameManager, Cone, Puddle |
| 8 | Tähdet ja pisteet | Kerättävät esineet ja palkkio | Pickup, pisteet, HUD | Lisää tähti radalle | Pickup, RaceHUD |
| 9 | Data ScriptableObjecteilla | Osat ja paketit datana | PartData, PackageData, FitChecker | Tee uusi paketti | PartData, PackageData |
| 10 | Halli 1: osien valinta | Rakentaa auto osista | CarBuilder, mittarit, esikatselu | Tee uusi osa | GarageScreen, CarBuilder |
| 11 | Halli 2: kolme vaihetta | Jakaa UI vaiheisiin koodilla | Runtime-UI, ankkurit, safe area | Lisää neljäs vaihe | GarageWizard |
| 12 | Tallennus | Tallentaa edistys JSONiin | SaveManager, SaveData, versiointi, vanhojen tallennusten tuki | Tallenna lempiväri | SaveManager |
| 13 | Edistys ja lukitukset | Tähdet 1-3, porrastetut avaukset | Arvosanalogiikka, avaustaulukko, vapaa radan valinta, "seuraava palkinto" | Säädä avausrajat | Progress, RunStarsView |
| 14 | Mehukkuus | Tehdä peli eläväksi | Konfetti, välähdys, pulssi, generoidut äänet | Tee oma tehoste | Juice, SoundSynth |
| 15 | Pesu ja korjaus | Pienpeli palkintona | Kosketuskupla-minipeli, mittarien tyhjennys | Muuta vaikeutta | WashScreen |

## Jakso 3: Julkaisu

| # | Otsikko | Tavoite | Sisältö | Tehtävä | Pakettipörinän vastine |
|---|---|---|---|---|---|
| 16 | Suorituskyky ja akku | Mitata ja korjata | Profiler-recorderit, FPS-raja, näytön uni, akkusäästö, ARM64, Performant-laatu | Mittaa oma pelisi | PerfProbe, PerformanceManager |
| 17 | Lapsiystävällinen käyttöliittymä | Isot kohteet ja selkeys | Kosketuskoot (>= 48 dp), kontrastit, vähän tekstiä, kuvat tekstin sijaan | Tarkista oma näkymä | Hallin mockup-prosessi |
| 18 | Editorityökalut | Automatisoida toistuva | MenuItem, tarkistus- ja korjaustyökalut, raportit Markdowniin | Tee oma tarkistustyökalu | 43 työkalua |
| 19 | Testaus | Löytää bugit järjestelmällisesti | Testilista, Play-testi, laitetesti, muistivuototesti | Kirjoita oma testilista | Docs/04_TESTAUS |
| 20 | Google Play | Julkaista | Build, allekirjoitus, kuvakaappaukset, lapsille suunnatun sovelluksen vaatimukset | Täytä Play-listaus | julkaisulista |

## Jakso 4: Oma peli

| # | Otsikko | Tavoite | Sisältö |
|---|---|---|---|
| 21 | **Tee oma variaatiosi** | Muokata peli omaksi | Valitse yksi: uusi rata, uusi paketti, uusi osa tai uusi pelimuoto. Tarkistuslista (toimiiko, tallentuuko, mahtuuko ruutuun). Jaa tuloksesi yhteisölle. |

---

## Maksullinen paketti: ehdotus

1. **Perus (n. 19 €):** FBX-mallit (auto, 10 pakettia, osat), UI-spritet ja ikonit.
2. **Työkalut (n. 29 €):** 43 editorityökalua, tarkistukset, suorituskykymittari, valmiit scenet.
3. **Kaikki (n. 39 €):** molemmat + loppuprojektit jokaiselle osalle.

(Hinnat ovat ehdotuksia: tarkista markkinan taso ennen julkaisua.)

## Validointi ennen koko sarjan kirjoittamista

Hakutulokset eivät paljastaneet suoraa vastinetta "ilmainen sarja + maksullinen FBX- ja työkalupaketti mobiilipelille" -mallille. Se voi olla markkinarako tai merkki puuttuvasta kysynnästä.

1. Kirjoita ja julkaise osat 1-3 ilmaisena.
2. Mittaa: lukijamäärä, kuinka moni lukee osaan 3 asti, kommentit ja sähköpostilistan liittymät.
3. Kysy yleisöltä, ostaisivatko he FBX-paketin, työkalut vai molemmat (yksi kysymys, kolme vaihtoehtoa).
4. Päätä jatko tuloksen mukaan: jatka koko sarja / keskity vain työkaluihin / lopeta.

## Ennen aloitusta

- Varmista mallien, äänten ja ikonien oikeudet myyntiin.
- Päätä kieli (suomi, englanti vai molemmat) ja alusta (oma sivusto, itch.io, Gumroad, Medium).
- Tee jokaisesta osasta lyhyt GIF tai video lopputuloksesta.
- Päivitä sarja kerran vuodessa Unity-version mukana.

## Muutokset versioon 1

| Muutos | Syy (vertailusta) |
|---|---|
| Osa 1 on nyt "Auto ajaa 15 minuutissa", asetukset siirrettiin osaan 2 | Create with Code aloittaa ajamalla heti; nopea voitto pitää lukijan |
| Jokaiseen osaan "Tehtävä" ja ratkaisu | Unity Learnissa tehtäviä ja kokeita joka yksikössä |
| Uusi osa 21 ja jakso "Oma peli" | Create with Coden vahvuus on oma projekti lopussa |
| Aikataulu lukijalle ja lähtötaso | Aloittelija- ja edistyneet sarjat erottuvat selvästi; lukija haluaa tietää sitoumuksen |
| Ilmainen näyte-FBX ja -työkalu | Ostaja näkee laadun; GameDev.tv ja Unity myyvät assetit erikseen, joten näyte tekee ostopäätöksestä helpon |
| Validointivaihe (osat 1-3 ensin) | Suoraa vastinetta ei löytynyt: testataan kysyntä ennen isoa työtä |
| GIF tai video jokaisesta osasta | Useimmat kilpailijat ovat videopohjaisia |
