using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Gọi hàm này khi bấm nút Play
    public void LoadLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection"); 
    }
}