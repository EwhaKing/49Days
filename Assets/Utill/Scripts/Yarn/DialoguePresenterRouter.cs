#nullable enable
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;

public class DialoguePresenterRouter : DialoguePresenterBase
{
    public PlayerDialoguePresenter? playerPresenter;
    public NpcDialoguePresenter? npcPresenter;
    public static bool isOptionPanelActive { get; set; } = false;

    private GameObject? npcObj;
    private string? currentSpeaker;

    public void SetSpeaker(string? speaker)
    {
        currentSpeaker = speaker ?? string.Empty;
    }

    void Update()
    {
        if (npcPresenter != null && !string.IsNullOrEmpty(currentSpeaker) && currentSpeaker != "Player")
        {
            var foundNpcObj = GameObject.Find(currentSpeaker);
            if (foundNpcObj != null && foundNpcObj != npcObj)
            {
                npcObj = foundNpcObj;
                npcPresenter.SetTargetTransform(npcObj.transform);
            }
        }
    }

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken cancellationToken)
    {
        SetSpeaker(line.CharacterName);
        DialoguePresenterBase? targetPresenter = null;

        if (line.CharacterName == "Player")
        {
            targetPresenter = playerPresenter;
        }
        else
        {
            targetPresenter = npcPresenter;
            if (npcPresenter != null)
            {
                var foundNpcObj = GameObject.Find(line.CharacterName);
                if (foundNpcObj != null)
                {
                    npcPresenter.SetTargetTransform(foundNpcObj.transform);
                }
                else
                {
                    Debug.LogWarning($"NPC ������Ʈ '{line.CharacterName}'�� ������ ã�� �� �����ϴ�.");
                }
            }
        }

        if (targetPresenter == null)
            throw new System.InvalidOperationException($"'{line.CharacterName}'�� �ش��ϴ� DialoguePresenter�� �Ҵ���� �ʾҽ��ϴ�.");

        return targetPresenter.RunLineAsync(line, cancellationToken);
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        // �÷��̾ �ɼ� ó�� ����
        if (playerPresenter != null)
            return playerPresenter.RunOptionsAsync(dialogueOptions, cancellationToken);

        throw new System.InvalidOperationException("�ɼ��� ó���� Presenter�� �Ҵ���� �ʾҽ��ϴ�.");
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;
}
