using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using JetBrains.Annotations;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
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

    enum IngredientState
    {
        Default,
        Roasted,
        Burnt
    }

    [SerializeField] GameObject roastingIngredientPrefab;       // 복제를 위한 RoastingIngredient 프리팹
    [SerializeField] private Transform spawnAreaCenter;         // 복제 RoastingIngredient가 생성될 중심 위치
    [SerializeField] private int spawnIngredientNum = 6;        // 복제할 재료의 수
    [SerializeField] private float roastDuration = 8f;          // 덖기 미니게임 총 시간
    [SerializeField] private float darkenInterval = 2f;         // 덖기 단계 올라가는(Darken) 시간 간격
    public float spawnRadius = 1.5f;                              // 재료 생성 반경

    private GameObject currentIngredient;                       // 현재 가마솥에 들어간 재료
    private List<RoastingIngredient> activeIngredients = new(); // 활성화 상태인 복제 RoastingIngredients
    private float roastTimer = 0f;                              // 덖기 타이머
    CauldronState cauldronState = CauldronState.Idle;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ClearCauldronLid();
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
                if (Hand.Instance.handIngredient == null && currentIngredient != null && activeIngredients.Count > 0)
                {
                    Debug.Log("덖기 완료된 재료를 수거합니다.");

                    currentIngredient.SetActive(true);
                    Hand.Instance.Grab(currentIngredient.gameObject);
                    currentIngredient = null;
                    cauldronState = CauldronState.Idle;

                    foreach (var ingredient in activeIngredients)       // 복제 재료 제거
                    {
                        if (ingredient != null)
                            Destroy(ingredient.gameObject);
                    }
                    activeIngredients.Clear();
                    return;
                }
                break;
        }
    }

    void SpawnRoastingIngredient(TeaIngredient ingredient, Sprite ingredientSprite)
    {
        for (int i = 0; i < spawnIngredientNum; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnAreaCenter.position + new Vector3(offset.x, offset.y, 0f);
            GameObject roastingObject = Instantiate(roastingIngredientPrefab, spawnPos, Quaternion.identity);

            RoastingIngredient roastingIngredient = roastingObject.GetComponent<RoastingIngredient>();
            if (roastingIngredient != null)
            {
                roastingIngredient.Init(ingredient, ingredientSprite);
            }
        }
        Debug.Log($"덖기용 {ingredient.ingredientType}를 {spawnIngredientNum}개 생성했습니다.");
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
        // currentIngredient.transform.position = transform.position;
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
        Sprite sprite = currentIngredient.GetComponent<SpriteRenderer>()?.sprite;
        SpawnRoastingIngredient(ingredient, sprite);

        activeIngredients.Clear();
        activeIngredients.AddRange(FindObjectsOfType<RoastingIngredient>());

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
                if (ingredient != null)
                {
                    ingredient.DarkenColor();
                }
            }
        }
        bool success = activeIngredients.All(roastingIngredient => roastingIngredient != null && !roastingIngredient.IsBurnt);
        StopRoasting(success);
    }

    public void OnIngredientBurned()
    {
        if (cauldronState != CauldronState.Roasting) return;

        cauldronState = CauldronState.Roasted;

        currentIngredient.GetComponent<SpriteRenderer>().color = Color.black;

        StopAllCoroutines();

        Debug.Log("한 개라도 탄 순간 덖기 실패 처리.");

        foreach (var ingredient in activeIngredients)
        {
            if (ingredient != null)
            {
                ingredient.Burn();
            }
        }

        StopRoasting(false);
    }


    private void StopRoasting(bool success)
    {
        TeaIngredient original = currentIngredient.GetComponent<TeaIngredient>();
        original.Roast(success);
        if (success)
        {
            currentIngredient.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.3f, 0.2f);
        }

        foreach (var ingredient in activeIngredients)
        {
            if (ingredient != null)
            {
                ingredient.Stop();
            }
        }
        cauldronState = CauldronState.Roasted;
        Debug.Log($"덖기에 {(success ? "성공" : "실패")}했습니다.");
    }


    void Update()
    {

    }

    void ClearCauldronLid()
    {
        
    }
}
