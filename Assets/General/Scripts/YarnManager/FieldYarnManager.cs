using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class FieldYarnManager : SceneSingleton<FieldYarnManager>
{
    [SerializeField] DialogueRunner runner;

    [SerializeField] FieldDialoguePresenter fieldDialoguePresenter;

    void Start()
    {
        runner.AddCommandHandler<string, string>("change_sprite", ChangeNpcSprite);
    }

    public void ChangeNpcSprite(string npcName, string poseName)
    {
        fieldDialoguePresenter.ChangeCharacterPose(poseName);
    }
}
