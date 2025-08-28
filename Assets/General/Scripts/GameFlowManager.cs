using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : SceneSingleton<GameFlowManager>
{
    public void StartGame()
    {
        string sceneName = "";
        switch (GameManager.Instance.testMode)
        {
            case TestMode.무한_주방_모드:
                sceneName = "Kitchen";
                GameManager.Instance.TutorialComplete();
                break;
            case TestMode.일차0_밤_스타트_모드:
                sceneName = "TeaHouseFront";
                OrderManager.Instance.SetAfterNodeTitle("일차0_밤");
                break;
        }

        CoroutineUtil.Instance.RunAfterFirstFrame(() => 
        {
            LoadScene(sceneName);
        });
    }

    /// <summary>
    /// 주방에서 차 만들고 나올 때 실행
    /// </summary>
    public void FinishKitchen()
    {
        if (GameManager.Instance.testMode == TestMode.무한_주방_모드)
        {
            LoadScene("Kitchen");
            return;
        }
        LoadScene("TeaHouseFront");
    }

    /// <summary>
    /// 찻집에 들어왔을 때 실행
    /// </summary>
    public void StartTeaHouseFront()
    {
        if(OrderManager.Instance.GetAfterNodeTitle() != "")
        {
            OrderManager.Instance.Evaluate();
            CoroutineUtil.Instance.RunAfterFirstFrame(() => 
            {
                TeaHouseYarnManager.Instance.RunDialogue(OrderManager.Instance.GetAfterNodeTitle());
                OrderManager.Instance.SetAfterNodeTitle("");
            });
        }
    }

    public void LoadScene(string sceneName)
    {
        UIManager.Instance.ClearAllBlockingUI();
        SceneManager.LoadScene(sceneName);
    }
}
