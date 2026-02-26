using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FinalDoor : MonoBehaviour
{
    public string sceneToLoad = "Dark";
    public string spawnID = "";
    public string correctAnswer = "secret";
    public int requiredClueCount = 4;
    public string promptMessage = "Enter the answer to open the door:";
    public string wrongAnswerMessage = "That's not the right answer. Check your clues again!";
    public string correctAnswerMessage = "Door Unlocked!";
    private bool playerInRange = false;
    private bool inputActive = false;
    private bool hasAutoOpened = false;
    private GameObject inputUI;
    private TMP_InputField inputField;
    private TextMeshProUGUI feedbackText;
    private Canvas uiCanvas;
    private GameObject hintCanvas;
    private TextMeshProUGUI hintText;
    private GameObject lockedCanvas;
    private float lockedTimer = 0f;
    private GameObject myEventSystem;
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    private bool justOpened = false;
    void Update()
    {
        if (justOpened)
        {
            justOpened = false;
            return;
        }
        if (playerInRange && !inputActive && !hasAutoOpened)
        {
            if (ClueManager.Instance != null &&
                requiredClueCount > 0 &&
                ClueManager.Instance.ClueCount >= requiredClueCount)
            {
                hasAutoOpened = true;
                HideHint();
                HideLockedMessage();
                ShowInputUI();
                return;
            }
        }
        if (inputActive && Input.GetKeyDown(KeyCode.Q))
        {
            CloseInputUI();
            return;
        }
        if (lockedCanvas != null)
        {
            lockedTimer -= Time.deltaTime;
            if (lockedTimer <= 0f)
            {
                Destroy(lockedCanvas);
                lockedCanvas = null;
            }
        }
        if (inputActive)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (inputField != null && !inputField.isFocused)
            {
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject);
                inputField.ActivateInputField();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (ClueManager.Instance != null &&
                requiredClueCount > 0 &&
                ClueManager.Instance.ClueCount < requiredClueCount)
            {
                ShowLockedMessage();
                ShowHint();
            }
            else if (!inputActive)
            {
                hasAutoOpened = true;
                ShowInputUI();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            hasAutoOpened = false;
            HideHint();
            CloseInputUI();
            HideLockedMessage();
        }
    }
    private void ShowHint()
    {
        if (hintCanvas != null) return;
        int collected = ClueManager.Instance != null ? ClueManager.Instance.ClueCount : 0;
        int required = requiredClueCount;
        GameObject canvasGO = new GameObject("HintCanvas");
        hintCanvas = canvasGO;
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 90;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        GameObject bgGO = new GameObject("HintBG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0);
        bgRect.anchorMax = new Vector2(0.5f, 0);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.anchoredPosition = new Vector2(0, 50);
        bgRect.sizeDelta = new Vector2(500, 50);
        GameObject textGO = new GameObject("HintText", typeof(RectTransform));
        textGO.transform.SetParent(bgGO.transform, false);
        hintText = textGO.AddComponent<TextMeshProUGUI>();
        hintText.text = "Collect all clues to unlock! (" + collected + " / " + required + ")";
        hintText.fontSize = 22;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(1f, 0.5f, 0.3f);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
    private void HideHint()
    {
        if (hintCanvas != null)
        {
            Destroy(hintCanvas);
            hintCanvas = null;
            hintText = null;
        }
    }
    private void ShowLockedMessage()
    {
        if (lockedCanvas != null)
            Destroy(lockedCanvas);
        int collected = ClueManager.Instance != null ? ClueManager.Instance.ClueCount : 0;
        int required = requiredClueCount;
        GameObject canvasGO = new GameObject("LockedCanvas");
        lockedCanvas = canvasGO;
        Canvas c = canvasGO.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 150;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        GameObject bgGO = new GameObject("LockedBG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bg = bgGO.AddComponent<Image>();
        bg.color = new Color(0.5f, 0.1f, 0.1f, 0.85f);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(550, 80);
        GameObject textGO = new GameObject("LockedText", typeof(RectTransform));
        textGO.transform.SetParent(bgGO.transform, false);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Door is locked!\nYou have collected " + collected + " / " + required + " clues.";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        lockedTimer = 3f;
    }
    private void HideLockedMessage()
    {
        if (lockedCanvas != null)
        {
            Destroy(lockedCanvas);
            lockedCanvas = null;
        }
    }
    private void ShowInputUI()
    {
        if (inputUI != null) return;
        inputActive = true;
        justOpened = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisablePlayerControls();
        EnsureEventSystem();
        GameObject canvasGO = new GameObject("FinalDoorCanvas");
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 200;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        GameObject overlay = new GameObject("Overlay", typeof(RectTransform));
        overlay.transform.SetParent(canvasGO.transform, false);
        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.6f);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        inputUI = new GameObject("InputPanel", typeof(RectTransform));
        inputUI.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = inputUI.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(500, 280);
        Image panelImg = inputUI.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        GameObject promptGO = new GameObject("Prompt", typeof(RectTransform));
        promptGO.transform.SetParent(inputUI.transform, false);
        TextMeshProUGUI promptTMP = promptGO.AddComponent<TextMeshProUGUI>();
        promptTMP.text = promptMessage;
        promptTMP.fontSize = 22;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.color = Color.white;
        promptTMP.raycastTarget = false;
        RectTransform promptRect = promptGO.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0, 0.78f);
        promptRect.anchorMax = new Vector2(1, 0.95f);
        promptRect.offsetMin = new Vector2(20, 0);
        promptRect.offsetMax = new Vector2(-20, 0);
        GameObject closeHintGO = new GameObject("CloseHint", typeof(RectTransform));
        closeHintGO.transform.SetParent(inputUI.transform, false);
        TextMeshProUGUI closeHintTMP = closeHintGO.AddComponent<TextMeshProUGUI>();
        closeHintTMP.text = "(Press Q to close)";
        closeHintTMP.fontSize = 14;
        closeHintTMP.alignment = TextAlignmentOptions.Center;
        closeHintTMP.color = new Color(0.6f, 0.6f, 0.6f);
        closeHintTMP.raycastTarget = false;
        RectTransform closeHintRect = closeHintGO.GetComponent<RectTransform>();
        closeHintRect.anchorMin = new Vector2(0, 0.68f);
        closeHintRect.anchorMax = new Vector2(1, 0.78f);
        closeHintRect.sizeDelta = Vector2.zero;
        GameObject inputGO = new GameObject("InputField", typeof(RectTransform));
        inputGO.transform.SetParent(inputUI.transform, false);
        RectTransform inputRect = inputGO.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.1f, 0.42f);
        inputRect.anchorMax = new Vector2(0.9f, 0.62f);
        inputRect.sizeDelta = Vector2.zero;
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        inputField = inputGO.AddComponent<TMP_InputField>();
        inputField.characterLimit = 50;
        GameObject textAreaGO = new GameObject("TextArea", typeof(RectTransform));
        textAreaGO.transform.SetParent(inputGO.transform, false);
        RectTransform textAreaRect = textAreaGO.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 2);
        textAreaRect.offsetMax = new Vector2(-10, -2);
        textAreaGO.AddComponent<RectMask2D>();
        GameObject inputTextGO = new GameObject("Text", typeof(RectTransform));
        inputTextGO.transform.SetParent(textAreaGO.transform, false);
        TextMeshProUGUI inputTMP = inputTextGO.AddComponent<TextMeshProUGUI>();
        inputTMP.fontSize = 22;
        inputTMP.color = Color.white;
        inputTMP.richText = false;
        RectTransform inputTMPRect = inputTextGO.GetComponent<RectTransform>();
        inputTMPRect.anchorMin = Vector2.zero;
        inputTMPRect.anchorMax = Vector2.one;
        inputTMPRect.sizeDelta = Vector2.zero;
        GameObject placeholderGO = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGO.transform.SetParent(textAreaGO.transform, false);
        TextMeshProUGUI placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholderTMP.text = "Type your answer here...";
        placeholderTMP.fontSize = 22;
        placeholderTMP.fontStyle = FontStyles.Italic;
        placeholderTMP.color = new Color(0.5f, 0.5f, 0.5f);
        RectTransform phRect = placeholderGO.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.sizeDelta = Vector2.zero;
        inputField.textViewport = textAreaRect;
        inputField.textComponent = inputTMP;
        inputField.placeholder = placeholderTMP;
        inputField.fontAsset = inputTMP.font;
        GameObject btnGO = new GameObject("SubmitBtn", typeof(RectTransform));
        btnGO.transform.SetParent(inputUI.transform, false);
        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.3f, 0.1f);
        btnRect.anchorMax = new Vector2(0.7f, 0.3f);
        btnRect.sizeDelta = Vector2.zero;
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.5f, 0.2f, 1f);
        Button submitBtn = btnGO.AddComponent<Button>();
        submitBtn.targetGraphic = btnImg;
        submitBtn.onClick.AddListener(OnSubmitAnswer);
        GameObject btnTextGO = new GameObject("BtnText", typeof(RectTransform));
        btnTextGO.transform.SetParent(btnGO.transform, false);
        TextMeshProUGUI btnTMP = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTMP.text = "Submit";
        btnTMP.fontSize = 22;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;
        btnTMP.raycastTarget = false;
        RectTransform btnTxtRect = btnTextGO.GetComponent<RectTransform>();
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;
        btnTxtRect.sizeDelta = Vector2.zero;
        GameObject fbGO = new GameObject("Feedback", typeof(RectTransform));
        fbGO.transform.SetParent(canvasGO.transform, false);
        feedbackText = fbGO.AddComponent<TextMeshProUGUI>();
        feedbackText.fontSize = 20;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.color = new Color(1f, 0.3f, 0.3f);
        feedbackText.text = "";
        feedbackText.raycastTarget = false;
        RectTransform fbRect = fbGO.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0.2f, 0.15f);
        fbRect.anchorMax = new Vector2(0.8f, 0.22f);
        fbRect.sizeDelta = Vector2.zero;
        inputField.onSubmit.AddListener(delegate { OnSubmitAnswer(); });
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(inputGO);
        inputField.Select();
        inputField.ActivateInputField();
    }
    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            myEventSystem = esGO;
            esGO.AddComponent<EventSystem>();
            System.Type uiInputModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (uiInputModule != null)
                esGO.AddComponent(uiInputModule);
            else
                esGO.AddComponent<StandaloneInputModule>();
        }
    }
    private void DisablePlayerControls()
    {
        disabledScripts.Clear();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>())
            {
                string typeName = mb.GetType().Name;
                if (typeName == "FinalDoor" || typeName == "ClueManager" ||
                    typeName == "ClueItem" || typeName == "ClueVisual" ||
                    typeName == "EventSystem" || typeName == "Animator")
                    continue;
                if (mb.enabled)
                {
                    mb.enabled = false;
                    disabledScripts.Add(mb);
                }
            }
        }
        Camera cam = Camera.main;
        if (cam != null)
        {
            foreach (var mb in cam.GetComponentsInChildren<MonoBehaviour>())
            {
                if (disabledScripts.Contains(mb)) continue;
                string typeName = mb.GetType().Name;
                if (typeName == "FinalDoor" || typeName == "ClueManager" ||
                    typeName == "ClueItem" || typeName == "ClueVisual" ||
                    typeName == "EventSystem" || typeName == "Animator")
                    continue;
                if (mb.enabled)
                {
                    mb.enabled = false;
                    disabledScripts.Add(mb);
                }
            }
            if (cam.transform.parent != null)
            {
                foreach (var mb in cam.transform.parent.GetComponents<MonoBehaviour>())
                {
                    if (disabledScripts.Contains(mb)) continue;
                    string typeName = mb.GetType().Name;
                    if (typeName == "FinalDoor" || typeName == "ClueManager" ||
                        typeName == "ClueItem" || typeName == "ClueVisual" ||
                        typeName == "EventSystem" || typeName == "Animator")
                        continue;
                    if (mb.enabled)
                    {
                        mb.enabled = false;
                        disabledScripts.Add(mb);
                    }
                }
            }
        }
    }
    private void EnablePlayerControls()
    {
        foreach (var mb in disabledScripts)
        {
            if (mb != null)
                mb.enabled = true;
        }
        disabledScripts.Clear();
    }
    private void OnSubmitAnswer()
    {
        if (inputField == null) return;
        string answer = inputField.text.Trim();
        if (string.Equals(answer, correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            if (feedbackText != null)
            {
                feedbackText.color = new Color(0.2f, 1f, 0.2f);
                feedbackText.text = correctAnswerMessage;
            }
            Invoke(nameof(LoadFinalScene), 1.5f);
        }
        else
        {
            if (feedbackText != null)
            {
                feedbackText.color = new Color(1f, 0.3f, 0.3f);
                feedbackText.text = wrongAnswerMessage;
            }
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }
    private void LoadFinalScene()
    {
        CloseInputUI();
        if (!string.IsNullOrEmpty(spawnID) && GameManager.Instance != null)
            GameManager.Instance.targetSpawnID = spawnID;
        SceneManager.LoadScene(sceneToLoad);
    }
    private void CloseInputUI()
    {
        inputActive = false;
        EnablePlayerControls();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (uiCanvas != null)
            Destroy(uiCanvas.gameObject);
        if (myEventSystem != null)
            Destroy(myEventSystem);
        inputUI = null;
        inputField = null;
        feedbackText = null;
        myEventSystem = null;
        if (playerInRange)
            ShowHint();
    }
}