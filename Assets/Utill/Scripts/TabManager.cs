using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject panel;
    }

    [SerializeField] private List<Tab> tabs;
    private Tab currentTab;

    private void Start()
    {
        foreach (var tab in tabs)
        {
            Tab localTab = tab;
            tab.button.onClick.AddListener(() => SwitchTab(localTab));
        }
        if (tabs.Count > 0)
            SwitchTab(tabs[0]);
    }

    public void SwitchTab(Tab newTab)
    {
        if (currentTab != null)
            currentTab.panel.SetActive(false);

        newTab.panel.SetActive(true);
        currentTab = newTab;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
