using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý giao diện Thắng/Thua (Win_Popup / Lose_Popup) trong Popup_Overlay.
/// Số sao hiển thị bằng StarDisplayController (3 ô Image cố định, đổi Sprite Vàng/Xám).
/// Bảng thống kê (Vàng / Khách / Combo) được hiển thị trên 3 dòng cố định riêng biệt
/// để tránh chữ đè lên Icon.
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
    [Tooltip("StarDisplayController (3 ô sao cố định) — nếu null sẽ tự tìm trong Win_Popup")]
    [SerializeField] private StarDisplayController starDisplay;
    [Tooltip("Bang_Thong_Ke (container thống kê thưởng)")]
    [SerializeField] private GameObject bangThongKe;
    [Tooltip("Text số Vàng trong Dong_Vang (TextMeshProUGUI)")]
    [SerializeField] private TextMeshProUGUI thongKeGoldText;
    [Tooltip("Text số Khách trong Dong_Khach (TextMeshProUGUI) — tùy chọn")]
    [SerializeField] private TextMeshProUGUI thongKeCustomerText;
    [Tooltip("Text số khách giận bỏ về trong Dong_Khach_Gian (TextMeshProUGUI) — tùy chọn")]
    [SerializeField] private TextMeshProUGUI thongKeAngryText;
    [Tooltip("Text số Combo trong Dong_Combo (TextMeshProUGUI) — tùy chọn")]
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
    [Tooltip("Btn_Home (Button) trong Lose_Popup — quay về MainMenu")]
    [SerializeField] private Button btnHomeLose;

    [Header("=== Sự kiện ngoài ===")]
    [Tooltip("Gán callback xử lý khi người chơi xem QC (VD: AdsManager). Nếu null, nút XemVideo chỉ log cảnh báo.")]
    public System.Action onWatchAdClicked;

    // Tránh gán listener trùng lặp khi ShowWinPopup/ShowLosePopup gọi lại.
private bool listenersReady = false;

    protected override void Awake()
    {
        // BẮT BUỘC: gọi base.Awake() để SingletonBehaviour set EndGameUI.Instance.
        base.Awake();

        // Đăng ký lắng nghe sự kiện GameOver NGAY trong Awake (thay vì OnEnable).
        // LÝ DO: Popup_Overlay (GameObject chứa EndGameUI) bị tắt lúc bắt đầu scene,
        // nên OnEnable/OnDisable/Start không bao giờ chạy → EndGameUI không đăng ký
        // được sự kiện → popup không hiện khi thắng/thua. Đăng ký trong Awake đảm bảo
        // luôn lắng nghe dù GameObject cha bị tắt.

        // Tự động tìm các reference theo tên (nếu chưa kéo thả) để giảm rủi ro thiếu ref.
        ResolveReferences();

        // Tự động ẩn cả 2 Popup khi bắt đầu Game.
        HideAllPanels();
    }

    protected override void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh rò rỉ listener khi scene bị hủy.
        base.OnDestroy();
    }

    private void Start()
    {
        SetupButtons();
    }

    private void OnEnable()
    {
        GameManager.GameOver += OnGameOver;
        GameManager.OnLevelCleared += OnLevelCleared;
    }

    private void OnDisable()
    {
        GameManager.GameOver -= OnGameOver;
        GameManager.OnLevelCleared -= OnLevelCleared;
    }

    private void OnGameOver(GameOverData result)
    {
        if (!result.IsWin)
        {
            ShowLosePopup(result.LoseReason);
        }
    }

    private void OnLevelCleared(WinData data)
    {
        ShowWinPopup(data.stars, data.gold, 0, 0, data.combos);
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
    /// <param name="stars">Số sao đạt được (1-3).</param>
    /// <param name="totalGold">Tổng vàng thưởng hiển thị trong Bang_Thong_Ke.</param>
    public void ShowWinPopup(int stars, int totalGold)
    {
        ShowWinPopup(stars, totalGold, 0, 0, 0);
    }

    /// <summary>
    /// Hiển thị Popup THẮNG và cập nhật đầy đủ UI (sao + bảng thống kê).
    /// </summary>
    /// <param name="stars">Số sao đạt được (1-3).</param>
    /// <param name="totalGold">Tổng vàng kiếm được trong màn.</param>
    /// <param name="servedCustomers">Số khách đã phục vụ thành công.</param>
    /// <param name="totalCustomers">Tổng số khách dự kiến trong level.</param>
    /// <param name="maxCombo">Combo cao nhất đạt được.</param>
    public void ShowWinPopup(int stars, int totalGold, int servedCustomers, int totalCustomers, int maxCombo)
    {
        ShowWinPopup(stars, totalGold, servedCustomers, totalCustomers, maxCombo, 0);
    }

    public void ShowWinPopup(int stars, int totalGold, int servedCustomers, int totalCustomers, int maxCombo, int angryCustomers)
    {
        // Bật Popup_Overlay (cha) để đảm bảo UI hiển thị
        gameObject.SetActive(true);

        SetPanelActive(losePopup, false);
        SetPanelActive(winPopup, true);

        // Căn giữa popup trên màn hình (bỏ qua giá trị y lệch trong scene).
        CenterPopup(winPopup);

        // Cập nhật đầy đủ: số sao + bảng thống kê (Vàng / Khách / Combo).
        UpdateWinUI(stars, totalGold, servedCustomers, totalCustomers, maxCombo, angryCustomers);

        // Tạm dừng game
        Time.timeScale = 0f;

        SetupButtons();
    }

    /// <summary>
    /// Hiển thị Popup THUA.
    /// </summary>
    /// <param name="reason">Lý do thua, VD: "Hết thời gian", "Chưa đạt chỉ tiêu tiền".</param>
    public void ShowLosePopup(string reason)
    {
        // Bật Popup_Overlay (cha) để đảm bảo UI hiển thị
        gameObject.SetActive(true);

        SetPanelActive(winPopup, false);
        SetPanelActive(losePopup, true);

        // Căn giữa popup trên màn hình (bỏ qua giá trị y lệch trong scene).
        CenterPopup(losePopup);

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
    /// Cập nhật toàn bộ UI Win_Popup: số sao + bảng thống kê (Vàng / Khách / Combo).
    ///   - Sao: delegate cho StarDisplayController (3 ô cố định, đổi Sprite Vàng/Xám).
    ///   - Vàng: hiển thị số vàng ở Dong_Vang.
    ///   - Khách: hiển thị "x/y" ở Dong_Khach.
    ///   - Combo: hiển thị "xN" ở Dong_Combo (KHÔNG kèm chữ "Combo" để tránh đè Icon).
    /// </summary>
    /// <param name="stars">Số sao đạt được (1-3).</param>
    /// <param name="totalGold">Tổng vàng kiếm được trong màn (hiện ở Dong_Vang).</param>
    /// <param name="servedCustomers">Số khách đã phục vụ thành công.</param>
    /// <param name="totalCustomers">Tổng số khách dự kiến trong level.</param>
    /// <param name="maxCombo">Combo cao nhất đạt được (hiện "x{maxCombo}" ở Dong_Combo).</param>
    public void UpdateWinUI(int stars, int totalGold, int servedCustomers, int totalCustomers, int maxCombo, int angryCustomers)
    {
        // 1. Cập nhật số sao qua StarDisplayController (đổi Sprite Vàng/Xám trên 3 ô cố định).
        if (starDisplay != null)
        {
            starDisplay.SetStars(stars);
        }
        else
        {
            Debug.LogWarning("EndGameUI.UpdateWinUI: starDisplay chưa được gán/tìm thấy → không hiển thị sao.", this);
        }

        // 2. Bảng thống kê — mỗi số nằm trên dòng riêng (không kèm nhãn để tránh đè Icon).
        if (thongKeGoldText != null)
        {
            thongKeGoldText.text = totalGold.ToString();
        }

        if (thongKeCustomerText != null)
        {
            thongKeCustomerText.text = $"{servedCustomers}/{totalCustomers}";
        }

        if (thongKeAngryText != null)
        {
            thongKeAngryText.text = angryCustomers.ToString();
        }

        if (thongKeComboText != null)
        {
            thongKeComboText.text = $"x{maxCombo}";
        }

        Debug.Log($"<color=cyan>[ENDGAME-UI] Win: {stars} sao | Vàng {totalGold} | Khách {servedCustomers}/{totalCustomers} | Angry {angryCustomers} | Combo x{maxCombo}</color>");
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
            TryLoadScene("Mainmenu");
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
        TryLoadScene("Mainmenu");
    }

    private bool TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("EndGameUI.TryLoadScene: sceneName trống.");
            return false;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                SceneManager.LoadScene(name);
                return true;
            }
        }

        Debug.LogWarning($"EndGameUI: Scene '{sceneName}' chưa được thêm vào Build Settings hoặc sai chính tả. Hãy kiểm tra File > Build Settings.", this);
        return false;
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

        if (btnHomeLose != null)
            btnHomeLose.onClick.AddListener(OnMainMenuClicked);
        else
            Debug.LogWarning("EndGameUI: btnHomeLose (Btn_Home trong Lose_Popup) chưa được gán.", this);

        listenersReady = true;
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

        // Tự tìm StarDisplayController trong Win_Popup (fallback nếu chưa kéo thả).
        if (starDisplay == null && win != null)
        {
            starDisplay = win.GetComponentInChildren<StarDisplayController>(true);
        }

        if (bangThongKe == null && win != null) bangThongKe = FindChild(win, "Bang_Thong_Ke");

        // ✅ Tự động tìm TextMeshProUGUI trong từng dòng của Bang_Thong_Ke.
        // Nếu Dong_Vang/Dong_Khach/Dong_Combo CHƯA có TMP (chỉ có Icon_Vang Image),
        // script sẽ tự TẠO 1 child Text (TMP) để hiển thị số thưởng — không bắn Warning spam.
        if (thongKeGoldText == null && bangThongKe != null)
            thongKeGoldText = EnsureThongKeText(bangThongKe.transform, "Dong_Vang", "Vàng");
        if (thongKeCustomerText == null && bangThongKe != null)
            thongKeCustomerText = EnsureThongKeText(bangThongKe.transform, "Dong_Khach", "Khách");
        if (thongKeAngryText == null && bangThongKe != null)
            thongKeAngryText = EnsureThongKeText(bangThongKe.transform, "Dong_Khach_Gian", "Khách Giận");
        if (thongKeComboText == null && bangThongKe != null)
            thongKeComboText = EnsureThongKeText(bangThongKe.transform, "Dong_Combo", "Combo");

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

        // Nút HOME trong Lose_Popup (Btn_Home) — quay về MainMenu.
        // Fallback tìm theo tên "Btn_Home" nếu chưa được kéo thả.
        if (btnHomeLose == null && lose != null)
            btnHomeLose = GetComponentInChild<Button>(lose, "Btn_Home");
        if (btnHomeLose == null && win != null)
            btnHomeLose = GetComponentInChild<Button>(win, "Btn_Home");
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

    /// <summary>
    /// Căn giữa popup trên màn hình.
    /// Popup có AnchorMin/Max = (0.5, 0.5) nên chỉ cần đặt anchoredPosition về (0,0)
    /// để tâm popup trùng tâm Canvas (bỏ qua giá trị y lệch dương trong scene).
    /// Không đổi pivot/scale/anchor của popup.
    /// </summary>
    private static void CenterPopup(GameObject panel)
    {
        if (panel == null) return;

        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) return;

        Vector2 pos = rt.anchoredPosition;
        pos.x = 0f;
        pos.y = 0f;
        rt.anchoredPosition = pos;
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

    /// <summary>
    /// Tìm (hoặc TỰ TẠO) TextMeshProUGUI trong một dòng của Bang_Thong_Ke.
    /// - Ưu tiên: chính node dòng có TMP → dùng luôn.
    /// - Kế tiếp: con/cháu của dòng có TMP → dùng TMP đầu tiên tìm được.
    /// - Không có: tự tạo 1 child GameObject "Text_<label>" gắn TextMeshProUGUI vào dòng
    ///   (tránh bắn Warning spam khi scene chưa được dựng TMP thủ công).
    /// </summary>
    /// <param name="rowParent">Transform của dòng (VD: Bang_Thong_Ke/Dong_Vang).</param>
    /// <param name="rowName">Tên dòng để log (VD: "Dong_Vang").</param>
    /// <param name="label">Nhãn hiển thị mặc định khi tạo mới (VD: "Vàng").</param>
    /// <returns>TextMeshProUGUI đã tìm/tạo, hoặc null nếu không thể tạo.</returns>
    private static TextMeshProUGUI EnsureThongKeText(Transform rowParent, string rowName, string label)
    {
        if (rowParent == null) return null;

        // 1. Chính node dòng có TMP không?
        TextMeshProUGUI tmp = rowParent.GetComponent<TextMeshProUGUI>();
        // 2. Ngược lại tìm TMP trong con/cháu (VD: nếu User đã kéo 1 Text TMP vào dòng).
        if (tmp == null) tmp = rowParent.GetComponentInChildren<TextMeshProUGUI>();

        if (tmp != null) return tmp;

        // 3. Không có TMP → TỰ TẠO 1 child Text (TMP) trong dòng.
        try
        {
            GameObject textGO = new GameObject(
                $"Text_{label}",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            textGO.transform.SetParent(rowParent, false);

            // Stretch to fill dòng (để text căn giữa theo LayoutGroup).
            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            TextMeshProUGUI newTmp = textGO.GetComponent<TextMeshProUGUI>();
            newTmp.text = label;               // Hiển thị nhãn mặc định
            newTmp.fontSize = 36f;             // Font size vừa phải cho dòng thống kê
            newTmp.alignment = TextAlignmentOptions.Center;
            newTmp.color = Color.white;
            newTmp.raycastTarget = false;      // Không chặn click vào panel

            Debug.Log($"EndGameUI: Không tìm thấy TMP trong {rowName} — đã tự tạo child 'Text_{label}'. " +
                      "Bạn có thể thay font/size/màu ngay trên Inspector.", textGO);
            return newTmp;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"EndGameUI: Không thể tự tạo TMP cho {rowName}. Lỗi: {e.Message}");
            return null;
        }
    }
}
