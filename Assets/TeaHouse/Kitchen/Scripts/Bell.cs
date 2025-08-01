using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class Bell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
    bool isMouseOver;

    private void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        skipButton.onClick.AddListener(ShowMakedTea);
        image = GetComponent<SpriteRenderer>();
        originSprite = image.sprite;

        TeaPot.Instance.onStateBrewing += () => {if (isMouseOver) image.sprite = highlightSprite;};
    }

    public void OnPointerEnter(PointerEventData e)
    {
        isMouseOver = true;
        if (TeaPot.Instance.GetCurrentState() != TeaPot.State.Brewing) return;
        image.sprite = highlightSprite;
    }

    public void OnPointerExit(PointerEventData e)
    {
        isMouseOver = false;
        image.sprite = originSprite;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (TeaPot.Instance.GetCurrentState() != TeaPot.State.Brewing) return;
        tea = TeaPot.Instance.getTea();

        if (tea != null)
        {
            manufacturingCompletedUI.SetActive(true);
            PauseGame();
            makedTea = TeaMaker.MakeTea(tea);
            StartCoroutine(LatePlayCutScene());
            image.sprite = originSprite;
        } 
        else
        {
            Debug.LogError("State가 Brewing인데 차를 가져오지 못함");
        }
    }

    void PauseGame()
    {
        //Time.timeScale = 0f;
        AudioListener.pause = true; // 전체 오디오 일시정지

        // 벨소리만 계속 들리게
        audioSource.ignoreListenerPause = true;
        audioSource.Play();

        TeaPot.Instance.StopSmokeAnimation();        
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
        VideoPlayer vp = cutScene.GetComponent<VideoPlayer>();
        vp.url = System.IO.Path.Combine(Application.streamingAssetsPath, "pourtea.mp4");
        vp.loopPointReached += (VideoPlayer vp) => {
            if(!isSkipped) ShowMakedTea();
        };
    }

}
