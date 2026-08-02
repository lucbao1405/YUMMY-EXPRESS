#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool tự động sửa toàn bộ wiring trong UI.unity:
///  - Xóa GameManager cũ (serialization cũ) → Add lại mới
///  - Gắn EndGameUI lên Popup_Overlay + wire winPopup/losePopup/các Button/TMP.
///  - Gắn EconomyManager + CustomerSpawner lên GO GameManager.
///  - Fix serialized field của GameManager (timerText, goldProgressText, levelConfigs, customerSpawner).
///  - Gắn CustomerSlotUI lên Slot_1/2/3.
///  - Gắn Plate + Button lên Dia1/Dia 2/Dia 3, GrillSlot lên Vi1/Vi2.
///  - Gắn PlateManager lên 3Dia.
///  - Thêm Assets/UI.unity vào Build Settings (đầu danh sách).
///  - Lưu scene + project.
///
/// Cách dùng: Menu bar → Tools → YummyExpress → 1. Fix & Wire Scene (UI.unity)
/// </summary>
public static class FixSceneWiring
{
    private const string ScenePath = "Assets/UI.unity";
    private const string CustomerDbPath = "Assets/Sprites/Customers/";

    [MenuItem("Tools/YummyExpress/1. Fix & Wire Scene (UI.unity)")]
    public static void FixAndWireScene()
    {
        // 1. Mở UI.unity (single mode)
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Không mở được scene: " + ScenePath);
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        GameObject canvas = FindRoot(roots, "Canvas");
        GameObject gameManagerGO = FindRoot(roots, "GameManager");

        if (canvas == null) { Debug.LogError("Không tìm thấy Canvas trong UI.unity!"); return; }
        if (gameManagerGO == null) { Debug.LogError("Không tìm thấy GameObject GameManager trong UI.unity!"); return; }

        // 2. Xóa GameManager component CŨ (serialization cũ) và Add lại MỚI
        var oldGM = gameManagerGO.GetComponent<GameManager>();
        if (oldGM != null)
        {
            Undo.DestroyObjectImmediate(oldGM);
            Debug.Log("✅ Đã xóa GameManager component cũ (serialization cũ).");
        }
        // Add GameManager mới
        var newGM = gameManagerGO.AddComponent<GameManager>();
        Debug.Log("✅ Đã Add GameManager component mới.");

        // 3. Add EconomyManager
        var econ = EnsureComponent<EconomyManager>(gameManagerGO);
        Debug.Log("✅ EconomyManager đã Add.");

        // 4. Add CustomerSpawner
        var spawner = EnsureComponent<CustomerSpawner>(gameManagerGO);
        Debug.Log("✅ CustomerSpawner đã Add.");

        // 5. Wire EndGameUI
        GameObject popupOverlay = FindDescendant(canvas, "Popup_Overlay");
        WireEndGameUI(popupOverlay);

        // 6. Wire CustomerSlots
        GameObject slots = FindDescendant(canvas, "Top_Zone/Customer_Slots");
        WireCustomerSlots(slots);

        // 7. Wire Plates + PlateManager
        GameObject dia3 = FindDescendant(canvas, "Middle_Zone/3Dia");
        WirePlates(dia3);

        // 8. Wire Grills
        GameObject grills = FindDescendant(canvas, "Bottom_Zone/Cot_Vi_Nuong");
        WireGrills(grills);

        // 9. Wire GameManager references
        WireGameManagerFields(newGM, spawner, canvas);

        // 10. Wire Spawner references
        WireSpawnerReferences(spawner, canvas);

        // 11. Add scene to Build Settings
        AddSceneToBuildSettings();

        // 12. Lưu scene + project
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("<color=green>✅ Đã fix & wire xong UI.unity! "
                  + "Scene hiện tại là UI.unity — bấm Play là chạy được (Win_Popup/Lose_Popup sẽ hiện đúng khi thắng/thua).</color>");
    }

    [MenuItem("Tools/YummyExpress/2. Add UI.unity to Build Settings")]
    public static void AddSceneToBuildSettingsMenu()
    {
        AddSceneToBuildSettings();
        Debug.Log("✅ UI.unity đã được thêm vào Build Settings (vị trí đầu).");
    }

    [MenuItem("Tools/YummyExpress/3. Open UI.unity")]
    public static void OpenUIScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (scene.IsValid())
            Debug.Log("✅ Đã mở " + ScenePath);
    }

    // =====================================================================
    //  ENDGAME UI — gắn lên Popup_Overlay
    // =====================================================================
    private static void WireEndGameUI(GameObject popupOverlay)
    {
        if (popupOverlay == null)
        {
            Debug.LogWarning("Không thấy Popup_Overlay, bỏ qua EndGameUI.");
            return;
        }

        var egu = EnsureComponent<EndGameUI>(popupOverlay);
        var so = new SerializedObject(egu);

        so.FindProperty("winPopup").objectReferenceValue = FindDescendant(popupOverlay, "Win_Popup");
        so.FindProperty("losePopup").objectReferenceValue = FindDescendant(popupOverlay, "Lose_Popup");
        so.FindProperty("iconWin").objectReferenceValue = FindDescendant(popupOverlay, "Win_Popup/icon_Win");
        so.FindProperty("khung3Sao").objectReferenceValue = FindDescendant(popupOverlay, "Win_Popup/Khung_3_Sao");
        so.FindProperty("bangThongKe").objectReferenceValue = FindDescendant(popupOverlay, "Win_Popup/Bang_Thong_Ke");
        so.FindProperty("iconLose").objectReferenceValue = FindDescendant(popupOverlay, "Lose_Popup/Icon_Lose");
        so.FindProperty("khuVucLyDoThua").objectReferenceValue = FindDescendant(popupOverlay, "Lose_Popup/KhuVuc_LyDoThua");

        so.FindProperty("btnTiepTuc").objectReferenceValue = GetComp<Button>(FindDescendant(popupOverlay, "Win_Popup/Btn_TiepTuc"));
        so.FindProperty("btnXemVideo").objectReferenceValue = GetComp<Button>(FindDescendant(popupOverlay, "Win_Popup/Btn_XemVideo"));
        so.FindProperty("btnReplay").objectReferenceValue = GetComp<Button>(FindDescendant(popupOverlay, "Lose_Popup/Btn/Btn_ChoiLai"));
        so.FindProperty("btnMainMenu").objectReferenceValue = GetComp<Button>(FindDescendant(popupOverlay, "Lose_Popup/Btn/Btn_CuuTro"));

        so.FindProperty("lyDoThuaText").objectReferenceValue = GetComp<TextMeshProUGUI>(FindDescendant(popupOverlay, "Lose_Popup/KhuVuc_LyDoThua/Text_LyDo_Chinh"));
        so.FindProperty("meoTipText").objectReferenceValue = GetComp<TextMeshProUGUI>(FindDescendant(popupOverlay, "Lose_Popup/KhuVuc_LyDoThua/Text_Meo_Tip"));

        // 3 ngôi sao
        var stars = so.FindProperty("starImages");
        stars.arraySize = 3;
        stars.GetArrayElementAtIndex(0).objectReferenceValue = GetComp<Image>(FindDescendant(popupOverlay, "Win_Popup/Khung_3_Sao/Sao_1"));
        stars.GetArrayElementAtIndex(1).objectReferenceValue = GetComp<Image>(FindDescendant(popupOverlay, "Win_Popup/Khung_3_Sao/Sao_2"));
        stars.GetArrayElementAtIndex(2).objectReferenceValue = GetComp<Image>(FindDescendant(popupOverlay, "Win_Popup/Khung_3_Sao/Sao_3"));

        so.ApplyModifiedProperties();

        // Đảm bảo Win/Lose popup ẩn lúc đầu
        var win = FindDescendant(popupOverlay, "Win_Popup");
        var lose = FindDescendant(popupOverlay, "Lose_Popup");
        if (win != null) win.SetActive(false);
        if (lose != null) lose.SetActive(false);

        Debug.Log("✅ EndGameUI đã gắn & wire trên Popup_Overlay.");
    }

    // =====================================================================
    //  GAME MANAGER — Wire fields (timerText, goldProgressText, levelConfigs, customerSpawner)
    // =====================================================================
    private static void WireGameManagerFields(GameManager gm, CustomerSpawner spawner, GameObject canvas)
    {
        var so = new SerializedObject(gm);
        so.FindProperty("timerText").objectReferenceValue =
            GetComp<TextMeshProUGUI>(FindDescendant(canvas, "Top_Zone/HUD_Panel/Timer_Text"));
        so.FindProperty("goldProgressText").objectReferenceValue =
            GetComp<TextMeshProUGUI>(FindDescendant(canvas, "Top_Zone/HUD_Panel/Gold_Text"));
        so.FindProperty("customerSpawner").objectReferenceValue = spawner;

        // Level Configs: 3 màn
        var configs = so.FindProperty("levelConfigs");
        configs.arraySize = 3;
        SetLevelConfig(configs.GetArrayElementAtIndex(0), 100, 90f, 3);
        SetLevelConfig(configs.GetArrayElementAtIndex(1), 120, 90f, 3);
        SetLevelConfig(configs.GetArrayElementAtIndex(2), 150, 90f, 3);

        so.ApplyModifiedProperties();

        Debug.Log("✅ GameManager (field mới) đã wire: timerText, goldProgressText, levelConfigs, customerSpawner.");
    }

    private static void SetLevelConfig(SerializedProperty elem, int gold, float time, int maxLost)
    {
        elem.FindPropertyRelative("targetGold").intValue = gold;
        elem.FindPropertyRelative("levelTimeLimit").floatValue = time;
        elem.FindPropertyRelative("maxLostCustomers").intValue = maxLost;
    }

    // =====================================================================
    //  SPAWNER REFERENCES
    // =====================================================================
    private static void WireSpawnerReferences(CustomerSpawner spawner, GameObject canvas)
    {
        if (spawner == null || canvas == null) return;
        var so = new SerializedObject(spawner);

        var slots = so.FindProperty("customerSlots");
        slots.arraySize = 3;
        slots.GetArrayElementAtIndex(0).objectReferenceValue =
            GetComp<CustomerSlotUI>(FindDescendant(canvas, "Top_Zone/Customer_Slots/Slot_1"));
        slots.GetArrayElementAtIndex(1).objectReferenceValue =
            GetComp<CustomerSlotUI>(FindDescendant(canvas, "Top_Zone/Customer_Slots/Slot_2"));
        slots.GetArrayElementAtIndex(2).objectReferenceValue =
            GetComp<CustomerSlotUI>(FindDescendant(canvas, "Top_Zone/Customer_Slots/Slot_3"));

        var db = so.FindProperty("customerDatabase");
        db.arraySize = 3;
        db.GetArrayElementAtIndex(0).objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<CustomerData>(CustomerDbPath + "Khach1.asset");
        db.GetArrayElementAtIndex(1).objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<CustomerData>(CustomerDbPath + "Khach2.asset");
        db.GetArrayElementAtIndex(2).objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<CustomerData>(CustomerDbPath + "Khach3.asset");

        so.ApplyModifiedProperties();
        Debug.Log("✅ CustomerSpawner đã wire: 3 slots + 3 customer database.");
    }

    // =====================================================================
    //  CUSTOMER SLOT UI — Slot_1/2/3
    // =====================================================================
    private static void WireCustomerSlots(GameObject slotsParent)
    {
        if (slotsParent == null)
        {
            Debug.LogWarning("Không thấy Customer_Slots, bỏ qua CustomerSlotUI.");
            return;
        }

        string[] names = { "Slot_1", "Slot_2", "Slot_3" };
        foreach (var n in names)
        {
            var slot = FindDescendant(slotsParent, n);
            if (slot == null)
            {
                Debug.LogWarning("Không thấy " + n);
                continue;
            }

            var c = EnsureComponent<CustomerSlotUI>(slot);
            var so = new SerializedObject(c);
            so.FindProperty("avatarImage").objectReferenceValue =
                GetComp<Image>(FindDescendant(slot, "Avatar_Khach"));
            so.FindProperty("orderBubble").objectReferenceValue =
                FindDescendant(slot, "Order_Bubble");
            so.FindProperty("orderItemImage").objectReferenceValue =
                GetComp<Image>(FindDescendant(slot, "Order_Bubble/Mon_1"));
            so.FindProperty("patienceBar").objectReferenceValue =
                GetComp<Image>(FindDescendant(slot, "Patience_Bar/Fill Area/Fill"));
            so.ApplyModifiedProperties();
        }

        Debug.Log("✅ CustomerSlotUI đã gắn & wire cho Slot_1/2/3.");
    }

    // =====================================================================
    //  PLATE + PLATE MANAGER — Dia1/Dia 2/Dia 3
    // =====================================================================
    private static void WirePlates(GameObject diaParent)
    {
        if (diaParent == null)
        {
            Debug.LogWarning("Không thấy 3Dia, bỏ qua Plate.");
            return;
        }

        string[] names = { "Dia1", "Dia 2", "Dia 3" };
        foreach (var n in names)
        {
            var dia = FindDescendant(diaParent, n);
            if (dia == null)
            {
                Debug.LogWarning("Không thấy " + n);
                continue;
            }

            // Button (nếu chưa có)
            var btn = dia.GetComponent<Button>();
            if (btn == null) btn = dia.AddComponent<Button>();

            var img = dia.GetComponent<Image>();
            if (img != null)
            {
                var bso = new SerializedObject(btn);
                bso.FindProperty("m_TargetGraphic").objectReferenceValue = img;
                bso.ApplyModifiedProperties();
            }

            // Plate
            var plate = EnsureComponent<Plate>(dia);
            var so = new SerializedObject(plate);
            so.FindProperty("foodImage").objectReferenceValue = img;
            so.FindProperty("plateButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            // Wire OnClick → Plate.OnPlateClicked
            WireButtonClick(btn, plate, "OnPlateClicked");
        }

        Debug.Log("✅ Plate + Button đã gắn & wire cho Dia1/Dia 2/Dia 3.");

        // PlateManager trên 3Dia
        var pm = EnsureComponent<PlateManager>(diaParent);
        var pso = new SerializedObject(pm);
        var list = pso.FindProperty("plates");
        list.arraySize = 3;
        for (int i = 0; i < names.Length; i++)
        {
            var dia = FindDescendant(diaParent, names[i]);
            list.GetArrayElementAtIndex(i).objectReferenceValue =
                dia != null ? dia.GetComponent<Plate>() : null;
        }
        pso.ApplyModifiedProperties();

        Debug.Log("✅ PlateManager đã gắn & wire cho 3Dia.");
    }

    // =====================================================================
    //  GRILL SLOT — Vi1/Vi2
    // =====================================================================
    private static void WireGrills(GameObject grillParent)
    {
        if (grillParent == null)
        {
            Debug.LogWarning("Không thấy Cot_Vi_Nuong, bỏ qua GrillSlot.");
            return;
        }

        string[] names = { "Vi1", "Vi2" };
        foreach (var n in names)
        {
            var grill = FindDescendant(grillParent, n);
            if (grill == null)
            {
                Debug.LogWarning("Không thấy " + n);
                continue;
            }

            var btn = grill.GetComponent<Button>();
            if (btn == null) btn = grill.AddComponent<Button>();
            var img = grill.GetComponent<Image>();

            var g = EnsureComponent<GrillSlot>(grill);
            var so = new SerializedObject(g);
            so.FindProperty("foodImage").objectReferenceValue = img;
            so.FindProperty("slotButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();

            WireButtonClick(btn, g, "OnGrillClicked");
        }

        Debug.Log("✅ GrillSlot đã gắn & wire cho Vi1/Vi2.");
    }

    // =====================================================================
    //  BUILD SETTINGS — thêm UI.unity
    // =====================================================================
    private static void AddSceneToBuildSettings()
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        foreach (var s in EditorBuildSettings.scenes)
        {
            if (s.path != ScenePath)
                list.Add(s);
        }
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("✅ UI.unity đã thêm vào Build Settings (vị trí đầu).");
    }

    // =====================================================================
    //  HELPERS
    // =====================================================================
    private static void WireButtonClick(Button btn, Component target, string methodName)
    {
        if (btn == null || target == null) return;

        // Xoá listener cũ (tránh duplicate)
        var evt = btn.onClick;
        for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(evt, i);

        var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        if (method == null)
        {
            // Try non-public as fallback
            method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        }
        if (method == null)
        {
            Debug.LogWarning($"Không tìm thấy method {methodName} trên {target.GetType().Name}.");
            return;
        }

        var action = Delegate.CreateDelegate(typeof(UnityAction), target, method) as UnityAction;
        if (action != null)
        {
            UnityEventTools.AddPersistentListener(evt, action);
        }
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    private static T GetComp<T>(GameObject go) where T : Component
    {
        return go != null ? go.GetComponent<T>() : null;
    }

    private static GameObject FindRoot(GameObject[] roots, string name)
    {
        foreach (var r in roots)
        {
            if (r.name == name) return r;
        }
        return null;
    }

    private static GameObject FindDescendant(GameObject parent, string path)
    {
        if (parent == null) return null;
        var t = parent.transform.Find(path);
        return t != null ? t.gameObject : null;
    }
}
#endif
