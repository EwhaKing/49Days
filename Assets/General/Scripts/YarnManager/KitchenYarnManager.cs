using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class KitchenYarnManager : MonoBehaviour
{
    [SerializeField] DialogueRunner tutorialRunner;
    [SerializeField] Transform transformCyrus;
    [SerializeField] CameraSmoothShift cameraSmoothShift;

    OrderManager orderManager;

    void Start()
    {
        tutorialRunner.AddCommandHandler<float, float>("move_cyrus", MoveCyrus);
        tutorialRunner.AddCommandHandler("move_camera", MoveCamera);
        tutorialRunner.AddCommandHandler<string>("go_teahouse_front", GoTeaHouseFront);

        orderManager = OrderManager.Instance;

        if(orderManager.kitchenNodeTitle != string.Empty)
        {
            tutorialRunner.onDialogueComplete.AddListener(() => {
                orderManager.kitchenNodeTitle = string.Empty;
                tutorialRunner.gameObject.SetActive(false);
            });

            tutorialRunner.gameObject.SetActive(true);
            tutorialRunner.StartDialogue(orderManager.kitchenNodeTitle);
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

    void GoTeaHouseFront(string nodeTitle)
    {
        orderManager.SetAfterNodeTitle(nodeTitle);
        GameFlowManager.LoadScene(GameFlowManager.TEA_HOUSE_FRONT_SCENE_NAME);
    }
}
