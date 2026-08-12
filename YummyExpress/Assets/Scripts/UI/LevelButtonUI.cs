using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    public int levelNumber = 1;

    [Header("Stars")]
    public Image[] yellowStars;   // Sao_Vang
    public Image[] grayStars;     // Sao_Đen

    [Header("Lock")]
    public GameObject lockIcon;
    public Button playButton;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        int stars = PlayerPrefs.GetInt($"Level{levelNumber}Stars", 0);

        for (int i = 0; i < 3; i++)
        {
            yellowStars[i].gameObject.SetActive(i < stars);
            grayStars[i].gameObject.SetActive(i >= stars);
        }

        bool unlocked = levelNumber == 1 ||
                        PlayerPrefs.GetInt($"Level{levelNumber}Unlocked", 0) == 1;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (playButton != null)
            playButton.interactable = unlocked;
    }
}