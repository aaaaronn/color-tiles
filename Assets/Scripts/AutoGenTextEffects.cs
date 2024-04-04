using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class AutoGenTextEffects : MonoBehaviour
{
    [Header("Sine Wave")]
    [SerializeField] private bool doSineWave = true;
    [SerializeField] private float frequency = 4f;
    [SerializeField] private float amplitude = 4f;
    [SerializeField] private float offsetAmt = 1f;

    [Header("Color")]
    [SerializeField] private bool doColorChange = true;
    [SerializeField] private Color[] colorArray;

    private string sourceString;
    private TextMeshProUGUI meshText;
    private string lastExitMarkup;
    private int InitTextLength;

    void Start()
    {
        meshText = GetComponent<TextMeshProUGUI>();
        sourceString = meshText.text;
        InitTextLength = sourceString.Length;

        if (doColorChange && colorArray.Length > 0)
        {
            int length = sourceString.Length;
            int currentStartIndex = 0;
            int colorIndex = 0;
            for (int i = 0; i < length; i++)
            {
                if (sourceString[currentStartIndex] == ' ')
                {
                    currentStartIndex++;
                    continue;
                }

                sourceString = sourceString.Insert(currentStartIndex + 1, "</color>").Insert(currentStartIndex, $"<color=#{ColorUtility.ToHtmlStringRGB(colorArray[colorIndex % colorArray.Length])}>");

                currentStartIndex = sourceString.IndexOf("</color>", currentStartIndex) + 8;
                //print(currentStartIndex + "  " + sourceString);
                colorIndex++;
            }
            meshText.text = sourceString;
            lastExitMarkup = "</color>";
        }
    }


    void Update()
    {
        if (doSineWave)
        {
            WiggleThings();
        }
    }

    readonly int wiggleAmtInserted = "</voffset><voffset=-0.1234567px>".Length + 4;
    private void WiggleThings()
    {
        string newString = sourceString;
        int currentStartIndex = sourceString.IndexOf(lastExitMarkup);

        for (int i = 0; i < InitTextLength - 1; i++)
        {
            // Calculate offset for current character
            float voffset = Mathf.Sin(Time.time * frequency + offsetAmt * i) * amplitude;

            // Replace 0 with proper voffset
            newString = newString.Insert(currentStartIndex, "</voffset>").Insert(currentStartIndex - 1, "<voffset=" + voffset + "px>");

            // Gets index of next character
            if (i != InitTextLength - 2)
                currentStartIndex = newString.IndexOf(lastExitMarkup, currentStartIndex + wiggleAmtInserted);
        }
        meshText.text = newString;
    }
}
