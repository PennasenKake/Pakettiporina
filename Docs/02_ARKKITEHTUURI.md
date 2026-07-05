# Arkkitehtuuri

_(Koodin ja järjestelmien yleiskuva: miten osat liittyvät toisiinsa.)_

## Yleiskuva (kerrokset)
_(Lyhyt kuvaus kerroksista: UI → pelilogiikka → auto & fysiikka → data → moottori. Voit viitata GDD:n kerroskaavioon.)_

## Hakemistokartta
_(Scripts/Core, Vehicle, Builder, Gameplay, CarCare, UI, Data — mitä kussakin on.)_

## Keskeiset järjestelmät
| Järjestelmä | Vastuu | Sijainti |
|---|---|---|
| _(GameManager)_ | _(keskustila, pisteet)_ | _(Scripts/Core)_ |
| _(RaceManager)_ | _(keikan kulku)_ | _(Scripts/Gameplay)_ |
| _(...)_ | _(...)_ | _(...)_ |

## Tapahtumat (GameEvents)
_(Listaa tapahtumat ja kuka lähettää / kuka kuuntelee: OnRaceStart, OnFinish, OnStarCollected, ...)_

## Datavirta
_(Esim. paketti → vaatimus → auton osat → fysiikka → ajo → huolto → pisteet → kertymä.)_

## Tallennus
_(SaveData-skeema: pisteet, avaukset, autot, asetukset. Tallennuspaikka ja formaatti (JSON).)_

## Riippuvuudet ja kokoonpano
_(asmdef: Pakettiporina-assembly, mahdolliset viittaukset.)_

## Laajennuspisteet
_(Miten lisätään uusi näkymä, uusi rata-palikka, uusi auton osa ilman ydinmuutoksia.)_
