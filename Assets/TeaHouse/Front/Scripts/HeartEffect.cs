using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartEffect : MonoBehaviour
{
    public enum HeartType { Full, Broken }

    [Header("UI 연결")]
    [SerializeField] private Image leftHeartImage;
    [SerializeField] private Image rightHeartImage;

    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float stayDuration = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float moveDistance = 50f;

    [Header("깨진 하트 설정")]
    [SerializeField] private float brokenHeartSpacing = 30f;

    private Vector3 initialLeftPos;
    private Vector3 initialRightPos;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (leftHeartImage != null) initialLeftPos = leftHeartImage.rectTransform.localPosition;
        if (rightHeartImage != null) initialRightPos = rightHeartImage.rectTransform.localPosition;
    }

    public void ShowEffect(HeartType type)
    {
        StartCoroutine(EffectRoutine(type));
    }

    private IEnumerator EffectRoutine(HeartType type)
    {
        SetAlpha(0);
        ArrangeHearts(type);

        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition + new Vector2(0, moveDistance);

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeInDuration);
            SetAlpha(progress);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, progress * 0.5f);
            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);
        
        elapsedTime = 0f;
        startPosition = rectTransform.anchoredPosition; // 현재 위치에서 다시 시작
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            SetAlpha(1 - progress);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, progress);
            yield return null;
        }

        Destroy(gameObject);
    }
    
    private void ArrangeHearts(HeartType type)
    {
        if (type == HeartType.Full)
        {
            leftHeartImage.rectTransform.localPosition = initialLeftPos;
            rightHeartImage.rectTransform.localPosition = initialRightPos;
        }
        else
        {
            leftHeartImage.rectTransform.localPosition = initialLeftPos - new Vector3(brokenHeartSpacing / 2, 0, 0);
            rightHeartImage.rectTransform.localPosition = initialRightPos + new Vector3(brokenHeartSpacing / 2, 0, 0);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color leftColor = leftHeartImage.color;
        leftColor.a = alpha;
        leftHeartImage.color = leftColor;

        Color rightColor = rightHeartImage.color;
        rightColor.a = alpha;
        rightHeartImage.color = rightColor;
    }
}
