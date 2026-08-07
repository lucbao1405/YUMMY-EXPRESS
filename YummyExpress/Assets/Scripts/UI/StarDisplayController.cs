using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StarDisplayController : MonoBehaviour
{
    // GUID cố định của 2 sprite sao trong project.
    // Sao sáng (Vàng) = "sao sáng .png" → guid 7c532f4d59291614f929e205233d01d7
    // Sao fail (Xám)  = "sao fail.png"   → guid f18ec8a5b535bd74eb0323f097da3294
    private const string YELLOW_STAR_GUID = "7c532f4d59291614f929e205233d01d7";
    private const string GRAY_STAR_GUID = "f18ec8a5b535bd74eb0323f097da3294";

    [Header("3 Ô Image Sao (Element 0: Trái, Element 1: Giữa, Element 2: Phải)")]
    [SerializeField] private List<Image> starImages;

    [Header("Sprite Resources")]
    [SerializeField] private Sprite yellowStarSprite; // Sprite Sao Vàng
    [SerializeField] private Sprite grayStarSprite;   // Sprite Sao Xám/Đen

    // Container Khung_3_Sao_Toi (lớp phủ sao tối CŨ) — phải ẩn đi vì nó nằm TRÊN Khung_3_Sao
    // theo thứ tự sibling (child thứ 2 render lên trên child thứ 1 trong Unity UI),
    // nếu không sẽ che hết sao vàng → hiển thị 3 sao xám dù SetStars đã chạy đúng.
    private Transform khungSaoToi;

    private void Awake()
    {
        // 0. Cache container "Khung_3_Sao_Toi" rồi ẩn nó (lớp phủ sao tối cũ).
        khungSaoToi = transform.Find("Khung_3_Sao_Toi");

        // 1. Tự động tìm 3 ô Sao (Sao_1, Sao_2, Sao_3) trong Khung_3_Sao.
        AutoResolveStarImages();

        // 2. Tự động nạp 2 Sprite Sao nếu chưa được kéo trong Inspector.
        AutoResolveStarSprites();

        // 3. Ẩn hẳn Khung_3_Sao_Toi ngay khi khởi tạo để không che sao vàng.
        HideDarkOverlay();
    }

    /// <summary>
    /// Tự động tìm 3 ô Sao theo tên trong CHỈ duy nhất container "Khung_3_Sao".
    /// Sao_1 = Trái, Sao_2 = Giữa, Sao_3 = Phải.
    /// LƯU Ý: KHÔNG dò trong toàn bộ cây con vì scene còn "Khung_3_Sao_Toi"
    /// (bộ sao tối cũ) cũng chứa Sao_1/2/3 → dễ lấy nhầm ô sai.
    /// </summary>
    private void AutoResolveStarImages()
    {
        if (starImages == null) starImages = new List<Image>();

        // Đảm bảo list có đủ 3 phần tử.
        while (starImages.Count < 3)
        {
            starImages.Add(null);
        }

        // 1. Tìm container "Khung_3_Sao" (chứa 3 ô sao sáng cần hiển thị).
        Transform khung = transform.Find("Khung_3_Sao");
        if (khung == null)
        {
            // Fallback: nếu không tìm thấy theo tên, dùng child đầu tiên.
            if (transform.childCount > 0)
            {
                khung = transform.GetChild(0);
            }
        }

        if (khung != null)
        {
            // 2. Tìm Sao_1, Sao_2, Sao_3 bên TRONG Khung_3_Sao (không lẫn Khung_3_Sao_Toi).
            for (int i = 0; i < 3; i++)
            {
                if (starImages[i] == null)
                {
                    Transform t = khung.Find($"Sao_{i + 1}");
                    if (t == null)
                    {
                        // Fallback tìm trong toàn bộ con/cháu của Khung_3_Sao.
                        Image[] imgs = khung.GetComponentsInChildren<Image>(true);
                        foreach (Image img in imgs)
                        {
                            if (img != null && img.gameObject.name == $"Sao_{i + 1}")
                            {
                                starImages[i] = img;
                                break;
                            }
                        }
                    }
                    else
                    {
                        starImages[i] = t.GetComponent<Image>();
                    }
                }
            }
        }

        if (starImages[0] == null || starImages[1] == null || starImages[2] == null)
        {
            Debug.LogWarning("[StarDisplay] Không tìm thấy đủ 3 ô Sao (Sao_1, Sao_2, Sao_3) " +
                             "bên trong Khung_3_Sao. Hãy kéo chúng vào Inspector theo thứ tự Trái - Giữa - Phải.", this);
        }
    }

    /// <summary>
    /// Tự động nạp 2 Sprite Sao (Vàng / Xám) nếu chưa được gán trong Inspector.
    /// Ở Editor: dùng AssetDatabase (tìm theo GUID). Ở Build: fallback Resources.Load.
    /// </summary>
    private void AutoResolveStarSprites()
    {
        if (yellowStarSprite == null)
        {
            yellowStarSprite = LoadSpriteByGuid(YELLOW_STAR_GUID);
        }
        if (grayStarSprite == null)
        {
            grayStarSprite = LoadSpriteByGuid(GRAY_STAR_GUID);
        }
    }

    /// <summary>
    /// Nạp Sprite từ một asset có GUID cho trước.
    /// - Trong Editor: AssetDatabase.GUIDToAssetPath + LoadAssetAtPath (chính xác theo GUID).
    /// - Trong Build: thử Resources.Load theo tên (nếu sprite được đặt trong thư mục Resources).
    /// </summary>
    private Sprite LoadSpriteByGuid(string guid)
    {
#if UNITY_EDITOR
        // 1. Editor: tìm đường dẫn asset từ GUID rồi nạp Sprite trực tiếp.
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(path))
        {
            Sprite loaded = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (loaded != null)
            {
                return loaded;
            }
        }
#endif

        // 2. Fallback runtime: Resources.Load theo tên file (không có phần mở rộng).
        string starName = (guid == YELLOW_STAR_GUID) ? "sao sáng" : "sao fail";
        string[] possiblePaths =
        {
            "Sprites/Food/Nguyen_lieu/" + starName,
            "Sprites/" + starName,
            starName
        };

        foreach (string p in possiblePaths)
        {
            Sprite s = Resources.Load<Sprite>(p);
            if (s != null) return s;
        }

        Debug.LogWarning($"[StarDisplay] Không tìm thấy Sprite GUID '{guid}' ({starName}). " +
                         "Hãy kéo Sprite Sao Vàng / Sao Xám vào Inspector.", this);
        return null;
    }

    /// <summary>
    /// Ẩn container Khung_3_Sao_Toi (lớp phủ sao tối cũ) để không che các sao vàng.
    /// </summary>
    private void HideDarkOverlay()
    {
        if (khungSaoToi != null && khungSaoToi.gameObject.activeSelf)
        {
            khungSaoToi.gameObject.SetActive(false);
            Debug.Log("[StarDisplay] Đã ẩn Khung_3_Sao_Toi (lớp phủ sao tối cũ).");
        }
    }

    /// <summary>
    /// Gọi hàm này để cập nhật UI Sao
    /// </summary>
    public void SetStars(int starCount)
    {
        // Luôn ẩn lớp phủ sao tối cũ trước khi cập nhật (phòng trường hợp popup bật lại sau Awake).
        HideDarkOverlay();

        if (starImages == null || starImages.Count < 3)
        {
            Debug.LogError("[StarDisplay] Chưa kéo đủ 3 Image Sao vào Inspector!");
            return;
        }

        if (yellowStarSprite == null || grayStarSprite == null)
        {
            Debug.LogError("[StarDisplay] Chưa kéo Sprite Sao Vàng / Sao Xám vào Inspector!");
            return;
        }

        // Đảm bảo khi Level Cleared thì số sao luôn từ 1 đến 3
        starCount = Mathf.Clamp(starCount, 1, 3);

        // Đổi Sprite từ trái sang phải theo số sao
        for (int i = 0; i < 3; i++)
        {
            if (starImages[i] != null)
            {
                // Nếu Index < starCount thì thành Sao Vàng, ngược lại là Sao Xám
                starImages[i].sprite = (i < starCount) ? yellowStarSprite : grayStarSprite;
                starImages[i].gameObject.SetActive(true);
            }
        }

        Debug.Log($"[StarDisplay] Đã hiển thị {starCount} sao từ trái sang phải thành công!");
    }
}
