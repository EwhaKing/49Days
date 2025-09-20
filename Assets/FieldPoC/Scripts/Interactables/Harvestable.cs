using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public string itemName;
    public int amount;
}

public class Harvestable : Interactable
{
    private GameObject dropItemPrefab;

    [Header("드랍될 아이템 설정")]
    [SerializeField] private List<DropItem> dropTable;

    [Header("쿨타임 설정")]
    public int cooldownDays = 1; // 작물별 리스폰 쿨타임

    [Header("나무 전용")]
    [SerializeField] private Sprite withoutFruitSprite; // 열매 없는 상태 스프라이트

    private bool available = true;
    public bool IsAvailable => available;

    private int respawnDay;
    public int RespawnDay => respawnDay;

    void Start()
    {
        dropItemPrefab = Resources.Load<GameObject>("DroppedItem");
    }

    /// <summary>
    /// 플레이어가 실제로 채집했을 때 호출.
    /// - 상태 변경
    /// - respawnDay 갱신
    /// - 외형 처리 (Destroy / Sprite 교체)
    /// </summary>
    public void Harvest(int currentDay)
    {
        if (!available) return; // 이미 수확 불가 상태라면 무시

        available = false;
        respawnDay = currentDay + cooldownDays;

        // 슬롯 데이터 갱신 (Destroy 전에!)
        CropManager.Instance.UpdateSlotData(this, respawnDay);


        switch (type)
        {
            case InteractableType.Flower:
            case InteractableType.Root:
                // 꽃/뿌리 → 오브젝트 자체 제거
                Destroy(gameObject);
                break;

            case InteractableType.Tree:
                // 나무 → 열매 없는 스프라이트로 교체
                if (sr != null && withoutFruitSprite != null)
                {
                    sr.sprite = withoutFruitSprite;
                    originalSprite = withoutFruitSprite; // 하이라이트 해제 시 원래 스프라이트를 위해 갱신
                }
                break;
        }

        DropItem();
    }

    /// <summary>
    /// 날짜가 바뀔 때마다 호출해서 리스폰 조건 확인.
    /// </summary>
    public void CheckRespawn(int currentDay)
    {
        if (!available && currentDay >= respawnDay)
        {
            available = true;

            switch (type)
            {
                case InteractableType.Flower:
                case InteractableType.Root:
                    // Flower/Root는 Destroy되었으므로 여기서는 아무것도 안 함.
                    // CropManager.OnDayChanged()에서 Instantiate 처리.
                    break;

                case InteractableType.Tree:
                    // Tree → 열매 있는 스프라이트로 복구
                    if (sr != null && originalSprite != null)
                        sr.sprite = originalSprite;
                    break;
            }
        }
    }

    /// <summary>
    /// 플레이어 상호작용 → PlayerHarvestController에게 자신 전달.
    /// </summary>
    public override void Interact(PlayerHarvestController player)
    {
        player.EnterHarvestMode(this);
    }

    // === 아이템 드랍 ===
    private void DropItem()
    {
        foreach (var drop in dropTable)
            CreateDroppedItem(drop.itemName, drop.amount);
    }

    private void CreateDroppedItem(string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            ItemData itemData = InventoryManager.Instance.GetItemDataByName(itemName);
            GameObject drop = Instantiate(dropItemPrefab, transform.position + Vector3.up, Quaternion.identity);
            drop.GetComponent<DroppedItem>().Initialize(itemData);
        }

        Debug.Log($"Dropped: {itemName} x{amount}");
    }

    // 저장/로드용 respawnDay 세터
    public void SetRespawnDay(int day)
    {
        respawnDay = day;
    }
}
