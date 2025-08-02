using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadButtonBehavior : MonoBehaviour
{
    public string practiceScenePath;
    public string dashboardScenePath;
    [SerializeField]
    private GameObject settingsPanel;
    [SerializeField]
    private TMP_InputField textFieldSettingNumQuestions;
    [SerializeField]
    private TMP_InputField textFieldSettingDelay;
    [SerializeField]
    private TMP_Dropdown dropdownDifficulty;
    public void mOnClickStartButton()
    {
        if(practiceScenePath != null)
            SceneManager.LoadScene(practiceScenePath);
    }
    public void PlaygroundScene()
    {
        SceneManager.LoadScene("Circular Waveform");
    }

    public void LearnScene()
    {
               SceneManager.LoadScene("Mouth_Animation");
    }

    public void SyllablesScene()
    {
        SceneManager.LoadScene("WaveformA");
    }
    public void mOnClickDashboardButton()
    {
        if(practiceScenePath != null)
            SceneManager.LoadScene(dashboardScenePath);
    }

    public void mOnNumQuestionsFieldChanged()
    {
        string xNumQuestionsStr = textFieldSettingNumQuestions.text;
        if (xNumQuestionsStr != string.Empty)
        {
            int lNumQuestions;
            if (int.TryParse(xNumQuestionsStr, out lNumQuestions))
            {
                DDOL.Instance.mSettingsNumQuestions = lNumQuestions;
            }
            else
            {
                textFieldSettingNumQuestions.text = "10";
                DDOL.Instance.mSettingsNumQuestions = 10;
            }
        }
    }

    public void mOnDelayFieldChanged()
    {
        string xDelayStr = textFieldSettingDelay.text;
        if (xDelayStr != string.Empty)
        {
            int lDelay;
            if (int.TryParse(xDelayStr, out lDelay))
            {
                DDOL.Instance.mSettingsDelay = lDelay;
            }
            else
            {
                textFieldSettingDelay.text = "3";
                DDOL.Instance.mSettingsDelay = 3;
            }
        }
    }

    public void mOnDifficultyChanged()
    {
        DDOL.Instance.mSettingsDifficulty = dropdownDifficulty.value;
    }
    public void mShowHideSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    public void mIncrementQuestions()
    {
        textFieldSettingNumQuestions.text = $"{DDOL.Instance.mSettingsNumQuestions + 1}";
    }

    public void mDecrementQuestions()
    {
        if (DDOL.Instance.mSettingsNumQuestions > 1)
        {
            textFieldSettingNumQuestions.text = $"{DDOL.Instance.mSettingsNumQuestions - 1}";
        }
    }
    public void mIncrementDelay()
    {
        textFieldSettingDelay.text = $"{DDOL.Instance.mSettingsDelay + 1}";
    }

    public void mDecrementDelay()
    {
        if (DDOL.Instance.mSettingsDelay > 1)
        {
            textFieldSettingDelay.text = $"{DDOL.Instance.mSettingsDelay - 1}";
        }
    }

    public void Start()
    {
        mOnDifficultyChanged();
        mOnDelayFieldChanged();
        mOnNumQuestionsFieldChanged();
    }
}
