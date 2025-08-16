using UnityEngine;

// 아이템 종류를 나타내는 열거형(Enum)
public enum ItemType
{
    Ingredient,     // 재료
    Topping,        // 추가재료
    Quest,          // 퀘스트
    Tool            // 도구
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemID;           // 아이템을 구분할 고유 ID
    public string itemName;         // 아이템 이름
    [TextArea]
    public string itemDescription;  // 아이템 설명
    public Sprite itemIcon;         // 아이템 아이콘
    public ItemType itemType;       // 아이템 종류

    [Header("기타 정보")]
    public int maxStack = 99;       // 최대 겹치기 수량
}