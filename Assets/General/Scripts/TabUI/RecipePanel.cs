using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePanel : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] private RectTransform recipeTitle;
    [SerializeField] private RectTransform pageControlBox;
    [SerializeField] GameObject recipeSlotPrefab;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] private Transform recipeGrid;
    [SerializeField] private Sprite unknownRecipeSprite; // 미해금 레시피에 사용할 '물음표' 스프라이트

    [Header("Right Side")]
    [SerializeField] private GameObject rightPanel;
    [SerializeField] Image recipeImage;
    [SerializeField] Image simpleRecipeImage;
    [SerializeField] private Text recipeNameText;
    [SerializeField] private Text recipeDescriptionText;
    [SerializeField] Button recipePen;
    private List<RecipeSlot> slots = new List<RecipeSlot>();
    // Start is called before the first frame update

    private const int PageSize = 9; // 한 페이지에 보여줄 슬롯 개수
    private int page = 0;
    private int maxPage;
    private List<RecipeDescription> allRecipes;

    void Awake()
    {
        var rm = RecipeDescriptionManager.Instance;
        int total = rm?.Count ?? 0;
        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;
    }

    void OnEnable()
    {
        // 1. 데이터 로드 및 페이지 계산
        var rm = RecipeDescriptionManager.Instance;
        allRecipes = rm.GetAllRecipeDescriptions();

        int totalRecipes = allRecipes?.Count ?? 0;
        maxPage = (totalRecipes == 0) ? 0 : (totalRecipes - 1) / PageSize;

        // 2. 페이지 초기화 및 UI 갱신
        page = 0;
        RefreshPage();
        ClearRightPanel();

        // 3. 버튼 이벤트 연결
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    void OnDisable()
    {
        if (nextButton) nextButton.onClick.RemoveListener(NextPage);
        if (prevButton) prevButton.onClick.RemoveListener(PrevPage);
    }

    private void NextPage()
    {
        if (page < maxPage)
        {
            page++;
            RefreshPage();
        }
    }

    private void PrevPage()
    {
        if (page > 0)
        {
            page--;
            RefreshPage();
        }
    }
    private void EnsureSlotCount()
    {
        // 현재 페이지에 필요한 슬롯 개수를 계산
        int startIdx = page * PageSize;
        int remainingRecipes = allRecipes.Count - startIdx;
        int needed = Mathf.Min(PageSize, remainingRecipes);

        // 슬롯이 부족하면 새로 생성
        while (slots.Count < needed)
        {
            var go = Instantiate(recipeSlotPrefab, recipeGrid);
            slots.Add(go.GetComponent<RecipeSlot>());
        }

        // 모든 슬롯의 활성화 상태를 결정
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(i < needed);
        }
    }

    /// <summary>
    /// 현재 페이지의 내용을 슬롯에 채워넣어 UI를 갱신하는 함수
    /// </summary>
    public void RefreshPage()
    {
        if (allRecipes == null) return;

        EnsureSlotCount();

        int startIdx = page * PageSize;

        for (int i = 0; i < slots.Count; i++)
        {
            int recipeIdx = startIdx + i;
            if (recipeIdx < allRecipes.Count)
            {
                // 유효한 레시피 데이터가 있으면 슬롯을 활성화하고 Bind
                slots[i].gameObject.SetActive(true);
                slots[i].Bind(allRecipes[recipeIdx], unknownRecipeSprite, this);
            }
            else
            {
                // 페이지의 남는 칸에 해당하는 슬롯은 비활성화
                slots[i].gameObject.SetActive(false);
            }
        }

        // 페이지 버튼 활성화/비활성화
        if (prevButton) prevButton.interactable = (page > 0);
        if (nextButton) nextButton.interactable = (page < maxPage);
    }

    /// <summary>
    /// RecipeSlot에서 호출하여 오른쪽 상세 정보 패널을 채우는 함수
    /// </summary>
    public void ShowRecipeDetails(RecipeDescription data)
    {
        if (rightPanel) rightPanel.SetActive(true);

        recipeImage.sprite = data.teaImage;
        recipeNameText.text = data.recipeName;
        recipeDescriptionText.text = data.description;
    }
        /// <summary>
    /// 오른쪽 상세 정보 패널을 초기 상태로 비우는 함수
    /// </summary>
    private void ClearRightPanel()
    {
        if(rightPanel) rightPanel.SetActive(false);
        
        recipeImage.sprite = null;
        recipeNameText.text = "";
        recipeDescriptionText.text = "";
    }
}