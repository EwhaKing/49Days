using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AffinityPanel : MonoBehaviour
{
    [Header("Left")]
    [SerializeField] Transform characterGrid;
    [SerializeField] GameObject characterSlotPrefab;
    [SerializeField] Sprite unknownSprite;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] TextMeshProUGUI pageText;

    [Header("Right")]
    [SerializeField] Image profileImage;
    [SerializeField] TextMeshProUGUI profileText;
    [SerializeField] HeartBarUI heartBar;
    [SerializeField] TextMeshProUGUI likesText;
    [SerializeField] TextMeshProUGUI dislikesText;

    const int PageSize = 9;

    readonly List<CharacterSlot> slots = new();
    int page, maxPage;

    void Awake()
    {
        var cm = CharacterManager.Instance;
        int total = cm?.Count ?? 0;
        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;

        // 하트바 초기화
        //heartBar.Init(10);
    }

    void OnEnable()
    {
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
        RefreshPage();
        ClearRightPanel();
    }

    void OnDisable()
    {
        if (nextButton) nextButton.onClick.RemoveListener(NextPage);
        if (prevButton) prevButton.onClick.RemoveListener(PrevPage);
    }

    void NextPage() { if (page < maxPage) { page++; RefreshPage(); } }
    void PrevPage() { if (page > 0) { page--; RefreshPage(); } }

    void EnsureSlotCount(int needed)
    {
        while (slots.Count < needed)
        {
            var go = Instantiate(characterSlotPrefab, characterGrid);
            slots.Add(go.GetComponent<CharacterSlot>());
        }
        for (int i = 0; i < slots.Count; i++)
            slots[i].gameObject.SetActive(i < needed);
    }

    public void RefreshPage()
    {
        var cm = CharacterManager.Instance;
        int total = cm?.Count ?? 0;

        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;
        page = Mathf.Clamp(page, 0, maxPage);

        int start = page * PageSize;
        int count = Mathf.Clamp(total - start, 0, PageSize);

        EnsureSlotCount(count);
        for (int i = 0; i < count; i++)
        {
            var data = cm.GetStatic(start + i);
            slots[i].Bind(data, unknownSprite, this);
        }

        if (prevButton) prevButton.interactable = page > 0;
        if (nextButton) nextButton.interactable = page < maxPage;

        if (pageText)
        {
            int dispPage = (total == 0) ? 0 : page + 1;
            int dispMax = (total == 0) ? 0 : maxPage + 1;
            pageText.text = $"{dispPage} / {dispMax}";
        }
    }

    // 필드 교체
    // [SerializeField] Transform heartContainer;
    [SerializeField] GameObject heartSectionRoot; // HeartSectionContainer를 연결

    // ...

    public void ShowCharacter(CharacterData data)
    {
        if (data == null) { ClearRightPanel(); return; }

        var cm = CharacterManager.Instance;
        if (cm == null || !cm.HasMet(data.fixedIndex))  // 미만남이면 아무 것도 안 띄움 (클릭 자체가 막혀있음)
        {
            ClearRightPanel();
            return;
        }

        // 프로필/텍스트
        profileImage.sprite = data.profileImage;
        profileImage.preserveAspect = true;
        profileImage.enabled = data.profileImage != null;

        profileText.text = data.profileText ?? "";
        likesText.text = data.likes ?? "";
        dislikesText.text = data.dislikes ?? "";

        // 하트 섹션 보이기 + 값 반영
        if (heartSectionRoot) heartSectionRoot.SetActive(true);
        if (heartBar) heartBar.SetValue(cm.GetAffinity(data.fixedIndex)); // 0~100
    }

    void ClearRightPanel()
    {
        profileImage.enabled = false;
        profileImage.sprite = null;
        profileText.text = "";
        likesText.text = "";
        dislikesText.text = "";

        // 하트는 전담 컴포넌트로 초기화하고 섹션만 숨김
        if (heartBar) heartBar.SetValue(0);
        if (heartSectionRoot) heartSectionRoot.SetActive(false);
    }

}
