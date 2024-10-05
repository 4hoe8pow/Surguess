using HoudiniEngineUnity;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Text timerText;
    public Text scoreText;
    public Text bestScoreText;
    public Button[] colorButtons;
    public HDAAttributeReader attributeReader;
    public GameObject gameEndPopup;  // Panel for game over popup
    public Image dimmedBackground;   // Dim background image

    private float timeRemaining = 60f;
    private bool isGameActive = true;
    private int correctTotalIndex;
    private int correctGuessCount = 0;  // Track correct guesses
    private const string BestScoreKey = "BestScore";  // Key for best score in PlayerPrefs

    void Start()
    {
        // Apply initial random parameters to HDA and button colors
        ApplyRandomHDAParameters();

        // Set up button listeners
        colorButtons
            .Select((button, index) => new { button, index })
            .ToList()
            .ForEach(b => b.button.onClick.AddListener(() => OnColorButtonPressed(b.index + 1)));

        UpdateTimerText();
        UpdateBestScoreText();
    }

    void Update()
    {
        if (!isGameActive || (timeRemaining -= Time.deltaTime) > 0)
        {
            UpdateTimerText();
        }
        else
        {
            EndGame("Time's up!");
        }
    }

    void UpdateTimerText() =>
        timerText.text = timerText != null ? $"Time: {Mathf.Max(0, Mathf.RoundToInt(timeRemaining))}" : throw new System.Exception("TimerText is not assigned!");

    void UpdateBestScoreText()
    {
        int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        bestScoreText.text = $"{bestScore}";
    }

    void OnColorButtonPressed(int buttonIndex)
    {
        if (!isGameActive) return;

        correctTotalIndex = attributeReader?.GetLargestTotalIndex() ?? 0;
        (buttonIndex == correctTotalIndex ? (System.Action)OnCorrectButtonPressed : () => EndGame("Incorrect guess!"))();
    }

    void OnCorrectButtonPressed()
    {
        timeRemaining += 2f;
        correctGuessCount++;  // Increment correct guesses
        scoreText.text = $"{correctGuessCount}";  // Update score display

        ApplyRandomHDAParameters();
    }

    void ApplyRandomHDAParameters()
{
    // HDAアセットを取得
    var hdaAsset = attributeReader.GetComponentInChildren<HEU_HoudiniAsset>();
    
    // HDAアセットが見つからなかった場合のエラーハンドリング
    if (hdaAsset == null)
    {
        Debug.LogError("Houdini Asset not found in the Attribute Reader.");
        return;
    }

    // HDAパラメータを取得
    var hdaParams = hdaAsset.Parameters;
    
    // HDAパラメータが見つからなかった場合のエラーハンドリング
    if (hdaParams == null)
    {
        Debug.LogError("HDA parameters not found in the Houdini Asset.");
        return;
    }

    // ランダムカラーを生成
    Color[] colors = null;
    try
    {
        colors = GenerateSquareHarmonyColors();
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error generating square harmony colors: {ex.Message}");
        return;
    }

    // HDAパラメータにランダム値を適用
    try
    {
        hdaParams.ApplyHDAParameters(colors[0], colors[1], colors[2], colors[3], Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error applying HDA parameters: {ex.Message}");
        return;
    }

    // アセットの変更を適用
    try
    {
        hdaAsset.RequestCook(true);
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error requesting cook on Houdini Asset: {ex.Message}");
        return;
    }

    // ボタンの色を更新
    for (int i = 0; i < colorButtons.Length; i++)
    {
        
        // ボタンイメージが見つからない場合のエラーハンドリング
        if (!colorButtons[i].TryGetComponent<Image>(out var buttonImage))
        {
            Debug.LogError($"Image component not found on button {colorButtons[i].name}.");
            continue; // スキップして次のボタンに進む
        }

        buttonImage.color = colors[i];
    }
}


    /// <summary>
    /// Generates four distinct colors that form a square-like harmony on the color wheel.
    /// </summary>
    Color[] GenerateSquareHarmonyColors()
    {
        float minContrast = 0.4f; // Minimum contrast to avoid colors being too similar
        Color[] selectedColors;

        do
        {
            // Generate four colors equally spaced on the hue wheel (90 degrees apart)
            float baseHue = Random.Range(0f, 1f);
            selectedColors = new Color[4];

            for (int i = 0; i < 4; i++)
            {
                // Offset hue by 90 degrees (1/4 of the color wheel)
                float hue = (baseHue + (i * 0.25f)) % 1f;
                selectedColors[i] = Color.HSVToRGB(hue, Random.Range(0.8f, 1f), Random.Range(0.8f, 1f)); // High saturation and brightness
            }
        }
        // Repeat generation until all colors have sufficient contrast
        while (!AreColorsDistinct(selectedColors, minContrast));

        return selectedColors;
    }

    /// <summary>
    /// Ensures that all colors have a minimum contrast and are distinct from each other.
    /// </summary>
    bool AreColorsDistinct(Color[] colors, float minContrast)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = i + 1; j < colors.Length; j++)
            {
                if (ColorDifference(colors[i], colors[j]) < minContrast)
                {
                    return false; // Too similar
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Computes the perceptual difference between two colors.
    /// </summary>
    float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b);
    }

    void EndGame(string reason)
{
    isGameActive = false;
    timerText.text = reason;

    // Dim background and show game end popup with score
    dimmedBackground.gameObject.SetActive(true);
    gameEndPopup.SetActive(true);

    // Update the best score
    int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    if (correctGuessCount > bestScore)
    {
        PlayerPrefs.SetInt(BestScoreKey, correctGuessCount);
        PlayerPrefs.Save();
    }

    UpdateBestScoreText();  // Show the best score on the popup
    colorButtons.ToList().ForEach(button => button.interactable = false);  // Disable buttons

    // 不正解のボタンの色を消す
    foreach (var button in colorButtons)
    {
        if (button == colorButtons[correctTotalIndex - 1]) // 正解のボタンはそのまま
        {
            continue;
        }

        // ボタンイメージを取得
        if (button.TryGetComponent<Image>(out var buttonImage))
        {
            buttonImage.color = Color.clear; // ボタンの色を透明にする
        }
        else
        {
            Debug.LogError($"Image component not found on button {button.name}.");
        }
    }
}

}

public static class HDAParameterExtensions
{
    public static void ApplyHDAParameters(this HEU_Parameters hdaParams, Color color1, Color color2, Color color3, Color color4, float colorBalance, float shape)
    {
        hdaParams.SetColorParameterValue("color1", color1);
        hdaParams.SetColorParameterValue("color2", color2);
        hdaParams.SetColorParameterValue("color3", color3);
        hdaParams.SetColorParameterValue("color4", color4);
        hdaParams.SetFloatParameterValue("colorBalance", colorBalance);
        hdaParams.SetFloatParameterValue("shape", shape);
    }
}
