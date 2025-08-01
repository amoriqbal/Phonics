using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Linq;

public class PhonemeZoneHighlighter : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField wordInputField;
    public TMP_Text outputTextField;
    public Button playButton;

    [Header("Zone Images")]
    public Image lipsImage;
    public Image upperTeethImage;
    public Image lowerTeethImage;
    public Image nasalImage;
    public Image vocalChordsImage;
    public Image tongueImage;

    private Dictionary<string, Dictionary<string, float>> phonemeZones = new Dictionary<string, Dictionary<string, float>>();
    private Dictionary<string, Image> zoneImages;

    private void Start()
    {
        InitPhonemeZones();

        zoneImages = new Dictionary<string, Image>()
        {
            { "lips", lipsImage },
            { "upper_teeth", upperTeethImage },
            { "lower_teeth", lowerTeethImage },
            { "nasal", nasalImage },
            { "vocal_chords", vocalChordsImage },
            { "tongue", tongueImage }
        };

        playButton.onClick.AddListener(() => StartCoroutine(ProcessWord(wordInputField.text.Trim().ToLower())));
        outputTextField.text = "";
        outputTextField.gameObject.SetActive(true);
    }

    IEnumerator ProcessWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            outputTextField.text = "Enter a word.";
            yield break;
        }

        string url = $"https://api.datamuse.com/words?sp={word}&md=r&max=1";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            outputTextField.text = "Error fetching phonemes.";
            yield break;
        }

        string json = request.downloadHandler.text;
        string phonemesRaw = ExtractPhonemesFromJSON(json);

        if (string.IsNullOrEmpty(phonemesRaw))
        {
            outputTextField.text = "Phoneme data not found.";
            yield break;
        }

        string[] phonemes = phonemesRaw.Split(' ')
                                       .Where(p => !string.IsNullOrWhiteSpace(p))
                                       .Select(p => new string(p.Where(c => !char.IsDigit(c)).ToArray()))
                                       .ToArray();

        outputTextField.text = string.Join(" - ", phonemes);

        StopAllCoroutines();
        StartCoroutine(AnimatePhonemes(phonemes));
    }

    string ExtractPhonemesFromJSON(string json)
    {
        int idx = json.IndexOf("\"tags\"");
        if (idx == -1) return null;

        int start = json.IndexOf("[", idx);
        int end = json.IndexOf("]", start);
        if (start == -1 || end == -1) return null;

        string tagsSection = json.Substring(start + 1, end - start - 1);
        string[] tags = tagsSection.Replace("\"", "").Split(',');

        foreach (string tag in tags)
        {
            if (tag.Trim().StartsWith("pron:"))
            {
                return tag.Trim().Substring(5);
            }
        }

        return null;
    }

void InitPhonemeZones()

{

    // Plosives

    phonemeZones["P"] = new() { { "lips", 1f } };

    phonemeZones["B"] = new() { { "lips", 1f }, { "vocal_chords", 0.9f } };

    phonemeZones["T"] = new() { { "tongue", 0.8f }, { "upper_teeth", 0.7f } };

    phonemeZones["D"] = new() { { "tongue", 0.8f }, { "vocal_chords", 0.7f }, { "upper_teeth", 0.6f } };

    phonemeZones["K"] = new() { { "tongue", 0.9f } };

    phonemeZones["G"] = new() { { "tongue", 0.9f }, { "vocal_chords", 0.7f } };



    // Nasals

    phonemeZones["M"] = new() { { "lips", 1f }, { "nasal", 1f }, { "vocal_chords", 1f } };

    phonemeZones["N"] = new() { { "tongue", 0.6f }, { "nasal", 1f }, { "vocal_chords", 1f } };

    phonemeZones["NG"] = new() { { "tongue", 0.9f }, { "nasal", 1f }, { "vocal_chords", 1f } };



    // Fricatives

    phonemeZones["F"] = new() { { "lips", 0.8f }, { "upper_teeth", 1f } };

    phonemeZones["V"] = new() { { "lips", 0.8f }, { "upper_teeth", 1f }, { "vocal_chords", 1f } };

    phonemeZones["TH"] = new() { { "tongue", 1f }, { "upper_teeth", 1f }, { "lower_teeth", 1f } };

    phonemeZones["DH"] = new() { { "tongue", 1f }, { "upper_teeth", 0.8f }, { "lower_teeth", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["S"] = new() { { "tongue", 0.8f }, { "upper_teeth", 1f } };

    phonemeZones["Z"] = new() { { "tongue", 0.8f }, { "upper_teeth", 1f }, { "vocal_chords", 1f } };

    phonemeZones["SH"] = new() { { "tongue", 0.9f }, { "upper_teeth", 0.8f } };

    phonemeZones["ZH"] = new() { { "tongue", 0.9f }, { "upper_teeth", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["HH"] = new() { { "vocal_chords", 0.4f } }; // breathy glottal fricative



    // Affricates

    phonemeZones["CH"] = new() { { "tongue", 1f }, { "upper_teeth", 0.8f } };

    phonemeZones["JH"] = new() { { "tongue", 1f }, { "upper_teeth", 0.8f }, { "vocal_chords", 1f } };



    // Approximants and liquids

    phonemeZones["L"] = new() { { "tongue", 0.9f }, { "vocal_chords", 1f } };

    phonemeZones["R"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["W"] = new() { { "lips", 0.8f }, { "tongue", 0.6f }, { "vocal_chords", 1f } };

    phonemeZones["Y"] = new() { { "tongue", 0.9f }, { "vocal_chords", 1f } };



    // Vowels


    phonemeZones["IY"] = new() { { "tongue", 0.9f }, { "vocal_chords", 1f } };

    phonemeZones["IH"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["EY"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["EH"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f } };

    phonemeZones["AE"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["AA"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f } };

    phonemeZones["AH"] = new() { { "tongue", 0.6f }, { "vocal_chords", 1f } };

    phonemeZones["AO"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f }, { "lips", 0.5f } };

    phonemeZones["OW"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f }, { "lips", 0.4f } };

    phonemeZones["UH"] = new() { { "tongue", 0.8f }, { "vocal_chords", 0.7f } };

    phonemeZones["UW"] = new() { { "tongue", 0.9f }, { "vocal_chords", 1f }, { "lips", 0.6f } };

    phonemeZones["AY"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["AW"] = new() { { "tongue", 0.8f }, { "vocal_chords", 1f } };

    phonemeZones["OY"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f }, { "lips", 0.3f } };



    // Special cases / reduced vowels

    phonemeZones["AX"] = new() { { "tongue", 0.6f }, { "vocal_chords", 0.8f } };

    phonemeZones["AXR"] = new() { { "tongue", 0.6f }, { "vocal_chords", 0.8f }, { "r", 0.5f } };

    phonemeZones["IX"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f } };

    phonemeZones["UX"] = new() { { "tongue", 0.7f }, { "vocal_chords", 0.9f } };

    phonemeZones["ER"] = new() { { "tongue", 0.7f }, { "vocal_chords", 1f } };



}


    IEnumerator AnimatePhonemes(string[] phonemes)
    {
        foreach (string phoneme in phonemes)
        {
            if (phonemeZones.ContainsKey(phoneme))
            {
                yield return AnimateZone(phoneme, 1.4f);
            }
        }
    }

IEnumerator AnimateZone(string phoneme, float duration)
{
    // Reset all zones to fully transparent immediately
    foreach (var zone in zoneImages.Values)
        SetZoneColor(zone, new Color(1f, 0f, 0f, 0f));

    Dictionary<string, float> zones = phonemeZones[phoneme];
    float halfDuration = duration / 2f;
    float elapsed = 0f;

    // Fade In with easing
    while (elapsed < halfDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / halfDuration);
        t = t * t * (3f - 2f * t);  // Smoothstep easing

        foreach (var zone in zoneImages)
        {
            float intensity = zones.ContainsKey(zone.Key) ? zones[zone.Key] : 0f;
            Color targetColor = new Color(1f, 0f, 0f, intensity);
            Color currentColor = Color.Lerp(new Color(1f, 0f, 0f, 0f), targetColor, t);
            SetZoneColor(zone.Value, currentColor);
        }

        yield return null;
    }

    elapsed = 0f;

    // Fade Out with easing
    while (elapsed < halfDuration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / halfDuration);
        t = t * t * (3f - 2f * t);  // Smoothstep easing

        foreach (var zone in zoneImages)
        {
            float intensity = zones.ContainsKey(zone.Key) ? zones[zone.Key] : 0f;
            Color startColor = new Color(1f, 0f, 0f, intensity);
            Color currentColor = Color.Lerp(startColor, new Color(1f, 0f, 0f, 0f), t);
            SetZoneColor(zone.Value, currentColor);
        }

        yield return null;
    }
}


void SetZoneColor(Image img, Color color)
{
    img.color = color;
}

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
