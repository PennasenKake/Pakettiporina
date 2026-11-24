# Suorituskyky: automaattimittaus 23.11.2026 22.23

Mitattu Unity Editorissa (Play-tila). **Editorin ruudunajat eivät ole puhelimen ruudunaikoja**; piirtokutsut, kolmiot, GC-allokaatiot ja muistin kasvu ovat kuitenkin vertailukelpoisia. Laitetesti tarvitaan vielä (PerfProbe-overlay: neljän sormen kosketus 1 s Development Buildissa).

Ympäristö: AMD Ryzen 7 5700U with Radeon Graphics , AMD Radeon(TM) Graphics, RAM 15693 MB, kuva 1080x2280, laatutaso mittauksessa Balanced (Androidin oletus; Performant-vertailu lopussa), FPS-raja pois mittauksen ajaksi

## Ruudunaika ja piirto per scene

| Scene | fps | ka ms | p95 ms | p99 ms | pahin ms | >33 ms | GC B/ruutu | GC max B | piirtokutsut (ka/max) | SetPass (ka/max) | kolmiot (ka/max) |
|---|---|---|---|---|---|---|---|---|---|---|---|
| MainMenu | 158 | 6,3 | 9,3 | 13,4 | 16,5 | 0 | 2926 | 11819 | 16/18 | 9/10 | 1756/1831 |
| Garage | 171 | 5,8 | 7,2 | 10,8 | 19,3 | 0 | 2939 | 26816 | 19/21 | 11/13 | 2104/2167 |
| Satama | 133 | 7,5 | 9,7 | 13,3 | 95,1 | 3 | 3114 | 81571 | 164/249 | 21/24 | 32363/43347 |
| Puisto | 116 | 8,6 | 10,9 | 15,2 | 308,9 | 2 | 3042 | 32516 | 126/201 | 22/26 | 49838/68744 |
| Huvipuisto | 114 | 8,8 | 11,1 | 14,5 | 17,7 | 0 | 3034 | 32516 | 161/233 | 24/27 | 78390/101595 |
| Maatila | 114 | 8,8 | 11,4 | 15,7 | 36,8 | 1 | 3122 | 79304 | 257/467 | 21/25 | 52325/65847 |
| Wash | 168 | 5,9 | 7,2 | 8,6 | 15,5 | 0 | 2899 | 3136 | 28/30 | 9/10 | 1803/1803 |
| Maatila (Performant) | 184 | 5,4 | 6,7 | 8,4 | 12,5 | 0 | 2547 | 32005 | 169/235 | 15/17 | 24791/28523 |
| Huvipuisto (Performant) | 184 | 5,4 | 6,8 | 8,0 | 12,4 | 0 | 2515 | 32044 | 96/118 | 18/20 | 37853/45344 |

## Muisti per scene (kaappauksen lopussa)

| Scene | yhteensä MB | gfx MB | tekstuurit MB | meshit MB | ääni MB | GC-keko MB | Mono MB | tekstuureja | mesh-objekteja |
|---|---|---|---|---|---|---|---|---|---|
| MainMenu | 1228,1 | 292,8 | 309,4 | 0,5 | 3,0 | 660,6 | 660,6 | 1001 | 36 |
| Garage | 1233,5 | 292,8 | 309,1 | 0,6 | 4,0 | 662,7 | 662,7 | 984 | 68 |
| Satama | 1237,5 | 295,2 | 310,2 | 2,4 | 4,6 | 663,2 | 663,2 | 995 | 43 |
| Puisto | 1240,1 | 296,8 | 310,7 | 5,0 | 4,6 | 662,8 | 662,8 | 1002 | 49 |
| Huvipuisto | 1244,5 | 299,1 | 312,0 | 8,3 | 4,6 | 662,7 | 662,7 | 1004 | 50 |
| Maatila | 1238,5 | 295,8 | 309,4 | 4,4 | 4,7 | 662,8 | 662,8 | 985 | 44 |
| Wash | 1233,2 | 294,5 | 310,7 | 0,4 | 5,5 | 661,7 | 661,7 | 982 | 19 |
| Maatila (Performant) | 1077,9 | 134,2 | 147,9 | 4,4 | 5,5 | 662,6 | 662,6 | 975 | 44 |
| Huvipuisto (Performant) | 1084,1 | 137,5 | 150,5 | 8,3 | 5,5 | 662,5 | 662,5 | 994 | 50 |

## Latausajat (LoadSceneAsync, sekuntia)

- MainMenu: 0,79 s
- Garage: 0,97 s
- Satama: 0,90 s
- Puisto: 0,73 s
- Huvipuisto: 0,79 s
- Maatila: 0,72 s
- Wash: 0,67 s
- Maatila (Performant): 0,76 s
- Huvipuisto (Performant): 0,72 s

## Muistivuototesti

Kierros = MainMenu → Satama → Garage → Huvipuisto → MainMenu, jonka jälkeen UnloadUnusedAssets + GC. Jos luvut kasvavat tasaisesti kierroksittain, muistia vuotaa.

| Kierros | yhteensä MB | tekstuurit MB | Mono MB | tekstuureja |
|---|---|---|---|---|
| 1 | 1233,7 | 314,3 | 659,6 | 1035 |
| 2 | 1233,8 | 314,3 | 659,6 | 1035 |
| 3 | 1233,8 | 314,3 | 659,6 | 1035 |
| 4 | 1233,9 | 314,3 | 659,6 | 1035 |
| 5 | 1233,9 | 314,3 | 659,6 | 1035 |
| 6 | 1234,0 | 314,3 | 659,6 | 1035 |

**Kasvu kierroksesta 2 kierrokseen 6: +0,2 MB.**

## Huomiot
- Scene 'Kyla' ei ole Build Settingsissa: ohitettu.
