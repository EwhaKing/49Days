using UnityEngine;

public class Harvestable : Interactable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;
    [SerializeField] private GameObject dropPrefab;

    public ItemData ItemData => itemData;
    public int Amount => amount;
    public GameObject DropPrefab => dropPrefab;

    public override void Interact(PlayerHarvestController player)
    {
        player.EnterHarvestMode(this);
    }
}
