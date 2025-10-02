using UnityEngine;

// 상호작용 가능한 오브젝트의 타입
public enum InteractableType
{
    Flower,
    Root,
    Tree,
    DroppedItem,
    NPC
}

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] public InteractableType type;
    [SerializeField] protected Sprite highlightSprite;

    protected SpriteRenderer sr;
    protected Sprite originalSprite;

    public InteractableType Type => type;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Assert(sr != null, "SpriteRenderer not found on Interactable: " + gameObject.name);
        originalSprite = sr.sprite; // 시작 시 프리팹이 가진 원래 스프라이트 저장
    }

    // 채집 모드 on/off 시 하이라이트 ↔ 원래 스프라이트 교체
    public void SetHighlight(bool on)
    {
        if (highlightSprite == null) return;
        sr.sprite = on ? highlightSprite : originalSprite;
    }

    // 구체 행동은 파생 클래스가 구현
    public abstract void Interact(PlayerHarvestController player);
}
