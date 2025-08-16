using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 관리")]
    [SerializeField] private Transform slotGrid;
    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    [Header("아이템 정보 패널")]
    [SerializeField] private Image itemInfoImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private GameObject itemInfoPanel;

    [Header("필터링")]
    [SerializeField] private Toggle[] categoryToggles;

    [Header("드래그 앤 드롭")]
    [SerializeField] private Image dragIcon;
    [SerializeField] private CanvasGroup categoryTogglesGroup;
    
    [Header("씬별 설정")]
    [SerializeField] private string kitchenSceneName = "Kitchen";
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.2f, 0.2f, 1f);
    [SerializeField] private int kitchenDefaultCategoryIndex = 3; 

    private bool isDragging = false;
    private ItemType currentCategory = ItemType.Ingredient;
    private int draggedSlotIndex = -1;
    private List<Color> originalToggleColors = new List<Color>();
    private bool dropSuccessful = false; // [추가] 드롭이 성공적으로 처리되었는지 확인하는 변수

    void Awake()
    {
        foreach (var toggle in categoryToggles)
        {
            var image = toggle.targetGraphic as Image;
            if (image != null) originalToggleColors.Add(image.color);
        }
    }

    void Start()
    {
        // [수정] 쓰레기통 이벤트를 구독
        TrashBin.OnItemDroppedOnTrash += HandleTrashDrop;

        slotGrid.GetComponentsInChildren<InventorySlotUI>(true, uiSlots);
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].Init(i);
            uiSlots[i].OnSlotClicked += ShowItemInfo;
            uiSlots[i].OnBeginDragSlot += StartDrag;
            uiSlots[i].OnDropOnSlot += OnDrop;
            uiSlots[i].OnEndDragSlot += EndDrag;
        }

        for (int i = 0; i < categoryToggles.Length; i++)
        {
            int index = i;
            categoryToggles[i].onValueChanged.AddListener((isOn) => { if (isOn) SetCategory((ItemType)index); });
        }

        bool isKitchen = SceneManager.GetActiveScene().name == kitchenSceneName;
        UpdateCategoryTogglesForScene(isKitchen);
        ItemType defaultCategory = isKitchen ? (ItemType)kitchenDefaultCategoryIndex : ItemType.Ingredient;
        categoryToggles[(int)defaultCategory].isOn = true;
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateInventoryDisplay;
        }

        if (itemInfoPanel != null) itemInfoPanel.SetActive(false);
        if (dragIcon != null)
        {
            dragIcon.raycastTarget = false;
            dragIcon.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isDragging && dragIcon != null)
        {
            dragIcon.transform.position = Mouse.current.position.ReadValue();
        }
    }
    
    private void OnDestroy()
    {
        // [수정] 쓰레기통 이벤트 구독 해제
        TrashBin.OnItemDroppedOnTrash -= HandleTrashDrop;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryDisplay;
        }
    }

    // [추가] 쓰레기통에 아이템이 드롭되었을 때 호출될 함수
    private void HandleTrashDrop()
    {
        if (isDragging)
        {
            InventoryManager.Instance.RemoveItem(currentCategory, draggedSlotIndex);
            dropSuccessful = true; // 드롭 성공으로 표시
        }
    }

    public void StartDrag(int slotIndex)
    {
        var inv = InventoryManager.Instance.GetInventory(currentCategory);
        if (inv == null || slotIndex < 0 || slotIndex >= inv.Length || inv[slotIndex] == null) return;
        
        dropSuccessful = false; // [추가] 드래그 시작 시 드롭 상태 초기화
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

    public void EndDrag()
    {
        // [수정] 드롭이 성공하지 않았다면(허공에 드롭했다면) 아이템이 원래대로 돌아가도록 UI만 새로고침
        if (!dropSuccessful)
        {
            UpdateInventoryDisplay();
        }

        isDragging = false;
        draggedSlotIndex = -1;

        if (dragIcon != null) dragIcon.gameObject.SetActive(false);
        if (categoryTogglesGroup != null) categoryTogglesGroup.blocksRaycasts = true;
    }

    public void OnDrop(int dropSlotIndex)
    {
        if (draggedSlotIndex != -1)
        {
            InventoryManager.Instance.SwapItems(currentCategory, draggedSlotIndex, dropSlotIndex);
            dropSuccessful = true; // [추가] 슬롯 간 교환도 성공으로 표시
        }
    }

    // --- 이하 함수들은 변경 없음 ---

    private void UpdateCategoryTogglesForScene(bool isKitchen)
    {
        for (int i = 0; i < categoryToggles.Length; i++)
        {
            Toggle toggle = categoryToggles[i];
            Image image = toggle.targetGraphic as Image;
            bool shouldDisable = isKitchen && (i == (int)ItemType.Ingredient || i == (int)ItemType.Topping);
            toggle.interactable = !shouldDisable;
            if (image != null && i < originalToggleColors.Count)
            {
                image.color = shouldDisable ? disabledColor : originalToggleColors[i];
            }
        }
    }

    private void UpdateInventoryDisplay()
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
    
    private void SetCategory(ItemType newCategory)
    {
        currentCategory = newCategory;
        if (itemInfoPanel != null) itemInfoPanel.SetActive(false); 
        UpdateInventoryDisplay();
    }
    
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
}
