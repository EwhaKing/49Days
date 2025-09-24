using System.Collections.Generic;
using UnityEngine;

public class CropManager : MonoBehaviour
{
    public static CropManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [System.Serializable]
    public class CropGroup
    {
        public string cropName;
        public int maxActive;
        public GameObject prefab;
        public List<Transform> spawnPoints;

        [HideInInspector] public List<Harvestable> slots = new List<Harvestable>();
    }

    [SerializeField] private List<CropGroup> groups;

    public void Start()
    {
        int currentDay = GameManager.Instance.GetDate();

        foreach (var g in groups)
        {
            var gData = FieldDataManager.Instance.GetGroupData(g.cropName);

            // ✅ 수정: 슬롯 리스트를 스폰포인트 개수만큼 null로 초기화 (인덱스=spawnPointIndex 고정)
            g.slots = new List<Harvestable>(new Harvestable[g.spawnPoints.Count]);

            if (currentDay == 1)
            {
                // 랜덤 스폰 인덱스 셔플
                List<int> spawnIndices = new List<int>();
                for (int i = 0; i < g.spawnPoints.Count; i++) spawnIndices.Add(i);
                for (int last = spawnIndices.Count - 1; last >= 0; last--)
                {
                    int r = Random.Range(0, last + 1);
                    (spawnIndices[last], spawnIndices[r]) = (spawnIndices[r], spawnIndices[last]);
                }

                // ✅ 수정: gData.slots를 spawnPointIndex 기준으로 "정렬된 배열"로 재구성
                var slotArr = new CropSlotData[g.spawnPoints.Count];
                for (int i = 0; i < slotArr.Length; i++)
                {
                    slotArr[i] = new CropSlotData
                    {
                        spawnPointIndex = i,
                        isAlive = false,
                        respawnDay = int.MaxValue // ✅ 수정: 빈 슬롯은 무한 대기
                    };
                }

                // maxActive만큼 생성 → 해당 spawnPointIndex 위치에 배치
                for (int k = 0; k < Mathf.Min(g.maxActive, g.spawnPoints.Count); k++)
                {
                    int si = spawnIndices[k];
                    var obj = Instantiate(g.prefab, g.spawnPoints[si].position, Quaternion.identity);
                    var h = obj.GetComponent<Harvestable>();
                    h.SetRespawnDay(-1);
                    h.spawnIndex = si;                  // ✅ 수정: 하베스터블에 슬롯 인덱스 기록
                    g.slots[si] = h;                    // ✅ 수정: Add가 아니라 정확한 인덱스에 배치

                    slotArr[si] = new CropSlotData
                    {
                        spawnPointIndex = si,
                        isAlive = true,
                        respawnDay = -1
                    };
                }

                // ✅ 수정: 정렬된 배열로 List 갱신
                gData.slots.Clear();
                gData.slots.AddRange(slotArr);
            }
            else // Day > 1
            {
                // ✅ 슬롯 배열 초기화 (spawnPointIndex 기준 정렬)
                var slotByPoint = new CropSlotData[g.spawnPoints.Count];
                for (int i = 0; i < slotByPoint.Length; i++)
                {
                    slotByPoint[i] = new CropSlotData
                    {
                        spawnPointIndex = i,
                        isAlive = false,
                        respawnDay = int.MaxValue
                    };
                }
                foreach (var s in gData.slots)
                {
                    if (s.spawnPointIndex >= 0 && s.spawnPointIndex < slotByPoint.Length)
                        slotByPoint[s.spawnPointIndex] = s;
                }

                int aliveCount = 0;
                var respawnCandidates = new List<int>();

                // 1) 살아있는 복구 & 후보 수집
                for (int i = 0; i < g.spawnPoints.Count; i++)
                {
                    var sData = slotByPoint[i];

                    if (sData.isAlive)
                    {
                        var obj = Instantiate(g.prefab, g.spawnPoints[i].position, Quaternion.identity);
                        var h = obj.GetComponent<Harvestable>();
                        h.SetRespawnDay(sData.respawnDay);
                        h.spawnIndex = i;
                        g.slots[i] = h;

                        aliveCount++;
                    }
                    else if (currentDay >= sData.respawnDay)
                    {
                        respawnCandidates.Add(i); // ✅ 성숙했지만 스폰은 여기서 안 함
                    }
                }

                // 2) 필요한 개수만큼 랜덤 offset으로 스폰
                int need = Mathf.Max(0, g.maxActive - aliveCount);
                for (int n = 0; n < need && respawnCandidates.Count > 0; n++)
                {
                    // 랜덤 후보 선택
                    int baseIndex = respawnCandidates[Random.Range(0, respawnCandidates.Count)];
                    respawnCandidates.Remove(baseIndex);

                    // 랜덤 offset 계산
                    int offset = Random.Range(1, g.spawnPoints.Count);
                    int newIndex = (baseIndex + offset) % g.spawnPoints.Count;

                    // 비어있는 자리 찾을 때까지 +1
                    int attempts = 0;
                    while (g.slots[newIndex] != null && attempts < g.spawnPoints.Count)
                    {
                        newIndex = (newIndex + 1) % g.spawnPoints.Count;
                        attempts++;
                    }

                    if (g.slots[newIndex] == null)
                    {
                        var obj = Instantiate(g.prefab, g.spawnPoints[newIndex].position, Quaternion.identity);
                        var h = obj.GetComponent<Harvestable>();
                        h.spawnIndex = newIndex;
                        h.SetRespawnDay(-1);

                        g.slots[newIndex] = h;

                        // 데이터 갱신
                        slotByPoint[newIndex].isAlive = true;
                        slotByPoint[newIndex].respawnDay = -1;

                        aliveCount++;
                    }

                    // ✅ 원래 슬롯은 초기화 (쿨타임 슬롯은 소진 처리)
                    slotByPoint[baseIndex].isAlive = false;
                    slotByPoint[baseIndex].respawnDay = int.MaxValue;
                }

                // 보정된 데이터 저장
                gData.slots.Clear();
                gData.slots.AddRange(slotByPoint);
            }

        }
    }

    // 수확 후 슬롯 상태 갱신
    public void UpdateSlotData(Harvestable h, int respawnDay)
    {
        foreach (var g in groups)
        {
            //spawnIndex로 정확히 매칭
            if (h.spawnIndex >= 0 && h.spawnIndex < g.spawnPoints.Count && g.slots[h.spawnIndex] == h)
            {
                var gData = FieldDataManager.Instance.GetGroupData(g.cropName);

                // gData.slots가 spawnPointIndex 정렬을 보장하므로 인덱스로 접근
                if (h.spawnIndex < gData.slots.Count)
                {
                    gData.slots[h.spawnIndex].isAlive = false;
                    gData.slots[h.spawnIndex].respawnDay = respawnDay;
                }

                g.slots[h.spawnIndex] = null;
                return;
            }
        }

        // 여기 도달하면 매칭 실패. 필요시 로그 확인.
        Debug.LogWarning($"UpdateSlotData: matching group/slot not found for {h.name} (spawnIndex={h.spawnIndex})"); // ✅ 수정
    }
}

