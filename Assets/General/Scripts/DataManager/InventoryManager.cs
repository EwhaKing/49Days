using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


/// <summary>
/// 인벤토리의 모든 데이터를 관리하는 싱글톤 클래스.
/// 아이템 추가, 삭제, 교환 및 데이터 저장/불러오기 기능을 담당함.
/// </summary>
public class InventoryManager : SceneSingleton<InventoryManager>
{
    private List<ItemData> allItemData; // 모든 아이템 데이터를 에셋에서 불러와 저장하는 리스트

    public ItemData GetItemDataByName(string itemName)
    {
        return allItemData.Find(item => item.itemName == itemName);
    }

    /// <summary>
    /// 인벤토리 한 칸(슬롯)에 해당하는 저장 데이터.
    /// </summary>
    [System.Serializable]
    public class InventorySlotData
    {
        public ItemData itemData;   // 무슨 아이템인가 - ScriptableObject
        public int count;           // 아이템 개수
        public InventorySlotData(ItemData data, int amount)
        {
            itemData = data;
            count = amount;
        }
    }


    /// <summary>
    /// JSON 저장 - 인벤토리 데이터를 변환할 때 사용하는 직렬화 가능 클래스.
    /// </summary>
    [System.Serializable]
    public class SerializableCategory
    {
        public ItemType category;
        public InventorySlotData[] slots;
    }



    [System.Serializable]
    public class SerializableInventory
    {
        public List<SerializableCategory> allInventories = new List<SerializableCategory>();
    }

    /// <summary>
    /// 테스트용 - Unity 인스펙터에서 테스트용 아이템을 설정할 수 있게 한다!!!
    /// </summary>
    [System.Serializable]
    public class TestCategorySetup
    {
        public ItemType category;
        public ItemData[] items = new ItemData[MAX_SLOTS];
    }

    private const int MAX_SLOTS = 12;   // 인벤토리 슬롯 최대 개수

    // 실제 모든 인벤토리 데이터를 저장하는 Dictionary
    // Key : 아이템 종류(Enum ItemType) / Value : 해당 분류의 12칸짜리 인벤토리 배열
    private Dictionary<ItemType, InventorySlotData[]> inventories = new Dictionary<ItemType, InventorySlotData[]>();
    public event Action OnInventoryChanged; // 인벤토리에 변경이 생겼을 때 UI를 업데이트하도록 알림.

    [Header("테스트용 아이템 설정")]
    [Tooltip("저장된 데이터가 없을 경우에만 이 아이템들이 인벤토리에 추가됩니다.")]
    [SerializeField] private List<TestCategorySetup> testInventoriesSetup;

    private bool hasLoadedData = false; // 데이터 로드 성공 여부를 저장하는 변수

    protected override void Awake()
    {
        base.Awake();
        // 모든 아이템 분류에 대해 12칸짜리 빈 인벤토리 배열을 생성하고 초기화.
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            inventories[type] = new InventorySlotData[MAX_SLOTS];
        }
    }

    private void Start()
    {
        // Addressables를 통해 모든 ItemData 에셋을 비동기적으로 불러옴.
        LoadAllItemData();

        // 만약 OnEnable에서 데이터를 성공적으로 불러왔다면, 테스트 아이템을 추가하지 않음
        if (hasLoadedData) return;

        // 저장된 데이터가 없을 경우, 인스펙터에서 테스s트 아이템을 불러와서 인벤토리에 추가
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
        OnInventoryChanged?.Invoke();   // UI 업데이트
    }

    async void LoadAllItemData()
    {
        AsyncOperationHandle<IList<ItemData>> handle =
            Addressables.LoadAssetsAsync<ItemData>("itemdata", null); // label 기반 로드

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            allItemData = new List<ItemData>(handle.Result);
        }
        else
        {
            Debug.LogError("itemdata 그룹 로드 실패");
        }
    }

    private void OnEnable()
    {
        SaveLoadManager.Instance.onSave += SaveInventory;
        SaveLoadManager.Instance.onLoad += LoadInventory;
    }

    private void OnDisable()
    {
        SaveLoadManager.Instance.onSave -= SaveInventory;
        SaveLoadManager.Instance.onLoad -= LoadInventory;
    }

    private void SaveInventory()
    {
        // 현재 인벤토리 데이터를 직렬화 가능한 형태로 변환.
        var serializableInventory = new SerializableInventory();
        foreach (var inv in inventories)
        {
            serializableInventory.allInventories.Add(new SerializableCategory { category = inv.Key, slots = inv.Value });
        }
        // SaveLoadManager를 통해 JSON 파일로 저장.
        SaveLoadManager.Instance.Save<SerializableInventory>(serializableInventory);
        Debug.Log("Inventory Saved.");
    }

    private void LoadInventory()
    {
        var loadedInventory = SaveLoadManager.Instance.Load<SerializableInventory>();
        if (loadedInventory != null && loadedInventory.allInventories.Count > 0)
        {
            // 불러온 데이터를 다시 Dictionary 형태로 변환하고 인벤토리에 적용.
            inventories = loadedInventory.allInventories.ToDictionary(x => x.category, x => x.slots);
            hasLoadedData = true; // 로드 성공 ^___^
            OnInventoryChanged?.Invoke();
            Debug.Log("Inventory Loaded.");
        }
        else
        {
            hasLoadedData = false; // 로드 실패 (저장된 파일 없음) ㅠ___ㅜ
            Debug.Log("No saved inventory data found.");
        }
    }


    /// <summary>
    /// 인벤토리에 아이템 추가 - 빈 슬롯을 찾아 추가 or 기존 슬롯에 병합
    /// </summary>
    public void AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null) return;
        var targetInventory = inventories[itemToAdd.itemType];
        // 1) 병합 가능(겹치기 가능)
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (targetInventory[i] != null && targetInventory[i].itemData == itemToAdd && targetInventory[i].count < itemToAdd.maxStack)
            {
                targetInventory[i].count += amount;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        // 2) 병합 불가 - 첫 번째 빈 슬롯을 찾아 추가.
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


    /// <summary>
    /// 드래그 앤 드롭 - 두 아이템의 위치를 교환
    /// </summary>
    public void SwapItems(ItemType category, int indexA, int indexB)
    {
        var targetInventory = inventories[category];
        if (indexA < 0 || indexA >= MAX_SLOTS || indexB < 0 || indexB >= MAX_SLOTS || indexA == indexB) return;
        var temp = targetInventory[indexA];
        targetInventory[indexA] = targetInventory[indexB];
        targetInventory[indexB] = temp;
        OnInventoryChanged?.Invoke();
    }


    /// <summary>
    /// TrashBin - 특정 슬롯의 아이템을 인벤토리에서 완전히 삭제
    /// TODO : 필드에 버릴 때 데이터 넘겨주고 삭제할 것.
    /// </summary>
    public void RemoveItem(ItemType category, int index)
    {
        var targetInventory = inventories[category];
        if (index < 0 || index >= MAX_SLOTS || targetInventory[index] == null) return;
        targetInventory[index] = null;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// UI가 특정 카테고리의 인벤토리 데이터를 요청할 때 사용.
    /// </summary>
    public InventorySlotData[] GetInventory(ItemType category)
    {
        if (inventories.ContainsKey(category))
        {
            return inventories[category];
        }
        return null;
    }
}