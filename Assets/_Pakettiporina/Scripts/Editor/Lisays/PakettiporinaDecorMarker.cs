#if UNITY_EDITOR
using UnityEngine;

namespace Pakettiporina.EditorTools
{
    // SIIRRETTY 19.8.2026 OMAAN TIEDOSTOONSA (oli aiemmin PakettiporinaDecor.cs:ssa
    // samassa tiedostossa staattisen PakettiporinaDecor-luokan kanssa). SYY: kun
    // yhdessa .cs-tiedostossa on kaksi luokkaa, Unityn script-fileID 11500000
    // (oletus-fileID skriptin "paaluokalle") osoittaa AINA siihen luokkaan jonka
    // nimi tasmaa TIEDOSTON nimeen - eli "PakettiporinaDecor.cs":ssa fileID 11500000
    // osoitti staattiseen PakettiporinaDecor-luokkaan, EI PakettiporinaDecorMarker-
    // komponenttiin. Kun scenetiedostoihin (Puisto/Huvipuisto/Maatila/Satama.unity)
    // korjattiin 19.8.2026 rikkinaiset m_Script-viittaukset suoraan YAML-tasolla
    // osoittamaan PakettiporinaDecor.cs:n guidiin + fileID 11500000, Unity yritti
    // sen vuoksi kayttaa STAATTISTA PakettiporinaDecor-luokkaa Component-tyyppina
    // ("missing class attribute ExtensionOfNativeClass", "class is abstract") -
    // vaara luokka, ei koskaan tarkoitus toimia komponenttina.
    //
    // KORJAUS: PakettiporinaDecorMarker on nyt AINOA luokka tassa tiedostossa, joten
    // fileID 11500000 tallä tiedoston OMALLA guidilla osoittaa nyt oikein siihen.
    // Scenetiedostot paivitetty vastaavasti (ks. M-LISTA.md 19.8.2026-merkinnat).
    //
    // Merkkikomponentti jokaisessa PakettiporinaDecor.AddDecor():n luomassa
    // koristeessa. Talla loydetaan ja siivotaan KAIKKI vanhat koristeet (myos ne
    // joiden nimi/sijainti on peraisin AIEMMASTA koodiversiosta eika enaa esiinny
    // nykyisessa PRESETS-listassa) ennen uudelleensijoitusta - muuten esim.
    // suihkulahteen/kukkapenkkien vanhat, jo korvatut kopiot jaisivat scenneen
    // nakymattomina paallekkain uusien kanssa (SceneDump ei naytä niita koska se
    // lukee vain nykyista PRESETS-listaa).
    public class PakettiporinaDecorMarker : MonoBehaviour
    {
        // FBX-tiedoston nimi (ilman paatetta) josta tama koriste on luotu - talteen
        // jotta "16 - Vie sijainnit koodiksi" -tyokalu voi tunnistaa oikean mallin
        // vaikka instanceName poikkeaisi siita (esim. Puisto_Kukkapenkki_A).
        public string modelName;
    }
}
#endif
