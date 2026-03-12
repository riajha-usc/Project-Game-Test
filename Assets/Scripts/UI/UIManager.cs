using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject interactLanePrompt;
    public GameObject lane1EntryText;

    [Header("UI Screens")]
    public GameObject startScreen;
    public GameObject gameOverScreen;
    [Tooltip("Optional: title text on game over screen. Used for both Game Over and You have won!")]
    public TMP_Text gameOverTitleText;

    [Header("Game Layout (optional - will auto-create if null)")]
    public GameLayout gameLayout;

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
        }
    }

    void Start()
    {
        if (gameLayout == null)
        {
            var layoutObj = new GameObject("GameLayout");
            layoutObj.transform.SetParent(transform, false);
            var rect = layoutObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            gameLayout = layoutObj.AddComponent<GameLayout>();
        }
        
        ShowStartScreen();
        
    }

    public void ShowStartScreen()
    {
        if (startScreen != null)
            startScreen.SetActive(true);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (gameLayout != null)
            gameLayout.gameObject.SetActive(false);
    }

    public void HideStartScreen()
    {
        if (startScreen != null)
            startScreen.SetActive(false);

        ShowLaneEntryTextForCurrentScene();

        if (gameLayout != null)
            gameLayout.gameObject.SetActive(true);

        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Start)
            GameManager.Instance.StartGame();
    }
    public void ShowLaneEntryTextForCurrentScene()
    {
        int lane = GameManager.Instance != null ? GameManager.Instance.GetCurrentLaneNumber() : 1;
        string objectName = $"Lane{lane}Start";

        if (lane == 1 && lane1EntryText != null)
        {
            lane1EntryText.SetActive(true);
            return;
        }

        var rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjs)
        {
            var found = FindChildByName(root.transform, objectName);
            if (found != null)
            {
                found.gameObject.SetActive(true);
                return;
            }
        }
    }

    static Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindChildByName(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    public void ShowGameOver(bool isVictory = false)
    {
        var title = gameOverTitleText ?? (gameOverScreen != null ? gameOverScreen.GetComponentInChildren<TMP_Text>(true) : null);
        if (title != null)
        {
            title.text = isVictory 
                ? "<b><color=#3CFF6E>You have Won!</color></b>" 
                : "<b><color=#FF4C4C>Game Over</color></b>";
            if (isVictory && title != null)
                StartCoroutine(PulseVictoryText(title));
        }
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (gameLayout != null)
            gameLayout.gameObject.SetActive(false);

        if (interactLanePrompt != null)
            interactLanePrompt.SetActive(false);

        if (lane1EntryText != null)
            lane1EntryText.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        ShowGameOver(isVictory: true);
    }

    public void RestartGame()
    {

        if (GameManager.Instance != null)
            GameManager.Instance.RestartLevel();
        ShowStartScreen();
    }

    public void ShowInteractPrompt()
    {
        if (interactLanePrompt != null)
            interactLanePrompt.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        if (interactLanePrompt != null)
            interactLanePrompt.SetActive(false);
    }

    IEnumerator PulseVictoryText(TMP_Text text)
    {
        Color baseColor = text.color;
        float t = 0f;

        while (t < 1.5f)
        {
            float pulse = (Mathf.Sin(t * 6f) + 1f) * 0.5f; // 0 → 1
            Color bright = new Color(baseColor.r, Mathf.Clamp01(baseColor.g + 0.4f * pulse), baseColor.b);
            text.color = bright;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        text.color = baseColor;
    }
}