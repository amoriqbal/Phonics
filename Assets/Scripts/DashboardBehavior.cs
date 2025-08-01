using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class DashboardBehavior : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI mText;
    private SQLiteConnection mConnection;

    
    void Start()
    {
        mConnection = new SQLiteConnection($"{Application.persistentDataPath}/PracticeRecords.db");
        mText.text = "Practice Report:\n";
        ReadReport();
    }

    private void OnDestroy()
    {
        mConnection.Close();
    }
    async void ReadReport()
    {
        StreamReader file = File.OpenText(Application.dataPath + "/practiceRecord.txt");
        var records = await Task<List<PracticeSceneControl.Record>>.Run(() => mConnection.Table<PracticeSceneControl.Record>().ToList());
        foreach (var rec in records)
        {
            mText.text += rec.ToString() + "\n";
        }
    }

    public void LoadMainMenu()
    {
         UnityEngine.SceneManagement.SceneManager.LoadScene("LandingScene");
    }
}
