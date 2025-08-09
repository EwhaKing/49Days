using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬마다 하나씩 존재하는 UI 매니저입니다. <br/>
/// </summary>
public class UIManager : SceneSingleton<UIManager>
{
    private HashSet<GameObject> blockingUISet = new HashSet<GameObject>();

    /// <summary>
    /// 키입력을 막아야 하는 UI요소를 active할 때 반드시 이 함수를 통하기
    /// </summary>
    /// <param name="blockingUIObject">해당 UI 오브젝트</param>
    public void BlockingUIOn(GameObject blockingUIObject)
    {
        blockingUISet.Add(blockingUIObject);
        blockingUIObject.SetActive(true);
    }

    /// <summary>
    /// 키입력을 막아야 하는 UI요소를 deactive 할 때 반드시 이 함수를 통하기
    /// </summary>
    /// <param name="blockingUIObject">해당 UI 오브젝트</param>
    public void BlockingUIOff(GameObject blockingUIObject)
    {
        blockingUISet.Remove(blockingUIObject);
        blockingUIObject.SetActive(false);
    }

    /// <summary>
    /// 현재 UI가 키입력을 막고 있는지 여부를 반환합니다. <br/>
    /// UI가 하나라도 active되어 있다면 true를 반환합니다. <br/>
    /// *키입력을 사용할 때 반드시 이 함수를 통해 UI가 활성화되어 있는지 확인해야 합니다.* <br/>
    /// </summary>
    /// <returns></returns>
    public bool IsBlockedByUI()
    {
        if (blockingUISet.Count == 0)
        {
            return false;
        }

        return true;
    }
}
