using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Scene References")]
    [Tooltip("The door the player must reach. Tag it 'Door' or assign manually.")]
    public Transform doorTransform;

    [Tooltip("The intro canvas panel (Image + MainText + StartButton). Shown on load, hidden on start.")]
    public GameObject introPanel;

    public GameObject startMenu;

    [Tooltip("Key TutorialEnd canvas panel — shown when player completes the key collection part of the tutorial.")]
    public GameObject keyTutorialEnd;

    [Tooltip("Clue TutorialEnd canvas panel — shown when the correct key is used at the door.")]
    public GameObject clueTutorialEnd;

    [Header("Popup Settings")]
    public float autoCloseDelay = 3f;

    bool _nearKeyShown;
    bool _collectedShown;
    bool _fourthKeyShown;
    bool _gameStarted;
    bool _atDoor;
    bool _trapTutorialEndShown;
    const float TRAP_DOOR_TRIGGER_DIST = 2f;

    int _keysCollected;
    public string tutorialType = "keys";

    GameObject _popupRoot;
    TMP_Text _popupText;
    Coroutine _autoCloseCoroutine;

    GameObject _arrowCanvas;
    GameObject _arrowObject;
    Image _arrowImg;
    GameObject _arrowLineRoot;
    List<RectTransform> _arrowLineDashes = new List<RectTransform>();
    List<Image> _arrowLineDashImages = new List<Image>();
    List<Image> _arrowLineShadows = new List<Image>();
    const int ARROW_LINE_DASH_COUNT = 36;
    const float ARROW_ENDPOINT_PADDING = 80f;
    ArrowTarget _arrowTarget = ArrowTarget.None;

    RectTransform _keyBarRect;
    RectTransform _keyButtonArrowTarget;
    float _keyButtonArrowBob;
    int _pendingKeyButtonIndex = -1;
    Coroutine _showDoorArrowsCoroutine;
    bool _doorUiShownWhenEnabled;

    enum ArrowTarget { None, KeyBar, Door, KeyButton, Clue }

    bool _clueOpened;
    GameObject _clueBoxGO;
    ClueBox _clueBox;

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

        if (introPanel != null)
        {
            Time.timeScale = 0f;
            introPanel.SetActive(true);
        }
        else
        {
            OnStartPressed();
        }
    }

    void Update()
    {
        if (!_gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OnStartPressed();
            return;
        }

        UpdateArrow();

        if (tutorialType == "traps" && !_trapTutorialEndShown && doorTransform != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(player.transform.position, doorTransform.position) <= TRAP_DOOR_TRIGGER_DIST)
            {
                _trapTutorialEndShown = true;
                ShowTrapTutorialEnd();
            }
        }

        if (_atDoor)
        {
            bool buttonsEnabled = KeyInventoryUI.Instance != null && KeyInventoryUI.Instance.KeyButtonsInteractable;
            if (buttonsEnabled)
            {
                if (!_doorUiShownWhenEnabled)
                {
                    _doorUiShownWhenEnabled = true;
                    ShowPopup("Click the button to select key.", 0f);
                    if (_showDoorArrowsCoroutine != null) StopCoroutine(_showDoorArrowsCoroutine);
                    _showDoorArrowsCoroutine = StartCoroutine(ShowDoorArrows());
                }
            }
            else
            {
                _doorUiShownWhenEnabled = false;
                if (_showDoorArrowsCoroutine != null)
                {
                    StopCoroutine(_showDoorArrowsCoroutine);
                    _showDoorArrowsCoroutine = null;
                }
                HideTutorialPopup();
                HideTutorialArrow();
                if (KeyInventoryUI.Instance != null) KeyInventoryUI.Instance.ClearHighlight();
            }
        }

        if (_atDoor && Input.GetKeyDown(KeyCode.Alpha3))
        {
            _atDoor = false;
            OnCorrectKeyUsedAtDoor();
        }
    }

    public void OnStartPressed()
    {
        if (_gameStarted) return;
        _gameStarted = true;

        if (introPanel != null)
            introPanel.SetActive(false);
        if (startMenu != null)
            startMenu.SetActive(false);

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();

        if (!_nearKeyShown)
        {
            _nearKeyShown = true;
            if (tutorialType == "keys")
                ShowPopup("Observe key shapes\n pass through, to collect.", 5f);
            if (tutorialType == "traps")
                ShowPopup("Analyze the traps, Assess your movements, and proceed carefully", 3.5f);
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
            StartCoroutine(ShowTutorialEndAfterFly());
        }
    }

    IEnumerator ShowTutorialEndAfterFly()
    {
        yield return new WaitForSeconds(0.75f);

        if (keyTutorialEnd != null)
        {
            keyTutorialEnd.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void OnTutorialEndNextPressed()
    {
        if (keyTutorialEnd != null)
            keyTutorialEnd.SetActive(false);

        Time.timeScale = 1f;
        StartCoroutine(Phase2Sequence());
    }

    IEnumerator Phase2Sequence()
    {
        SpawnClueBox();
        ShowPopup("Walk to the clue on the wall.", 0f);
        ShowArrow(ArrowTarget.Clue);

        yield return WaitUntilNearClue(2.5f);

        HideTutorialPopup();
        HideTutorialArrow();

        yield return new WaitUntil(() => _clueOpened);

        HideTutorialPopup();
        yield return new WaitForSeconds(0.5f);

        ShowPopup("Note the key whose shape\nmatches the clue description!", 5f);
        int correctIdx = GetCorrectKeyButtonIndex();
        if (correctIdx >= 0) ShowArrowOnKeyButton(correctIdx);
        yield return new WaitForSecondsRealtime(5.5f);
        HideTutorialArrow();
    }

    void SpawnClueBox()
    {
        ClueBoxGenerator gen = new GameObject("_TutClueGen").AddComponent<ClueBoxGenerator>();
        gen.boxWidth = 0.7f;
        gen.boxHeight = 0.6f;
        _clueBoxGO = gen.CreateClueBox(
            "TutorialClueBox",
            new Vector3(1.82f, 2.05f, 33.7f),
            Quaternion.identity,
            "Perfect balance: <color=#079D68><b>four equal sides</b></color> forming harmony.",
            0);
        Destroy(gen.gameObject);
        _clueBox = _clueBoxGO.GetComponent<ClueBox>();
        _clueBox.OnClueOpenedEvent += () => _clueOpened = true;
    }

    IEnumerator WaitUntilNearClue(float threshold)
    {
        Transform player = null;
        while (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            yield return null;
        }
        while (_clueBoxGO != null)
        {
            if (Vector3.Distance(player.position, _clueBoxGO.transform.position) <= threshold)
                yield break;
            yield return null;
        }
    }


    public void HideTutorialPopup()
    {
        if (_popupRoot != null) _popupRoot.SetActive(false);
        if (_autoCloseCoroutine != null)
        {
            StopCoroutine(_autoCloseCoroutine);
            _autoCloseCoroutine = null;
        }
    }

    public void ShowKeyBarArrow() => ShowArrow(ArrowTarget.KeyBar);
    public void HideTutorialArrow() => HideArrow();

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
            _popupText.textWrappingMode = TextWrappingModes.Normal;
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
        arrowRect.sizeDelta = new Vector2(50f, 50f);

        _arrowImg = _arrowObject.AddComponent<Image>();
        _arrowImg.sprite = BuildArrowSprite();
        _arrowImg.color = Color.white;

        _arrowLineRoot = new GameObject("ArrowLine");
        _arrowLineRoot.transform.SetParent(_arrowCanvas.transform, false);
        for (int i = 0; i < ARROW_LINE_DASH_COUNT; i++)
        {
            var shadowGO = new GameObject("DashShadow");
            shadowGO.transform.SetParent(_arrowLineRoot.transform, false);
            var shadowRect = shadowGO.AddComponent<RectTransform>();
            shadowRect.sizeDelta = new Vector2(18f, 5f);
            shadowRect.pivot = new Vector2(0.5f, 0.5f);
            var shadowImg = shadowGO.AddComponent<Image>();
            shadowImg.color = new Color(0f, 0f, 0f, 0f);
            _arrowLineShadows.Add(shadowImg);

            var dashGO = new GameObject("Dash");
            dashGO.transform.SetParent(_arrowLineRoot.transform, false);
            var dashRect = dashGO.AddComponent<RectTransform>();
            dashRect.sizeDelta = new Vector2(16f, 4f);
            dashRect.pivot = new Vector2(0.5f, 0.5f);
            var dashImg = dashGO.AddComponent<Image>();
            dashImg.color = Color.white;
            _arrowLineDashes.Add(dashRect);
            _arrowLineDashImages.Add(dashImg);
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
                float nx = (x + 0.5f) / S * 2f - 1f;
                float ny = (y + 0.5f) / S * 2f - 1f;

                bool inTriangle = ny > 0.1f && Mathf.Abs(nx) < (0.6f - ny * 0.6f);
                bool inStem = ny >= -0.75f && ny <= 0.1f && Mathf.Abs(nx) < 0.13f;

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
            _arrowLineRoot.SetActive(_arrowTarget == ArrowTarget.KeyBar || _arrowTarget == ArrowTarget.Clue);

        if (_arrowTarget == ArrowTarget.KeyBar)
            UpdateKeyBarArrow();
        else if (_arrowTarget == ArrowTarget.KeyButton)
            UpdateKeyButtonArrow();
        else if (_arrowTarget == ArrowTarget.Clue)
            UpdateClueArrow();
    }

    void UpdateKeyButtonArrow()
    {
        if (_keyButtonArrowTarget == null && _pendingKeyButtonIndex >= 0 && KeyInventoryUI.Instance != null)
            _keyButtonArrowTarget = KeyInventoryUI.Instance.GetButtonAtIndex(_pendingKeyButtonIndex);

        if (_keyButtonArrowTarget == null) return;

        Vector3[] corners = new Vector3[4];
        _keyButtonArrowTarget.GetWorldCorners(corners);
        Vector2 buttonTopCentre = ((Vector2)(corners[1] + corners[2])) * 0.5f;

        _keyButtonArrowBob += Time.unscaledDeltaTime * 3f;
        float bobOffset = Mathf.Sin(_keyButtonArrowBob) * 6f + 30f;

        Vector2 arrowPos = buttonTopCentre + new Vector2(0f, bobOffset);

        RectTransform arrowRect = _arrowObject.GetComponent<RectTransform>();
        arrowRect.position = new Vector3(arrowPos.x, arrowPos.y, 0f);
        _arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
    }

    void UpdateClueArrow()
    {
        if (_clueBoxGO == null || Camera.main == null) return;

        Vector3 clueScreenRaw = Camera.main.WorldToScreenPoint(_clueBoxGO.transform.position);
        if (clueScreenRaw.z < 0f) clueScreenRaw = -clueScreenRaw;

        float margin = 100f;
        Vector2 clueScreen = new Vector2(
            Mathf.Clamp(clueScreenRaw.x, margin, Screen.width - margin),
            Mathf.Clamp(clueScreenRaw.y, margin, Screen.height - margin));

        Vector2 lineStart = new Vector2(Screen.width * 0.5f, Screen.height * 0.35f);
        Vector2 lineEnd = clueScreen;

        Vector2 dir = lineEnd - lineStart;
        float length = dir.magnitude;
        if (length < 1f) return;
        dir /= length;

        float pad = Mathf.Min(ARROW_ENDPOINT_PADDING, length * 0.35f);
        Vector2 startPadded = lineStart + dir * pad;
        Vector2 endPadded = lineEnd - dir * pad;

        float march = (Time.time * 0.8f) % 1f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < _arrowLineDashes.Count; i++)
        {
            float t = ((i + march) / _arrowLineDashes.Count) % 1f;
            float alpha = Mathf.Lerp(0.12f, 1.0f, t);

            Vector2 pos = Vector2.Lerp(startPadded, endPadded, t);

            var dash = _arrowLineDashes[i];
            dash.gameObject.SetActive(true);
            dash.position = new Vector3(pos.x, pos.y, 0f);
            dash.rotation = Quaternion.Euler(0f, 0f, angle);
            dash.sizeDelta = new Vector2(16f, 4f);
            if (i < _arrowLineDashImages.Count)
                _arrowLineDashImages[i].color = new Color(1f, 1f, 1f, alpha);

            if (i < _arrowLineShadows.Count)
            {
                var shadow = _arrowLineShadows[i];
                shadow.GetComponent<RectTransform>().position = new Vector3(pos.x + 1.5f, pos.y - 1.5f, 0f);
                shadow.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, angle);
                shadow.GetComponent<RectTransform>().sizeDelta = new Vector2(18f, 5f);
                shadow.color = new Color(0f, 0f, 0f, alpha * 0.35f);
            }
        }

        float finalAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        RectTransform arrowRect = _arrowObject.GetComponent<RectTransform>();
        arrowRect.position = new Vector3(endPadded.x, endPadded.y, 0f);
        _arrowObject.transform.rotation = Quaternion.Euler(0f, 0f, finalAngle - 90f);
        if (_arrowImg != null) _arrowImg.color = Color.white;
    }

    void UpdateKeyBarArrow()
    {
        for (int i = 0; i < _arrowLineDashImages.Count; i++)
            if (_arrowLineDashImages[i] != null)
                _arrowLineDashImages[i].color = Color.white;
        for (int i = 0; i < _arrowLineShadows.Count; i++)
            if (_arrowLineShadows[i] != null)
                _arrowLineShadows[i].color = new Color(0f, 0f, 0f, 0f);

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
            float t = (i + 1) / (float)(_arrowLineDashes.Count + 1);
            float oneMinusT = 1f - t;
            Vector2 pos = oneMinusT * oneMinusT * startPadded
                            + 2f * oneMinusT * t * control
                            + t * t * endPadded;
            Vector2 tangent = 2f * oneMinusT * (control - startPadded)
                            + 2f * t * (endPadded - control);
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

    public void ShowPopup(string message, float duration)
    {
        if (_autoCloseCoroutine != null)
            StopCoroutine(_autoCloseCoroutine);

        if (_popupText != null)
            _popupText.text = message;
        _popupRoot.SetActive(true);

        if (duration > 0f)
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

        _atDoor = true;
    }

    IEnumerator ShowDoorArrows()
    {
        ShowArrow(ArrowTarget.KeyBar);
        yield return new WaitForSecondsRealtime(2f);
        int correctIdx = GetCorrectKeyButtonIndex();
        if (correctIdx >= 0)
        {
            ShowArrowOnKeyButton(correctIdx);
            if (KeyInventoryUI.Instance != null)
                KeyInventoryUI.Instance.HighlightButton(correctIdx);
        }
        _showDoorArrowsCoroutine = null;
    }

    public void OnKeyUsedAtDoor()
    {
        HideTutorialArrow();
        HideTutorialPopup();
        if (KeyInventoryUI.Instance != null)
            KeyInventoryUI.Instance.ClearHighlight();

        if (clueTutorialEnd != null)
        {
            clueTutorialEnd.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void OnCorrectKeyUsedAtDoor()
    {
        HideTutorialArrow();
        HideTutorialPopup();
        if (KeyInventoryUI.Instance != null)
            KeyInventoryUI.Instance.ClearHighlight();

        if (clueTutorialEnd != null)
        {
            clueTutorialEnd.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    int GetCorrectKeyButtonIndex()
    {
        if (KeyGenerator.Instance == null || KeyInventory.Instance == null) return -1;
        return KeyInventory.Instance.GetIndexForShape(KeyGenerator.Instance.correctShape);
    }

    public void ShowArrowOnKeyButton(int index)
    {
        _keyButtonArrowTarget = null;
        _keyButtonArrowBob = 0f;
        _pendingKeyButtonIndex = index;
        ShowArrow(ArrowTarget.KeyButton);
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

    void ShowTrapTutorialEnd()
    {
        HideTutorialPopup();
        HideTutorialArrow();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            var ctrl = p.GetComponent<CharacterController>();
            if (ctrl != null) ctrl.enabled = false;
        }

        if (clueTutorialEnd != null)
        {
            clueTutorialEnd.SetActive(true);

            // Wire up any buttons to return to main menu
            foreach (var btn in clueTutorialEnd.GetComponentsInChildren<UnityEngine.UI.Button>(true))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("MainMenu-Scene");
                });
            }
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_popupRoot != null) Destroy(_popupRoot);
        if (_arrowCanvas != null) Destroy(_arrowCanvas);
    }
}