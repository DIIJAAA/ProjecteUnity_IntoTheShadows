using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button restartButton;

    [Header("Missatges")]
    [SerializeField] private string[] deathMessages =
    {
        "HAS MORT",
        "PERDUT AL LABERINT",
        "CONSUMIT PER LA FOSCOR",
        "L'ENTITAT T'HA RECLAMAT"
    };

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(ReturnToMenu);

        ShowStats();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowStats()
    {
        if (titleText != null && deathMessages.Length > 0)
            titleText.text = deathMessages[Random.Range(0, deathMessages.Length)];

        if (statsText != null)
        {
            float time = PlayerPrefs.GetFloat("LastGameTime", 0f);
            bool hadKey = PlayerPrefs.GetInt("LastHadKey", 0) == 1;

            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);

            statsText.text =
                $"TEMPS DE SUPERVIVÈNCIA: {min:00}:{sec:00}\n\n" +
                (hadKey
                    ? "Vas trobar la clau...\nperò no vas poder escapar."
                    : "Mai vas trobar la clau.");
        }
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}