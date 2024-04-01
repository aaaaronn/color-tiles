using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        meshText = GetComponent<TextMeshProUGUI>();
        sourceString = meshText.text;

        if (doColorChange && colorArray.Length > 0)
        {
            int length = sourceString.Length;
            int currentStartIndex = 0;
            int colorIndex = 0;
            for (int i = 0; i < length; i++)
            {
                print(sourceString[currentStartIndex]);
                if (sourceString[currentStartIndex] == ' ')
                {
                    currentStartIndex++;
                    colorIndex--;
                    continue;
                }
                print(sourceString[currentStartIndex]);

                sourceString = sourceString.Insert(currentStartIndex + 1, "</color>").Insert(currentStartIndex, $"<color=#{ColorUtility.ToHtmlStringRGB(colorArray[colorIndex % colorArray.Length])}>");

                currentStartIndex = sourceString.IndexOf("</color>", currentStartIndex) + 8;
                //print(currentStartIndex + "  " + sourceString);
                colorIndex++;
            }
            meshText.text = sourceString;
        }
    }


    void Update()
    {
        if (doSineWave)
        {
            WiggleThings();
        }
    }

    private void WiggleThings()
    {
        string newString = sourceString;
        int currentStartIndex = 0;

        for (int i = 0; i < sourceString.Length; i++)
        {
            // Calculate offset for current character
            float voffset = Mathf.Sin(Time.time * frequency + offsetAmt * i) * amplitude;

            // Replace 0 with proper voffset
            newString = newString.Insert(currentStartIndex + 1, "</voffset>").Insert(currentStartIndex, "<voffset=" + voffset + "px>");

            // Gets index of next character
            currentStartIndex = newString.IndexOf("</voffset>", currentStartIndex) + 10;
        }

        meshText.text = newString;
    }
}
