# Päätökset ja avoimet kysymykset opetussarjalle

## Tehdyt päätökset
1. Sarja: "How to code your own game", 21 osaa, 4 jaksoa (Perusta, Peli, Julkaisu, Oma peli). Ks. 03_osaluettelo_v2.md.
2. Yksi osa = yksi käsite = 20-40 min = alle 100 riviä uutta koodia; jokaisessa tehtävä + ratkaisu; osa 1 antaa pelattavan hetken alle 15 min.
3. Ilmainen: kaikki artikkelit ja opetuskoodi + harmaat laatikot + yksi näyte-FBX + yksi näytetyökalu. Maksullinen: FBX-mallit, 43 työkalua, UI-spritet ja äänet, scenet, loppuprojektit (hinnat vain ehdotuksia: 19/29/39 EUR).
4. Validointi ensin: julkaise osat 1-3 ilmaisena, mittaa kiinnostus, kysy yleisöltä, mitä ostaisivat.
5. Opetuskoodi kirjoitetaan ERIKSEEN yksinkertaistettuna; projektin koodi (17 651 riviä) on vain lähde.

## Pakolliset ennen myyntiä (blokkerit)
| # | Asia | Miksi |
|---|---|---|
| 1 | Täytä Docs/05 / 05_lisenssi_inventaario.md: jokaisen mallin, äänen, fontin lähde ja lisenssi | ilman tätä ei voi myydä; README sanoo omat Blender-mallit, M6.2 mainitsee Kenneyn |
| 2 | Päätä repo-jako: julkinen opetusrepo + yksityinen myytävä repo | nykyinen julkinen repo sisältää FBX:t ja työkalut ilmaiseksi |
| 3 | Committaa ja pushaa; päivitä GitHub-README (kuvat, nykytila) | GitHub on 8 commitia jäljessä, README vanha |
| 4 | Korjaa pelin sanasto: "tähti" = sekä pisteet että ajon arvosana | osa 13 opettaisi sekavaa mallia |
| 5 | Lisää automaattitestit (Progress.RateRun, SaveData) | osa 19 lupaa testauksen, testejä ei ole |

## Avoimet kysymykset käyttäjälle
- Kieli: suomi, englanti vai molemmat?
- Alusta: oma sivusto, itch.io, Gumroad, Medium?
- Kohderyhmä: täysi aloittelija vai koodaaja joka haluaa mobiilipelin?
- Mallien alkuperä (omat Blender-työt vai Kenney / muu)?
- Verotus ja yritysmuoto tuotemyyntiin: selvitettävä erikseen (ei neuvontaa tässä).
- Unity-versio: pysytäänkö 2022.3 LTS:ssä vai siirrytäänkö uudempaan? Tarkista Unity Hubin suositus.

## Tiedossa olevat riskit
Kysyntää "ilmainen sarja + FBX- ja työkalupaketti mobiilille" ei löytynyt vastineena: voi olla markkinarako tai kysynnän puute. Versiot vanhenevat vuodessa. Työkalut pitää yleistää ennen myyntiä (kovakoodattuja nimiä).
