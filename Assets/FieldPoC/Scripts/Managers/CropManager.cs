using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropGroup
{
    public string cropName;
    public int maxActive;
    public GameObject prefab;
    public List<Transform> spawnPoints;

    [HideInInspector] public List<Harvestable> slots = new List<Harvestable>();
}

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 혹시 중복 생기면 제거
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [SerializeField] private List<CropGroup> groups;

    void Start()
    {
        int currentDay = GameManager.Instance.GetDate();

        foreach (var g in groups)
        {
            var gData = FieldDataManager.Instance.GetGroupData(g.cropName);

            // 첫날 → 랜덤으로 maxActive 개수 생성
            if (currentDay == 1)
            {
                int spawnCount = g.maxActive; // 생성할 개수

                // 랜덤 스폰 순서 만들기 (Fisher–Yates shuffle)
                // 1) 스폰 포인트 인덱스 목록 생성
                List<int> spawnIndices = new List<int>();
                for (int pointIndex = 0; pointIndex < g.spawnPoints.Count; pointIndex++)
                    spawnIndices.Add(pointIndex);

                // 2) 꼬리부터 하나씩 무작위 위치와 스왑
                for (int last = spawnIndices.Count - 1; last >= 0; last--)
                {
                    int randomIndex = Random.Range(0, last + 1);
                    (spawnIndices[last], spawnIndices[randomIndex]) = (spawnIndices[randomIndex], spawnIndices[last]);
                }

                // maxActive 개수만큼 생성
                for (int activeIndex = 0; activeIndex < g.maxActive; activeIndex++)
                {
                    int spawnPointIndex = spawnIndices[activeIndex];
                    GameObject cropObj = Instantiate(g.prefab, g.spawnPoints[spawnPointIndex].position, Quaternion.identity);
                    Harvestable harvestable = cropObj.GetComponent<Harvestable>();

                    harvestable.SetRespawnDay(-1);
                    g.slots.Add(harvestable);

                    gData.slots.Add(new CropSlotData
                    {
                        spawnPointIndex = spawnPointIndex,
                        isAlive = true,
                        respawnDay = -1 //살아있음. 
                    });
                }

                // 나머지 슬롯은 비어 있음(null)
                for (int emptyIndex = g.maxActive; emptyIndex < g.spawnPoints.Count; emptyIndex++)
                {
                    int spawnPointIndex = spawnIndices[emptyIndex];
                    g.slots.Add(null);
                    gData.slots.Add(new CropSlotData
                    {
                        spawnPointIndex = spawnPointIndex,
                        isAlive = false,
                        respawnDay = 0
                    });
                }
            }
            else
            {
                // ★ 첫날 이후에는 데이터 기반 복구
                for (int i = 0; i < g.spawnPoints.Count; i++)
                {
                    CropSlotData sData = (i < gData.slots.Count) ? gData.slots[i] : null;

                    //비어있는 자리는 일단 초기화
                    if (sData == null)
                    {
                        gData.slots.Add(new CropSlotData
                        {
                            spawnPointIndex = i,
                            isAlive = false,
                            respawnDay = 0
                        });
                        g.slots.Add(null);
                        continue;
                    }

                    // 1) 살아있는 슬롯 → 즉시 복구
                    if (sData.isAlive)
                    {
                        GameObject obj = Instantiate(g.prefab, g.spawnPoints[i].position, Quaternion.identity);
                        Harvestable h = obj.GetComponent<Harvestable>();
                        h.SetRespawnDay(sData.respawnDay); //
                        g.slots.Add(h);

                    }
                    // 2) 죽어있는데 쿨타임 끝남 → 리스폰
                    else if (!sData.isAlive && currentDay >= sData.respawnDay)
                    { //랜덤 로직 다시 넣기

                        GameObject obj = Instantiate(g.prefab, g.spawnPoints[i].position, Quaternion.identity);
                        Harvestable h = obj.GetComponent<Harvestable>();
                        h.SetRespawnDay(-1); // 살아있는 상태
                        g.slots.Add(h);

                        sData.isAlive = true;
                        sData.respawnDay = -1; // “살아있다” 표시
                        gData.slots[i] = sData;
                    }
                    // 3) 죽어있고 아직 쿨타임 중 → 비워둠
                    else
                    {
                        g.slots.Add(null);
                    }
                }
            }
        }
    }

    // CropManager.cs (class CropManager 내부에 추가)
    void OnEnable()
    {
        // 씬 활성화 시 날짜 이벤트 구독
        if (GameManager.Instance != null)
            GameManager.Instance.onDayChanged += OnDayChanged;
    }

    void OnDisable()
    {
        // 비활성화/파괴 시 구독 해제(중복 호출/누수 방지)
        if (GameManager.Instance != null)
            GameManager.Instance.onDayChanged -= OnDayChanged;
    }

    private void OnDayChanged()
    {
        int currentDay = GameManager.Instance.GetDate();

        // ▼ 여기부터는 네가 Start()의 “첫날 이후” 분기에서 돌리던 복구/리스폰 루프를 그대로 넣으면 됨.
        //   (gData 읽고, sData.isAlive면 복구, 아니고 currentDay >= sData.respawnDay면 리스폰, 아니면 null 유지)
        foreach (var g in groups)
        {
            var gData = FieldDataManager.Instance.GetGroupData(g.cropName);

            // g.slots가 비어있다면 길이 맞춰 초기화(인덱스 꼬임 방지)
            if (g.slots.Count == 0)
                for (int i = 0; i < g.spawnPoints.Count; i++) g.slots.Add(null);

            for (int i = 0; i < g.spawnPoints.Count; i++)
            {
                CropSlotData sData = (i < gData.slots.Count) ? gData.slots[i] : null;
                if (sData == null) continue;

                if (sData.isAlive)
                {
                    if (g.slots[i] == null)
                    {
                        var obj = Instantiate(g.prefab, g.spawnPoints[i].position, Quaternion.identity);
                        var h = obj.GetComponent<Harvestable>();
                        h.SetRespawnDay(sData.respawnDay);
                        g.slots[i] = h;    // ★ Add 말고 인덱스 대입
                    }
                }
                else if (currentDay >= sData.respawnDay)
                {
                    if (g.slots[i] == null)
                    {
                        var obj = Instantiate(g.prefab, g.spawnPoints[i].position, Quaternion.identity);
                        var h = obj.GetComponent<Harvestable>();
                        h.SetRespawnDay(-1);
                        g.slots[i] = h;    // ★ Add 말고 인덱스 대입

                        sData.isAlive = true;
                        sData.respawnDay = -1;
                        gData.slots[i] = sData;
                    }
                }
                // else: 쿨다운 중 → 그대로 null 유지
            }
        }
    }


    //작물이 수확된 후 슬롯 상태를 비우고, 리스폰 예정일을 기록한다
    public void UpdateSlotData(Harvestable h, int respawnDay)
    {
        foreach (var g in groups)
        {
            for (int i = 0; i < g.slots.Count; i++)
            {
                if (g.slots[i] == h)
                {
                    var gData = FieldDataManager.Instance.GetGroupData(g.cropName);
                    if (i >= gData.slots.Count)
                        gData.slots.Add(new CropSlotData());

                    gData.slots[i].spawnPointIndex = i;
                    gData.slots[i].isAlive = false;
                    gData.slots[i].respawnDay = respawnDay;

                    g.slots[i] = null; // 즉시 비워둠
                    return;
                }
            }
        }
    }
}
