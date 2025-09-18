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
    [SerializeField] private List<DropItem> dropTable; //아이템 이름과 개수

    [Header("나무 전용")]
    [SerializeField] private Sprite withoutFruitSprite;

    [SerializeField] private int cooldownDays = 1; //작물 종류마다 조정 필요. 

    public bool available = true;
    public bool IsAvailable => available;
    private int respawnDay;

    void Start()
    {
        dropItemPrefab = Resources.Load<GameObject>("DroppedItem");
    }

    /// <summary>
    /// 실제로 플레이어가 채집했을 때 호출.
    /// - available을 false로 전환
    /// - respawnDay 갱신
    /// - 타입별 외형 처리 (사라지거나, 열매 없는 스프라이트로 바뀜)
    /// </summary>
    public void Harvest(int currentDay) //작물 상태를 변경해줌. 
    {
        Debug.Log("Harvest 실행됨, sr=" + sr + " sprite=" + withoutFruitSprite);
        if (!available) return; // 이미 쿨타임 중이면 무시

        available = false;
        respawnDay = currentDay + cooldownDays;

        switch (type)
        {
            case InteractableType.Flower:
            case InteractableType.Root:
                // 꽃/뿌리: 오브젝트 자체가 사라진 것처럼 보임
                gameObject.SetActive(false); //
                break;

            case InteractableType.Tree:
                // 나무: 본체는 유지, 열매만 없는 스프라이트로 교체
                if (sr != null && withoutFruitSprite != null)
                {
                    sr.sprite = withoutFruitSprite;
                    originalSprite = withoutFruitSprite; // 하이라이트 해제 시 원래 스프라이트로 돌아갈 때를 위해 갱신
                }
                break;
        }
        DropItem();  // 아이템 드랍
    }

    /// <summary>
    /// 날짜가 바뀔 때마다 호출해서, respawnDay가 되면 다시 활성화/복구.
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
                    // 다시 등장
                    gameObject.SetActive(true);
                    break;

                case InteractableType.Tree:
                    // 열매 있는 상태로 복구
                    if (sr != null && originalSprite != null)
                        sr.sprite = originalSprite;
                    break;
            }
        }
    }

    /// <summary>
    /// 플레이어가 Interact 입력했을 때 호출되는 함수.
    /// PlayerHarvestController에 자신(Harvestable)을 넘겨서 수확 모드로 진입시킴.
    /// </summary>
    public override void Interact(PlayerHarvestController player)
    {
        player.EnterHarvestMode(this);
    }

    //dropitem 원래 playerharvestcontroller에 있었음.
    private void DropItem()
    {
        foreach (var drop in dropTable)
        {
            CreateDroppedItem(drop.itemName, drop.amount);
        }
    }

    private void CreateDroppedItem(string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0) return;

        for (int i = 0; i < amount; i++)
        {
            ItemData itemData = InventoryManager.Instance.GetItemDataByName(itemName);

            GameObject drop = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            drop.GetComponent<DroppedItem>().Initialize(itemData);
        }

        Debug.Log($"Dropped: {itemName} x{amount}");
    }

}