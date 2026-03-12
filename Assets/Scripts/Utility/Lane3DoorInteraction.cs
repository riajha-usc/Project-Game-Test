using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Lane3DoorInteraction : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Interaction")]
    public float interactionDistance = 3.0f;

    [Header("Answer")]
    [Tooltip("Set this to the correct answer, or leave blank to pull from GameManager.")]
    public string correctAnswer = "Square White";   // e.g. "circle small"

    [Header("Optional — assign if you have a door mesh to animate")]
    public GameObject doorMesh;

    // ── Runtime refs ──────────────────────────────────────────────────────────
    private Transform player;

    // UI roots
    private Canvas     rootCanvas;
    private GameObject promptPanel;
    private GameObject inputPanel;
    private GameObject resultPanel;

    // Prompt
    private TextMeshProUGUI promptText;

    // Input panel
    private TMP_InputField answerInputField;

    // Result popup
    private Image           resultBG;
    private TextMeshProUGUI resultText;

    // State
    private bool isNearDoor      = false;
    private bool isInputOpen     = false;
    private bool isResultShowing = false;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        FindPlayer();
        BuildUI();

        if (GameManager.Instance != null && !string.IsNullOrWhiteSpace(GameManager.Instance.finalAnswer))
            correctAnswer = GameManager.Instance.finalAnswer;
    }

    void Update()
    {
        if (player == null) { FindPlayer(); return; }
        if (isResultShowing) return;   // freeze interaction while popup is up

        float dist = Vector3.Distance(transform.position, player.position);
        bool wasNear = isNearDoor;
        isNearDoor = dist <= interactionDistance;

        // Show / hide proximity prompt
        if (isNearDoor && !wasNear && !isInputOpen)   ShowPrompt();
        if (!isNearDoor && wasNear)                   { HidePrompt(); if (isInputOpen) CloseInputPanel(); }

        // Press Q to open input
        if (isNearDoor && !isInputOpen && Input.GetKeyDown(KeyCode.Q))
            OpenInputPanel();

        // ESC to close input
        if (isInputOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseInputPanel();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Proximity prompt
    // ─────────────────────────────────────────────────────────────────────────
    private void ShowPrompt()
    {
        if (promptPanel != null) promptPanel.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptPanel != null) promptPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Input panel
    // ─────────────────────────────────────────────────────────────────────────
    private void OpenInputPanel()
    {
        HidePrompt();
        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();
        isInputOpen = true;
        inputPanel.SetActive(true);
        answerInputField.text = "";
        answerInputField.ActivateInputField();

        // Lock cursor so player can type
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        Time.timeScale = 0f;
    }

    public void CloseInputPanel()
    {
        isInputOpen = false;
        inputPanel.SetActive(false);
        if (isNearDoor) ShowPrompt();

        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Answer check
    // ─────────────────────────────────────────────────────────────────────────
    private void OnSubmitAnswer()
    {
        string playerAnswer = answerInputField.text.Trim().ToLower();
        string expected     = correctAnswer.Trim().ToLower();
        Debug.Log("Player answer: " + playerAnswer);
        Debug.Log("Expected answer: " + expected);

        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();

        CloseInputPanel();

        if (GameManager.Instance != null)
            GameManager.Instance.RecordCodeAttempt();
        if (playerAnswer == expected)
            ShowResult(success: true);
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RecordIncorrectCode();
            int max = GameManager.Instance != null ? GameManager.Instance.GetMaxAttemptsForCurrentLane() : 2;
            if (GameManager.Instance != null && GameManager.Instance.incorrectCodeCount < max)
            {
                if (GameLayout.Instance != null)
                    GameLayout.Instance.ShowWrongCodeFeedback();
            }
            else
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Result popup
    // ─────────────────────────────────────────────────────────────────────────
    private void ShowResult(bool success)
    {
        isResultShowing = true;
        HidePrompt();

        if (success)
        {
            if (doorMesh != null) doorMesh.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.LoadNextLane();
            return;
        }

        resultPanel.SetActive(true);
        resultBG.color  = new Color(0.95f, 0.85f, 0.05f); 
        resultText.text = "Game Over!!";
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void OnRestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UI BUILDER — constructs all panels at runtime, no prefab needed
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        // ── Root canvas ───────────────────────────────────────────
        GameObject canvasGO = new GameObject("Lane3DoorCanvas");
        DontDestroyOnLoad(canvasGO);
        rootCanvas = canvasGO.AddComponent<Canvas>();
        rootCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 30;
        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        BuildPromptPanel(canvasGO.transform);
        BuildInputPanel(canvasGO.transform);
        BuildResultPanel(canvasGO.transform);

        // Start hidden
        promptPanel.SetActive(false);
        inputPanel.SetActive(false);
        resultPanel.SetActive(false);
    }

    // ── Prompt panel: "Press Q to enter Clue Answer" ─────────────
    private void BuildPromptPanel(Transform parent)
    {
        promptPanel = MakePanel(parent, "PromptPanel",
            new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f),
            new Vector2(500f, 52f));

        Image bg = promptPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.72f);

        promptText = MakeText(promptPanel.transform, "PromptText",
            "Press  Q  to enter Clue Answer",
            22, Color.white, FontStyles.Bold, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-16f, 0f));
    }

    // ── Input panel ───────────────────────────────────────────────
    private void BuildInputPanel(Transform parent)
    {
        // Darkened full-screen blocker
        GameObject blocker = MakePanel(parent, "InputBlocker",
            Vector2.zero, Vector2.one, Vector2.zero);
        blocker.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        inputPanel = blocker;

        // Centred card
        GameObject card = MakePanel(blocker.transform, "InputCard",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(540f, 280f));
        Image cardBG = card.AddComponent<Image>();
        cardBG.color = new Color(0.10f, 0.10f, 0.12f);
        AddOutline(cardBG, new Color(0.4f, 0.4f, 0.45f));

        // Title
        MakeText(card.transform, "Title",
            "Enter Clue Answer",
            26, Color.white, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 0.72f), new Vector2(1f, 1f),
            new Vector2(20f, 0f), new Vector2(-20f, 0f));

        // Divider
        GameObject div = new GameObject("Divider");
        div.transform.SetParent(card.transform, false);
        RectTransform dr = div.AddComponent<RectTransform>();
        dr.anchorMin = new Vector2(0.05f, 0.65f); dr.anchorMax = new Vector2(0.95f, 0.67f);
        dr.offsetMin = dr.offsetMax = Vector2.zero;
        div.AddComponent<Image>().color = new Color(0.35f, 0.35f, 0.40f);

        // Input field background
        GameObject ifBG = MakePanel(card.transform, "InputFieldBG",
            new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.62f), Vector2.zero);
        ifBG.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f);

        // TMP Input Field
        GameObject ifGO = new GameObject("AnswerInputField");
        ifGO.transform.SetParent(ifBG.transform, false);
        RectTransform ifRT = ifGO.AddComponent<RectTransform>();
        ifRT.anchorMin = Vector2.zero; ifRT.anchorMax = Vector2.one;
        ifRT.offsetMin = new Vector2(8f, 4f); ifRT.offsetMax = new Vector2(-8f, -4f);

        answerInputField = ifGO.AddComponent<TMP_InputField>();

        // Placeholder
        GameObject phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(ifGO.transform, false);
        RectTransform phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = new Vector2(4f, 2f); phRT.offsetMax = new Vector2(-4f, -2f);
        TextMeshProUGUI phTMP = phGO.AddComponent<TextMeshProUGUI>();
        phTMP.text      = "Type your answer here…";
        phTMP.fontSize  = 18;
        phTMP.color     = new Color(0.55f, 0.55f, 0.60f);
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Text
        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(ifGO.transform, false);
        RectTransform txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(4f, 2f); txtRT.offsetMax = new Vector2(-4f, -2f);
        TextMeshProUGUI txtTMP = txtGO.AddComponent<TextMeshProUGUI>();
        txtTMP.fontSize  = 20;
        txtTMP.color     = Color.white;
        txtTMP.alignment = TextAlignmentOptions.MidlineLeft;

        answerInputField.textComponent   = txtTMP;
        answerInputField.placeholder      = phTMP;
        answerInputField.caretWidth       = 2;
        answerInputField.caretColor       = Color.white;

        // Hint label
        MakeText(card.transform, "HintLabel",
            "Hint: colour + shape (e.g. \"white square\")",
            14, new Color(0.65f, 0.65f, 0.70f), FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0f, 0.22f), new Vector2(1f, 0.38f),
            new Vector2(20f, 0f), new Vector2(-20f, 0f));

        // Submit button
        GameObject submitBtn = MakeButton(card.transform, "SubmitBtn",
            "Submit Answer",
            new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.20f),
            new Color(0.10f, 0.55f, 0.85f), Color.white, 20);
        submitBtn.GetComponent<Button>().onClick.AddListener(OnSubmitAnswer);

        // Allow Enter key to submit
        answerInputField.onSubmit.AddListener(_ => OnSubmitAnswer());
    }

    // ── Result popup ──────────────────────────────────────────────
    private void BuildResultPanel(Transform parent)
    {
        // Full-screen dimmer
        GameObject dimmer = MakePanel(parent, "ResultBlocker",
            Vector2.zero, Vector2.one, Vector2.zero);
        dimmer.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
        resultPanel = dimmer;

        // Centred popup card
        GameObject card = MakePanel(dimmer.transform, "ResultCard",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(520f, 280f));
        resultBG = card.AddComponent<Image>();   // colour set at runtime
        AddOutline(resultBG, new Color(1f, 1f, 1f, 0.25f));

        // Result message
        resultText = MakeText(card.transform, "ResultText",
            "",   // set at runtime
            34, Color.white, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0f, 0.05f), new Vector2(1f, 0.95f),
            new Vector2(24f, 0f), new Vector2(-24f, 0f));

        // No restart button — game ends, popup stays up.
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UI Helper methods
    // ─────────────────────────────────────────────────────────────────────────

    /// Creates an anchored RectTransform panel with optional fixed sizeDelta.
    private GameObject MakePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 sizeDelta = default)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        if (sizeDelta != default) rt.sizeDelta = sizeDelta;
        return go;
    }

    // TMP version — always visible, no font asset needed
    private TextMeshProUGUI MakeText(Transform parent, string name, string content,
        int fontSize, Color color, FontStyles style, TextAlignmentOptions anchor,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text               = content;
        tmp.fontSize           = fontSize;
        tmp.color              = color;
        tmp.fontStyle          = style;
        tmp.alignment          = anchor;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode       = TextOverflowModes.Overflow;
        return tmp;
    }

    private GameObject MakeButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor, Color textColor, int fontSize)
    {
        GameObject go = MakePanel(parent, name, anchorMin, anchorMax);
        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(
            Mathf.Clamp01(bgColor.r + 0.15f),
            Mathf.Clamp01(bgColor.g + 0.15f),
            Mathf.Clamp01(bgColor.b + 0.15f));
        cb.pressedColor = new Color(
            Mathf.Clamp01(bgColor.r - 0.15f),
            Mathf.Clamp01(bgColor.g - 0.15f),
            Mathf.Clamp01(bgColor.b - 0.15f));
        btn.colors = cb;
        btn.targetGraphic = img;

        // TMP label on button
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        RectTransform lr = labelGO.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(8f, 4f); lr.offsetMax = new Vector2(-8f, -4f);
        TextMeshProUGUI t = labelGO.AddComponent<TextMeshProUGUI>();
        t.text               = label;
        t.fontSize           = fontSize;
        t.color              = textColor;
        t.fontStyle          = FontStyles.Bold;
        t.alignment          = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;

        return go;
    }

    private void AddOutline(Graphic target, Color color)
    {
        Outline o = target.gameObject.AddComponent<Outline>();
        o.effectColor    = color;
        o.effectDistance = new Vector2(2f, -2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}