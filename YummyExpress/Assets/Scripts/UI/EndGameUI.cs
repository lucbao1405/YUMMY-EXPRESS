using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện EndGame (Win/Lose Panel) và chuyển Scene.
/// Gắn script này vào GameObject trong Canvas chứa WinPanel và LosePanel.
/// </summary>
public class EndGameUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Button References")]
    [SerializeField] private Button btnReplay;
    [SerializeField] private Button btnNextLevel;
    [SerializeField] private Button btnMainMenu;

    private void Awake()
    {
        // Ẩn panel khi bắt đầu
        SetPanelActive(winPanel, false);
        SetPanelActive(losePanel, false);
    }

    private void Start()
    {
        // Gán sự kiện cho các Button
        SetupButtons();
    }

    /// <summary>
    /// Gán callback cho từng Button.
    /// </summary>
    private void SetupButtons()
    {
        if (btnReplay != null)
            btnReplay.onClick.AddListener(OnReplayClicked);

        if (btnNextLevel != null)
            btnNextLevel.onClick.AddListener(OnNextLevelClicked);

        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(OnMainMenuClicked);
    }

    // ---- Button Callbacks ----

    /// <summary>
    /// Load lại Scene hiện tại (Replay).
    /// </summary>
    public void OnReplayClicked()
    {
        Time.timeScale = 1f; // Reset time scale trước khi load lại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Load Scene tiếp theo (Next Level).
    /// </summary>
    public void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Kiểm tra nếu scene tiếp theo tồn tại trong Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("EndGameUI: Không còn Scene tiếp theo! Quay về MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
    }

    /// <summary>
    /// Load Scene "MainMenu".
    /// </summary>
    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ---- Public Methods ----

    /// <summary>
    /// Kết thúc game: hiển thị WinPanel hoặc LosePanel tương ứng.
    /// GameManager gọi method này sau khi set Time.timeScale = 0.
    /// </summary>
    /// <param name="isWin">true nếu thắng (hiện WinPanel), false nếu thua (hiện LosePanel)</param>
    public void ShowEndGame(bool isWin)
    {
        if (isWin)
        {
            SetPanelActive(winPanel, true);
            SetPanelActive(losePanel, false);
        }
        else
        {
            SetPanelActive(losePanel, true);
            SetPanelActive(winPanel, false);
        }
    }

    /// <summary>
    /// Ẩn tất cả EndGame panels. Gọi khi bắt đầu level mới.
    /// </summary>
    public void HideAllPanels()
    {
        SetPanelActive(winPanel, false);
        SetPanelActive(losePanel, false);
    }

    // ---- Helpers ----

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
