using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    void Update()
    {
        // マウスクリックまたはタッチを検知
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            // シーンを切り替える
            SceneManager.LoadScene("InGameScene");
        }
    }
}
