using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Enigma.UI
{
    public class KeyCardUI : MonoBehaviour
    {
        [Header("Card Elements")]
        [Tooltip("Top-left label: 'K1', 'K2', etc.")]
        [SerializeField] private TextMeshProUGUI keyIDLabel;

        [Tooltip("Centre icon sprite (key shape / symbol).")]
        [SerializeField] private Image keyIconImage;

        [Tooltip("Bottom colour label: 'RED', 'BLUE', 'YELLOW', etc.")]
        [SerializeField] private TextMeshProUGUI colorLabel;

        [Header("Accent / Border")]
        [Tooltip("The outline / border Image whose color is set to the key's accent color.")]
        [SerializeField] private Image borderImage;

        [Tooltip("The card background Image (dark navy #0D1B2A).")]
        [SerializeField] private Image backgroundImage;

        private static readonly Color CardBackground = new Color(0.05f, 0.11f, 0.17f); 

        public void Populate(ClueKeyStore.ClueKeyDisplayData data)
        {
            if (data == null) return;
            if (keyIDLabel   != null) keyIDLabel.text   = data.keyID;
            if (colorLabel   != null) colorLabel.text   = data.colorLabel.ToUpper();
            if (keyIconImage != null)
            {
                keyIconImage.sprite = data.keyIcon;
                keyIconImage.color  = data.accentColor;
            }
            if (borderImage  != null) borderImage.color = data.accentColor;
        }
    }
}