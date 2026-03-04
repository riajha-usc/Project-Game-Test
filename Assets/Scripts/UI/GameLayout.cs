using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameLayout : MonoBehaviour
{
    public static GameLayout Instance { get; private set; }

    [Header("Optional: Assign in Editor to use existing UI")]
    public TMP_Text laneProgressText;
    public TMP_Text cluesProgressText;
    public TMP_Text currentHintText;

    [Header("Auto-build UI if references are null")]
    public bool buildUIAtRuntime = true;

    [Header("Floating effect")]
    [Tooltip("Vertical bobbing amount in pixels")]
    public float floatAmplitude = 4f;
    [Tooltip("Speed of the float animation")]
    public float floatSpeed = 2f;

    RectTransform rootRect;
    RectTransform hintRect;
    Vector2 hintBasePos;
    float updateInterval = 0.2f;
    float nextUpdate;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ReparentHealthBarToTopLeft();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (buildUIAtRuntime && laneProgressText == null)
            BuildHUDUI();

        ReparentHealthBarToTopLeft();
        nextUpdate = Time.unscaledTime + updateInterval;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReparentHealthBarToTopLeft();
    }

    void ReparentHealthBarToTopLeft()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var pm = player.GetComponent<PlayerMovement3D>();
        if (pm == null || pm.healthbar == null) return;

        var healthBarRect = pm.healthbar.GetComponent<RectTransform>();
        if (healthBarRect == null) return;

        RectTransform pulseBarRect = null;
        if (pm.pulseBar != null)
            pulseBarRect = pm.pulseBar.GetComponent<RectTransform>();

        if (pulseBarRect == null)
        {
            if (healthBarRect.parent == transform) return;
        }
        else
        {
            if (healthBarRect.parent == transform && pulseBarRect.parent == transform) return;
        }

        var toDestroy = new List<GameObject>();
        foreach (Transform t in transform)
        {
            if (t.name == "HealthBarLabel" || t.name == "PulseBarLabel")
                toDestroy.Add(t.gameObject);
            else if (t.GetComponent<Slider>() != null && t != healthBarRect && t != pulseBarRect)
                toDestroy.Add(t.gameObject);
        }
        foreach (var go in toDestroy)
            Destroy(go);

        // health bar (same line)
        var labelObj = new GameObject("HealthBarLabel");
        labelObj.transform.SetParent(transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = new Vector2(16, -16);
        labelRect.sizeDelta = new Vector2(95, 35);
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "Health:";
        labelTmp.fontSize = 25;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            labelTmp.font = TMP_Settings.defaultFontAsset;

        healthBarRect.SetParent(transform, false);
        healthBarRect.anchorMin = new Vector2(0f, 1f);
        healthBarRect.anchorMax = new Vector2(0f, 1f);
        healthBarRect.pivot = new Vector2(0f, 1f);
        healthBarRect.anchoredPosition = new Vector2(16 + 95 + 8, -16);
        healthBarRect.sizeDelta = new Vector2(200, 40);

        if (pulseBarRect != null)
        {
            var pulseLabelObj = new GameObject("PulseBarLabel");
            pulseLabelObj.transform.SetParent(transform, false);
            var pulseLabelRect = pulseLabelObj.AddComponent<RectTransform>();
            pulseLabelRect.anchorMin = new Vector2(0f, 1f);
            pulseLabelRect.anchorMax = new Vector2(0f, 1f);
            pulseLabelRect.pivot = new Vector2(0f, 1f);
            pulseLabelRect.anchoredPosition = new Vector2(16, -70);
            pulseLabelRect.sizeDelta = new Vector2(95, 35);
            var pulseLabelTmp = pulseLabelObj.AddComponent<TextMeshProUGUI>();
            pulseLabelTmp.text = "Pulse:";
            pulseLabelTmp.fontSize = 25;
            pulseLabelTmp.fontStyle = FontStyles.Bold;
            pulseLabelTmp.color = Color.white;
            pulseLabelTmp.alignment = TextAlignmentOptions.Left;
            pulseLabelTmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            if (TMP_Settings.defaultFontAsset != null)
                pulseLabelTmp.font = TMP_Settings.defaultFontAsset;

            pulseBarRect.SetParent(transform, false);
            pulseBarRect.anchorMin = new Vector2(0f, 1f);
            pulseBarRect.anchorMax = new Vector2(0f, 1f);
            pulseBarRect.pivot = new Vector2(0f, 1f);
            pulseBarRect.anchoredPosition = new Vector2(16 + 95 + 8, -70);
            pulseBarRect.sizeDelta = new Vector2(200, 40);
        }
    }

    void Update()
    {
        if (hintRect != null)
        {
            float t = Time.unscaledTime * floatSpeed;
            hintRect.anchoredPosition = hintBasePos + new Vector2(0, Mathf.Sin(t) * floatAmplitude);
        }

        if (Time.unscaledTime < nextUpdate) return;
        nextUpdate = Time.unscaledTime + updateInterval;
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.Instance == null) return;

        int currentLane = GameManager.Instance.GetCurrentLaneNumber();
        int totalLanes = GameManager.Instance.totalLanes;

        if (laneProgressText != null)
            laneProgressText.text = $"Lanes: <color=#4A90D9>{currentLane}/{totalLanes}</color>";

        if (cluesProgressText != null)
            cluesProgressText.text = "Clues: <color=#4A90D9>0/4</color>"; // Placeholder

        if (currentHintText != null)
            currentHintText.text = "\"HINT: Explore to find clues.\""; // Placeholder
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

        // Lane
        var laneObj = new GameObject("LaneText");
        laneObj.transform.SetParent(rootRect, false);
        var laneRect = laneObj.AddComponent<RectTransform>();
        laneRect.anchorMin = new Vector2(0.5f, 1f);
        laneRect.anchorMax = new Vector2(0.5f, 1f);
        laneRect.pivot = new Vector2(0.5f, 1f);
        laneRect.anchoredPosition = new Vector2(0, -16);
        laneRect.sizeDelta = new Vector2(140, 38);
        laneProgressText = CreateText(laneObj.transform, "Lane 1/3", 25);
        laneProgressText.alignment = TextAlignmentOptions.Center;
        AddLightBackground(laneObj, 12);

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

        // Hint - bottom left
        var hintObj = new GameObject("HintText");
        hintObj.transform.SetParent(rootRect, false);
        var hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(0f, 0f);
        hintRect.pivot = new Vector2(0f, 0f);
        hintRect.anchoredPosition = new Vector2(16, 16);
        hintRect.sizeDelta = new Vector2(400, 40);
        this.hintRect = hintRect;
        hintBasePos = hintRect.anchoredPosition;
        currentHintText = CreateText(hintObj.transform, "\"Hint: Explore to find clues.\"", 30);
        currentHintText.color = Color.white;
        currentHintText.alignment = TextAlignmentOptions.BottomLeft;

        Refresh();
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
