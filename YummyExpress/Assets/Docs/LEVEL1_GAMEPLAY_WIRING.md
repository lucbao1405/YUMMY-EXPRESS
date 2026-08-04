# 🎯 Yummy Express Level 1 — Wiring & Setup hướng dẫn

> Kiến trúc dùng **đúng 5 script**: `FoodState`, `FoodItem`, `CookingProcess`, `GrillStation`, `Plate`.
> Không dùng `Update` để điều khiển gameplay — chỉ Coroutine + method call.

---

## 1. Hierarchy bắt buộc

```
Canvas
├── Middle_Zone
│   └── Dia1
│       └── BanhMi
│           ├── BottomBreadPoint
│           ├── MeatPoint
│           ├── VegetablePoint
│           ├── TopBreadPoint
│           └── CompleteBanhMiPoint
├── Cot_Vi_Nuong
│   └── Vi1
│       └── CookPoint
└── Bottom_Zone
    └── Nguyen_Lieu
        ├── Banh   (Button)
        ├── Thit   (Button)
        └── Rau    (Button)
```

> ⚠️ Đảm bảo các point (`BottomBreadPoint`, `MeatPoint`, `VegetablePoint`, `TopBreadPoint`, `CompleteBanhMiPoint`, `CookPoint`) tồn tại trong scene và đúng vị trí.

---

## 2. Gắn script vào GameObject

| Script | Gắn vào GameObject | Vị trí |
|---|---|---|
| `Plate` | `Dia1` | `Canvas → Middle_Zone → Dia1` |
| `GrillStation` | `Vi1` | `Canvas → Cot_Vi_Nuong → Vi1` |

### Prefab (kéo vào Inspector)
- `BottomBread.prefab`, `TopBread.prefab`, `Vegetable.prefab`, `CompleteBanhMi.prefab` → `Plate`
- `Meat.prefab` → `GrillStation`

---

## 3. Kéo thả serialized fields

### Plate (trên `Dia1`)
| Field | Kéo vào |
|---|---|
| `Bottom Bread Point` | `BanhMi/BottomBreadPoint` |
| `Top Bread Point` | `BanhMi/TopBreadPoint` |
| `Meat Point` | `BanhMi/MeatPoint` |
| `Vegetable Point` | `BanhMi/VegetablePoint` |
| `Complete Banh Mi Point` | `BanhMi/CompleteBanhMiPoint` |
| `Bottom Bread Prefab` | `BottomBread.prefab` |
| `Top Bread Prefab` | `TopBread.prefab` |
| `Vegetable Prefab` | `Vegetable.prefab` |
| `Complete Banh Mi Prefab` | `CompleteBanhMi.prefab` |

### GrillStation (trên `Vi1`)
| Field | Kéo vào |
|---|---|
| `Cook Point` | `Vi1/CookPoint` |
| `Meat Prefab` | `Meat.prefab` |

---

## 4. Meat prefab (thiết lập)

`Meat.prefab` phải chứa:
- `FoodItem` → gán `Raw Sprite`, `Cooked Sprite`, `Burnt Sprite`.
- `CookingProcess` → (`Cook Time` = 4, `Burn Time` = 4).
- `Button` (để tap) → trong `Button.onClick`:
  - Kéo chính `Meat.prefab` vào ô.
  - Dropdown chọn `CookingProcess` → `TryServeToPlate`.

---

## 5. Wiring Button.onClick (nút nguyên liệu)

| Nút | Object kéo vào | Hàm chọn |
|---|---|---|
| `Banh` | `Dia1` (có `Plate`) | `Plate.PlaceBread` |
| `Thit` | `Vi1` (có `GrillStation`) | `GrillStation.PlaceMeatOnGrill` |
| `Rau` | `Dia1` (có `Plate`) | `Plate.PlaceVegetable` |

---

## 6. Luồng gameplay

1. **Banh** → `Plate.PlaceBread()` → spawn `BottomBread` + `TopBread` (chỉ 1 lần).
2. **Thit** → `GrillStation.PlaceMeatOnGrill()` → `PlaceFood()` → Instantiate `Meat` vào `CookPoint` → `StartCooking()`.
3. **Meat** tự nấu: `Raw → Cooked (4s) → Burnt (4s)` (Coroutine).
4. Khi **Cooked**, tap miếng thịt → `CookingProcess.TryServeToPlate()` → `Plate.PlaceMeat()` → chuyển thịt lên đĩa, `StopCooking()`, `ClearStation()`.
5. **Rau** → `Plate.PlaceVegetable()` (chỉ khi đã có bánh).
6. Khi đủ `BottomBread + TopBread + Meat + Vegetable` → `CheckCompleteBanhMi()` → ẩn 4 thành phần, spawn `CompleteBanhMi` (chỉ 1 lần).

---

## 7. Chống lỗi (đã xử lý trong code)

- ✅ NullReferenceException (null-check mọi nơi).
- ✅ Double spawn bánh (flag `hasBread`).
- ✅ Spawn khi thiếu point/prefab (log lỗi, return).
- ✅ Meat lên Plate khi chưa chín (chỉ chấp nhận khi `IsCooked()`).
- ✅ Rau lên Plate khi chưa có bánh (chỉ khi `hasBread`).
- ✅ CompleteBanhMi spawn nhiều lần (chỉ khi `completeBanhMi == null`).
- ✅ Bếp spawn thịt 2 lần (chỉ khi `IsOccupied()` trả về false).

## 8. Lưu ý Singleton (KHÔNG mất object)

> ⚠️ **Quan trọng:** `Plate` và `GrillStation` dùng Singleton **không-destructive**.
> Khi scene có **nhiều đĩa** (Dia1, Dia2, Dia3) và **nhiều vỉ** (Vi1, Vi2), các object
> **KHÔNG bị Destroy** khi chạy game. `Instance` chỉ trỏ đến **đối tượng đầu tiên** khởi tạo.

**Hệ quả khi wire:**
- Nút **Banh** nên wire vào `Plate` của **Dia1** (vì `Plate.Instance` = Dia1) → `PlaceBread`.
- Nút **Thit** nên wire vào `GrillStation` của **Vi1** (vì `GrillStation.Instance` = Vi1) → `PlaceMeatOnGrill`.
- Miếng thịt (Meat prefab) khi tap → `CookingProcess.TryServeToPlate()` tự gọi `Plate.Instance.PlaceMeat()` và `GrillStation.Instance.ClearStation()` — nên đúng Dia1/Vi1.

> Nếu bạn muốn đường bánh dùng đĩa/vỉ khác, hãy wire trực tiếp các nút vào đúng component đó (không qua `Instance`), nhưng luồng Meat→Plate vẫn dùng `Instance` (Dia1).
