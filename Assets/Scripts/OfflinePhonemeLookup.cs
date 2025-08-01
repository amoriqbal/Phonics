using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class OfflinePhonemeLookup : MonoBehaviour
{
    private Dictionary<string, string> phonemeDict = new();

    void Awake()
    {
        LoadDictionary();
    }

    private void LoadDictionary()
{
    TextAsset cmuText = Resources.Load<TextAsset>("cmudict");
    if (cmuText == null)
    {
        Debug.LogError("Phoneme dictionary file not found in Resources/cmudict");
        return;
    }

    string[] lines = cmuText.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    foreach (string line in lines)
    {
        if (line.StartsWith(";;;")) continue; // skip comments

        // Split by double space or multiple spaces, as some dict files separate by two spaces
        var parts = line.Split(new[] { "  ", " " }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            string word = parts[0].Trim().ToLower();

            // Remove possible "(number)" from word like "BAT(1)" => "bat"
            word = System.Text.RegularExpressions.Regex.Replace(word, @"\(\d+\)$", "");

            // Join the phonemes back together (parts[1..] all phonemes)
            string phonemeLine = string.Join(" ", parts, 1, parts.Length - 1);

            if (!phonemeDict.ContainsKey(word))
            {
                phonemeDict[word] = phonemeLine;
            }
            // If duplicates exist, you can handle them here (e.g., keep first)
        }
    }
}

public void LookupAndPrintFirstPhoneme(string word)
{
    if (string.IsNullOrWhiteSpace(word)) return;

    string cleanWord = word.Trim().ToLower();
    cleanWord = Regex.Replace(cleanWord, @"[^\w]", ""); // remove punctuation and dots

    if (phonemeDict.TryGetValue(cleanWord, out string phonemeLine))
    {
        string[] phonemes = phonemeLine.Split(' ');
        string firstPhoneme = Regex.Replace(phonemes[0], @"\d", ""); // remove stress markers like AH0

        Debug.Log($"First phoneme for '{word}': {firstPhoneme}");

        // Highlight in UI
        FindAnyObjectByType<PhonemeHighlighter>()?.HighlightPhoneme(firstPhoneme.ToLower());
    }
    else
    {
        Debug.LogWarning($"Word '{word}' not found in phoneme dictionary");
    }
}

}
