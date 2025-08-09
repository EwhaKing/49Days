using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashCan : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] Sprite openedSprite;
    Sprite closedSprite;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on TrashCan.");
        }
        else
        {
            closedSprite = spriteRenderer.sprite;
        }
    }

    public void OnPointerClick(PointerEventData e) 
    {
        if (Hand.Instance.handIngredient != null)
        {
            Destroy(Hand.Instance.Drop());
            spriteRenderer.sprite = closedSprite;
            Debug.Log("쓰레기통에 재료를 버렸습니다.");
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (Hand.Instance.handIngredient != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        spriteRenderer.sprite = closedSprite;
    }
}
