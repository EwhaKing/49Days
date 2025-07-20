using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CauldronLid : MonoBehaviour
{
    public static CauldronLid Instance { get; private set; }

    enum CauldronState
    {
        Idle,
        Roasting,
        Roasted
    }

    [Header("프리팹 설정")]
    [SerializeField] private GameObject roastingIngredientPrefab;       // 복제를 위한 RoastingIngredient 프리팹
    [SerializeField] private Transform spawnAreaCenter;         // 복제 RoastingIngredient가 생성될 중심 위치
    [SerializeField] private GameObject cursorFollowerPrefab;
    private GameObject currentCursorFollower;

    [Header("미니게임 설정")]
    [SerializeField] private int spawnIngredientNum;            // 복제할 재료의 수
    [SerializeField] private float roastDuration;               // 덖기 미니게임 총 시간
    [SerializeField] private float darkenInterval;              // 덖기 단계 올라가는(Darken) 시간 간격
    [SerializeField] private float spawnRadius;                 // 재료 생성 반경

    private GameObject currentIngredient;                       // 현재 가마솥에 들어간 재료
    private readonly List<RoastingIngredient> activeIngredients = new(); // 활성화 상태인 복제 RoastingIngredients
    private float roastTimer;                                   // 덖기 타이머
    private CauldronState cauldronState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ClearCauldronLid();
    }

    void OnMouseEnter()
    {
        // TODO: 마우스 오버 시 스프라이트 커짐 + 하이라이트    
    }

    private void OnMouseUp()
    {
        switch (cauldronState)
        {
            case CauldronState.Idle:
                StartRoasting();
                break;
            case CauldronState.Roasting:
                if (Hand.Instance.handIngredient != null && currentIngredient != null)
                {
                    Debug.LogWarning($"가마솥에 이미 {currentIngredient}이(가) 들어있으므로 새로운 재료는 넣을 수 없습니다.");
                }
                else
                {
                    Debug.LogWarning("덖기 중에는 재료를 건드릴 수 없습니다.");
                }
                break;
            case CauldronState.Roasted:
                if (Hand.Instance.handIngredient != null)
                {
                    Debug.LogWarning($"손에 재료를 든 상태로 다른 재료를 들 수 없습니다. 현재 {Hand.Instance.handIngredient.name}을(를) 쥐고 있습니다.");
                }
                if (Hand.Instance.handIngredient == null && currentIngredient != null)
                {
                    Debug.Log("덖기 완료된 재료를 수거합니다.");
                    currentIngredient.SetActive(true);
                    Hand.Instance.Grab(currentIngredient.gameObject);
                    ClearCauldronLid();
                }
                break;
        }
    }

    void SpawnRoastingIngredient(TeaIngredient ingredient)
    {
        for (int i = 0; i < spawnIngredientNum; i++)
        {
            // 가마솥 안에서 랜덤 위치를 지정해 미니게임 재료 생성
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnAreaCenter.position + new Vector3(offset.x, offset.y, 0f);
            GameObject roastingObject = Instantiate(roastingIngredientPrefab, spawnPos, Quaternion.identity);

            RoastingIngredient roastingIngredient = roastingObject.GetComponent<RoastingIngredient>();
            if (roastingIngredient != null)
            {
                roastingIngredient.Init(ingredient, spawnAreaCenter);
                roastingIngredient.OnBurnt += HandleIngredientBurnt;
                activeIngredients.Add(roastingIngredient);
            }
        }
        Debug.Log($"{currentIngredient.name}을(를) {spawnIngredientNum}개 생성했습니다.");
    }

    void ClearRoastingIngredient()
    {
        foreach (var ingredient in activeIngredients)       // 복제 재료 제거
        {
            if (ingredient != null)
            {
                ingredient.OnBurnt -= HandleIngredientBurnt;
                Destroy(ingredient.gameObject);
            }
        }
        activeIngredients.Clear();
    }

    private bool ValidateRoastingCondition()
    {
        TeaIngredient handIngredient = Hand.Instance?.handIngredient;

        // 손에 쥔 재료가 없으면 거부
        if (handIngredient == null)
        {
            Debug.LogWarning("재료를 들고 있지 않습니다. Hand가 null입니다.");
            return false;
        }

        // 손에 쥔 재료가 대용차 용도일 경우 거부
        if (handIngredient.ingredientType == IngredientType.Substitute)
        {
            Debug.Log("대용차 재료는 가마솥에 덖을 수 없습니다.");
            return false;
        }

        // 이미 덖은 재료 거부
        if (handIngredient.roasted != ResultStatus.None)
        {
            Debug.Log("이미 덖은 재료는 가마솥에 재차 덖을 수 없습니다.");
            return false;
        }

        // 유념을 실패했거나, 산화에 실패한 재료 거부
        if (handIngredient.rolled == ResultStatus.Failed || handIngredient.oxidizedDegree == OxidizedDegree.Over)
        {
            Debug.Log("뭉개짐 상태(유념 실패)이거나, 산화에 실패한 재료는 덖을 수 없습니다.");
            return false;
        }

        // 위 조건 이외론 손에 든 재료 Drop
        currentIngredient = Hand.Instance.Drop();
        Debug.Log($"{handIngredient.spriteStatus} 상태 {handIngredient.ingredientName}을(를) 가마솥에 넣었습니다.");
        currentIngredient.SetActive(false);
        return true;
    }
    
    private void StartRoasting()
    {
        if (!ValidateRoastingCondition()) return;

        cauldronState = CauldronState.Roasting;
        roastTimer = 0f;

        TeaIngredient ingredient = currentIngredient.GetComponent<TeaIngredient>();
        SpawnRoastingIngredient(ingredient);
        // activeIngredients.AddRange(FindObjectsOfType<RoastingIngredient>());

        currentCursorFollower = Instantiate(cursorFollowerPrefab);

        StartCoroutine(RoastingRoutine());
        Debug.Log("덖기 시작");
    }

    private IEnumerator RoastingRoutine()
    {
        while (roastTimer < roastDuration)
        {
            yield return new WaitForSeconds(darkenInterval);
            roastTimer += darkenInterval;

            foreach (var ingredient in activeIngredients)
            {
                ingredient.DarkenColor();
            }
        }
        bool success = activeIngredients.All(roastingIngredient => !roastingIngredient.IsBurnt);
        StopRoasting(success);
    }

    private void HandleIngredientBurnt()
    {
        if (cauldronState != CauldronState.Roasting) return;

        Debug.Log("한 개라도 탄 순간 덖기는 실패 처리합니다.");
        StopAllCoroutines();

        foreach (var ingredient in activeIngredients)
        {
            ingredient.Burn();
        }
        StopRoasting(false);
    }


    private void StopRoasting(bool success)
    {
        cauldronState = CauldronState.Roasted;

        TeaIngredient original = currentIngredient.GetComponent<TeaIngredient>();
        original.Roast(success);

        if (success)
        {
            currentIngredient.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.3f, 0.2f);
        }
        else
        {
            currentIngredient.GetComponent<SpriteRenderer>().color = Color.black;
        }

        foreach (var ingredient in activeIngredients)
        {
            ingredient.Stop();
        }

        if (currentCursorFollower != null)
        {
            Destroy(currentCursorFollower);
        }
        
        Debug.Log($"덖기에 {(success ? "성공" : "실패")}했습니다.");
    }


    void Update()
    {

    }

    void ClearCauldronLid()
    {
        cauldronState = CauldronState.Idle;
        currentIngredient = null;
        ClearRoastingIngredient();
        roastTimer = 0f;
    }
}
