using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogMapUI : MonoBehaviour
{
    [SerializeField] private GameObject logMapIcon;
    [SerializeField] private Sprite logSprite;
    [SerializeField] private Sprite mapSprite;  // TODO: 맵 아이콘 스프라이트 교체 필요.
    private Image iconImage;

    private string kitchenSceneName = "Kitchen";
    private string teaHouseFrontSceneName = "TeaHouseFront";
    private string fieldSceneName = "FieldPoC";

    private Button button;
    void Start()
    {
        if (SceneManager.GetActiveScene().name == kitchenSceneName)
        {
            logMapIcon.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name == teaHouseFrontSceneName)
        {
            iconImage = logMapIcon.GetComponent<Image>(); 
            iconImage.sprite = logSprite;
            logMapIcon.SetActive(true);

            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                DialogueLogManager.Instance.ToggleLog();
            });

        }
        else if (SceneManager.GetActiveScene().name == fieldSceneName)
        {
            iconImage = logMapIcon.GetComponent<Image>();
            iconImage.sprite = mapSprite;
            logMapIcon.SetActive(true);
        }


    }
}
