using UnityEngine.InputSystem;
using UnityEngine;

public class MouseFollowingUI : MonoBehaviour
{
    public Vector2 offset = new Vector2(0, 0);
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        rectTransform.position = Mouse.current.position.ReadValue() + offset;
    }
}