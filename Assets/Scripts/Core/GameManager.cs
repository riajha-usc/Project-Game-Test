using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Settings")]
    public string levelStartSceneName;

    [Header("Progress (persisted across lanes)")]
    [HideInInspector] public int lanesCompleted = 0;
    public int totalLanes = 3;

    [Header("Session Metrics (for analytics)")]
    [HideInInspector] public long sessionId;
    [HideInInspector] public int deathsToEnemy;
    [HideInInspector] public int incorrectKeyCount;
    [HideInInspector] public int incorrectCodeCount;
    [HideInInspector] public int cluesSolved;
    [HideInInspector] public float levelStartTime;
    [HideInInspector] public float levelCompleteTime;

    [Header("Key Data (persisted across lanes)")]
    [HideInInspector] public string lane1CorrectShape;
    [HideInInspector] public string lane1CorrectColor;
    [HideInInspector] public string lane2CorrectShape;
    [HideInInspector] public string lane2CorrectColor;
    [HideInInspector] public bool lane2CorrectKeySpinning;
    [HideInInspector] public List<string> lane2Clues = new List<string>();

    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }

    public GameState currentState = GameState.Start;

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
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        sessionId = System.DateTime.Now.Ticks;
        levelStartTime = Time.unscaledTime;
    }

    public void RecordDeathToEnemy()
    {
        deathsToEnemy++;
    }

    public void RecordIncorrectKey()
    {
        incorrectKeyCount++;
    }

    public void RecordIncorrectCode()
    {
        incorrectCodeCount++;
    }

    public void RecordClueSolved()
    {
        cluesSolved++;
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver)
            return;

        Debug.Log("GameOver");
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        var sendToGoogle = GetComponent<SendToGoogle>();
        if (sendToGoogle != null)
            sendToGoogle.Send();

        UIManager.Instance.ShowGameOver();
    }

    // Restart the entire level (load the first lane scene)
    public void RestartLevel()
    {
        StartCoroutine(RestartLevelCoroutine());
    }

    IEnumerator RestartLevelCoroutine()
    {
        currentState = GameState.Start;
        lanesCompleted = 0;
        deathsToEnemy = 0;
        incorrectKeyCount = 0;
        incorrectCodeCount = 0;
        cluesSolved = 0;

        Time.timeScale = 1f;

        yield return null;

        AsyncOperation op = !string.IsNullOrEmpty(levelStartSceneName)
            ? SceneManager.LoadSceneAsync(levelStartSceneName)
            : SceneManager.LoadSceneAsync(0);

        if (op != null)
        {
            while (!op.isDone)
                yield return null;
        }
    }

    public void LoadNextLane()
    {
        currentState = GameState.Playing;
        lanesCompleted++;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            levelCompleteTime = Time.unscaledTime - levelStartTime;
            Debug.Log("No more lanes. Level complete!");

            var sendToGoogle = GetComponent<SendToGoogle>();
            if (sendToGoogle != null)
                sendToGoogle.Send();
        }
    }

    public float GetLevelTimeSeconds()
    {
        if (currentState == GameState.GameOver)
            return Time.unscaledTime - levelStartTime;
        if (levelCompleteTime > 0f)
            return levelCompleteTime;
        return Time.unscaledTime - levelStartTime;
    }

    public int GetCurrentLaneNumber()
    {
        // Use scene build index for accuracy (handles starting in any lane)
        int idx = SceneManager.GetActiveScene().buildIndex;
        int lane = Mathf.Clamp(idx + 1, 1, totalLanes);
        return lane;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When restarting, show the start screen after the new scene loads
        if (currentState == GameState.Start && UIManager.Instance != null)
        {
            UIManager.Instance.ShowStartScreen();
        }

        Time.timeScale = (currentState == GameState.Playing) ? 1f : 0f;
        if (PlayerSpawnPoint.Instance != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                CharacterController controller = player.GetComponent<CharacterController>();

                if (controller != null)
                    controller.enabled = false;

                player.transform.position = PlayerSpawnPoint.Instance.transform.position;
                player.transform.rotation = PlayerSpawnPoint.Instance.transform.rotation;

                if (controller != null)
                    controller.enabled = true;
            }
        }
    }
}
