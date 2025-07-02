using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private void OnMouseUp() 
    {
        if (Hand.Instance.handIngredient != null)
        {
            Destroy(Hand.Instance.Drop());
            Debug.Log("쓰레기통에 재료를 버렸습니다.");
        }
    }
}
