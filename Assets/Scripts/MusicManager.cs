using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("Volum")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.35f;

    private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null) 
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ApplyMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForScene(scene.name);
    }

    private void ApplyMusicForScene(string sceneName)
    {
        AudioClip target = null;

        if (sceneName == "MainMenu") 
            target = mainMenuMusic;
        else if (sceneName == "Game") 
            target = gameMusic;
        else if (sceneName == "GameOver") 
            target = gameOverMusic;

        if (target == null) return;

        if (musicSource.clip == target && musicSource.isPlaying) 
            return;

        musicSource.clip = target;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void SetVolume(float newVolume01)
    {
        volume = Mathf.Clamp01(newVolume01);
        if (musicSource != null) 
            musicSource.volume = volume;
    }
}