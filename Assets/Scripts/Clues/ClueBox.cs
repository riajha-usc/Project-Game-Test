using System;
using UnityEngine;
using TMPro;

public class ClueBox : MonoBehaviour
{
    public string clueText         = "";
    public int    clueIndex        = 0;
    public float  interactionRange = 1.8f;

    public event Action OnClueOpenedEvent;

    private Transform  player;
    private bool       isPlayerNearby = false;
    private GameObject clueDisplayUI;

    private static ClueBox currentlyOpenClue = null;

    private const float DISPLAY_HEIGHT_ABOVE_BOX = 0.8f;
    private const float DISPLAY_INSET_FROM_WALL  = 0.12f;

    void Start()
    {
        FindPlayer();
    }

    void OnDestroy()
    {
        if (clueDisplayUI != null) Destroy(clueDisplayUI);
    }

    void Update()
    {
        if (player == null) { FindPlayer(); if (player == null) return; }

        float dist    = Vector3.Distance(transform.position, player.position);
        bool  inRange = dist <= interactionRange;

        bool isFacing = false;
        if (inRange)
        {
            Vector3 dirToClue = (transform.position - player.position).normalized;
            isFacing = Vector3.Dot(player.forward, dirToClue) > 0.4f;
        }

        bool wasNearby = isPlayerNearby;
        isPlayerNearby = inRange && isFacing;

        if (isPlayerNearby && !wasNearby) OpenClue();
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void OpenClue()
    {
        if (currentlyOpenClue != null && currentlyOpenClue != this)
            currentlyOpenClue.CloseClue();

        currentlyOpenClue = this;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RecordClueZoneEntry();
            GameManager.Instance.RecordClueSolved(clueIndex);
        }

        if (GameLayout.Instance != null)
            GameLayout.Instance.Refresh();

        ShowCluePanel();

        OnClueOpenedEvent?.Invoke();
    }

    private void CloseClue()
    {
        if (currentlyOpenClue == this) currentlyOpenClue = null;
        if (clueDisplayUI != null) { Destroy(clueDisplayUI); clueDisplayUI = null; }
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
        dc.worldCamera  = Camera.main;

        RectTransform dr = clueDisplayUI.GetComponent<RectTransform>();
        dr.sizeDelta  = new Vector2(220f, 170f);
        dr.localScale = Vector3.one * 0.006f;

        clueDisplayUI.AddComponent<UnityEngine.UI.CanvasScaler>();

        Color sepia     = new Color(0.28f, 0.17f, 0.06f);
        Color parchment = new Color(0.86f, 0.81f, 0.68f, 0.92f);
        Color inkBrown  = new Color(0.18f, 0.10f, 0.04f);

        MakeImage(clueDisplayUI.transform, "OuterBorder",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            new Color(0.28f, 0.17f, 0.06f, 0.9f));

        MakeImage(clueDisplayUI.transform, "BG",
            Vector2.zero, Vector2.one,
            new Vector2(3f, 3f), new Vector2(-3f, -3f),
            parchment);

        MakeImage(clueDisplayUI.transform, "HeaderBG",
            new Vector2(0f, 0.74f), new Vector2(1f, 1f),
            new Vector2(3f, 0f), new Vector2(-3f, -3f),
            new Color(0.22f, 0.13f, 0.04f, 1f));

        MakeImage(clueDisplayUI.transform, "Divider",
            new Vector2(0.05f, 0.737f), new Vector2(0.95f, 0.748f),
            Vector2.zero, Vector2.zero,
            new Color(0.55f, 0.38f, 0.14f, 1f));

        MakeTMP(clueDisplayUI.transform, "Header",
            new Vector2(0f, 0.74f), new Vector2(1f, 1f),
            new Vector2(8f, 2f), new Vector2(-8f, -2f),
            "~ Clue " + (clueIndex + 1) + " ~",
            20, new Color(0.96f, 0.88f, 0.68f), TextAlignmentOptions.Center, FontStyles.Bold);

        MakeTMP(clueDisplayUI.transform, "ClueText",
            new Vector2(0f, 0f), new Vector2(1f, 0.74f),
            new Vector2(18f, 12f), new Vector2(-18f, -8f),
            "\u201c" + clueText + "\u201d",
            17, inkBrown, TextAlignmentOptions.Center, FontStyles.Italic,
            wrap: true);
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
}