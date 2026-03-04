using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject interactLanePrompt;
    public GameObject lane1EntryText;

    [Header("UI Screens")]
    public GameObject startScreen;
    public GameObject gameOverScreen;

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

        if (lane1EntryText != null)
            lane1EntryText.SetActive(true);

        if (gameLayout != null)
            gameLayout.gameObject.SetActive(true);

        // Ensure time runs when starting (in case Start button only calls HideStartScreen)
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Start)
            GameManager.Instance.StartGame();
    }

    public void ShowGameOver()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }

    /// <summary>Called by Restart button. Uses static Instance for WebGL reliability.</summary>
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
}