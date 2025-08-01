using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResetOffset : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] GameObject resetButton; // 리셋 버튼 오브젝트
    [SerializeField] Kettle kettle; // 인스펙터에서 할당


    bool isVisible = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (kettle != null && kettle.IsPouring) return; // 🔒 물 붓는 중이면 버튼 비활성화 유지

        if (!isVisible)
        {
            isVisible = true;
            resetButton?.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isVisible) return;
        isVisible = false;
        resetButton?.SetActive(false);

    }


}
