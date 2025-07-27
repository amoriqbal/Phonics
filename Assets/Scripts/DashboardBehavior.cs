using System.IO;
using TMPro;
using UnityEngine;

public class DashboardBehavior : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI mText;

    void Start()
    {
        mText.text = "Practice Report:\n";
        ReadReport();
    }

    async void ReadReport()
    {
        StreamReader file = File.OpenText(Application.dataPath + "/practiceRecord.txt");
        for (string line = await file.ReadLineAsync(); line != null; line = await file.ReadLineAsync())
        {
            mText.text += line + "\n";
        }
    }
}
