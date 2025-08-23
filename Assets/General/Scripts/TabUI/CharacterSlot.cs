using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image charImage; // 왼쪽 그리드의 슬롯 썸네일
    [SerializeField] Image highlight; // 하이라이트용 (별도 오버레이 이미지)

    CharacterData boundData;
    AffinityPanel panel;
    bool clickable; // 안전장치(미만남 클릭 방지)
    // 페이지 갱신 때마다 호출
    public void Bind(CharacterData data, Sprite unknown, AffinityPanel owner)
    {
        // 참조 캐시
        boundData = data;
        panel = owner;

        var cm = CharacterManager.Instance;

        // 1) 캐릭터가 없거나(페이지 남는 칸), 매니저가 없으면 이 슬롯은 아예 숨김
        if (cm == null || data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 2) 정상 데이터면 슬롯을 보이게 하고, 만남 여부에 따라 썸네일 결정
        gameObject.SetActive(true);

        bool met = cm.HasMet(data.characterName);
        clickable = met; // 미만남이면 클릭 불가

        // 왼쪽 슬롯 썸네일: 만남 → data.slotImage, 미만남 → unknown
        if (charImage == null)
        {
            Debug.LogError("[CharacterSlot] charImage 미할당");
            return;
        }
        charImage.enabled = true;
        charImage.sprite = met && data.slotImage != null ? data.slotImage : unknown;

        //하이라이트는 항상 초기화(꺼진 상태)로 시작
        if (highlight != null)
            highlight.enabled = false;
    }


    public void OnPointerClick(PointerEventData _)
    {
        // 미만남 또는 데이터 없음이면 무시
        if (!clickable || boundData == null || panel == null) return;

        // 안전: 런타임 중 상태가 바뀌었을 수도 있으니 한 번 더 체크
        var cm = CharacterManager.Instance;
        if (cm == null || !cm.HasMet(boundData.characterName)) return;

        panel.ShowCharacter(boundData);
    }

    //만날 수 있는 애들은 마우스 오버 시 하이라이트
    // 마우스 오버
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickable && highlight != null)
            highlight.enabled = true;
    }

    // 마우스 아웃
    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null)
            highlight.enabled = false;
    }
}
