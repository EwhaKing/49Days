using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Customer : MonoBehaviour
{
    [Header("자식 오브젝트 연결")]
    [Tooltip("몸 스프라이트를 표시할 자식 오브젝트의 SpriteRenderer")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [Tooltip("눈 스프라이트를 표시할 자식 오브젝트의 SpriteRenderer")]
    [SerializeField] private SpriteRenderer eyesRenderer;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("퇴장 시 옆으로 이동할 거리")]
    [SerializeField] private Vector3 exitOffset = new Vector3(-0.5f, 0, 0);

    [Header("눈 깜빡임 설정")]  // 일단 만들긴 했는데 스프라이트 아트에서 전달 받은 뒤에 사용/수정 가능...
    [SerializeField] private float minBlinkInterval = 3f;
    [SerializeField] private float maxBlinkInterval = 7f;
    [SerializeField] private float blinkDuration = 0.1f;
    
    private CustomerData customerData;
    private CharacterPose currentPose;
    private Coroutine blinkCoroutine;
    private Coroutine currentActionCoroutine;
    private Color originalColor = Color.white;

    public void Initialize(CustomerData data)
    {
        customerData = data;
        ChangePose("무표정");  // 디폴트는 무표정?
    }

    public void GoToTarget(Transform target)
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        currentActionCoroutine = StartCoroutine(EnterRoutine(target.position));
    }

    // 스프라이트 변경할 때 이거 사용.
    public void ChangePose(string poseName)
    {
        if (customerData == null) return;

        // 이름에 맞는 감정 포즈를 찾아오기
        CharacterPose newPose = customerData.poses.Find(p => p.poseName == poseName);
        if (newPose != null)
        {
            currentPose = newPose;
            bodyRenderer.sprite = currentPose.bodySprite;
            eyesRenderer.sprite = currentPose.eyesOpenSprite;
        }
        else
        {
            Debug.LogWarning($"'{customerData.characterName}'에게 '{poseName}' 포즈가 없습니다.");
        }
    }

    private IEnumerator EnterRoutine(Vector3 targetPosition)
    {
        // 등장 연출: 이동하는 동안 검은색에서 원래 색으로 점차 변경
        Vector3 startPosition = transform.position;
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        
        SetRenderersColor(Color.black); // 검은 실루엣으로 시작

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 이동 거리에 비례하여 색상을 변경
            if (totalDistance > 0)
            {
                float coveredDistance = Vector3.Distance(startPosition, transform.position);
                float progress = Mathf.Clamp01(coveredDistance / totalDistance);
                SetRenderersColor(Color.Lerp(Color.black, originalColor, progress));
            }

            yield return null;
        }

        transform.position = targetPosition;
        SetRenderersColor(originalColor); // 최종적으로 원래 색상으로 고정
        
        // 눈 깜빡임 시작
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(delay);

            if (currentPose != null && currentPose.eyesClosedSprite != null)
            {
                eyesRenderer.sprite = currentPose.eyesClosedSprite;
                yield return new WaitForSeconds(blinkDuration);
                eyesRenderer.sprite = currentPose.eyesOpenSprite;
            }
        }
    }

    private void SetRenderersColor(Color color)
    {
        if (bodyRenderer != null) bodyRenderer.color = color;
        if (eyesRenderer != null) eyesRenderer.color = color;
    }
    
    // CustomerManager가 퇴장시킬 때 호출
    public void Exit()
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        currentActionCoroutine = StartCoroutine(ExitRoutine());
    }

    // 손님 퇴장 코루틴
    private IEnumerator ExitRoutine()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);  // 퇴장 시 눈 깜박임 멈춤

        float time = 0;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + exitOffset;
        Color startColor = bodyRenderer.color;
        Color endColor = Color.clear;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float progress = time / fadeDuration;
            // 위치 및 색상 보간
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);
            SetRenderersColor(Color.Lerp(startColor, endColor, progress));
            
            yield return null;
        }

        Destroy(gameObject);
    }
}
