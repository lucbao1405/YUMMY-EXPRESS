# TODO — Auto-Wiring References trong Unity 2D YummyExpress (Hướng C)

> Giữ nguyên UIAutoWiring hiện tại, chỉ bổ sung các phần còn thiếu để toàn bộ
> UI tự nhận diện & hoạt động, KHÔNG cần kéo thả thủ công trên Inspector.

## Kế hoạch chi tiết

- [x] 0. Phân tích codebase (Plate, PlateManager, UIAutoWiring, CustomerSlotUI, GameManager, FoodData, spawners)
- [x] 1. Sửa `Assets/Scripts/Plate&Serving/Plate.cs`:
      - Thêm `public bool IsEmpty => currentFood == null;`
      - Thêm `public void TryPlaceFood(FoodData food)` (chỉ đặt khi đĩa trống)
      - Giữ nguyên auto-wire Image/Button trong Awake() — KHÔNG tự AddListener (tránh phục vụ 2 lần)
- [x] 2. Sửa `Assets/Scripts/Plate&Serving/PlateManager.cs`:
      - Thêm danh sách `List<Plate> plates` + quét `GetComponentsInChildren<Plate>(true)` trong Awake()
      - Thêm `public Plate GetEmptyPlate()` duyệt danh sách tìm đĩa có `IsEmpty == true`
      - Đảm bảo Instance singleton an toàn
- [x] 3. Tạo mới `Assets/Scripts/UI/IngredientButton.cs`:
      - Field `FoodData foodData` (người dùng tự gán trên Inspector)
      - Awake(): tự lấy Button (thêm nếu thiếu) + gắn listener `OnIngredientClicked`
      - Khi click: `PlateManager.Instance.GetEmptyPlate()` → nếu có → `plate.TryPlaceFood(foodData)`
      - Null-check an toàn ở mọi nơi
- [x] 4. Sửa `Assets/Scripts/Managers/UIAutoWiring.cs`:
      - Trong `WireIngredients()`: bỏ qua nút đã có component `IngredientButton` (tránh trùng listener)
- [x] 5. Sửa `Assets/Scripts/Customer/CustomerSlotUI.cs`:
      - Tự tìm `patienceBar` nếu null (child Fill / Patience_Bar)
      - Giữ nguyên logic ép Filled/Horizontal + đếm ngược + đổi màu + chớp nháy

## Follow-up
- [x] 6. Kiểm tra compile & toàn bộ flow auto-wiring (đĩa phục vụ, nguyên liệu đặt món, khách kiên nhẫn)
- [x] 7. Cập nhật tài liệu hướng dẫn (UI_SCENE_WIRING_CHECKLIST.md) cho phần IngredientButton

