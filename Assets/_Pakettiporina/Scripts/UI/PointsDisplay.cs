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

        void OnEnable() { Refresh(); }

        void Refresh()
        {
            if (pointsText == null) pointsText = GetComponent<TMP_Text>();
            int p = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            if (pointsText != null) pointsText.text = p.ToString();
        }
    }
}