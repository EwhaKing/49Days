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
        runner.AddFunction<string>("get_tea_name", GetTeaNameInLowerCase);
        runner.AddFunction<string>("get_additional_ingredient", GetAdditionalIngredientInLowerCase);
        runner.AddFunction<int>("get_brew_time_gap", GetBrewTimeGap);
        runner.AddFunction<int>("get_temperature_gap", GetTemperatureGap);
        runner.AddCommandHandler<string>("show_image", FadeIn);
        runner.AddCommandHandler<string>("play_sfx", PlaySfx);
        runner.AddCommandHandler<string>("change_bgm", ChangeBgm);
        runner.AddCommandHandler("skip_tutorial", SkipTutorial);

        runner.gameObject.SetActive(false);
        
        GameFlowManager.Instance.StartTeaHouseFront();
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
        UIManager.Instance.BlockingUIOn(runner.gameObject);
        runner.StartDialogue(nodeTitle);
    }

    void EndDialogue()
    {
        UIManager.Instance.BlockingUIOff(runner.gameObject);
        UIManager.Instance.BlockingUIOff(fadeImage.transform.parent.gameObject);  // TODO: 리팩토링
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

    public string GetTeaNameInLowerCase()
    {
        string teaName = OrderManager.Instance.GetMakedTeaName().ToLowerString();
        return teaName;
    }

    public string GetAdditionalIngredientInLowerCase()
    {
        string ingredientName = OrderManager.Instance.GetMakedAdditionalIngredient().ToLowerString();
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

    public void SkipTutorial()
    {
        GameManager.Instance.TutorialComplete();
    }
}
