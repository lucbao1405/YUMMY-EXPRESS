# TODO — Yummy Express Level 1 (Kiến trúc 5 Script)

> Xây dựng gameplay Level 1: người chơi làm Bánh Mì (Banh → Thit → Rau → CompleteBanhMi).
> Chỉ dùng đúng 5 script: FoodState, FoodItem, CookingProcess, GrillStation, Plate.

## Kế hoạch chi tiết

- [x] 0. Phân tích codebase cũ (Plate, PlateManager, IngredientButton, GrillSlot, FoodController).
- [x] 1. `Food/FoodState.cs`: giữ enum Raw / Cooked / Burnt.
- [x] 2. `Food/FoodItem.cs`: quản lý state + sprite + image. KHÔNG chứa timer.
- [x] 3. `Food/CookingProcess.cs`: dùng Coroutine nấu Raw→Cooked(4s)→Burnt(4s). Có `StartCooking()`, `StopCooking()`, `TryServeToPlate()` (tap Meat).
- [x] 4. `Kitchen/GrillStation.cs`: Singleton. Quản lý CookPoint, CurrentFood, `IsOccupied()`, `PlaceFood()`, `PlaceMeatOnGrill()` (nút Thit), `ClearStation()`.
- [x] 5. `Plate&Serving/Plate.cs`: Singleton. Quản lý BottomBread/TopBread/Meat/Vegetable/CompleteBanhMi. Có `PlaceBread()`, `PlaceMeat()`, `PlaceVegetable()`, `CheckCompleteBanhMi()`.
- [x] 6. Xoá các script cũ không còn dùng (không nằm trong 5 script):
      - `Food/FoodController.cs`
      - `UI/IngredientButton.cs`
      - `Kitchen/GrillSlot.cs`
      - `Plate&Serving/PlateManager.cs`

## Follow-up
- [ ] 7. Kiểm tra compile trong Unity (Console không còn lỗi).
- [ ] 8. Gắn script + wire Button trong Inspector (theo hướng dẫn).
- [ ] 9. Test gameplay: Banh → Thit → cook → Meat → Rau → CompleteBanhMi.

## Fix Plate UI không cập nhật (RefreshUI)
- [x] 10. Thêm 5 field `Image` (bottomBreadImage, topBreadImage, meatImage, vegetableImage, sauceImage) vào `Plate.cs`.
- [x] 11. Thêm `RefreshUI()` + `SetImageActive()` để bật/tắt Image theo state (gameObject.SetActive + image.enabled).
- [x] 12. Gọi `RefreshUI()` sau mỗi `PlaceBread()`, `PlaceMeat()`, `PlaceVegetable()`, `CheckCompleteBanhMi()`.
- [x] 13. Thêm `using UnityEngine.UI;` và field `sauceActive`.
- [ ] 14. Kiểm tra UnityEngine.UI.Image reference được gán đúng trên Inspector cho từng đĩa.

## Viết lại Plate.cs theo gameplay Yummy Express (4 layer)
- [x] 15. State `hasBread`, `hasMeat`, `hasVegetable`.
- [x] 16. 4 field Image: bottomBreadImage, vegetableImage, meatImage, topBreadImage.
- [x] 17. `PlaceBread()` — hiện cả Bottom + Top bread, chỉ 1 lần.
- [x] 18. `PlaceMeat(GameObject)` — chỉ khi có bánh + Cooked, kẹp giữa 2 ổ.
- [x] 19. `PlaceVegetable()` — chỉ khi có bánh, kẹp giữa 2 ổ.
- [x] 20. `IsComplete()`, `ClearPlate()`, `RefreshUI()` — chỉ bật/tắt Image, không tạo object mới.
- [x] 21. Giữ Singleton + `PlaceMeat(GameObject)` cho tương thích CookingProcess/GrillStation.
- [ ] 22. Gán 4 Image reference trên Inspector cho từng đĩa + test luồng.
