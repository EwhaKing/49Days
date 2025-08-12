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

    public bool IsSelected { get; private set; }
    private Vector2 originalPosition;
    private Coroutine animationCoroutine;

    void Awake()
    {
        if (targetRect == null)
        {
            targetRect = GetComponent<RectTransform>();
        }
        // 책갈피 토글의 초기 위치를 저장함
        originalPosition = targetRect.anchoredPosition;
    }

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected)
        {
            AnimateUp();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected)
        {
            AnimateDown();
        }
    }

    // 탭이 비활성화될 때 자동으로 호출 - 자꾸 호버링이 남은 상태로 로드되어서...^ ^
    void OnDisable()
    {
        // 선택된 상태가 아닌 탭은 원래 위치로 초기화.
        if (!IsSelected)
        {
            StopAnimation();
            targetRect.anchoredPosition = originalPosition;
        }
    }

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
        StopAnimation();
        animationCoroutine = StartCoroutine(AnimatePosition(targetPos));
    }

    private void StopAnimation()
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
