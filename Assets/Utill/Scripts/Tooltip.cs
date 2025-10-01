using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : SceneSingleton<Tooltip>
{
    [SerializeField] GameObject normal;
    [SerializeField] GameObject fade;
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] float waitDuration = 1.5f;

    TMP_Text normalText;
    TextMeshProUGUI fadeText;
    CanvasGroup fadeCanvasGroup;
    RectTransform fadeRectTransform;
    RectTransform normalRectTransform;

    void Start()
    {
        normal.SetActive(false);
        fade.SetActive(false);
        normalText = normal.GetComponentInChildren<TMP_Text>();
        fadeText = fade.GetComponentInChildren<TextMeshProUGUI>();
        fadeCanvasGroup = fade.GetComponent<CanvasGroup>();
        fadeRectTransform = fade.GetComponent<RectTransform>();
        normalRectTransform = normal.GetComponent<RectTransform>();
    }

    public void Show(string text)
    {
        normalText.text = text;
        normal.SetActive(true);
        normalText.ForceMeshUpdate(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(normalText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(normalRectTransform);
    }
    public void Hide()
    {
        normal.SetActive(false);
    }

    public void ShowFade(string text)
    {
        StopAllCoroutines();
        fadeText.text = text;
        fadeCanvasGroup.alpha = 1f;
        fade.SetActive(true);
        fadeText.ForceMeshUpdate(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(fadeText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(fadeRectTransform);
        StartCoroutine(FadeOutAndDisableCoroutine());
    }
    public void HideFadeImmidately()
    {
        StopAllCoroutines();
        fade.SetActive(false);
    }

    private IEnumerator FadeOutAndDisableCoroutine()
    {
        yield return new WaitForSecondsRealtime(waitDuration);

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        fade.SetActive(false);
    }
}
