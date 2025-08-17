using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartBarUI : MonoBehaviour
{
    [Header("Optional: 지정하면 이 프리팹으로 복제, 미지정이면 첫 자식을 템플릿으로 사용")]
    [SerializeField] GameObject heartSlotPrefab; // 내부에 "White","Red" 자식 포함

    [SerializeField, Min(1)] int slots = 10;     // 만들어질 하트 슬롯 개수(기본 10)

    readonly List<Image> reds = new();           // 각 슬롯의 Red(Image) 캐시
    bool built = false;

    void Awake()
    {
        BuildIfNeeded();
    }

    // 외부에서 값만 넘겨주면 됨 (0~100, 5단위 반영, 반하트 지원)
    public void SetValue(int affinity01to100)
    {
        if (!built) BuildIfNeeded();

        int a = Mathf.Clamp(affinity01to100, 0, 100);
        a -= a % 5;

        int full = a / 10;          // 10점 = 1하트
        bool half = (a % 10) == 5;  // 반 하트

        for (int i = 0; i < reds.Count; i++)
        {
            if (i < full) reds[i].fillAmount = 1f;
            else if (i == full && half) reds[i].fillAmount = 0.5f;
            else reds[i].fillAmount = 0f;
        }
    }

    // ===== 내부 =====
    void BuildIfNeeded()
    {
        if (built) return;

        // 1) 템플릿 결정
        GameObject template = heartSlotPrefab;
        if (template == null)
        {
            if (transform.childCount == 0)
            {
                Debug.LogError("[HeartBarUI] HeartBar 아래에 HeartSlot이 없습니다. 템플릿 1개를 배치하거나 프리팹을 지정하세요.");
                return;
            }
            template = transform.GetChild(0).gameObject; // 첫 자식을 템플릿으로 사용
        }

        // 2) 기존 자식들을 정리: 템플릿만 남기고 나머지는 제거 (플레이 재진입시 중복 방지)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i).gameObject;
            if (c != template) Destroy(c);
        }

        reds.Clear();

        // 3) 템플릿 포함하여 slots 개수만큼 준비
        for (int i = 0; i < slots; i++)
        {
            GameObject slotGO;
            if (i == 0)
            {
                // 템플릿을 그대로 0번으로 사용
                slotGO = template;
                slotGO.name = "HeartSlot_0";
                slotGO.transform.SetAsLastSibling();
            }
            else
            {
                // 템플릿 복제
                slotGO = Instantiate(template, transform);
                slotGO.name = $"HeartSlot_{i}";
            }

            // 4) Red 이미지 캐시 + 채우기 모드 강제
            var redTr = slotGO.transform.Find("Red");
            if (redTr == null) { Debug.LogError("[HeartBarUI] HeartSlot에 'Red' 자식이 필요합니다."); continue; }

            var redImg = redTr.GetComponent<Image>();
            if (redImg == null) { Debug.LogError("[HeartBarUI] 'Red'에 Image 컴포넌트가 필요합니다."); continue; }

            redImg.type = Image.Type.Filled;
            redImg.fillMethod = Image.FillMethod.Horizontal;
            redImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            redImg.fillAmount = 0f;
            redImg.raycastTarget = false;

            // White가 Red를 가리지 않도록 Red를 맨 뒤(위)로
            redImg.transform.SetAsLastSibling();

            reds.Add(redImg);
        }

        reds.Reverse(); // 좌 > 우 순서로 정렬(하트를)
        built = true;
    }
}
