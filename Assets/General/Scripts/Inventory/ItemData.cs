using UnityEngine;

public enum ItemType    // 아이템 분류 Enum
{
    Ingredient,     // 재료
    Topping,        // 추가재료 (Additive나 Supplement로 바꾸고 싶으시면... 그러세요.)
    Quest,          // 퀘스트
    Tool            // 도구
}

/// <summary>
/// 아이템 하나의 데이터를 정의하는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemID;           // 아이템을 구분할 고유 ID - 세이브 로드할 때 사용함.
    public string itemName;         // 정보 표시에 사용 - 아이템 이름
    [TextArea]
    public string itemDescription;  // 정보 표시에 사용 - 아이템 설명
    public Sprite itemIcon;         // 정보 표시에 사용 - 아이템 아이콘
    public ItemType itemType;       // 아이템 분류 - 인벤토리 카테고리 토글 용.

    [Header("기타 정보")]
    public int maxStack = 99;       // 한 슬롯에 최대로 겹칠 수 있는 수량 (99개까지 가능)
}
