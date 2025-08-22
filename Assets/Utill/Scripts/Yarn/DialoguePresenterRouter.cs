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

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken cancellationToken)
    {
        var speaker = line.CharacterName;
        DialoguePresenterBase? targetPresenter = null;

        if (speaker == "Player")
        {
            targetPresenter = playerPresenter;
        }
        else
        {
            targetPresenter = npcPresenter;
            if (npcPresenter != null)
            {
                // ������ NPC ������Ʈ�� �̸����� ã��
                var npcObj = GameObject.Find(speaker);
                if (npcObj != null)
                {
                    npcPresenter.SetTargetTransform(npcObj.transform);
                }
                else
                {
                    Debug.LogWarning($"NPC ������Ʈ '{speaker}'�� ������ ã�� �� �����ϴ�.");
                }
            }
        }

        if (targetPresenter == null)
            throw new System.InvalidOperationException($"'{speaker}'�� �ش��ϴ� DialoguePresenter�� �Ҵ���� �ʾҽ��ϴ�.");

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
