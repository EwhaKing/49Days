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

    [Header("Right Side")]
    [SerializeField] Image profileImage;
    [SerializeField] TextMeshProUGUI profileText;
    [SerializeField] Transform heartContainer;
    [SerializeField] TextMeshProUGUI likesText;
    [SerializeField] TextMeshProUGUI dislikesText;

    [Header("Data")]
    [SerializeField] List<CharacterData> characterDataList;

    private List<GameObject> slots = new List<GameObject>();

    void Start()
    {
        // 슬롯 생성 및 초기화
        for (int i = 0; i < characterDataList.Count; i++)
        {
            GameObject go = Instantiate(characterSlotPrefab, characterGrid);
            slots.Add(go);
            var slotScript = go.GetComponent<CharacterSlot>();
            slotScript.Init(characterDataList[i], unknownSprite, this);
        }

        ClearRightPanel();
    }

    public void ShowCharacter(CharacterData data)
    {
        profileImage.sprite = data.profileImage;
        profileText.text = data.profileText;
        likesText.text = data.likes;
        dislikesText.text = data.dislikes;

        float heartsToShow = data.affinity / 2f; // 예: affinity=7 → 3.5칸

        for (int i = 0; i < heartContainer.childCount; i++)
        {
            var img = heartContainer.GetChild(i).GetComponent<Image>();

            // Image가 없으면 건너뜀
            if (img == null)
                continue;

            if (i < Mathf.FloorToInt(heartsToShow))
            {
                // 가득 찬 하트
                img.gameObject.SetActive(true);
                img.fillAmount = 1f;
            }
            else if (i == Mathf.FloorToInt(heartsToShow) && heartsToShow % 1 != 0)
            {
                // 반칸 하트
                img.gameObject.SetActive(true);
                img.fillAmount = 0.5f;
            }
            else
            {
                // 비어있는 하트
                img.gameObject.SetActive(true); // 숨기고 싶으면 false
                img.fillAmount = 0f;
            }
        }
    }

    private void ClearRightPanel()
    {
        profileImage.sprite = null;
        profileText.text = "";
        likesText.text = "";
        dislikesText.text = "";

        for (int i = 0; i < heartContainer.childCount; i++)
            heartContainer.GetChild(i).gameObject.SetActive(false);
    }
}
