#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



[Serializable]
public class CharacterProgress
{
    //public int fixedIndex;   // 0-base, 리스트 인덱스와 동일(이거 안 쓸 거임.)
    public string characterName;   // 이름을 고유 키로 사용
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
    //[SerializeField] private List<CharacterData> characters = new(); // 인스펙트 드래그 드롭 형식
    private List<CharacterData> characters = new(); // Addressables로 로드(로딩 후 자동으로 채워지니까 리스트를 빈 상태로)


    private CharacterDB db = new CharacterDB();

    // ===== 외부 API (UI/게임 로직에서 사용) =====
    public int Count => characters?.Count ?? 0;
    public CharacterData GetStatic(int fixedIndex) => characters[fixedIndex];

    public bool HasMet(string name) => GetProgress(name).hasMet;
    public int GetAffinity(string name) => GetProgress(name).affinity;

    public void Meet(string name, bool met = true)
    {
        GetProgress(name).hasMet = met;
    }
    public void SetAffinity(string name, int value)
    {
        var p = GetProgress(name);
        p.affinity = Mathf.Clamp(value, 0, 100);
    }

    public void AddAffinity(string name, int delta)
    {
        var p = GetProgress(name);
        p.affinity = Mathf.Clamp(p.affinity + delta, 0, 100);
    }

    //비동기 로드
    private async Task LoadCharactersAsync()
    {
        AsyncOperationHandle<IList<CharacterData>> handle =
            Addressables.LoadAssetsAsync<CharacterData>("characterdata", null); // label 기반 로드

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            characters = new List<CharacterData>(handle.Result);
        }
        else
        {
            Debug.LogError("CharacterData 로드 실패");
        }
    }


    // ===== 라이프사이클 =====
    protected override async void Awake()
    {
        base.Awake(); //부모 Awake 호출

        await LoadCharactersAsync();   // Addressables에서 로드

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
            // 변경됨: fixedIndex 개수 검사 대신 이름 기반 매칭
            db = new CharacterDB { list = new List<CharacterProgress>() };

            foreach (var c in characters)
            {
                var saved = loaded.list.Find(x => x.characterName == c.characterName);
                if (saved != null)
                {
                    db.list.Add(new CharacterProgress
                    {
                        characterName = saved.characterName,
                        hasMet = saved.hasMet,
                        affinity = Mathf.Clamp(saved.affinity, 0, 100)
                    });
                }
                else
                {
                    // 신규 캐릭터 → 기본값으로 추가
                    db.list.Add(new CharacterProgress
                    {
                        characterName = c.characterName,
                        hasMet = false,
                        affinity = 0
                    });
                }
            }
        }
        else
        {
            // 첫 실행이거나 캐릭터 수가 바뀐 경우 → 현재 정적 데이터 기준으로 재생성
            InitializeProgressFromStatic();
        }

        // // 인덱스 정합성 보장
        // db.list.Sort((a, b) => a.fixedIndex.CompareTo(b.fixedIndex));
        // for (int i = 0; i < db.list.Count; i++)
        // {
        //     if (db.list[i] == null)
        //         db.list[i] = new CharacterProgress();
        //     db.list[i].fixedIndex = i;
        //     db.list[i].affinity = Mathf.Clamp(db.list[i].affinity, 0, 100);
        // }
    }

    // ===== 내부 유틸 =====
    private void InitializeProgressFromStatic()
    {
        db.list = new List<CharacterProgress>(characters.Count);
        foreach (var c in characters)
        {
            db.list.Add(new CharacterProgress
            {
                characterName = c.characterName,
                hasMet = false,
                affinity = 0
            });
        }
    }

    private CharacterProgress GetProgress(string name)
    {
        var p = db.list.Find(x => x.characterName == name);
        if (p == null)
            throw new ArgumentException($"Character with name '{name}' not found.");
        return p;
    }

}
