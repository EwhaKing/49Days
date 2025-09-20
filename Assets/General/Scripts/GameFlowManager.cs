using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager
{
    public static readonly string KITCHEN_SCENE_NAME = "Kitchen";
    public static readonly string TEA_HOUSE_FRONT_SCENE_NAME = "TeaHouseFront";
    public static readonly string FIELD_SCENE_NAME = "FieldPoC";

    static string currentSceneName;
    public static string GetCurruntSceneName => currentSceneName;
    
    public static void StartGame()
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
    public static void FinishKitchen()
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
    public static void StartTeaHouseFront()
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

    /// <summary>
    /// 검은화면으로 페이드 인 하며 씬 변경, 씬 로딩 후 페이드 아웃.
    /// </summary>
    /// <param name="sceneName"></param>
    public static void LoadScene(string sceneName)
    {
        GeneralDirection.Instance.FadeInBlack(0.5f);
        currentSceneName = sceneName;
        CoroutineUtil.Instance.RunCoroutine(LoadSceneRoutine(sceneName));
    }
    private static IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. 검은 화면으로 페이드 인
        yield return new WaitForSecondsRealtime(0.5f);

        // 2. 씬 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // 3. 로딩이 끝날 때까지 대기
        while (op.progress < 0.9f)
            yield return null;

        // 4. 씬 전환 허용
        op.allowSceneActivation = true;

        // 5. 씬이 실제로 로드 완료될 때까지 대기
        yield return new WaitUntil(() => op.isDone);

        // 6. 다음 프레임에서 페이드아웃 시작
        yield return null;
        GeneralDirection.Instance.FadeOutBlack(1f);
    }

    public static bool IsInKitchen()
    {
        return currentSceneName == KITCHEN_SCENE_NAME;
    }

    public static bool IsInTeaHouseFront()
    {
        return currentSceneName == TEA_HOUSE_FRONT_SCENE_NAME;
    }

    public static bool IsInField()
    {
        return currentSceneName == FIELD_SCENE_NAME;
    }
}
