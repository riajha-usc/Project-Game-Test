using UnityEngine;

namespace Enigma.Tutorial
{
    public class TutorialArrowController : MonoBehaviour
    {
        [Header("Arrow Object")]
        [Tooltip("The GameObject that IS the arrow (world or UI).")]
        [SerializeField] private GameObject arrowObject;

        [Tooltip("If the arrow is a UI element, assign its RectTransform here " +
                 "so it can be repositioned in screen space.")]
        [SerializeField] private RectTransform arrowRectTransform;

        [Header("World → Screen Conversion")]
        [Tooltip("Assign if you need to convert a world-space target to screen " +
                 "position for a UI arrow. Leave blank for world-space arrows.")]
        [SerializeField] private Camera worldCamera;

        [Header("Offset")]
        [Tooltip("Offset applied to the final arrow position (world units or pixels).")]
        [SerializeField] private Vector3 positionOffset = new Vector3(0f, 1.5f, 0f);

        public void PointAt(Transform target)
        {
            if (arrowObject == null || target == null) return;

            arrowObject.SetActive(true);

            if (arrowRectTransform != null)
            {
                Camera cam = worldCamera != null ? worldCamera : Camera.main;
                Vector3 screenPos = cam.WorldToScreenPoint(target.position);
                arrowRectTransform.position = screenPos + positionOffset;
            }
            else
            {
                arrowObject.transform.position = target.position + positionOffset;
            }

            Debug.Log($"[TutorialArrow] Pointing at '{target.name}'");
        }

        public void Hide()
        {
            if (arrowObject != null)
                arrowObject.SetActive(false);

            Debug.Log("[TutorialArrow] Hidden");
        }

        public void Show()
        {
            if (arrowObject != null)
                arrowObject.SetActive(true);
        }
    }
}