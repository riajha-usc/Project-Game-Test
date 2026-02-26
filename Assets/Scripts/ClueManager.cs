using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;
    public float clueDisplayDuration = 10f;
    private List<CollectedClue> collectedClues = new List<CollectedClue>();
    private string finalClueAnswer = "";
    public bool HasFinalClue { get; private set; } = false;
    private Canvas uiCanvas;
    private GameObject cluePanel;
    private GameObject noCluesObject;
    private GameObject cluePopup;
    private float cluePopupTimer = 0f;
    private TextMeshProUGUI headerText;
    private bool panelExpanded = false;
    [System.Serializable]
    public class CollectedClue
    {
        public string title;
        public string text;
        public bool isFinal;
        public CollectedClue(string title, string text, bool isFinal)
        {
            this.title = title;
            this.text = text;
            this.isFinal = isFinal;
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void Start()
    {
        EnsureEventSystem();
        BuildUI();
        RefreshClueList();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        panelExpanded = false;
        EnsureEventSystem();
        BuildUI();
        RefreshClueList();
    }
    void Update()
    {
        if (cluePopup != null && cluePopup.activeSelf)
        {
            cluePopupTimer -= Time.deltaTime;
            if (cluePopupTimer <= 0f)
                cluePopup.SetActive(false);
        }
    }
    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            System.Type uiInputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (uiInputModule != null)
                esGO.AddComponent(uiInputModule);
            else
                esGO.AddComponent<StandaloneInputModule>();
        }
    }
    public void CollectClue(string title, string text, bool isFinal)
    {
        foreach (var c in collectedClues)
        {
            if (c.title == title && c.text == text)
                return;
        }
        CollectedClue clue = new CollectedClue(title, text, isFinal);
        collectedClues.Add(clue);
        if (isFinal)
        {
            finalClueAnswer = text;
            HasFinalClue = true;
        }
        ShowCluePopup(title, text, isFinal);
        RefreshClueList();
    }
    public string GetFinalAnswer() { return finalClueAnswer; }
    public List<CollectedClue> GetClues() { return new List<CollectedClue>(collectedClues); }
    public int ClueCount => collectedClues.Count;
    private void BuildUI()
    {
        if (uiCanvas != null)
            Destroy(uiCanvas.gameObject);
        GameObject canvasGO = new GameObject("ClueCanvas");
        canvasGO.transform.SetParent(transform);
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        GameObject btnGO = new GameObject("ClueButton", typeof(RectTransform));
        btnGO.transform.SetParent(canvasGO.transform, false);
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-10, -10);
        btnRect.sizeDelta = new Vector2(180, 40);
        Image btnBg = btnGO.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        cb.pressedColor = new Color(0.4f, 0.4f, 0.1f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(TogglePanel);
        GameObject btnTextGO = new GameObject("BtnText", typeof(RectTransform));
        btnTextGO.transform.SetParent(btnGO.transform, false);
        headerText = btnTextGO.AddComponent<TextMeshProUGUI>();
        headerText.text = "Clues (0)";
        headerText.fontSize = 18;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = Color.white;
        headerText.raycastTarget = false;
        RectTransform btnTxtRect = btnTextGO.GetComponent<RectTransform>();
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;
        btnTxtRect.sizeDelta = Vector2.zero;
        cluePanel = new GameObject("CluePanel", typeof(RectTransform));
        cluePanel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = cluePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520, 420);
        Image panelBg = cluePanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        GameObject titleBar = new GameObject("TitleBar", typeof(RectTransform));
        titleBar.transform.SetParent(cluePanel.transform, false);
        RectTransform titleBarRect = titleBar.GetComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.pivot = new Vector2(0.5f, 1);
        titleBarRect.anchoredPosition = Vector2.zero;
        titleBarRect.sizeDelta = new Vector2(0, 40);
        Image titleBarBg = titleBar.AddComponent<Image>();
        titleBarBg.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        GameObject closeBtnGO = new GameObject("CloseBtn", typeof(RectTransform));
        closeBtnGO.transform.SetParent(titleBar.transform, false);
        RectTransform closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1, 0);
        closeBtnRect.anchorMax = new Vector2(1, 1);
        closeBtnRect.pivot = new Vector2(1, 0.5f);
        closeBtnRect.anchoredPosition = new Vector2(-5, 0);
        closeBtnRect.sizeDelta = new Vector2(60, 0);
        Image closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.6f, 0.15f, 0.15f, 1f);
        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBtnImg;
        closeBtn.onClick.AddListener(TogglePanel);
        GameObject closeTxtGO = new GameObject("X", typeof(RectTransform));
        closeTxtGO.transform.SetParent(closeBtnGO.transform, false);
        TextMeshProUGUI closeTMP = closeTxtGO.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 20;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;
        closeTMP.raycastTarget = false;
        RectTransform closeTxtRect = closeTxtGO.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.sizeDelta = Vector2.zero;
        GameObject titleText = new GameObject("TitleText", typeof(RectTransform));
        titleText.transform.SetParent(titleBar.transform, false);
        TextMeshProUGUI titleTMP = titleText.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Collected Clues";
        titleTMP.fontSize = 19;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.9f, 0.3f);
        titleTMP.raycastTarget = false;
        RectTransform titleTxtRect = titleText.GetComponent<RectTransform>();
        titleTxtRect.anchorMin = Vector2.zero;
        titleTxtRect.anchorMax = Vector2.one;
        titleTxtRect.offsetMax = new Vector2(-65, 0);
        titleTxtRect.sizeDelta = Vector2.zero;
        noCluesObject = new GameObject("NoCluesText", typeof(RectTransform));
        noCluesObject.transform.SetParent(cluePanel.transform, false);
        TextMeshProUGUI noCluesTMP = noCluesObject.AddComponent<TextMeshProUGUI>();
        noCluesTMP.text = "No clues collected yet.\n\nExplore the worlds and\nclick on glowing orbs\nto collect clues.";
        noCluesTMP.fontSize = 18;
        noCluesTMP.alignment = TextAlignmentOptions.Center;
        noCluesTMP.color = new Color(0.6f, 0.6f, 0.6f);
        noCluesTMP.raycastTarget = false;
        RectTransform noCluesRect = noCluesObject.GetComponent<RectTransform>();
        noCluesRect.anchorMin = new Vector2(0.05f, 0.1f);
        noCluesRect.anchorMax = new Vector2(0.95f, 0.8f);
        noCluesRect.sizeDelta = Vector2.zero;
        cluePanel.SetActive(false);
        cluePopup = new GameObject("CluePopup", typeof(RectTransform));
        cluePopup.transform.SetParent(canvasGO.transform, false);
        RectTransform popupRect = cluePopup.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = new Vector2(0, 150);
        popupRect.sizeDelta = new Vector2(500, 150);
        Image popupBg = cluePopup.AddComponent<Image>();
        popupBg.color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        Button popupBtn = cluePopup.AddComponent<Button>();
        popupBtn.targetGraphic = popupBg;
        ColorBlock pcb = popupBtn.colors;
        pcb.highlightedColor = new Color(0.15f, 0.15f, 0.15f, 0.92f);
        pcb.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.92f);
        popupBtn.colors = pcb;
        popupBtn.onClick.AddListener(DismissPopup);
        GameObject popupTitleGO = new GameObject("PopupTitle", typeof(RectTransform));
        popupTitleGO.transform.SetParent(cluePopup.transform, false);
        TextMeshProUGUI popupTitleTMP = popupTitleGO.AddComponent<TextMeshProUGUI>();
        popupTitleTMP.text = "";
        popupTitleTMP.fontSize = 20;
        popupTitleTMP.fontStyle = FontStyles.Bold;
        popupTitleTMP.alignment = TextAlignmentOptions.Center;
        popupTitleTMP.color = new Color(0.2f, 0.8f, 0.2f);
        popupTitleTMP.raycastTarget = false;
        RectTransform popupTitleRect = popupTitleGO.GetComponent<RectTransform>();
        popupTitleRect.anchorMin = new Vector2(0.05f, 0.6f);
        popupTitleRect.anchorMax = new Vector2(0.95f, 0.95f);
        popupTitleRect.sizeDelta = Vector2.zero;
        GameObject popupTextGO = new GameObject("PopupText", typeof(RectTransform));
        popupTextGO.transform.SetParent(cluePopup.transform, false);
        TextMeshProUGUI popupTextTMP = popupTextGO.AddComponent<TextMeshProUGUI>();
        popupTextTMP.text = "";
        popupTextTMP.fontSize = 22;
        popupTextTMP.alignment = TextAlignmentOptions.Center;
        popupTextTMP.color = Color.white;
        popupTextTMP.raycastTarget = false;
        RectTransform popupTextRect = popupTextGO.GetComponent<RectTransform>();
        popupTextRect.anchorMin = new Vector2(0.05f, 0.15f);
        popupTextRect.anchorMax = new Vector2(0.95f, 0.6f);
        popupTextRect.sizeDelta = Vector2.zero;
        GameObject dismissGO = new GameObject("DismissHint", typeof(RectTransform));
        dismissGO.transform.SetParent(cluePopup.transform, false);
        TextMeshProUGUI dismissTMP = dismissGO.AddComponent<TextMeshProUGUI>();
        dismissTMP.text = "(click to dismiss)";
        dismissTMP.fontSize = 13;
        dismissTMP.alignment = TextAlignmentOptions.Center;
        dismissTMP.color = new Color(0.5f, 0.5f, 0.5f);
        dismissTMP.raycastTarget = false;
        RectTransform dismissRect = dismissGO.GetComponent<RectTransform>();
        dismissRect.anchorMin = new Vector2(0.2f, 0.0f);
        dismissRect.anchorMax = new Vector2(0.8f, 0.15f);
        dismissRect.sizeDelta = Vector2.zero;
        cluePopup.SetActive(false);
    }
    private void TogglePanel()
    {
        panelExpanded = !panelExpanded;
        cluePanel.SetActive(panelExpanded);
    }
    private void DismissPopup()
    {
        if (cluePopup != null)
            cluePopup.SetActive(false);
    }
    private void ShowCluePopup(string title, string text, bool isFinal)
    {
        if (cluePopup == null) return;
        TextMeshProUGUI titleTMP = cluePopup.transform.Find("PopupTitle")?.GetComponent<TextMeshProUGUI>();
        if (titleTMP != null)
        {
            titleTMP.text = "Clue Collected: " + title;
            titleTMP.color = isFinal ? new Color(1f, 0.85f, 0.2f) : new Color(0.2f, 0.8f, 0.2f);
        }
        TextMeshProUGUI textTMP = cluePopup.transform.Find("PopupText")?.GetComponent<TextMeshProUGUI>();
        if (textTMP != null)
            textTMP.text = text;
        Image bg = cluePopup.GetComponent<Image>();
        if (bg != null)
            bg.color = isFinal ? new Color(0.2f, 0.15f, 0.0f, 0.95f) : new Color(0.1f, 0.1f, 0.1f, 0.92f);
        cluePopup.SetActive(true);
        cluePopupTimer = clueDisplayDuration;
        UpdateHeader();
    }
    private void UpdateHeader()
    {
        if (headerText != null)
            headerText.text = "Clues (" + collectedClues.Count + ")";
    }
    private void RefreshClueList()
    {
        if (cluePanel == null) return;
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in cluePanel.transform)
        {
            if (child.name.StartsWith("ClueEntry"))
                toDestroy.Add(child.gameObject);
        }
        foreach (var go in toDestroy)
            Destroy(go);
        if (noCluesObject != null)
            noCluesObject.SetActive(collectedClues.Count == 0);
        float yOffset = -50f; // Start below title bar
        float entryWidth = 480f;
        for (int i = 0; i < collectedClues.Count; i++)
        {
            var clue = collectedClues[i];
            GameObject entry = new GameObject("ClueEntry_" + i, typeof(RectTransform));
            entry.transform.SetParent(cluePanel.transform, false);
            RectTransform entryRect = entry.GetComponent<RectTransform>();
            entryRect.anchorMin = new Vector2(0.5f, 1);
            entryRect.anchorMax = new Vector2(0.5f, 1);
            entryRect.pivot = new Vector2(0.5f, 1);
            entryRect.anchoredPosition = new Vector2(0, yOffset);
            entryRect.sizeDelta = new Vector2(entryWidth, 70);
            Image entryBg = entry.AddComponent<Image>();
            entryBg.color = clue.isFinal
                ? new Color(0.5f, 0.4f, 0.05f, 0.7f)
                : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            GameObject titleGO = new GameObject("Title", typeof(RectTransform));
            titleGO.transform.SetParent(entry.transform, false);
            TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = clue.isFinal ? "* " + clue.title : clue.title;
            titleTMP.fontSize = 16;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = clue.isFinal ? new Color(1f, 0.85f, 0.2f) : Color.white;
            titleTMP.raycastTarget = false;
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(10, 0);
            titleRect.offsetMax = new Vector2(-10, -5);
            GameObject textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(entry.transform, false);
            TextMeshProUGUI textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.text = clue.text;
            textTMP.fontSize = 15;
            textTMP.color = new Color(1f, 1f, 1f, 0.9f);
            textTMP.raycastTarget = false;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.5f);
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, 0);
            yOffset -= 78f; // Move down for next entry
        }
        UpdateHeader();
    }
}