using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Botons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveAndExitButton;
    
    void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResume);
        
        if (saveAndExitButton != null)
            saveAndExitButton.onClick.AddListener(OnSaveAndExit);
    }
    
    private void OnResume()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }
    
    private void OnSaveAndExit()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SaveAndExit();
    }
}


