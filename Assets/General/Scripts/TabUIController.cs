using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabUIController : MonoBehaviour
{
    [SerializeField] private GameObject commonPanel;

    /// <summary>
    /// Player Input의 ToggleUI 액션이 호출하는 함수
    /// Tab 키 입력시 CommonPanel 활성화 상태를 토글할 것.
    /// </summary>
    public void OnToggleUI()
    {
        bool nextState = !commonPanel.activeSelf;
        commonPanel.SetActive(nextState);
    }

    /// <summary>
    /// Player Input의 CloseUI 액션이 호출하는 함수
    /// ESC 키 입력 시 CommonPanel이 활성화 상태라면 비활성화.
    /// </summary>
    public void OnCloseUI()
    {
        if (commonPanel.activeSelf)
        {
            commonPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // 게임 시작 시 UI가 닫혀 있도록 세팅.
        commonPanel.SetActive(false);
    }
}
