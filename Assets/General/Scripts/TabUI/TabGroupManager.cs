using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;


// 뭐하는 스크립트인가요?
// 현재 어떤 토글(탭)이 선택되어 있는 상태인지 저장/복원하고, 탭 활성화 및 애니메이션 재생을 명령.
// 토글 그룹(토글들의 부모 오브젝트, TabTags)에 붙여서 사용.
public class TabGroupManager : MonoBehaviour
{
    // 개별 탭 하나를 구성하는 UI 요소들의 묶음.
    [System.Serializable]
    public class TabInfo
    {
        public Toggle toggle;                           // 책갈피 역할의 토글 버튼
        public GameObject contentPanel;                 // 해당 토글이 선택되었을 때 활성화할 패널
        [HideInInspector] public TabAnimator animator;  // 토글에 연결된 애니메이터.
    }

    [Header("탭 설정")]
    public List<TabInfo> tabs;
    [Tooltip("토글들을 관리하는 ToggleGroup 컴포넌트")]
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("기본 탭 인덱스 설정")]
    [SerializeField] private string kitchenSceneName = "Kitchen";
    [SerializeField] private string frontSceneName = "TeaHouseFront";
    [SerializeField] private int kitchenDefaultIndex = 1;
    [SerializeField] private int fieldDefaultIndex = 0;

    private static int? lastKnownTabIndex = null;      // 마지막으로 선택한 탭을 기억함.
    private int isTeaHouseScene => SceneManager.GetActiveScene().name == frontSceneName || SceneManager.GetActiveScene().name == kitchenSceneName ? 1 : 0;
    private int _currentTabIndex = -1;

    // 게임을 시작할 때 static 변수를 초기화, Scene을 다시 로드해도 이전 Tab 열람 기록이 남지 않음.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData() { lastKnownTabIndex = null; }

    // public static event Action OnItemDroppedOnBackground;

    // public void OnDrop(PointerEventData eventData)
    // {
    //     OnItemDroppedOnBackground?.Invoke();
    // }

    void Awake()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }

        for (int i = 0; i < tabs.Count; i++)    // 모든 탭을 순회하며 초기 설정.
        {
            var tab = tabs[i];
            if (tab.toggle != null)
            {
                tab.animator = tab.toggle.GetComponent<TabAnimator>();  // TabAnimator 컴포넌트를 찾아오기
                int index = i;
                tab.toggle.onValueChanged.RemoveAllListeners();         // 인덱스를 지역 변수에 복사 (뭔 클로저 문제가 있다는데요)
                tab.toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) SelectTab(index, true);
                });
            }
            else
            {
                Debug.Log($"TabGroupManager: 탭 리스트의 {i}번째 Toggle이 비어 있으니 인스펙터 확인 요망.");
            }

        }
    }

    void OnEnable()
    {
        // 토글-패널 간섭을 방지하기 위해 비활성화
        toggleGroup.enabled = false;
        int indexToOpen;
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Kitchen" || currentScene == "TeaHouseFront")
        {
            SelectTab(1, false);
        }
        else { SelectTab(0, false); }

        // if (lastKnownTabIndex.HasValue) // 이전에 탭을 열람한 기록이 있다면, 해당 탭을 표시
        // {
        //     indexToOpen = lastKnownTabIndex.Value;
        // }
        // else    // 이전에 열람한 기록이 없다면(e.g. 씬 로드)
        // {
        //     // indexToOpen = (currentScene == kitchenSceneName) ? kitchenDefaultIndex : fieldDefaultIndex; // 주방이라면 레시피, 필드라면 인벤토리
        //     // indexToOpen = isTeaHouseScene == 1 ? kitchenDefaultIndex : fieldDefaultIndex;
        //     indexToOpen = (currentScene == kitchenSceneName || currentScene == frontSceneName) ? kitchenDefaultIndex : fieldDefaultIndex;
        // }
        // SelectTab(indexToOpen, false);
        StartCoroutine(ReEnableToggleGroupAfterFrame());

        // UI가 열릴 때마다 인벤토리 UI가 최신 상태를 반영하도록 강제 갱신.
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryDisplay();
        }
        else
        {
            Debug.LogWarning("InventoryUI instance not found in the scene.");
        }
        Debug.Log($"[탭 확인] OnEnable 시작. lastKnownTabIndex의 현재 값: {lastKnownTabIndex}");
    }

    private IEnumerator ReEnableToggleGroupAfterFrame()
    {
        yield return null;
        toggleGroup.enabled = true;
    }



    /// <summary>
    /// 특정 인덱스의 탭을 선택하고 나머지 탭들은 비선택 상태로 만듭니다.
    /// </summary>
    /// <param name="index">선택할 탭의 인덱스</param>
    /// <param name="animate">애니메이션 재생 여부</param>
    private void SelectTab(int index, bool animate)
    {
        // 현재 인덱스 OR 유효하지 않은 인덱스 -> 아무고토 하지 않은.
        if (index < 0 || index >= tabs.Count || (_currentTabIndex != -1 && index == _currentTabIndex)) return;

        _currentTabIndex = index;
        lastKnownTabIndex = index;  // 마지막 탭을 static 변수로 기록

        for (int i = 0; i < tabs.Count; i++)    // 탭을 돌며 상태 업데이트
        {
            var tab = tabs[i];
            bool isSelected = (i == index);

            if (tab.animator != null)
            {
                tab.animator.SetSelectionState(isSelected, animate);
            }
            else
            {
                Debug.Log($"Tab {i} ({tab.toggle.name})에 TabAnimator 컴포넌트가 없으니 확인.");
            }
            if (tab.contentPanel != null)
            {
                tab.contentPanel.SetActive(isSelected);
            }
            if (tab.toggle != null)
            {
                tab.toggle.SetIsOnWithoutNotify(isSelected);
            }
        }
        Debug.Log($"TabGroupManager: 탭 {index} 선택됨");
    }

    public void ForceSelectTab(int index)
    {
        SelectTab(index, true);
    }
}