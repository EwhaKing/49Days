using UnityEngine;
using System.Collections;

public class UIFadeInOnEnable : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;
    private float alpha;

    private void OnEnable()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        alpha = canvasGroup.alpha;
        canvasGroup.alpha = 0f;
        StartCoroutine(FadeInCoroutine());
    }
    private IEnumerator FadeInCoroutine()
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, alpha, t);
            yield return null;
        }
    }
}
