using HoudiniEngineUnity;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Text timerText;
    public Text scoreText;
    public Text bestScoreText;
    public Text errorMessageText;
    public Button[] colorButtons;
    public HDAAttributeReader attributeReader;
    public GameObject gameEndPopup;
    public Image dimmedBackground;

    private float timeRemaining = 60f;
    private bool isGameActive = true;
    private int correctTotalIndex;
    private int correctGuessCount = 0;
    private const string BestScoreKey = "BestScore";

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
        correctGuessCount++;
        scoreText.text = $"{correctGuessCount}";

        ApplyRandomHDAParameters();
    }

    void ApplyRandomHDAParameters()
    {
        // HDAアセットを取得
        var hdaAsset = attributeReader.GetComponentInChildren<HEU_HoudiniAsset>();


        if (hdaAsset == null)
        {
            DisplayErrorMessage("Houdini Asset not found in the Attribute Reader.");
            return;
        }

        var hdaParams = hdaAsset.Parameters;

        if (hdaParams == null)
        {
            DisplayErrorMessage("HDA parameters not found in the Houdini Asset.");
            return;
        }

        Color[] colors = null;
        try
        {
            colors = GenerateSquareHarmonyColors();
        }
        catch (System.Exception ex)
        {
            DisplayErrorMessage($"Error generating square harmony colors: {ex.Message}");
            return;
        }

        try
        {
            hdaParams.ApplyHDAParameters(colors[0], colors[1], colors[2], colors[3], Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
        }
        catch (System.Exception ex)
        {
            DisplayErrorMessage($"Error applying HDA parameters: {ex.Message}");
            return;
        }

        try
        {
            hdaAsset.RequestCook(true, true, true, true);
        }
        catch (System.Exception ex)
        {
            DisplayErrorMessage($"Error requesting cook on Houdini Asset: {ex.Message}");
            return;
        }

        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (!colorButtons[i].TryGetComponent<Image>(out var buttonImage))
            {
                DisplayErrorMessage($"Image component not found on button {colorButtons[i].name}.");
                continue;
            }

            buttonImage.color = colors[i];
        }
    }

    Color[] GenerateSquareHarmonyColors()
    {
        float minContrast = 0.4f;
        Color[] selectedColors;

        do
        {
            float baseHue = Random.Range(0f, 1f);
            selectedColors = new Color[4];

            for (int i = 0; i < 4; i++)
            {
                float hue = (baseHue + (i * 0.25f)) % 1f;
                selectedColors[i] = Color.HSVToRGB(hue, Random.Range(0.8f, 1f), Random.Range(0.8f, 1f));
            }
        }
        while (!AreColorsDistinct(selectedColors, minContrast));

        return selectedColors;
    }

    bool AreColorsDistinct(Color[] colors, float minContrast)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            for (int j = i + 1; j < colors.Length; j++)
            {
                if (ColorDifference(colors[i], colors[j]) < minContrast)
                {
                    return false;
                }
            }
        }
        return true;
    }

    float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b);
    }

    void EndGame(string reason)
    {
        isGameActive = false;
        timerText.text = reason;

        dimmedBackground.gameObject.SetActive(true);
        gameEndPopup.SetActive(true);

        int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (correctGuessCount > bestScore)
        {
            PlayerPrefs.SetInt(BestScoreKey, correctGuessCount);
            PlayerPrefs.Save();
        }

        UpdateBestScoreText();
        colorButtons.ToList().ForEach(button => button.interactable = false);

        if (correctTotalIndex < 1 || correctTotalIndex > colorButtons.Length)
        {
            DisplayErrorMessage($"Invalid correctTotalIndex: {correctTotalIndex}. It should be between 1 and {colorButtons.Length}.");
            return;
        }

        foreach (var button in colorButtons)
        {
            if (button == colorButtons[correctTotalIndex - 1])
            {
                continue;
            }

            if (button.TryGetComponent<Image>(out var buttonImage))
            {
                buttonImage.color = Color.clear;
            }
            else
            {
                DisplayErrorMessage($"Image component not found on button {button.name}.");
            }
        }
    }

    /// <summary>
    /// Displays error messages on the screen using a UI Text component.
    /// </summary>
    /// <param name="message">The error message to display</param>
    void DisplayErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text += $"{message}\n";

        }
        else
        {
            Debug.LogError("ErrorMessageText component is not assigned!");
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
