using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// 뭐하는 스크립트인가요?
// 현재 어떤 토글(탭)이 선택되어 있는 상태인지 저장/복원하고, 탭 활성화 및 애니메이션 재생을 명령.
// 토글 그룹(토글들의 부모 오브젝트, TabTags)에 붙여서 사용.
public class TabGroupManager : MonoBehaviour
{
    [System.Serializable]
    public class TabInfo
    {
        public Toggle toggle;
        public GameObject contentPanel;
        [HideInInspector] public TabAnimator animator;
    }

    [Header("탭 설정")]
    public List<TabInfo> tabs;
    [Tooltip("토글들을 관리하는 ToggleGroup 컴포넌트")]
    [SerializeField] private ToggleGroup toggleGroup;

    [Header("기본 탭 인덱스 설정")]
    [SerializeField] private string kitchenSceneName = "KitchenScene";
    [SerializeField] private int kitchenDefaultIndex = 1;
    [SerializeField] private int fieldDefaultIndex = 0;

    private static int? lastKnownTabIndex = null;
    private int _currentTabIndex = -1;

    // 게임을 시작할 때 static 변수를 초기화, Scene을 다시 로드해도 이전 Tab 열람 기록이 남지 않음.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData() { lastKnownTabIndex = null; }

    void Awake()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            if (tab.toggle != null)
            {
                tab.animator = tab.toggle.GetComponent<TabAnimator>();
                int index = i;
                tab.toggle.onValueChanged.RemoveAllListeners();
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

        if (lastKnownTabIndex.HasValue) // 이전에 탭을 열람한 기록이 있다면, 해당 탭을 표시
        {
            indexToOpen = lastKnownTabIndex.Value;
        }
        else    // 이전에 열람한 기록이 없다면(e.g. 씬 로드)
        {
            string currentScene = SceneManager.GetActiveScene().name;   // 현재 씬 확인 후 탭 결정
            indexToOpen = (currentScene == kitchenSceneName) ? kitchenDefaultIndex : fieldDefaultIndex; // 주방이라면 레시피, 필드라면 인벤토리
        }
        SelectTab(indexToOpen, false);
        StartCoroutine(ReEnableToggleGroupAfterFrame());
    }

    private IEnumerator ReEnableToggleGroupAfterFrame()
    {
        yield return null;
        toggleGroup.enabled = true;
    }

    private void SelectTab(int index, bool animate)
    {
        if (index < 0 || index >= tabs.Count || (_currentTabIndex != -1 && index == _currentTabIndex)) return;

        _currentTabIndex = index;
        lastKnownTabIndex = index;  // 마지막 탭을 static 변수로 기록

        for (int i = 0; i < tabs.Count; i++)
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