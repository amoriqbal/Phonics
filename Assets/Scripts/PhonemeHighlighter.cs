using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class PhonemeHighlighter : MonoBehaviour
{
    private Dictionary<string, (Color color, FontStyles style)> originalTextStyles = new();
    private Dictionary<string, Color> originalLineColors = new();

    private Dictionary<string, string> phonemeToLineName = new()
    {
        { "p", "Line (0)" }, { "dh", "Line (1)" }, { "th", "Line (2)" }, { "v", "Line (3)" },
        { "f", "Line (4)" }, { "j", "Line (5)" }, { "ch", "Line (6)" }, { "g", "Line (7)" },
        { "k", "Line (8)" }, { "d", "Line (9)" }, { "t", "Line (10)" }, { "b", "Line (11)" },
        { "ar", "Line (12)" }, { "oor", "Line (13)" }, { "air", "Line (14)" }, { "ier", "Line (15)" },
        { "ow", "Line (16)" }, { "aw", "Line (17)" }, { "oy", "Line (18)" }, { "ay", "Line (19)" },
        { "ey", "Line (20)" }, { "er", "Line (21)" }, { "ao", "Line (22)" }, { "aa", "Line (23)" },
        { "uw", "Line (25)" }, { "iy", "Line (26)" }, { "hh", "Line (27)" }, { "ah", "Line (28)" },
        { "uh", "Line (29)" }, { "ae", "Line (30)" }, { "eh", "Line (31)" }, { "ih", "Line (32)" },
        { "w", "Line (33)" }, { "y", "Line (34)" }, { "r", "Line (35)" }, { "i", "Line (36)" },
        { "ng", "Line (37)" }, { "n", "Line (38)" }, { "m", "Line (39)" }, { "h", "Line (40)" },
        { "zh", "Line (41)" }, { "sh", "Line (42)" }, { "z", "Line (43)" }, { "s", "Line (44)" }
    };

    private HashSet<string> validPhonemes;

    void Start()
    {
        validPhonemes = new HashSet<string>(phonemeToLineName.Keys);
        CacheOriginalStyles();
        CacheOriginalLineColors();
    }

    void CacheOriginalStyles()
    {
        foreach (var phoneme in validPhonemes)
        {
            TMP_Text tmp = FindPhonemeText(phoneme);
            if (tmp != null)
            {
                originalTextStyles[phoneme] = (tmp.color, tmp.fontStyle);
            }
        }
    }

    void CacheOriginalLineColors()
    {
        foreach (var pair in phonemeToLineName)
        {
            GameObject lineGO = GameObject.Find(pair.Value);
            if (lineGO != null)
            {
                RawImage img = lineGO.GetComponent<RawImage>();
                if (img != null)
                {
                    originalLineColors[pair.Key] = img.color;
                }
            }
        }
    }

    public void ResetHighlights()
    {
        // Reset TMP_Text
        foreach (var phoneme in originalTextStyles.Keys)
        {
            TMP_Text tmp = FindPhonemeText(phoneme);
            if (tmp != null)
            {
                var (color, style) = originalTextStyles[phoneme];
                tmp.color = color;
                tmp.fontStyle = style;
            }
        }

        // Reset RawImage colors
        foreach (var pair in phonemeToLineName)
        {
            GameObject lineGO = GameObject.Find(pair.Value);
            if (lineGO != null && originalLineColors.ContainsKey(pair.Key))
            {
                RawImage img = lineGO.GetComponent<RawImage>();
                if (img != null)
                {
                    img.color = originalLineColors[pair.Key];
                }
            }
        }
    }

    public void HighlightPhoneme(string phoneme)
    {
        if (string.IsNullOrEmpty(phoneme)) return;

        phoneme = phoneme.ToLower();

        if (!validPhonemes.Contains(phoneme))
        {
            Debug.LogWarning($"Phoneme '{phoneme}' not in valid phoneme list.");
            return;
        }

        ResetHighlights();

        // Highlight text
        TMP_Text tmp = FindPhonemeText(phoneme);
if (tmp != null)
{
    tmp.color = new Color(1f, 0.5f, 0f); // Orange
    tmp.fontStyle = FontStyles.Bold;
}

        // Highlight RawImage line
        if (phonemeToLineName.TryGetValue(phoneme, out string lineName))
        {
            GameObject lineGO = GameObject.Find(lineName);
            if (lineGO != null)
            {
                RawImage img = lineGO.GetComponent<RawImage>();
                if (img != null)
                {
                    img.color = Color.yellow;
                }
                else
                {
                    Debug.LogWarning($"RawImage component missing on '{lineName}'");
                }
            }
            else
            {
                Debug.LogWarning($"Line object '{lineName}' not found.");
            }
        }
    }

    TMP_Text FindPhonemeText(string phoneme)
    {
        GameObject go = GameObject.Find(phoneme);
        if (go != null)
        {
            return go.GetComponent<TMP_Text>();
        }
        return null;
    }
}

