# Huvipuisto — ratadata

Sama muoto kuin Puisto-speksissä, laskettu suoraan `PakettiporinaElements.cs`/
`PakettiporinaStars.cs`/Blender-generaattorin oikeasta `bends`-datasta.

## Maaston kokonaismitat

| Ulottuvuus | Arvo | Rajat |
|---|---|---|
| Leveys (X) | 71 | −35.5 … +35.5 |
| Pituus (Z) | 380 | −190 … +190 |
| Korkeus (Y) | ~4.4 | ajopinta tasolla 0 |

Origo on maaston keskellä ja ajopinnan tasolla — maasto asetetaan Unityssä kohtaan (0, 0, 0).
Huvipuisto on pisin neljästä radasta (Rata 1: 271, Puisto/Satama: 340, Huvipuisto: 380).

## Tie

- Leveys: 12 (ajokaista), lisäksi molemmin puolin ~1.8 leveä piennar
- Kulkee Z-suunnassa, lähdöstä maaliin
- Tien alku: Z = −170, loppu: Z = +170
- Muoto: **kolme erillistä mutkaa** peräkkäin, suorien osuuksien erottamina — haastavin neljästä radasta, vaatii kaikki opitut ajotaidot.

## Tien keskilinja (X kullakin Z:lla)

| Z | X (keskilinja) | Osuus |
|---|---|---|
| −190 → −150 | 0 | suora (lähtö) |
| −115 | +16 | mutka 1:n huippu (oikealle) |
| −80 → −60 | 0 | suora |
| −40 → 0 | 0 → −18 | mutka 2 alkaa, kaartaa vasemmalle |
| 0 | −18 | mutka 2:n huippu (vasemmalle) |
| 40 | 0 | mutka 2 päättyy |
| 60 → 80 | 0 | suora |
| 115 | +16 | mutka 3:n huippu (oikealle, sama suunta kuin mutka 1) |
| 150 → 190 | 0 | suora (maali) |

Ylhäältä katsottuna: suora → oikea mutka → suora → vasen mutka → suora →
oikea mutka → suora. Kolme S:ää peräkkäin, kahden suoran välissä hengähdystauko.

## Kiintopisteet radalla

| Kohde | Sijainti (X, Y, Z) | Huom |
|---|---|---|
| StartPoint (lähtö) | (0, 1, −170) | suoralla |
| Finish (maali) | (0, 0, +170) | suoralla |
| Maailmanpyörä (Huvipuisto_Maailmanpyora) | (28, 0, 0) | mutka 2:n ulkopuolella, iso maamerkki |
| Teltta (Huvipuisto_Teltta) | (−25, 0, −60) | suoralla osuudella mutkien 1/2 välissä |
| Karuselli (Huvipuisto_Karuselli) | (−15, 0, −60) | sama alue kuin Teltta |
| Myyntikoju (Huvipuisto_Myyntikoju) | (−32, 0, −55) | sama klusteri |
| Ilmapallokaari (Huvipuisto_Ilmapallokaari) | (14, 0, −160) | lähtöalueen sisäänkäynti |
| Lippunauha (Huvipuisto_Lippunauha) | (−16, 0, +60) | suoralla mutkien 2/3 välissä |
| Pomppulinna (Huvipuisto_Pomppulinna) | (20, 0, −45) | vastapaino Teltta/Karuselli-klusterille |
| Lätäkkö | (−18, 0.03, 0) | mutka 2:n huipulla, tiellä |
| Kartio × 2 | (z=−115, offset +3.5) / (z=+115, offset −3.5) | mutkien 1 ja 3 huipuilla |
| Boost × 3 | z=−140 (mutka 1), z=−20 (mutka 2), z=+100 (mutka 3) | yksi per mutka-osuus, ei enää vain radan päissä |

## Reunatolpat

Värikkäät tolpat rajaavat kentän joka reunalla (X = ±35.5 ja Z = ±190) noin 12 yksikön välein.

## Käytännön hyöty

Koska mutkia on kolme ja ne vuorottelevat suunnasta, tarkista AINA kumman
puolen mutka on kyseessä ennen kuin sijoitat jotain: mutka 1 ja 3 kaartavat
OIKEALLE (+X), mutka 2 VASEMMALLE (−X). Suorat osuudet (−80…−40 ja
40…80 sekä 150…190 ja −190…−150) ovat turvallisia paikkoja isommille
koristeryhmille (Teltta/Karuselli/Myyntikoju-klusteri istuu juuri tällaisella
suoralla).
