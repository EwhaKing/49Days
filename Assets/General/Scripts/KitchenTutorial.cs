using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class KitchenTutorial : MonoBehaviour
{
    [SerializeField] DialogueRunner tutorialRunner;
    [SerializeField] Transform transformCyrus;
    [SerializeField] CameraSmoothShift cameraSmoothShift;

    void Start()
    {
        tutorialRunner.AddCommandHandler<float, float>("move_cyrus", MoveCyrus);
        tutorialRunner.AddCommandHandler("move_camera", MoveCamera);

        if(!GameManager.Instance.IsTutorialCompleted())
        {
            tutorialRunner.onDialogueComplete.AddListener(() => {
                UIManager.Instance.BlockingUIOff(tutorialRunner.gameObject);
                GameManager.Instance.TutorialComplete();
                tutorialRunner.enabled = false;
            });

            tutorialRunner.gameObject.SetActive(true);
            tutorialRunner.StartDialogue("튜토리얼_주방");
        }
        else
        {
            tutorialRunner.gameObject.SetActive(false);
        }
    }

    void MoveCyrus(float x, float y)
    {
        transformCyrus.position = new Vector3(x, y, transformCyrus.position.z);
    }

    IEnumerator MoveCamera()
    {
        cameraSmoothShift.OnMoveCamera();
        yield return cameraSmoothShift.transitionDuration;
    }
}
