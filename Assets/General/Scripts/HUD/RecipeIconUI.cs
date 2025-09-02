using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class RecipeIconUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Tooltip("인벤토리, 레시피 등 모든 탭 UI를 포함하는 최상위 패널")]
    [SerializeField] private GameObject commonPanel;
    
    [Header("스프라이트 설정")]
    [Tooltip("열림/닫힘/하이라이트 이미지를 표시할 Image 컴포넌트")]
    [SerializeField] private Image backgroundIamge;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;


    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("포인터가 레시피 아이콘 위에 올라감.");
        backgroundIamge.sprite = openSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("포인터가 레시피 아이콘에서 나감.");
        backgroundIamge.sprite = closedSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (commonPanel == null) return;
        commonPanel.SetActive(!commonPanel.activeSelf); // 현재 상태 반전
    }


}
