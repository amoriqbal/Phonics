using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using System.Linq; // ✅ Add this line for Where(), Select(), etc.

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
                           string rawPhonemes = phonemeTag.Substring(5).Trim(); // remove extra spaces
                            string[] parts = rawPhonemes.Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                            string phonemes = string.Join(" - ", parts);
                            outputTextField.text = phonemes;
                            outputTextField.gameObject.SetActive(true);
                            yield break;
                        }
                    }

           }

                outputTextField.text = "Phonemes not found.";
            }
        }
    }
}
