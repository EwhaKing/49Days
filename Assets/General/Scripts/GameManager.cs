using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    private HashSet<GameObject> blockingUISet = new HashSet<GameObject>();

    /// <summary>
    /// 키입력을 막아야 하는 UI요소를 active할 때 반드시 호출
    /// </summary>
    /// <param name="blockingUIObject">해당 UI 오브젝트</param>
    public void BlockingUIOn(GameObject blockingUIObject)
    {
        blockingUISet.Add(blockingUIObject);
    }

    /// <summary>
    /// 키입력을 막아야 하는 UI요소를 deactive 할 때 반드시 호출
    /// </summary>
    /// <param name="blockingUIObject">해당 UI 오브젝트</param>
    public void BlockingUIOff(GameObject blockingUIObject)
    {
        blockingUISet.Remove(blockingUIObject);
    }

    public bool IsBlockedByUI()
    {
        if (blockingUISet.Count == 0)
        {
            return false;
        }

        return true;
    }

    //test
    private void Start()
    {
        StartCoroutine("test");
    }
    IEnumerator test(){
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("SampleScene");
    }
    public void testSave(){
        SaveLoadManager.Instance.SaveAllByDate(1);
    }
    public void testLoad(){
        SaveLoadManager.Instance.LoadAllByDate(1);
    }
}
