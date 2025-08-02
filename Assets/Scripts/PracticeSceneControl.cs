using System.IO;
using UnityEngine;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using System.Data.Common;
using SQLite;
using System.Linq;

public class PracticeSceneControl : MonoBehaviour
{
    [SerializeField]
    public TextAsset[] mDatasetPaths;
    [SerializeField]
    public int mMaxQuestions = 10;
    [SerializeField]
    public int mMaxAudioLength = 20; // in seconds
    [SerializeField]
    public int mDelayBetweenQuestions = 2; // in seconds
    [SerializeField]
    public TextMeshProUGUI exerciseText;
    [SerializeField]
    public TextMeshProUGUI resultText;
    [SerializeField]
    public TextMeshProUGUI scoreText;
    [SerializeField]
    public TextMeshProUGUI questionNumberText;
    private int mDifficulty;
    private string[] mWordBank;
    private SQLiteConnection mConnection;
    private AudioClip mAudioClip;
    private bool recording = false;
    private byte[] bytes;
    private int _questionNumber;
    private int QuestionNumber { 
        get => _questionNumber;
        set
        { 
            _questionNumber = value;
            questionNumberText.text = $"Question {value}";
        } 
    }
    private int _score;
    private int Score
    {
        get => _score;
        set
        {
            _score = value;
            scoreText.text = $"Score: {value}";
        }
    }
    public void mOnVoiceInputButtonHold()
    {
        mAudioClip = Microphone.Start(null, false, mMaxAudioLength, 44100);
        recording = true;
        resultText.text = $"Recording started";
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (MemoryStream memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    private async void SendRecording()
    {
        string text = await SpeechToText.decodeFile(Application.dataPath + "/test.wav");
        text = text.ToLower().Trim();
        if (text == string.Empty)
        {
            Debug.Log("ERROR: Speech to Text failed");
        }
        else
        {
            resultText.text = text;
            await Task.Delay(mDelayBetweenQuestions * 1000);
            if (resultText.text == exerciseText.text.ToLower().Trim() || resultText.text == exerciseText.text.ToLower().Trim()+".")
            {
                GetComponent<Animator>().SetTrigger("right");
                Score++;
            }
            else
            {
                GetComponent<Animator>().SetTrigger("wrong");
            }
        }
        if (QuestionNumber < mMaxQuestions)
        {
            NextQuestion();
        }
        else
        {  
            DDOL.Instance.mQuestions = QuestionNumber;
            DDOL.Instance.mScore = Score;
            SceneManager.LoadScene("Assets/Scenes/ScoreScene.unity");
            mConnection.CreateTable<Record>();
            mConnection.Insert(new Record
            {
                Score = Score,
                DateTime = DateTime.Now,
                NumQuestions = QuestionNumber,
                Difficulty = DDOL.Instance.mSettingsDifficulty
            });
        }
    }
    public void mOnVoiceInputButtonRelease()
    {
        int position = Microphone.GetPosition(null);
        Microphone.End(null);
        resultText.text = $"Recording stopped. Processing audio";
        recording = false;
        var samples = new float[position * mAudioClip.channels];
        mAudioClip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, mAudioClip.frequency, mAudioClip.channels);
        recording = false;
        File.WriteAllBytes(Application.dataPath + "/test.wav", bytes);
        
        SendRecording();
    }
    private void Update()
    {
        if (recording && 
            Microphone.GetPosition(null) >= mAudioClip.samples)
        {
            mOnVoiceInputButtonRelease();
        }
    }

    private void Start()
    {
        Score = 0;
        QuestionNumber = 0;
        mMaxQuestions = DDOL.Instance.mSettingsNumQuestions;
        mDelayBetweenQuestions = DDOL.Instance.mSettingsDelay;
        mDifficulty = DDOL.Instance.mSettingsDifficulty;
        mPrepareSampleDataset();
        mConnection = new SQLiteConnection($"{Application.persistentDataPath}/PracticeRecords.db");
        NextQuestion();
    }

    private void mPrepareSampleDataset()
    {
        TextAsset sampleFile = mDatasetPaths[mDifficulty];
        if (sampleFile)
        {
            string[] lines = sampleFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();
            mWordBank = lines.OrderBy(x => UnityEngine.Random.value).Take(mMaxQuestions).ToArray();
        }
        else
        {
            Debug.LogError($"Sample dataset file not found: {sampleFile.name}");
            mWordBank = new string[0];
        }
    }
    private void OnDestroy()
    {
        mConnection.Close();
    }

    private void NextQuestion()
    {
        QuestionNumber++;
        questionNumberText.text = $"Question {QuestionNumber} / {mMaxQuestions}";
        resultText.text = "";
        if (mWordBank.Length > 0 && exerciseText != null)
        {
            exerciseText.text = mWordBank[QuestionNumber - 1];
        }
    }

    public class Record
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NumQuestions { get; set; }
        public int Score { get; set; }
        public int Difficulty { get; set; }
        public DateTime DateTime { get; set; }
        public override string ToString() 
        {
            return $"Record: {Id}, Score: {Score}, Questions: {NumQuestions}, Difficulty: {Difficulty}, DateTime: {DateTime}";
        }
    }
    public class SpeechToText
    {
        static readonly string BaseUrl = "https://api.assemblyai.com";
        static readonly string ApiKey = "0dbf57727ac345c2a4df5f2d594337f8";


        static async Task<string> UploadFileAsync(string filePath, HttpClient httpClient)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            using (StreamContent fileContent = new StreamContent(fileStream))
            {
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                using (HttpResponseMessage response = await httpClient.PostAsync("https://api.assemblyai.com/v2/upload", fileContent))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject jsonObj = JsonConvert.DeserializeObject<JObject>(responseBody);
                    if (jsonObj?["upload_url"] == null)
                    {
                        Debug.Log("ERROR: Failed to get upload URL from response");
                        return string.Empty;
                    }
                    return jsonObj["upload_url"].ToString();
                }
            }
        }

        public static async Task<string> decodeFile(string filePath)
        {
            using var httpClient = new HttpClient();
            string result = string.Empty;
            httpClient.DefaultRequestHeaders.Add("authorization", ApiKey);

            var audioUrl = await UploadFileAsync(filePath, httpClient);

            var requestData = new
            {
                audio_url = audioUrl,
                speech_model = "universal"
            };

            string json = JsonConvert.SerializeObject(requestData);

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json");

            using var transcriptResponse = await httpClient.PostAsync($"{BaseUrl}/v2/transcript", jsonContent);
            var transcriptResponseBody = await transcriptResponse.Content.ReadAsStringAsync();
            var transcriptData = JsonConvert.DeserializeObject<JObject>(transcriptResponseBody);

            string status = transcriptData["status"]?.ToString();

            if (!transcriptData.TryGetValue("id", out JToken idElement))
            {
                Debug.Log("ERROR: Failed to get transcript ID");
                return string.Empty;
            }

            string transcriptId = idElement?.ToString();
            if (transcriptId == null)
            {
                Debug.Log("ERROR: Transcript ID is null");
                return string.Empty;
            }

            string pollingEndpoint = $"{BaseUrl}/v2/transcript/{transcriptId}";

            while (true)
            {
                using var pollingResponse = await httpClient.GetAsync(pollingEndpoint);
                string pollingResponseBody = await pollingResponse.Content.ReadAsStringAsync();
                var transcriptionResult = JsonConvert.DeserializeObject<JObject>(pollingResponseBody);

                if (!transcriptionResult.TryGetValue("status", out JToken statusElement))
                {
                    Debug.Log("ERROR: Failed to get transcription status");
                    return string.Empty;
                }

                status = statusElement?.ToString();
                if (status == null)
                {
                    Debug.Log("ERROR: Status is null");
                    return string.Empty;
                }

                if (status == "completed")
                {
                    if (!transcriptionResult.TryGetValue("text", out JToken textElement))
                    {
                        Debug.Log("ERROR: Failed to get transcript text");
                        return string.Empty;
                    }

                    result = textElement.ToString() ?? string.Empty;
                    break;
                }
                else if (status == "error")
                {
                    string errorMessage = transcriptionResult.TryGetValue("error", out JToken errorElement)
                        ? errorElement.ToString() ?? "Unknown error"
                        : "Unknown error";

                    Debug.Log($"ERROR: Transcription failed: {errorMessage}");
                    return string.Empty;
                }
                else
                {
                    await Task.Delay(3000);
                }
            }
            return result;
        }
    }
}
