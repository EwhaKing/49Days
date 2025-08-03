using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabUIManager : MonoBehaviour
{
    [Header("책갈피(토클 버튼)")]
    [SerializeField] private Toggle[] tabToggles;   // 이곳에 책갈피를 붙이면 됩니다. 인벤토리/레시피/호감도/퀘스트 4개 예정

    [Header("정보 패널")]
    [SerializeField] private GameObject[] panels;   // 이곳에는 패널들을 붙입니다.

    private void Start()
    {
        for (int i = 0; i < tabToggles.Length; i++)
        {
            int index = i;
            tabToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn) ShowPanel(index);
            });
        }
        ShowPanel(0);
    }

    private void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }
}
