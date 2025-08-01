using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class YarnManager : SceneSingleton<YarnManager>
{
    [SerializeField] DialogueRunner runner;
    [SerializeField] Image fadeImage;
    
    void Start()
    {
        runner.AddCommandHandler<string>("fade_in", FadeIn);
        runner.gameObject.SetActive(false);
        runner.onDialogueComplete.AddListener(EndDialogue);
    }

    public void RunDialogue(string nodeTitle)
    {
        runner.gameObject.SetActive(true);

        runner.StartDialogue(nodeTitle);
    }

    IEnumerator FadeIn(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>($"Arts/{spriteName}");
        fadeImage.sprite = sprite;
        fadeImage.SetNativeSize();
        fadeImage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(fadeImage.GetComponent<UIFadeInOnEnable>().fadeDuration);
    }

    void EndDialogue()
    {
        fadeImage.gameObject.SetActive(false);
        runner.gameObject.SetActive(true);
    }
}
