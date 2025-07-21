using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetOffset : MonoBehaviour
{

    [SerializeField] GameObject resetButton; // 리셋 버튼 오브젝트

    void OnMouseEnter()
    {
        bool wasHoldingIngredient = Hand.Instance.handIngredient != null;

        // ✅ 클릭 직전 손이 비어 있었을 때만 버튼 표시
        if (!wasHoldingIngredient)
        {
            resetButton?.SetActive(true);
        }

    }

    void OnMouseExit()
    {
        resetButton.SetActive(false);
    }

}
