using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using System.Linq;

public class SpeechProcessor : MonoBehaviour
{
    public TMP_InputField wordInputField;
    public TextMeshProUGUI outputTextField;
    public Button playButton;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
    }

    void OnPlayClicked()
    {
        string inputWord = wordInputField.text.Trim().ToLower();

        if (string.IsNullOrEmpty(inputWord))
        {
            Debug.LogWarning("Empty input.");
            return;
        }

        StartCoroutine(GetPhonemesOnline(inputWord));
    }

    IEnumerator GetPhonemesOnline(string word)
    {
        string url = $"https://api.datamuse.com/words?sp={word}&md=r";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching phonemes: " + www.error);
                outputTextField.text = $"Error: {www.error}";
            }
            else
            {
                string json = www.downloadHandler.text;

                var data = JSON.Parse(json);
                if (data.Count > 0 && data[0]["tags"] != null)
                {
                    foreach (var tag in data[0]["tags"].AsArray)
                    {
                        string phonemeTag = tag.Value;
                        if (phonemeTag.StartsWith("pron:"))
                        {
                            string rawPhonemes = phonemeTag.Substring(5).Trim();
                            string[] parts = rawPhonemes.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                            string phonemeString = string.Join(" - ", parts);
                            outputTextField.gameObject.SetActive(true);

                            StartCoroutine(AnimatePhonemes(parts, 1.4f)); // Highlight each phoneme for 1.4 sec
                            yield break;
                        }
                    }
                }

                outputTextField.text = "Phonemes not found.";
            }
        }
    }

    IEnumerator AnimatePhonemes(string[] phonemes, float interval)
    {
        for (int i = 0; i < phonemes.Length; i++)
        {
            string display = "";

            for (int j = 0; j < phonemes.Length; j++)
            {
                if (i == j)
                    display += $"<color=red>{phonemes[j]}</color>";
                else
                    display += $"<color=grey>{phonemes[j]}</color>";

                if (j < phonemes.Length - 1)
                    display += " - ";
            }

            outputTextField.text = display;
            yield return new WaitForSeconds(interval);
        }

        // Optional: show final string in white
        outputTextField.text = string.Join(" - ", phonemes);
    }
}

