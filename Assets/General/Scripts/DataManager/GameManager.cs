using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GeneralData
{
    public bool tutorialCompleted = false;
    public int money = 0;
}

public class GameManager : Singleton<GameManager>
{
    private GeneralData generalData = new GeneralData();

    public Action<int> onMoneyChanged;
    public int GetMoney() 
    { 
        return generalData.money; 
    }
    public void SetMoney(int value)
    { 
        generalData.money = Math.Clamp(value, 0, 9999999);
        onMoneyChanged?.Invoke(value);
    }
    public void AddMoney(int amount) 
    {
        SetMoney(generalData.money + amount);
    }

    public bool IsTutorialCompleted()
    {
        return generalData.tutorialCompleted;
    }
    public void TutorialComplete() 
    {
        generalData.tutorialCompleted = true; 
    }


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
        GameFlowManager.Instance.StartGame();
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
    일차0_밤_스타트_모드
}
