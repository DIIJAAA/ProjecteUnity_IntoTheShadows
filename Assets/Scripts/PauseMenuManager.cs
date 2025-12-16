using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el menú de pausa dins del joc - Compatible amb Unity 6
/// Sense settings
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Botons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveAndExitButton;
    
    void Start()
    {
        // Configura botons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResume);
        
        if (saveAndExitButton != null)
            saveAndExitButton.onClick.AddListener(OnSaveAndExit);
    }
    
    /// <summary>
    /// Reprèn el joc
    /// </summary>
    private void OnResume()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    /// <summary>
    /// Guarda i surt
    /// </summary>
    private void OnSaveAndExit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveAndExit();
        }
    }
}