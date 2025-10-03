using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject slotPanel;
    public GameObject newSlotPanel;

    public void OnclickNewGame()
    {
        // 유니콘 빌드 제출용
        GameFlowManager.StartGame();

        // mainMenu.SetActive(false);
        // newSlotPanel.SetActive(true);
    }

    public void OnclickLoadGame()
    {
        mainMenu.SetActive(false);
        slotPanel.SetActive(true);
    }

    public void OnclickExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}