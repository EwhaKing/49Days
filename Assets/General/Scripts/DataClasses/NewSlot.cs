using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
/// <summary>
/// [통합 스크립트] 저장 슬롯의 생성, UI 관리, 클릭 이벤트, 씬 로드 기능을 모두 담당합니다.
public class NewSaveSlotUI : MonoBehaviour
{
    [Header("프리펩 및 UI 설정")]
    [SerializeField] private GameObject newSlotPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [Tooltip("버튼과 두 개의 TextMeshProUGUI 자식 오브젝트를 가진 UI 프리펩")]
    [SerializeField] private GameObject slotPrefab;
    [Tooltip("슬롯 프리펩들이 생성되어 자식으로 들어갈 부모 UI 오브젝트")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private int numberOfSlots = 3;
    [Header("상호작용할 UI 버튼")]
    [Tooltip("슬롯이 선택되었을 때 활성화될 불러오기 버튼")]
    [SerializeField] private Button startButton;
    [Header("씬 설정")]
    [SerializeField] private Button exitButton;
    [SerializeField] private string gameScene = "Beginning";
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
        public SlotInfo()
        {
            RecentPlayDate = DateTime.Now.ToString("yyyy.MM.dd");
        }
    }
    void Awake()
    {
        // 씬 시작 시 슬롯을 초기화하고 생성합니다.
        InitializeSlots();

        if (startButton != null)
        {
            startButton.interactable = false;
            // 불러오기 버튼에 클릭 이벤트를 연결합니다.
            startButton.onClick.AddListener(OnStartClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }
    }
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
            TextMeshProUGUI slotNumberText = slotInstance.transform.Find("SlotNumberText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = slotInstance.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            // 4. UI 텍스트 업데이트
            if (newSlotInfo.IsEmpty)
            {
                if (statusText != null) statusText.text = "- 비어있음 -";
            }
            else
            {
                if (statusText != null) statusText.text = $"{newSlotInfo.StatusText}\n최근에 플레이한 날짜\n{newSlotInfo.RecentPlayDate}";
            }
            if (slotNumberText != null) slotNumberText.text = $"SLOT {currentSlotNumber}";
            // 5. 버튼 클릭 이벤트에 리스너(함수) 동적 연결
            Button slotButton = slotInstance.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() => OnSlotClicked(currentSlotNumber));
            }
            Button deleteButtonInPrefab = slotInstance.transform.Find("DeleteButton")?.GetComponent<Button>();
            //6. 삭제 버튼 이벤트 연결
            if (deleteButtonInPrefab != null)
            {
                deleteButtonInPrefab.onClick.AddListener(() => OnDeleteFromSlotClicked(currentSlotNumber));
            }
            else
            {
                Debug.LogWarning($"슬롯 프리펩({slotPrefab.name}) 내에 'DeleteButton'이라는 이름의 자식 버튼이 없습니다.");
            }
        }
    }
    /// 슬롯 버튼이 클릭되었을 때 호출되는 공용 메소드
    private void OnSlotClicked(int slotNumber)
    {
        lastSelectedSlot = slotInfoList[slotNumber - 1];
        Debug.Log($"슬롯 {lastSelectedSlot.SlotNumber}번이 선택되었습니다. (비어있음: {lastSelectedSlot.IsEmpty})");
        if (lastSelectedSlot.IsEmpty==true)
        startButton.interactable = lastSelectedSlot.IsEmpty;
    }
    ////////////////////////////////데이터 연결 여기다 하면 될 듯요//////////////////////////////////
    public void OnStartClicked()
    {
        Debug.Log($"슬롯 {lastSelectedSlot.SlotNumber}번 데이터를 불러옵니다.");
        SceneManager.LoadScene(gameScene);
    }
    //////////////////////////////////////////////////////////////////////////////////////////////
    private void OnDeleteFromSlotClicked(int slotNumber)
    {
        Debug.Log($"슬롯 {slotNumber}번의 데이터 삭제를 시도합니다.");
        // 1. 삭제할 슬롯의 정보를 가져옵니다.
        SlotInfo slotToDelete = slotInfoList[slotNumber - 1];
        // 2. 슬롯이 이미 비어있다면 아무것도 하지 않고 함수를 종료합니다.
        if (slotToDelete.IsEmpty)
        {
            Debug.Log($"슬롯 {slotNumber}번은 이미 비어있습니다.");
            return;
        }
        // 3. 메모리상의 데이터를 초기화합니다.
        slotToDelete.IsEmpty = true;
        slotToDelete.StatusText = "";
        slotToDelete.RecentPlayDate = "";
        // 4. 전체 UI를 새로고침하여 변경사항을 즉시 반영합니다.
        InitializeSlots();
    }
    private void OnExitClicked()
    {
        newSlotPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Debug.Log("메인 메뉴로 돌아갑니다.");
    }
}