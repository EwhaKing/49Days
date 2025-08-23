using UnityEngine;
using UnityEngine.UI;

public class LogScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    // 스크롤 필요 여부 판단 및 활성화/비활성화
    public void UpdateScrollInteractable()
    {
        if (scrollRect == null || content == null || viewport == null) return;

        bool needScroll = content.rect.height > viewport.rect.height;

        // 스크롤이 필요할 때만 ScrollRect 활성화
        scrollRect.enabled = needScroll;

        // 스크롤바도 필요에 따라 활성화/비활성화
        if (scrollRect.verticalScrollbar != null)
            scrollRect.verticalScrollbar.gameObject.SetActive(needScroll);
    }

    // 최신 텍스트가 추가될 때만 호출
    public void ScrollToBottom()
    {
        if (scrollRect == null) return;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
