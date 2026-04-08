using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class KeyInventoryUI : MonoBehaviour
{
    public static KeyInventoryUI Instance { get; private set; }
    public Transform keyBarParent;
    public Button keyButtonTemplate;
    public GameObject keysText;
    List<Button> spawnedButtons = new List<Button>();
    bool doorInRange = false;
    Coroutine flashCoroutine;
    Coroutine vibrateCoroutine;
    Coroutine _hintPopupCoroutine;
    Vector2 keyBarBasePos;

    GameObject _highlightOverlay;
    Coroutine _highlightCoroutine;
    Coroutine _sequenceRevealCoroutine;
    readonly List<Image> _sequenceShapeIcons = new List<Image>();
    readonly List<TMP_Text> _buttonLabels = new List<TMP_Text>();
    bool _sequenceRevealInProgress;
    bool _sequenceVRevealUsed;
    const KeyCode SEQUENCE_REVEAL_KEY = KeyCode.V;
    GameObject _hintPopupRoot;
    TMP_Text _hintPopupText;

    readonly DoorKeyArrowOverlay _doorKeyArrowOverlay = new DoorKeyArrowOverlay();
    Transform _cachedPlayerTransform;

    [Header("Optional: Key Shape Sprites (for Tutorial-1)")]
    public Sprite circleSprite;
    public Sprite squareSprite;
    public Sprite capsuleSprite;
    public Sprite crossSprite;

    public bool KeyButtonsInteractable { get; private set; }
    string SceneName => SceneManager.GetActiveScene().name;

    public string GetSequenceRevealUsedForAnalytics()
    {
        if (SceneName == "Level2")
            return _sequenceVRevealUsed ? "Yes" : "No";
        return "N/A";
    }
    bool UsesDoorSequenceReveal => SceneName == "Level2" || SceneName == "Level3";

    Dictionary<KeyHeadShape, Sprite> _generatedShapeSprites = new Dictionary<KeyHeadShape, Sprite>();

    void Awake()
    {
        Instance = this;
        if (keyBarParent != null)
            keyBarParent.gameObject.SetActive(false);
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        if (_doorKeyArrowOverlay.IsVisible && SceneName != "Level1")
            HideKeybarPressArrow();
        else if (_doorKeyArrowOverlay.IsVisible && SceneName == "Level1")
            _doorKeyArrowOverlay.Tick(_cachedPlayerTransform, keyBarParent);

        if (KeyInventory.Instance == null || KeyInventory.Instance.keys.Count == 0) return;
        bool hasAllKeys = KeyInventory.Instance.HasAllKeys();

        if (UsesDoorSequenceReveal && doorInRange && hasAllKeys && Input.GetKeyDown(SEQUENCE_REVEAL_KEY))
        {
            HidePopUp();
            if (_sequenceVRevealUsed)
            {
                ShowPopup("You already used the sequence hint.", 2.5f);
                return;
            }
            if (TryDoorSequenceRevealWithAttemptCost())
                _sequenceVRevealUsed = true;
            return;
        }

        if (!doorInRange || !hasAllKeys) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { NotifyTutorialKeyUsed(); TryUnlockWithKeyAtIndex(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { NotifyTutorialKeyUsed(); TryUnlockWithKeyAtIndex(1); }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { NotifyTutorialKeyUsed(); TryUnlockWithKeyAtIndex(2); }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { NotifyTutorialKeyUsed(); TryUnlockWithKeyAtIndex(3); }
    }

    void NotifyTutorialKeyUsed()
    {
        // Stop keybar shake immediately on key press
        if (vibrateCoroutine != null)
        {
            StopCoroutine(vibrateCoroutine);
            vibrateCoroutine = null;
            var rect = keyBarParent != null ? keyBarParent.GetComponent<RectTransform>() : null;
            if (rect != null) rect.anchoredPosition = keyBarBasePos;
        }

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnKeyUsedAtDoor();
    }

    void TryUnlockWithKeyAtIndex(int index)
    {
        var list = KeyInventory.Instance.keys;
        if (index >= 0 && index < list.Count)
            TryUnlockWithKey(list[index]);
    }

    public RectTransform GetFlyTarget()
    {
        if (spawnedButtons.Count > 0)
            return spawnedButtons[spawnedButtons.Count - 1].GetComponent<RectTransform>();
        return keyBarParent != null ? keyBarParent.GetComponent<RectTransform>() : null;
    }

    // Returns the RectTransform of the key button at the given 0-based index (K1=0, K2=1, ...)
    public RectTransform GetButtonAtIndex(int index)
    {
        if (index >= 0 && index < spawnedButtons.Count)
            return spawnedButtons[index].GetComponent<RectTransform>();
        return null;
    }

    public void SetDoorInRange(bool inRange)
    {
        doorInRange = inRange;
        UpdateButtonsInteractable();
    }

    public void Refresh()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (_sequenceRevealCoroutine != null)
        {
            StopCoroutine(_sequenceRevealCoroutine);
            _sequenceRevealCoroutine = null;
        }
        _sequenceShapeIcons.Clear();
        _buttonLabels.Clear();
        if (keysText != null)
            keysText.transform.SetParent(keyBarParent, true);

        for (int i = 0; i < spawnedButtons.Count; i++)
            Destroy(spawnedButtons[i].gameObject);

        spawnedButtons.Clear();

        bool hasKeys = KeyInventory.Instance != null && KeyInventory.Instance.keys.Count > 0;
        if (keyBarParent != null)
            keyBarParent.gameObject.SetActive(hasKeys);

        if (KeyInventory.Instance == null) return;

        if (UsesDoorSequenceReveal && !KeyInventory.Instance.HasAllKeys())
            _sequenceVRevealUsed = false;

        var list = KeyInventory.Instance.keys;

        for (int i = 0; i < list.Count; i++)
        {
            int idx = i;
            var kd = list[i];
            Button b = Instantiate(keyButtonTemplate, keyBarParent);
            b.gameObject.SetActive(true);
            TMP_Text t = b.GetComponentInChildren<TMP_Text>(true);
            if (SceneName == "Tutorial-1" || SceneName == "Level1" || SceneName == "Level2")
            {
                if (t != null)
                {
                    t.text = $"K{idx + 1}";
                    var labelRect = t.GetComponent<RectTransform>();
                    if (labelRect != null)
                    {
                        labelRect.anchorMin = new Vector2(0f, 0.8f);
                        labelRect.anchorMax = new Vector2(0.35f, 1f);
                        labelRect.offsetMin = new Vector2(2f, 0f);
                        labelRect.offsetMax = new Vector2(-2f, -2f);
                        t.alignment = TextAlignmentOptions.TopLeft;
                        t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                        t.fontSize = 18f;
                    }
                }
                _buttonLabels.Add(t);

                var shapeIconGO = new GameObject("ShapeIcon");
                shapeIconGO.transform.SetParent(b.transform, false);
                var shapeRect = shapeIconGO.AddComponent<RectTransform>();
                shapeRect.anchorMin = new Vector2(0f, 0f);
                shapeRect.anchorMax = new Vector2(1f, 0.75f);
                shapeRect.offsetMin = new Vector2(2f, 2f);
                shapeRect.offsetMax = new Vector2(-2f, 0f);

                var shapeImage = shapeIconGO.AddComponent<Image>();
                shapeImage.sprite = GetShapeSprite(kd.shape);
                shapeImage.color = new Color(1f, 0.84f, 0f);
                shapeImage.preserveAspect = true;

                if (UsesDoorSequenceReveal)
                    _sequenceShapeIcons.Add(shapeImage);
            }
            else
            {
                if (t != null)
                {
                    t.text = (idx + 1).ToString();
                    t.alignment = TextAlignmentOptions.Center;
                    t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                }
            }
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => TryUnlockWithKey(kd));
            spawnedButtons.Add(b);

            if (idx == list.Count - 1)
                flashCoroutine = StartCoroutine(FlashButton(b));
        }

        if (keysText != null)
        {
            int collected = KeyInventory.Instance != null ? KeyInventory.Instance.keys.Count : 0;
            int total = KeyInventory.Instance != null ? KeyInventory.Instance.requiredKeyCount : 0;
            TMP_Text keysLabel = keysText.GetComponentInChildren<TMP_Text>(true);
            if (keysLabel != null)
                keysLabel.text = $"Collected Keys: <color=#4a90d9><b>{collected}/{total}</b></color>";

            if (spawnedButtons.Count > 0)
            {
                keysText.SetActive(true);
                RectTransform firstRect = spawnedButtons[0].GetComponent<RectTransform>();
                RectTransform keysRect = keysText.GetComponent<RectTransform>();
                if (keysRect != null && firstRect != null)
                {
                    keysRect.SetParent(firstRect, false);
                    keysRect.anchorMin = new Vector2(0, 1);
                    keysRect.anchorMax = new Vector2(0, 1);
                    keysRect.pivot = new Vector2(0, 0);
                    keysRect.anchoredPosition = new Vector2(0, 4f);
                    keysRect.sizeDelta = new Vector2(280f, 24f);
                }
            }
        }

        UpdateButtonsInteractable();

        if (UsesDoorSequenceReveal && _sequenceShapeIcons.Count > 0)
        {
            _sequenceRevealCoroutine = StartCoroutine(HideKeySequenceAfterIntro());
            if (list.Count == 1)
                ShowKeySequenceCollectionPrompt();
        }
    }

    IEnumerator HideKeySequenceAfterIntro()
    {
        int last = _sequenceShapeIcons.Count - 1;
        for (int i = 0; i < _sequenceShapeIcons.Count; i++)
        {
            bool iconVisible = i == last;
            if (_sequenceShapeIcons[i] != null)
                _sequenceShapeIcons[i].gameObject.SetActive(iconVisible);
            if (i < _buttonLabels.Count && _buttonLabels[i] != null)
            {
                _buttonLabels[i].gameObject.SetActive(true);
                ApplyKeySequenceLabelStyle(_buttonLabels[i], iconVisible);
            }
        }

        yield return new WaitForSeconds(1f);

        if (last >= 0 && last < _sequenceShapeIcons.Count && _sequenceShapeIcons[last] != null)
            _sequenceShapeIcons[last].gameObject.SetActive(false);

        if (last >= 0 && last < _buttonLabels.Count)
        {
            if (_buttonLabels[last] != null)
            {
                _buttonLabels[last].gameObject.SetActive(true);
                ApplyKeySequenceLabelStyle(_buttonLabels[last], false);
            }
        }

        _sequenceRevealCoroutine = null;
    }

    void ShowKeySequenceCollectionPrompt()
    {
        ShowPopup("Remember the key shape sequence.", 3f);
    }

    void ShowPopup(string message, float duration)
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowPopup(message, duration);
            return;
        }

        EnsureHintPopupUI();
        if (_hintPopupText != null)
            _hintPopupText.text = message;
        if (_hintPopupRoot != null)
            _hintPopupRoot.SetActive(true);

        if (_hintPopupCoroutine != null)
        {
            StopCoroutine(_hintPopupCoroutine);
            _hintPopupCoroutine = null;
        }

        if (duration > 0f)
            _hintPopupCoroutine = StartCoroutine(HideHintPopupAfter(duration));
    }

    public void HidePopUp()
    {
        if (_hintPopupCoroutine != null)
        {
            StopCoroutine(_hintPopupCoroutine);
            _hintPopupCoroutine = null;
        }
        if (_hintPopupRoot != null)
            _hintPopupRoot.SetActive(false);
    }

    void EnsureHintPopupUI()
    {
        if (_hintPopupRoot != null) return;
        _hintPopupRoot = PopupMsgs.Create(out _hintPopupText, "LevelHintPopup");
    }

    IEnumerator HideHintPopupAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (_hintPopupRoot != null)
            _hintPopupRoot.SetActive(false);
        _hintPopupCoroutine = null;
    }

    void ApplyKeySequenceLabelStyle(TMP_Text label, bool iconVisible)
    {
        if (label == null) return;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        if (labelRect == null) return;

        if (iconVisible)
        {
            labelRect.anchorMin = new Vector2(0f, 0.8f);
            labelRect.anchorMax = new Vector2(0.35f, 1f);
            labelRect.offsetMin = new Vector2(2f, 0f);
            labelRect.offsetMax = new Vector2(-2f, -2f);
            label.alignment = TextAlignmentOptions.TopLeft;
            label.fontSize = 18f;
        }
        else
        {
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 20f;
        }
    }

    IEnumerator FlashButton(Button btn)
    {
        Image img = btn.GetComponent<Image>();
        Color originalColor = img != null ? img.color : Color.white;

        btn.transform.localScale = Vector3.one * 2.5f;
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float p = t / 0.4f;
            float bounce = 1f + 1.5f * (1f - p) * Mathf.Abs(Mathf.Sin(p * Mathf.PI * 3f));
            btn.transform.localScale = Vector3.one * bounce;
            yield return null;
        }
        btn.transform.localScale = Vector3.one;

        if (img != null)
        {
            for (int i = 0; i < 4; i++)
            {
                img.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                img.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            img.color = originalColor;
        }
        flashCoroutine = null;
    }

    Sprite GetShapeSprite(KeyHeadShape shape)
    {
        if (_generatedShapeSprites.TryGetValue(shape, out var cached))
            return cached;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        var clear = new Color(0, 0, 0, 0);
        var col = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color c = clear;
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;

                switch (shape)
                {
                    case KeyHeadShape.Circle:
                        if (nx * nx + ny * ny <= 0.8f * 0.8f) c = col;
                        break;
                    case KeyHeadShape.Square:
                        if (Mathf.Abs(nx) <= 0.75f && Mathf.Abs(ny) <= 0.75f) c = col;
                        break;
                    case KeyHeadShape.Capsule:
                        {
                            float rx = Mathf.Abs(nx);
                            float ry = Mathf.Abs(ny);
                            if (ry <= 0.4f && rx <= 0.7f) c = col; 
                            else
                            {
                                Vector2 leftCenter  = new Vector2(-0.7f, 0f);
                                Vector2 rightCenter = new Vector2( 0.7f, 0f);
                                if ((new Vector2(nx, ny) - leftCenter).sqrMagnitude  <= 0.4f * 0.4f ||
                                    (new Vector2(nx, ny) - rightCenter).sqrMagnitude <= 0.4f * 0.4f)
                                    c = col;
                            }
                        }
                        break;
                    case KeyHeadShape.Cross:
                        if (Mathf.Abs(nx) <= 0.2f && Mathf.Abs(ny) <= 0.8f) c = col;
                        if (Mathf.Abs(ny) <= 0.2f && Mathf.Abs(nx) <= 0.8f) c = col;
                        break;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        _generatedShapeSprites[shape] = sprite;
        return sprite;
    }

    void TryUnlockWithKey(KeyInventory.KeyData kd)
    {
        if (!doorInRange) return;

        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.HideTutorialPopup();

        bool correct = false;


        if (SceneName == "Tutorial-1")
        {
            correct = KeyGenerator.Instance != null
                && kd.shape == KeyGenerator.Instance.correctShape
                && kd.color == KeyGenerator.Instance.correctColor;
            if (correct && TutorialManager.Instance != null)
                TutorialManager.Instance.OnCorrectKeyUsedAtDoor();
            else
            {
                // once game manager is there should be cleaned up
                /*if (GameLayout.Instance != null)
                    GameLayout.Instance.ShowWrongKeyFeedback();
                else*/ if (TutorialManager.Instance != null)
                    TutorialManager.Instance.ShowPopup("Wrong key!", 2f);
            }
            return;
        }

        if (GameManager.Instance == null) return;

        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();


        if (SceneName == "Level1" || UsesDoorSequenceReveal)
        {
            correct = KeyGenerator.Instance != null
                   && kd.shape == KeyGenerator.Instance.correctShape
                   && kd.color == KeyGenerator.Instance.correctColor;
        }

        GameManager.Instance.RecordKeyAttempt();
        if (correct)
        {
            Debug.Log($"Correct key! {kd.color} {kd.shape}");

            if (SceneName == "Tutorial-1" && TutorialManager.Instance != null)
                TutorialManager.Instance.OnCorrectKeyUsedAtDoor();
            else if (SceneName == "Level1" || SceneName == "Level2" && UIManager.Instance != null)
            {
                HideKeybarPressArrow();
                UIManager.Instance.ShowLevelComplete();
            }
            else
                GameManager.Instance.LoadNextLane();
        }
        else
        {
            Debug.Log($"Wrong key: {kd.color} {kd.shape}");
            GameManager.Instance.RecordIncorrectKey();
            int max = 2;
            if (GameManager.Instance.keyAttemptCount < max)
                ShowPopup("Wrong key!", 2f);
            else
            {
                HideKeybarPressArrow();
                foreach (Button b in spawnedButtons)
                    b.interactable = false;
                GameManager.Instance.GameOver();
            }
        }
    }


    void UpdateButtonsInteractable()
    {
        bool all = (KeyInventory.Instance != null) && KeyInventory.Instance.HasAllKeys();
        bool canClick = doorInRange && all;
        KeyButtonsInteractable = canClick;
        for (int i = 0; i < spawnedButtons.Count; i++)
            spawnedButtons[i].interactable = canClick;

        if (SceneName == "Level1")
            UpdateKeybarPressArrowVisibility(canClick);
        else
            HideKeybarPressArrow();

        if (canClick && keyBarParent != null)
        {
            if (vibrateCoroutine == null)
                vibrateCoroutine = StartCoroutine(VibrateKeyBar());
        }
        else if (vibrateCoroutine != null)
        {
            StopCoroutine(vibrateCoroutine);
            vibrateCoroutine = null;
            var rect = keyBarParent.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = keyBarBasePos;
        }

        if (UsesDoorSequenceReveal)
            ShowDoorSequenceRevealHint(all, canClick);
    }

    void ShowDoorSequenceRevealHint(bool hasAllKeys, bool canClick)
    {
        if (!hasAllKeys)
            return;

        if (canClick)
        {
            if (!_sequenceVRevealUsed)
                ShowPopup("If you're stuck, press V to view sequence, costs 1 attempt", 0f);
        }
    }

    bool TryDoorSequenceRevealWithAttemptCost()
    {
        if (_sequenceRevealInProgress || GameManager.Instance == null || KeyInventory.Instance == null)
            return false;

        _sequenceRevealInProgress = true;
        GameManager.Instance.RecordKeyAttempt();

        int max = GameManager.Instance.GetMaxAttemptsForCurrentLane();
        if (GameManager.Instance.keyAttemptCount >= max)
        {
            foreach (Button b in spawnedButtons)
                b.interactable = false;
            _sequenceRevealInProgress = false;
            GameManager.Instance.GameOver();
            return false;
        }

        StartCoroutine(RevealKeySequenceAtDoor());
        return true;
    }

    IEnumerator RevealKeySequenceAtDoor()
    {
        for (int i = 0; i < _sequenceShapeIcons.Count; i++)
            if (_sequenceShapeIcons[i] != null)
                _sequenceShapeIcons[i].gameObject.SetActive(true);
        for (int i = 0; i < _buttonLabels.Count; i++)
            if (_buttonLabels[i] != null)
            {
                _buttonLabels[i].gameObject.SetActive(true);
                ApplyKeySequenceLabelStyle(_buttonLabels[i], true);
            }

        yield return new WaitForSeconds(0.9f);

        _sequenceRevealInProgress = false;
    }

    void UpdateKeybarPressArrowVisibility(bool canClick)
    {
        if (!canClick)
        {
            HideKeybarPressArrow();
            return;
        }

        if (_cachedPlayerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _cachedPlayerTransform = p.transform;
        }

        if (_cachedPlayerTransform == null) return;
        _doorKeyArrowOverlay.Show();
        _doorKeyArrowOverlay.Tick(_cachedPlayerTransform, keyBarParent);
    }

    void HideKeybarPressArrow()
    {
        _doorKeyArrowOverlay.Hide();
    }

    IEnumerator VibrateKeyBar()
    {
        var rect = keyBarParent.GetComponent<RectTransform>();
        if (rect == null) yield break;

        keyBarBasePos = rect.anchoredPosition;
        float amplitude = 3f;
        float speed = 25f;

        while (true)
        {
            float offset = Mathf.Sin(Time.unscaledTime * speed) * amplitude;
            rect.anchoredPosition = keyBarBasePos + new Vector2(offset, 0f);
            yield return null;
        }
    }

    public void HighlightButton(int index)
    {
        ClearHighlight();
        if (index < 0 || index >= spawnedButtons.Count) return;

        Button btn = spawnedButtons[index];

        _highlightOverlay = new GameObject("HighlightBorder");
        _highlightOverlay.transform.SetParent(btn.transform, false);
        _highlightOverlay.transform.SetAsFirstSibling();

        RectTransform rt = _highlightOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-5f, -5f);
        rt.offsetMax = new Vector2(5f, 5f);

        Image img = _highlightOverlay.AddComponent<Image>();
        img.color = new Color(1f, 0.84f, 0f, 0f);
        img.raycastTarget = false;

        _highlightCoroutine = StartCoroutine(PulseHighlight(img));
    }

    public void ClearHighlight()
    {
        if (_highlightCoroutine != null) { StopCoroutine(_highlightCoroutine); _highlightCoroutine = null; }
        if (_highlightOverlay != null) { Destroy(_highlightOverlay); _highlightOverlay = null; }
    }

    IEnumerator PulseHighlight(Image img)
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * 4f;
            float alpha = (Mathf.Sin(t) + 1f) * 0.5f;
            if (img != null)
                img.color = new Color(1f, 0.84f, 0f, alpha * 0.85f);
            yield return null;
        }
    }

    public void ShowForTutorial()
    {
        if (keyBarParent != null)
            keyBarParent.gameObject.SetActive(true);
 
        Refresh();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        if (_sequenceRevealCoroutine != null)
            StopCoroutine(_sequenceRevealCoroutine);
        if (_hintPopupCoroutine != null)
            StopCoroutine(_hintPopupCoroutine);
        if (_hintPopupRoot != null)
            Destroy(_hintPopupRoot);
        _doorKeyArrowOverlay.Dispose();
    }
}
