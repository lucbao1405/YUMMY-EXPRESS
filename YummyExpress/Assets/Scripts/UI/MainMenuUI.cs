using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Điều khiển màn hình chính (Mainmenu).
/// - Nút Play/Bắt đầu: chuyển sang scene chơi game.
/// - Nút Thoát (nếu có): thoát game.
/// Gắn script này lên GameObject chứa các nút, hoặc kéo thả tham chiếu trong Inspector.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
[Header("=== Scene Settings ===")]
    [Tooltip("Tên scene chơi game sẽ mở khi bấm Play. Mặc định: UI")]
    [SerializeField] private string gameplaySceneName = "UI";

    [Header("=== Buttons (tùy chọn) ===")]
    [Tooltip("Nút Play/Bắt đầu (auto-find theo tên 'Play' nếu để trống)")]
    [SerializeField] private Button playButton;
    [Tooltip("Nút Thoát game (tùy chọn, auto-find theo tên 'Quit' nếu để trống)")]
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        ResolveButtons();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        else
            Debug.LogWarning("MainMenuUI: Không tìm thấy nút Play. Gắn Script này lên nút Play hoặc kéo thả playButton.", this);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    /// <summary>Nút Play — chuyển sang scene gameplay.</summary>
    public void OnPlayClicked()
    {
        Time.timeScale = 1f; // Reset lại thời gian phòng khi trước đó bị pause
        Debug.Log($"MainMenuUI: Bấm Play → Load scene '{gameplaySceneName}'", this);
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>Nút Play Game — chuyển sang scene LevelSelection.</summary>
    public void OnClickPlayGame()
    {
        Time.timeScale = 1f;
        Debug.Log("MainMenuUI: Bấm Play Game → Load scene 'LevelSelection'", this);
        SceneManager.LoadScene("LevelSelection");
    }

    /// <summary>Nút Thoát — thoát game (chỉ có tác dụng khi build).</summary>
    public void OnQuitClicked()
    {
        Debug.Log("MainMenuUI: Thoát game.");
        Application.Quit();
    }

    // Tự động tìm nút theo tên nếu chưa được kéo thả.
    private void ResolveButtons()
    {
        if (playButton == null)
            playButton = FindButtonInChildren("Play") ?? FindButtonInChildren("BatDau") ?? FindButtonInChildren("Start");

        if (quitButton == null)
            quitButton = FindButtonInChildren("Quit") ?? FindButtonInChildren("Thoat") ?? FindButtonInChildren("Exit");
    }

    private Button FindButtonInChildren(string childName)
    {
        Transform t = transform.Find(childName);
        if (t == null) return null;
        return t.GetComponent<Button>();
    }
}
