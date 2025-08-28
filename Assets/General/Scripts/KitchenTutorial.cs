using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class KitchenTutorial : MonoBehaviour
{
    [SerializeField] DialogueRunner tutorialRunner;
    void Start()
    {
        if(!GameManager.Instance.IsTutorialCompleted())
        {
            tutorialRunner.onDialogueComplete.AddListener(() => {
                UIManager.Instance.BlockingUIOff(tutorialRunner.gameObject);
                GameManager.Instance.TutorialComplete();
                tutorialRunner.enabled = false;
            });

            UIManager.Instance.BlockingUIOn(tutorialRunner.gameObject);
            tutorialRunner.StartDialogue("튜토리얼_주방");
        }
        else
        {
            tutorialRunner.gameObject.SetActive(false);
        }
    }
}
