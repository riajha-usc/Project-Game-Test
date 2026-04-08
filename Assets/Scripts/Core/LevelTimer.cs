using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelTimer : MonoBehaviour
{
   [Tooltip("Total seconds for this level. Leave 0 to auto-detect (Level1=60, Level2=90).")]
    public float totalTime = 0f;

    [Tooltip("Warning countdown threshold. Leave 0 to auto-detect (Level1=20, Level2=30).")]
    public float warnThreshold = 0f;

    [Tooltip("HP drained during warning period, every drainInterval seconds.")]
    public float hpDrainAmount = 2f;

    [Tooltip("How often (seconds) to drain HP during warning period.")]
    public float drainInterval = 5f;

    float _timeRemaining;
    bool _active;
    bool _inWarnPhase;
    float _drainAccumulator;

    TMP_Text _timerText;
    TMP_Text _warnLabel;
    GameObject _timerRoot;
    Coroutine _blinkCoroutine;

    static readonly Color ColNormal  = Color.white;
    static readonly Color ColWarning = new Color(1f, 0.22f, 0.1f, 1f);

    void Start()
    {
        AutoDetectConfig();
        _timeRemaining = totalTime;
        _drainAccumulator = 0f;
        BuildTimerUI();
        _active = true;
    }

    void Update()
    {
        if (!_active) return;

        if (GameManager.Instance == null ||
            GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _active = false;
            UpdateDisplay(0f);
            GameManager.Instance?.GameOver();
            return;
        }

        UpdateDisplay(_timeRemaining);

        bool shouldWarn = _timeRemaining <= warnThreshold;
        if (shouldWarn && !_inWarnPhase)
            EnterWarnPhase();
        else if (!shouldWarn && _inWarnPhase)
            ExitWarnPhase();

        if (_inWarnPhase)
        {
            _drainAccumulator += Time.deltaTime;
            if (_drainAccumulator >= drainInterval)
            {
                _drainAccumulator -= drainInterval;
                DrainPlayerHP();
            }
        }
    }

    void AutoDetectConfig()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (totalTime <= 0f)
            totalTime = scene.StartsWith("Level2") ? 90f : 60f;

        if (warnThreshold <= 0f)
            warnThreshold = scene.StartsWith("Level2") ? 30f : 20f;
    }

    void EnterWarnPhase()
    {
        _inWarnPhase = true;
        _drainAccumulator = 0f;
        if (_warnLabel != null) _warnLabel.gameObject.SetActive(true);
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkTimer());
    }

    void ExitWarnPhase()
    {
        _inWarnPhase = false;
        if (_blinkCoroutine != null) { StopCoroutine(_blinkCoroutine); _blinkCoroutine = null; }
        if (_timerText != null) _timerText.color = ColNormal;
        if (_warnLabel != null) _warnLabel.gameObject.SetActive(false);
    }

    IEnumerator BlinkTimer()
    {
        bool visible = true;
        while (_inWarnPhase)
        {
            if (_timerText != null)
                _timerText.color = visible ? ColWarning : new Color(ColWarning.r, ColWarning.g, ColWarning.b, 0.15f);
            visible = !visible;
            yield return new WaitForSeconds(0.4f);
        }
    }

    void DrainPlayerHP()
    {
        var player = FindObjectOfType<PlayerMovement3D>();
        if (player != null)
            player.TakeDamage(hpDrainAmount);
    }

    void UpdateDisplay(float t)
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        _timerText.text = $"Time Left  {minutes:D2}:{seconds:D2}";
    }

    void SetVisible(bool show)
    {
        if (_timerRoot != null && _timerRoot.activeSelf != show)
            _timerRoot.SetActive(show);
    }

    void BuildTimerUI()
    {
        Transform parent = GameLayout.Instance != null
            ? GameLayout.Instance.transform
            : transform;

        _timerRoot = new GameObject("LevelTimer");
        _timerRoot.transform.SetParent(parent, false);

        var rect = _timerRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot     = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -62f);
        rect.sizeDelta = new Vector2(220f, 52f);

        var bg = new GameObject("BG");
        bg.transform.SetParent(_timerRoot.transform, false);
        bg.transform.SetAsFirstSibling();
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-6f, -6f);
        bgRect.offsetMax = new Vector2(6f, 6f);
        var img = bg.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.1f, 0.42f);   

        var textGO = new GameObject("TimerText");
        textGO.transform.SetParent(_timerRoot.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.38f);
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        _timerText = textGO.AddComponent<TextMeshProUGUI>();
        _timerText.fontSize = 20f;
        _timerText.fontStyle = FontStyles.Bold;
        _timerText.color = ColNormal;
        _timerText.alignment = TextAlignmentOptions.Center;
        _timerText.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            _timerText.font = TMP_Settings.defaultFontAsset;

        var warnGO = new GameObject("WarnLabel");
        warnGO.transform.SetParent(_timerRoot.transform, false);
        var warnRect = warnGO.AddComponent<RectTransform>();
        warnRect.anchorMin = Vector2.zero;
        warnRect.anchorMax = new Vector2(1f, 0.42f);
        warnRect.offsetMin = Vector2.zero;
        warnRect.offsetMax = Vector2.zero;

        _warnLabel = warnGO.AddComponent<TextMeshProUGUI>();
        _warnLabel.text = $"[!]  -{hpDrainAmount} HP every {drainInterval}s";
        _warnLabel.fontSize = 15f;
        _warnLabel.color = ColWarning;
        _warnLabel.alignment = TextAlignmentOptions.Center;
        _warnLabel.textWrappingMode = TextWrappingModes.NoWrap;
        if (TMP_Settings.defaultFontAsset != null)
            _warnLabel.font = TMP_Settings.defaultFontAsset;
        warnGO.SetActive(false); 

        UpdateDisplay(_timeRemaining);
    }
}
