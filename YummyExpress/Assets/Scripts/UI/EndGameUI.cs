using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện Thắng/Thua (Win_Popup / Lose_Popup) trong Popup_Overlay.
/// Gắn script này lên GameObject "Popup_Overlay".
///
/// ✅ KHỚP với Hierarchy thực tế trong UI.unity:
///   Popup_Overlay [Image]
///   ├── Win_Popup [Image]
///   │   ├── icon_Win                → Image
///   │   ├── Khung_3_Sao             → container (HorizontalLayoutGroup)
///   │   │   ├── Sao_1 / Sao_2 / Sao_3 → Image (sao sáng)
///   │   ├── Bang_Thong_Ke           → container (VerticalLayoutGroup)
///   │   │   └── Dong_Vang / Dong_Khach / Dong_Combo (mỗi dòng có TextMeshProUGUI)
///   │   ├── Btn_TiepTuc             → Button (màn tiếp theo)
///   │   └── Btn_XemVideo            → Button (xem QC nhân đôi thưởng)
///   └── Lose_Popup [Image]
///       ├── Icon_Lose               → Image
///       ├── KhuVuc_LyDoThua         → container
///       │   ├── Text_LyDo_Chinh     → TextMeshProUGUI (lý do thua)
///       │   └── Text_Meo_Tip        → TextMeshProUGUI (gợi ý)
///       └── Btn [HorizontalLayoutGroup]
///           ├── Btn_ChoiLai         → Button (chơi lại)
///           └── Btn_CuuTro          → Button (quay về MainMenu / trợ giúp)
/// </summary>
public class EndGameUI : SingletonBehaviour<EndGameUI>
{
    [Header("=== Panels (Popup_Overlay) ===")]
    [Tooltip("Popup thắng: Popup_Overlay/Win_Popup")]
    [SerializeField] private GameObject winPopup;
    [Tooltip("Popup thua: Popup_Overlay/Lose_Popup")]
    [SerializeField] private GameObject losePopup;

    [Header("=== Win_Popup ===")]
    [Tooltip("icon_Win (Image)")]
    [SerializeField] private GameObject iconWin;
    [Tooltip("Khung_3_Sao (container chứa Sao_1..Sao_3)")]
    [SerializeField] private GameObject khung3Sao;
    [Tooltip("Mảng 3 ngôi sao: Sao_1, Sao_2, Sao_3 (Image) — kéo lần lượt")]
    [SerializeField] private Image[] starImages = new Image[3];
    [Tooltip("Bang_Thong_Ke (container thống kê thưởng)")]
    [SerializeField] private GameObject bangThongKe;
    [Tooltip("Text số Vàng trong Dong_Vang (TextMeshProUGUI)")]
    [SerializeField] private TextMeshProUGUI thongKeGoldText;
    [Tooltip("Text số Khách trong Dong_Khach (TextMeshProUGUI) — tùy chọn")]
    [SerializeField] private TextMeshProUGUI thongKeCustomerText;
    [Tooltip("Text Combo trong Dong_Combo (TextMeshProUGUI) — tùy chọn")]
    [SerializeField] private TextMeshProUGUI thongKeComboText;
    [Tooltip("Btn_TiepTuc (Button) — chuyển màn tiếp theo")]
    [SerializeField] private Button btnTiepTuc;
    [Tooltip("Btn_XemVideo (Button) — xem QC nhân đôi thưởng")]
    [SerializeField] private Button btnXemVideo;

    [Header("=== Lose_Popup ===")]
    [Tooltip("Icon_Lose (Image)")]
    [SerializeField] private GameObject iconLose;
    [Tooltip("KhuVuc_LyDoThua (container)")]
    [SerializeField] private GameObject khuVucLyDoThua;
    [Tooltip("Text_LyDo_Chinh (TextMeshProUGUI) — hiển thị lý do thua")]
    [SerializeField] private TextMeshProUGUI lyDoThuaText;
    [Tooltip("Text_Meo_Tip (TextMeshProUGUI) — gợi ý (tùy chọn)")]
    [SerializeField] private TextMeshProUGUI meoTipText;
    [Tooltip("Btn_ChoiLai (Button) — chơi lại màn hiện tại")]
    [SerializeField] private Button btnReplay;
    [Tooltip("Btn_CuuTro (Button) — quay về MainMenu (tùy chọn)")]
    [SerializeField] private Button btnMainMenu;

    [Header("=== Sự kiện ngoài ===")]
    [Tooltip("Gán callback xử lý khi người chơi xem QC (VD: AdsManager). Nếu null, nút XemVideo chỉ log cảnh báo.")]
    public System.Action onWatchAdClicked;

    // Tránh gán listener trùng lặp khi ShowWinPopup/ShowLosePopup gọi lại.
    private bool listenersReady = false;

    protected override void Awake()
    {
        // BẮT BUỘC: gọi base.Awake() để SingletonBehaviour set EndGameUI.Instance.
        base.Awake();

        // Tự động tìm các reference theo tên (nếu chưa kéo thả) để giảm rủi ro thiếu ref.
        ResolveReferences();

        // Tự động ẩn cả 2 Popup khi bắt đầu Game.
        HideAllPanels();
    }

    private void Start()
    {
        SetupButtons();
    }

    // =====================================================================
    //  PUBLIC API
    // =====================================================================

    /// <summary>
    /// Hiển thị Popup THẮNG.
    /// - Bật Popup_Overlay (gameObject cha) để đảm bảo hiển thị.
    /// - Bật Win_Popup, ẩn Lose_Popup.
    /// - Tạm dừng game (Time.timeScale = 0f).
    /// - Cập nhật sao và bảng thống kê.
    /// </summary>
    /// <param name="stars">Số sao đạt được (0-3).</param>
    /// <param name="totalGold">Tổng vàng thưởng hiển thị trong Bang_Thong_Ke.</param>
    public void ShowWinPopup(int stars, int totalGold)
    {
        // Bật Popup_Overlay (cha) để đảm bảo UI hiển thị
        gameObject.SetActive(true);

        SetPanelActive(losePopup, false);
        SetPanelActive(winPopup, true);

        SetStars(stars);
        UpdateThongKe(totalGold, null, null);

        // Tạm dừng game
        Time.timeScale = 0f;

        SetupButtons();
    }

    /// <summary>
    /// Hiển thị Popup THUA.
    /// - Bật Popup_Overlay (gameObject cha) để đảm bảo hiển thị.
    /// - Bật Lose_Popup, ẩn Win_Popup.
    /// - Tạm dừng game (Time.timeScale = 0f).
    /// - Cập nhật lý do thua với null-check đầy đủ.
    /// </summary>
    /// <param name="reason">Lý do thua, VD: "Hết thời gian", "Chưa đạt chỉ tiêu tiền".</param>
    public void ShowLosePopup(string reason)
    {
        // Bật Popup_Overlay (cha) để đảm bảo UI hiển thị
        gameObject.SetActive(true);

        SetPanelActive(winPopup, false);
        SetPanelActive(losePopup, true);

        // Null-check an toàn cho text lý do thua
        if (lyDoThuaText != null)
        {
            lyDoThuaText.text = string.IsNullOrEmpty(reason) ? "Thua cuộc!" : reason;
        }
        else
        {
            Debug.LogWarning("EndGameUI.ShowLosePopup: lyDoThuaText chưa được gán.", this);
        }

        // Tạm dừng game
        Time.timeScale = 0f;

        SetupButtons();
    }

    /// <summary>
    /// Cập nhật chi tiết Bang_Thong_Ke (dòng Vàng / Khách / Combo).
    /// Truyền null để giữ nguyên text hiện tại của dòng đó.
    /// </summary>
    public void SetThongKe(string gold, string customers, string combo)
    {
        if (thongKeGoldText != null && gold != null) thongKeGoldText.text = gold;
        if (thongKeCustomerText != null && customers != null) thongKeCustomerText.text = customers;
        if (thongKeComboText != null && combo != null) thongKeComboText.text = combo;
    }

    /// <summary>
    /// Ẩn toàn bộ Popup. Gọi khi bắt đầu màn chơi mới.
    /// </summary>
    public void HideAllPanels()
    {
        SetPanelActive(winPopup, false);
        SetPanelActive(losePopup, false);
    }

    /// <summary>
    /// Wrapper tương thích với GameManager cũ (ShowEndGame(bool)).
    /// </summary>
    public void ShowEndGame(bool isWin)
    {
        if (isWin)
        {
            int gold = EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 0;
            ShowWinPopup(1, gold);
        }
        else
        {
            ShowLosePopup("Thua cuộc!");
        }
    }

    // =====================================================================
    //  BUTTON CALLBACKS (có thể wiring OnClick thủ công trong Inspector)
    // =====================================================================

    /// <summary>Chơi lại màn hiện tại (Btn_ChoiLai).</summary>
    public void OnReplayClicked()
    {
        Time.timeScale = 1f; // Reset time scale trước khi load lại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Chuyển sang màn tiếp theo (Btn_TiepTuc).</summary>
    public void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

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

    /// <summary>Xem quảng cáo nhân đôi thưởng (Btn_XemVideo).</summary>
    public void OnWatchAdClicked()
    {
        if (onWatchAdClicked != null)
        {
            onWatchAdClicked.Invoke();
        }
        else
        {
            Debug.LogWarning("EndGameUI: Btn_XemVideo được bấm nhưng chưa gán onWatchAdClicked. " +
                             "Nối vào AdsManager (gọi ShowWinPopup lại với vàng x2 sau khi xem xong).", this);
        }
    }

    /// <summary>Quay về MainMenu (Btn_CuuTro).</summary>
    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // =====================================================================
    //  SETUP BUTTONS
    // =====================================================================

    private void SetupButtons()
    {
        if (listenersReady) return;

        if (btnReplay != null)
            btnReplay.onClick.AddListener(OnReplayClicked);
        else
            Debug.LogWarning("EndGameUI: btnReplay (Btn_ChoiLai) chưa được gán.", this);

        if (btnTiepTuc != null)
            btnTiepTuc.onClick.AddListener(OnNextLevelClicked);
        else
            Debug.LogWarning("EndGameUI: btnTiepTuc (Btn_TiepTuc) chưa được gán.", this);

        if (btnXemVideo != null)
            btnXemVideo.onClick.AddListener(OnWatchAdClicked);
        else
            Debug.LogWarning("EndGameUI: btnXemVideo (Btn_XemVideo) chưa được gán.", this);

        if (btnMainMenu != null)
            btnMainMenu.onClick.AddListener(OnMainMenuClicked);

        listenersReady = true;
    }

    // =====================================================================
    //  HELPERS — HIỂN THỊ SAO & BẢNG THỐNG KÊ
    // =====================================================================

    private void SetStars(int stars)
    {
        if (starImages == null) return;

        stars = Mathf.Clamp(stars, 0, starImages.Length);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            bool lit = i < stars;
            Color c = starImages[i].color;
            c.a = lit ? 1f : 0.15f; // Sao đạt = sáng, chưa đạt = mờ
            starImages[i].color = c;
        }
    }

    private void UpdateThongKe(int totalGold, string customers, string combo)
    {
        if (thongKeGoldText != null)
        {
            thongKeGoldText.text = totalGold.ToString();
        }
        else
        {
            Debug.LogWarning("EndGameUI: thongKeGoldText chưa được gán (không hiển thị vàng thưởng).", this);
        }

        SetThongKe(null, customers, combo);
    }

    // =====================================================================
    //  AUTO-RESOLVE REFERENCES (tìm theo tên nếu chưa kéo thả)
    // =====================================================================

    private void ResolveReferences()
    {
        Transform root = transform;

        if (winPopup == null) winPopup = FindChild(root, "Win_Popup");
        if (losePopup == null) losePopup = FindChild(root, "Lose_Popup");

        Transform win = winPopup != null ? winPopup.transform : null;
        Transform lose = losePopup != null ? losePopup.transform : null;

        // --- Win ---
        if (iconWin == null && win != null) iconWin = FindChild(win, "icon_Win");
        if (khung3Sao == null && win != null) khung3Sao = FindChild(win, "Khung_3_Sao");

        if (starImages == null) starImages = new Image[3];

        if (khung3Sao != null)
        {
            if (starImages.Length >= 1 && starImages[0] == null)
                starImages[0] = GetComponentInChild<Image>(khung3Sao.transform, "Sao_1");
            if (starImages.Length >= 2 && starImages[1] == null)
                starImages[1] = GetComponentInChild<Image>(khung3Sao.transform, "Sao_2");
            if (starImages.Length >= 3 && starImages[2] == null)
                starImages[2] = GetComponentInChild<Image>(khung3Sao.transform, "Sao_3");
        }

        if (bangThongKe == null && win != null) bangThongKe = FindChild(win, "Bang_Thong_Ke");

        if (thongKeGoldText == null && bangThongKe != null)
            thongKeGoldText = GetComponentInChild<TextMeshProUGUI>(bangThongKe.transform, "Dong_Vang");
        if (thongKeCustomerText == null && bangThongKe != null)
            thongKeCustomerText = GetComponentInChild<TextMeshProUGUI>(bangThongKe.transform, "Dong_Khach");
        if (thongKeComboText == null && bangThongKe != null)
            thongKeComboText = GetComponentInChild<TextMeshProUGUI>(bangThongKe.transform, "Dong_Combo");

        // Gợi ý: Trong scene hiện tại, Dong_Vang/Dong_Khach/Dong_Combo chỉ chứa Icon_Vang (Image),
        // chưa có TextMeshProUGUI → cần tạo thêm 1 child Text (TMP) trong mỗi dòng nếu muốn hiển thị số thưởng.
        if (thongKeGoldText == null)
            Debug.LogWarning("EndGameUI: Không tìm thấy TextMeshProUGUI trong Bang_Thong_Ke/Dong_Vang. " +
                             "Hãy tạo 1 Text (TMP) bên trong Dong_Vang hoặc kéo thủ công vào field 'Thong Ke Gold Text'.", this);
        if (thongKeCustomerText == null)
            Debug.LogWarning("EndGameUI: Không tìm thấy TextMeshProUGUI trong Bang_Thong_Ke/Dong_Khach. " +
                             "Hãy tạo 1 Text (TMP) bên trong Dong_Khach hoặc kéo thủ công vào field 'Thong Ke Customer Text'.", this);
        if (thongKeComboText == null)
            Debug.LogWarning("EndGameUI: Không tìm thấy TextMeshProUGUI trong Bang_Thong_Ke/Dong_Combo. " +
                             "Hãy tạo 1 Text (TMP) bên trong Dong_Combo hoặc kéo thủ công vào field 'Thong Ke Combo Text'.", this);

        if (btnTiepTuc == null && win != null)
            btnTiepTuc = GetComponentInChild<Button>(win, "Btn_TiepTuc");
        if (btnXemVideo == null && win != null)
            btnXemVideo = GetComponentInChild<Button>(win, "Btn_XemVideo");

        // --- Lose ---
        if (iconLose == null && lose != null) iconLose = FindChild(lose, "Icon_Lose");
        if (khuVucLyDoThua == null && lose != null) khuVucLyDoThua = FindChild(lose, "KhuVuc_LyDoThua");

        if (lyDoThuaText == null && khuVucLyDoThua != null)
            lyDoThuaText = GetComponentInChild<TextMeshProUGUI>(khuVucLyDoThua.transform, "Text_LyDo_Chinh");
        if (meoTipText == null && khuVucLyDoThua != null)
            meoTipText = GetComponentInChild<TextMeshProUGUI>(khuVucLyDoThua.transform, "Text_Meo_Tip");

        if (btnReplay == null && lose != null)
            btnReplay = GetComponentInChild<Button>(lose, "Btn/Btn_ChoiLai");
        if (btnMainMenu == null && lose != null)
            btnMainMenu = GetComponentInChild<Button>(lose, "Btn/Btn_CuuTro");
    }

    // =====================================================================
    //  UTILITY
    // =====================================================================

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    /// <summary>Tìm GameObject con theo đường dẫn (path) từ parent.</summary>
    private static GameObject FindChild(Transform parent, string path)
    {
        if (parent == null) return null;
        Transform t = parent.Find(path);
        return t != null ? t.gameObject : null;
    }

    /// <summary>Tìm component loại T trên GameObject con theo path (bao gồm cả chính node con).</summary>
    private static T GetComponentInChild<T>(Transform parent, string path) where T : Component
    {
        GameObject child = FindChild(parent, path);
        if (child == null) return null;
        return child.GetComponent<T>() ?? child.GetComponentInChildren<T>();
    }
}

