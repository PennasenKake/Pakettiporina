# Pakettipörinä: projektin tiivistelmä (24.11.2026)

**Mikä:** lapsiystävällinen 3D-mobiilipeli (Android, 6-10-vuotiaat), Unity 2022.3.23f1 LTS, URP, C#. Lapsi valitsee paketin, valitsee radan, rakentaa auton osista ja värittää sen, ajaa paketin perille, pesee/korjaa auton.
Repo: github.com/PennasenKake/Pakettiporina (julkinen, portfolioprojekti). Tekijä: Eero Korhonen. Peli on offline, mainokseton, ei datankeruuta.

## Pelisilmukka
Halli (3 vaihetta: Paketti -> Rata -> Auto+väri) -> Ajo (kallistus tai napit, tähtien keräys, esteet) -> Maali (palkkio, 1-3 tähden arvosana) -> Pesu/korjaus -> Halli. Edistys tallentuu (JSON).

## Sisältö
- 5 rataa: Kaupunki (Game), Satama, Puisto, Huvipuisto, Maatila (+ Kyla, ei Build Settingsissä). 8 sceneä käytössä.
- 10 pakettia, 6 osakategoriaa (Kori, Renkaat, Moottori, Jouset, Lisät, Väri), 32 osaa (5 salaista kultaosaa + sateenkaarimaali), 36 tarraa.
- 73 FBX-mallia, 8 musiikkia/tehosteääniä koodista generoituna + WAV-tiedostot, runtime-UI koodilla.

## Tekniikka (tärkeimmät)
- Arcade-fysiikka (Rigidbody, ei WheelCollider). GameManager-singleton + GameEvents.
- Data: ScriptableObjectit (PartData, PackageData, StickerData). Tallennus: SaveManager/SaveData (JSON, versioitu oletusarvoilla).
- UI rakennetaan ajonaikaisesti koodilla (GarageWizard, RunStarsView), Juice-kirjasto (konfetti, pulssi, äänet).
- Edistys: Progress.cs (ajotähdet avaavat värejä, osia, paketteja, ratoja; taulukko koodissa).
- Suorituskyky: PerfProbe, PerfBenchmark, PerformanceManager (30/60 fps, akkusäästö, sleepTimeout). Raportit Docs/Suorituskyky_*.
- 43 editorityökalua valikossa Pakettiporina (tarkistus, korjaus, lisäys, suorituskyky, esittely). Ne ovat projektikohtaisia.

## Koko
17 651 riviä C# (Editor mukana). Isoimmat: Juice 710, GarageWizard 646, RaceHUD 623, WashScreen 600, GarageScreen 574. Ks. 04_koodikartta.md.

## Tila
- Ydinpeli valmis ja Unityssa testattu; laitetestaus (oikea puhelin) tekemättä.
- Avoimet bugit: auto voi ajaa mereen / ei pääse maaliin; lukitut paketit/osat ja "!"-vihjeet testaamatta; Kyla/Tarrat/Yötila testaamatta.
- Git: paikallisesti 44 commitia, GitHubissa 36; 492 tiedostoa committaamatta (mm. Juice.cs, CarColor.cs, GarageWizard).
