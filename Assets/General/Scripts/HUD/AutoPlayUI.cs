using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AutoPlayUI : MonoBehaviour
{
    [SerializeField] private GameObject autoPlayButton;
    private string kitchenSceneName = "Kitchen";
    private string teaHouseFrontSceneName = "TeaHouseFront";
    private string fieldSceneName = "FieldPoC";

    void Start()
    {
        if (SceneManager.GetActiveScene().name == kitchenSceneName)
        {
            gameObject.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == teaHouseFrontSceneName)
        {
            gameObject.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name == fieldSceneName)
        {
            gameObject.SetActive(false);
        }
    }
}
