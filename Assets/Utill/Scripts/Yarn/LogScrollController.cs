using UnityEngine;
using UnityEngine.UI;

public class LogScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    // ��ũ�� �ʿ� ���� �Ǵ� �� Ȱ��ȭ/��Ȱ��ȭ
    public void UpdateScrollInteractable()
    {
        if (scrollRect == null || content == null || viewport == null) return;

        bool needScroll = content.rect.height > viewport.rect.height;

        // ��ũ���� �ʿ��� ���� ScrollRect Ȱ��ȭ
        scrollRect.enabled = needScroll;

        // ��ũ�ѹٵ� �ʿ信 ���� Ȱ��ȭ/��Ȱ��ȭ
        if (scrollRect.verticalScrollbar != null)
            scrollRect.verticalScrollbar.gameObject.SetActive(needScroll);
    }

    // �ֽ� �ؽ�Ʈ�� �߰��� ���� ȣ��
    public void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
