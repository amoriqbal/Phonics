using UnityEngine;

public class DDOL : MonoBehaviour
{
    public int mQuestions = 10;
    public int mScore = 0;
    public static DDOL Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
