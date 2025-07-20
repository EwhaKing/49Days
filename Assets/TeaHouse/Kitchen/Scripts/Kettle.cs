using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kettle : MonoBehaviour
{
    private float maxAngle = 360f; // -355f=5f
    private float minAngle = 72f; // -72f=288f

    enum KettleState { OnFire, OnHook, Dragging }
    KettleState currentState = KettleState.OnFire;

    public float Temperature { get; private set; } = 100;

    [SerializeField] float tempChangePerSec = 2f;
    [SerializeField] float pourRadius = 1f;
    [SerializeField] float hookSnapDistance = 1.2f;
    [SerializeField] Transform gaugeNeedle;
    [SerializeField] Transform stovePosition;
    [SerializeField] Transform hookPosition;
    [SerializeField] Transform teapotPosition;
    [SerializeField] Transform kettleHandlePosition; //주전자 손잡이 위치 (고리에 걸기 위하여)
    [SerializeField] Transform kettleSpoutPosition; //주전자 주둥이 위치 (다병에 붓기 위하여)
    [SerializeField] Transform kettleBottomPosition;
    [SerializeField] float smokeFadeSpeed = 0.2f; // 연기 투명도 변화 속도 (초당 변화량)




    //주전자 회전 관련 변수들 
    [SerializeField] float pourDuration = 2f;
    [SerializeField] float pourAngle = 15f; // 시계 방향 기울기
    bool isPouring = false;
    Quaternion originalRotation;
    // [SerializeField] Transform kettlePivot; // 회전용 빈 부모 오브젝트

    public TeaPot teapot; // Inspector에서 할당
    GameObject heldSmokeObject;
    Animator smokeAnimator;

    Vector3 dragOffset;
    bool isDragging = false;
    float cachedTemperature = -1;

    void Start()
    {
        Debug.Log("[피벗 위치] " + transform.position);
        Debug.Log("[바닥 위치] " + kettleBottomPosition.position);
        Debug.Log("[화로 위치] " + stovePosition.position);


        SetToFire(); // 시작 시 화로 위치로 이동

        // SmokeObject와 Animator 찾기
        heldSmokeObject = transform.Find("SmokeObject")?.gameObject;
        if (heldSmokeObject != null)
            smokeAnimator = heldSmokeObject.GetComponent<Animator>();
        else
            Debug.LogWarning("[연기] SmokeObject를 찾을 수 없습니다.");


        if (stovePosition == null)
            stovePosition = GameObject.Find("StovePosition")?.transform;

        if (hookPosition == null)
            hookPosition = GameObject.Find("KettleHookPosition")?.transform;

        if (teapotPosition == null)
            teapotPosition = GameObject.Find("PourPosition")?.transform;

        if (teapot == null)
            teapot = GameObject.FindObjectOfType<TeaPot>();


    }

    void Update()
    {
        // Debug.Log($"[위치 확인] 주전자: {transform.position}, 바닥: {kettleBottomPosition.position}, 화로: {stovePosition.position}");


        //물 붓는 동안에는 움직이지 마세요
        if (isPouring) return;

        // 온도 변경 (드래그 중이 아닐 때만)
        if (!isDragging)
        {
            float delta = 0f;
            switch (currentState)
            {
                case KettleState.OnFire:
                    delta = tempChangePerSec * Time.deltaTime;
                    break;
                case KettleState.OnHook:
                    if (Temperature > 50f)
                        delta = -tempChangePerSec * Time.deltaTime;
                    else if (Temperature <= 50f && Temperature > 25f) //25~50도 사이에서는 온도 천천히 감소
                        delta = -tempChangePerSec / 2 * Time.deltaTime;
                    else if (Temperature == 25f)
                        delta = 0f;

                    break;
            }
            Temperature = Mathf.Clamp(Temperature + delta, 0f, 100f);
            //Debug.Log($"[온도] 상태: {currentState}, 현재 온도: {Temperature:F2}");
            UpdateNeedleRotation();
        }

        if (heldSmokeObject != null && kettleSpoutPosition != null)
        {
            heldSmokeObject.transform.position = kettleSpoutPosition.position + new Vector3(-6.18f, -0.74f, 0); // 연기 내 이미지 때문에 벡터 조정

            bool shouldShow = Temperature >= 85f;

            if (!heldSmokeObject.activeSelf)
                heldSmokeObject.SetActive(true);

            //Debug.Log($"[연기] 온도: {Temperature}, isSmoking: {shouldShow}");
        }
    }

    //연기는 천천히 회전시키기(그게 자연스러움)
    void LateUpdate()
    {
        if (heldSmokeObject != null)
        {
            heldSmokeObject.transform.position = kettleSpoutPosition.position + new Vector3(-6.18f, -0.74f, 0); // 연기 내 이미지 때문에 벡터 조정;

            // 주전자(Z축) 회전값의 절반만 연기에 적용
            float parentZ = transform.eulerAngles.z;
            float smokeZ = parentZ * 0.5f;

            heldSmokeObject.transform.rotation = Quaternion.Euler(0, 0, smokeZ);
        }

        // 연기 알파값 고정 유지용
        var spriteRenderer = heldSmokeObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            float targetAlpha = 0f;

            if (Temperature >= 85f)
            {
                targetAlpha = 1f;
            }
            else if (Temperature >= 70f)
            {
                targetAlpha = (Temperature - 70f) / (85f - 70f); // 선형 보간
            }
            else
            {
                targetAlpha = 0f;
            }
            float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, Time.deltaTime * smokeFadeSpeed);
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            //디버그
            //  Debug.Log($"[연기 상태] alpha={newAlpha:F2}, sprite={(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "null")}");
        }
    }


    //바늘 회전
    void UpdateNeedleRotation()
    {
        float t = 1 - Temperature / 100f; // 0~1로 정규화
        // 온도가 100→0으로 떨어지므로, t=1(100도)일 때 시작점, t=0(0도)일 때 끝점
        float angle = Mathf.Lerp(minAngle, maxAngle, t); // minAngle=시작, maxAngle=끝
        gaugeNeedle.transform.localEulerAngles = new Vector3(0, 0, angle);
    }


    void OnMouseDown() //누를 때
    {
        //물 붓는 동안에는 움직이지 마세요
        if (isPouring) return;

        if (Hand.Instance.handIngredient != null) return;
        isDragging = true;
        currentState = KettleState.Dragging;


        Vector3 mouseWorld = GetMouseWorldPos();
        dragOffset = transform.position - mouseWorld;


        cachedTemperature = Temperature;
    }

    void OnMouseDrag()
    {
        //물 붓는 동안에는 움직이지 마세요 + 드래그 안 하는 중이면 함수 실행시키지 마세요.(당연함)
        if (isPouring || !isDragging) return;


        // 드래그 중일 때 주전자의 위치를 마우스 위치로 업데이트
        transform.position = GetMouseWorldPos() + dragOffset;
    }

    void OnMouseUp() //땔 때
    {
        //물 붓는 동안에는 움직이지 마세요
        if (isPouring) return;

        isDragging = false;
        /* 위치 확인용...
                Debug.Log($"[좌표] 주전자 위치: {transform.position}");
                Debug.Log($"[좌표] 손잡이 위치: {kettleHandlePosition.position}");
                Debug.Log($"[좌표] 주둥이 위치: {kettleSpoutPosition.position}");
                Debug.Log($"[좌표] 고리 위치: {hookPosition.position}");
                Debug.Log($"[좌표] 다병 위치: {teapotPosition.position}");
                Debug.Log($"[좌표] 화로 위치: {stovePosition.position}");
        */
        float distToTeapot = Vector3.Distance(kettleSpoutPosition.position, teapot.pourPosition.position);
        float distToHook = Vector3.Distance(kettleHandlePosition.position, hookPosition.position);
        float distToFire = Vector3.Distance(kettleBottomPosition.position, stovePosition.position);


        /*
                Debug.Log($"[거리] 다병까지 거리: {distToTeapot}");
                Debug.Log($"[거리] 고리까지 거리: {distToHook}");
                Debug.Log($"[거리] 화로까지 거리: {distToFire}");
        */
        bool triedPour = false;

        // 1. 다병 범위 안이면 물 붓기 시도
        if (distToTeapot <= pourRadius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(kettleSpoutPosition.position, pourRadius);
            foreach (var hit in hits)
            {
                TeaPot pot = hit.GetComponent<TeaPot>();
                if (pot != null && cachedTemperature <= 100)
                {
                    StartCoroutine(PourWaterAnimation(pot));
                    triedPour = true;
                    break;
                }
            }
            if (!triedPour)
            {
                Debug.Log("물 붓기 실패: 거리 조건은 맞지만 다병 없음");
            }
        }

        // 2. 물 안 부었고, 고리 반경 안이면 고리에 걸기
        if (!triedPour && distToHook <= hookSnapDistance)
        {
            Vector3 offset = transform.position - kettleHandlePosition.position;
            transform.position = hookPosition.position + offset;
            currentState = KettleState.OnHook;
            Debug.Log("[상태 변경] 고리에 걸림 → OnHook 상태");
        }

        // 3. 물도 못 붓고 고리도 아니면 화로로 복귀
        else if (!triedPour)
        {
            if (distToFire > hookSnapDistance)
                Debug.Log("잘못된 위치 드롭 → 화로로 복귀");
            SetToFire();
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = 10f;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    public void SetToFire()
    {
        // kettleBottomPosition이 stovePosition 위치에 정확히 맞도록 KettleObject의 위치 조정
        Vector3 offset = kettleBottomPosition.position - transform.position;
        transform.position = stovePosition.position - offset;

        currentState = KettleState.OnFire;
        Debug.Log($"[SetToFire] 최종 주전자 위치: {transform.position}");
    }



    // 주전자에 물 붓기 애니메이션 함수
    IEnumerator PourWaterAnimation(TeaPot pot)
    {
        isPouring = true;
        isDragging = false;
        currentState = KettleState.Dragging;

        // 연기 알파를 0으로 줄이기 시작
        StartCoroutine(FadeSmokeTo(0f, smokeFadeSpeed * 3f));


        Quaternion originalRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, pourAngle);
        float elapsed = 0f;

        while (elapsed < pourDuration)
        {
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, elapsed / pourDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        pot.PourWater(cachedTemperature);
        Debug.Log("[행동] 물 붓기 완료");

        yield return new WaitForSeconds(0.5f); // 0.5초 정지

        elapsed = 0f;
        while (elapsed < pourDuration)
        {
            transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, elapsed / pourDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation;

        // 온도 기반 알파값 계산
        float targetAlpha = 0f;
        if (Temperature >= 85f) targetAlpha = 1f;
        else if (Temperature >= 70f) targetAlpha = (Temperature - 70f) / 15f;
        // 연기 알파를 다시 복원
        StartCoroutine(FadeSmokeTo(targetAlpha, smokeFadeSpeed * 3f));



        isPouring = false;
    }

    //주전자에 물 부을 때 바뀌는 연기의 투명도 조절
    IEnumerator FadeSmokeTo(float targetAlpha, float speed)
    {
        if (heldSmokeObject == null) yield break;

        var spriteRenderer = heldSmokeObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color currentColor = spriteRenderer.color;

        while (!Mathf.Approximately(currentColor.a, targetAlpha))
        {
            float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, Time.deltaTime * speed);
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            currentColor = spriteRenderer.color;
            yield return null;
        }
    }

}