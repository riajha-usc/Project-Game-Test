using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameLayout : MonoBehaviour
{
    public static GameLayout Instance { get; private set; }

    [Header("Optional: Assign in Editor to use existing UI")]
    public TMP_Text cluesProgressText;
    public TMP_Text attemptsProgressText;

    [Header("Auto-build UI if references are null")]
    public bool buildUIAtRuntime = true;

    RectTransform rootRect;
    float updateInterval = 0.2f;
    float nextUpdate;
    GameObject wrongKeyFeedbackObj;
    Coroutine wrongKeyFeedbackCoroutine;
    GameObject controlsPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (buildUIAtRuntime)
            BuildHUDUI();

        // ReparentHealthBarToTopLeft();
        nextUpdate = Time.unscaledTime + updateInterval;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.GameOver)
        {
            if (rootRect != null)
                rootRect.gameObject.SetActive(false);
            return;
        }
        else if (rootRect != null && !rootRect.gameObject.activeSelf)
        {
            rootRect.gameObject.SetActive(true);
        }

        if (Time.unscaledTime < nextUpdate) return;
        nextUpdate = Time.unscaledTime + updateInterval;
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.Instance == null) return;

        if (cluesProgressText != null)
        {
            int totalClues = GameManager.Instance.GetTotalCluesForCurrentLane();
            int solved = GameManager.Instance.cluesSolved;
            cluesProgressText.text = $"Clues: <color=#5B9BD5><b>{solved}/{totalClues}</b></color>";
            var cluesContainer = cluesProgressText.transform.parent;
            if (cluesContainer != null) cluesContainer.gameObject.SetActive(true);
        }

        if (attemptsProgressText != null)
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            int used = scene == "Level1-Lane3" ? GameManager.Instance.codeAttemptCount : GameManager.Instance.keyAttemptCount;
            int max = GameManager.Instance.GetMaxAttemptsForCurrentLane();
            attemptsProgressText.text = $"Unlock Attempts: <color=#5B9BD5><b>{used}/{max}</b></color>";
        }

    }

    public void ShowWrongKeyFeedback()
    {
        ShowWrongFeedback("Wrong key!");
    }

    public void ShowWrongCodeFeedback()
    {
        ShowWrongFeedback("Wrong code!");
    }

    public void HideWrongFeedback()
    {
        if (wrongKeyFeedbackCoroutine != null)
        {
            StopCoroutine(wrongKeyFeedbackCoroutine);
            wrongKeyFeedbackCoroutine = null;
        }
        if (wrongKeyFeedbackObj != null)
            wrongKeyFeedbackObj.SetActive(false);
    }

    void ShowWrongFeedback(string message)
    {
        if (wrongKeyFeedbackCoroutine != null)
        {
            StopCoroutine(wrongKeyFeedbackCoroutine);
            wrongKeyFeedbackCoroutine = null;
        }
        EnsureWrongKeyFeedback();
        if (wrongKeyFeedbackObj != null)
        {
            var tmp = wrongKeyFeedbackObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null) tmp.text = message;
            wrongKeyFeedbackObj.SetActive(true);
            wrongKeyFeedbackCoroutine = StartCoroutine(HideWrongKeyFeedbackAfter(2f));
        }
    }

    void EnsureWrongKeyFeedback()
    {
        if (wrongKeyFeedbackObj != null) return;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        wrongKeyFeedbackObj = new GameObject("WrongKeyFeedback");
        wrongKeyFeedbackObj.transform.SetParent(canvas.transform, false);
        var rect = wrongKeyFeedbackObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400f, 80f);

        var bg = wrongKeyFeedbackObj.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.9f, 0.2f, 0.2f, 0.85f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(wrongKeyFeedbackObj.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Wrong key!";
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        wrongKeyFeedbackObj.SetActive(false);
    }

    IEnumerator HideWrongKeyFeedbackAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (wrongKeyFeedbackObj != null)
            wrongKeyFeedbackObj.SetActive(false);
        wrongKeyFeedbackCoroutine = null;
    }

    void BuildHUDUI()
    {
        rootRect = GetComponent<RectTransform>();
        if (rootRect == null)
        {
            rootRect = gameObject.AddComponent<RectTransform>();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
        }

        // Clues 
        var cluesObj = new GameObject("CluesText");
        cluesObj.transform.SetParent(rootRect, false);
        var cluesRect = cluesObj.AddComponent<RectTransform>();
        cluesRect.anchorMin = new Vector2(1f, 1f);
        cluesRect.anchorMax = new Vector2(1f, 1f);
        cluesRect.pivot = new Vector2(1f, 1f);
        cluesRect.anchoredPosition = new Vector2(-16, -16);
        cluesRect.sizeDelta = new Vector2(130, 38);
        cluesProgressText = CreateText(cluesObj.transform, "Clues: 0/4", 25);
        cluesProgressText.alignment = TextAlignmentOptions.Center;
        cluesProgressText.textWrappingMode = TextWrappingModes.NoWrap;
        AddLightBackground(cluesObj, 12);

        var attemptsObj = new GameObject("AttemptsText");
        attemptsObj.transform.SetParent(rootRect, false);
        var attemptsRect = attemptsObj.AddComponent<RectTransform>();
        attemptsRect.anchorMin = new Vector2(1f, 1f);
        attemptsRect.anchorMax = new Vector2(1f, 1f);
        attemptsRect.pivot = new Vector2(1f, 1f);
        attemptsRect.anchoredPosition = new Vector2(-180, -16);
        attemptsRect.sizeDelta = new Vector2(220, 38);
        attemptsProgressText = CreateText(attemptsObj.transform, "Unlocked Attempts: 0", 25);
        attemptsProgressText.alignment = TextAlignmentOptions.Center;
        attemptsProgressText.textWrappingMode = TextWrappingModes.NoWrap;
        AddLightBackground(attemptsObj, 12);

        BuildControlsPanel();
        Refresh();
    }

    void BuildControlsPanel()
    {
        // Top-left controls hint panel
        controlsPanel = new GameObject("ControlsPanel");
        controlsPanel.transform.SetParent(rootRect, false);

        var panelRect = controlsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot     = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(16f, -16f);
        panelRect.sizeDelta = new Vector2(270f, 70f);

        AddLightBackground(controlsPanel, 14);

        // Line 1 — Movement
        var line1 = new GameObject("ControlsLine1");
        line1.transform.SetParent(controlsPanel.transform, false);
        var r1 = line1.AddComponent<RectTransform>();
        r1.anchorMin = new Vector2(0f, 0.5f);
        r1.anchorMax = new Vector2(1f, 1f);
        r1.offsetMin = new Vector2(8f, 0f);
        r1.offsetMax = new Vector2(-8f, 0f);
        var t1 = line1.AddComponent<TextMeshProUGUI>();
        t1.text = "<color=#AAAAAA>Move:</color>  <b>W A D</b>";
        t1.fontSize = 20;
        t1.color = Color.white;
        t1.alignment = TextAlignmentOptions.Left;
        t1.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            t1.font = TMP_Settings.defaultFontAsset;

        // Line 2 — Key selection
        var line2 = new GameObject("ControlsLine2");
        line2.transform.SetParent(controlsPanel.transform, false);
        var r2 = line2.AddComponent<RectTransform>();
        r2.anchorMin = new Vector2(0f, 0f);
        r2.anchorMax = new Vector2(1f, 0.5f);
        r2.offsetMin = new Vector2(8f, 0f);
        r2.offsetMax = new Vector2(-8f, 0f);
        var t2 = line2.AddComponent<TextMeshProUGUI>();
        t2.text = "<color=#AAAAAA>Select Key:</color>  <b>Mouse Click</b>";
        t2.fontSize = 20;
        t2.color = Color.white;
        t2.alignment = TextAlignmentOptions.Left;
        t2.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            t2.font = TMP_Settings.defaultFontAsset;
    }

    static Sprite roundedSprite;

    void AddLightBackground(GameObject parent, float padding)
    {
        var bg = new GameObject("Background");
        bg.transform.SetParent(parent.transform, false);
        bg.transform.SetAsFirstSibling();
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-padding, -padding);
        bgRect.offsetMax = new Vector2(padding, padding);
        var img = bg.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        img.sprite = GetRoundedSprite();
    }

    Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;
        int w = 64, h = 32;
        float r = 8f;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float px = x + 0.5f, py = y + 0.5f;
            bool inCenter = px >= r && px < w - r && py >= r && py < h - r;
            bool inCorner = (px < r || px >= w - r) && (py < r || py >= h - r);
            bool inside = inCenter;
            if (!inside && inCorner)
            {
                float cx = px < r ? r : w - r - 0.5f;
                float cy = py < r ? r : h - r - 0.5f;
                inside = (px - cx) * (px - cx) + (py - cy) * (py - cy) <= r * r;
            }
            else if (!inside) inside = true;
            tex.SetPixel(x, y, inside ? Color.white : Color.clear);
        }
        tex.Apply();
        roundedSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        return roundedSprite;
    }

    TMP_Text CreateText(Transform parent, string content, int fontSize)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
    }
}
