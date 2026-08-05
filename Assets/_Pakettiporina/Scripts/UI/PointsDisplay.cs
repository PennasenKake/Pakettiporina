using UnityEngine;
using TMPro;

namespace Pakettiporina
{
    // Nayttaa GameManagerin pisteet TMP-tekstissa (esim. MainMenun tai hallin tahti-napissa).
    // Liita objektiin, jossa on TMP_Text, tai kytke pointsText kasin.
    public class PointsDisplay : MonoBehaviour
    {
        public TMP_Text pointsText;

        void Reset() { pointsText = GetComponent<TMP_Text>(); }

        void OnEnable()
        {
            Refresh();
            // HUOM: pelkka OnEnable-paivitys ei riita, koska pisteet voivat muuttua
            // SAMAN scenen sisalla (esim. tarran ostaminen paavalikossa) ilman etta
            // tama objekti koskaan disabloituu/enabloituu uudelleen - siksi tilataan
            // myos GameManagerin OnPointsChanged-tapahtuma, joka paivittaa tekstin
            // valittomasti aina kun pisteet oikeasti muuttuvat.
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged += HandlePointsChanged;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged -= HandlePointsChanged;
        }

        void HandlePointsChanged(int p)
        {
            if (pointsText == null) pointsText = GetComponent<TMP_Text>();
            if (pointsText != null) pointsText.text = p.ToString();
        }

        void Refresh()
        {
            if (pointsText == null) pointsText = GetComponent<TMP_Text>();
            int p = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            if (pointsText != null) pointsText.text = p.ToString();
        }
    }
}