using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Kettle : MonoBehaviour, IPointerEnterHandler, IDragHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private float maxAngle = 150f; // -355f=5f
    private float minAngle = -150f; // -72f=288f

    enum KettleState { OnFire, OnHook, Dragging }
    KettleState currentState = KettleState.OnFire;

    public float Temperature { get; private set; } = 100;

    [SerializeField] float tempChangePerSec = 2f;
    [SerializeField] float pourRadius = 1f;

    //고리에 걸기 위한 거리 판단
    [SerializeField] float hookSnapDistance = 1.2f;
    //화로까지의 거리 판단
    [SerializeField] float stoveSnapDistance = 0.3f;

    [SerializeField] Transform gaugeNeedle;
    [SerializeField] Transform stovePosition;
    [SerializeField] Transform hookPosition;
    [SerializeField] Transform teapotPosition;
    [SerializeField] Transform kettleHandlePosition; //주전자 손잡이 위치 (고리에 걸기 위하여)
    [SerializeField] Transform kettleSpoutPosition; //주전자 주둥이 위치 (다병에 붓기 위하여)
    [SerializeField] Transform kettleBottomPosition;
    [SerializeField] float smokeFadeSpeed = 0.2f; // 연기 투명도 변화 속도 (초당 변화량)
    [SerializeField] GameObject highlightSprite; // 하이라이트용 스프라이트 오브젝트

    //주전자 회전 관련 변수들 
    [SerializeField] float pourDuration = 2f;
    [SerializeField] float pourAngle = 15f; // 시계 방향 기울기
    bool isPouring = false;
    Quaternion originalRotation;

    //물 파티클 관련 변수들
    [SerializeField] private ParticleSystem waterParticle; // Inspector에서 할당
                                                           //kettlespoutposition도 사용

    public bool IsPouring => isPouring;


    public TeaPot teapot; // Inspector에서 할당
    GameObject heldSmokeObject;
    Animator smokeAnimator;

    Vector3 dragOffset;
    bool isDragging = false;
    float cachedTemperature = -1;

    void Start()
    {
        SetToFire(); // 시작 시 화로 위치로 이동

        // 하이라이트 비활성화
        highlightSprite.SetActive(false);

        // SmokeObject와 Animator 찾기
        heldSmokeObject = transform.Find("SmokeObject")?.gameObject;
        if (heldSmokeObject != null)
            smokeAnimator = heldSmokeObject.GetComponent<Animator>();
        else
            Debug.LogWarning("[연기] SmokeObject를 찾을 수 없습니다.");


        if (stovePosition == null)
            stovePosition = GameObject.Find("stovePosition")?.transform;

        if (hookPosition == null)
            hookPosition = GameObject.Find("kettleHookPosition")?.transform;

        if (teapotPosition == null)
            teapotPosition = GameObject.Find("pourPosition")?.transform;

        if (teapot == null)
            teapot = GameObject.FindObjectOfType<TeaPot>();
    }

    void Update()
    {
        //waterparticle은 주둥이 위치를 따라간다
        if (kettleSpoutPosition != null && waterParticle != null)
        {
            waterParticle.transform.position = kettleSpoutPosition.position + new Vector3(-0.1f, 0.05f, 0f); // 주둥이 위치에 약간 아래 위치
            waterParticle.transform.rotation = kettleSpoutPosition.rotation;
        }

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
            // Debug.Log($"[온도] 상태: {currentState}, 현재 온도: {Temperature:F2}");
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


    public void OnPointerDown(PointerEventData eventData) //누를 때
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
    public void OnDrag(PointerEventData eventData)
    {
        //물 붓는 동안에는 움직이지 마세요 + 드래그 안 하는 중이면 함수 실행시키지 마세요.(당연함)
        if (isPouring || !isDragging) return;

        // 드래그 중일 때 주전자의 위치를 마우스 위치로 업데이트
        transform.position = GetMouseWorldPos() + dragOffset;

        GetComponent<SpriteRenderer>().sortingOrder = 9;
        highlightSprite.GetComponent<SpriteRenderer>().sortingOrder = 9;
    }

    public void OnPointerUp(PointerEventData eventData) //땔 때
    {
        //물 붓는 동안에는 움직이지 마세요
        if (isPouring) return;

        isDragging = false;

        float distToTeapot = Vector3.Distance(kettleSpoutPosition.position, teapot.pourPosition.position);
        float distToHook = Vector3.Distance(kettleHandlePosition.position, hookPosition.position);
        float distToFire = Vector3.Distance(kettleBottomPosition.position, stovePosition.position);

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
                    bool success = pot.PourWater(cachedTemperature); // 물 이미 부었나?

                    if (success)
                    {
                        //삭제할 로그
                        Debug.Log("✅ 다병에 물 붓기 시도");
                        //주전자 위치를 다병의 지정된 위치로 강제 이동
                        Vector3 offset = transform.position - kettleSpoutPosition.position;
                        transform.position = teapot.pourPosition.position + offset;

                        StartCoroutine(PourWaterAnimation(pot)); // 물 붓기 애니메이션은 성공할 때만 실행
                        triedPour = true;
                    }
                    else
                    {
                        Debug.Log("물 붓기 실패: 이미 다병에 물이 있음");
                    }
                    break;
                }
            }
        }

        // 2. 물 안 부었고, 고리 반경 안이면 고리에 걸기
        if (!triedPour && distToHook <= hookSnapDistance)
        {
            //삭제할 로그
            Debug.Log("✅ 고리에 걸기 시도");
            Vector3 offset = transform.position - kettleHandlePosition.position;
            transform.position = hookPosition.position + offset;
            currentState = KettleState.OnHook;
        }

        // 3. 물도 못 붓고 고리도 아니면 화로로 복귀
        else if (!triedPour)
        {
            if (distToFire > stoveSnapDistance)
            {
                SetToFire();
                // 삭제할 로그
                Debug.Log("✅ 화로로 복귀 시도");
            }

        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("마우스오버함.");
        if (Hand.Instance.handIngredient != null)
            return;
        highlightSprite.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlightSprite.SetActive(false);
    }

    Vector3 GetMouseWorldPos()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane);

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f; // 2D 고정
        return worldPosition;
    }

    public void SetToFire()
    {
        // Debug.Log($"[🔥 위치 로그] kettleBottomPosition.localPosition: {kettleBottomPosition.localPosition}");
        // Debug.Log($"[🔥 위치 로그] kettleBottomPosition.position (world): {kettleBottomPosition.position}");
        // Debug.Log($"[🔥 위치 로그] transform.position (kettle 본체): {transform.position}");
        // Debug.Log($"[🔥 위치 로그] stovePosition.position: {stovePosition.position}");
        // kettleBottomPosition이 stovePosition 위치에 정확히 맞도록 KettleObject의 위치 조정
        transform.position += stovePosition.position - kettleBottomPosition.position;
        currentState = KettleState.OnFire;
        GetComponent<SpriteRenderer>().sortingOrder = 3;
    }

    // 주전자에 물 붓기 애니메이션 함수
    IEnumerator PourWaterAnimation(TeaPot pot)
    {
        isPouring = true;
        isDragging = false;
        currentState = KettleState.Dragging;

        // 정렬 순서 낮게 조정
        GetComponent<SpriteRenderer>().sortingOrder = 6;
        highlightSprite.GetComponent<SpriteRenderer>().sortingOrder = 6;


        // 연기 알파를 0으로 줄이기 시작
        StartCoroutine(FadeSmokeTo(0f, smokeFadeSpeed * 3f));

        Quaternion originalRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, pourAngle);
        float elapsed = 0f;

        //물 붓기, 주전자 기울이기
        while (elapsed < pourDuration)
        {
            float t = elapsed / pourDuration;
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, t);

            pot.UpdatePourProgress(t); // 다병에게 진행도 전달

            elapsed += Time.deltaTime;

            // 기울인 뒤 0.5초 후에 파티클 시작
            if (elapsed >= 0.4f && !waterParticle.isPlaying)
            {
                waterParticle.GetComponent<Renderer>().sortingOrder = 7;
                ConfigureWaterParticleVelocity();
                waterParticle.Play();
            }

            yield return null;
        }
        transform.rotation = targetRotation;

        pot.PourWater(cachedTemperature);

        yield return new WaitForSeconds(0.4f); // n초 정지

        // // 파티클 생성 종료
        // if (waterParticle != null && waterParticle.isPlaying)
        // {
        //     waterParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // }
        elapsed = 0f;

        while (elapsed < pourDuration)
        {
            transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, elapsed / pourDuration);
            elapsed += Time.deltaTime;
            if (elapsed > 0.4f && waterParticle != null && waterParticle.isPlaying)
            {
                waterParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            yield return null;
        }
        transform.rotation = originalRotation;

        // 온도 기반 알파값 계산
        float targetAlpha = 0f;
        if (Temperature >= 85f) targetAlpha = 1f;
        else if (Temperature >= 70f) targetAlpha = (Temperature - 70f) / 15f;
        // 연기 알파를 다시 복원
        StartCoroutine(FadeSmokeTo(targetAlpha, smokeFadeSpeed * 3f));

        // 정렬 순서 원래대로 복원
        GetComponent<SpriteRenderer>().sortingOrder = 3;

        isPouring = false;
        // 애니메이션 끝났으니 화로로 복귀(0.2초만 있다가)
        yield return new WaitForSeconds(0.5f);

        teapot.SetBrewingState(); // 다병 상태를 Brewing으로 변경

        SetToFire();
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

    //유니티 내에서 조정이 안 돼서, 코드로 조절(파티클 시스템 속도 조절)
    void ConfigureWaterParticleVelocity()
    {
        if (waterParticle == null) return;

        var velocityOverLifetime = waterParticle.velocityOverLifetime;
        velocityOverLifetime.enabled = true;

        // x축 속도 곡선: -2 → 0
        AnimationCurve xCurve = new AnimationCurve();
        xCurve.AddKey(0f, -0.8f);
        xCurve.AddKey(1f, 0f);

        // y축 속도 곡선: 0 → -1.5
        AnimationCurve yCurve = new AnimationCurve();
        yCurve.AddKey(0f, -1f);
        yCurve.AddKey(1f, -3f);

        // z축도 동일한 모드 (Curve)로 맞춰야 함
        AnimationCurve zCurve = new AnimationCurve();
        zCurve.AddKey(0f, 0f);
        zCurve.AddKey(1f, 0f);

        // 모두 Curve 모드로 설정
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(1f, xCurve);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, yCurve);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(1f, zCurve); // 중요
    }



    /// <summary>
    /// 연기 애니메이션 정지 (Animator bool 파라미터 'isSmoking'을 false로 설정)
    /// </summary>
    public void StopSmokeAnimation()
    {
        if (smokeAnimator != null)
            smokeAnimator.SetBool("isSmoking", false);
    }

    //pourradius는 어디까지인가?
    void OnDrawGizmos()
    {
        if (kettleSpoutPosition == null) return;

        Gizmos.color = Color.cyan; // 확인용 색상
        Gizmos.DrawWireSphere(kettleSpoutPosition.position, pourRadius);
    }


}