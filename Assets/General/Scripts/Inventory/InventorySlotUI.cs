using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

// [수정] IPointerClickHandler 인터페이스 추가
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("자식 오브젝트 연결")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private GameObject countBackground;

    public event Action<int> OnSlotClicked;
    public event Action<int> OnBeginDragSlot;
    public event Action<int> OnDropOnSlot;
    public event Action OnEndDragSlot;

    public int slotIndex { get; private set; }
    private bool hasItem = false;

    private void Awake()
    {
        // 아이템 아이콘은 마우스 이벤트를 가로채지 않도록 설정
        if (itemIconImage != null)
        {
            itemIconImage.raycastTarget = false;
        }
        
        ClearSlot();
    }

    public void Init(int index)
    {
        slotIndex = index;
    }
    
    public void UpdateSlot(InventoryManager.InventorySlotData slotData)
    {
        hasItem = true;
        itemIconImage.sprite = slotData.itemData.itemIcon;
        itemIconImage.gameObject.SetActive(true);

        if (slotData.count > 1)
        {
            itemCountText.gameObject.SetActive(true);
            itemCountText.text = slotData.count.ToString();
            if (countBackground != null) countBackground.SetActive(true);
        }
        else
        {
            itemCountText.gameObject.SetActive(false);
            if (countBackground != null) countBackground.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        hasItem = false;
        itemIconImage.sprite = null;
        itemIconImage.gameObject.SetActive(false);
        
        itemCountText.gameObject.SetActive(false);
        if (countBackground != null) countBackground.SetActive(false);
    }

    // [수정] Button.onClick 대신 IPointerClickHandler.OnPointerClick을 직접 구현
    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그 동작의 일부가 아닌, 순수한 클릭일 때만 실행
        if (eventData.dragging) return;

        if (hasItem)
        {
            Debug.Log($"Slot {slotIndex} clicked!");
            OnSlotClicked?.Invoke(slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!hasItem) return;
        OnBeginDragSlot?.Invoke(slotIndex);
    }

    public void OnDrag(PointerEventData eventData) { }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragSlot?.Invoke();
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropOnSlot?.Invoke(slotIndex);
    }
}
