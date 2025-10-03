using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager
{
    public static readonly string KITCHEN_SCENE_NAME = "Kitchen";
    public static readonly string TEA_HOUSE_FRONT_SCENE_NAME = "TeaHouseFront";
    public static readonly string FIELD_SCENE_NAME = "FieldPoC";
    public static readonly string START_SECENE_NAME = "Beginning";

    static string currentSceneName = START_SECENE_NAME;
    public static string GetCurruntSceneName => currentSceneName;
    public static event Action<string> onSceneLoaded;
    public static event Action onFinishField;

    public static void StartGame()
    {
        string sceneName = "";
        switch (GameManager.Instance.testMode)
        {
            case TestMode.무한_주방_모드:
                sceneName = KITCHEN_SCENE_NAME;
                break;
            case TestMode.일차0_밤_스타트_모드:
                sceneName = TEA_HOUSE_FRONT_SCENE_NAME;
                GameManager.Instance.SetDateZero();
                OrderManager.Instance.SetAfterNodeTitle("일차0_밤");
                break;
            case TestMode.낮_스타트_모드:
                sceneName = TEA_HOUSE_FRONT_SCENE_NAME;
                OrderManager.Instance.SetAfterNodeTitle("낮_주문");
                break;
            case TestMode.필드_스타트_모드:
                sceneName = FIELD_SCENE_NAME;
                break;
        }

        CoroutineUtil.Instance.RunAfterFirstFrame(() =>
        {
            LoadScene(sceneName);
        });
    }

    /// <summary>
    /// 필드 끝내고 찻집으로 돌아갈 때 실행
    /// </summary>
    public static void FinishField()
    {
        onFinishField?.Invoke();
        OrderManager.Instance.SetAfterNodeTitle("낮_주문");
        LoadScene(TEA_HOUSE_FRONT_SCENE_NAME);
    }

    public static void FinishDay()
    {
        if (GameManager.Instance.GetDate() == 2)
        {
            var panelController = UnityEngine.Object.FindObjectOfType<ReviewPanel>(true); 
            panelController.ShowReviewPanel();
            Debug.Log("테스트 분량 종료.");
            return;
        }
        GameManager.Instance.NextDay();
        LoadScene(FIELD_SCENE_NAME);
    }

    /// <summary>
    /// 주방에서 차 만들고 나올 때 실행
    /// </summary>
    public static void FinishKitchen()
    {
        if (GameManager.Instance.testMode == TestMode.무한_주방_모드)
        {
            LoadScene(KITCHEN_SCENE_NAME);
            return;
        }
        LoadScene(TEA_HOUSE_FRONT_SCENE_NAME);
    }

    /// <summary>
    /// 찻집에 들어왔을 때 실행
    /// </summary>
    public static void StartTeaHouseFront()
    {
        if (OrderManager.Instance.GetAfterNodeTitle() != "")
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
        Cursor.Instance.SetCursorInvisible();
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
        onSceneLoaded?.Invoke(sceneName);
        GeneralDirection.Instance.FadeOutBlack(1f);
        Cursor.Instance.SetCursorVisible();
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
    
    public static bool IsInStart()
    {
        return currentSceneName == START_SECENE_NAME;
    }
}
