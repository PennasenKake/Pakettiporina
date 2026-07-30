using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakettiporina
{
    // Yksi saippuakupla pesukohtauksessa. Liita UI-objektiin, jossa on Image
    // (pyorea kupla-sprite, Raycast Target paalla). Napautus/klikkaus poksauttaa.
    [RequireComponent(typeof(Image))]
    public class Bubble : MonoBehaviour, IPointerDownHandler
    {
        [Header("Poksahdusanimaatio")]
        [Tooltip("Kuinka paljon kupla kasvaa poksahtaessa")]
        public float popScale = 1.3f;
        [Tooltip("Animaation kesto sekunteina")]
        public float popDuration = 0.15f;

        Image img;
        bool popped;

        void Awake()
        {
            img = GetComponent<Image>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Pop();
        }

        void Pop()
        {
            if (popped) return;
            popped = true;

            if (img != null) img.raycastTarget = false; // ei uudestaan napautettavissa

            if (AudioManager.Instance != null) AudioManager.Instance.PlayBubblePop();
            if (WashScreen.Instance != null) WashScreen.Instance.OnBubblePopped();
            Debug.Log("[Bubble] Poks!");

            StartCoroutine(PopAnim());
        }

        IEnumerator PopAnim()
        {
            Vector3 startScale = transform.localScale;
            Vector3 bigScale = startScale * popScale;
            float t = 0f;
            while (t < popDuration)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / popDuration);
                transform.localScale = Vector3.Lerp(startScale, bigScale, f);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        // Sallii kuplan kayttoonoton uudelleen (esim. jos scene ladataan uudelleen ilman jarjestelman resetointia).
        void OnEnable()
        {
            popped = false;
            if (img != null) img.raycastTarget = true;
            transform.localScale = Vector3.one;
        }
    }
}
