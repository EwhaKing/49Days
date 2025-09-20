using UnityEngine;
using System.Collections;

public class UIFadeInOnEnable : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;
    private float alpha;

    bool isFading = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Debug.Assert(canvasGroup != null, "CanvasGroup component is missing: " + gameObject.name);
        alpha = canvasGroup.alpha;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 0f;
        StartCoroutine(FadeInCoroutine());
    }

    public void FadeOutAndDisable()
    {
        StartCoroutine(FadeOutAndDisableCoroutine());
    }

    private IEnumerator FadeOutAndDisableCoroutine()
    {
        while (isFading) yield return null;
        isFading = true;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(alpha, 0f, t);
            yield return null;
        }
        gameObject.SetActive(false);
        isFading = false;
    }

    private IEnumerator FadeInCoroutine()
    {
        if (isFading) yield return null;
        isFading = true;

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, alpha, t);
            yield return null;
        }
        isFading = false;
    }
}
