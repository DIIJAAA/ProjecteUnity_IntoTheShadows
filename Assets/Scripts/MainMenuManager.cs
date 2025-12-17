using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        if (SaveSystem.HasSaveData())
            SaveSystem.DeleteSave();

        SceneManager.LoadScene("Game");
    }

    private void OnContinueGame()
    {
        if (!SaveSystem.HasSaveData()) return;

        if (GameManager.Instance == null) return;

        GameManager.Instance.ContinueGame();
    }

    private void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}