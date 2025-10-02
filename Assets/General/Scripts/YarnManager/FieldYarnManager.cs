using System.Collections;
using System;
using UnityEngine;
using Yarn.Unity;

public class FieldYarnManager : SceneSingleton<FieldYarnManager>
{
    [SerializeField] DialogueRunner runner;
    [SerializeField] FieldDialoguePresenter fieldDialoguePresenter;

    public event Action onDialogueStart;
    public event Action onDialogueEnd;

    void Start()
    {
        runner = FindObjectOfType<DialogueRunner>();
        fieldDialoguePresenter = FindObjectOfType<FieldDialoguePresenter>();
        runner.AddCommandHandler<string, string>("change_sprite", ChangeNpcSprite);
        runner.onDialogueComplete.AddListener(EndDialogue);
    }

    public void RunDialogue(string nodeTitle)
    {
        onDialogueStart?.Invoke();
        GameManager.Instance.onUIOn?.Invoke();
        runner.gameObject.SetActive(true);
        Debug.Log($"FieldYarnManager: RunDialogue {nodeTitle}");
        runner.StartDialogue(nodeTitle);
    }

    void EndDialogue()
    {
        onDialogueEnd?.Invoke();
        runner.gameObject.SetActive(false);
    }

    public void ChangeNpcSprite(string npcName, string poseName)
    {
        fieldDialoguePresenter.ChangeCharacterPose(poseName);
    }
}
