using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashCan : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData e) 
    {
        if (Hand.Instance.handIngredient != null)
        {
            Destroy(Hand.Instance.Drop());
            Debug.Log("쓰레기통에 재료를 버렸습니다.");
        }
    }
}
