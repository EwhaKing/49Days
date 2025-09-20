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

    private Button button;
    void Start()
    {
        if (GameFlowManager.IsInField())
        {
            iconImage = logMapIcon.GetComponent<Image>();
            iconImage.sprite = mapSprite;
            logMapIcon.SetActive(true);
        }
        else
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
    }
}
