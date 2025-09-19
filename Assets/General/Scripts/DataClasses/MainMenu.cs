using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject slotPanel;

    public void OnclickNewGame()
    {
        Debug.Log("New Game");
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
#endif
    }
}