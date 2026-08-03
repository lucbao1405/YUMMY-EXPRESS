# 🛠 HƯỚNG DẪN FIX MISSING SCRIPT & HOÀN THIỆN TAP PHỤC VỤ KHÁCH (UI.unity)

> **Áp dụng cho:** `Assets/UI.unity` (Unity 2022.3 LTS, TextMeshPro đã cài)
> **Trạng thái code:** Đã sửa xong 6 file C# bên dưới. Bạn chỉ cần làm theo các bước Unity Editor bên dưới.

---

## ✅ TÓM TẮT ĐÃ SỬA

| File | Nội dung |
|---|---|
| `Plate&Serving/Plate.cs` **(MỚI)** | `OnPlateClicked()` → `GameManager.ServeFoodToCustomer(CurrentFood)` → nếu thành công `ClearPlate()`. Tự tìm Image/Button + tự AddListener. |
| `Plate&Serving/Plate.cs.meta` **(MỚI)** | GUID `9bf3335a...` **khớp GUID scene** → Unity auto re-associate 2 component Plate missing. |
| `Kitchen/GrillSlot.cs` **(MỚI)** | `OnGrillClicked()` → nếu thịt `Cooked` → `PlateManager.PlaceMeat()` + `StoveManager.RemoveMeat()`. Tự tìm Image/Button + AddListener. |
| `Kitchen/GrillSlot.cs.meta` **(MỚI)** | GUID `e70b75ce...` **khớp GUID scene** → Unity auto re-associate component GrillSlot missing. |
| `UI/EndGameUI.cs` | Tự tìm TMP trong `Dong_Vang/Dong_Khach/Dong_Combo`; nếu **chưa có TMP → TỰ TẠO** child text (không còn Warning spam). |
| `Customer/CustomerSlotUI.cs` | `OnReceiveFood()` nay **trả về `int` (tiền thưởng)** rồi mới `ClearSlot()`. |
| `Managers/GameManager.cs` | `ServeFoodToCustomer()` dùng giá trị trả về từ `OnReceiveFood()` để cộng vàng (kèm fallback `food.price`). |

---

## 1️⃣ BƯỚC 1 — MỞ UNITY & CHỜ IMPORT

1. Mở Unity Hub → Project **YummyExpress** → mở scene `Assets/UI.unity`.
2. Đợi Unity import xong (thanh progress dưới góc phải). Quan sát **Console**:
   - **3 lỗi "Missing Script" sẽ TỰ BIẾN MẤT** vì `Plate.cs` và `GrillSlot.cs` giờ có `.meta` trùng GUID với scene.
   - Nếu Console còn lỗi **compile** → bấm **Clear** rồi **bấm Play** 1 lần để Unity compile lại.

> ⚠️ **Nếu vẫn còn Missing Script** (thường do Unity chưa tải meta mới):
> 1. Bấm chuột phải thư mục `Assets/Scripts` → **Reimport**.
> 2. Hoặc `Assets → Reimport All`.

---

## 2️⃣ BƯỚC 2 — ADD COMPONENT `Plate` CHO DIA1 (nếu chưa có)

Scene hiện có 3 đĩa `Dia1`, `Dia 2`, `Dia 3`.
- `Dia 2` và `Dia 3` sau khi fix sẽ **tự có** component `Plate` (vì scene đã serialize).
- **`Dia1` có thể CHƯA có** component Plate (do conflict scene). Kiểm tra:

1. Chọn `Dia1` (trong `Middle_Zone → 3Dia`).
2. Nhìn **Inspector**:
   - Nếu **KHÔNG có** component `Plate` → bấm **Add Component** → tìm `Plate` → thêm.
   - Nếu **có** `Plate` nhưng báo **Missing Script** → bấm chuột phải component → **Remove** → Add lại.
3. Kéo thả (tùy chọn — nếu để trống script tự tìm):
   - **Food Image** → Image của `Dia1` (chính Image trên đĩa).
   - **Plate Button** → Button của `Dia1`.
4. Kiểm tra **Button** của `Dia1`:
   - Chọn `Dia1` → component `Button` → **OnClick ()** → nếu **chưa có** entry `Plate.OnPlateClicked` thì bấm **+**:
     - Kéo `Dia1` (có Plate) vào ô trống.
     - Dropdown chọn `Plate` → `OnPlateClicked`.

> 💡 Lưu ý: script `Plate.Start()` **tự AddListener** nếu `plateButton` được tìm thấy → nếu đĩa có Button là đủ, OnClick wire thủ công chỉ là lớp dự phòng. Tuy nhiên nên wire cả 2 cách để chắc chắn (AddListener sẽ không ghi đè OnClick thủ công).

---

## 3️⃣ BƯỚC 3 — ADD COMPONENT `GrillSlot` CHO VI1 (nếu chưa có)

Tương tự, `Vi1` và `Vi2` (trong `Bottom_Zone → Cot_Vi_Nuong`):
- `Vi2` sẽ tự có `GrillSlot` sau khi fix.
- Kiểm tra `Vi1`: nếu chưa có component nào tên `GrillSlot` → **Add Component** → `GrillSlot` (hoặc Remove + Add lại nếu bị Missing).

Kéo thả:
- **Food Image** → tạo 1 child Image tên `Food_Image` dưới `Vi1/Vi2` (hiển thị thịt đang nướng), kéo vào đây. *(Nếu để trống, `Awake` tự tìm child Image đầu tiên có thể nhầm với `DongHo_Timer` — khuyến nghị tạo child riêng.)*
- **Slot Button** → Button chính của `Vi1` / `Vi2`.

Wire OnClick (nếu muốn thủ công):
- Chọn `Vi1` → Button → OnClick → **+** → kéo `Vi1` → chọn `GrillSlot.OnGrillClicked`.
- Làm tương tự `Vi2`.

---

## 4️⃣ BƯỚC 4 — TẠO TEXT (TMP) TRONG BANG_THONG_KE (hiển thị số thưởng khi THẮNG)

> ✅ **Tin tốt:** code `EndGameUI.cs` mới **TỰ TẠO** text nếu chưa có — bạn **không bắt buộc** phải làm tay. Nhưng nếu muốn **đẹp & chủ động chỉnh font/size** thì làm như sau:

### Cách 1: Auto (không cần làm gì)
- Bấm Play → thắng 1 màn → `UpdateThongKe` sẽ tự tạo child `Text_Vàng`, `Text_Khách`, `Text_Combo` bên trong từng dòng và hiển thị số.
- Sau khi tạo, bạn có thể chỉnh font/size/màu ngay trong Inspector rồi **Save Scene** (các text này sẽ lưu vào scene).

### Cách 2: Tạo tay (đẹp hơn)
Với **mỗi dòng** `Dong_Vang`, `Dong_Khach`, `Dong_Combo` (nằm trong `Win_Popup → Bang_Thong_Ke`):

1. **Chuột phải** vào `Dong_Vang` → **UI → Text - TextMeshPro** (nếu chưa import TMP → bấm **Import TMP Essentials** khi được hỏi).
2. Unity tạo 1 child `Text (TMP)` mới. Đặt tên gameObject là `Text_Vang`.
3. Chỉnh trên **Inspector** (RectTransform):
   - **Anchor Presets:** chọn **stretch** (giữ chuột giữa-alt để stretch) hoặc để căn nhanh:
     - Chọn anchor **middle-center**, set **Pos X = 30** (đẩy sang phải icon Vàng), **Width = 150**, **Height = 60**.
4. Chỉnh **TextMeshProUGUI**:
   - **Text Input:** nhập sẵn `0` (số vàng sẽ được script ghi đè khi thắng).
   - **Font Size:** 40–48.
   - **Alignment:** Center.
   - **Color:** vàng gold `#FFD700` cho `Text_Vang`; trắng cho `Text_Khach`; cam cho `Text_Combo`.
5. Lặp lại cho `Dong_Khach` (tạo `Text_Khach`, nhập `0` khách) và `Dong_Combo` (tạo `Text_Combo`, nhập `0` combo).
6. **Gán thủ công (tùy chọn):** Chọn `Popup_Overlay` → component `EndGameUI` → kéo:
   - `Thong Ke Gold Text` → `Bang_Thong_Ke/Dong_Vang/Text_Vang`
   - `Thong Ke Customer Text` → `Bang_Thong_Ke/Dong_Khach/Text_Khach`
   - `Thong Ke Combo Text` → `Bang_Thong_Ke/Dong_Combo/Text_Combo`

> 💡 Code tự tìm bằng `GetComponentInChildren<TextMeshProUGUI>()` nên **không cần kéo tay** cũng hoạt động. Kéo tay chỉ để ưu tiên đúng text bạn muốn.

---

## 5️⃣ BƯỚC 5 — KIỂM THỬ TAP PHỤC VỤ KHÁCH

1. **Window → General → Console** mở sẵn.
2. Bấm **Play**.
3. Đợi vài giây → khách tự spawn vào `Slot_1/2/3` (có avatar + bubble món + thanh kiên nhẫn).
4. Luồng nấu (nếu có thịt):
   - Bấm nguyên liệu → thịt lên vỉ → chờ `Cooked` (log "Meat Cooked") → bấm vỉ → thịt lên đĩa (hoặc bấm đĩa trống để nhận).
5. **Test Tap đúng khách:**
   - Đĩa có món đúng món khách đang chờ → bấm đĩa → Console log `Khách ... đã nhận món ... +25 vàng.` → vàng tăng, khách biến mất, đĩa dọn sạch.
   - Bấm đĩa có món **không** khớp → log `Không có khách nào đang chờ món này.` → đĩa **giữ nguyên món** (không bị mất).
6. **Test EndGameUI:**
   - Đạt đủ `targetGold` → popup THẮNG hiện, Bang_Thong_Ke hiển thị số vàng/khách/combo hoặc text auto tạo.
   - Hết giờ hoặc quá số khách bỏ đi → popup THUA hiện.

---

## 6️⃣ BƯỚC 6 — LƯU SCENE & COMMIT (GIỮ REFERENCE)

1. **File → Save Scene** (Ctrl+S) — bắt buộc để lưu mọi thay đổi scene (text mới, component Plate/GrillSlot đã gán).
2. **File → Save Project**.
3. Kiểm tra `Assets/UI.unity` **và** `.meta` của **mọi script mới** tồn tại:
   - `Assets/Scripts/Plate&Serving/Plate.cs.meta`
   - `Assets/Scripts/Kitchen/GrillSlot.cs.meta`
4. Khi commit Git: **commit cả file `.cs` lẫn `.meta`** (`.meta` chứa GUID — gốc của mọi reference).

---

## ❓ FAQ

**Q: Vì sao 3 dòng "Missing Script" biến mất?**
A: Scene tham chiếu script bằng **GUID**. Hai GUID `9bf3335a…` (Plate) và `e70b75ce…` (GrillSlot) trước đây không có file `.cs` tương ứng → Unity hiển thị "Missing Script". Giờ ta tạo đúng 2 file + `.meta` trùng GUID → Unity tự liên kết lại.

**Q: EndGameUI còn bắn Warning "Không tìm thấy TextMeshProUGUI"?**
A: Không. Code mới dùng `EnsureThongKeText()` — tìm TMP trong con/cháu, nếu không có thì **tự tạo** child text, chỉ log 1 dòng thông tin (không phải Warning) và không spam.

**Q: Nút XemVideo vẫn log Warning khi bấm?**
A: Đó là cảnh báo có chủ đích (chưa nối AdsManager). Nếu chưa dùng QC, bạn có thể bỏ qua hoặc để trống không phải lỗi.

**Q: Dia1 bấm không phản hồi?**
A: Kiểm tra Dia1 có component **Button** và **Plate** không; nếu tự AddListener không chạy → wire thủ công `OnClick → Plate.OnPlateClicked`.

