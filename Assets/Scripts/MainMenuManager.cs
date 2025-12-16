using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona el menú principal - Compatible amb Unity 6
/// Sense settings
/// IMPORTANT: El GameManager s'ha de posar a l'escena MainMenu (no crear-lo per codi)
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Botons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueGame);
            continueButton.interactable = SaveSystem.HasSaveData();
        }

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExit);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnStartGame()
    {
        Debug.Log("Start Game");

        if (SaveSystem.HasSaveData())
            SaveSystem.DeleteSave();

        SceneManager.LoadScene("Game");
    }

    private void OnContinueGame()
    {
        Debug.Log("Continue Game");

        if (!SaveSystem.HasSaveData())
        {
            Debug.LogWarning("No hi ha partida guardada!");
            return;
        }

        // GameManager ha d'existir (posat a MainMenu i DontDestroyOnLoad)
        if (GameManager.Instance == null)
        {
            Debug.LogError("No existeix GameManager a l'escena! Afegeix-lo a MainMenu.");
            return;
        }

        GameManager.Instance.ContinueGame();
    }

    private void OnExit()
    {
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
