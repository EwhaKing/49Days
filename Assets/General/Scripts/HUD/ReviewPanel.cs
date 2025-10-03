using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviewPanel : MonoBehaviour
{
    public GameObject reviewPanel;
    public void ShowReviewPanel()
    {
        if (reviewPanel != null)
        {
            reviewPanel.SetActive(true);
        }
    }

public void GoToBeginningScene()
    {
        GameFlowManager.LoadScene(GameFlowManager.START_SECENE_NAME);
    }
}
