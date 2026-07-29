using UnityEngine;
using TMPro;

[System.Serializable]
public class LevelConfig
{
    public int thoiGian;     
    public int mucTieuVang;  
}

public class GameManager : MonoBehaviour
{
    [Header("Giao diện UI")]
    public TextMeshProUGUI textThoiGian;
    public TextMeshProUGUI textVang;
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private CustomerSlotUI[] customerSlots;

    [Header("Cấu hình các màn chơi")]
    public LevelConfig[] danhSachManChoi;
    
    public int manChoiHienTai = 0; 
    private int vangHienTai = 0;
    private float thoiGianConLai = 0f;

    void Start()
    {
        if (customerSpawner == null)
        {
            customerSpawner = FindObjectOfType<CustomerSpawner>();
        }

        if (customerSlots == null || customerSlots.Length == 0)
        {
            customerSlots = FindObjectsOfType<CustomerSlotUI>();
        }

        BatDauManChoi(manChoiHienTai);
    }

    void BatDauManChoi(int levelIndex)
    {
        if (danhSachManChoi == null || danhSachManChoi.Length == 0)
        {
            Debug.LogWarning("Chưa cấu hình danh sách màn chơi.");
            return;
        }

        if (levelIndex < 0 || levelIndex >= danhSachManChoi.Length)
        {
            levelIndex = 0;
        }

        LevelConfig levelData = danhSachManChoi[levelIndex];
        vangHienTai = 0;
        thoiGianConLai = levelData != null ? levelData.thoiGian : 0;
        CapNhatUIVang(levelData != null ? levelData.mucTieuVang : 0);
    }

    void Update()
    {
        if (thoiGianConLai > 0)
        {
            thoiGianConLai -= Time.deltaTime;
            if (textThoiGian != null)
            {
                textThoiGian.text = Mathf.CeilToInt(thoiGianConLai).ToString() + "s";
            }

            if (thoiGianConLai <= 0)
            {
                thoiGianConLai = 0;
                Debug.Log("Hết giờ! Hiện Popup Game Over hoặc Chiến thắng.");
            }
        }
    }

    public void CongVang(int soVangNhanDuoc)
    {
        vangHienTai += soVangNhanDuoc;
        LevelConfig levelData = danhSachManChoi[manChoiHienTai];
        CapNhatUIVang(levelData.mucTieuVang);
        
        if (vangHienTai >= levelData.mucTieuVang)
        {
            Debug.Log("Đủ vàng! Bạn đã thắng màn này!");
        }
    }

    void CapNhatUIVang(int mucTieu)
    {
        if (textVang != null)
        {
            textVang.text = vangHienTai.ToString() + "/" + mucTieu.ToString();
        }
    }
}