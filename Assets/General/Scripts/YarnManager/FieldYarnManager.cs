using System.Collections;
using System;
using UnityEngine;
using Yarn.Unity;

public class FieldYarnManager : SceneSingleton<FieldYarnManager>
{
    [SerializeField] DialogueRunner runner;
    [SerializeField] FieldDialoguePresenter fieldDialoguePresenter;

public event Action<SimpleStaticAgent> onDialogueStart;
public event Action<SimpleStaticAgent> onDialogueEnd;

//대화의 시작/끝 알리는 플래그 변수
    public bool IsDialogueRunning { get; private set; } = false;

    void Start()
    {
        runner = FindObjectOfType<DialogueRunner>();
        fieldDialoguePresenter = FindObjectOfType<FieldDialoguePresenter>();
        runner.AddCommandHandler<string, string>("change_sprite", ChangeNpcSprite);
        runner.onDialogueComplete.AddListener(EndDialogue);
    }

    public void RunDialogue(string nodeTitle, SimpleStaticAgent target)
    {
        CurrentTarget = target;
        IsDialogueRunning = true;  //대화 시작 플래그
        onDialogueStart?.Invoke(target);
        GameManager.Instance.onUIOn?.Invoke();
        runner.gameObject.SetActive(true);
        Debug.Log($"FieldYarnManager: RunDialogue {nodeTitle}");
        runner.StartDialogue(nodeTitle);
    }

    void EndDialogue()
    {
        onDialogueEnd?.Invoke(CurrentTarget);
        runner.gameObject.SetActive(false);
        IsDialogueRunning = false; //대화 종료 플래그
    }

    public SimpleStaticAgent CurrentTarget { get; private set; }

    public void ChangeNpcSprite(string npcName, string poseName)
    {
        fieldDialoguePresenter.ChangeCharacterPose(poseName);
    }
}
