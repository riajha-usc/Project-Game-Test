using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    static readonly Dictionary<int, float> _spikeGroupLastAnalyticsHitUnscaled = new Dictionary<int, float>();
    const float SpikeGroupHitAnalyticsDebounceSeconds = 0.35f;

    [Header("Level Settings")]
    public string levelStartSceneName;

    [Header("Progress (persisted across lanes)")]
    [HideInInspector] public int lanesCompleted = 0;

    [Header("Session Metrics (for analytics)")]
    [HideInInspector] public long sessionId;
    [HideInInspector] public int incorrectKeyCount;
    [HideInInspector] public int incorrectCodeCount;
    [HideInInspector] public int keyAttemptCount;
    [HideInInspector] public int codeAttemptCount;
    [HideInInspector] public int cluesSolved;
    HashSet<int> readClueIndices = new HashSet<int>();
    [HideInInspector] public float levelStartTime;
    [HideInInspector] public float levelCompleteTime;
    [HideInInspector] public int safeZoneEntryCount;
    [HideInInspector] public int clueZoneEntryCount;
    [HideInInspector] public int beamHitsCount;
    [HideInInspector] public int spikesHitsCount;
    [HideInInspector] public int dividerHitsCount;

    [Header("Key Data (persisted across lanes)")]
    [HideInInspector] public string levelCorrectShape;
    [HideInInspector] public string levelCorrectColor;
    [HideInInspector] public List<string> lane2Clues = new List<string>();

    public string finalAnswer;

    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }

    public GameState currentState = GameState.Start;
    bool _skipStartScreenOnReload;

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

    public void StartGame()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        // sessionId is set in PrepareEntryFromMainMenu only; progression keeps the same id.
        if (sessionId == 0L)
            sessionId = System.DateTime.Now.Ticks;
        levelStartTime = Time.unscaledTime;
    }

    public void PrepareEntryFromMainMenu()
    {
        currentState = GameState.Start;
        levelCompleteTime = 0f;
        sessionId = System.DateTime.Now.Ticks;
    }

    public void PrepareNextLevelFromProgression()
    {
        currentState = GameState.Playing;
    }

    public void RecordIncorrectKey()
    {
        incorrectKeyCount++;
    }

    public void RecordIncorrectCode()
    {
        incorrectCodeCount++;
    }

    public void RecordKeyAttempt()
    {
        keyAttemptCount++;
    }

    public void RecordCodeAttempt()
    {
        codeAttemptCount++;
    }

    public void RecordClueSolved(int clueIndex)
    {
        if (readClueIndices.Add(clueIndex))
            cluesSolved++;
    }

    public void RecordSafeZoneEntry()
    {
        safeZoneEntryCount++;
    }

    public void RecordClueZoneEntry()
    {
        clueZoneEntryCount++;
    }

    public void RecordBeamHit()
    {
        beamHitsCount++;
    }

    public void RecordSpikeHitForGroup(int spikeGroupRootInstanceId)
    {
        if (spikeGroupRootInstanceId == 0) return;
        float now = Time.unscaledTime;
        if (_spikeGroupLastAnalyticsHitUnscaled.TryGetValue(spikeGroupRootInstanceId, out float prev)
            && now - prev < SpikeGroupHitAnalyticsDebounceSeconds)
            return;
        _spikeGroupLastAnalyticsHitUnscaled[spikeGroupRootInstanceId] = now;
        spikesHitsCount++;
    }

    static void ClearSpikeGroupHitDebounceState()
    {
        _spikeGroupLastAnalyticsHitUnscaled.Clear();
    }

    public void RecordDividerHit()
    {
        dividerHitsCount++;
    }

    public void SubmitLevelAnalytics(bool won)
    {
        levelCompleteTime = Time.unscaledTime - levelStartTime;

        var sendToGoogle = GetComponent<SendToGoogle>();
        Debug.Log("SendToGoogle: " + sendToGoogle != null);
        if (sendToGoogle != null)
            sendToGoogle.Send(won);
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver)
            return;

        Debug.Log("GameOver");
        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SubmitLevelAnalytics(false);

        string sceneName = SceneManager.GetActiveScene().name;
        bool isTutorialScene =
            sceneName == "Tutorial-1" ||
            sceneName == "Traps-Prototype";

        if (isTutorialScene && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTutorialGameOver();
        }
        else if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        if (GameLayout.Instance != null)
            GameLayout.Instance.HideWrongFeedback();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.HideTutorialPopup();

        foreach (var lane3 in FindObjectsByType<Lane3DoorInteraction>(FindObjectsSortMode.None))
        {
            lane3.CloseInputPanel();
            lane3.HidePrompt();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Restart the entire level (load the first lane scene)
    public void RestartLevel()
    {
        StartCoroutine(RestartLevelCoroutine());
    }

    IEnumerator RestartLevelCoroutine()
    {
        _skipStartScreenOnReload = true;
        currentState = GameState.Playing;
        lanesCompleted = 0;
        TrapCombatAgentManager.ResetAll();
        incorrectKeyCount = 0;
        incorrectCodeCount = 0;
        keyAttemptCount = 0;
        codeAttemptCount = 0;
        cluesSolved = 0;
        readClueIndices.Clear();
        clueZoneEntryCount = 0;
        beamHitsCount = 0;
        spikesHitsCount = 0;
        dividerHitsCount = 0;
        ClearSpikeGroupHitDebounceState();

        Time.timeScale = 1f;

        yield return null;

        //AsyncOperation op = !string.IsNullOrEmpty(levelStartSceneName)
        //    ? SceneManager.LoadSceneAsync(levelStartSceneName)
        //    : SceneManager.LoadSceneAsync(0);

        string sceneToReload = SceneManager.GetActiveScene().name;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToReload);

        if (op != null)
        {
            while (!op.isDone)
                yield return null;
        }
    }

    public void LoadNextLane()
    {
        lanesCompleted++;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            MenuManager.LoadLevel2(fromMainMenu: false);
            return;
        }

        if (currentScene == "Level2")
        {
            MenuManager.LoadLevel3(fromMainMenu: false);
            return;
        }

        levelCompleteTime = Time.unscaledTime - levelStartTime;
        Debug.Log("No more levels!");

        Time.timeScale = 0f;

        if (GameLayout.Instance != null)
        {
            GameLayout.Instance.HideWrongFeedback();
            GameLayout.Instance.Refresh();
        }

        foreach (var lane3 in FindObjectsByType<Lane3DoorInteraction>(FindObjectsSortMode.None))
        {
            lane3.CloseInputPanel();
            lane3.HidePrompt();
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowVictoryScreen();

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
        return 1;
    }

    public int GetTotalCluesForCurrentLane()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Level1") return 1;
        return 2;
    }

    public int GetMaxAttemptsForCurrentLane()
    {
        string scene = SceneManager.GetActiveScene().name;
        // Tutorial has unlimited attempts – handled by TutorialKeyDeductionManager
        return 2;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void EnsureSingleEventSystem()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        for (int i = 1; i < eventSystems.Length; i++)
        {
            Destroy(eventSystems[i].gameObject);
        }
    }

    static bool IsGameplayLevelScene(string sceneName)
    {
        return sceneName == "Level1" || sceneName == "Level2" || sceneName == "Level3"
            || sceneName == "Tutorial-1" || sceneName == "Traps-Prototype";
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        EnsureSingleEventSystem();

        if (currentState == GameState.GameOver && IsGameplayLevelScene(scene.name))
        {
            currentState = GameState.Start;
            levelCompleteTime = 0f;
        }
        TrapCombatAgentManager.ResetAll();
        cluesSolved = 0;
        readClueIndices.Clear();
        incorrectKeyCount = 0;
        incorrectCodeCount = 0;
        keyAttemptCount = 0;
        codeAttemptCount = 0;
        safeZoneEntryCount = 0;
        clueZoneEntryCount = 0;
        beamHitsCount = 0;
        spikesHitsCount = 0;
        dividerHitsCount = 0;
        ClearSpikeGroupHitDebounceState();

        if (IsGameplayLevelScene(scene.name) && currentState == GameState.Playing)
        {
            levelStartTime = Time.unscaledTime;
            levelCompleteTime = 0f;
        }

        if (_skipStartScreenOnReload)
        {
            _skipStartScreenOnReload = false;
            currentState = GameState.Playing;
            if (UIManager.Instance != null)
                UIManager.Instance.HideStartScreen();
            else
                Time.timeScale = 1f;
        }
        else if (currentState == GameState.Start && UIManager.Instance != null)
        {
            if (UIManager.Instance.startScreen != null)
                UIManager.Instance.ShowStartScreen();
            else
                UIManager.Instance.HideStartScreen();
        }
        else if (currentState == GameState.Start)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }

        else if (currentState == GameState.Playing && UIManager.Instance != null)
        {
            UIManager.Instance.ShowLaneEntryTextForCurrentScene();
        }

        Time.timeScale = (currentState == GameState.Playing) ? 1f : 0f;
        if (scene.name == "MainMenu-Scene")
            Time.timeScale = 1f;

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

                // Reset player health / damage state here
                var pl = player.GetComponent<PlayerMovement3D>();
                if (pl != null)
                {
                    pl.hp = pl.maxHp;
                }
            }
        }
    }
}
