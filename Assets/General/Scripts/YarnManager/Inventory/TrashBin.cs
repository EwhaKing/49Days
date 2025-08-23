using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TrashBin : MonoBehaviour, IDropHandler
{
    public static event Action OnItemDroppedOnTrash;

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOnTrash?.Invoke();
    }
}
