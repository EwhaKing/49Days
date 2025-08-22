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
                // 씬에서 NPC 오브젝트를 이름으로 찾기
                var npcObj = GameObject.Find(speaker);
                if (npcObj != null)
                {
                    npcPresenter.SetTargetTransform(npcObj.transform);
                }
                else
                {
                    Debug.LogWarning($"NPC 오브젝트 '{speaker}'를 씬에서 찾을 수 없습니다.");
                }
            }
        }

        if (targetPresenter == null)
            throw new System.InvalidOperationException($"'{speaker}'에 해당하는 DialoguePresenter가 할당되지 않았습니다.");

        return targetPresenter.RunLineAsync(line, cancellationToken);
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        // 플레이어만 옵션 처리 가능
        if (playerPresenter != null)
            return playerPresenter.RunOptionsAsync(dialogueOptions, cancellationToken);

        throw new System.InvalidOperationException("옵션을 처리할 Presenter가 할당되지 않았습니다.");
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;
}
