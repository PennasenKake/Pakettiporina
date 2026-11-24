# Puisto — ratadata

Sama tiedosto kuin lähettämäsi speksi, mutta tarkistettu suoraan siitä
`bends`-datasta jota `PakettiporinaElements.cs` ja Blender-generaattori
oikeasti käyttävät. Muutama luku poikkesi hieman (merkitty alle) — luultavasti
pyöristettiin/arvioitiin aiemmin. Tämä versio täsmää täsmälleen peliin.

## Maaston kokonaismitat

| Ulottuvuus | Arvo | Rajat |
|---|---|---|
| Leveys (X) | 71 | −35.5 … +35.5 |
| Pituus (Z) | 340 *(lähetetyssä: 301)* | −170 … +170 *(lähetetyssä: −150…+150)* |
| Korkeus (Y) | ~4.4 | ajopinta tasolla 0 |

Origo on maaston keskellä ja ajopinnan tasolla — maasto asetetaan Unityssä kohtaan (0, 0, 0).

## Tie

- Leveys: 12 (ajokaista), lisäksi molemmin puolin ~1.8 leveä piennar
- Kulkee Z-suunnassa, lähdöstä maaliin
- Tien alku: Z = −150, loppu: Z = +150 (StartPoint/Finish — sama kuin lähetetyssä)
- Muoto: suora päistä, loiva S-mutka keskellä — oikea, mutkien huiput ovat
  kuitenkin Z = ±75, ei ±50 (ks. taulukko).

## Tien keskilinja (X kullakin Z:lla)

| Z | X (keskilinja) | Osuus |
|---|---|---|
| −170 → −120 | 0 | suora (lähtö) |
| −100 | +7 | kaartaa oikealle |
| **−75** | **+18** *(lähetetyssä: +20 kohdassa −50)* | mutkan huippu oikealla |
| −50 | +7 | palaa kohti keskustaa |
| −30 → +30 | 0 | suora keskiosuus (tässä on lampi, ei tie) |
| +50 | −7 | kaartaa vasemmalle |
| **+75** | **−18** *(lähetetyssä: −20 kohdassa +50)* | mutkan huippu vasemmalla |
| +100 | −7 | palaa kohti keskustaa |
| +120 → +170 | 0 | suora (maali) |

## Kiintopisteet radalla

| Kohde | Sijainti (X, Y, Z) | Huom |
|---|---|---|
| StartPoint (lähtö) | (0, 1, −150) | suoralla |
| Finish (maali) | (0, 0, +150) | suoralla |
| Lampi (Puisto_Lampi + kaarisilta) | (−30, 0, +70) *(lähetetyssä: −24, +62)*, halkaisija ~18 | keskiosuuden suoralla, ei tiellä |
| Puisto_Puu × 4 | (14,−110) (−15,−20) (14,45) (6,95) | ripoteltu radan varrelle |
| Suihkulähde | (20, 0, 0) | keskiosuudella |
| Liukumäki | (−16, 0, −115) | lähtöalueella |
| Keinu / Penkki / Lyhtypylväs | (15,−40) / (15,−34) / (16,−100) | lähtöosuudella |
| Boost × 3 | z=−150, 0, +150 | radan päissä ja keskellä |

## Reunatolpat

Värikkäät tolpat rajaavat kentän joka reunalla (X = ±35.5 ja Z = ±170) noin 12 yksikön välein.

## Käytännön hyöty

Sama periaate kuin muillakin radoilla: tähdet/boostit/lätäköt keskilinjalle
(Y ≈ 1), koristeet vähintään ~8 yksikköä keskilinjasta poispäin ja pois
lammen kohdalta (X ≈ −30, Z ≈ 55…85 -alue on varattu lammelle/sillalle).
