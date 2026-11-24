# Suorituskyky: staattinen tarkistus 23.11.2026 22.13

## Player-asetukset (Android)
- Arkkitehtuurit: ARMv7, ARM64
- ! ARMv7 on mukana: kasvattaa kokoa; minSdk AndroidApiLevel31 -laitteet ovat 64-bittisiä (Korjaus: Suorituskyky/03).
- Scripting backend: IL2CPP
- Managed stripping: Minimal
- minSdk: AndroidApiLevel31 (Android 12+: vanhat lapsen kierrätyspuhelimet jäävät pois)
- Graphics API:t: Vulkan, OpenGLES3
- Development Build -overlay: Debug-buildissa neljän sormen kosketus 1 s näyttää FPS/GC/muisti.

## Laatu ja renderöinti
- Nykyinen laatutaso editorissa: High Fidelity
- Performant: vSync 0, AA 0, varjot Disable, pikselivalot 0
- Balanced: vSync 1, AA 0, varjot HardOnly, pikselivalot 1
- High Fidelity: vSync 1, AA 4, varjot All, pikselivalot 2
- Android käyttää oletuksena laatutasoa, joka on määritelty QualitySettingsissa (Android: Balanced). Balanced: HDR päällä, varjot päällä.
- ! URP-Balanced: HDR on päällä (kallis mobiilissa, ei tarvita tässä tyylissä). Harkitse HDR pois ja Performant-profiilia matalan tason laitteille.

## Tekstuurit
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_aloitusnakyma_tausta.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_halli_tausta.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_halli_tausta_uusi.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_taivas_skybox.png (2048x1024, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Docs/Kartat/_verify_satama_top.png (1024x2048, pakkaus Compressed)
- 285 tekstuuria projektissa; yli 1,5 Mpx yhteensä 10,4151 Mpx; luettavia 0.

## Mallit
- 73 mallia; luettavia (Read/Write) 0.

## Äänet
- pakettiporina_musiikki_halli.wav: 21,3 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 1,0 MB)
- pakettiporina_musiikki_pesu.wav: 16,6 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 0,8 MB)
- pakettiporina_musiikki_rata.wav: 13,0 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 0,6 MB)
- pakettiporina_musiikki_valikko.wav: 16,0 s, 32000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 1,0 MB)
- 13 tehosteääntä ilman Preload Audio Data: ensimmäinen toisto voi nykäistä (lataus levyltä). Lyhyet SFX kannattaa esiladata.

## Koodi (tiedossa olevat kuumat kohdat)
- Juice: nappien automaattiskannaus FindObjectsOfType<Button> 0,6 s välein (korjattu: harvennettu).
- CutoutAvoider: FindObjectsOfType<Selectable> 0,1 s välein laitteilla joissa on kameraloveksi (korjattu: harvennettu).
- RaceHUD.LateUpdate: laattojen koko ja paikka päivitettiin joka ruutu (korjattu: vain tekstin muuttuessa).
- Application.targetFrameRate / Screen.sleepTimeout: ei hallittu (korjattu: PerformanceManager).

**2 varoitusta.**
