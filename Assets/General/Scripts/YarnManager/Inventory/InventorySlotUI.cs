using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리의 각 슬롯 UI를 담당하는 클래스.
/// 아이템 정보 표시, 클릭/드래그/드롭 이벤트를 감지 -> 상위 UI에 전달함.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("자식 UI 요소 연결")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private GameObject countBackground;


    // 아래로는 다른 스크립트와 연결되는 이벤트
    public event Action<int> OnSlotClicked;
    public event Action<int> OnBeginDragSlot;
    public event Action<int> OnDropOnSlot;
    public event Action OnEndDragSlot;

    public int slotIndex { get; private set; } // 슬롯의 고유 번호 (0~11)
    private bool hasItem = false; // 현재 이 슬롯에 아이템이 있는지 여부

    private void Awake()
    {
        // 아이템 아이콘이 마우스 이벤트를 가로채지 않도록 설정.
        // 클릭과 드래그는 이 스크립트가 붙어있는 GameObject에서 처리함.
        if (itemIconImage != null)
        {
            itemIconImage.raycastTarget = false;
        }
        
        ClearSlot();
    }

    /// <summary>
    /// 슬롯을 처음 생성할 때 호출하여 인덱스를 부여.
    /// </summary>
    public void Init(int index)
    {
        slotIndex = index;
    }
    
    /// <summary>
    /// 아이템 데이터를 받아와 슬롯의 UI를 업데이트.
    /// </summary>
    public void UpdateSlot(InventoryManager.InventorySlotData slotData)
    {
        hasItem = true;
        itemIconImage.sprite = slotData.itemData.itemIcon;
        itemIconImage.gameObject.SetActive(true);

        // 아이템 수량이 2 이상일 때만 숫자와 배경을 표시하도록 함. (도구/퀘스트 아이템 같은 거.)
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

    /// <summary>
    /// 슬롯을 빈 상태로 만들기.
    /// </summary>
    public void ClearSlot()
    {
        hasItem = false;
        itemIconImage.sprite = null;
        itemIconImage.gameObject.SetActive(false);
        
        itemCountText.gameObject.SetActive(false);
        if (countBackground != null) countBackground.SetActive(false);
    }

    #region Event Handlers
    // IPointerClickHandler를 사용해서 드래그와 클릭이 충돌하는 현상을 방지함.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭일 때만 이벤트 발동.
        if (eventData.dragging) return;

        if (hasItem)
        {
            OnSlotClicked?.Invoke(slotIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (hasItem)
        {
            OnBeginDragSlot?.Invoke(slotIndex);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중 시각 효과는 InventoryUI가 담당하는 것으로 함.
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragSlot?.Invoke();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 드롭은 아이템 유무와 상관없이 모든 슬롯에서 감지할 것.
        OnDropOnSlot?.Invoke(slotIndex);
    }
    #endregion
}
