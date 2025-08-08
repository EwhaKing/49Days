using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public int num = 1;
    public string name;
}

public class Inventory
{
    public List<ItemData> items;
}

public class InventoryManager : SceneSingleton<InventoryManager>
{
    Inventory inventory = new Inventory();

    public void SaveItem(ItemData itemData)
    {
        ItemData item = inventory.items.Find(it => it.name == itemData.name);
        if (item is not null)
        {
            item.num ++;
        }
        else
        {
            inventory.items.Add(itemData);
        }
    }

    public ItemData getItem(string name)
    {
        return inventory.items.Find(it => it.name == name);
    }

    private void OnEnable() 
    {
        SaveLoadManager.Instance.onSave += () => SaveLoadManager.Instance.Save<Inventory>(inventory);
        SaveLoadManager.Instance.onLoad += () => {inventory = SaveLoadManager.Instance.Load<Inventory>();};
    }

    private void OnDisable() 
    {
        SaveLoadManager.Instance.onSave -= () => SaveLoadManager.Instance.Save<Inventory>(inventory);
        SaveLoadManager.Instance.onLoad -= () => {inventory = SaveLoadManager.Instance.Load<Inventory>();};
    }
}
