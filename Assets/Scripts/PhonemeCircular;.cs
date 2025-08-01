using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public class PhonemeRecorder : MonoBehaviour
{
    public Button startButton;
    public TMP_Text buttonText;

    private bool isRecording = false;
    private AudioClip recordedClip;

    private const int SampleRate = 16000;
    private const float MaxRecordingDuration = 10f;

    private string assemblyAI_API_KEY = "1e789136031c4e9687a2e8da2dc34906";

    void Start()
    {
        startButton.onClick.AddListener(OnStartStopButtonClick);
        buttonText.text = "Start";
    }

    void OnStartStopButtonClick()
    {
        if (!isRecording)
        {
            StartRecording();
            buttonText.text = "Stop";
        }
        else
        {
            StopRecording();
            buttonText.text = "Start";
        }
    }

    void StartRecording()
    {
        FindAnyObjectByType<PhonemeHighlighter>()?.ResetHighlights();
         PhonemeHighlighter highlighter = FindAnyObjectByType<PhonemeHighlighter>();
highlighter?.ResetHighlights();

        recordedClip = Microphone.Start(null, false, (int)MaxRecordingDuration, SampleRate);
        isRecording = true;
        Debug.Log("Recording started...");
    }

    void StopRecording()
    {
        if (!isRecording) return;

        int samplePos = Microphone.GetPosition(null);
        Microphone.End(null);
        isRecording = false;

        Debug.Log("Recording stopped.");
        float[] samples = new float[samplePos];
        recordedClip.GetData(samples, 0);
        StartCoroutine(ProcessRecording(samples));
    }

    IEnumerator ProcessRecording(float[] samples)
    {
        byte[] wavData = ConvertToWav(samples, recordedClip.channels, recordedClip.frequency);
        yield return StartCoroutine(SendToAssemblyAI(wavData));
    }

    byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * short.MaxValue);
            byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        using (MemoryStream stream = new MemoryStream())
        {
            int fileSize = 44 + bytesData.Length;

            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            stream.Write(System.BitConverter.GetBytes(fileSize - 8), 0, 4);
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            stream.Write(System.BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
            stream.Write(System.BitConverter.GetBytes((short)1), 0, 2); // PCM
            stream.Write(System.BitConverter.GetBytes((short)channels), 0, 2);
            stream.Write(System.BitConverter.GetBytes(sampleRate), 0, 4);
            stream.Write(System.BitConverter.GetBytes(sampleRate * channels * 2), 0, 4);
            stream.Write(System.BitConverter.GetBytes((short)(channels * 2)), 0, 2);
            stream.Write(System.BitConverter.GetBytes((short)16), 0, 2);

            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            stream.Write(System.BitConverter.GetBytes(bytesData.Length), 0, 4);
            stream.Write(bytesData, 0, bytesData.Length);

            return stream.ToArray();
        }
    }

    IEnumerator SendToAssemblyAI(byte[] audioData)
    {
        UnityWebRequest uploadRequest = new UnityWebRequest("https://api.assemblyai.com/v2/upload", "POST");
        uploadRequest.uploadHandler = new UploadHandlerRaw(audioData);
        uploadRequest.downloadHandler = new DownloadHandlerBuffer();
        uploadRequest.SetRequestHeader("authorization", assemblyAI_API_KEY);
        uploadRequest.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return uploadRequest.SendWebRequest();

        if (uploadRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Upload failed: " + uploadRequest.error);
            yield break;
        }

        string uploadJson = uploadRequest.downloadHandler.text;
        string uploadUrl = JObject.Parse(uploadJson)["upload_url"]?.ToString();

        if (!string.IsNullOrEmpty(uploadUrl))
        {
            yield return StartCoroutine(StartTranscription(uploadUrl));
        }
        else
        {
            Debug.LogError("Upload URL not received.");
        }
    }

    IEnumerator StartTranscription(string audioUrl)
    {
        JObject payload = new JObject
        {
            { "audio_url", audioUrl },
            { "language_code", "en_us" },
            { "format_text", false }
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload.ToString());
        UnityWebRequest transcriptRequest = new UnityWebRequest("https://api.assemblyai.com/v2/transcript", "POST");
        transcriptRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        transcriptRequest.downloadHandler = new DownloadHandlerBuffer();
        transcriptRequest.SetRequestHeader("authorization", assemblyAI_API_KEY);
        transcriptRequest.SetRequestHeader("Content-Type", "application/json");

        yield return transcriptRequest.SendWebRequest();

        if (transcriptRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Transcription request failed: " + transcriptRequest.error);
            yield break;
        }

        string responseText = transcriptRequest.downloadHandler.text;
        string transcriptId = JObject.Parse(responseText)["id"]?.ToString();

        if (!string.IsNullOrEmpty(transcriptId))
        {
            yield return StartCoroutine(PollTranscriptionResult(transcriptId));
        }
        else
        {
            Debug.LogError("Transcript ID not found.");
        }
    }

   IEnumerator PollTranscriptionResult(string transcriptId)
{
    string endpoint = $"https://api.assemblyai.com/v2/transcript/{transcriptId}";
    UnityWebRequest pollRequest;

    while (true)
    {
        pollRequest = UnityWebRequest.Get(endpoint);
        pollRequest.SetRequestHeader("authorization", assemblyAI_API_KEY);

        yield return pollRequest.SendWebRequest();

        if (pollRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Polling failed: " + pollRequest.error);
            yield break;
        }

        JObject result = JObject.Parse(pollRequest.downloadHandler.text);
        string status = result["status"]?.ToString();

        if (status == "completed")
        {
            JArray words = (JArray)result["words"];
            if (words != null && words.Count > 0)
            {
                string firstWord = words[0]["text"]?.ToString();
                Debug.Log("Word: " + firstWord);

                GameObject manager = GameObject.Find("PhonemeManager");
                var lookup = manager?.GetComponent<OfflinePhonemeLookup>();
                lookup?.LookupAndPrintFirstPhoneme(firstWord);
            }
            else
            {
                Debug.LogWarning("No words found in transcript.");
            }

            break;
        }
        else if (status == "error")
        {
            Debug.LogError("Transcription failed.");
            yield break;
        }

        yield return new WaitForSeconds(2f);
    }
}

}
