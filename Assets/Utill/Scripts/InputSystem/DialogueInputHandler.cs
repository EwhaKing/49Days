using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 다이얼로그를 관리하는 핸들러
/// </summary> 

public class DialogueInputHandler : MonoBehaviour, IInputHandler
{
    public int Priority => 10;

    [Header("관리 오브젝트 목록")]
    [SerializeField] public GameObject playerDialogueBox;
    [SerializeField] public GameObject npcDialogueBox;

    // 액션 동작의 구현부 메서드를 이벤트에 연결해 HandleInput에서 Invoke
    public event System.Action OnDialogueContinueRequested;

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
        if (!(playerDialogueBox.activeSelf || npcDialogueBox.activeSelf))
        {
            return false;
        }

        if (action.name == "DialogueContinue")
        {
            OnDialogueContinueRequested?.Invoke();
        }

        return true;
    }

}