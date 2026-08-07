# TODO — ScoreManager (Tính sao theo Độ hài lòng + Combo Vàng)

## Steps
- [x] Phân tích code hiện có (GameManager, ServingManager, CustomerManager, CustomerSlotUI, SaveSystem, EndGameUI)
- [x] Tạo/sửa `Assets/Scripts/Managers/ScoreManager.cs` (logic tính điểm sao + sự kiện `OnStarsCalculated`)
- [x] Tích hợp `OnCustomerServed` từ `GameManager.ServeFoodToCustomer` (đọc `RemainingPatiencePercent` của slot)
- [x] Tích hợp `OnCustomerLeftAngry` từ `CustomerManager.NotifyCustomerLeft`
- [x] `GameManager.StartLevel` gọi `ScoreManager.InitializeLevel(currentLevel.totalCustomers)`
- [x] `GameManager.EndGame` gọi `CalculateAndDisplayStars()` + `SaveSystem.SaveLevelStars(...)` + `ShowWinPopup(...)`
- [x] Bổ sung property `RemainingPatiencePercent` trong `CustomerSlotUI`
- [x] `StarDisplayController` — cơ chế ĐỔI SPRITE Vàng/Xám trên 3 ô Image cố định, clamp sao trong [1,3] để không bao giờ hiện 0 khi Win
- [x] `ScoreManager.CalculateStars()` — đảm bảo trả về 1-3 sao dựa trên Tỷ lệ phục vụ khách (≥100%→3, ≥75%→2, còn lại→1)
- [x] `EndGameUI` — dùng `StarDisplayController`, hiển thị số vàng/khách/combo trên 3 dòng riêng (bỏ chữ "Combo" thừa để tránh đè Icon)
- [x] Xóa hoàn toàn cơ chế 2 khung (Khung_3_Sao_Toi/activeStars/darkStars) khỏi EndGameUI
- [x] `StarDisplayController` — đổi logic hiển thị sao TRÁI→PHẢI (Index 0,1,2): 1 Sao=Vàng-Xám-Xám, 2 Sao=Vàng-Vàng-Xám, 3 Sao=Vàng-Vàng-Vàng
- [x] `StarDisplayController` — chuẩn hóa `SetStars()` clamp [1,3] + tự tìm 3 ô Sao (Sao_1/2/3) trong toàn cây con để tránh lỗi 0 sao khi thắng
- [x] `StarDisplayController` — SỬA LỖI "0 sao": tự tìm 3 ô Sao CHỈ trong `Khung_3_Sao` (không dò `Khung_3_Sao_Toi` vì trùng tên Sao_1/2/3) + tự nạp 2 Sprite Sao Vàng/Xám theo GUID
- [x] `StarDisplayController` — NGUYÊN NHÂN GỐC: `Khung_3_Sao_Toi` (chứa 3 sao XÁM) là sibling SAU `Khung_3_Sao` nên render ĐÈ LÊN sao vàng → luôn hiện 3 sao xám. Đã thêm `HideDarkOverlay()` ẩn `Khung_3_Sao_Toi` trong Awake + SetStars
- [x] **SỬA LỖI** "thắng luôn full sao": đổi cách tính `CalculateStars()` từ serveRatio sang **Độ hài lòng trung bình**:
  - Dùng `averageSatisfaction = TotalEarnedPoints / (TotalCustomers × 3)` — dựa trên điểm kiên nhẫn thuần (mỗi khách max 3 điểm).
  - ≥ 70% → 3 sao
  - 40% → 69% → 2 sao
  - < 40% → 1 sao
- [x] **SỬA LỖI** "thua không có sao": thêm `ScoreManager.DisplayNoStars()` → `starDisplay.SetStars(0)` gọi trong `GameManager.EndGame(false)`
- [x] **THÊM TÍNH NĂNG**: Combo thưởng Vàng +5/10/15/20 trong `ScoreManager.OnCustomerServed()` — gọi `EconomyManager.Instance.AddGold(comboGold)` (x1→5, x2→10, x3→15, x4+→20), KHÔNG thêm vào điểm hài lòng để giữ "độ hài lòng trung bình" phản ánh đúng chất lượng phục vụ.

# YUM-242: Luồng lưu trạng thái 1-3 sao khi thắng Level

## Steps
- [x] Khi chiến thắng Level → `GameManager.EndGame(true)` lấy `currentStars` từ `ScoreManager.CalculateAndDisplayStars()`.
- [x] `SaveSystem.SaveLevelStars(levelIndex, stars)` — đã tự so sánh với `savedStars` và lưu `bestStars = Mathf.Max(savedStars, currentStars)` (giữ kỷ lục cao nhất).
- [x] Dữ liệu lưu vào file JSON (persistentDataPath/player_data.json) qua class `SaveSystem` (YUM-240).
- [x] `SaveSystem.GetLevelStars(levelIndex)` — tải số sao kỷ lục đã lưu.
- [x] Tự động mở khóa Level kế tiếp: thêm `SaveSystem.UnlockNextLevel(currentLevelIndex + 1)` trong nhánh thắng → level sau được thêm vào `unlockedLevels`; người chơi vào qua `Btn_TiepTuc` (EndGame UI → `OnNextLevelClicked`).
- [x] Kiểm tra trạng thái mở khóa: `SaveSystem.IsLevelUnlocked(levelIndex)` — sẵn sàng cho MainMenu/Level Select UI dùng.
- [x] **PERSISTENCE (Nhớ khi vào game lần sau)**:
  - `SaveSystem.UnlockNextLevel()` giờ cũng cập nhật `data.currentLevel` lên level mở khóa mới nhất.
  - `SaveSystem.GetCurrentLevel()` — đọc level hiện tại (1-based) từ file JSON.
  - `GameManager.Start()` gọi `SaveSystem.GetCurrentLevel()` → `StartLevel(savedLevel - 1)` để khôi phục đúng level khi mở game lần sau.
  - Dữ liệu ghi xuống `Application.persistentDataPath/player_data.json` → tồn tại qua nhiều lần mở/đóng game.


