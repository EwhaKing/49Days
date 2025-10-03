using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ItemDropZone : MonoBehaviour, IDropHandler
{
    public static event Action OnItemDroppedOnBackground;

    public void OnDrop(PointerEventData eventData)
    {
        // 기존 쓰레기통과 동일한 이벤트를 발생시켜 InventoryUI가 반응하게 함
        OnItemDroppedOnBackground?.Invoke();
    }
}