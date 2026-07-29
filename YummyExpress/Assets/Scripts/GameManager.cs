using UnityEngine;
using UnityEngine.UI;
using TMPro; // 1. Thêm dòng này để gọi thư viện TextMeshPro

[System.Serializable]
public class LevelConfig
{
    public int thoiGian;     
    public int mucTieuVang;  
}

public class GameManager : MonoBehaviour
{
    [Header("Giao diện UI (Kéo thả từ Hierarchy vào đây)")]
    // 2. Đổi 'Text' thành 'TextMeshProUGUI'
    public TextMeshProUGUI textThoiGian; 
    public TextMeshProUGUI textVang;     

    [Header("Cấu hình các màn chơi")]
    public LevelConfig[] danhSachManChoi;
    
    public int manChoiHienTai = 0; 
    private int vangHienTai = 0;
    private float thoiGianConLai = 0f;

    void Start()
    {
        BatDauManChoi(manChoiHienTai);
    }

    void BatDauManChoi(int levelIndex)
    {
        LevelConfig levelData = danhSachManChoi[levelIndex];
        vangHienTai = 0;
        thoiGianConLai = levelData.thoiGian;
        CapNhatUIVang(levelData.mucTieuVang);
    }

    void Update()
    {
        if (thoiGianConLai > 0)
        {
            thoiGianConLai -= Time.deltaTime; 
            textThoiGian.text = Mathf.CeilToInt(thoiGianConLai).ToString() + "s";
            
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
        textVang.text = vangHienTai.ToString() + "/" + mucTieu.ToString();
    }
}