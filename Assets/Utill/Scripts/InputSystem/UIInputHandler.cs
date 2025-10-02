using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// UI를 관리하는 핸들러
/// 활성화/비활성화가 유동적인 오브젝트 관리 예정
/// </summary>

public class UIInputHandler : MonoBehaviour, IInputHandler
{
    public int Priority => 100;

    // [Header("관리 오브젝트 목록")]
    [SerializeField] private TabUIController tabUIController;
    [SerializeField] private DialogueLogManager dialogueLogManager;
    [SerializeField] private GameObject pauseMenu;

    // 액션 동작의 구현부 메서드를 이벤트에 연결해 HandleInput에서 Invoke
    public event System.Action OnToggleUIRequested;
    public event System.Action OnCloseUIRequested;

    void Start()
    {
        tabUIController = TabUIController.Instance;
        dialogueLogManager = DialogueLogManager.Instance;
        Debug.Assert(tabUIController != null, "TabUIController instance is missing in the scene.");
        Debug.Assert(dialogueLogManager != null, "DialogueLogManager instance is missing in the scene.");
    }

    void OnEnable()
    {
        InputManager.Instance.RegisterHandler(this);
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.UnregisterHandler(this);
    }

    /// <summary>
    /// InputManager의 OnAction 메서드에 의해 호출
    /// true 반환: 입력 차단 필요한 상황 or 입력 짝 찾음 -> 입력 전달 끝
    /// false 반환: 다음 핸들러에 입력 토스
    /// </summary>
    public bool HandleInput(InputAction action, InputAction.CallbackContext context)
    {
        if ((tabUIController.IsUIOpen() || dialogueLogManager.IsLogPanelOpen())
        && action.name != "ToggleUI" && action.name != "CloseUI" && action.name != "ToggleLog")
        {
            return true;
        }

        if (pauseMenu.activeSelf && action.name != "CloseUI")
        {
            return true;
        }
        
        if (!tabUIController.IsUIOpen() && action.name == "ToggleLog")
        {
            DialogueLogManager.Instance.ToggleLog();
            return true;
        }

        if (!dialogueLogManager.IsLogPanelOpen() && action.name == "ToggleUI")
        {
            OnToggleUIRequested?.Invoke();
            return true;
        }

        if (action.name == "CloseUI")
        {
            OnCloseUIRequested?.Invoke();
            return true;
        }

        return false;
    }
}
