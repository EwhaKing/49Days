using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System;

public class TeaHouseYarnManager : MonoBehaviour
{
    [SerializeField] DialogueRunner runner;

    void Start()
    {
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
        runner.AddCommandHandler<string>("show_image", ShowImage);
        runner.AddCommandHandler<string>("play_sfx", PlaySfx);
        runner.AddCommandHandler<string>("change_bgm", ChangeBgm);

        runner.gameObject.SetActive(false);
    }

    public void RunDialogue(string nodeTitle)
    {
        UIManager.Instance.BlockingUIOn(runner.gameObject);
        runner.StartDialogue(nodeTitle);
    }

    public void EndDialogue()
    {
        UIManager.Instance.BlockingUIOff(runner.gameObject);
    }

    public void EnterAndSit(string npcName, int seatIndex)
    {
        // 종 울리고 1초뒤 착석
    }

    public void Exit(string npcName)
    {

    }

    public void AffinityChange(string npcName, int change)
    {
        // npc는 enum 만들고 변환해서 사용
    }

    public void ChangeNpcSprite(string npcName, string spriteName)
    {

    }

    public void CameraZoom(int zoomPreset)
    {

    }

    public void CameraSplitZoom(string left, string right)
    {

    }

    public void Order(string afterNodeTitle, bool autoPay = true)
    {

    }

    public void AddSuccessTea(string teaName, string additionalIngredient = "None")
    {
        // 성공 차 추가 로직
    }

    public void Evaluate(bool autoPay = true)  // afterNodeTitle 재생할때 자동으로 채점
    {
        // TeaName teaName = tea.ToEnum<TeaName>();
        // IngredientName additionalIngredientName = additionalIngredient.ToEnum<IngredientName>();

        // 평가 후 자동으로 pay 처리
        if (autoPay)
        {
            Pay();
        }
    }

    public void Pay()
    {
        // 결제 로직
    }

    public string GetEvaluationResult()
    {
        // 평가 결과 반환
        return "평가 결과";  // 완벽, 보통, 나쁨
    }

    public string GetTeaNameInLowerCase()
    {
        // 만든 차 이름 반환
        return "차 이름";
    }

    public string GetAdditionalIngredientInLowerCase()
    {
        // 추가 재료 이름 반환
        return "추가 재료 이름";
    }

    public int GetBrewTimeGap()
    {
        // 우린 시간 차 반환
        return 0;
    }

    public int GetTemperatureGap()
    {
        // 온도 차 반환
        return 0;
    }

    public void ShowImage(string imageName)
    {

    }

    public void PlaySfx(string sfxName)
    {
        
    }

    public void ChangeBgm(string bgmName)
    {

    }
}
