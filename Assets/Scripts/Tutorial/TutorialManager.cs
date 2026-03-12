using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Scene References")]
    [Tooltip("The door the player must reach. Tag it 'Door' or assign manually.")]
    public Transform doorTransform;

    [Tooltip("The intro canvas panel (Image + MainText + StartButton). Shown on load, hidden on start.")]
    public GameObject introPanel;

    [Tooltip("TutorialEnd canvas panel — shown when player presses 1-4 at the door.")]
    public GameObject tutorialEndPanel;

    [Header("Popup Settings")]
    public float autoCloseDelay = 3f;       
    public float proximityRadius = 2f;

    bool _nearKeyShown;
    bool _collectedShown;
    bool _fourthKeyShown;
    bool _gameStarted;

    int _keysCollected;

    GameObject _popupRoot;
    TMP_Text   _popupText;
    Coroutine  _autoCloseCoroutine;

    GameObject _arrowCanvas;
    GameObject _arrowObject;
    GameObject _arrowLineRoot;
    List<RectTransform> _arrowLineDashes = new List<RectTransform>();
    const int ARROW_LINE_DASH_COUNT = 48;
    const float ARROW_ENDPOINT_PADDING = 80f;
    ArrowTarget _arrowTarget = ArrowTarget.None;

    RectTransform _keyBarRect;

    Transform _doorArrowTarget;

    enum ArrowTarget { None, KeyBar, Door }


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        BuildPopupUI();
        BuildArrowUI();

        if (doorTransform == null)
        {
            var doorGO = GameObject.FindGameObjectWithTag("Door");
            if (doorGO != null) doorTransform = doorGO.transform;
        }

        // Freeze game and show intro panel on scene load
        Time.timeScale = 0f;
        if (introPanel != null)
            introPanel.SetActive(true);
    }

    void Update()
    {
        // While intro is showing, only listen for Space to start
        if (!_gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OnStartPressed();
            return;
        }

        UpdateArrow();
    }

    // Called by StartButton OnClick or Space bar
    public void OnStartPressed()
    {
        if (_gameStarted) return;
        _gameStarted = true;

        if (introPanel != null)
            introPanel.SetActive(false);

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();

        if (!_nearKeyShown)
        {
            _nearKeyShown = true;
            ShowPopup("Observe key shapes\n pass through, to collect.", 5f);
        }
    }

    public void OnKeyCollected()
    {
        _keysCollected++;

        if (!_collectedShown)
        {
            _collectedShown = true;
            ShowPopup("Collected Keys are to the upper right!", autoCloseDelay);
            ShowArrow(ArrowTarget.KeyBar);
        }

        if (!_fourthKeyShown && _keysCollected >= 4)
        {
            _fourthKeyShown = true;
            ShowPopup("All Keys Collected!\n Head to the door!", autoCloseDelay);
        }
    }


    void BuildPopupUI()
    {
        _popupRoot = new GameObject("TutorialPopup");
        _popupRoot.transform.SetParent(null);
        DontDestroyOnLoad(_popupRoot);

        Canvas canvas = _popupRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = _popupRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("PopupPanel");
        panel.transform.SetParent(_popupRoot.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.4f, 0.52f);
        panelRect.anchorMax = new Vector2(0.6f, 0.57f);
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

        _popupText = textGO.AddComponent<TextMeshProUGUI>();
        if (_popupText != null)
        {
            _popupText.fontSize = 22;
            _popupText.color = Color.white;
            _popupText.fontStyle = FontStyles.Bold;
            _popupText.alignment = TextAlignmentOptions.Center;
            _popupText.enableWordWrapping = true;
            if (TMP_Settings.defaultFontAsset != null)
                _popupText.font = TMP_Settings.defaultFontAsset;
        }

        _popupRoot.SetActive(false);
    }


    void BuildArrowUI()
    {
        _arrowCanvas = new GameObject("TutorialArrowCanvas");
        _arrowCanvas.transform.SetParent(null);
        DontDestroyOnLoad(_arrowCanvas);

        Canvas canvas = _arrowCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 49;
        _arrowCanvas.AddComponent<CanvasScaler>();

        _arrowObject = new GameObject("Arrow");
        _arrowObject.transform.SetParent(_arrowCanvas.transform, false);

        RectTransform arrowRect = _arrowObject.AddComponent<RectTransform>();
        arrowRect.sizeDelta = new Vector2(28f, 28f);

        Image arrowImg = _arrowObject.AddComponent<Image>();

        arrowImg.sprite = BuildArrowSprite();
        arrowImg.color = Color.white;

        _arrowLineRoot = new GameObject("ArrowLine");
        _arrowLineRoot.transform.SetParent(_arrowCanvas.transform, false);
        for (int i = 0; i < ARROW_LINE_DASH_COUNT; i++)
        {
            var dashGO = new GameObject("Dash");
            dashGO.transform.SetParent(_arrowLineRoot.transform, false);
            var dashRect = dashGO.AddComponent<RectTransform>();
            dashRect.sizeDelta = new Vector2(12f, 2f);
            dashRect.pivot = new Vector2(0.5f, 0.5f);
            var dashImg = dashGO.AddComponent<Image>();
            dashImg.color = Color.white;
            _arrowLineDashes.Add(dashRect);
        }

        _arrowCanvas.SetActive(false);
    }

    Sprite BuildArrowSprite()
    {
        const int S = 64;
        Texture2D tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;

        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                // Normalised coords [-1,1]
                float nx = (x + 0.5f) / S * 2f - 1f;
                float ny = (y + 0.5f) / S * 2f - 1f;

                // Arrow pointing UP: triangle top + rectangle stem
                bool inTriangle = ny > 0.1f && Mathf.Abs(nx) < (0.6f - ny * 0.6f);
                bool inStem     = ny >= -0.75f && ny <= 0.1f && Mathf.Abs(nx) < 0.13f;

                tex.SetPixel(x, y, (inTriangle || inStem) ? white : clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f));
    }

    void UpdateArrow()
    {
        if (_arrowTarget == ArrowTarget.None || !_arrowCanvas.activeSelf) return;

        if (_arrowLineRoot != null)
            _arrowLineRoot.SetActive(_arrowTarget == ArrowTarget.KeyBar);

        if (_arrowTarget == ArrowTarget.KeyBar)
        {
            UpdateKeyBarArrow();
        }
    }

    void UpdateKeyBarArrow()
    {
        if (_keyBarRect == null && KeyInventoryUI.Instance != null)
        {
            var flyTarget = KeyInventoryUI.Instance.GetFlyTarget();
            if (flyTarget != null)
                _keyBarRect = flyTarget.parent as RectTransform ?? flyTarget;
        }

        RectTransform popupPanelRect = null;
        if (_popupRoot != null && _popupRoot.transform.childCount > 0)
            popupPanelRect = _popupRoot.transform.GetChild(0).GetComponent<RectTransform>();

        if (_keyBarRect == null) return;

        Vector3[] keyCorners = new Vector3[4];
        _keyBarRect.GetWorldCorners(keyCorners);
        Vector2 lineEnd = (Vector2)(keyCorners[0] + keyCorners[1]) * 0.5f;

        Vector2 lineStart;
        if (popupPanelRect != null)
        {
            Vector3[] popupCorners = new Vector3[4];
            popupPanelRect.GetWorldCorners(popupCorners);
            lineStart = (Vector2)(popupCorners[2] + popupCorners[3]) * 0.5f;
        }
        else
        {
            lineStart = lineEnd + new Vector2(-200f, 0f);
        }

        Vector2 dir = lineEnd - lineStart;
        float length = dir.magnitude;
        if (length < 1f) length = 1f;
        dir /= length;
        // Inset both ends so the arrow doesn't touch the popup or key bar
        float pad = Mathf.Min(ARROW_ENDPOINT_PADDING, length * 0.4f);
        Vector2 startPadded = lineStart + dir * pad;
        Vector2 endPadded = lineEnd - dir * pad;
        float paddedLength = (endPadded - startPadded).magnitude;
        if (paddedLength < 1f) { startPadded = lineStart; endPadded = lineEnd; }

        Vector2 mid = (startPadded + endPadded) * 0.5f;
        Vector2 perp = new Vector2(dir.y, -dir.x);
        Vector2 control = mid - perp * Mathf.Min(80f, paddedLength * 0.4f);

        float endAngle = 0f;
        for (int i = 0; i < _arrowLineDashes.Count; i++)
        {
            float t = (i + 1) / (float)(ARROW_LINE_DASH_COUNT + 1);
            float oneMinusT = 1f - t;
            Vector2 pos = oneMinusT * oneMinusT * startPadded + 2f * oneMinusT * t * control + t * t * endPadded;
            Vector2 tangent = 2f * oneMinusT * (control - startPadded) + 2f * t * (endPadded - control);
            if (tangent.sqrMagnitude < 0.01f) tangent = endPadded - startPadded;
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            if (i == _arrowLineDashes.Count - 1) endAngle = angle;

            var dash = _arrowLineDashes[i];
            dash.position = new Vector3(pos.x, pos.y, 0f);
            dash.rotation = Quaternion.Euler(0f, 0f, angle);
            dash.sizeDelta = new Vector2(12f, 2f);
        }
        if (_arrowLineDashes.Count > 0)
            endAngle = Mathf.Atan2(endPadded.y - control.y, endPadded.x - control.x) * Mathf.Rad2Deg;

        RectTransform arrowRect = _arrowObject.GetComponent<RectTransform>();
        arrowRect.position = new Vector3(endPadded.x, endPadded.y, 0f);
        _arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, endAngle - 90f);
    }


    void ShowPopup(string message, float duration)
    {
        if (_autoCloseCoroutine != null)
            StopCoroutine(_autoCloseCoroutine);

        if (_popupText != null)
            _popupText.text = message;
        _popupRoot.SetActive(true);
        _autoCloseCoroutine = StartCoroutine(AutoClosePopup(duration));
    }

    IEnumerator AutoClosePopup(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _popupRoot.SetActive(false);
        _autoCloseCoroutine = null;

        if (_arrowTarget == ArrowTarget.KeyBar)
            HideArrow();
    }

    public void OnPlayerReachedDoor()
    {
        if (_arrowTarget == ArrowTarget.Door)
            HideArrow();

        ShowPopup("Press keys 1, 2, 3, 4\nto select key!", autoCloseDelay);
    }

    // Called by KeyInventoryUI when player presses 1-4 at the door
    public void OnKeyUsedAtDoor()
    {
        if (tutorialEndPanel != null)
            tutorialEndPanel.SetActive(true);
    }

    void ShowArrow(ArrowTarget target)
    {
        _arrowTarget = target;
        _arrowCanvas.SetActive(true);
    }

    void HideArrow()
    {
        _arrowTarget = ArrowTarget.None;
        _arrowCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_popupRoot != null) Destroy(_popupRoot);
        if (_arrowCanvas != null) Destroy(_arrowCanvas);
    }
}
