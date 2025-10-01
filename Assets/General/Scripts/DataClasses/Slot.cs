using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

/// <summary>
/// [통합 스크립트] 저장 슬롯의 생성, UI 관리, 클릭 이벤트, 씬 로드 기능을 모두 담당합니다.

public class SaveSlotUI : MonoBehaviour 
{
    [Header("프리펩 및 UI 설정")]
    [Tooltip("버튼과 두 개의 TextMeshProUGUI 자식 오브젝트를 가진 UI 프리펩")]
    [SerializeField] private GameObject slotPrefab; 
    [Tooltip("슬롯 프리펩들이 생성되어 자식으로 들어갈 부모 UI 오브젝트")]
    [SerializeField] private Transform slotContainer; 
    [SerializeField] private int numberOfSlots = 3;

    [Header("상호작용할 UI 버튼")]
    [Tooltip("슬롯이 선택되었을 때 활성화될 불러오기 버튼")]
    [SerializeField] private Button loadButton;

    [Header("씬 설정")]
    [SerializeField] private string gameScene = "MainGame";

    // 각 슬롯의 상태 정보를 내부적으로 관리하기 위한 리스트
    private List<SlotInfo> slotInfoList = new List<SlotInfo>();
    private SlotInfo lastSelectedSlot = null;
    // 각 슬롯의 상태를 관리하는 내부 클래스
    DateTime now = DateTime.Now;
    private class SlotInfo
    {
        public int SlotNumber;
        public bool IsEmpty = true;
        public string StatusText;
        public string RecentPlayDate;
        // 추후 날짜, 진행도 등 추가 데이터 저장 가능
        //test
        public SlotInfo()
        {
            RecentPlayDate = DateTime.Now.ToString("yyyy.MM.dd");
        }
    }

    void Awake()
    {
        // 씬 시작 시 슬롯을 초기화하고 생성합니다.
        InitializeSlots();

        if (loadButton != null)
        {
            loadButton.interactable = false;
            // 불러오기 버튼에 클릭 이벤트를 연결합니다.
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
    }

    /// <summary>
    /// 모든 슬롯 UI를 제거한 후 다시 생성합니다. UI 버튼의 onClick 이벤트에 연결하여 사용할 수 있습니다.
    public void InitializeSlots()
    {
        // 기존에 생성된 슬롯 UI가 있다면 먼저 삭제 (중복 생성 방지)
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        slotInfoList.Clear();

        // 설정된 개수만큼 슬롯 프리펩을 생성하고 초기화
        for (int i = 0; i < numberOfSlots; i++)
        {
            int currentSlotNumber = i + 1; // 슬롯 번호는 1부터 시작

            // 1. 프리펩 인스턴스화
            GameObject slotInstance = Instantiate(slotPrefab, slotContainer);
            slotInstance.name = $"SaveSlot_{currentSlotNumber}";

            // 2. 슬롯 데이터 생성 및 리스트에 추가
            SlotInfo newSlotInfo = new SlotInfo { SlotNumber = currentSlotNumber };
            slotInfoList.Add(newSlotInfo);
            
            // 3. UI 컴포넌트 가져오기
            // **중요**: 프리펩의 자식 오브젝트 이름이 아래와 같아야 합니다.
            // 슬롯 번호 텍스트 오브젝트 이름: "SlotNumberText"
            // 상태 텍스트 오브젝트 이름: "StatusText"
            TextMeshProUGUI slotNumberText = slotInstance.transform.Find("SlotNumberText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = slotInstance.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();

            // 4. UI 텍스트 업데이트
            // TODO: 나중에 실제 파일 존재 여부를 확인하여 IsEmpty 값을 설정해야 함
            if (newSlotInfo.IsEmpty)
            {
                if(slotNumberText != null) slotNumberText.text = $"SLOT {currentSlotNumber}";
                if(statusText != null) statusText.text = "- 비어있음 -";
            }
            else
            {
                if(slotNumberText != null) slotNumberText.text = $"SLOT {currentSlotNumber}";

                if(statusText != null) statusText.text = $"{newSlotInfo.StatusText}\n최근에 플레이한 날짜\n{newSlotInfo.RecentPlayDate}";
            }

            // 5. 버튼 클릭 이벤트에 리스너(함수) 동적 연결
            Button slotButton = slotInstance.GetComponent<Button>();
            if (slotButton != null)
            {
                // 람다식을 사용하여, 버튼이 클릭될 때 OnSlotClicked 함수가 '자신의 슬롯 번호'를 인자로 받도록 설정
                slotButton.onClick.AddListener(() => OnSlotClicked(currentSlotNumber));
            }
            else
            {
                Debug.LogError($"슬롯 프리펩({slotPrefab.name})에 Button 컴포넌트가 없습니다.");
            }
        }
    }


    /// <summary>
    /// 슬롯 버튼이 클릭되었을 때 호출되는 공용 메소드
    /// </summary>
    private void OnSlotClicked(int slotNumber)
    {
        // 리스트에서 클릭된 슬롯 번호에 해당하는 데이터를 찾음 (인덱스는 slotNumber - 1)
        SlotInfo selectedSlot = slotInfoList[slotNumber - 1];
        
        Debug.Log($"슬롯 {selectedSlot.SlotNumber}번이 선택되었습니다. (비어있음: {selectedSlot.IsEmpty})");

        if (selectedSlot.IsEmpty)
        {
            return;
        }
        else
        {
            loadButton.interactable = !lastSelectedSlot.IsEmpty;
        }
    }
    public void OnLoadButtonClicked()
    {
            Debug.Log($"슬롯 {lastSelectedSlot.SlotNumber}번 데이터를 불러옵니다.");
            SceneManager.LoadScene(gameScene);
        
    }
    /// <summary>
    /// 새 게임을 시작하고 게임 씬을 로드합니다.
    // private void StartNewGame()
    // {
    //     Debug.Log($"새 게임을 시작합니다.");
    //     SceneManager.LoadScene(gameScene);
    // }
}