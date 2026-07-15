using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Aloitusnakyman logiikka: "Pelaa" lataa pelin scenen, "Lopeta" sulkee sovelluksen.
    // Liita tyhjaan objektiin tai Canvasiin aloitusnakymassa.
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("Pelin scenen nimi. Lisaa scene Build Settingsiin! (nyt esim. SampleScene)")]
        public string gameSceneName = "SampleScene";

        void Start()
        {
            Debug.Log("[MainMenu] Aloitusnakyma valmis.");
        }

        // Kytke tama "Pelaa"-napin OnClickiin.
        public void PlayGame()
        {
            Debug.Log("[MainMenu] Ladataan peli: " + gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }

        // Kytke tama halutessasi "Lopeta"-napin OnClickiin.
        public void QuitGame()
        {
            Debug.Log("[MainMenu] Suljetaan peli.");
            Application.Quit();
        }
    }
}