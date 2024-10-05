using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartGame : MonoBehaviour
{
    public Button restartButton;  // Assign this in the Inspector

    void Start()
    {
        // Add the listener to the button
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    // This function will be called when the button is clicked
    void OnRestartButtonClicked()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
