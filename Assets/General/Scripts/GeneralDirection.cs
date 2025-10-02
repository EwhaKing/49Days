using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체에서 사용 가능한 연출
/// </summary>
public class GeneralDirection : SceneSingleton<GeneralDirection>
{
    [SerializeField] private UIFadeInOnEnable blackFadeImage;

    /// <summary>
    /// 전체 검은 화면으로 페이드인. 후에 페이드 아웃을 불러서 꺼야함
    /// </summary>
    /// <param name="duration"></param>
    public void FadeInBlack(float duration = 1f)
    {
        blackFadeImage.fadeDuration = duration;
        blackFadeImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// 검은 화면 페이드 아웃. 이전에 페이드 인을 불렀어야 함. <br/>
    /// 페이드 인이 진행중이라면 끝난 후에 페이드 아웃이 진행됨.
    /// </summary>
    public void FadeOutBlack(float duration = 1f)
    {
        blackFadeImage.fadeDuration = duration;
        blackFadeImage.FadeOutAndDisable();
    }

}
