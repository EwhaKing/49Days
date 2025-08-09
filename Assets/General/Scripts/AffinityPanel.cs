using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AffinityPanel : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] Transform characterGrid;
    [SerializeField] GameObject characterSlotPrefab;
    [SerializeField] Sprite unknownSprite;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] TextMeshProUGUI pageText; // 페이지 표시(없으면 null 허용)

    [Header("Right Side")]
    [SerializeField] Image profileImage;
    [SerializeField] TextMeshProUGUI profileText;
    [SerializeField] Transform heartContainer;
    [SerializeField] TextMeshProUGUI likesText;
    [SerializeField] TextMeshProUGUI dislikesText;

    [Header("Data")]
    [SerializeField] List<CharacterData> characterDataList;

    const int PageSize = 9;
    int page = 0;            // 내부 인덱스: 0부터
    int maxPage = 0;         // 내부 인덱스: 0부터
    readonly List<CharacterSlot> slots = new(); // 풀

    void Start()
    {
        int total = characterDataList.Count;
        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;

        if (nextButton) nextButton.onClick.AddListener(() => { if (page < maxPage) { page++; RefreshPage(); } });
        if (prevButton) prevButton.onClick.AddListener(() => { if (page > 0) { page--; RefreshPage(); } });

        RefreshPage();
        ClearRightPanel();
    }

    // 필요 개수만큼 슬롯 보유(부족하면 생성, 남으면 끔)
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

    void RefreshPage()
    {
        int total = characterDataList.Count;
        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;
        page = Mathf.Clamp(page, 0, maxPage);

        int start = page * PageSize;                       // 이 페이지의 전역 시작 인덱스
        int countOnPage = Mathf.Clamp(total - start, 0, PageSize); // 이 페이지에 실제로 보여줄 개수

        EnsureSlotCount(countOnPage);

        // i = 페이지 내부 인덱스(0..countOnPage-1)
        // globalIndex = start + i  (원하는 수식으로 쓰면 slotIndex = (start + i) % 9)
        for (int i = 0; i < countOnPage; i++)
        {
            var data = characterDataList[start + i];
            slots[i].Bind(data, unknownSprite, this);
        }

        if (prevButton) prevButton.interactable = page > 0;
        if (nextButton) nextButton.interactable = page < maxPage;

        // 페이지 표시는 1부터
        int displayPage = (total == 0) ? 0 : page + 1;
        int displayMax = (total == 0) ? 0 : maxPage + 1;
        if (pageText) pageText.text = $"{displayPage} / {displayMax}";
        Debug.Log($"[AffinityPanel] 페이지 표시: {displayPage} / {displayMax}");
    }

    public void ShowCharacter(CharacterData data)
    {
        if (data == null) return;

        profileImage.type = Image.Type.Simple;
        profileImage.preserveAspect = true;
        profileImage.sprite = data.profileImage;
        profileImage.enabled = (data.profileImage != null);

        profileText.text = data.profileText ?? "";
        likesText.text = data.likes ?? "";
        dislikesText.text = data.dislikes ?? "";

        float heartsToShow = data.affinity / 2f;

        for (int i = 0; i < heartContainer.childCount; i++)
        {
            var img = heartContainer.GetChild(i).GetComponent<Image>();
            if (!img) continue;

            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.gameObject.SetActive(true);

            if (i < Mathf.FloorToInt(heartsToShow)) img.fillAmount = 1f;
            else if (i == Mathf.FloorToInt(heartsToShow) && heartsToShow % 1 != 0) img.fillAmount = 0.5f;
            else img.fillAmount = 0f;
        }
    }

    void ClearRightPanel()
    {
        profileImage.enabled = false;
        profileImage.sprite = null;
        profileText.text = "";
        likesText.text = "";
        dislikesText.text = "";

        for (int i = 0; i < heartContainer.childCount; i++)
            heartContainer.GetChild(i).gameObject.SetActive(false);
    }
}
