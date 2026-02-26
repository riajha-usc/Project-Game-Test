using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractHint : MonoBehaviour
{
    private GameObject hintUI;
    private Canvas hintCanvas;
    private bool showing = false;

    public void Show()
    {
        if (showing) return;
        showing = true;

        GameObject canvasGO = new GameObject("HintCanvas");
        hintCanvas = canvasGO.AddComponent<Canvas>();
        hintCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hintCanvas.sortingOrder = 50;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        hintUI = new GameObject("HintText", typeof(RectTransform));
        hintUI.transform.SetParent(canvasGO.transform, false);

        TextMeshProUGUI tmp = hintUI.AddComponent<TextMeshProUGUI>();
        tmp.text = "Press <b>E</b> to interact";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        RectTransform rect = hintUI.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 60);
        rect.sizeDelta = new Vector2(400, 40);

        GameObject bgGO = new GameObject("HintBG", typeof(RectTransform));
        bgGO.transform.SetParent(hintUI.transform, false);
        bgGO.transform.SetAsFirstSibling();
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.5f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(20, 10);
    }

    public void Hide()
    {
        showing = false;
        if (hintCanvas != null)
            Destroy(hintCanvas.gameObject);
        hintUI = null;
    }
}