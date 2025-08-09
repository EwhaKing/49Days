using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image charImage;
    // [SerializeField] private GameObject highlight; // 하이라이트 오브젝트 (필요 시 사용)

    private CharacterData data;
    private AffinityPanel panel;
    private Sprite unknownSprite;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Init(CharacterData data, Sprite unknown, AffinityPanel panel)
    {
        this.data = data;
        this.panel = panel;
        this.unknownSprite = unknown;

        if (charImage != null)
        {
            charImage.sprite = data.hasMet ? data.slotImage : unknownSprite;
        }

        // if (highlight != null) 
        //     highlight.SetActive(false); // 시작 시 하이라이트 꺼두기
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data != null && data.hasMet && panel != null)
            panel.ShowCharacter(data);
    }

    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (data != null && data.hasMet && highlight != null)
    //         highlight.SetActive(true);
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     if (highlight != null)
    //         highlight.SetActive(false);
    // }
}
