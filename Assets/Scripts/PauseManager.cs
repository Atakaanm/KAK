using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button mainMenuButton;
    
    private bool isPaused = false;
    private float savedTimeScale = 1f;
    
    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (pauseButton != null)
            pauseButton.onClick.AddListener(Pause);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }
    
    void OnApplicationPause(bool paused)
    {
        if (paused && !isPaused)
            Pause();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
            Pause();
    }
    
    public void Pause()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;
            
        isPaused = true;
        savedTimeScale = Time.timeScale;
        if (savedTimeScale == 0f) savedTimeScale = 1f;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = savedTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
    
    void GoToMainMenu()
    {
        Resume(); // Restore timeScale before loading
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
    
    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = savedTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
}
