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
            case TestMode.낮_스타트_모드:
                sceneName = "TeaHouseFront";
                GameManager.Instance.TutorialComplete();
                OrderManager.Instance.SetAfterNodeTitle("낮_주문");
                break;
            case TestMode.필드_스타트_모드:
                sceneName = "FieldPoC";
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
        SceneManager.LoadScene(sceneName);
    }
}
