using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kettle : MonoBehaviour
{
    private float maxAngle = 0f;
    private float minAngle = 288f;

    float tempChangeAccumulator = 0f;

    enum KettleState { OnFire, OnHook, Dragging }
    KettleState currentState = KettleState.OnFire;

    public float Temperature { get; private set; } = 100;

    [SerializeField] float tempChangePerSec = 5f;
    [SerializeField] float pourRadius = 1.5f;
    [SerializeField] float hookSnapDistance = 1.2f;
    [SerializeField] Transform gaugeNeedle;
    [SerializeField] Transform firePosition;
    [SerializeField] Transform hookPosition;
    [SerializeField] Transform teapotPosition;
    [SerializeField] Transform kettleHandlePosition;
    [SerializeField] Transform kettleSpoutPosition;

    GameObject heldSmokeObject;         // Drop()으로 받은 오브젝트 저장
    Animator smokeAnimator;

    Vector3 dragOffset;
    bool isDragging = false;
    float cachedTemperature = -1;

    void Start()
    {
        SetToFire(); // 시작 시 화로 위치로 이동

        // SmokeObject와 Animator 찾기
        heldSmokeObject = transform.Find("SmokeObject")?.gameObject;
        if (heldSmokeObject != null)
            smokeAnimator = heldSmokeObject.GetComponent<Animator>();
        else
            Debug.LogWarning("[연기] SmokeObject를 찾을 수 없습니다.");
    }

    void Update()
    {
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
                    delta = -tempChangePerSec * Time.deltaTime;
                    break;
            }
            Temperature = Mathf.Clamp(Temperature + delta, 0f, 100f);
            Debug.Log($"[온도] 상태: {currentState}, 현재 온도: {Temperature:F2}");
            UpdateNeedleRotation();
        }
        /*
        if (!isDragging)
        {
            switch (currentState)
            {
                case KettleState.OnFire:
                    Temperature = Mathf.Min(100, Temperature + Mathf.RoundToInt(tempChangePerSec * Time.deltaTime));
                    Debug.Log($"🔥 OnFire: 온도 증가 → {Temperature}");
                    break;

                case KettleState.OnHook:
                    Temperature = Mathf.Max(0, Temperature - Mathf.RoundToInt(tempChangePerSec * Time.deltaTime));
                    Debug.Log($"🧊 OnHook: 온도 감소 → {Temperature}");
                    break;

                case KettleState.Dragging:
                    Debug.Log($"✋ Dragging 중 → 온도 고정: {Temperature}");
                    break;
            }
        }
        */

        //연기처리_투명도 반영 x, 다시 해야 함.
        if (heldSmokeObject != null && kettleSpoutPosition != null)
        {
            heldSmokeObject.transform.position = kettleSpoutPosition.position;

            bool shouldShow = Temperature >= 70f;

            if (!heldSmokeObject.activeSelf)
                heldSmokeObject.SetActive(true); // Animator 작동용

            if (smokeAnimator != null)
            {
                smokeAnimator.SetBool("isSmoking", shouldShow);

                var spriteRenderer = heldSmokeObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.enabled = shouldShow; // 여기서 이미지 자체도 On/Off

                Debug.Log($"[연기] 온도: {Temperature}, isSmoking: {shouldShow}");
            }
        }


    }


    void UpdateNeedleRotation()
    {
        float t = Temperature / 100f; // 0~1로 정규화
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        gaugeNeedle.transform.localEulerAngles = new Vector3(0, 0, angle);
    }


    void OnMouseDown()
    {
        if (Hand.Instance.handIngredient != null) return;
        isDragging = true;
        currentState = KettleState.Dragging;
        dragOffset = transform.position - GetMouseWorldPos();
        cachedTemperature = Temperature;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        transform.position = GetMouseWorldPos() + dragOffset;
    }

    void OnMouseUp()
    {
        isDragging = false;
        /* 위치 확인용...
                Debug.Log($"[좌표] 주전자 위치: {transform.position}");
                Debug.Log($"[좌표] 손잡이 위치: {kettleHandlePosition.position}");
                Debug.Log($"[좌표] 주둥이 위치: {kettleSpoutPosition.position}");
                Debug.Log($"[좌표] 고리 위치: {hookPosition.position}");
                Debug.Log($"[좌표] 다병 위치: {teapotPosition.position}");
                Debug.Log($"[좌표] 화로 위치: {firePosition.position}");
        */
        float distToTeapot = Vector3.Distance(kettleSpoutPosition.position, teapotPosition.position);
        float distToHook = Vector3.Distance(kettleHandlePosition.position, hookPosition.position);
        float distToFire = Vector3.Distance(transform.position, firePosition.position);
        /*
                Debug.Log($"[거리] 다병까지 거리: {distToTeapot}");
                Debug.Log($"[거리] 고리까지 거리: {distToHook}");
                Debug.Log($"[거리] 화로까지 거리: {distToFire}");
        */
        bool poured = false;

        if (distToTeapot <= pourRadius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(kettleSpoutPosition.position, pourRadius);
            foreach (var hit in hits)
            {
                TeaPot pot = hit.GetComponent<TeaPot>();
                if (pot != null && cachedTemperature >= 50 && cachedTemperature <= 100)
                {
                    pot.PourWater(cachedTemperature);
                    poured = true;
                    break;
                }
            }
        }

        if (distToHook <= hookSnapDistance)
        {
            Vector3 offset = transform.position - kettleHandlePosition.position;
            transform.position = hookPosition.position + offset;
            currentState = KettleState.OnHook;
            Debug.Log("[상태 변경] 고리에 걸림 → OnHook 상태");
        }
        else if (distToTeapot <= pourRadius)
        {
            // 물 붓기 시도만 하고, 상태 변경 없음
            Debug.Log("[행동] 다병에 물 붓기 시도");
        }
        else
        {
            if (distToFire > hookSnapDistance)
                Debug.Log("잘못된 위치 드롭 → 화로로 복귀");
            SetToFire();
        }

        if (distToTeapot <= pourRadius && !poured)
            Debug.Log("물 붓기 실패: 거리 부족 또는 온도 조건 미달");
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = 10f;
        return Camera.main.ScreenToWorldPoint(mouse);
    }

    public void SetToFire()
    {
        currentState = KettleState.OnFire;
        transform.position = firePosition.position;
    }

    //참고용으로 보려고 색깔 입혀놓음
    void OnDrawGizmosSelected()
    {
        if (hookPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hookPosition.position, hookSnapDistance);
        }

        if (teapotPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(teapotPosition.position, pourRadius);
        }

        if (kettleSpoutPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(kettleSpoutPosition.position, pourRadius);
        }

        if (kettleHandlePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kettleHandlePosition.position, hookSnapDistance);
        }
    }
}
