#nullable enable
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;
using System.Collections.Generic;
using Yarn.Markup;

/// <summary>
/// Yarn Script의 화자에 따라 적절한 Presenter를 선택하여
/// 대화 흐름을 관리하는 라우터 클래스입니다.
/// 
/// Yarn Script에서 각 대사에 포함된 CharacterName을 기반으로,
/// 해당 캐릭터의 CharacterData를 찾아 Presenter에 할당하고, pose 정보 유무에 따라 Presenter를 자동으로 분기합니다.
/// 
/// E 액션 등으로 대화가 시작될 때, Router를 통해 StartDialogue를 호출하면
/// 이후 대사마다 화자에 맞는 Presenter와 캐릭터 데이터가 자동으로 할당되어 UI가 표시됩니다.
/// 때문에 사용할 때 E 액션으로 호출되는 Yarn Script만 StartDialogue로 호출하면 나머지는 Router가 자동 처리합니다.
/// 캐릭터 데이터 외부에서 따로 할당해줄 필요 없다는 뜻
/// </summary>
public class FieldDialoguePresenterRouter : DialoguePresenterBase
{
    public FieldDialoguePresenter? fieldPresenter;
    public FieldDialogueExtraPresenter? fieldExtraPresenter;

    // CharacterData 리스트를 인스펙터에서 할당
    public List<CharacterData>? characterDataList;
    private CharacterData? currentCharacterData;

    public static bool isOptionPanelActive { get; set; } = false;

    public void SetSpeaker(string? speaker)
    {
        if (string.IsNullOrEmpty(speaker))
        {
            currentCharacterData = null;
            return;
        }

        if (characterDataList != null)
        {
            var found = characterDataList.Find(c => c.characterName == speaker);
            currentCharacterData = found;
        }
        else
        {
            currentCharacterData = null;
        }
    }

    /// <summary>
    /// CharacterData에서 pose 정보가...
    /// 있다 -> FieldDialoguePresenter RunLineAsync() 호출
    /// 없다 -> FieldExtraDialoguePresenter RunLineAsync() 호출
    /// </summary>
    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken cancellationToken)
    {
        DialoguePresenterBase? targetPresenter = null;

        SetSpeaker(line.CharacterName);

        var characterData = currentCharacterData;
        bool hasPose = characterData != null
            && characterData.poses != null
            && characterData.poses.Count > 0;

        if (hasPose)
        {
            fieldPresenter?.SetCharacterData(characterData);
            targetPresenter = fieldPresenter;
        }
        else
        {
            targetPresenter = fieldExtraPresenter;
        }

        if (targetPresenter == null)
            throw new System.InvalidOperationException($"'{line.CharacterName}'에 해당하는 DialoguePresenter가 할당되지 않았습니다.");

        return targetPresenter.RunLineAsync(line, cancellationToken);
    }

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        if (fieldPresenter != null)
            return fieldPresenter.RunOptionsAsync(dialogueOptions, cancellationToken);

        throw new System.InvalidOperationException("옵션을 처리할 Presenter가 할당되지 않았습니다.");
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;

    public override YarnTask OnDialogueCompleteAsync()
    {
        fieldPresenter?.OnDialogueCompleteAsync();
        return YarnTask.CompletedTask;
    }
}
