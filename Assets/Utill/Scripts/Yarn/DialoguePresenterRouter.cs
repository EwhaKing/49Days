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
                    Debug.LogWarning($"NPC 오브젝트 '{line.CharacterName}'를 씬에서 찾을 수 없습니다.");
                }
            }
        }

        if (targetPresenter == null)
            throw new System.InvalidOperationException($"'{line.CharacterName}'에 해당하는 DialoguePresenter가 할당되지 않았습니다.");

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
