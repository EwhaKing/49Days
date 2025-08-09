using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{


    //test
    private void Start()
    {
        StartCoroutine("test");
    }
    IEnumerator test(){
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("SampleScene");
    }
    public void testSave(){
        SaveLoadManager.Instance.SaveAllByDate(1);
    }
    public void testLoad(){
        SaveLoadManager.Instance.LoadAllByDate(1);
    }
}
