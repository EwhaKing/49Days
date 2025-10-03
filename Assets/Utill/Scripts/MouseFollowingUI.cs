using UnityEngine.InputSystem;
using UnityEngine;

public class MouseFollowingUI : MonoBehaviour
{
    public Vector2 offset = new Vector2(0, 0);
    RectTransform rectTransform;
    Canvas parentCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        // 1. 마우스의 스크린 좌표를 캔버스의 로컬 좌표로 변환합니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, // 좌표 변환의 기준이 될 RectTransform (부모 캔버스)
            Mouse.current.position.ReadValue(),       // 변환할 스크린 좌표 (마우스 위치)
            parentCanvas.worldCamera,                 // 캔버스 렌더링 카메라 (Overlay의 경우 null)
            out Vector2 localPoint);                  // 변환된 로컬 좌표가 저장될 변수

        // 2. 변환된 로컬 좌표에 오프셋을 더해 anchoredPosition을 설정합니다.
        // position 대신 anchoredPosition을 사용하는 것이 RectTransform을 다룰 때 더 안정적입니다.
        rectTransform.anchoredPosition = localPoint + offset;
    }
}