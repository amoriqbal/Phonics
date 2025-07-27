using System;
using System.IO;
using TMPro;
using UnityEngine;

public class ScoreSceneBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int score = DDOL.Instance.mScore;
        int questions = DDOL.Instance.mQuestions;
        GetComponent<TextMeshProUGUI>().text = 
            $"Score : {score} / {questions}\n" +
            $"Percentage : {((float)score / questions) * 100}%";
        string recordPath = Application.dataPath + "/practiceRecord.txt";
        StreamWriter sw = File.AppendText(recordPath);
        sw.WriteLine($"{questions},{score},{DateTime.Now.ToString()}\n");
        sw.Close();
    }
}
