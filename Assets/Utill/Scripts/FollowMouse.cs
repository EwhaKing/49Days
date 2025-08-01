using UnityEngine;
using UnityEngine.InputSystem; // 신 Input System 네임스페이스

public class FollowMouse : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null) return; // 마우스가 없는 환경(모바일 등) 예외 처리

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane);

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f; // 2D 고정
        transform.position = worldPosition;
    }
}
