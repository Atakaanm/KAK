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

    public bool IsPaused => isPaused;

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

    // Cihazda uygulama arka plana gidince otomatik duraklat.
    // Editörde kapalı: editör odağı kaybedince (başka pencereye geçiş, otomatik testler) oyun durmasın.
    void OnApplicationPause(bool paused)
    {
        if (Application.isEditor) return;
        if (paused && !isPaused)
            Pause();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (Application.isEditor) return;
        if (!hasFocus && !isPaused)
            Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        isPaused = true;
        savedTimeScale = Time.timeScale;
        if (savedTimeScale <= 0f) savedTimeScale = 1f;
        // fixedDeltaTime'a dokunma: timeScale = 0 iken fizik zaten ilerlemez
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        KakTime.SetTimeScale(savedTimeScale);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void GoToMainMenu()
    {
        Resume();
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            SceneLoader.LoadMenu();
    }

    void OnDestroy()
    {
        if (isPaused)
            KakTime.SetTimeScale(savedTimeScale);
    }
}
