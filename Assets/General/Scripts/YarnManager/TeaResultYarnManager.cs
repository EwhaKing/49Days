using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class TeaResultYarnManager : SceneSingleton<TeaResultYarnManager>
{
    [SerializeField] DialogueRunner runner;
    [SerializeField] LineAdvancer lineAdvancer;
    [SerializeField] Image fadeImage;

    [SerializeField] private DialogueInputHandler dialogueInputHandler;

    void OnEnable()
    {
        dialogueInputHandler.OnDialogueContinueRequested += lineAdvancer.RequestLineHurryUp;
    }

    void OnDestroy()
    {
        dialogueInputHandler.OnDialogueContinueRequested -= lineAdvancer.RequestLineHurryUp;
    }
    
    void Start()
    {
        runner.AddCommandHandler<string>("fade_in", FadeIn);
        runner.AddCommandHandler<int>("price", SetPrice);
        runner.gameObject.SetActive(false);
        runner.onDialogueComplete.AddListener(EndDialogue);
    }

    public void RunDialogue(string nodeTitle)
    {
        UIManager.Instance.BlockingUIOn(runner.gameObject);

        runner.StartDialogue(nodeTitle);
    }

    public bool HasNode(string nodeTitle)
    {
        return runner.Dialogue.NodeExists(nodeTitle);
    }

    IEnumerator FadeIn(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>($"Arts/{spriteName}");
        fadeImage.sprite = sprite;
        fadeImage.SetNativeSize();
        UIManager.Instance.BlockingUIOn(fadeImage.gameObject);
        yield return new WaitForSecondsRealtime(fadeImage.GetComponent<UIFadeInOnEnable>().fadeDuration);
    }

    void SetPrice(int price)
    {
        OrderManager.Instance.SetPrice(price);
    }

    void EndDialogue()
    {
        UIManager.Instance.BlockingUIOff(runner.gameObject);
        UIManager.Instance.BlockingUIOff(fadeImage.gameObject);
        
        GameFlowManager.Instance.FinishKitchen();
    }
}
