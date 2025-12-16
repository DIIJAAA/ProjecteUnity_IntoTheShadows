using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estat del Joc")]
    private bool hasKey = false;
    private int playerLives = 2;
    private float gameStartTime = 0f;
    private bool gameActive = false;
    private bool isLoadedGame = false;

    [Header("Configuració del Laberint")]
    public int currentMazeSeed = 0;
    public int mazeWidth = 10;
    public int mazeHeight = 10;

    [Header("UI In-Game (TMP)")]
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject messagePanel;

    [Header("Menú de Pausa")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip keyPickupSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;


    private AudioSource sfxSource;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Game") return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (gameActive && !isPaused)
            UpdateTimer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sempre reset per seguretat
        Time.timeScale = 1f;
        isPaused = false;

        if (scene.name == "Game")
        {
            FindGameUIReferences();

            if (isLoadedGame) LoadGameState();
            else StartNewGame();
        }
        else if (scene.name == "MainMenu")
        {
            // Cursor menú
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void FindGameUIReferences()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null)
        {
            Debug.LogWarning("GameCanvas no trobat.");
            return;
        }

        Transform inGameUI = canvas.transform.Find("InGameUI");
        if (inGameUI != null)
        {
            livesText = inGameUI.Find("LivesText")?.GetComponent<TMP_Text>();
            timerText = inGameUI.Find("TimerText")?.GetComponent<TMP_Text>();
        }

        messagePanel = canvas.transform.Find("MessagePanel")?.gameObject;
        if (messagePanel != null)
        {
            messageText = messagePanel.GetComponentInChildren<TMP_Text>(true);
            messagePanel.SetActive(false);
        }

        pauseMenuPanel = canvas.transform.Find("PauseMenu")?.gameObject;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }



    public void StartNewGame()
    {
        gameStartTime = Time.time;
        gameActive = true;

        playerLives = 2;
        hasKey = false;
        isLoadedGame = false;

        currentMazeSeed = Random.Range(0, 999999);

        GenerateMaze(currentMazeSeed);
        UpdateUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ContinueGame()
    {
        isLoadedGame = true;
        SceneManager.LoadScene("Game");
    }

    private void LoadGameState()
    {
        GameData data = SaveSystem.LoadGame();
        if (data == null)
        {
            StartNewGame();
            return;
        }

        playerLives = data.playerLives;
        hasKey = data.hasKey;
        gameStartTime = Time.time - data.playTime;
        gameActive = true;

        currentMazeSeed = data.mazeSeed;
        mazeWidth = data.mazeWidth;
        mazeHeight = data.mazeHeight;

        GenerateMaze(currentMazeSeed);
        StartCoroutine(PositionPlayerAfterLoad(data.playerPosition));

        UpdateUI();
        ShowMessage("Game loaded", 2f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator PositionPlayerAfterLoad(Vector3 position)
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.position = position;

        if (cc != null) cc.enabled = true;
    }

    private void GenerateMaze(int seed)
    {
        Random.InitState(seed);

        MazeGenerator generator = FindFirstObjectByType<MazeGenerator>();
        if (generator != null)
        {
            generator.mazeWidth = mazeWidth;
            generator.mazeHeight = mazeHeight;
        }

        MazeRenderer renderer = FindFirstObjectByType<MazeRenderer>();
        if (renderer != null)
        {
            renderer.GenerateNewMaze();
        }
        else
        {
            Debug.LogError("MazeRenderer no trobat.");
        }
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        GameData data = new GameData
        {
            playerLives = playerLives,
            hasKey = hasKey,
            playTime = Time.time - gameStartTime,
            mazeSeed = currentMazeSeed,
            mazeWidth = mazeWidth,
            mazeHeight = mazeHeight,
            playerPosition = playerPos
        };

        SaveSystem.SaveGame(data);
        ShowMessage("Game saved", 1.5f);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveAndExit()
    {
        SaveGame();
        Time.timeScale = 1f;
        isPaused = false;
        gameActive = false;

        SceneManager.LoadScene("MainMenu");
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;

        float elapsed = Time.time - gameStartTime;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);

        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateUI()
    {
        if (livesText != null)
            livesText.text = $"Lives: {playerLives}";
    }

    public void CollectKey()
    {
        hasKey = true;

        if (keyPickupSound != null && sfxSource != null)
            sfxSource.PlayOneShot(keyPickupSound);

        ShowMessage("Key found! Now find the exit!", 3f);
    }

    public bool HasKey() => hasKey;

    public void ShowMessage(string msg, float duration)
    {
        if (messagePanel == null || messageText == null) return;

        StopAllCoroutines();
        StartCoroutine(MessageRoutine(msg, duration));
    }

    private IEnumerator MessageRoutine(string msg, float duration)
    {
        messageText.text = msg;
        messagePanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        messagePanel.SetActive(false);
    }

    private void GameOver()
    {
        gameActive = false;

        if (deathSound != null && sfxSource != null)
            sfxSource.PlayOneShot(deathSound);

        SaveSystem.DeleteSave();

        float finalTime = Time.time - gameStartTime;
        PlayerPrefs.SetFloat("LastGameTime", finalTime);
        PlayerPrefs.SetInt("LastHadKey", hasKey ? 1 : 0);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    public void CompleteGame()
    {
        gameActive = false;

        // Guardar stats de victòria (opcional)
        float finalTime = Time.time - gameStartTime;
        PlayerPrefs.SetFloat("VictoryTime", finalTime);
        PlayerPrefs.SetInt("VictoryLives", playerLives);
        PlayerPrefs.Save();

        // Esborrar save perquè ja has acabat
        SaveSystem.DeleteSave();

        ShowMessage("ESCAPED!", 3f);

        // Tornar al menú després d’un moment
        StartCoroutine(ReturnToMenuAfterVictory());
    }

    private IEnumerator ReturnToMenuAfterVictory()
    {
        yield return new WaitForSeconds(3f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
