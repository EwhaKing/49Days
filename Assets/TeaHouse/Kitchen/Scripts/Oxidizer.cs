using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxidizer : MonoBehaviour
{
    enum OxidizerState
    {
        ClosedIdle,     // 기본 상태
        OpenReady,      // 찻잎 Grab + 산화기 마우스오버 상태
        Oxidizing,       // 미니게임 진행 중
        OverReady       // 탐 상태에서 유저 클릭 대기 상태
    }

    [Header("산화기 외형")]
    [SerializeField] SpriteRenderer backgroundRenderer; // 산화기 열림/닫힘 이미지 관리
    [SerializeField] Sprite openSprite;                 // 산화기 열렸을 때 이미지
    [SerializeField] Sprite closedSprite;               // 산화기 닫혔을 때 이미지

    [Header("게이지 관련")]
    [SerializeField] GameObject gaugePlate;         // 게이지 판
    [SerializeField] List<GameObject> gaugePlates;  // 게이지 판 리스트 (0~4까지 5개)
    [SerializeField] GameObject gaugeArrow;         // 게이지 화살표
    [SerializeField] Transform arrowTransform;      // 화살표 Transform

    [Header("산화 관련")]
    [SerializeField] float totalTime = 5f;          // 최대 산화 시간 (이후 탐)

    OxidizerState state = OxidizerState.ClosedIdle;
    TeaIngredient currentIngredient;
    float elapsedTime = 0f;                         // 현재 경과 시간
    int currentTick = 0;                            // 현재 게이지 판 인덱스 (0~4)
    float gaugeAngle = 0f;                          // 게이지 화살표 회전 각도

    void Start()
    {
        ResetOxidizer(); // 기본 초기화
    }

    void OnMouseEnter()
    {
        TeaIngredient ingredient = Hand.Instance.handIngredient;

        if (state == OxidizerState.ClosedIdle &&
            ingredient != null &&
            ingredient.ingredientType == IngredientType.TeaLeaf &&
            (ingredient.oxidizedDegree == OxidizedDegree.None ||
             ingredient.oxidizedDegree == OxidizedDegree.Zero))
        {
            // 산화기 열기
            state = OxidizerState.OpenReady;
            backgroundRenderer.sprite = openSprite;
        }
    }

    void OnMouseExit()
    {
        if (state == OxidizerState.OpenReady)
        {
            // 산화기 다시 닫기
            state = OxidizerState.ClosedIdle;
            backgroundRenderer.sprite = closedSprite;
        }
    }

    void OnMouseUp()
    {
        switch (state)
        {
            case OxidizerState.OpenReady:
                TryStartOxidation();        // 산화 시작 시도
                break;

            case OxidizerState.Oxidizing:
                if (Hand.Instance.handIngredient != null)
                {
                    Debug.LogWarning("손에 재료가 이미 들려있습니다.");
                    break;
                }
                HandleEarlyFinish();        // 중간 클릭 → 즉시 산화 완료 + Grab
                break;

            case OxidizerState.OverReady:   // Over 상태 후 클릭 → Grab 및 리셋
                if (Hand.Instance.handIngredient != null)
                {
                    Debug.LogWarning("손에 재료가 이미 들려있습니다.");
                    break;
                }
                StartCoroutine(DelayedReset());
                break;
        }
    }

    void TryStartOxidation()
    {
        TeaIngredient ingredient = Hand.Instance.handIngredient;

        // 이미 조리 단계를 거친 재료 거부 (덖기/유념)
        if (ingredient.processSequence.Contains(ProcessStep.Roast) ||
            ingredient.processSequence.Contains(ProcessStep.Roll))
        {
            Debug.LogWarning($"{ingredient.name}은(는) 조리 단계(유념/덖기)를 거쳐 산화할 수 없습니다.");
            return;
        }

        // 조건 통과 → 산화 시작
        currentIngredient = ingredient;
        currentIngredient.gameObject.SetActive(false);
        Hand.Instance.Drop();

        backgroundRenderer.sprite = closedSprite;
        gaugeArrow.SetActive(true);
        gaugePlate.SetActive(true);

        state = OxidizerState.Oxidizing;
        gaugePlates[0].SetActive(true);

        Debug.Log($"{ingredient.name}의 산화를 시작합니다.");
    }

    void Update()
    {
        if (state != OxidizerState.Oxidizing) return;

        elapsedTime += Time.deltaTime;
        gaugeAngle = (elapsedTime / totalTime) * 360f;                  // 5초 동안 360도 회전
        arrowTransform.rotation = Quaternion.Euler(0, 0, -gaugeAngle);  // 시계 방향 회전

        // 게이지 판 활성화 로직
        if (elapsedTime >= (currentTick + 1) * 1f &&
            currentTick + 1 < gaugePlates.Count)
        {
            currentTick++;
            gaugePlates[currentTick].SetActive(true);
        }

        if (elapsedTime >= totalTime)
        {
            state = OxidizerState.OverReady;
            StartCoroutine(CompleteOxidation(OxidizedDegree.Over));
        }
    }

    void HandleEarlyFinish()
    {
        if (currentIngredient == null) return;

        OxidizedDegree degree = GetOxidizedDegreeFromGauge();
        StartCoroutine(CompleteOxidation(degree));
    }

    IEnumerator CompleteOxidation(OxidizedDegree degree)
    {
        currentIngredient.Oxidize(degree);
        ApplyColorByOxidation(currentIngredient, degree);

        if (state == OxidizerState.OverReady)
        {
            // 산화기 닫힌 상태 + 게이지 유지
            backgroundRenderer.sprite = closedSprite;
            foreach (var plate in gaugePlates)
                plate.SetActive(true);
            yield break;
        }
        else
        {
            // 바로 Grab 및 리셋
            StartCoroutine(DelayedReset());
        }
    }

    IEnumerator DelayedReset()
    {
        currentIngredient.gameObject.SetActive(true);
        Hand.Instance.Grab(currentIngredient.gameObject);

        // 시각적 리셋
        backgroundRenderer.sprite = openSprite;
        arrowTransform.rotation = Quaternion.Euler(0, 0, 0);
        foreach (var plate in gaugePlates)
            plate.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        ResetOxidizer();
    }


    OxidizedDegree GetOxidizedDegreeFromGauge()
    {
        int activeCount = 0;
        foreach (var plate in gaugePlates)
        {
            if (plate.activeSelf)
                activeCount++;
        }

        switch (activeCount)
        {
            case 1:
            case 2: Debug.Log("0"); return OxidizedDegree.Zero;
            case 3: Debug.Log("50"); return OxidizedDegree.Half;
            case 4: Debug.Log("100"); return OxidizedDegree.Full;
            case 5: Debug.Log("탐"); return OxidizedDegree.Over;
            default: Debug.LogWarning($"게이지 개수가 비정상입니다: {activeCount}개가 활성화됨"); return OxidizedDegree.None;
        }
    }

    void ApplyColorByOxidation(TeaIngredient ingredient, OxidizedDegree degree)
    {
        var sr = ingredient.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (degree)
        {
            case OxidizedDegree.Zero:
                sr.color = new Color(0.8f, 1f, 0.8f); break;    // 초록
            case OxidizedDegree.Half:
                sr.color = new Color(1f, 0.8f, 0.3f); break;    // 노랑
            case OxidizedDegree.Full:
                sr.color = new Color(0.8f, 0.4f, 0.2f); break;  // 주황
            case OxidizedDegree.Over:
                sr.color = new Color(0.2f, 0.13f, 0.05f); break; // 검정
        }
    }

    void ResetOxidizer()
    {
        state = OxidizerState.ClosedIdle;
        backgroundRenderer.sprite = closedSprite;

        arrowTransform.rotation = Quaternion.Euler(0, 0, 0);
        foreach (var plate in gaugePlates)
            plate.SetActive(false);

        currentIngredient = null;
        elapsedTime = 0f;
        currentTick = 0;
    }

}