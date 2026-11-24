# Suorituskyky: automaattimittaus 23.11.2026 22.16

Mitattu Unity Editorissa (Play-tila). **Editorin ruudunajat eivät ole puhelimen ruudunaikoja**; piirtokutsut, kolmiot, GC-allokaatiot ja muistin kasvu ovat kuitenkin vertailukelpoisia. Laitetesti tarvitaan vielä (PerfProbe-overlay: neljän sormen kosketus 1 s Development Buildissa).

Ympäristö: AMD Ryzen 7 5700U with Radeon Graphics , AMD Radeon(TM) Graphics, RAM 15693 MB, kuva 1080x2280, laatutaso High Fidelity, targetFrameRate -1, vSync 1

## Ruudunaika ja piirto per scene

| Scene | fps | ka ms | p95 ms | p99 ms | pahin ms | >33 ms | GC B/ruutu | GC max B | piirtokutsut (ka/max) | SetPass (ka/max) | kolmiot (ka/max) |
|---|---|---|---|---|---|---|---|---|---|---|---|
| MainMenu | 121 | 8,3 | 12,4 | 17,3 | 28,8 | 0 | 2942 | 11819 | 16/18 | 9/10 | 1755/1831 |
| Garage | 124 | 8,0 | 11,2 | 15,5 | 48,9 | 1 | 2975 | 26816 | 20/21 | 12/13 | 2105/2167 |
| Satama | 60 | 16,6 | 20,7 | 31,7 | 120,0 | 4 | 3439 | 81659 | 232/340 | 24/27 | 48506/58899 |
| Puisto | 55 | 18,1 | 23,1 | 36,1 | 48,0 | 15 | 3360 | 76220 | 175/279 | 25/29 | 87706/113446 |
| Huvipuisto | 53 | 18,7 | 22,9 | 37,0 | 51,3 | 15 | 3287 | 32516 | 226/300 | 27/30 | 130885/157829 |
| Maatila | 55 | 18,2 | 22,7 | 37,5 | 41,6 | 19 | 3383 | 32564 | 393/632 | 24/28 | 90521/98683 |
| Wash | 120 | 8,4 | 11,3 | 13,2 | 20,4 | 0 | 2907 | 3134 | 25/27 | 9/10 | 1795/1795 |

## Muisti per scene (kaappauksen lopussa)

| Scene | yhteensä MB | gfx MB | tekstuurit MB | meshit MB | ääni MB | GC-keko MB | Mono MB | tekstuureja | mesh-objekteja |
|---|---|---|---|---|---|---|---|---|---|
| MainMenu | 1258,4 | 323,3 | 339,9 | 0,5 | 2,4 | 665,4 | 665,4 | 995 | 36 |
| Garage | 1263,2 | 323,3 | 339,6 | 0,6 | 3,4 | 666,9 | 667,0 | 979 | 68 |
| Satama | 1294,8 | 355,7 | 370,7 | 2,4 | 4,4 | 665,7 | 665,7 | 990 | 43 |
| Puisto | 1297,7 | 357,3 | 371,2 | 5,0 | 4,5 | 665,5 | 665,5 | 997 | 49 |
| Huvipuisto | 1302,2 | 359,6 | 372,5 | 8,3 | 4,5 | 665,4 | 665,4 | 999 | 50 |
| Maatila | 1296,1 | 356,3 | 369,9 | 4,4 | 4,5 | 665,4 | 665,4 | 980 | 44 |
| Wash | 1292,6 | 355,0 | 371,2 | 0,4 | 5,2 | 665,2 | 665,2 | 977 | 19 |

## Latausajat (LoadSceneAsync, sekuntia)

- MainMenu: 0,72 s
- Garage: 1,05 s
- Satama: 1,16 s
- Puisto: 0,89 s
- Huvipuisto: 0,84 s
- Maatila: 0,82 s
- Wash: 0,72 s

## Muistivuototesti

Kierros = MainMenu → Satama → Garage → Huvipuisto → MainMenu, jonka jälkeen UnloadUnusedAssets + GC. Jos luvut kasvavat tasaisesti kierroksittain, muistia vuotaa.

| Kierros | yhteensä MB | tekstuurit MB | Mono MB | tekstuureja |
|---|---|---|---|---|
| 1 | 1295,0 | 374,8 | 664,7 | 1028 |
| 2 | 1295,1 | 374,8 | 664,7 | 1028 |
| 3 | 1295,2 | 374,8 | 664,7 | 1028 |
| 4 | 1295,2 | 374,8 | 664,7 | 1028 |
| 5 | 1295,3 | 374,8 | 664,7 | 1028 |
| 6 | 1295,4 | 374,8 | 664,7 | 1028 |

**Kasvu kierroksesta 2 kierrokseen 6: +0,3 MB.**

## Huomiot
- Scene 'Kyla' ei ole Build Settingsissa: ohitettu.
