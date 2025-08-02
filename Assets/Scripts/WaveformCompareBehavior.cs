using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XCharts.Runtime;
using System.IO;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json; 
public class WaveformCompareBehavior : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textPhonetics;
    [SerializeField]
    private LineChart lineChart;
    [SerializeField]
    private LineChart targetChart;
    [SerializeField]
    private TMP_InputField inputField;
    public string waveFilePath;
    public string targetWaveFilePath; 
    public string[] phonemes;
    public string[] phonemeFiles;
    AudioClip audioClip;
    public int currentIndex = 0;

    public void Awake()
    {
        waveFilePath = Application.streamingAssetsPath + "/WaveformARecording.wav";
        targetWaveFilePath = Application.streamingAssetsPath + "/concat.wav";
    }

    public async void OnAnalyzeButtonPressed()
    {
        string [] lArpabetPhonemes = await GetPhonemesFromDatamuseAsync(inputField.text);
        Debug.Log("Input = " + inputField.text);
        if (lArpabetPhonemes != null)
        {
            Debug.Log("Arpabet set: " + string.Join(", ", lArpabetPhonemes));
            string [] lMappedPhonemes = MapArpabetTo44Phonemes(lArpabetPhonemes);
            Debug.Log("Mapped Phonemes: " + string.Join(", ", lMappedPhonemes));
            phonemeFiles = MapArpabetToFileNames(lArpabetPhonemes, "Assets/StreamingAssets/Phonemes/", ".wav");
            ConcatenateWavFiles(phonemeFiles, targetWaveFilePath);
            DrawWaveformWithXCharts(targetWaveFilePath, targetChart);
            SetPhonemes(lMappedPhonemes);
        }
    }
    public void SetPhonemes(string[] phonemes)
    {
        this.phonemes = phonemes;
        currentIndex = 0;
        UpdateText();
        //UpdateTargetChart();
    }

    void UpdateText()
    {
        if (phonemes != null && phonemes.Length > 0)
        {
            string lTempText = string.Empty;
            for (int i = 0; i < phonemes.Length; i++)
            {
                if (i == currentIndex)
                {
                    lTempText += $"<color=yellow>[{phonemes[i]}]</color>";
                }
                else
                {
                    lTempText += phonemes[i];
                }
                if (i < phonemes.Length - 1)
                {
                    lTempText += " - ";
                }
            }
            textPhonetics.text = lTempText;
        }
        else
        {
            textPhonetics.text = string.Empty;
        }
    }

    void UpdateTargetChart()
    {
        if (targetChart != null)
        {
            targetChart.ClearData();
            if (phonemes != null && phonemes.Length > 0)
            {
                DrawWaveformWithXCharts(phonemeFiles[currentIndex], targetChart);
            }
            targetChart.RefreshChart();
        }
    }
    void AttemptPhoneme()
    {
        WriteAudioClipToWav(audioClip, waveFilePath);
        DrawWaveformWithXCharts(waveFilePath, lineChart);
        
    }

    public void StartRecordAudio()
    {
        audioClip = Microphone.Start(null, false, 10, 44100);
        if (audioClip == null)
        {
            Debug.LogError("Failed to start recording audio.");
            return;
        }
        Debug.Log("Recording started.");
    }

    public void StopRecordAudio()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
        AttemptPhoneme();
        Debug.Log("Recording stopped.");
    }

    public void DrawWaveformWithXCharts(string wavFilePath, LineChart lineChart, int resolution = 512)
    {
        // Load the wav file as an AudioClip
        string phonemeFile = Path.Combine(Application.streamingAssetsPath, "Phonemes/iaay.wav");
        string url = "file://" + phonemeFile;
        StartCoroutine(LoadAndDrawWaveform(url, lineChart, resolution));
    }

    private IEnumerator LoadAndDrawWaveform(string url, LineChart lineChart, int resolution)
    {
        using (var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load wav file: " +url+ ":" + www.error);
                yield break;
            }
            var clip = DownloadHandlerAudioClip.GetContent(www);

            // Get audio samples
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            // Downsample for chart
            int step = Mathf.Max(1, samples.Length / resolution);
            var data = new List<float>();
            for (int i = 0; i < resolution; i++)
            {
                int sampleIndex = i * step;
                data.Add(samples[sampleIndex]);
            }

            // Prepare XCharts series
            lineChart.ClearData();
            // Replace 'LineSerie' with 'Serie' in the following line:
            // var serie = lineChart.AddSerie<LineSerie>("Waveform");
            var serie = lineChart.AddSerie<Serie>("Waveform");
            for (int i = 0; i < data.Count; i++)
            {
                lineChart.AddData(0, data[i], i.ToString());
            }
            lineChart.RefreshChart();
        }
    }

    public static void WriteAudioClipToWav(AudioClip clip, string filePath)
    {
        if (clip == null)
            throw new ArgumentNullException(nameof(clip));

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            int sampleCount = samples.Length;
            int channels = clip.channels;
            int sampleRate = clip.frequency;
            int byteRate = sampleRate * channels * 2; // 16 bit

            // Write WAV header
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(36 + sampleCount * 2); // File size minus 8 bytes
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size for PCM
            writer.Write((short)1); // AudioFormat PCM
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2)); // BlockAlign
            writer.Write((short)16); // BitsPerSample

            // Data subchunk
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(sampleCount * 2);

            // Write samples as 16-bit PCM
            for (int i = 0; i < sampleCount; i++)
            {
                short val = (short)Mathf.Clamp(samples[i] * 32767f, short.MinValue, short.MaxValue);
                writer.Write(val);
            }
        }
    }

    public async Task<string[]> GetPhonemesFromDatamuseAsync(string word)
    {
        string url = $"https://api.datamuse.com/words?sp={word}&md=r";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Datamuse API error: " + www.error);
                return null;
            }

            var json = www.downloadHandler.text;
            var words = JsonConvert.DeserializeObject<List<DatamuseWord>>(json);
            if (words != null && words.Count > 0 && words[0].tags != null)
            {
                foreach (var tag in words[0].tags)
                {
                    if (tag.StartsWith("pron:"))
                    {
                        var pron = tag.Substring(5);
                        Debug.Log("Pronunciation found: " + pron);
                        return pron.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    }
                }
            }
        }
        return null;
    }

    public static readonly Dictionary<string, string> ArpabetToAudioFileMap = new Dictionary<string, string>
    {
        // Vowels
        { "AA", "o" },      // as in 'hot'
        { "AE", "a" },      // as in 'cat'
        { "AH", "u" },      // as in 'cup'
        { "AO", "aw" },     // as in 'dog'
        { "AW", "ow" },     // as in 'cow'
        { "AY", "iaay" },      // as in 'my'
        { "EH", "e" },      // as in 'bed'
        { "ER", "ur" },     // as in 'her'
        { "EY", "ai" },      // as in 'cake'
        { "IH", "i" },      // as in 'sit'
        { "IY", "ee" },     // as in 'see'
        { "OW", "oa" },     // as in 'go'
        { "OY", "oi" },     // as in 'boy'
        { "UH", "oo" },     // as in 'book'
        { "UW", "ooue" },     // as in 'blue'

        // Consonants
        { "B", "b" },       // as in 'bat'
        { "CH", "ch" },     // as in 'chin'
        { "D", "d" },       // as in 'dog'
        { "DH", "the" },     // as in 'this' (voiced)
        { "F", "f" },       // as in 'fish'
        { "G", "g" },       // as in 'go'
        { "HH", "h" },      // as in 'hat'
        { "JH", "j" },      // as in 'jump'
        { "K", "k" },       // as in 'kite'
        { "L", "l" },       // as in 'leg'
        { "M", "m" },       // as in 'man'
        { "N", "n" },       // as in 'nose'
        { "NG", "ng" },     // as in 'sing'
        { "P", "p" },       // as in 'pig'
        { "R", "r" },       // as in 'run'
        { "S", "s" },       // as in 'sun'
        { "SH", "sh" },     // as in 'she'
        { "T", "t" },       // as in 'top'
        { "TH", "th" },     // as in 'thin' (unvoiced)
        { "V", "v" },       // as in 'van'
        { "W", "w" },       // as in 'wet'
        { "Y", "y" },       // as in 'yes'
        { "Z", "z" },       // as in 'zip'
        { "ZH", "zh" }      // as in 'measure'
    };

    public static readonly Dictionary<string, string> ArpabetTo44PhonemeMap = new Dictionary<string, string>
{
    // Vowels
    { "AA", "o" },      // as in 'hot'
    { "AE", "a" },      // as in 'cat'
    { "AH", "u" },      // as in 'cup'
    { "AO", "aw" },     // as in 'dog'
    { "AW", "ow" },     // as in 'cow'
    { "AY", "i" },      // as in 'my'
    { "EH", "e" },      // as in 'bed'
    { "ER", "ur" },     // as in 'her'
    { "EY", "a" },      // as in 'cake'
    { "IH", "i" },      // as in 'sit'
    { "IY", "ee" },     // as in 'see'
    { "OW", "oa" },     // as in 'go'
    { "OY", "oi" },     // as in 'boy'
    { "UH", "oo" },     // as in 'book'
    { "UW", "oo" },     // as in 'blue'

    // Consonants
    { "B", "b" },       // as in 'bat'
    { "CH", "ch" },     // as in 'chin'
    { "D", "d" },       // as in 'dog'
    { "DH", "th" },     // as in 'this' (voiced)
    { "F", "f" },       // as in 'fish'
    { "G", "g" },       // as in 'go'
    { "HH", "h" },      // as in 'hat'
    { "JH", "j" },      // as in 'jump'
    { "K", "k" },       // as in 'kite'
    { "L", "l" },       // as in 'leg'
    { "M", "m" },       // as in 'man'
    { "N", "n" },       // as in 'nose'
    { "NG", "ng" },     // as in 'sing'
    { "P", "p" },       // as in 'pig'
    { "R", "r" },       // as in 'run'
    { "S", "s" },       // as in 'sun'
    { "SH", "sh" },     // as in 'she'
    { "T", "t" },       // as in 'top'
    { "TH", "th" },     // as in 'thin' (unvoiced)
    { "V", "v" },       // as in 'van'
    { "W", "w" },       // as in 'wet'
    { "Y", "y" },       // as in 'yes'
    { "Z", "z" },       // as in 'zip'
    { "ZH", "zh" }      // as in 'measure'
};


    public static string[] MapArpabetTo44Phonemes(string[] arpabetPhonemes)
    {
        var result = new List<string>();
        foreach (var phoneme in arpabetPhonemes)
        {
            // Remove stress digits from vowels (e.g., AH0 -> AH)
            var basePhoneme = phoneme.TrimEnd('0', '1', '2');
            if (ArpabetTo44PhonemeMap.TryGetValue(basePhoneme, out var mapped))
            {
                result.Add(mapped);
            }
            else
            {
                result.Add(basePhoneme); // fallback: use ARPABET symbol
            }
        }
        return result.ToArray();
    }

    public static string[] MapArpabetToFileNames(string[] arpabetPhonemes, string audioDirectory = "Assets/PhonemeAudio/", string extension = ".wav")
    {
        var result = new List<string>();
        foreach (var phoneme in arpabetPhonemes)
        {
            // Remove stress digits from vowels (e.g., AH0 -> AH)
            var basePhoneme = phoneme.TrimEnd('0', '1', '2');
            if (ArpabetToAudioFileMap.TryGetValue(basePhoneme, out var fileBase))
            {
                result.Add(Path.Combine(audioDirectory, fileBase + extension));
            }
            else
            {
                // Fallback: use ARPABET symbol as filename
                result.Add(Path.Combine(audioDirectory, basePhoneme + extension));
            }
        }
        return result.ToArray();
    }

    public static void ConcatenateWavFiles(string[] inputFilePaths, string outputFilePath)
    {
        if (inputFilePaths == null || inputFilePaths.Length == 0)
            throw new ArgumentException("No input files provided.");

        List<byte[]> pcmDataList = new List<byte[]>();
        int sampleRate = 0, channels = 0, bitsPerSample = 0;

        foreach (var filePath in inputFilePaths)
        {
            using (var reader = new BinaryReader(File.OpenRead(filePath)))
            {
                // Read WAV header
                reader.BaseStream.Seek(22, SeekOrigin.Begin); // channels
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.BaseStream.Seek(34, SeekOrigin.Begin); // bits per sample
                bitsPerSample = reader.ReadInt16();

                // Find "data" chunk
                reader.BaseStream.Seek(12, SeekOrigin.Begin);
                while (reader.ReadUInt32() != 0x61746164) // "data"
                {
                    int chunkSize = reader.ReadInt32();
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                }
                int dataSize = reader.ReadInt32();
                byte[] pcmData = reader.ReadBytes(dataSize);
                pcmDataList.Add(pcmData);
            }
        }

        // Concatenate PCM data
        int totalDataSize = pcmDataList.Sum(d => d.Length);

        using (var writer = new BinaryWriter(File.Create(outputFilePath)))
        {
            // Write WAV header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + totalDataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // PCM
            writer.Write((short)1); // PCM format
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bitsPerSample / 8);
            writer.Write((short)(channels * bitsPerSample / 8));
            writer.Write((short)bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(totalDataSize);

            // Write concatenated PCM data
            foreach (var pcm in pcmDataList)
                writer.Write(pcm);
        }
    }
}

public class DatamuseWord
{
    public string word;
    public List<string> tags;
}
