using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using Cinemachine;
using System;

public class TeaHouseYarnManager : SceneSingleton<TeaHouseYarnManager>
{
    [SerializeField] DialogueRunner runner;
    [SerializeField] Image fadeImage;
    [SerializeField] CinemachineVirtualCamera[] cameras;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera subCamera;

    [SerializeField] private CinemachineVirtualCamera mainVCam;  // left 
    [SerializeField] private CinemachineVirtualCamera subVCam;  // right

    void Start()
    {
        subCamera.enabled = false;
        mainCamera = Camera.main;
        CameraZoom(OrderManager.Instance.zoomPreset);
        fadeImage.transform.parent.gameObject.SetActive(false);

        runner.onDialogueComplete.AddListener(EndDialogue);
        
        runner.AddCommandHandler<string, int>("enter_and_sit", EnterAndSit);
        runner.AddCommandHandler<string>("exit", Exit);
        runner.AddCommandHandler<string, int>("affinity_change", AffinityChange);
        runner.AddCommandHandler<string, string>("change_sprite", ChangeNpcSprite);
        runner.AddCommandHandler<int>("zoom", CameraZoom);
        runner.AddCommandHandler<string, string>("split_zoom", CameraSplitZoom);
        runner.AddCommandHandler<string, bool>("order", Order);
        runner.AddCommandHandler<string, string>("add_success_tea", AddSuccessTea);
        runner.AddCommandHandler("pay", Pay);
        runner.AddFunction<string>("get_evaluation_result", GetEvaluationResult);
        runner.AddFunction<string>("get_tea_name", GetTeaNameInKor);
        runner.AddFunction<string>("get_additional_ingredient", GetAdditionalIngredientInKor);
        runner.AddFunction<int>("get_brew_time_gap", GetBrewTimeGap);
        runner.AddFunction<int>("get_temperature_gap", GetTemperatureGap);
        runner.AddCommandHandler<string>("show_image", FadeIn);
        runner.AddCommandHandler<string>("play_sfx", PlaySfx);
        runner.AddCommandHandler<string>("change_bgm", ChangeBgm);
        runner.AddFunction<int>("get_day", GetDay);
        runner.AddFunction<int>("get_week", GetWeek);
        runner.AddFunction<int>("get_money", GetMoney);
        runner.AddFunction<string>("get_ordered_tea", GetOrderedTeaInKorean);
        runner.AddFunction<string>("get_ordered_additional_ingredient", GetOrderedAdditionalIngredientInKorean);
        runner.AddCommandHandler("add_random_success_tea", OrderManager.Instance.GenerateRandomSuccessTea);
        runner.AddFunction<bool>("is_recent_unlocked_tea", IsRecentUnlockedTea);
        runner.AddCommandHandler("make_night", MakeNight);
        runner.AddCommandHandler("finish_night", FinishNight);
        runner.AddFunction<int>("get_day_order_count", GetDayOrderCount);
        runner.AddCommandHandler("increment_day_order_count", IncrementDayOrderCount);
        runner.AddCommandHandler("reset_day_order_count", ResetDayOrderCount);
        runner.AddCommandHandler<string>("go_to_kitchen", GoToKitchen);
        runner.AddCommandHandler<string>("set_kitchen_node_title", SetKitchenNodeTitle);

        runner.gameObject.SetActive(false);
        
        GameFlowManager.StartTeaHouseFront();
    }

    public void MakeNight()
    {
        // 밤 전환 연출 넣기
        Action startNightDialogue = () => RunDialogue($"일차{GameManager.Instance.GetDate()}_밤");
        CoroutineUtil.Instance.RunAfterSeconds(startNightDialogue, 1.0f);
    }

    public void FinishNight()
    {
        // 하루가 지나는 연출 넣기
        GameFlowManager.FinishDay();
    }

    /// <summary>
    /// 주방으로 넘어가는 함수. 넘어가서 실행할 노드를 인자로 넘겨준다. <br/>
    /// 후에 go_back_teahouse_front를 해주지 않으면 게임 플로우가 꼬이니 주의
    /// </summary>
    /// <param name="nodeTitle"></param>
    public void GoToKitchen(string nodeTitle)
    {
        OrderManager.Instance.kitchenNodeTitle = nodeTitle;
        GameFlowManager.LoadScene(GameFlowManager.KITCHEN_SCENE_NAME);
    }

    /// <summary>
    /// 지금 당장은 아니고 order로 넘어간 주방에서 실행할 노드를 설정한다.
    /// </summary>
    public void SetKitchenNodeTitle(string nodeTitle)
    {
        OrderManager.Instance.kitchenNodeTitle = nodeTitle;
    }

    public int GetDayOrderCount()
    {
        return OrderManager.Instance.GetDayOrderCount();
    }
    public void IncrementDayOrderCount()
    {
        OrderManager.Instance.IncrementDayOrderCount();
    }
    public void ResetDayOrderCount()
    {
        OrderManager.Instance.ResetDayOrderCount();
    }

    public int GetDay()
    {
        return GameManager.Instance.GetDay();
    }

    public int GetWeek()
    {
        return GameManager.Instance.GetWeek();
    }

    public int GetMoney()
    {
        return GameManager.Instance.GetMoney();
    }

    /// <summary>
    /// 주문받은 차의 한글 이름 반환
    /// </summary>
    /// <returns></returns>
    public string GetOrderedTeaInKorean()
    {
        return OrderManager.Instance.GetOrderedTea().ToKorean();
    }

    /// <summary>
    /// 주문받은 추가 재료의 한글 이름 반환
    /// </summary>
    /// <returns></returns>
    public string GetOrderedAdditionalIngredientInKorean()
    {
        return OrderManager.Instance.GetOrderedAdditionalIngredient().ToKorean();
    }

    /// <summary>
    /// 주문받은 차가 최근 해금된 차인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsRecentUnlockedTea()
    {
        return OrderManager.Instance.IsRecentUnlockedTea();
    }

    void Update() // TODO: 최적화
    {
        if (subVCam != null && subCamera != null) 
        {
            subCamera.transform.position = subVCam.transform.position;
        }
    }

    public void RunDialogue(string nodeTitle)
    {
        runner.gameObject.SetActive(true);
        Debug.Log($"TeaHouseYarnManager: RunDialogue {nodeTitle}");
        runner.StartDialogue(nodeTitle);
    }

    void EndDialogue()
    {
        runner.gameObject.SetActive(false);
        fadeImage.transform.parent.gameObject.SetActive(false);  // TODO: 리팩토링
    }

    IEnumerator EnterAndSit(string npcName, int seatIndex)
    {
        SoundManager.Instance.PlaySfx("bell");
        yield return new WaitForSeconds(1f);
        CustomerManager.Instance.SpawnCustomer(npcName, seatIndex);
    }

    public void Exit(string npcName)
    {
        CustomerManager.Instance.ExitCustomerAt(npcName);
    }

    public void AffinityChange(string npcName, int change)
    {
        if (change > 0)
            CustomerManager.Instance.HeartUp(npcName);
        else
            CustomerManager.Instance.HeartDown(npcName);

        CharacterManager.Instance.AddAffinity(npcName, change);
    }

    public void ChangeNpcSprite(string npcName, string poseName)
    {
        CustomerManager.Instance.ChangeCustomerPose(npcName, poseName);
    }

    public void CameraZoom(int zoomPreset)
    {
        OrderManager.Instance.zoomPreset = zoomPreset;
        for(int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == zoomPreset) ? 10 : 0;
        }

        mainCamera.rect = new Rect(0, 0, 1, 1);
        subCamera.enabled = false;

        Debug.Log($"카메라 줌: {zoomPreset}");
    }

    public void CameraSplitZoom(string left, string right)
    {
        var leftTarget = GameObject.Find(left);
        var rightTarget = GameObject.Find(right);

        if (leftTarget == null || rightTarget == null) return;

        mainVCam.Priority = 10;

        for(int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = 0;
        }

        // 왼쪽 뷰
        mainVCam.Follow = leftTarget.transform;
        mainVCam.LookAt = leftTarget.transform;
        mainCamera.rect = new Rect(0, 0, 0.5f, 1);

        // 오른쪽 뷰
        subCamera.enabled = true;
        subCamera.rect = new Rect(0.5f, 0, 0.5f, 1);

        subVCam.Follow = rightTarget.transform;
        subVCam.LookAt = rightTarget.transform;
    }

    public void Order(string afterNodeTitle, bool autoPay = true)
    {
        OrderManager.Instance.Order(afterNodeTitle, autoPay);
    }

    public void AddSuccessTea(string teaName, string additionalIngredient = "None")
    {
        TeaName tea = teaName.ToEnum<TeaName>();
        IngredientName additionalIngredientName = additionalIngredient.ToEnum<IngredientName>();
        OrderManager.Instance.AddSuccessTea(tea, additionalIngredientName);
    }

    public void Pay()
    {
        OrderManager.Instance.Pay();
    }

    public string GetEvaluationResult()
    {
        switch(OrderManager.Instance.GetEvaluationResult())
        {
            case EvaluationResult.Excellent:
                Debug.Log("평가 결과: 완벽");
                return "완벽";
            case EvaluationResult.Normal:
                Debug.Log("평가 결과: 보통");
                return "보통";
            case EvaluationResult.Bad:
                Debug.Log("평가 결과: 나쁨");
                return "나쁨";
            default:
                return "컴파일러 오류 방지용";
        }
    }

    public string GetTeaNameInKor()
    {
        string teaName = OrderManager.Instance.GetMakedTeaName().ToKorean();
        return teaName;
    }

    public string GetAdditionalIngredientInKor()
    {
        string ingredientName = OrderManager.Instance.GetMakedAdditionalIngredient().ToKorean();
        return ingredientName;
    }

    public int GetBrewTimeGap()
    {
        return OrderManager.Instance.GetMakedBrewTimeGap();
    }

    public int GetTemperatureGap()
    {
        return OrderManager.Instance.GetMakedTemperatureGap();
    }

    IEnumerator FadeIn(string spriteName)  // TODO: 리팩토링
    {
        yield return new WaitForSecondsRealtime(1f);
        Sprite sprite = Resources.Load<Sprite>($"Arts/{spriteName}");
        fadeImage.sprite = sprite;
        fadeImage.SetNativeSize();
        UIManager.Instance.BlockingUIOn(fadeImage.transform.parent.gameObject);
        yield return new WaitForSecondsRealtime(fadeImage.transform.parent.GetComponent<UIFadeInOnEnable>().fadeDuration);
    }

    public void PlaySfx(string sfxName)
    {
        SoundManager.Instance.PlaySfx(sfxName);
    }

    public void ChangeBgm(string bgmName)
    {
        SoundManager.Instance.PlayBgm(bgmName);
    }
}
