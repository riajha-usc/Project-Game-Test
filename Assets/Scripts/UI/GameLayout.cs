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
    public TMP_Text levelTitleText;
    public TMP_Text cluesProgressText;
    public TMP_Text attemptsProgressText;

    [Header("Auto-build UI if references are null")]
    public bool buildUIAtRuntime = true;

    [Header("Health HUD")]
    public bool buildHealthHUD = true;
    public Vector2 healthHUDTopLeftOffset = new Vector2(16f, -110f);
    public string healthLabelText = "Health";
    public int healthLabelFontSize = 22;
    public float healthLabelGap = 10f;
    public float healthLabelYOffset = -18f;
    public Vector2 healthSliderSize = new Vector2(172f, 26f);

    [Header("Health HUD Colors")]
    [Range(0f, 1f)] public float healthYellowThreshold = 0.5f;
    [Range(0f, 1f)] public float healthRedThreshold = 0.3f;
    public Color healthNormalColor = new Color(0.15f, 0.8f, 0.35f, 1f);
    public Color healthYellowColor = new Color(0.95f, 0.8f, 0.2f, 1f);
    public Color healthRedColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    RectTransform rootRect;
    float updateInterval = 0.2f;
    float nextUpdate;
    GameObject wrongKeyFeedbackObj;
    Coroutine wrongKeyFeedbackCoroutine;
    GameObject controlsPanel;
    GameObject levelTitleRoot;
    GameObject healthHUDRoot;
    Slider healthHUDSlider;
    Image healthHUDFillImage;
    TextMeshProUGUI healthHUDValueText;
    Coroutine healthBlinkCoroutine;
    bool healthBlinkVisible = true;
    Image healthVignetteImage;
    Coroutine healthVignetteCoroutine;

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
        if (rootRect == null)
            rootRect = GetComponent<RectTransform>();

        if (buildUIAtRuntime)
            BuildHUDUI();
        else if (levelTitleText == null)
            BuildLevelTitle();

        if (buildHealthHUD)
            EnsureHealthHUD();

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
        UpdateHealthHUD();
        Refresh();
    }

    public void Refresh()
    {
        string scene = SceneManager.GetActiveScene().name;
        ApplyLevelTitleForScene(scene);

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

        BuildLevelTitle();

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
        if (buildHealthHUD)
            EnsureHealthHUD();
        Refresh();
    }

    void BuildLevelTitle()
    {
        if (levelTitleText != null) return;
        if (rootRect == null)
            rootRect = GetComponent<RectTransform>();
        if (rootRect == null) return;

        levelTitleRoot = new GameObject("LevelTitle");
        levelTitleRoot.transform.SetParent(rootRect, false);
        var levelRect = levelTitleRoot.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 1f);
        levelRect.anchorMax = new Vector2(0.5f, 1f);
        levelRect.pivot = new Vector2(0.5f, 1f);
        levelRect.anchoredPosition = new Vector2(0f, -16f);
        levelRect.sizeDelta = new Vector2(260f, 36f);

        levelTitleText = CreateText(levelTitleRoot.transform, "Level 1", 28);
        levelTitleText.fontStyle = FontStyles.Bold;
        levelTitleText.alignment = TextAlignmentOptions.Center;
        levelTitleText.textWrappingMode = TextWrappingModes.NoWrap;
        // Lighter than clues/attempts: less padding + lower alpha so it does not dominate the top bar
        AddLightBackground(levelTitleRoot, 6f);
    }

    static string GetLevelTitleForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        if (sceneName == "Level1" || sceneName.StartsWith("Level1")) return "Level 1";
        if (sceneName == "Level2" || sceneName.StartsWith("Level2")) return "Level 2";
        if (sceneName == "Level3" || sceneName.StartsWith("Level3")) return "Level 3";
        return null;
    }

    void ApplyLevelTitleForScene(string sceneName)
    {
        if (levelTitleText == null) return;
        string title = GetLevelTitleForScene(sceneName);
        GameObject root = levelTitleRoot != null ? levelTitleRoot : levelTitleText.transform.parent?.gameObject;
        if (title != null)
        {
            levelTitleText.text = title;
            if (root != null) root.SetActive(true);
        }
        else if (root != null)
        {
            root.SetActive(false);
        }
    }

    void BuildControlsPanel()
    {
        controlsPanel = new GameObject("ControlsPanel");
        controlsPanel.transform.SetParent(rootRect, false);

        var panelRect = controlsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot     = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(16f, -16f);
        panelRect.sizeDelta = new Vector2(270f, 70f);

        AddLightBackground(controlsPanel, 14);

        var line1 = new GameObject("ControlsLine1");
        line1.transform.SetParent(controlsPanel.transform, false);
        var r1 = line1.AddComponent<RectTransform>();
        r1.anchorMin = new Vector2(0f, 0.5f);
        r1.anchorMax = new Vector2(1f, 1f);
        r1.offsetMin = new Vector2(8f, 0f);
        r1.offsetMax = new Vector2(-8f, 0f);
        var t1 = line1.AddComponent<TextMeshProUGUI>();
        t1.text = "<color=#AAAAAA>Move:</color>  <b>W A S D</b>";
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
        t2.text = "<color=#AAAAAA>Deactivate Traps:</color>  <b>F</b>";
        t2.fontSize = 20;
        t2.color = Color.white;
        t2.alignment = TextAlignmentOptions.Left;
        t2.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            t2.font = TMP_Settings.defaultFontAsset;
    }

    void AddLightBackground(GameObject parent, float padding, float alpha = 0.92f)
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
        img.color = new Color(0.08f, 0.08f, 0.1f, alpha);
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

    void EnsureHealthHUD()
    {
        if (healthHUDRoot != null) return;
        if (rootRect == null) rootRect = GetComponent<RectTransform>();
        if (rootRect == null) return;

        var player = GameObject.FindObjectOfType<PlayerMovement3D>();
        if (player == null) return;

        foreach (var legacySlider in player.GetComponentsInChildren<Slider>(true))
        {
            var c = legacySlider.GetComponentInParent<Canvas>();
            if (c != null && c.renderMode == RenderMode.WorldSpace)
                Destroy(legacySlider.gameObject);
        }

        healthHUDRoot = new GameObject("HealthHUD");
        healthHUDRoot.transform.SetParent(rootRect, false);
        var root = healthHUDRoot.AddComponent<RectTransform>();
        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = healthHUDTopLeftOffset;
        root.sizeDelta = new Vector2(270f, 54f);

        AddLightBackground(healthHUDRoot, 14f);

        const float labelWidth = 66f;
        var labelGO = new GameObject("HealthLabel");
        labelGO.transform.SetParent(root, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.45f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot    = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(8f, 0f);
        labelRect.sizeDelta = new Vector2(labelWidth, 0f);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = healthLabelText;
        labelTMP.fontSize = healthLabelFontSize;
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.color = Color.white;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;
        labelTMP.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null) labelTMP.font = TMP_Settings.defaultFontAsset;

        var sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(root, false);
        var sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.45f);
        sliderRect.anchorMax = new Vector2(0f, 1f);
        sliderRect.pivot = new Vector2(0f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(labelWidth + 8f + healthLabelGap, 0f);
        sliderRect.sizeDelta = new Vector2(healthSliderSize.x, 0f);

        var healthSlider = sliderGO.AddComponent<Slider>();
        healthSlider.transition = Selectable.Transition.None;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = 1f;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = null;
        bgImg.type = Image.Type.Simple;
        bgImg.color = new Color(0f, 0f, 0f, 0.35f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(4f, 3f);
        fillAreaRect.offsetMax = new Vector2(-4f, -3f);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRect = fillGO.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite = null;
        fillImg.type = Image.Type.Simple;
        fillImg.color = healthNormalColor;

        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = fillImg;

        var valueGO = new GameObject("HealthValueText");
        valueGO.transform.SetParent(root, false);
        var valueRect = valueGO.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0.42f);
        valueRect.offsetMin = Vector2.zero;
        valueRect.offsetMax = Vector2.zero;
        var valueTMP = valueGO.AddComponent<TextMeshProUGUI>();
        valueTMP.text = "100 / 100";
        valueTMP.fontSize = 16;
        valueTMP.fontStyle = FontStyles.Bold;
        valueTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        valueTMP.alignment = TextAlignmentOptions.Center;
        valueTMP.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null) valueTMP.font = TMP_Settings.defaultFontAsset;

        healthHUDSlider = healthSlider;
        healthHUDFillImage = fillImg;
        healthHUDValueText = valueTMP;
    }

    void UpdateHealthHUD()
    {
        if (!buildHealthHUD) return;
        if (healthHUDRoot == null) EnsureHealthHUD();
        if (healthHUDRoot == null) return;

        var player = GameObject.FindObjectOfType<PlayerMovement3D>();
        if (player == null) return;

        float max = Mathf.Max(0.001f, player.maxHp);
        float ratio = Mathf.Clamp01(player.hp / max);

        if (healthHUDSlider != null)
            healthHUDSlider.value = ratio;

        if (healthHUDValueText != null)
            healthHUDValueText.text = $"{Mathf.RoundToInt(player.hp)} / {Mathf.RoundToInt(player.maxHp)}";

        if (healthHUDFillImage != null)
        {
            var target =
                ratio <= healthRedThreshold ? healthRedColor :
                ratio <= healthYellowThreshold ? healthYellowColor :
                healthNormalColor;
            if (healthHUDFillImage.color != target)
                healthHUDFillImage.color = target;
        }

        bool shouldBlink = ratio <= healthYellowThreshold;
        if (shouldBlink && healthBlinkCoroutine == null)
            healthBlinkCoroutine = StartCoroutine(BlinkHealthBar(ratio));
        else if (!shouldBlink && healthBlinkCoroutine != null)
        {
            StopCoroutine(healthBlinkCoroutine);
            healthBlinkCoroutine = null;
            if (healthHUDFillImage != null) healthHUDFillImage.color = healthNormalColor;
            healthBlinkVisible = true;
        }

        bool shouldVignette = ratio <= healthRedThreshold;
        if (shouldVignette && healthVignetteCoroutine == null)
            healthVignetteCoroutine = StartCoroutine(PulseHealthVignette(ratio));
        else if (!shouldVignette && healthVignetteCoroutine != null)
        {
            StopCoroutine(healthVignetteCoroutine);
            healthVignetteCoroutine = null;
            if (healthVignetteImage != null) healthVignetteImage.color = Color.clear;
        }
    }

    IEnumerator PulseHealthVignette(float initialRatio)
    {
        EnsureHealthVignette();
        if (healthVignetteImage == null) yield break;

        while (true)
        {
            var player = FindObjectOfType<PlayerMovement3D>();
            if (player == null) yield break;

            float max = Mathf.Max(0.001f, player.maxHp);
            float ratio = Mathf.Clamp01(player.hp / max);

            if (ratio > healthYellowThreshold)
            {
                healthVignetteCoroutine = null;
                healthVignetteImage.color = Color.clear;
                yield break;
            }

            float maxAlpha = 0.28f;
            float speed    = 0.35f;
            var vigColor   = new Color(0.8f, 0f, 0f);

            float t = 0f;
            while (t < 1f) { t += Time.unscaledDeltaTime / speed; healthVignetteImage.color = new Color(vigColor.r, vigColor.g, vigColor.b, Mathf.Lerp(0f, maxAlpha, t)); yield return null; }
            t = 0f;
            while (t < 1f) { t += Time.unscaledDeltaTime / speed; healthVignetteImage.color = new Color(vigColor.r, vigColor.g, vigColor.b, Mathf.Lerp(maxAlpha, 0f, t)); yield return null; }

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    void EnsureHealthVignette()
    {
        if (healthVignetteImage != null) return;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("HealthVignette");
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetAsFirstSibling();

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        healthVignetteImage = go.AddComponent<Image>();
        healthVignetteImage.sprite = BuildVignetteSprite();
        healthVignetteImage.color = Color.clear;
        healthVignetteImage.raycastTarget = false;
    }

    static Sprite BuildVignetteSprite()
    {
        int res = 128;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[res * res];
        float borderWidth = res * 0.10f;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float edgeDist = Mathf.Min(x, y, res - 1 - x, res - 1 - y);
            float a = Mathf.InverseLerp(borderWidth, 0f, edgeDist);
            pixels[y * res + x] = new Color(1f, 1f, 1f, a);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    IEnumerator BlinkHealthBar(float initialRatio)
    {
        while (true)
        {
            var player = GameObject.FindObjectOfType<PlayerMovement3D>();
            if (player == null) yield break;

            float max = Mathf.Max(0.001f, player.maxHp);
            float ratio = Mathf.Clamp01(player.hp / max);

            if (ratio > healthYellowThreshold)
            {
                healthBlinkCoroutine = null;
                if (healthHUDFillImage != null) healthHUDFillImage.color = healthNormalColor;
                yield break;
            }

            Color barColor = ratio <= healthRedThreshold ? healthRedColor : healthYellowColor;
            Color dimColor = new Color(barColor.r, barColor.g, barColor.b, 0.25f);
            // Fast if both health red AND timer warning, slow if health red only
            bool bothCritical = ratio <= healthRedThreshold && LevelTimer.InWarnPhase;
            float beatSpeed = bothCritical ? 0.1f : 1f;
            float pauseTime = bothCritical ? 0.05f : 0.12f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / beatSpeed;
                if (healthHUDFillImage != null)
                    healthHUDFillImage.color = Color.Lerp(dimColor, barColor, t);
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / beatSpeed;
                if (healthHUDFillImage != null)
                    healthHUDFillImage.color = Color.Lerp(barColor, dimColor, t);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(pauseTime);
        }
    }
}
