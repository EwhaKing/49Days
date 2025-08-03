using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TabUIController : MonoBehaviour
{
    [SerializeField] private GameObject commonPanel;
    private systemActions controls;     // InputActions에서 생성한 클래스

    private void OnEnable()
    {
        if (controls == null) controls = new systemActions();
        controls.Enable();
        controls.SystemActions.ToggleUI.performed += OnToggleUI;    // SystemActions의 Tab 키 액션
    }

    private void OnDisable()
    {
        controls.SystemActions.ToggleUI.performed -= OnToggleUI;
        controls.Disable();
    }

    /// <summary>
    /// Tab 키 입력시 CommonPanel 활성화 상태를 토글할 것.
    /// </summary>
    private void OnToggleUI(InputAction.CallbackContext context)
    {
        commonPanel.SetActive(!commonPanel.activeSelf);
    }


    private void Start()
    {
        commonPanel.SetActive(false);
    }

    void Update()
    {
        
    }
}
