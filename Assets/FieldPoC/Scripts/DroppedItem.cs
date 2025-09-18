using UnityEngine;

public class DroppedItem : Interactable
{
    private ItemData itemData;
    private SpriteRenderer spriteRenderer;
    // public int amount;

    public void Initialize(ItemData data)
    {
        itemData = data;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = data.itemIcon;
        // TODO: 드랍 애니메이션
    }

    public override void Interact(PlayerHarvestController player)
    {
        InventoryManager.Instance.AddItem(itemData, 1);
        Destroy(gameObject);
        Debug.Log($"Picked up {itemData.itemName}");
    }

    // public void Pickup()
    // {
    //     InventoryManager.Instance.AddItem(itemData, amount);
    //     Destroy(gameObject);
    //     Debug.Log($"Picked up {itemData.itemName} x{amount}");
    // }
}
