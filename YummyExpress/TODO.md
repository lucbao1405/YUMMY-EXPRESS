# TODO - Customer Slot UI Slide-In Animation

## Steps
- [x] 1. `CustomerSlotUI.cs`: Thêm `using System.Collections`, field `moveDuration`, `slideFromLeft`, `rectTransform`, `isAnimating`.
- [x] 2. `CustomerSlotUI.cs`: Tách helper `ShowCustomer(...)` khỏi `SetupCustomer()`.
- [x] 3. `CustomerSlotUI.cs`: Thêm public method `SpawnCustomerWithAnimation(CustomerData)`.
- [x] 4. `CustomerSlotUI.cs`: Thêm coroutine `AnimateSlideIn()` dùng `Vector2.Lerp` + `Mathf.SmoothStep`.
- [x] 5. `CustomerSlotUI.cs`: Chặn đếm ngược trong `Update()` khi `isAnimating`.
- [x] 6. `CustomerSlotUI.cs`: Reset `isAnimating` + `StopAllCoroutines()` trong `ClearSlot()`.
- [x] 7. `CustomerSpawner.cs`: Đổi `SetCustomer(...)` → `SpawnCustomerWithAnimation(...)`.
