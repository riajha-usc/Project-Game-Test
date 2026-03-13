using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    public class ClueLetterUI : MonoBehaviour
    {
        public event System.Action OnShowKeysRequested;

        [Header("── Clue Letter Panel ──")]
        [Tooltip("The root panel that wraps the entire Clue Letter overlay.")]
        [SerializeField] private GameObject clueLetterPanel;

        [Tooltip("The 'CLUE 1' label at the top of the clue card.")]
        [SerializeField] private TextMeshProUGUI clueNumberLabel;

        [Tooltip("The main clue body text (supports rich-text for keyword highlights).")]
        [SerializeField] private TextMeshProUGUI clueBodyText;

        [Tooltip("The '✗ Ignore spinning keys' label.")]
        [SerializeField] private TextMeshProUGUI wrongInterpretationText;

        [Tooltip("The '✓ Find the key that stands still' label.")]
        [SerializeField] private TextMeshProUGUI correctInterpretationText;

        [Header("── Footer Buttons ──")]
        [Tooltip("The ← BACK button.")]
        [SerializeField] private Button backButton;

        [Tooltip("The SHOW KEYS → button (replaces 'NEXT STEP' per reference notes).")]
        [SerializeField] private Button showKeysButton;

        [Tooltip("Label on the Show Keys button (set in inspector or keep default).")]
        [SerializeField] private TextMeshProUGUI showKeysButtonLabel;

        [Header("── Collected Keys Panel ──")]
        [Tooltip("The panel that slides in / replaces the clue letter when " +
                 "'Show Keys' is pressed. Should be inactive at start.")]
        [SerializeField] private GameObject collectedKeysPanel;

        [Tooltip("Parent transform inside collectedKeysPanel where KeyCard " +
                 "prefabs are spawned (a HorizontalLayoutGroup works well).")]
        [SerializeField] private Transform keyCardContainer;

        [Tooltip("Prefab for a single key card — should match the K1/K2/K3/K4 " +
                 "card style shown in the reference image.")]
        [SerializeField] private KeyCardUI keyCardPrefab;

        [Tooltip("'← BACK' button inside the collected keys panel.")]
        [SerializeField] private Button keysBackButton;

        [Header("── Clue Content ──")]
        [TextArea(2, 5)]
        [Tooltip("Rich-text body. Use <mark=#C8860080>word</mark> for gold highlights.")]
        [SerializeField] private string clueBody =
            "\"Not all that <mark=#C8860080>spins</mark> is gold. " +
            "One stands <mark=#C8860080>still</mark> with <mark=#C8860080>purpose</mark>.\"";

        [SerializeField] private string wrongInterpretation  = "Ignore spinning keys";
        [SerializeField] private string correctInterpretation= "Find the key that stands <b>still</b>";

        private void Awake()
        {
            if (clueLetterPanel    != null) clueLetterPanel.SetActive(false);
            if (collectedKeysPanel != null) collectedKeysPanel.SetActive(false);

            if (showKeysButton != null)
                showKeysButton.onClick.AddListener(HandleShowKeysClicked);

            if (backButton != null)
                backButton.onClick.AddListener(Hide);

            if (keysBackButton != null)
                keysBackButton.onClick.AddListener(HandleKeysBack);

            if (clueNumberLabel  != null) clueNumberLabel.text  = "CLUE 1";
            if (clueBodyText     != null) clueBodyText.text     = clueBody;
            if (wrongInterpretationText  != null)
                wrongInterpretationText.text  = wrongInterpretation;
            if (correctInterpretationText != null)
                correctInterpretationText.text = correctInterpretation;
            if (showKeysButtonLabel != null)
                showKeysButtonLabel.text = "SHOW KEYS  →";
        }

        public void Show()
        {
            if (clueLetterPanel != null)
                clueLetterPanel.SetActive(true);

            Debug.Log("[ClueLetterUI] Clue Letter panel shown");
        }

        public void Hide()
        {
            if (clueLetterPanel    != null) clueLetterPanel.SetActive(false);
            if (collectedKeysPanel != null) collectedKeysPanel.SetActive(false);
        }

        public void ShowCollectedKeys(IReadOnlyList<ClueKeyStore.ClueKeyDisplayData> keys)
        {
            if (clueLetterPanel    != null) clueLetterPanel.SetActive(false);
            if (collectedKeysPanel != null) collectedKeysPanel.SetActive(true);
            if (keyCardContainer != null)
                foreach (Transform child in keyCardContainer)
                    Destroy(child.gameObject);
            if (keyCardPrefab == null || keyCardContainer == null) return;
            foreach (var keyData in keys)
            {
                KeyCardUI card = Instantiate(keyCardPrefab, keyCardContainer);
                card.Populate(keyData);
            }
            Debug.Log($"[ClueLetterUI] Showing {keys.Count} collected key(s)");
        }

        private void HandleShowKeysClicked()
        {
            Debug.Log("[ClueLetterUI] Show Keys clicked");
            OnShowKeysRequested?.Invoke();
        }

        private void HandleKeysBack()
        {
            if (collectedKeysPanel != null) collectedKeysPanel.SetActive(false);
            if (clueLetterPanel    != null) clueLetterPanel.SetActive(true);
        }
    }
}