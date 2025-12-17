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

    [Header("UI In-Game")]
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text hintsText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Sistema de Hints")]
    [SerializeField] private int hintsPerRun = 3;
    [SerializeField] private float hintCooldownSeconds = 8f;
    [SerializeField] private AudioClip hintPingClip;
    [SerializeField] private float hintPingMaxDistance = 45f;
    [SerializeField] private float hintPingMinDistance = 3f;

    private int hintsRemaining;
    private float nextHintTime;
    private float lastHintDistance = -1f;
    private AudioSource hint3DSource;

    [Header("Audio SFX")]
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

        InitializeAudio();
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
        if (SceneManager.GetActiveScene().name != "Game" || !gameActive || isPaused) 
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();

        if (Input.GetKeyDown(KeyCode.H))
            TryUseHint();

        UpdateTimer();
    }

    // ============================================
    // INICIALITZACIÓ
    // ============================================

    private void InitializeAudio()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        hint3DSource = gameObject.AddComponent<AudioSource>();
        hint3DSource.playOnAwake = false;
        hint3DSource.spatialBlend = 1f;
        hint3DSource.rolloffMode = AudioRolloffMode.Linear;
        hint3DSource.minDistance = 2f;
        hint3DSource.maxDistance = 60f;
        hint3DSource.dopplerLevel = 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void FindGameUIReferences()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas == null) return;

        Transform inGameUI = canvas.transform.Find("InGameUI");
        if (inGameUI != null)
        {
            livesText = inGameUI.Find("LivesText")?.GetComponent<TMP_Text>();
            timerText = inGameUI.Find("TimerText")?.GetComponent<TMP_Text>();
            hintsText = inGameUI.Find("HintsText")?.GetComponent<TMP_Text>();
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

    // ============================================
    // GESTIÓ DEL JOC
    // ============================================

    public void StartNewGame()
    {
        gameStartTime = Time.time;
        gameActive = true;
        playerLives = 2;
        hasKey = false;
        isLoadedGame = false;
        currentMazeSeed = Random.Range(0, 999999);

        GenerateMaze(currentMazeSeed);
        
        hintsRemaining = hintsPerRun;
        nextHintTime = 0f;
        lastHintDistance = -1f;

        UpdateUI();
        UpdateHintsUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ShowMessage("Prem H per un hint", 2f);
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

        hintsRemaining = hintsPerRun;
        nextHintTime = 0f;
        lastHintDistance = -1f;

        UpdateUI();
        UpdateHintsUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ShowMessage("Partida carregada", 2f);
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
    }

    // ============================================
    // SISTEMA DE HINTS
    // ============================================

    private void TryUseHint()
    {
        if (hintsRemaining <= 0)
        {
            ShowMessage("No queden hints", 1.5f);
            return;
        }

        if (Time.time < nextHintTime)
        {
            float wait = nextHintTime - Time.time;
            ShowMessage($"Espera {wait:0.0}s", 1.5f);
            return;
        }

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Transform target = FindHintTarget();

        if (player == null || target == null)
        {
            ShowMessage("No hi ha objectiu disponible", 1.5f);
            return;
        }

        float dist = Vector3.Distance(player.position, target.position);
        PlayHintPing(target, player, dist);

        string proximity =
            dist < 6f ? "MOLT A PROP" :
            dist < 14f ? "A PROP" :
            dist < 28f ? "LLUNY" : "MOLT LLUNY";

        string trend = "";
        if (lastHintDistance >= 0f)
            trend = (dist < lastHintDistance) ? " (més a prop)" : " (més lluny)";

        lastHintDistance = dist;

        string targetName = hasKey ? "SORTIDA" : "CLAU";
        ShowMessage($"{targetName}: {proximity}{trend}", 2.0f);

        hintsRemaining--;
        nextHintTime = Time.time + hintCooldownSeconds;
        UpdateHintsUI();
    }

    private Transform FindHintTarget()
    {
        if (!hasKey)
        {
            GameObject keyObj = GameObject.Find("Key");
            if (keyObj != null) return keyObj.transform;
        }

        GameObject doorObj = GameObject.Find("ExitDoor");
        if (doorObj != null) return doorObj.transform;

        return null;
    }

    private void PlayHintPing(Transform target, Transform player, float dist)
    {
        if (hintPingClip == null || hint3DSource == null) return;

        float t = Mathf.InverseLerp(hintPingMinDistance, hintPingMaxDistance, dist);
        float volume = Mathf.Lerp(0.7f, 0.05f, t);
        float pitch = Mathf.Lerp(1.15f, 0.9f, t);

        Vector3 dir = (target.position - player.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            dir = player.forward;

        dir.Normalize();

        Vector3 pingPos = player.position + dir * 6f;
        pingPos.y = player.position.y + 1.2f;

        hint3DSource.transform.position = pingPos;
        hint3DSource.volume = volume;
        hint3DSource.pitch = pitch;
        hint3DSource.PlayOneShot(hintPingClip);
    }

    // ============================================
    // SISTEMA DE VIDES
    // ============================================

    public void LoseLife()
    {
        if (!gameActive) return;

        playerLives--;

        if (hurtSound != null)
            sfxSource.PlayOneShot(hurtSound);

        UpdateUI();

        if (playerLives <= 0)
            GameOver();
        else
            ShowMessage($"Vides restants: {playerLives}", 1.2f);
    }

    public void OnPlayerHit()
    {
        LoseLife();
    }

    // ============================================
    // CLAU I SORTIDA
    // ============================================

    public void CollectKey()
    {
        hasKey = true;

        if (keyPickupSound != null)
            sfxSource.PlayOneShot(keyPickupSound);

        ShowMessage("Clau trobada! Cerca la sortida!", 3f);
    }

    public bool HasKey() => hasKey;

    public void CompleteGame()
    {
        gameActive = false;

        float finalTime = Time.time - gameStartTime;
        PlayerPrefs.SetFloat("VictoryTime", finalTime);
        PlayerPrefs.SetInt("VictoryLives", playerLives);
        PlayerPrefs.Save();

        SaveSystem.DeleteSave();
        ShowMessage("HAS ESCAPAT!", 3f);

        StartCoroutine(ReturnToMenuAfterVictory());
    }

    private IEnumerator ReturnToMenuAfterVictory()
    {
        yield return new WaitForSeconds(3f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ============================================
    // GAME OVER
    // ============================================

    private void GameOver()
    {
        gameActive = false;

        if (deathSound != null)
            sfxSource.PlayOneShot(deathSound);

        SaveSystem.DeleteSave();

        float finalTime = Time.time - gameStartTime;
        PlayerPrefs.SetFloat("LastGameTime", finalTime);
        PlayerPrefs.SetInt("LastHadKey", hasKey ? 1 : 0);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    // ============================================
    // PAUSA
    // ============================================

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
        ShowMessage("Partida guardada", 1.5f);
    }

    // ============================================
    // UI
    // ============================================

    private void UpdateTimer()
    {
        if (timerText == null) return;

        float elapsed = Time.time - gameStartTime;
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);

        timerText.text = $"Temps: {minutes:00}:{seconds:00}";
    }

    private void UpdateUI()
    {
        if (livesText != null)
            livesText.text = $"Vides: {playerLives}";
    }

    private void UpdateHintsUI()
    {
        if (hintsText != null)
            hintsText.text = $"Hints: {hintsRemaining}";
    }

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
}