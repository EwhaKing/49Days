using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

/// <summary>
/// 인벤토리 UI의 모든 Visual Element와 Interaction을 관리하는 클래스.
/// InventoryManager로부터 데이터를 받아와 UI에 표시함.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI 요소 연결")]
    [SerializeField] private Transform slotGrid;                    // 슬롯들이 배치될 부모 오브젝트(SlotGrid)의 Transform
    [SerializeField] private Image itemInfoImage;                   // 아이템 정보 패널 - 아이템 이미지
    [SerializeField] private TextMeshProUGUI itemNameText;          // 아이템 정보 패널 - 아이템 이름
    [SerializeField] private TextMeshProUGUI itemDescriptionText;   // 아이템 정보 패널 - 설명
    [SerializeField] private GameObject itemInfoPanel;              // 아이템 정보 패널 전체 (On/Off 제어용)
    [SerializeField] private Toggle[] categoryToggles;              // 카테고리 필터링 토글 버튼들
    [SerializeField] private Image dragIcon;                        // 드래그 시 마우스를 따라다닐 아이콘
    [SerializeField] private CanvasGroup categoryTogglesGroup;      // 드래그 시 상호작용을 막을 토글 그룹

    [Header("씬별 특별 설정")]
    [SerializeField] private string kitchenSceneName = "Kitchen";   // 주방 씬에서 토글 비활성화 해야 하니까 이름 넣어둡니다.
    [SerializeField] private string frontSceneName = "TeaHouseFront"; // 다과회 전경 씬 이름


    [SerializeField] private Color disabledColor = new Color(0.5f, 0.2f, 0.2f, 1f);     // 비활성화된 토글의 색상
    [SerializeField] private int kitchenDefaultCategoryIndex = 2;   // 주방 씬에서의 기본 카테고리 인덱스 (2가 도구, 3이 퀘스트, 4가 기타)

    [SerializeField]private GameObject droppedItemPrefab;
    [SerializeField]private Transform playerTransform;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private bool isDragging = false;
    private ItemType currentCategory = ItemType.Ingredient;
    private int draggedSlotIndex = -1;
    private bool dropSuccessful = false;
    private List<Color> originalToggleColors = new List<Color>();

    private void Awake()
    {
        droppedItemPrefab = Resources.Load<GameObject>("DroppedItem");
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 주방에서 토글 비활성화(색상 변경)하기 전 원래 색상을 저장.
        foreach (var toggle in categoryToggles)
        {
            var image = toggle.targetGraphic as Image;
            if (image != null) originalToggleColors.Add(image.color);
        }
    }

    private void Start()
    {
        // TrashBin 이벤트를 구독 - 아이템이 버려졌을 때를 감지.
        TrashBin.OnItemDroppedOnTrash += HandleTrashDrop;
        ItemDropZone.OnItemDroppedOnBackground += HandleTrashDrop;

        // 자식으로 있는 모든 슬롯 UI를 찾아와 초기화. 그리고 이벤트 연결.
        slotGrid.GetComponentsInChildren(true, uiSlots);
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].Init(i);
            uiSlots[i].OnSlotClicked += ShowItemInfo;
            uiSlots[i].OnBeginDragSlot += StartDrag;
            uiSlots[i].OnDropOnSlot += OnDrop;
            uiSlots[i].OnEndDragSlot += EndDrag;
        }

        // 카테고리 토글 버튼들에 리스너를 추가.
        for (int i = 0; i < categoryToggles.Length; i++)
        {
            int index = i;
            categoryToggles[i].onValueChanged.AddListener((isOn) => { if (isOn) SetCategory((ItemType)index); });
        }

        // 현재 씬에 따라 토글 상태를 업데이트 -> 기본 카테고리를 설정.
        bool isTeaHouseScene = SceneManager.GetActiveScene().name == frontSceneName || SceneManager.GetActiveScene().name == kitchenSceneName ? true : false;
        UpdateCategoryTogglesForScene(isTeaHouseScene);
        ItemType defaultCategory = isTeaHouseScene ? (ItemType)kitchenDefaultCategoryIndex : ItemType.Ingredient;
        categoryToggles[(int)defaultCategory].isOn = true;

        // InventoryManager의 데이터 변경 이벤트 구독.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateInventoryDisplay;
        }

        // 초기 UI 상태 설정.
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
        if (dragIcon != null)
        {
            dragIcon.raycastTarget = false;
            dragIcon.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 드래그 중일 때, dragIcon이 마우스 위치를 따라다니도록 조정.
        if (isDragging && dragIcon != null)
        {
            dragIcon.transform.position = Mouse.current.position.ReadValue();
        }
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시, 구독했던 모든 이벤트를 해제.
        TrashBin.OnItemDroppedOnTrash -= HandleTrashDrop;
        ItemDropZone.OnItemDroppedOnBackground -= HandleTrashDrop;
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryDisplay;
        }
    }

    /// <summary>
    /// InventoryManager의 데이터가 변경될 때마다 호출 -> 화면 새로고침.
    /// </summary>
    public void UpdateInventoryDisplay()
    {
        var inventory = InventoryManager.Instance.GetInventory(currentCategory);
        if (inventory == null) return;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].gameObject.SetActive(true);
            if (i < inventory.Length && inventory[i] != null)
            {
                uiSlots[i].UpdateSlot(inventory[i]);
            }
            else
            {
                uiSlots[i].ClearSlot();
            }
        }
    }

    #region 이벤트 핸들러
    /// <summary>
    /// 슬롯을 클릭했을 때 호출 -> 아이템 정보 패널 표시.
    /// </summary>
    private void ShowItemInfo(int slotIndex)
    {
        var inv = InventoryManager.Instance.GetInventory(currentCategory);
        if (inv == null || slotIndex >= inv.Length) return;
        var slotData = inv[slotIndex];
        if (slotData == null || slotData.itemData == null)
        {
            if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
            return;
        }
        if (itemInfoPanel == null || itemInfoImage == null || itemNameText == null || itemDescriptionText == null) return;

        itemInfoImage.sprite = slotData.itemData.itemIcon;
        itemNameText.text = slotData.itemData.itemName;
        itemDescriptionText.text = slotData.itemData.itemDescription;
        itemInfoPanel.SetActive(true);
    }

    /// <summary>
    /// 아이템을 쓰레기통에 버렸을 때 호출.
    /// 드래그 중인 아이템이 있다면 해당 아이템을 인벤토리에서 삭제.
    /// </summary>
    private void HandleTrashDrop()
    {
        if (isDragging)
        {
            var inventory = InventoryManager.Instance.GetInventory(currentCategory);
            if (inventory == null || draggedSlotIndex < 0 || draggedSlotIndex >= inventory.Length || inventory[draggedSlotIndex] == null) return;
            var draggedItemData = inventory[draggedSlotIndex];
            
            if (draggedItemData.itemData.itemType == ItemType.Ingredient)
            {
                if (GameFlowManager.IsInField())
                {
                    SpawnDroppedItem(draggedItemData.itemData, draggedItemData.count);
                }
                InventoryManager.Instance.RemoveItem(currentCategory, draggedSlotIndex);
                dropSuccessful = true;
            }
            else
            {
                Debug.Log("아이템 타입이 재료가 아니므로 버릴 수 없습니다.");
            }

        }
    }

    /// <summary>
    /// 카테고리 탭을 변경했을 때 호출.
    /// 현재 카테고리를 변경하고, UI를 업데이트.
    /// 씬이 "Kitchen"일 때는 특정 카테고리 토글을 비활성화.
    /// </summary>
    private void SetCategory(ItemType newCategory)
    {
        currentCategory = newCategory;
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
        UpdateInventoryDisplay();
    }

    /// <summary>
    /// 주방에서 일부 카테고리 토글을 비활성화.
    /// </summary>
    private void UpdateCategoryTogglesForScene(bool isTeaHouseScene)
    {
        for (int i = 0; i < categoryToggles.Length; i++)
        {
            Toggle toggle = categoryToggles[i];
            Image image = toggle.targetGraphic as Image;
            bool shouldDisable = isTeaHouseScene && (i == (int)ItemType.Ingredient);
            toggle.interactable = !shouldDisable;
            if (image != null && i < originalToggleColors.Count)
            {
                image.color = shouldDisable ? disabledColor : originalToggleColors[i];
            }
        }
    }
    #endregion

    #region 드래그 앤 드롭
    /// <summary>
    /// 슬롯에서 드래그를 시작했을 때 호출.
    /// </summary>
    public void StartDrag(int slotIndex)
    {
        var inv = InventoryManager.Instance.GetInventory(currentCategory);
        if (inv == null || slotIndex < 0 || slotIndex >= inv.Length || inv[slotIndex] == null) return;

        dropSuccessful = false;
        draggedSlotIndex = slotIndex;
        isDragging = true;

        if (dragIcon != null)
        {
            dragIcon.sprite = inv[slotIndex].itemData.itemIcon;
            dragIcon.gameObject.SetActive(true);
        }
        if (categoryTogglesGroup != null) categoryTogglesGroup.blocksRaycasts = false;

        uiSlots[slotIndex].ClearSlot();
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
    }

    /// <summary>
    /// 드래그를 마쳤을 때 호출 - 마우스 버튼을 뗐을 때.
    /// 드롭이 성공하지 않았다면 아이템을 원래 자리로 되돌림.
    /// </summary>
    public void EndDrag()
    {
        // 만약 드롭이 성공하지 않았다면/허공에 드롭하면, 아이템을 원래 자리로 되돌림.
        if (!dropSuccessful)
        {
            UpdateInventoryDisplay();
        }

        isDragging = false;
        draggedSlotIndex = -1;

        if (dragIcon != null) dragIcon.gameObject.SetActive(false);
        if (categoryTogglesGroup != null) categoryTogglesGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 다른 슬롯 위에 드롭했을 때 호출.
    /// 드래그 중인 아이템이 있다면, 해당 슬롯과의 위치를 교환.
    /// </summary>
    public void OnDrop(int dropSlotIndex)
    {
        if (draggedSlotIndex != -1)
        {
            InventoryManager.Instance.SwapItems(currentCategory, draggedSlotIndex, dropSlotIndex);
            dropSuccessful = true;
        }
    }
    
    private void SpawnDroppedItem(ItemData itemData, int amount)
    {
        // if (droppedItemPrefab == null || playerTransform == null)
        // {
        //     Debug.LogError("DroppedItem Prefab 또는 Player Transform이 InventoryUI에 할당되지 않았습니다.");
        //     return;
        // }
        Vector3 spawnPosition = playerTransform.position; 
        GameObject drop = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
        drop.GetComponent<DroppedItem>().Initialize(itemData, amount);

        Debug.Log($"필드에 드랍된 아이템: {itemData.itemName} x{amount}");
    }
    #endregion
}
