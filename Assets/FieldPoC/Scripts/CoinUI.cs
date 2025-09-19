using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CoinUI : MonoBehaviour
{
    [SerializeField] TMP_Text coinText;
    [SerializeField] GameObject floatingTextObj;
    TMP_Text floatingText;
    CanvasGroup floatingCanvasGroup;

    void Start()
    {
        floatingText = floatingTextObj.GetComponent<TMP_Text>();
        floatingCanvasGroup = floatingTextObj.GetComponent<CanvasGroup>();

        coinText.text = GameManager.Instance.GetMoney().ToString("N0");
        
        GameManager.Instance.onMoneyChanged += AddCoins;
    }

    void OnDestroy()
    {
        GameManager.Instance.onMoneyChanged -= AddCoins;
    }

    void AddCoins(int diff, int current)
    {
        ShowFloatingText(diff);
        DOTween.To(() => current - diff, x => UpdateCoinText(x), current, 0.5f).SetEase(Ease.OutQuad);
    }

    void UpdateCoinText(int value)
    {
        coinText.text = value.ToString("N0"); // 천 단위 콤마
    }

    void ShowFloatingText(int amount)
    {
        floatingText.text = (amount >= 0 ? "+" : "-") + amount.ToString();
        floatingText.color = amount > 0 ? Color.green : Color.red;
        floatingCanvasGroup.alpha = 1;

        floatingText.rectTransform.anchoredPosition = coinText.rectTransform.anchoredPosition 
            + Vector2.up*coinText.rectTransform.sizeDelta.y*0.5f
            + Vector2.left*coinText.rectTransform.sizeDelta.x*0.1f;

        // 위로 이동 + 페이드 아웃
        floatingTextObj.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(floatingText.rectTransform.DOAnchorPos(floatingText.rectTransform.anchoredPosition + new Vector2(0, 50), 0.7f));
        seq.Join(floatingCanvasGroup.DOFade(0, 0.7f));
        seq.OnComplete(() => floatingTextObj.SetActive(false));
    }
}
