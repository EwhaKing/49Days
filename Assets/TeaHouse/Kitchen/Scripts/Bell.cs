using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Video;

public class Bell : MonoBehaviour
{
    [SerializeField] GameObject manufacturingCompletedUI;
    [SerializeField] GameObject cutScene;
    [SerializeField] Button skipButton;
    [SerializeField] Sprite highlightSprite;
    Tea tea;
    MakedTea makedTea;
    AudioSource audioSource;
    SpriteRenderer image;
    Sprite originSprite;
    bool isSkipped;

    private void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        skipButton.onClick.AddListener(ShowMakedTea);
        image = GetComponent<SpriteRenderer>();
        originSprite = image.sprite;
    }
    private void OnMouseEnter()
    {
        image.sprite = highlightSprite;
    }

    private void OnMouseExit() 
    {
        image.sprite = originSprite;
    }

    private void OnMouseUp()
    {
        tea = TeaPot.Instance.getTea();

        if (tea != null)
        {
            manufacturingCompletedUI.SetActive(true);
            PauseGame();
            makedTea = TeaMaker.MakeTea(tea);
            StartCoroutine(LatePlayCutScene());
            
        }
    }

    void PauseGame()
    {
        //Time.timeScale = 0f;
        AudioListener.pause = true; // 전체 오디오 일시정지

        // 벨소리만 계속 들리게
        audioSource.ignoreListenerPause = true;
        audioSource.Play();
    }

    void ShowMakedTea()
    {
        isSkipped = true;
        cutScene.SetActive(false);

        string teaString = makedTea.TeaName.ToLowerString();
        if (makedTea.additionalIngredient != IngredientName.None)
            teaString += "_" + makedTea.additionalIngredient;

        Debug.Log(teaString);

        // 얀스피너로 makedTea이름 및 추가재료로 설명과 이미지 띄우기
        YarnManager.Instance.RunDialogue(teaString);
    }

    IEnumerator LatePlayCutScene()
    {
        yield return new WaitForSecondsRealtime(2f);
        cutScene.SetActive(true);
        cutScene.GetComponent<VideoPlayer>().loopPointReached += (VideoPlayer vp) => {
            if(!isSkipped) ShowMakedTea();
        };
    }

}
