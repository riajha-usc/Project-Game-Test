using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrapAgentHUD : MonoBehaviour
{
    public static TrapAgentHUD Instance { get; private set; }

    [Header("Settings")]
    public int maxPills    = 1;
    public int usesPerPill = 2;

    private List<Image> _fillRings = new List<Image>();
    private TextMeshProUGUI _label;
    private GameObject _panel;
    private int _displayedCharges = 0;

    static readonly Color ColFull  = new Color(0.18f, 0.85f, 0.28f);
    static readonly Color ColEmpty = new Color(0.18f, 0.18f, 0.18f, 1f);
    static readonly Color ColBg    = new Color(0.05f, 0.05f, 0.05f, 0.88f);
    static readonly Color ColText  = new Color(0.85f, 0.85f, 0.85f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildHUD();
        _panel.SetActive(true);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        bool playing = GameManager.Instance == null ||
                       GameManager.Instance.currentState == GameManager.GameState.Playing;

        bool overlayShowing =
            (UIManager.Instance != null &&
             ((UIManager.Instance.gameOverScreen != null && UIManager.Instance.gameOverScreen.activeSelf) ||
              (UIManager.Instance.startScreen    != null && UIManager.Instance.startScreen.activeSelf))) ||
            (TutorialManager.Instance != null &&
              TutorialManager.Instance.gameOverScreen != null &&
              TutorialManager.Instance.gameOverScreen.activeSelf);

        if (_panel != null) _panel.SetActive(playing && !overlayShowing);
        if (!playing || overlayShowing) return;

        int charges = TrapCombatAgentManager.Charges;
        _displayedCharges = charges;
        RefreshVisuals(charges);
    }

    void RefreshVisuals(int charges)
    {
        for (int i = 0; i < _fillRings.Count; i++)
        {
            float circleCharges = Mathf.Clamp(charges - i * usesPerPill, 0, usesPerPill);
            float fill = circleCharges / usesPerPill;
            _fillRings[i].fillAmount = fill;
            _fillRings[i].color = fill > 0f ? ColFull : ColEmpty;
        }

        if (_label != null)
            _label.text = $"{charges} charges left";
    }

    void BuildHUD()
    {
        float circleSize = 48f;
        float spacing    = 24f;
        float panelW     = Mathf.Max(220f, maxPills * circleSize + (maxPills - 1) * spacing + 64f);
        float panelH     = 150f;

        _panel = new GameObject("AgentPanel");
        _panel.transform.SetParent(transform, false);
        RectTransform panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.5f, 0f);
        panelRT.anchorMax        = new Vector2(0.5f, 0f);
        panelRT.pivot            = new Vector2(0.5f, 0f);
        panelRT.anchoredPosition = new Vector2(0f, 0f);
        panelRT.sizeDelta        = new Vector2(panelW, panelH);

        var panelImg = _panel.AddComponent<Image>();
        panelImg.color = ColBg;

        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_panel.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 0.75f);
        titleRT.anchorMax        = new Vector2(1f, 1f);
        titleRT.offsetMin        = new Vector2(8f, 0f);
        titleRT.offsetMax        = new Vector2(-8f, -4f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Deactivating Agents";
        titleTMP.fontSize  = 19f;
        titleTMP.color     = ColText;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        float circleY    = 0f; 
        float totalW     = maxPills * circleSize + (maxPills - 1) * spacing;
        float startX     = -totalW / 2f + circleSize / 2f;

        for (int i = 0; i < maxPills; i++)
        {
            float x = startX + i * (circleSize + spacing);

            var bgGO = new GameObject($"Pill_Bg_{i}");
            bgGO.transform.SetParent(_panel.transform, false);
            RectTransform bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchoredPosition = new Vector2(x, circleY);
            bgRT.sizeDelta        = new Vector2(circleSize, circleSize);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite     = GetCircleSprite();
            bgImg.color      = ColEmpty;
            bgImg.type       = Image.Type.Filled;
            bgImg.fillMethod = Image.FillMethod.Radial360;
            bgImg.fillAmount = 1f;

            var fillGO = new GameObject($"Pill_Fill_{i}");
            fillGO.transform.SetParent(_panel.transform, false);
            RectTransform fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchoredPosition = new Vector2(x, circleY);
            fillRT.sizeDelta        = new Vector2(circleSize, circleSize);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite        = GetCircleSprite();
            fillImg.color         = ColEmpty;
            fillImg.type          = Image.Type.Filled;
            fillImg.fillMethod    = Image.FillMethod.Radial360;
            fillImg.fillOrigin    = (int)Image.Origin360.Top;
            fillImg.fillClockwise = true;
            fillImg.fillAmount    = 0f;
            _fillRings.Add(fillImg);

            float innerSize = circleSize * 0.52f;
            var innerGO = new GameObject($"Pill_Inner_{i}");
            innerGO.transform.SetParent(_panel.transform, false);
            RectTransform innerRT = innerGO.AddComponent<RectTransform>();
            innerRT.anchoredPosition = new Vector2(x, circleY);
            innerRT.sizeDelta        = new Vector2(innerSize, innerSize);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.sprite = GetCircleSprite();
            innerImg.color  = ColBg;
        }

        var usesGO = new GameObject("UsesLabel");
        usesGO.transform.SetParent(_panel.transform, false);
        RectTransform usesRT = usesGO.AddComponent<RectTransform>();
        usesRT.anchorMin = new Vector2(0f, 0f);
        usesRT.anchorMax = new Vector2(1f, 0.25f);
        usesRT.offsetMin = new Vector2(8f, 2f);
        usesRT.offsetMax = new Vector2(-8f, 0f);
        _label = usesGO.AddComponent<TextMeshProUGUI>();
        _label.fontSize  = 18f;
        _label.color     = Color.white;
        _label.alignment = TextAlignmentOptions.Center;
    }

    static Sprite _circleSprite;
    static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        int res = 128;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        float center = res / 2f;
        float radius = res / 2f - 1f;
        Color[] pixels = new Color[res * res];

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dist  = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                pixels[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }

        tex.SetPixels(pixels);
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        return _circleSprite;
    }
}
