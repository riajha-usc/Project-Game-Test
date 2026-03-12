using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyInventoryUI : MonoBehaviour
{
    public static KeyInventoryUI Instance { get; private set; }
    public Transform keyBarParent;
    public Button keyButtonTemplate;
    public GameObject keysText;
    public GameObject openPromptUI;
    List<Button> spawnedButtons = new List<Button>();
    bool doorInRange = false;
    Coroutine flashCoroutine;
    Coroutine vibrateCoroutine;
    Vector2 keyBarBasePos;

    [Header("Optional: Key Shape Sprites (for Tutorial-1)")]
    public Sprite circleSprite;
    public Sprite squareSprite;
    public Sprite capsuleSprite;
    public Sprite crossSprite;

    public string sceneName;

    Dictionary<KeyHeadShape, Sprite> _generatedShapeSprites = new Dictionary<KeyHeadShape, Sprite>();

    void Awake()
    {
        Instance = this;
        if (openPromptUI != null) openPromptUI.SetActive(false);
        if (keyBarParent != null)
            keyBarParent.gameObject.SetActive(false);
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        if (KeyInventory.Instance == null || KeyInventory.Instance.keys.Count == 0) return;
        if (!doorInRange || !KeyInventory.Instance.HasAllKeys()) return;

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

    void UpdateOpenPrompt()
    {
        if (openPromptUI == null) return;
        bool hasAllKeys = KeyInventory.Instance != null && KeyInventory.Instance.HasAllKeys();
        openPromptUI.SetActive(doorInRange && hasAllKeys);
    }

    public RectTransform GetFlyTarget()
    {
        if (spawnedButtons.Count > 0)
            return spawnedButtons[spawnedButtons.Count - 1].GetComponent<RectTransform>();
        return keyBarParent != null ? keyBarParent.GetComponent<RectTransform>() : null;
    }

    public void SetDoorInRange(bool inRange)
    {
        doorInRange = inRange;
        UpdateButtonsInteractable();
        UpdateOpenPrompt();
    }

    public void Refresh()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (keysText != null)
            keysText.transform.SetParent(keyBarParent, true);

        for (int i = 0; i < spawnedButtons.Count; i++)
            Destroy(spawnedButtons[i].gameObject);

        spawnedButtons.Clear();

        bool hasKeys = KeyInventory.Instance != null && KeyInventory.Instance.keys.Count > 0;
        if (keyBarParent != null)
            keyBarParent.gameObject.SetActive(hasKeys);

        if (KeyInventory.Instance == null) return;

        var list = KeyInventory.Instance.keys;

        for (int i = 0; i < list.Count; i++)
        {
            int idx = i;
            var kd = list[i];
            Button b = Instantiate(keyButtonTemplate, keyBarParent);
            b.gameObject.SetActive(true);
            TMP_Text t = b.GetComponentInChildren<TMP_Text>(true);
            if (sceneName == "Tutorial-1")
            {
                if (t != null)
                {
                    t.text = $"K{idx + 1}";
                    var labelRect = t.GetComponent<RectTransform>();
                    if (labelRect != null)
                    {
                        // Small badge in the north‑west corner
                        labelRect.anchorMin = new Vector2(0f, 0.8f);
                        labelRect.anchorMax = new Vector2(0.35f, 1f);
                        labelRect.offsetMin = new Vector2(2f, 0f);
                        labelRect.offsetMax = new Vector2(-2f, -2f);
                        t.alignment = TextAlignmentOptions.TopLeft;
                        t.enableWordWrapping = false;
                        // Slightly larger K1/K2 labels for readability
                        t.fontSize = 18f;
                    }
                }

                var shapeIconGO = new GameObject("ShapeIcon");
                shapeIconGO.transform.SetParent(b.transform, false);
                var shapeRect = shapeIconGO.AddComponent<RectTransform>();
                // Fill most of the button under the K label
                shapeRect.anchorMin = new Vector2(0f, 0f);
                shapeRect.anchorMax = new Vector2(1f, 0.75f);
                shapeRect.offsetMin = new Vector2(2f, 2f);
                shapeRect.offsetMax = new Vector2(-2f, 0f);

                var shapeImage = shapeIconGO.AddComponent<Image>();
                shapeImage.sprite = GetShapeSprite(kd.shape);
                // Fill with tutorial yellow; sprite is white shape on top
                shapeImage.color = new Color(1f, 0.84f, 0f);
                shapeImage.preserveAspect = true;
            }
            else
            {
                // Normal lanes: just show 1,2,3,4 as before
                if (t != null)
                {
                    t.text = (idx + 1).ToString();
                    t.alignment = TextAlignmentOptions.Center;
                    t.enableWordWrapping = false;
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
        UpdateOpenPrompt();
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
        // If sprites are wired in the inspector, prefer those.
        switch (shape)
        {
            case KeyHeadShape.Circle:  if (circleSprite  != null) return circleSprite;  break;
            case KeyHeadShape.Square:  if (squareSprite  != null) return squareSprite;  break;
            case KeyHeadShape.Capsule: if (capsuleSprite != null) return capsuleSprite; break;
            case KeyHeadShape.Cross:   if (crossSprite   != null) return crossSprite;   break;
        }

        // Otherwise, generate simple procedural sprites in code (cached).
        if (_generatedShapeSprites.TryGetValue(shape, out var cached))
            return cached;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        // Base: transparent
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
                            if (ry <= 0.4f && rx <= 0.7f) c = col; // center bar
                            else
                            {
                                // rounded ends
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

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (GameManager.Instance == null) return;

        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();

        bool correct = false;

        Debug.Log("Key shape: " + kd.shape.ToString());
        Debug.Log("Key color: " + kd.color.ToString());
        Debug.Log("Key spinning: " + kd.spinning);
        Debug.Log("Scene name: " + sceneName);

        if (sceneName == "Level1-Lane1")
        {
            Debug.Log("Correct shape: " + GameManager.Instance.lane1CorrectShape);
            Debug.Log("Correct color: " + GameManager.Instance.lane1CorrectColor);
            correct = kd.shape.ToString() == GameManager.Instance.lane1CorrectShape
                   && kd.color.ToString() == GameManager.Instance.lane1CorrectColor;
        }
        else if (sceneName == "Level1-Lane2")
        {
            Debug.Log("Correct shape: " + GameManager.Instance.lane2CorrectShape);
            Debug.Log("Correct color: " + GameManager.Instance.lane2CorrectColor);
            Debug.Log("Correct spinning: " + GameManager.Instance.lane2CorrectKeySpinning);
            correct = kd.shape.ToString() == GameManager.Instance.lane2CorrectShape
                   && kd.color.ToString() == GameManager.Instance.lane2CorrectColor
                   && kd.spinning == GameManager.Instance.lane2CorrectKeySpinning;
        }

        GameManager.Instance.RecordKeyAttempt();
        if (correct)
        {
            Debug.Log($"Correct key! {kd.color} {kd.shape}");
            if (openPromptUI != null)
                openPromptUI.SetActive(false);
            GameManager.Instance.LoadNextLane();
        }
        else
        {
            Debug.Log($"Wrong key: {kd.color} {kd.shape}");
            GameManager.Instance.RecordIncorrectKey();
            int max = GameManager.Instance.GetMaxAttemptsForCurrentLane();
            if (GameManager.Instance.incorrectKeyCount < max)
            {
                if (GameLayout.Instance != null)
                    GameLayout.Instance.ShowWrongKeyFeedback();
            }
            else
            {
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
        for (int i = 0; i < spawnedButtons.Count; i++)
            spawnedButtons[i].interactable = canClick;

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
}
