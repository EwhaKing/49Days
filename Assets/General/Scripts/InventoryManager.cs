using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : SceneSingleton<InventoryManager>
{
    [System.Serializable]
    public class InventorySlotData
    {
        public ItemData itemData;
        public int count;
        public InventorySlotData(ItemData data, int amount)
        {
            itemData = data;
            count = amount;
        }
    }
    
    [System.Serializable]
    public class TestCategorySetup
    {
        public ItemType category;
        public ItemData[] items = new ItemData[MAX_SLOTS];
    }

    private const int MAX_SLOTS = 12;
    private Dictionary<ItemType, InventorySlotData[]> inventories = new Dictionary<ItemType, InventorySlotData[]>();

    public event Action OnInventoryChanged;

    [Header("테스트용 아이템 설정")]
    [SerializeField] private List<TestCategorySetup> testInventoriesSetup;

    protected override void Awake()
    {
        base.Awake();
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            inventories[type] = new InventorySlotData[MAX_SLOTS];
        }
    }

    void Start()
    {
        if (testInventoriesSetup != null)
        {
            foreach (var setup in testInventoriesSetup)
            {
                var targetInventory = inventories[setup.category];
                for (int i = 0; i < setup.items.Length && i < MAX_SLOTS; i++)
                {
                    var item = setup.items[i];
                    if (item != null)
                    {
                        int amount = (setup.category == ItemType.Ingredient) ? 5 : 1;
                        targetInventory[i] = new InventorySlotData(item, amount);
                    }
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }
    
    // [추가] 지정된 슬롯의 아이템을 인벤토리에서 삭제하는 함수
    public void RemoveItem(ItemType category, int index)
    {
        var targetInventory = inventories[category];

        if (index < 0 || index >= MAX_SLOTS || targetInventory[index] == null)
            return;

        targetInventory[index] = null; // 해당 슬롯 데이터를 null로 만들어 아이템을 삭제
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null) return;
        var targetInventory = inventories[itemToAdd.itemType];
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (targetInventory[i] != null && targetInventory[i].itemData == itemToAdd && targetInventory[i].count < itemToAdd.maxStack)
            {
                targetInventory[i].count += amount;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (targetInventory[i] == null)
            {
                targetInventory[i] = new InventorySlotData(itemToAdd, amount);
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void SwapItems(ItemType category, int indexA, int indexB)
    {
        var targetInventory = inventories[category];
        if (indexA < 0 || indexA >= MAX_SLOTS || indexB < 0 || indexB >= MAX_SLOTS || indexA == indexB)
            return;
        var temp = targetInventory[indexA];
        targetInventory[indexA] = targetInventory[indexB];
        targetInventory[indexB] = temp;
        OnInventoryChanged?.Invoke();
    }

    public InventorySlotData[] GetInventory(ItemType category)
    {
        if (inventories.ContainsKey(category))
        {
            return inventories[category];
        }
        return null;
    }
}
