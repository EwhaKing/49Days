using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GeneralData
{
    public int money = 0;
    public int week = 1; // 1 ~ 7 주차
    public int day = 1;  // 1 ~ 7 각 주의 일차
}

public class GameManager : Singleton<GameManager>
{
    private GeneralData generalData = new GeneralData();

    /// <summary>
    ///  변동된 금액, 현재 잔액 순서
    /// </summary>
    public Action<int, int> onMoneyChanged;
    public Action onWeekChanged;
    public Action onDayChanged;
    public Action onUIOn;

    public int GetMoney() { return generalData.money; }
    public void AddMoney(int amount) 
    {
        generalData.money = Math.Clamp(amount + generalData.money, 0, 9999999);
        onMoneyChanged?.Invoke(amount, generalData.money);
    }

    /// <summary>
    /// 49일 중 몇 일째인지를 반환합니다. (1 ~ 49)
    /// </summary>
    /// <returns></returns>
    public int GetDate() { return (generalData.week - 1) * 7 + generalData.day; }

    /// <summary>
    /// 7주차 중 몇 주차인지를 반환합니다. (1 ~ 7)
    /// </summary>
    /// <returns></returns>
    public int GetWeek() { return generalData.week; }
    private void NextWeek()
    {
        generalData.week++;
        onWeekChanged?.Invoke();
    }

    /// <summary>
    /// 각 주의 몇 일차인지를 반환합니다. (1 ~ 7)
    /// </summary>
    /// <returns></returns>
    public int GetDay() { return generalData.day; }

    /// <summary>
    /// 다음 날로 넘기는 함수입니다.
    /// </summary>
    public void NextDay()
    {
        generalData.day++;
        if (generalData.day > 7)
        {
            generalData.day = 1;
            NextWeek();
        }
        onDayChanged?.Invoke();
    }

    public void SetDateZero() // 임시
    {
        generalData.week = 1;
        generalData.day = 0;
    }


    // private string timeOfDay = "낮"; // "낮" or "밤"
    // public string GetTimeOfDay() { return timeOfDay; }
    // public void SetTimeOfDay(string timeOfDay)
    // {
    //     if (timeOfDay != "낮" && timeOfDay != "밤") 
    //     {
    //         Debug.LogError("SetTimeOfDay: '낮' 또는 '밤'만 설정 가능");
    //         return;
    //     }
    //     this.timeOfDay = timeOfDay;
    // }


    void OnEnable() {
        SaveLoadManager.Instance.onSave += () => SaveLoadManager.Instance.Save<GeneralData>(generalData);
        SaveLoadManager.Instance.onLoad += () => {generalData = SaveLoadManager.Instance.Load<GeneralData>();};
    }

    void OnDisable() {
        SaveLoadManager.Instance.onSave -= () => SaveLoadManager.Instance.Save<GeneralData>(generalData);
        SaveLoadManager.Instance.onLoad -= () => {generalData = SaveLoadManager.Instance.Load<GeneralData>();};
    }

    //test

    [Header("테스트 모드 설정")]

    public TestMode testMode;

    private void Start()
    {
        GameFlowManager.StartGame();
    }
    public void testSave(){
        SaveLoadManager.Instance.SaveAllByDate(1);
    }
    public void testLoad(){
        SaveLoadManager.Instance.LoadAllByDate(1);
    }
}

public enum TestMode
{
    무한_주방_모드,
    일차0_밤_스타트_모드,
    낮_스타트_모드,
    필드_스타트_모드,
}
