using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// 뭐하는 스크립트인가요?
// 토글 선택/비선택 시 책갈피가 위아래로 움직이는 애니메이션을 담당.
// 각 토글 오브젝트(e.g. InventoryToggle)에 붙여 작동.

public class TabAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("움직이게 만들 UI 오브젝트의 RectTransform을 연결")]
    public RectTransform targetRect;

    [Tooltip("책갈피 상승 높이")]
    public float moveAmount = 20f;

    [Tooltip("애니메이션 재생 시간")]
    public float animationDuration = 0.15f;

    public bool IsSelected { get; private set; }    // 선택되었나요~?
    private Vector2 originalPosition;               // 원래 위치 저장
    private Coroutine animationCoroutine;           // 현재 애니메이션 코루틴

    void Awake()
    {
        if (targetRect == null)
        {
            targetRect = GetComponent<RectTransform>();
        }
        // 책갈피 토글의 초기 위치를 저장함
        originalPosition = targetRect.anchoredPosition;
    }

    /// <summary>
    /// TabGroupManager에 의해 호출되어 탭의 선택 상태를 설정합니다.
    /// </summary>
    /// <param name="selected">선택 여부</param>
    /// <param name="animate">애니메이션 재생 여부</param>
    public void SetSelectionState(bool selected, bool animate)
    {
        IsSelected = selected;
        if (IsSelected)
        {
            if (animate) AnimateUp();
            else SetUp();
        }
        else
        {
            if (animate) AnimateDown();
            else SetDown();
        }
    }

    #region 마우스 이벤트
    public void OnPointerEnter(PointerEventData eventData)      // 마우스 포인터가 이 UI 요소 위로 올라왔을 때 호출
    {
        // 아직 선택되지 않은 탭 위에 마우스를 올렸을 때만 위로 올라가는 효과.
        if (!IsSelected)
        {
            AnimateUp();
        }
    }

    public void OnPointerExit(PointerEventData eventData)       // 마우스 포인터가 이 UI 요소에서 벗어났을 때 호출
    {
        // 선택되지 않은 탭에서 마우스가 벗어났을 때는 아래로 내려가는 효과.
        if (!IsSelected)
        {
            AnimateDown();
        }
    }
    #endregion

    public void AnimateUp()       // 토글을 위로 올리는 애니메이션 시작
    {
        StartAnimation(originalPosition + new Vector2(0, moveAmount));
    }

    public void AnimateDown()     // 토글을 아래로 내리는 애니메이션 시작
    {
        StartAnimation(originalPosition);
    }

    public void SetUp()         // 즉시 토글 위로 세팅
    {
        StopAnimation();
        targetRect.anchoredPosition = originalPosition + new Vector2(0, moveAmount);
    }

    public void SetDown()       // 즉시 토글 원위치 세팅
    {
        StopAnimation();
        targetRect.anchoredPosition = originalPosition;
    }

    private void StartAnimation(Vector2 targetPos)
    {
        StopAnimation();    // 새로운 애니메이션 시작하기 전에 기존 애니메이션 중지
        animationCoroutine = StartCoroutine(AnimatePosition(targetPos));
    }

    private void StopAnimation()    // 현재 재생 중인 애니메이션 코루틴 중지
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }

    // 지정한 위치로 토글을 부드럽게 이동시키는 코루틴
    private IEnumerator AnimatePosition(Vector2 targetPosition)
    {
        float time = 0;
        Vector2 startPosition = targetRect.anchoredPosition;
        while (time < animationDuration)
        {
            // Lerp를 이용해 시작 위치-목표 위치로 보간
            targetRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        // 애니메이션이 끝난 이후 목표 위치로 보정 후 고정
        targetRect.anchoredPosition = targetPosition;
    }
}
