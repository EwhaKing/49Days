using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 이것저것 관리하는 잡탕 핸들러
/// 씬 전환 전까지는 비활성화 되지 않는 오브젝트 관리 예정
/// </summary> 

public class GameInputHandler : MonoBehaviour, IInputHandler
{
    public int Priority => 1;

    [Header("관리 오브젝트 목록")]
    [SerializeField] public GameObject moveCamera;
    [SerializeField] public GameObject rollingMachine;

    // 액션 동작의 구현부 메서드를 이벤트에 연결해 HandleInput에서 Invoke
    public event System.Action OnMoveCameraRequested;
    public event System.Action<InputAction.CallbackContext> OnWASDRequested;
    public event System.Action<Vector2> OnPlayerMoveRequested;

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
        if (action.name != "WASD" && action.name != "MoveCamera" && action.name != "PlayerMove")
        {
            return true;
        }

        if (action.name == "WASD")
        {
            OnWASDRequested?.Invoke(context);
            return true;
        }
        
        if (action.name == "PlayerMove")
        {
            OnPlayerMoveRequested?.Invoke(context.ReadValue<Vector2>());
            return true;
        }

        if (action.name == "MoveCamera")
        {
            OnMoveCameraRequested?.Invoke();
            return true;
        }

        return false;
    }

}