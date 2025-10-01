using UnityEngine;
using UnityEngine.EventSystems; // Event System 사용을 위해 추가

public class HoverableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 마우스 포인터가 이 오브젝트 영역에 들어왔을 때 호출됨
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cursor에게 마우스가 오브젝트 위에 있다고 알림
        if (Cursor.Instance != null)
        {
            Cursor.Instance.isMouseOverObject = true;
        }
    }

    // 마우스 포인터가 이 오브젝트 영역에서 나갔을 때 호출됨
    public void OnPointerExit(PointerEventData eventData)
    {
        // Cursor에게 마우스가 오브젝트 위에서 벗어났다고 알림
        if (Cursor.Instance != null)
        {
            Cursor.Instance.isMouseOverObject = false;
        }
    }
}