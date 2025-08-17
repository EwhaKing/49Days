#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterProgress
{
    public int fixedIndex;   // 0-base, 리스트 인덱스와 동일
    public bool hasMet;
    public int affinity;     // 0..100
}

[Serializable]
public class CharacterDB
{
    public List<CharacterProgress> list = new();
}

public class CharacterManager : SceneSingleton<CharacterManager>
{
    [Header("Static Character Data (order = fixedIndex)")]
    [SerializeField] private List<CharacterData> characters = new(); // 프로필/텍스트 등 정적 정보

    private CharacterDB db = new CharacterDB();

    // ===== 외부 API (UI/게임 로직에서 사용) =====
    public int Count => characters?.Count ?? 0;
    public CharacterData GetStatic(int fixedIndex) => characters[fixedIndex];

    public bool HasMet(int fixedIndex) => GetByIndex(fixedIndex).hasMet;
    public int GetAffinity(int fixedIndex) => GetByIndex(fixedIndex).affinity;

    public void Meet(int fixedIndex, bool met = true)
    {
        GetByIndex(fixedIndex).hasMet = met;
    }

    public void SetAffinity(int fixedIndex, int value)
    {
        var p = GetByIndex(fixedIndex);
        p.affinity = Mathf.Clamp(value, 0, 100); // 이제 0~100 단위로 저장
    }

    public void AddAffinity(int fixedIndex, int delta)
    {
        var p = GetByIndex(fixedIndex);
        p.affinity = Mathf.Clamp(p.affinity + delta, 0, 100); // 0~100 범위
    }

    // ===== 라이프사이클 =====
    protected override void Awake()
    {
        base.Awake(); //부모 Awake 호출

        // CharacterData 고정 인덱스 동기화 (리스트 순서 = fixedIndex)
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i] != null)
                characters[i].fixedIndex = i;
        }

        // 저장이 아직 로드되기 전 상황을 고려해, 일단 비어있다면 메모리 상태만 기본 생성
        if (db.list == null || db.list.Count == 0)
            InitializeProgressFromStatic();
    }

    private void OnEnable()
    {
        if (SaveLoadManager.Instance == null) return;
        SaveLoadManager.Instance.onSave += HandleSave;
        SaveLoadManager.Instance.onLoad += HandleLoad;
    }
    private void OnDisable()
    {
        if (SaveLoadManager.Instance == null) return;
        SaveLoadManager.Instance.onSave -= HandleSave;
        SaveLoadManager.Instance.onLoad -= HandleLoad;
    }

    // ===== Save/Load 연동 =====
    private void HandleSave()
    {
        SaveLoadManager.Instance.Save(db); // 파일명: "CharacterDB"
    }

    private void HandleLoad()
    {
        CharacterDB loaded = null;
        try
        {
            loaded = SaveLoadManager.Instance.Load<CharacterDB>();
        }
        catch (Exception)
        {
            // 파일 없음/깨짐 등 → 첫 실행으로 간주
        }

        if (loaded != null && loaded.list != null && loaded.list.Count == characters.Count)
        {
            db = loaded;
        }
        else
        {
            // 첫 실행이거나 캐릭터 수가 바뀐 경우 → 현재 정적 데이터 기준으로 재생성
            InitializeProgressFromStatic();
        }

        // 인덱스 정합성 보장
        db.list.Sort((a, b) => a.fixedIndex.CompareTo(b.fixedIndex));
        for (int i = 0; i < db.list.Count; i++)
        {
            if (db.list[i] == null)
                db.list[i] = new CharacterProgress();
            db.list[i].fixedIndex = i;
            db.list[i].affinity = Mathf.Clamp(db.list[i].affinity, 0, 100);
        }
    }

    // ===== 내부 유틸 =====
    private void InitializeProgressFromStatic()
    {
        db.list = new List<CharacterProgress>(characters.Count);
        for (int i = 0; i < characters.Count; i++)
        {
            db.list.Add(new CharacterProgress
            {
                fixedIndex = i,
                hasMet = false, //첫 시작 : 모두 미만남
                affinity = 0 //  첫 시작 : 모두 호감도 0
            });
        }
    }

    private CharacterProgress GetByIndex(int fixedIndex)
    {
        if (fixedIndex < 0 || fixedIndex >= db.list.Count)
            throw new IndexOutOfRangeException(
                $"Character index {fixedIndex} out of range (Count={db.list.Count}).");
        return db.list[fixedIndex];
    }


    //여기부터 테스트용 함수. 삭제해야 함

    // #if UNITY_EDITOR
    //     [ContextMenu("Dev/Seed: Meet ALL + 0..100 by 5")]
    //     private void Dev_SeedAll_Stepped()
    //     {
    //         for (int i = 0; i < Count; i++)
    //         {
    //             Meet(i, true);
    //             SetAffinity(i, (i * 5) % 105); // 0,5,10,...,100 반복
    //         }
    //         // 패널 새로고침
    //         var panel = FindObjectOfType<AffinityPanel>();
    //         if (panel) panel.RefreshPage();
    //     }

    //     [ContextMenu("Dev/Seed: Meet FIRST PAGE + Random(5-step)")]
    //     private void Dev_SeedFirstPage_Random()
    //     {
    //         var rand = new System.Random(1234);
    //         int firstPageCount = Mathf.Min(Count, 9);
    //         for (int i = 0; i < Count; i++)
    //         {
    //             bool met = i < firstPageCount; // 첫 페이지(0~8)만 만남 처리
    //             Meet(i, met);
    //             int val = met ? (rand.Next(0, 21) * 5) : 0; // 0..100, 5단위
    //             SetAffinity(i, val);
    //         }
    //         var panel = FindObjectOfType<AffinityPanel>();
    //         if (panel) panel.RefreshPage();
    //     }
    // #endif

}
