using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingPanel; 
    public Image loadingFillImage;  
    public TextMeshProUGUI progressText; 
    
    [Header("Settings")]
    public float minLoadingTime = 2f; // <--- CÁI NÀY SẼ HIỆN RA NÀY

    public void LoadLevel(int sceneIndex)
    {
        Debug.Log("Đã bấm nút Start!");
        loadingPanel.SetActive(true);
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false; 

        float currentTime = 0f;

        while (currentTime < minLoadingTime)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / minLoadingTime);

            if (loadingFillImage != null)
            {
                loadingFillImage.fillAmount = progress; 
            }

            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
            }

            yield return null; 
        }

        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}
