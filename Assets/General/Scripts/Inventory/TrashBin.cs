using UnityEngine;
using UnityEngine.EventSystems;
using System;

// 이 스크립트를 쓰레기통 역할을 할 UI 패널에 붙여주세요.
// 해당 패널에는 Image나 RawImage 컴포넌트가 있어야 드롭을 감지할 수 있습니다.
public class TrashBin : MonoBehaviour, IDropHandler
{
    // 아이템이 이 위로 드롭되었을 때 InventoryUI에 알려주기 위한 이벤트
    public static event Action OnItemDroppedOnTrash;

    public void OnDrop(PointerEventData eventData)
    {
        // InventoryUI가 드래그 상태를 관리하고 있으므로,
        // 드래그 중인 아이템이 드롭되면 이벤트를 호출하기만 하면 됩니다.
        Debug.Log("Item dropped on Trash Can!");
        OnItemDroppedOnTrash?.Invoke();
    }
}
