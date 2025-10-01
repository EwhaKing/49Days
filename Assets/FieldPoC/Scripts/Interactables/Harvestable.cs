using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public int cooldownDays = 2; // 작물별 리스폰 쿨타임

    [Header("나무 전용")]
    [SerializeField] private Sprite fruitSprite;        // 열매 달린 상태
    [SerializeField] private Sprite withoutFruitSprite; // 열매 없는 상태

    private bool available = true;
    public bool IsAvailable => available;

    private int respawnDay;
    public int RespawnDay => respawnDay;

    [HideInInspector] public int spawnIndex = -1; // CropManager 슬롯 인덱스 기억
    public int slotDataIndex; // 쿨타임 데이터 인덱스 (필요 시)

    [Header("UI 아이콘 프리팹")]
    [SerializeField] private GameObject KeyIconPrefab;
    private List<KeyIcon> activeIcons = new List<KeyIcon>();

    [Header("아이콘 오프셋")]
    [SerializeField] private float verticalOffset = 2f;   // 위쪽 오프셋
    [SerializeField] private float treeSpacing = 0.5f;    // 좌우 간격

    //게이지 변수
    [SerializeField] private GameObject progressBarPrefab;
    private Slider progressBar;
    private int progressCount = 0;
    private int maxProgress = 0;

    void Start()
    {
        dropItemPrefab = Resources.Load<GameObject>("DroppedItem");
    }

    //icon 관련 함수들
    public void ShowEnterIcon()
    {
        ClearIcons();
        SpawnIcon('E', Vector3.up * verticalOffset);
    }

    private int highlightIndex = 0; // 현재 어떤 아이콘을 강조할지

    public void HighlightNextKey()
    {
        if (activeIcons.Count == 0) return;

        // 모든 아이콘 off
        foreach (var icon in activeIcons)
            icon.SetHighlight(false);

        // 현재 인덱스 on
        activeIcons[highlightIndex].SetHighlight(true);

        // 다음 턴 준비 (A → D → A … 번갈아가기)
        highlightIndex = (highlightIndex + 1) % activeIcons.Count;
    }


    public void ShowHarvestIcons()
    {
        ClearIcons();
        highlightIndex = 0; //초기화

        switch (Type)
        {
            case InteractableType.Tree:
                SpawnIcon('A', new Vector3(-treeSpacing, verticalOffset, 0));
                SpawnIcon('D', new Vector3(treeSpacing, verticalOffset, 0));
                break;
            case InteractableType.Root:
                SpawnIcon('W', Vector3.up * verticalOffset);
                break;
                // Flower는 따로 없음
        }
    }

    //[SerializeField] private GameObject KeyIconPrefab;
    private static Transform keyIconParent;

    void Awake()
    {
        base.Awake();  // 부모 초기화 먼저 실행
        if (keyIconParent == null)
        {
            GameObject canvasObj = GameObject.Find("KeyIconCanvas");
            if (canvasObj != null)
            {
                keyIconParent = canvasObj.transform;
            }
            else
            {
                Debug.LogError("씬에 KeyIconCanvas가 없습니다! World Space Canvas를 미리 배치하세요.");
            }
        }
    }

    private void SpawnIcon(char key, Vector3 offset)
    {
        if (keyIconParent == null) return;

        // Canvas 밑에 생성
        var obj = Instantiate(KeyIconPrefab, keyIconParent);
        obj.transform.position = transform.position + offset; // 월드 좌표 그대로

        var icon = obj.GetComponent<KeyIcon>();
        icon.SetKey(key);
        activeIcons.Add(icon);
    }


    public void ClearIcons()
    {
        foreach (var icon in activeIcons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        activeIcons.Clear();
    }

    //게이지 생성, 제거 함수
    public void SpawnProgressBar(int requiredPresses)
    {
        if (keyIconParent == null) return;

        GameObject obj = Instantiate(progressBarPrefab, keyIconParent);
        obj.transform.position = transform.position + new Vector3(0, verticalOffset - 1.0f, 0); // 아이콘보다 아래에 위치.

        progressBar = obj.GetComponent<Slider>();
        progressBar.value = 0;

        progressCount = 0;
        maxProgress = requiredPresses; // Tree=8, Root=5

        progressBar.value = 0f;
    }

    public void ClearProgressBar()
    {
        if (progressBar != null)
        {
            Destroy(progressBar.gameObject);
            progressBar = null;
        }
    }

    public void AddProgress()
    {
        if (progressBar == null) return;

        progressCount++;
        if (progressCount > maxProgress) progressCount = maxProgress;

        progressBar.value = (float)progressCount / maxProgress;
    }



    public void StartRootHighlight()
    {
        if (activeIcons.Count > 0)
            activeIcons[0].StartBlinkHighlight();
    }

    public void StopRootHighlight()
    {
        if (activeIcons.Count > 0)
            activeIcons[0].StopBlinkHighlight();
    }


    /// <summary>
    /// 플레이어가 실제로 채집했을 때 호출
    /// </summary>
    public void Harvest(int currentDay)
    {
        if (!available) return;

        available = false;
        respawnDay = currentDay + cooldownDays;

        // 슬롯 데이터 갱신
        CropManager.Instance.UpdateSlotData(this, respawnDay);

        switch (type)
        {
            case InteractableType.Flower:
            case InteractableType.Root:
                // 꽃/뿌리 → 오브젝트 제거
                Destroy(gameObject);
                break;

            case InteractableType.Tree:
                // 나무 → 열매 없는 스프라이트로 교체
                if (sr != null && withoutFruitSprite != null)
                {
                    sr.sprite = withoutFruitSprite;
                    originalSprite = withoutFruitSprite; // 하이라이트 복원용
                }
                break;
        }

        DropItem();
    }

    /// <summary>
    /// 날짜가 바뀔 때 리스폰 여부 체크
    /// </summary>
    public bool CheckRespawn(int currentDay)
    {
        if (!available && currentDay >= respawnDay)
        {
            available = true;

            if (type == InteractableType.Tree && sr != null && fruitSprite != null)
            {
                sr.sprite = fruitSprite;       // 열매 복구
                originalSprite = fruitSprite;  // 하이라이트 복구용
            }

            return true;
        }
        return false;
    }

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

    // 쿨타임 전용: 강제로 열매 없는 상태 세팅
    public void SetWithoutFruit()
    {
        if (sr != null && withoutFruitSprite != null)
        {
            sr.sprite = withoutFruitSprite;
            originalSprite = withoutFruitSprite;
        }
        available = false;
    }

    // 저장/로드용 respawnDay 세터
    public void SetRespawnDay(int day)
    {
        respawnDay = day;
    }
}
