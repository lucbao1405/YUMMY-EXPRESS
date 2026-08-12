# Hướng dẫn cấu hình hệ thống Level Selection (Phiên bản mới)

## 📋 Tổng quan
Đã viết lại hoàn toàn hệ thống Level Selection theo cấu trúc trong AGENTS.md với các cải tiến:
- Tự động tìm và gán sự kiện cho các nút level
- Hỗ trợ cả LevelConfigAsset (ScriptableObject) và LevelConfigs trực tiếp
- Đồng bộ với SaveSystem để quản lý unlock level
- UI hiển thị thông tin level đầy đủ trong Goals Panel

---

## 🔧 Cấu hình trong Unity Editor

### 1. Tạo LevelConfigAsset (ScriptableObject)

**Bước 1: Tạo asset**
1. Trong Unity Project window, right-click vào thư mục bất kỳ (đề nghị: `Assets/Resources/`)
2. Chọn: `Create > YummyExpress > Level Config Data`
3. Đặt tên file (ví dụ: `LevelConfigData`)

**Bước 2: Cấu hình dữ liệu level**
Trong Inspector của LevelConfigAsset:
- **LevelConfigs**: Thêm các level vào danh sách (+)
- **Level Index**: Số thứ tự level (1, 2, 3...) - tự động cập nhật
- **Level Name**: Tên hiển thị (ví dụ: "Level 1", "Màn 1")
- **Total Time**: Thời gian màn chơi (giây) - ví dụ: 60
- **Total Customers**: Tổng số khách dự kiến - ví dụ: 5
- **Target Gold**: Mục tiêu vàng để qua màn - ví dụ: 100
- **Max Missed Customers**: Số khách bỏ đi tối đa cho phép - ví dụ: 3
- **Spawn Timeline**: Thêm các mốc thời gian spawn khách
  - **Spawn Time**: Thời điểm spawn (giây từ đầu màn)
  - **Order Foods**: Danh sách món khách gọi (có thể để trống để random)
  - **Description**: Mô tả để dễ chỉnh sửa

---

### 2. Cấu hình LevelManager trong Scene (LevelSelection)

**Bước 1: Tìm hoặc tạo GameObject**
1. Trong scene LevelSelection, tìm hoặc tạo GameObject có tên `LevelManager`
2. Add component `LevelManager` vào GameObject này

**Bước 2: Gán tham chiếu trong Inspector**
- **Level Config Asset**: Kéo thả file LevelConfigAsset đã tạo ở bước 1 vào đây (ƯU TIÊN)
- **Customer Spawner**: Kéo thả GameObject CustomerSpawner vào (nếu có trong scene)
- **Game Manager**: Kéo thả GameObject GameManager vào (nếu có trong scene)
- **Customer Pool**: Thêm các CustomerData vào danh sách khách có thể spawn
- **Bread Variants**: Kéo thả các FoodData cho bánh mì và cà phê (nếu có)

**Lưu ý:**
- Nếu không gán LevelConfigAsset, LevelManager sẽ sử dụng levelConfigs trực tiếp (fallback)
- Đảm bảo ít nhất một trong hai nguồn dữ liệu được cấu hình

---

### 3. Cấu hình LevelSelectUI trong Scene (LevelSelection)

**Bước 1: Tìm hoặc tạo GameObject**
1. Trong scene LevelSelection, tìm hoặc tạo GameObject có script `LevelSelectUI`
2. Thường gắn vào Canvas hoặc GameObject UI chính

**Bước 2: Gán tham chiếu trong Inspector**

**UI References:**
- **Goals Panel**: Kéo thả GameObject Popup Goals vào đây
- **Gold Text**: Kéo thả TextMeshProUGUI hiển thị mục tiêu vàng vào đây
- **Time Text**: Kéo thả TextMeshProUGUI hiển thị thời gian vào đây
- **AVT Text**: Kéo thả TextMeshProUGUI hiển thị số khách bỏ đi vào đây
- **Level Name Text**: Kéo thả TextMeshProUGUI hiển thị tên level vào đây
- **Total Customers Text**: Kéo thả TextMeshProUGUI hiển thị tổng số khách vào đây

**Buttons:**
- **Play Button**: Kéo thả nút Play trong goals panel vào đây
- **Close Button**: Kéo thả nút Close trong goals panel vào đây

**Level Buttons (Tự động tìm):**
- **Level Button Prefix**: Đặt prefix tên các nút level (mặc định: "Btn_Lever_")
- **Max Level Buttons**: Số lượng level button tối đa cần tìm (mặc định: 5)

**Lưu ý:**
- Script sẽ tự động tìm các nút theo naming convention: `Btn_Lever_1`, `Btn_Lever_2`, v.v.
- Script sẽ tự động gán sự kiện click cho các nút tìm thấy
- Các nút Play và Close cũng được tự động gán sự kiện

---

### 4. Đặt tên các GameObject trong Scene

**Quan trọng:** Script tự động tìm các nút level theo tên, đảm bảo đặt tên đúng:

**Nút level:**
- `Btn_Lever_1` (cho Level 1)
- `Btn_Lever_2` (cho Level 2)
- `Btn_Lever_3` (cho Level 3)
- ... đến `Btn_Lever_5` (hoặc theo số lượng maxLevelButtons)

**Panel Goals:**
- GameObject chứa panel hiển thị mục tiêu level
- Có thể đặt tên bất kỳ, nhưng nên là `GoalsPanel` hoặc `Goals_1`

**Text components:**
- Các TextMeshProUGUI trong Goals Panel:
  - Tên level (optional)
  - Mục tiêu vàng
  - Thời gian
  - Số khách bỏ đi (AVT)
  - Tổng số khách

---

## 🎯 Luồng hoạt động

### 1. Người chơi vào scene LevelSelection
- `LevelSelectUI.Start()` được gọi
- Script tự động tìm các nút level theo naming convention
- Script tự động gán sự kiện click cho các nút
- Script cập nhật trạng thái unlock dựa trên SaveSystem
- Goals Panel được ẩn mặc định

### 2. Người chơi click vào nút level
- `OnClickLevelButton(int levelIndex)` được gọi
- Script lấy LevelConfigData từ LevelManager
- Script cập nhật các Text trong Goals Panel:
  - Level Name: Tên level
  - Gold: "0/{targetGold}"
  - Time: "{totalTime}s"
  - AVT: "{totalCustomers}"
  - Total Customers: "{totalCustomers}"
- Goals Panel được hiển thị

### 3. Người chơi click nút Play
- `OnClickPlay()` được gọi
- Script lưu selectedLevelIndex vào PlayerPrefs ("SelectedLevel")
- Script cũng lưu vào SaveSystem để đảm bảo tương thích
- Script load scene "MAN1"

### 4. Khi vào scene MAN1
- `GameManager.Start()` đọc PlayerPrefs "SelectedLevel"
- Nếu có level được chọn, dùng level đó
- Nếu không, load level gần nhất từ SaveSystem
- GameManager gọi `StartLevel(levelIndex)`

---

## 🔍 Debug và Troubleshooting

### Kiểm tra log trong Console:
Script có đầy đủ log để debug:
- `LevelSelectUI: Start - Khởi tạo hệ thống chọn level`
- `LevelSelectUI: Tìm thấy Btn_Lever_X`
- `LevelSelectUI: Đã gán sự kiện click cho Btn_Lever_X -> levelIndex X`
- `LevelSelectUI: Level X - Unlocked: true/false`
- `LevelSelectUI: Click level button - Index: X`
- `LevelSelectUI: Level config - Name: ..., Gold: ..., Time: ..., Customers: ...`
- `LevelSelectUI: Đã chọn level X, đang load scene MAN1`

### Các lỗi thường gặp:

**Lỗi: "Không tìm thấy GameObject Btn_Lever_X"**
- Kiểm tra xem GameObject có đúng tên trong scene không
- Kiểm tra xem GameObject có nằm trong cùng Canvas không
- Kiểm tra xem levelButtonPrefix có đúng không

**Lỗi: "LevelManager.Instance không tồn tại"**
- Đảm bảo GameObject LevelManager có trong scene LevelSelection
- Đảm bảo component LevelManager được gán
- Đảm bảo LevelConfigAsset hoặc levelConfigs được cấu hình

**Lỗi: "Không thể lấy LevelConfigData cho index X"**
- Kiểm tra xem LevelConfigAsset có đủ level configs không
- Kiểm tra xem levelIndex có nằm trong phạm vi không
- Kiểm tra xem level configs có được cấu hình đúng không

**Lỗi: "Goals Panel không hiển thị"**
- Kiểm tra xem goalsPanel reference có được gán không
- Kiểm tra xem GameObject có active không
- Kiểm tra xem Panel có nằm trong Canvas đúng không

---

## 📝 Tóm tắt các file đã sửa/tạo mới

### File đã sửa:
1. **LevelSelectUI.cs** - Viết lại hoàn toàn hệ thống chọn level
2. **LevelManager.cs** - Thêm hỗ trợ LevelConfigAsset
3. **MainMenuUI.cs** - Sửa scene name từ "UI" thành "LevelSelection"
4. **LevelLoader.cs** - Thêm method LoadLevelByName

### File mới tạo:
1. **LevelConfigAsset.cs** - ScriptableObject để quản lý level configs

---

## 🎨 Tùy chỉnh nâng cao

### Thêm nhiều level hơn:
1. Tăng `maxLevelButtons` trong LevelSelectUI Inspector
2. Đảm bảo naming convention đúng (Btn_Lever_6, Btn_Lever_7, v.v.)
3. Thêm level configs tương ứng trong LevelConfigAsset

### Thay đổi naming convention:
1. Đổi `levelButtonPrefix` trong LevelSelectUI Inspector
2. Đặt tên lại các GameObject nút level trong scene

### Sử dụng logic unlock tùy chỉnh:
1. Modify `UpdateLevelButtonStates()` trong LevelSelectUI.cs
2. Thay thế `SaveSystem.IsLevelUnlocked(i)` bằng logic tùy chỉnh

---

## ✅ Checklist trước khi test

- [ ] Đã tạo LevelConfigAsset trong thư mục Resources
- [ ] Đã cấu hình ít nhất 1 level trong LevelConfigAsset
- [ ] Đã gán LevelConfigAsset vào LevelManager trong scene LevelSelection
- [ ] Đã gán các UI references vào LevelSelectUI trong scene LevelSelection
- [ ] Đã đặt tên đúng các nút level (Btn_Lever_1, Btn_Lever_2, v.v.)
- [ ] Đã có GameObject LevelManager trong scene LevelSelection
- [ ] Scene "MAN1" đã được thêm vào Build Settings
- [ ] Đã test flow: Mainmenu → LevelSelection → MAN1

---

## 🆘 Hỗ trợ

Nếu gặp vấn đề:
1. Kiểm tra Console log để xem lỗi cụ thể
2. Đảm bảo tất cả references được gán đúng trong Inspector
3. Kiểm tra naming convention của các GameObject
4. Verify scene "MAN1" có trong Build Settings
5. Test từng bước: Mainmenu → LevelSelection → Click level → Click Play → MAN1
