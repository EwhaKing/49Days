using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


// 뭐하는 스크립트인가요?
// 현재 어떤 토글(탭)이 선택되어 있는 상태인지 저장/복원하고, 탭 활성화 및 애니메이션 재생을 명령.
// 토글 그룹(토글들의 부모 오브젝트, TabTags)에 붙여서 사용.
public class TabGroupManager : MonoBehaviour
{
    // 각 탭의 토글과 패널을 연결하기 위한 클래스
    [System.Serializable]
    public class TabInfo
    {
        public Toggle toggle;
        public GameObject contentPanel;
        [HideInInspector] public TabAnimator animator;
    }

    [Tooltip("관리할 탭 리스트")]
    public List<TabInfo> tabs;

    private static int? lastKnownTabIndex = null;
    private int _currentTabIndex = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        lastKnownTabIndex = null;
    }

    void Awake()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            tab.animator = tab.toggle.GetComponent<TabAnimator>();

            int index = i;
            tab.toggle.onValueChanged.AddListener((isOn) => {
                if (isOn)
                {
                    SelectTab(index, true);
                }
            });
        }
    }

    void OnEnable()
    {
        // static 변수에서 마지막 인덱스를 불러옴. 값이 없다면(e.g. 씬 첫 실행) 0을 사용
        int lastIndex = lastKnownTabIndex ?? 0;
        SelectTab(lastIndex, false);
    }

    private void SelectTab(int index, bool animate)
    {
        if (index < 0 || index >= tabs.Count || index == _currentTabIndex)
        {
            return;
        }
        _currentTabIndex = index;
        lastKnownTabIndex = index;  // static 변수에 현재 탭 인덱스를 저장

        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            bool IsSelected = (i == index);

            tab.animator.SetSelectionState(IsSelected, animate);
            tab.contentPanel.SetActive(IsSelected);
            tab.toggle.SetIsOnWithoutNotify(IsSelected);

            // if (i == index)
            // {
            //     if (animate) tab.animator.AnimateUp();
            //     else tab.animator.SetUp();

            //     tab.contentPanel.SetActive(true);
            //     tab.toggle.SetIsOnWithoutNotify(true);
            // }

            // else
            // {
            //     if (animate) tab.animator.AnimateDown();
            //     else tab.animator.SetDown();

            //     tab.contentPanel.SetActive(false);
            //     tab.toggle.SetIsOnWithoutNotify(false);
            // }
        }
    }
}
