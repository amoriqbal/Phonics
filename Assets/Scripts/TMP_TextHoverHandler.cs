using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TMP_TextHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text displayField; // Assign in Inspector (one display field for all)
    private string originalText = "";

    private static readonly Dictionary<string, string> phonemeExamples = new Dictionary<string, string>()
    {
       { "p", "pin" },{ "b", "bat" },{ "t", "top" },{ "d", "dog" },{ "k", "kit" },
    { "g", "go" },{ "ch", "chip" },{ "j", "jog" },{ "f", "fan" },{ "v", "van" },{ "th", "think" },{ "dh", "that" },
    { "s", "sip" },{ "z", "zoo" },{ "sh", "ship" },{ "zh", "genre" }, 

    // Nasals
    { "m", "man" },
    { "n", "net" },
    { "ng", "ngoma" },      // Swahili word (used to show initial "ng")

    // Approximants
    { "l", "lip" },
    { "r", "red" },
    { "y", "yes" },
    { "w", "wet" },

    // Glottal
    { "h", "hat" },
    {"hh", "house"},

    // Vowels (Monophthongs)
    { "iy", "eat" },{ "ih", "it" },{ "eh", "ed" },{ "ae", "apple" },{ "uh", "up" },{ "ah", "ox" },{ "ax", "ago" },        // unstressed schwa
    { "uw", "ooze" },{ "aa", "odd" },{ "ao", "awe" },{ "er", "earn" },

    // Vowels (Diphthongs)
    { "ey", "ate" },{ "ay", "ice" },{ "oy", "oil" },{ "aw", "out" },{ "ow", "oak" },
    // R-controlled diphthongs
    { "ier", "ear" }, { "air", "air" },{ "oor", "oar" },{ "ar", "arm" }
    };

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (displayField == null) return;

        originalText = displayField.text;

        string phonemeName = gameObject.name.ToLower(); // assumes GameObject is named like "p", "dh", etc.
        if (phonemeExamples.TryGetValue(phonemeName, out string word))
        {
            displayField.text = word;
        }
        else
        {
            displayField.text = $"({phonemeName})";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (displayField != null)
        {
            displayField.text = originalText;
        }
    }
}
