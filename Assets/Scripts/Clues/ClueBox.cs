using UnityEngine;
using TMPro;

public class ClueBox : MonoBehaviour
{
    public event System.Action OnClueBoxOpened;
    public string clueText         = "";
    public int    clueIndex        = 0;
    public float  interactionRange = 4.0f;

    private Transform  player;
    private bool       isPlayerNearby = false;
    private bool       isClueOpen     = false;
    private GameObject clueDisplayUI;
    private GameObject screenPromptObj;

    private static ClueBox currentlyOpenClue = null;
    private static Canvas  screenCanvas;

    private const float DISPLAY_HEIGHT_ABOVE_BOX = 0.8f;
    private const float DISPLAY_INSET_FROM_WALL  = 0.12f;

    void Start()
    {
        FindPlayer();
        EnsureScreenCanvas();
        CreateScreenPrompt();
    }

    void OnDestroy()
    {
        if (screenPromptObj != null) Destroy(screenPromptObj);
        if (clueDisplayUI   != null) Destroy(clueDisplayUI);
    }

    void Update()
    {
        if (player == null) { FindPlayer(); if (player == null) return; }

        float dist     = Vector3.Distance(transform.position, player.position);
        bool  wasNearby = isPlayerNearby;
        isPlayerNearby  = dist <= interactionRange;

        if       ( isPlayerNearby && !wasNearby)  ShowPrompt();
        else if  (!isPlayerNearby &&  wasNearby)  { HidePrompt(); if (isClueOpen) CloseClue(); }

        if (isPlayerNearby && !isClueOpen && Input.GetMouseButtonDown(0))
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, interactionRange + 10f))
                {
                    if (hit.collider != null &&
                        (hit.collider.transform == transform ||
                         hit.collider.transform.IsChildOf(transform)))
                        OpenClue();
                }
            }
        }
        else if (isClueOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            CloseClue();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private static void EnsureScreenCanvas()
    {
        if (screenCanvas != null) return;
        GameObject go = new GameObject("_ClueScreenCanvas");
        DontDestroyOnLoad(go);
        screenCanvas = go.AddComponent<Canvas>();
        screenCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        screenCanvas.sortingOrder = 20;
        go.AddComponent<UnityEngine.UI.CanvasScaler>();
        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    }

    private void CreateScreenPrompt()
    {
        screenPromptObj = new GameObject("ClueProximityPrompt_" + clueIndex);
        screenPromptObj.transform.SetParent(screenCanvas.transform, false);

        RectTransform rt = screenPromptObj.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.06f);
        rt.anchorMax        = new Vector2(0.5f, 0.06f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(460f, 56f);
        rt.anchoredPosition = Vector2.zero;

        var bg = screenPromptObj.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.06f, 0.05f, 0.04f, 0.82f);

        GameObject labelGO = new GameObject("TMPLabel");
        labelGO.transform.SetParent(screenPromptObj.transform, false);
        RectTransform lr = labelGO.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(14f, 5f); lr.offsetMax = new Vector2(-14f, -5f);

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text               = "<b>[ Click to read Clue " + (clueIndex + 1) + " ]</b>";
        tmp.fontSize           = 25;
        tmp.color              = Color.white;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.textWrappingMode   = TextWrappingModes.NoWrap;

        screenPromptObj.SetActive(false);
    }

    private void ShowPrompt() { if (screenPromptObj != null && !isClueOpen) screenPromptObj.SetActive(true); }
    private void HidePrompt() { if (screenPromptObj != null) screenPromptObj.SetActive(false); }

    private void OpenClue()
    {
        if (currentlyOpenClue != null && currentlyOpenClue != this)
            currentlyOpenClue.CloseClue();

        isClueOpen        = true;
        currentlyOpenClue = this;
        HidePrompt();

        OnClueBoxOpened?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.RecordClueSolved(clueIndex);

        if (GameLayout.Instance != null)
            GameLayout.Instance.Refresh();

        ShowCluePanel();
    }

    private void CloseClue()
    {
        isClueOpen = false;
        if (currentlyOpenClue == this) currentlyOpenClue = null;
        if (clueDisplayUI != null) { Destroy(clueDisplayUI); clueDisplayUI = null; }
        if (isPlayerNearby) ShowPrompt();
    }

    private void ShowCluePanel()
    {
        if (clueDisplayUI != null) Destroy(clueDisplayUI);

        clueDisplayUI = new GameObject("ClueDisplay_" + clueIndex);

        Vector3 cardPos = transform.position
            + Vector3.up * DISPLAY_HEIGHT_ABOVE_BOX
            + (-transform.forward) * DISPLAY_INSET_FROM_WALL;
        Quaternion cardRot = transform.rotation;

        clueDisplayUI.transform.position = cardPos;
        clueDisplayUI.transform.rotation = cardRot;
        clueDisplayUI.layer = 5;

        Canvas dc = clueDisplayUI.AddComponent<Canvas>();
        dc.renderMode   = RenderMode.WorldSpace;
        dc.sortingOrder = 11;

        RectTransform dr = clueDisplayUI.GetComponent<RectTransform>();
        dr.sizeDelta  = new Vector2(200f, 150f);
        dr.localScale = Vector3.one * 0.006f;

        clueDisplayUI.AddComponent<UnityEngine.UI.CanvasScaler>();

        MakeImage(clueDisplayUI.transform, "BG",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.95f, 0.90f, 0.80f, 0.97f));

        // Black header bar
        MakeImage(clueDisplayUI.transform, "HeaderBar",
            new Vector2(0f, 0.76f), new Vector2(1f, 1f),
            Vector2.zero, Vector2.zero,
            new Color(0.05f, 0.05f, 0.05f, 1f));

        // Header text (white on black)
        MakeTMP(clueDisplayUI.transform, "Header",
            new Vector2(0f, 0.76f), new Vector2(1f, 1f),
            new Vector2(8f, 0f), new Vector2(-8f, 0f),
            "CLUE " + (clueIndex + 1),
            22, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);

        // Clue body
        MakeTMP(clueDisplayUI.transform, "ClueText",
            new Vector2(0f, 0.18f), new Vector2(1f, 0.76f),
            new Vector2(18f, 0f),   new Vector2(-18f, 0f),
            "\"" + clueText + "\"",
            18, new Color(0.12f, 0.08f, 0.04f), TextAlignmentOptions.Center, FontStyles.Italic,
            wrap: true);

        // Close hint
        MakeTMP(clueDisplayUI.transform, "CloseHint",
            new Vector2(0f, 0f), new Vector2(1f, 0.18f),
            new Vector2(8f, 0f),  new Vector2(-8f, 0f),
            "[Right-click or ESC to close]",
            13, new Color(0.45f, 0.35f, 0.25f, 0.8f), TextAlignmentOptions.Center, FontStyles.Normal);
    }

    private void MakeImage(Transform parent, string n,
        Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color color)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent, false);
        go.layer = 5;
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
        go.AddComponent<UnityEngine.UI.Image>().color = color;
    }

    private void MakeTMP(Transform parent, string n,
        Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax,
        string text, float size, Color color,
        TextAlignmentOptions align, FontStyles style, bool wrap = false)
    {
        GameObject go = new GameObject(n);
        go.transform.SetParent(parent, false);
        go.layer = 5;
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = align;
        tmp.fontStyle = style;
        if (wrap) tmp.textWrappingMode = TextWrappingModes.Normal;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void SetInteractable(bool value)
    {
        enabled = value;
    }
}