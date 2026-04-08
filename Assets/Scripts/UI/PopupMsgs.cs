using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PopupMsgs
{
    public static GameObject Create(out TMP_Text popupText, string rootObjectName = "PopupMsg")
    {
        var root = new GameObject(rootObjectName);
        root.transform.SetParent(null);
        UnityEngine.Object.DontDestroyOnLoad(root);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("PopupPanel");
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.4f, 0.58f);
        panelRect.anchorMax = new Vector2(0.6f, 0.63f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var bgImage = panel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.8f);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panel.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        popupText = textGO.AddComponent<TextMeshProUGUI>();
        if (popupText != null)
        {
            popupText.fontSize = 22;
            popupText.color = Color.white;
            popupText.fontStyle = FontStyles.Bold;
            popupText.alignment = TextAlignmentOptions.Center;
            popupText.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.defaultFontAsset != null)
                popupText.font = TMP_Settings.defaultFontAsset;
        }

        root.SetActive(false);
        return root;
    }
}
