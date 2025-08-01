using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using XCharts.Runtime;
using System.IO;
using System;

public class WaveformCompareBehavior : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textPhonetics;
    [SerializeField]
    private LineChart lineChart;
    public string waveFilePath = Application.persistentDataPath + "/WaveformArecorded.wav";
    public string[] phonemes;
    AudioClip audioClip;
    public int currentIndex = 0;

    public void SetPhonemes(string[] phonemes)
    {
        this.phonemes = phonemes;
        currentIndex = 0;
        UpdateText();
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
        }
        else
        {
            textPhonetics.text = string.Empty;
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
    }

    public void StopRecordAudio()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
        AttemptPhoneme();
    }

    public void DrawWaveformWithXCharts(string wavFilePath, LineChart lineChart, int resolution = 512)
    {
        // Load the wav file as an AudioClip
        var url = "file://" + wavFilePath;
        StartCoroutine(LoadAndDrawWaveform(url, lineChart, resolution));
    }

    private IEnumerator LoadAndDrawWaveform(string url, LineChart lineChart, int resolution)
    {
        using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load wav file: " + www.error);
                yield break;
            }
            var clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);

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
}
