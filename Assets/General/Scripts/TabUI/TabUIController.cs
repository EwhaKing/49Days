using UnityEngine;

/// <summary>
/// 뭐하는 놈인가요? Tab/ESC 키보드 입력을 받아 전체 탭 UI 패널을 활성화/비활성화 하는 최상위 컨트롤러임.
/// </summary>
public class TabUIController : SceneSingleton<TabUIController>
{
    [Tooltip("인벤토리, 레시피 등 모든 탭 UI를 포함하는 최상위 패널")]
    [SerializeField] private GameObject commonPanel;
    [SerializeField] private UIInputHandler uiInputHandler;

    void OnEnable()
    {
        uiInputHandler = FindObjectOfType<UIInputHandler>();
        Debug.Assert(uiInputHandler != null, "UIInputHandler is missing on TabUIController GameObject.");
        
        uiInputHandler.OnToggleUIRequested += ToggleUI;
        uiInputHandler.OnCloseUIRequested += CloseUI;
    }

    void OnDestroy()
    {
        uiInputHandler.OnToggleUIRequested -= ToggleUI;
        uiInputHandler.OnCloseUIRequested -= CloseUI;
    }

    /// <summary>
    /// Player Input의 ToggleUI 액션에 의해 호출
    /// </summary>
    public void ToggleUI()
    {
        if (GameFlowManager.IsInStart()) return;

        commonPanel.SetActive(!commonPanel.activeSelf); // 현재 상태 반전
        if (commonPanel.activeSelf)
            GameManager.Instance.onUIOn?.Invoke();
    }

    /// <summary>
    /// Player Input의 CloseUI 액션에 의해 호출
    /// </summary>
    public void CloseUI()
    {
        if (commonPanel.activeSelf)  // 활성화 상태일 때만 닫기
        {
            commonPanel.SetActive(false);
        }
    }

    public bool IsUIOpen()
    {
        return commonPanel.activeSelf;
    }

    private void Start()
    {
        // 게임 시작 시에는 UI가 닫혀 있도록 설정.
        Debug.Assert(commonPanel != null, "Common Panel is not assigned in TabUIController.");
        commonPanel.SetActive(false);
    }
}
