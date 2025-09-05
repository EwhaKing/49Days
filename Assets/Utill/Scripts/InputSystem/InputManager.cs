using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

/// <summary>
/// 키 입력을 받는 모든 오브젝트 관리
/// 우선순위가 높은 핸들러의 입력 처리, 낮은 핸들러의 입력 차단
/// 모든 액션의 입력은 InputManager을 통해 구현부로 전달됨
/// InputManager -> Handler -> 구현부 순으로 입력 이동
/// </summary>

public class InputManager : Singleton<InputManager>
{
    private List<IInputHandler> handlers = new List<IInputHandler>();
    [SerializeField] public InputActionAsset inputActions;

    void Start()
    {
        // 모든 액션에 콜백 등록
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.performed += ctx => OnAction(action, ctx);
            }
        }
        inputActions.Enable();
    }

    /// <summary>
    /// 핸들러를 우선순위 기반 내림차순 정렬
    /// </summary>
    public void RegisterHandler(IInputHandler handler)
    {
        if (!handlers.Contains(handler))
            handlers.Add(handler);
            handlers = handlers.OrderByDescending(h => h.Priority).ToList();
    }

    public void UnregisterHandler(IInputHandler handler)
    {
        handlers.Remove(handler);
    }

    /// <summary>
    /// 액션의 입력이 들어오면 호출됨
    /// 핸들러를 순회하며 입력에 대한 적절한 액션 탐색
    /// 액션 찾으면 break
    /// </summary>
    private void OnAction(InputAction actionName, InputAction.CallbackContext context)
    {
        foreach (var handler in handlers)
        {
            if (handler.HandleInput(actionName, context))
                break;
        }
    }
}