# 📋 CHECKLIST KHÔI PHỤC REFERENCES TRONG UI.unity (YummyExpress)

> **Mục đích:** Khôi phục toàn bộ liên kết Inspector bị mất sau merge, hướng dẫn thứ tự kéo thả an toàn, và quy trình Save/Git để không bị mất lại.
>
> **Áp dụng cho scene:** `Assets/UI.unity`
> **Trạng thái chẩn đoán:** Chỉ còn `GameManager` script được attach. `GameManager` đang chứa field serialized CŨ (`textThoiGian`, `textVang`, `danhSachManChoi`, `manChoiHienTai`) — không khớp với code mới → toàn bộ Inspector trống.

---

## 🧭 MỤC LỤC

- [0. Chuẩn bị trước khi làm](#0-chuẩn-bị)
- [1. Bản đồ Hierarchy thực tế](#1-bản-đồ-hierarchy-thực-tế)
- [2. Bảng kéo thả từng Script](#2-bảng-kéo-thả-từng-script)
  - [2.1 EconomyManager](#21-economymanager)
  - [2.2 CustomerSpawner](#22-customerspawner)
  - [2.3 CustomerSlotUI](#23-customerslotui)
  - [2.4 Plate](#24-plate)
  - [2.5 GrillSlot](#25-grillslot)
  - [2.6 PlateManager](#26-platemanager)
  - [2.7 EndGameUI](#27-endgameui)
  - [2.8 GameManager](#28-gamemanager)
- [3. Bảng wiring Button.onClick](#3-bảng-wiring-buttononclick)
- [4. Thứ tự kéo thả an toàn (tránh NullReference)](#4-thứ-tự-kéo-thả-an-toàn)
- [5. Quy trình kiểm thử Play](#5-quy-trình-kiểm-thử-play)
- [6. Save Scene / Save Project / Create Prefab](#6-save--git-giữ-reference)
- [7. Phụ lục: Database ScriptableObject](#7-phụ-lục-database)

---

## 0. CHUẨN BỊ

1. **Mở Unity Hub → Project** YummyExpress, đợi import xong (kiểm tra Console **không còn lỗi compile**).
2. **Backup scene:** Copy `Assets/UI.unity` ra ngoài project (VD desktop) để phòng hờ.
3. **Bật chế độ text + meta file** (quan trọng cho Git):
   - `Edit → Project Settings → Editor`
   - **Asset Serialization → Mode: `Force Text`**
   - **Version Control → Mode: `Visible Meta Files`**
4. Xác nhận menu `Window → General → Console` đang mở để bắt lỗi kịp thời.

---

## 1. BẢN ĐỒ HIERARCHY THỰC TẾ

> Đây là cấu trúc **parse trực tiếp từ file scene** — dùng làm bản đồ để kéo thả.

```
📁 Canvas (CanvasScaler)
├── 📁 Top_Zone
│   ├── 📁 HUD_Panel (HorizontalLayoutGroup)
│   │   ├── ✏️ Timer_Text  (TMP)
│   │   └── ✏️ Gold_Text   (TMP)
│   └── 📁 Customer_Slots (HorizontalLayoutGroup)
│       ├── 📁 Slot_1 (Image)
│       │   ├── 📁 Order_Bubble (Image + VerticalLayoutGroup)
│       │   │   ├── 🖼 Mon_1 (Image)
│       │   │   └── 🖼 Mon_2 (Image)
│       │   ├── 🖼 Avatar_Khach (Image)
│       │   └── 📁 Patience_Bar (Slider)
│       │       ├── 🖼 Background (Image)
│       │       └── 📁 Fill Area
│       │           └── 🖼 Fill (Image)   ← thanh đầy
│       ├── 📁 Slot_2 (Image)  [Patience_Bar, Order_Bubble(Mon_2,Mon_1), Avatar_Khach]
│       └── 📁 Slot_3 (Image)  [Patience_Bar, Avatar_Khach, Order_Bubble(Mon_1,Mon_2)]
├── 📁 Middle_Zone
│   └── 📁 3Dia (HorizontalLayoutGroup)
│       ├── 🖼 Dia1 (Image)        ← CHƯA có Button
│       ├── 🖼 Dia 2 (Image)       ← CHƯA có Button
│       └── 🖼 Dia 3 (Image)       ← CHƯA có Button
├── 📁 Bottom_Zone
│   ├── 📁 Cot_Vi_Nuong (VerticalLayoutGroup)
│   │   ├── 🔘 Vi1 (Button + Image)  ← ĐÃ có Button
│   │   │   ├── 🖼 DongHo_Timer (Image)
│   │   │   └── ✏️ Text (TMP)
│   │   └── 🔘 Vi2 (Button + Image)  ← ĐÃ có Button
│   │       ├── ✏️ Text (TMP)
│   │       └── 🖼 DongHo_Timer (Image)
│   ├── 📁 Nguyen_Lieu (GridLayoutGroup)
│   │   ├── 🔘 Rau, 🔘 Pate, 🔘 Banh, 🔘 Thit   (Button + Image)
│   └── 📁 Sauce (VerticalLayoutGroup)
│       ├── 🔘 Hot_Sauce, 🔘 Mayone   (Button + Image)
├── 📁 Popup_Overlay
│   ├── 📁 Lose_Popup (Image)
│   │   ├── 📁 KhuVuc_LyDoThua (VerticalLayoutGroup) → Text_Meo_Tip, Text_LyDo_Chinh
│   │   ├── 📁 Btn (HorizontalLayoutGroup)
│   │   │   ├── 🔘 Btn_ChoiLai (Button + Image)
│   │   │   └── 🔘 Btn_CuuTro  (Button + Image)
│   │   └── 🖼 Icon_Lose (Image)
│   └── 📁 Win_Popup (Image)
│       ├── 📁 Khung_3_Sao (HorizontalLayoutGroup) → Sao_1, Sao_2, Sao_3
│       ├── 🔘 Btn_XemVideo (Button + Image)
│       ├── 🔘 Btn_TiepTuc  (Button + Image)
│       ├── 🖼 icon_Win (Image)
│       └── 📁 Bang_Thong_Ke (VerticalLayoutGroup)
│           ├── 📁 Dong_Combo (HLG) → Icon_Vang
│           ├── 📁 Dong_Khach (HLG) → Icon_Vang
│           └── 📁 Dong_Vang  (HLG) → Icon_Vang
📁 GameManager   → (đã có script GameManager)
📁 EventSystem   → (EventSystem + StandaloneInputModule) ✓ OK
📁 Main Camera
```

**Ký hiệu:** 🖼 = có Image, 🔘 = có Button, ✏️ = TextMeshPro, 📁 = container.

---

## 2. BẢNG KÉO THẢ TỪNG SCRIPT

> ⚠️ Mỗi script chỉ **Add Component** khi chưa có. Dùng nút **Add Component** (không kéo file .cs vào để tránh nhầm).

### 2.1 EconomyManager
**Nơi gắn:** GameObject `GameManager` (Add Component `EconomyManager`)

| Field trong Inspector | Kéo vào | Ghi chú |
|---|---|---|
| `Gold Text` (TextMeshProUGUI) | `Top_Zone/HUD_Panel/Gold_Text` | ⚠️ **Xem lưu ý mục 2.8** — nếu `GameManager.goldProgressText` cũng trỏ vào đây sẽ xung đột nội dung text. **Khuyến nghị: để TRỐNG** (script an toàn null) để `Gold_Text` hiển thị dạng `75/100` do GameManager điều khiển. |

---

### 2.2 CustomerSpawner
**Nơi gắn:** GameObject `GameManager` (Add Component `CustomerSpawner`)

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Customer Slots` (List, size = 3) | Phần tử 0 → `Slot_1`<br>Phần tử 1 → `Slot_2`<br>Phần tử 2 → `Slot_3` | Phải có **3 phần tử, KHÔNG được để trống ô nào** (nếu list size > số ô gán → null element → NRE khi `GetRandomCustomerData`). |
| `Customer Database` (List) | `Khach1`, `Khach2`, `Khach3` (trong `Assets/Sprites/Customers/`) | Kéo thẳng file `.asset`. |
| `Min Spawn Delay` / `Max Spawn Delay` | (mặc định 3 / 6) | Không bắt buộc. |

---

### 2.3 CustomerSlotUI
**Nơi gắn:** Add Component `CustomerSlotUI` lần lượt vào `Slot_1`, `Slot_2`, `Slot_3`.

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Avatar Image` (Image) | con `Avatar_Khach` của slot đó | |
| `Order Bubble` (GameObject) | con `Order_Bubble` của slot đó | |
| `Order Item Image` (Image) | `Order_Bubble/Mon_1` | `Mon_2` để nguyên (chỉ 1 icon hiển thị). |
| `Patience Bar` (Image) | `Patience_Bar/Fill Area/Fill` | ⚠️ Đây là **Image con Fill** — KHÔNG kéo cả `Patience_Bar` (Slider) vì field là `Image`. |
| Màu + ngưỡng kiên nhẫn | (mặc định) | Không bắt buộc đổi. |

> ⚠️ **Quan trọng:** `Fill` phải có `Image Type = Filled` (`FillMethod = Horizontal`, `FillOrigin = 0`) thì `fillAmount` mới hiển thị đúng thanh kiên nhẫn. Nếu chưa đúng, chọn `Fill` → Inspector → Image → Type: **Filled**.

---

### 2.4 Plate
**Nơi gắn:** Add Component `Plate` vào `Dia1`, `Dia 2`, `Dia 3`.

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Food Image` (Image) | chính Image trên `Dia1` / `Dia 2` / `Dia 3` | Nếu để trống, `Awake()` tự tìm `GetComponentInChildren<Image>`. |
| `Plate Button` (Button) | Button vừa thêm trên đĩa (xem bên dưới) | Nếu để trống, `Awake()` tự tìm. |

**Bắt buộc — Thêm Button cho từng đĩa** (hiện Dia chưa có Button):
1. Chọn `Dia1` → **Add Component → Button**.
2. Nếu bị cảnh báo "Image is required", chọn **Add** để tự thêm (đĩa đã có Image nên sẽ OK).
3. Làm tương tự cho `Dia 2`, `Dia 3`.
4. Wiring OnClick → xem [mục 3](#3-bảng-wiring-buttononclick).

---

### 2.5 GrillSlot
**Nơi gắn:** Add Component `GrillSlot` vào `Vi1`, `Vi2` (đã có sẵn Button).

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Food Image` (Image) | ⚠️ Nên **tạo thêm 1 child Image tên `Food_Image`** dưới Vi1/Vi2 để hiển thị món đã chín, rồi kéo vào đây. Nếu để trống, `Awake` tự tìm child Image đầu tiên (có thể là `DongHo_Timer` — không đúng ý đồ). | |
| `Slot Button` (Button) | Button trên chính `Vi1` / `Vi2` | Nếu để trống tự tìm được. |
| `Current Food` / `Current State` | (runtime) | Không kéo gì. |

**Lưu ý:** `GrillSlot` không tự gán sự kiện click — phải wiring `Button.onClick → GrillSlot.OnGrillClicked` (mục 3). Ngoài ra, scene **chưa có script xử lý nguyên liệu** (Rau/Pate/Banh/Thit/Hot_Sauce/Mayone) nên luồng "nấu chín → SetFood" chưa hoạt động — chỉ `OnGrillClicked` (chuyển món chín lên đĩa) chạy được khi có món `Cooked`.

---

### 2.6 PlateManager
**Nơi gắn:** Add Component `PlateManager` vào `3Dia` (cha của các đĩa — vì `RefreshPlateList()` dùng `GetComponentsInChildren<Plate>()`).

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Plates` (List) | **(Để trống)** | `Awake()` tự tìm 3 đĩa con. Nếu muốn chắc chắn, kéo `Dia1`, `Dia 2`, `Dia 3` vào. |

---

### 2.7 EndGameUI
**Nơi gắn:** Add Component `EndGameUI` vào `Popup_Overlay`.

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Win Panel` (GameObject) | `Popup_Overlay/Win_Popup` | |
| `Lose Panel` (GameObject) | `Popup_Overlay/Lose_Popup` | |
| `Btn Replay` (Button) | `Lose_Popup/Btn/Btn_ChoiLai` | "Chơi lại" |
| `Btn Next Level` (Button) | `Win_Popup/Btn_TiepTuc` | "Tiếp tục" |
| `Btn Main Menu` (Button) | `Lose_Popup/Btn/Btn_CuuTro` | ⚠️ Chỉ gán nếu scene `MainMenu` có trong **Build Settings**. Nếu chưa có, **để trống** (script bỏ qua null an toàn). |

> ✅ `EndGameUI` tự gán listener cho 3 Button trong `Start()` → **KHÔNG cần** wiring OnClick thủ công cho các nút này (tránh bị gọi 2 lần).

---

### 2.8 GameManager
**Nơi:** GameObject `GameManager` (script đã có — chỉ cần gán lại field, xóa dữ liệu cũ).

| Field | Kéo vào | Ghi chú |
|---|---|---|
| `Timer Text` (TMP) | `Top_Zone/HUD_Panel/Timer_Text` | Đếm ngược thời gian. |
| `Gold Progress Text` (TMP) | `Top_Zone/HUD_Panel/Gold_Text` | Hiển thị `75/100`. |
| `End Game UI` | GameObject `Popup_Overlay` (kéo cả GameObject có component EndGameUI) | |
| `Level Configs` (array) | Thêm **3 element**, set giá trị (xem bảng dưới) | Không kéo asset. |
| `Customer Spawner` | GameObject `GameManager` (kéo cả GameObject có component CustomerSpawner) | Nếu để trống, `Start()` tự `FindObjectOfType` tìm được. |

**Giá trị gợi ý cho `Level Configs`** (khớp config cũ trong scene):

| Element | Target Gold | Level Time Limit | Max Lost Customers |
|---|---|---|---|
| 0 | 100 | 90 | 3 |
| 1 | 120 | 90 | 3 |
| 2 | 150 | 90 | 3 |

> ⚠️ **Xung đột tiềm ẩn:** Nếu bạn gán cả `EconomyManager.goldText` VÀ `GameManager.goldProgressText` vào `Gold_Text`, khi vàng đổi, `EconomyManager` ghi `"Gold: 75"` còn `GameManager` ghi `"75/100"` → text nhấp nháy. **Khuyến nghị:** chỉ GameManager dùng `Gold_Text`; để `EconomyManager.goldText` trống (hoặc tạo 1 Text riêng nếu muốn hiện cả 2 dạng).

---

## 3. BẢNG WIRING BUTTON.ONCLICK

> Cách làm: chọn GameObject → Inspector → component **Button** → cuộn xuống **OnClick ()** → bấm **+** → kéo GameObject có script vào ô trống → chọn hàm trong dropdown.

| Button (GameObject) | Object kéo vào OnClick | Hàm chọn | Ghi chú |
|---|---|---|---|
| `Middle_Zone/3Dia/Dia1` | `Dia1` (có Plate) | `Plate.OnPlateClicked` | Bấm đĩa → phục vụ khách |
| `Middle_Zone/3Dia/Dia 2` | `Dia 2` (có Plate) | `Plate.OnPlateClicked` | |
| `Middle_Zone/3Dia/Dia 3` | `Dia 3` (có Plate) | `Plate.OnPlateClicked` | |
| `Bottom_Zone/Cot_Vi_Nuong/Vi1` | `Vi1` (có GrillSlot) | `GrillSlot.OnGrillClicked` | Chuyển món chín lên đĩa |
| `Bottom_Zone/Cot_Vi_Nuong/Vi2` | `Vi2` (có GrillSlot) | `GrillSlot.OnGrillClicked` | |
| `Popup_Overlay/Lose_Popup/Btn/Btn_ChoiLai` | *(không cần)* | — | EndGameUI tự gán `OnReplayClicked` |
| `Popup_Overlay/Win_Popup/Btn_TiepTuc` | *(không cần)* | — | EndGameUI tự gán `OnNextLevelClicked` |
| `Popup_Overlay/Lose_Popup/Btn/Btn_CuuTro` | *(không cần)* | — | EndGameUI tự gán `OnMainMenuClicked` |
| `Popup_Overlay/Win_Popup/Btn_XemVideo` | *(không dùng)* | — | Không có hàm xử lý trong các script chính |
| Nguyên liệu `Rau/Pate/Banh/Thit/Hot_Sauce/Mayone` | *(không cần)* | — | **`IngredientButton`** tự gắn listener trong `Awake()` → bấm sẽ đặt `FoodData` lên đĩa trống. Chỉ cần gán **`Food Data`** (asset) trên Inspector cho từng nút. |

---

## 4. THỨ TỰ KÉO THẢ AN TOÀN

> Mục tiêu: làm theo thứ tự này để **bấm Play lần đầu không bị NullReferenceException**, và dễ kiểm soát lỗi nếu có.

### Phase 1 — Gắn Singleton (nền tảng)
1. Add `EconomyManager` vào `GameManager`.
2. Add `CustomerSpawner` vào `GameManager`.
3. Add `PlateManager` vào `3Dia`.

> ✅ Vì `SingletonBehaviour` set `Instance` trong `Awake()`, cả 3 sẽ sẵn sàng trước khi bất kỳ `Start()` nào chạy.

### Phase 2 — Gắn Script lên các node con
4. Add `CustomerSlotUI` → `Slot_1`, `Slot_2`, `Slot_3`.
5. Add `Plate` → `Dia1`, `Dia 2`, `Dia 3` **+ thêm Button** cho từng đĩa.
6. Add `GrillSlot` → `Vi1`, `Vi2`.
7. Add `EndGameUI` → `Popup_Overlay`.

### Phase 3 — Kéo thả reference từng script (theo mục 2)
8. Wire `CustomerSlotUI` (3 slot) — avatar, bubble, item image, patience fill.
9. Wire `Plate` (3 đĩa) — food image, plate button.
10. Wire `GrillSlot` (2 vỉ) — food image, slot button.
11. Wire `EndGameUI` — win/lose panel, 3 button.
12. Wire `CustomerSpawner` — list slots (3), list database (3).
13. Wire `EconomyManager` — để trống (hoặc Gold_Text nếu chấp nhận xung đột).

### Phase 4 — Wiring Button OnClick (theo mục 3)
14. Gán `OnPlateClicked` cho 3 đĩa.
15. Gán `OnGrillClicked` cho 2 vỉ.

### Phase 5 — Cấu hình GameManager CUỐI CÙNG
16. Gán `Timer Text`, `Gold Progress Text`, `End Game UI`, `Customer Spawner`.
17. Tạo `Level Configs` (3 element) với giá trị như mục 2.8.

> 🔑 **Vì sao GameManager làm cuối:** `StartLevel()` gọi ngay `EconomyManager`, `endGameUI`, `customerSpawner`. Khi tất cả các script khác đã wire xong, bấm Play sẽ chạy trơn tru.

---

## 5. QUY TRÌNH KIỂM THỬ PLAY

1. `Ctrl+S` lưu scene → `Ctrl+Shift+B` bấm **Add Open Scenes** (đảm bảo `UI.unity` trong Build Settings).
2. Bấm **Play**:
   - Console không có lỗi đỏ. Nếu có `MissingReferenceException` → tìm object bị thiếu theo tên log.
   - HUD hiện `Timer_Text` đếm ngược, `Gold_Text` hiện `0/100`.
   - Sau vài giây, khách tự spawn vào Slot (avatar + order bubble + thanh kiên nhẫn).
   - Khách bỏ đi → `Lose_Popup` hiện khi quá số khách hoặc hết giờ.
3. Test tay: bấm `Vi1` (đang Cooked) → món lên đĩa trống → bấm đĩa → khách đúng món được phục vụ, vàng tăng.
4. Test nút: `Btn_ChoiLai` load lại scene; `Btn_TiepTuc` load scene kế (nếu có).

---

## 6. SAVE / GIT (GIỮ REFERENCE KHI PUSH/PULL)

### 6.1 Lưu đúng cách trong Unity
1. **File → Save Scene** (`Ctrl+S`) — lưu mọi thay đổi của scene.
2. **File → Save Project** — lưu settings/prefab.
3. Kiểm tra `Assets/UI.unity` **và** `Assets/UI.unity.meta` đều tồn tại. **Không bao giờ** xóa `.meta`.

### 6.2 Git — 3 nguyên tắc vàng
1. **Commit toàn bộ `.meta`**: `.meta` chứa **GUID** — nguồn gốc của mọi reference. File bị bỏ quên `.meta` = reference mất.
2. **Chỉ sửa `.unity` bằng Unity Editor**, không sửa tay bằng text editor (tránh hỏng YAML/instanceID).
3. **Tránh 2 người sửa cùng scene 1 lúc** — đây là nguyên nhân chính gây conflict.

### 6.3 Cấu hình UnityYAMLMerge (bắt buộc cho team)
1. `Edit → Project Settings → Editor → Version Control`
2. Chọn **Mode: `Visible Meta Files`** (đã làm ở mục 0).
3. Trong Git, cấu hình **merge driver** cho `*.unity` và `*.prefab` dùng `UnityYAMLMerge`:
   - File `.gitattributes` (thêm nếu chưa có):
     ```
     *.unity merge=unityyamlmerge
     *.prefab merge=unityyamlmerge
     ```
   - Cấu hình git driver (chạy 1 lần, mỗi máy):
     ```
     git config --global merge.unityyamlmerge.name "Unity YAML merge tool"
     git config --global merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/<VERSION>/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %A %B"
     ```

### 6.4 Khi bị conflict ở UI.unity (như lần này)
1. `git pull` → báo conflict ở `UI.unity`.
2. **KHÔNG** bấm "Accept ours/theirs" một cách máy móc — dễ sinh ra scene lai như bạn đang gặp (field cũ `textThoiGian` + script mới).
3. Chạy merge tool (nếu có driver) hoặc tự chọn 1 bên, mở Unity lên và **re-wire lại theo checklist này**.
4. Sau khi re-wire xong, Unity sẽ ghi lại field mới đúng chuẩn → lần sau merge dễ hơn.

### 6.5 Prefab (khuyến nghị để không lặp lại)
- **Slot khách hàng:** Chuột phải `Slot_1` → **Create → Prefab** → lưu `Prefabs/CustomerSlot.prefab`. Gán các child ref 1 lần. Các slot khác dùng **Prefab** (thay vì copy rời) → mọi thay đổi chỉ làm 1 nơi.
- **Đĩa:** Tương tự tạo `Plate.prefab` từ `Dia1` (đã có Plate + Button + OnClick).
- **Vỉ nướng:** Tạo `GrillSlot.prefab` từ `Vi1`.
- **Popup:** Tạo `EndGamePopup.prefab` từ `Popup_Overlay`.
- Khi tạo prefab, chọn **Apply All** để lưu override, và nhớ **Save Project**.

> 💡 Prefab giúp: reference luôn nhất quán, khi pull về các thành viên khác chỉ cần đảm bảo prefab có trong Assets (`.meta` đã commit) là mọi liên kết bên trong prefab tự khôi phục.

---

## 7. PHỤ LỤC: DATABASE

### CustomerData (`Assets/Sprites/Customers/`)
| Asset | customerName | requiredFood | maxPatienceTime |
|---|---|---|---|
| `Khach1.asset` | Emilia | Bánh mì (BanhMi) | 10 |
| `Khach2.asset` | Phong Cách | Bánh mì sốt (BanhMiSot) | 10 |
| `Khach3.asset` | Anime Girl | Mì tôn (MiTon) | 10 |

### FoodData (`Assets/Sprites/Food/`)
| Asset | foodID | price |
|---|---|---|
| `Bánh mì.asset` | BanhMi | 25 |
| `Bánh mì sốt.asset` | BanhMiSot | 30 |
| `Mì tôn.asset` | MiTon | 20 |

### Script GUIDs (tham khảo khi debug YAML)
| Script | GUID |
|---|---|
| GameManager | `4c8e59542a22f7c48b9f3c93153658b5` |
| EconomyManager | `2fae2c46d7c75834a816e987e7fb4787` |
| CustomerSpawner | `7166b762fd6d193428132621b996890a` |
| CustomerSlotUI | `c867045f797274a428b89c42706c5408` |
| EndGameUI | `9d3cc57cc2d9397468b83d369fa8f68c` |
| Plate | `9bf3335a019c63f4ca667d4ab673342f` |
| GrillSlot | `e70b75ced6b7ca448861c4303d6a61a6` |
| PlateManager | `eed915ffc6d00644a962cc8590192789` |
| Customer | `b1247da060d49da47bdbc5897998f628` |
| FoodData | `dc0e5a07f1d77e14886b85974ac0cdb0` |
| CustomerData | `40c305a2c07a01d4ba3a349538f8cbec` |

---

*Checklist hoàn tất. Làm theo Phase 1 → 5 ở mục 4, rồi lưu scene + project trước khi commit.*

