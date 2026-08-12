# Hướng dẫn cấu hình tính năng Level Selection trong Unity

## 1. Cấu hình LevelConfigData (ScriptableObject)

### Cách tạo LevelConfigAsset:
1. Trong Unity Project window, right-click vào thư mục Resources (hoặc thư mục bất kỳ)
2. Chọn: `Create > YummyExpress > Level Config Data`
3. Đặt tên file (ví dụ: `LevelConfigData`)

### Cấu hình dữ liệu level:
Trong Inspector của LevelConfigAsset:
- **LevelConfigs**: Thêm các level vào danh sách
- **Level Index**: Số thứ tự level (1, 2, 3...)
- **Level Name**: Tên hiển thị (ví dụ: "Level 1", "Màn 1")
- **Total Time**: Thời gian màn chơi (giây) - ví dụ: 60
- **Total Customers**: Tổng số khách dự kiến - ví dụ: 5
- **Target Gold**: Mục tiêu vàng để qua màn - ví dụ: 100
- **Max Missed Customers**: Số khách bỏ đi tối đa cho phép - ví dụ: 3
- **Spawn Timeline**: Thêm các mốc thời gian spawn khách
  - **Spawn Time**: Thời điểm spawn (giây từ đầu màn)
  - **Order Foods**: Danh sách món khách gọi (có thể để trống để random)
  - **Description**: Mô tả để dễ chỉnh sửa

## 2. Cấu hình LevelManager trong Scene

### Làm theo các bước sau:
1. Trong scene LevelSelection, tìm hoặc tạo GameObject có tên `LevelManager`
2. Add component `LevelManager` vào GameObject này
3. Trong Inspector của LevelManager:
   - **Level Config Asset**: Kéo thả file LevelConfigAsset đã tạo ở bước 1 vào đây
   - **Customer Spawner**: Kéo thả GameObject CustomerSpawner vào (nếu có)
   - **Game Manager**: Kéo thả GameObject GameManager vào (nếu có)
   - **Customer Pool**: Thêm các CustomerData vào danh sách khách có thể spawn
   - **Bread Variants**: Kéo thả các FoodData cho bánh mì và cà phê (nếu có)

## 3. Cấu hình LevelSelectUI trong Scene

### Làm theo các bước sau:
1. Trong scene LevelSelection, tìm hoặc tạo GameObject có tên `LevelSelectUI` (hoặc gắn vào Canvas)
2. Add component `LevelSelectUI` vào GameObject này
3. Trong Inspector của LevelSelectUI:
   - **Goals Panel**: Kéo thả GameObject Popup Goals vào đây
   - **Gold Text**: Kéo thả TextMeshProUGUI hiển thị mục tiêu vàng vào đây
   - **Time Text**: Kéo thả TextMeshProUGUI hiển thị thời gian vào đây
   - **Avt Text**: Kéo thả TextMeshProUGUI hiển thị số khách bỏ đi vào đây

## 4. Gắn sự kiện (On Click) cho các nút

### Cấu hình nút chọn level (Btn_Level_1, Btn_Level_2, ...):
1. Chọn nút Btn_Level_1
2. Trong Inspector, tìm component `Button`
3. Trong phần `On Click ()`:
   - Click dấu `+` để thêm event mới
   - Kéo thả GameObject có script `LevelSelectUI` vào ô object
   - Trong dropdown, chọn: `LevelSelectUI > OnClickLevelButton`
   - Nhập số vào ô argument: `0` (cho Level 1), `1` (cho Level 2), v.v.

### Cấu hình nút Play trong bảng Goals:
1. Chọn nút Play trong bảng Goals
2. Trong Inspector, tìm component `Button`
3. Trong phần `On Click ()`:
   - Click dấu `+` để thêm event mới
   - Kéo thả GameObject có script `LevelSelectUI` vào ô object
   - Trong dropdown, chọn: `LevelSelectUI > OnClickPlay`

### (Tùy chọn) Cấu hình nút Close trong bảng Goals:
1. Chọn nút Close trong bảng Goals
2. Trong Inspector, tìm component `Button`
3. Trong phần `On Click ()`:
   - Click dấu `+` để thêm event mới
   - Kéo thả GameObject có script `LevelSelectUI` vào ô object
   - Trong dropdown, chọn: `LevelSelectUI > OnClickCloseGoals`

## 5. Tóm tắt luồng hoạt động

1. Người chơi click vào nút chọn level (Btn_Level_X)
2. `OnClickLevelButton(int levelIndex)` được gọi với index tương ứng
3. Script lấy LevelConfigData từ LevelManager.Instance
4. Cập nhật các Text hiển thị: Gold (0/target), Time (XXs), Max Missed (X)
5. Bật bảng Goals lên
6. Người chơi click nút Play
7. `OnClickPlay()` lưu selectedLevelIndex vào PlayerPrefs
8. Load scene "MAN1"

## 6. Lưu ý quan trọng

- Đảm bảo LevelManager được cấu hình đúng với LevelConfigAsset
- Đảm bảo các Text reference trong LevelSelectUI được gán đúng
- Scene "MAN1" phải tồn tại trong Build Settings
- Nếu dùng PlayerPrefs để load level trong scene game, nhớ đọc key "SelectedLevelIndex" khi start game
