using UnityEngine;
using Enigma.Tutorial;

namespace Enigma.GamePlay
{
    public class DoorKeyValidator : MonoBehaviour
    {
        [Header("Key Validation")]
        [Tooltip("The keyID that is considered the correct choice (e.g. 'K3').")]
        [SerializeField] private string correctKeyID = "K3";

        [Header("Tutorial Hook")]
        [Tooltip("The ClueTutorialManager in this scene.")]
        [SerializeField] private ClueTutorialManager clueTutorialManager;

        [Header("Door (Optional)")]
        [Tooltip("Animator on the door object — set trigger 'Open' on correct key.")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private string   doorOpenTrigger = "Open";

        public void OnPlayerSelectsKey(string selectedKeyID)
        {
            if (selectedKeyID == correctKeyID)
            {
                HandleCorrectKey();
            }
            else
            {
                HandleWrongKey(selectedKeyID);
            }
        }

        private void HandleCorrectKey()
        {
            Debug.Log("[DoorKeyValidator] Correct key selected! Opening door.");
            if (doorAnimator != null && !string.IsNullOrEmpty(doorOpenTrigger))
                doorAnimator.SetTrigger(doorOpenTrigger);
            if (clueTutorialManager != null)
                clueTutorialManager.BeginClueTutorial();
            else
                Debug.LogWarning("[DoorKeyValidator] ClueTutorialManager not assigned — " +
                                 "clue tutorial will not start.");
        }

        private void HandleWrongKey(string selectedID)
        {
            Debug.Log($"[DoorKeyValidator] Wrong key '{selectedID}' — try again.");
        }
    }
}