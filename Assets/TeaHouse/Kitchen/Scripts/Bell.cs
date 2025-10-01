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
    VideoPlayer videoPlayer;
    SpriteRenderer image;
    Sprite originSprite;
    bool isSkipped;
    bool isMouseOver;

    private void Start() 
    {
        audioSource = GetComponent<AudioSource>();
        skipButton.onClick.AddListener(ShowMakedTea);
        image = GetComponent<SpriteRenderer>();
        videoPlayer = cutScene.GetComponent<VideoPlayer>();
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "pourtea.mp4");
        originSprite = image.sprite;

        TeaPot.Instance.onStateBrewing += () => {if (isMouseOver) image.sprite = highlightSprite;};
    }

    public void OnPointerEnter(PointerEventData e)
    {
        Tooltip.Instance.Show("종");

        isMouseOver = true;
        if (TeaPot.Instance.GetCurrentState() != TeaPot.State.Brewing) return;
        image.sprite = highlightSprite;
    }

    public void OnPointerExit(PointerEventData e)
    {
        Tooltip.Instance.Hide();
        Tooltip.Instance.HideFadeImmidately();
        isMouseOver = false;
        image.sprite = originSprite;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (TeaPot.Instance.GetCurrentState() != TeaPot.State.Brewing)
        {
            Tooltip.Instance.ShowFade("지금은 종을 칠 수 없습니다.");
            return;
        }

        tea = TeaPot.Instance.getTea();
        Debug.Assert(tea != null, "State가 Brewing인데 차를 가져오지 못함");

        UIManager.Instance.BlockingUIOn(manufacturingCompletedUI);
        PauseGame();
        makedTea = TeaMaker.MakeTea(tea);
        StartCoroutine(LatePlayCutScene());
        image.sprite = originSprite;
    }

    void PauseGame()
    {
        // AudioListener.pause = true; // 전체 오디오 일시정지

        // 벨소리만 계속 들리게
        audioSource.ignoreListenerPause = true;
        audioSource.Play();   
    }

    void ShowMakedTea()
    {
        isSkipped = true;
        cutScene.SetActive(false);
        RenderTexture rt = videoPlayer.targetTexture;
        Graphics.SetRenderTarget(rt);
        GL.Clear(true, true, Color.black);
        Graphics.SetRenderTarget(null);

        string teaString = makedTea.teaName.ToLowerString();
        if (makedTea.additionalIngredient != IngredientName.None)
            teaString += "_" + makedTea.additionalIngredient.ToLowerString();

        // 만약 얀스피너에 해당 노드가 없으면 알 수 없는 차로 처리
        if (!TeaResultYarnManager.Instance.HasNode(teaString))
        {
            makedTea.teaName = TeaName.Unknown;
            teaString = "unknown";
        }

        // 탭 레시피 해금
        if (makedTea.teaName != TeaName.Unknown)
            RecipeDescriptionManager.Instance.UnlockRecipeDescription(makedTea.teaName);

        // 만든 차 OrderManager에 저장
        OrderManager.Instance.SetMakedTea(makedTea);

        // 얀스피너로 makedTea이름 및 추가재료로 설명과 이미지 띄우기
        TeaResultYarnManager.Instance.RunDialogue(teaString);
    }

    IEnumerator LatePlayCutScene()
    {
        yield return new WaitForSecondsRealtime(2f);

        cutScene.SetActive(true);
        videoPlayer.loopPointReached += (VideoPlayer vp) => {
            if(!isSkipped) ShowMakedTea();
        };
    }

}
