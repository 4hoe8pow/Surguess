using HoudiniEngineUnity;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Text timerText;
    public Button[] colorButtons;
    public HDAAttributeReader attributeReader;

    private float timeRemaining = 60f;
    private bool isGameActive = true;
    private int correctTotalIndex;

    void Start()
    {
        colorButtons
            .Select((button, index) => new { button, index })
            .ToList()
            .ForEach(b => b.button.onClick.AddListener(() => OnColorButtonPressed(b.index + 1)));

        UpdateTimerText();
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

    void OnColorButtonPressed(int buttonIndex)
    {
        if (!isGameActive) return;

        correctTotalIndex = attributeReader?.GetLargestTotalIndex() ?? 0;
        (buttonIndex == correctTotalIndex ? (System.Action)OnCorrectButtonPressed : () => EndGame("Incorrect guess!"))();
    }

    void OnCorrectButtonPressed()
    {
        timeRemaining += 2f;

        var hdaAsset = attributeReader.GetComponentInChildren<HEU_HoudiniAsset>();
        var hdaParams = hdaAsset != null ? hdaAsset.Parameters : null;

        if (hdaParams != null)
        {
            hdaParams.ApplyHDAParameters(
                Random.ColorHSV(), Random.ColorHSV(), Random.ColorHSV(), Random.ColorHSV(),
                Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f)
            );

            // Apply the changes to the asset
            hdaAsset.RequestCook(true);
        }
        else
        {
            Debug.LogError("HDA parameters not found.");
        }
    }

    void EndGame(string reason)
    {
        isGameActive = false;
        timerText.text = reason;
        colorButtons.ToList().ForEach(button => button.interactable = false);
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
