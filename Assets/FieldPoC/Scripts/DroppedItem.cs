using UnityEngine;

public class DroppedItem : Interactable
{
    public ItemData itemData;
    public int amount;

    public override void Interact(PlayerHarvestController player)
    {
        InventoryManager.Instance.AddItem(itemData, amount);
        Destroy(gameObject);
        Debug.Log($"Picked up {itemData.itemName} x{amount}");
    }

    // public void Pickup()
    // {
    //     InventoryManager.Instance.AddItem(itemData, amount);
    //     Destroy(gameObject);
    //     Debug.Log($"Picked up {itemData.itemName} x{amount}");
    // }
}
