using UnityEngine;
using Enigma.UI;

namespace Enigma.Tutorial
{
    public class ClueTutorialManager : MonoBehaviour
    {
        [Header("Reused Tutorial Systems")]
        [Tooltip("Your existing arrow controller — reused, not duplicated.")]
        [SerializeField] private TutorialArrowController tutorialArrowController;

        [Header("Clue Tutorial Targets")]
        [Tooltip("Transform of the Clue Box object the arrow should point at.")]
        [SerializeField] private Transform clueBoxTransform;

        [Tooltip("The ClueBox interactable component in the scene.")]
        [SerializeField] private ClueBox clueBoxInteractable;

        [Tooltip("The Clue Letter UI panel component.")]
        [SerializeField] private ClueLetterUI clueLetterUI;

        private bool _tutorialActive = false;

        public static ClueTutorialManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void BeginClueTutorial()
        {
            if (_tutorialActive) return;
            _tutorialActive = true;
            Debug.Log("[ClueTutorial] Phase 1 – Pointing arrow at Clue Box");
            if (tutorialArrowController != null && clueBoxTransform != null)
                tutorialArrowController.PointAt(clueBoxTransform);
            if (clueBoxInteractable != null)
                clueBoxInteractable.SetInteractable(true);
            if (clueBoxInteractable != null)
                clueBoxInteractable.OnClueBoxOpened += HandleClueBoxOpened;
        }

        private void HandleClueBoxOpened()
        {
            Debug.Log("[ClueTutorial] Phase 2 – Clue Box opened, showing Clue Letter UI");
            tutorialArrowController?.Hide();
            if (clueLetterUI != null)
                clueLetterUI.Show();
            if (clueLetterUI != null)
                clueLetterUI.OnShowKeysRequested += HandleShowKeysRequested;
        }

        private void HandleShowKeysRequested()
        {
            Debug.Log("[ClueTutorial] Phase 3 – Showing collected key list");
            if (clueLetterUI != null)
                clueLetterUI.ShowCollectedKeys(ClueKeyStore.Instance.GetAllKeys());

            _tutorialActive = false;
        }

        private void OnDestroy()
        {
            if (clueBoxInteractable != null)
                clueBoxInteractable.OnClueBoxOpened -= HandleClueBoxOpened;
            if (clueLetterUI != null)
                clueLetterUI.OnShowKeysRequested -= HandleShowKeysRequested;
        }
    }
}