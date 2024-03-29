using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextSineScript : MonoBehaviour
{
    [SerializeField] private float frequency = 4f;
    [SerializeField] private float amplitude = 4f;
    [SerializeField] private float offsetAmt = 1f;

    private string sourceString;
    private TextMeshProUGUI meshText;

    void Start()
    {
        meshText = GetComponent<TextMeshProUGUI>();
        sourceString = meshText.text;
    }
    

    void Update()
    {
        WiggleThings();
    }

    private void WiggleThings()
    {
        string newString = sourceString;
        int currentStartIndex = 0;

        // For each <voffset> group
        for (int i = 0; i < Regex.Matches(sourceString, "<voffset=0px>").Count; i++)
        {
            // Get index of number to change (default one character "0")
            int index = newString.IndexOf("<voffset=", currentStartIndex) + 9;

            // Calculate offset for current character
            float voffset = Mathf.Sin(Time.time * frequency + offsetAmt * i) * amplitude;

            // Replace 0 with proper voffset
            newString = newString.Remove(index, 1).Insert(index, voffset.ToString());

            // Set up next starting index to look for next voffset
            currentStartIndex = index;
        }
        
        meshText.text = newString;
    }
}
