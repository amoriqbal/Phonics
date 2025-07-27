using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadButtonBehavior : MonoBehaviour
{
    public string practiceScenePath;
    //{
    //    get
    //    {
    //        return practiceScenePath;
    //    }
    //    set
    //    {
    //        if (SceneManager.GetSceneByPath(value).IsValid())
    //        {
    //            practiceScenePath = value;
    //        }
    //    }
    //}
    public string dashboardScenePath;
    //{
    //    get
    //    {
    //        return dashboardScenePath;
    //    }
    //    set
    //    {
    //        if (SceneManager.GetSceneByPath(value).IsValid())
    //        {
    //            dashboardScenePath = value;
    //        }
    //    }
    //}
    public void mOnClickStartButton()
    {
        if(practiceScenePath != null)
            SceneManager.LoadScene(practiceScenePath);
    }

    public void mOnClickDashboardButton()
    {
        if(practiceScenePath != null)
            SceneManager.LoadScene(practiceScenePath);
    }
}
