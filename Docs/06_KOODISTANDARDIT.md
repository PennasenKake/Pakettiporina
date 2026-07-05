# Koodistandardit

_(Yhtenäiset käytännöt, jotta koodi pysyy luettavana ja ylläpidettävänä.)_

## Nimeäminen
_(Luokat PascalCase, julkiset jäsenet PascalCase, yksityiset kentät camelCase, vakiot UPPER_CASE. Namespace: Pakettiporina.)_

## Tiedostot ja kansiot
_(Yksi luokka per tiedosto, tiedoston nimi = luokan nimi. Kansiot vastaavat järjestelmiä (Core/Vehicle/...).)_

## Kommentit ja lokit
_(Lyhyet selittävät kommentit. Konsolilokit tagilla, esim. "[Car] ...". Ei lokitusta joka ruutu.)_

## MonoBehaviour-rakenne
_(Järjestys: kentät → Awake → OnEnable/OnDisable → Start → Update/FixedUpdate → omat metodit. Fysiikka FixedUpdatessa.)_

## Tapahtumat
_(Tilaa OnEnablessa, peru OnDisablessa. Vältä muistivuotoja staattisten tapahtumien kanssa.)_

## Data (ScriptableObject)
_(Pelin sisältö datana, ei koodiin kovakoodattuna. CreateAssetMenu-attribuutit.)_

## Versionhallinta
_(Commit-viestit selkeästi, esim. "M2: maali + tähdet". .meta-tiedostot mukaan, Library/ pois.)_
