using UnityEngine;

public class FollowMouse : MonoBehaviour
{
     void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane; // 일반적으로 0 또는 약간 더 크게 (예: 10f)

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f; // 2D니까 z축은 고정
        transform.position = worldPosition;
    }
}
