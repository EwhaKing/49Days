using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipePanel : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] GameObject recipeSlotPrefab;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] Transform recipeGrid;
    [SerializeField] Sprite unknownRecipeSprite;

    [Header("Right Side")]
    [SerializeField] GameObject rightArea; // 인스펙터에서 RightArea를 연결하세요.
    [SerializeField] Image recipeImage;
    [SerializeField] Text recipeNameText;
    [SerializeField] Text recipeDescriptionText;

    private List<RecipeSlot> slots = new List<RecipeSlot>();
    private const int PageSize = 9;
    private int currentPage = 0;
    private int maxPage;

    void OnEnable()
    {
        RecipeDescriptionManager.OnRecipesLoaded += RefreshPanel;
        
        if (RecipeDescriptionManager.Instance != null && RecipeDescriptionManager.Instance.IsLoaded)
        {
            RefreshPanel();
        }

        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    void OnDisable()
    {
        RecipeDescriptionManager.OnRecipesLoaded -= RefreshPanel;
        
        if (nextButton) nextButton.onClick.RemoveListener(NextPage);
        if (prevButton) prevButton.onClick.RemoveListener(PrevPage);
    }
    
    private void RefreshPanel()
    {
        var rm = RecipeDescriptionManager.Instance;
        if (rm == null) return;

        int total = rm.Count;
        maxPage = (total == 0) ? 0 : (total - 1) / PageSize;
        
        currentPage = 0;
        RefreshPageDisplay();
        ClearRightPanel();
    }

    private void RefreshPageDisplay()
    {
        var allRecipes = RecipeDescriptionManager.Instance.GetAllRecipeDescriptions();
        if (allRecipes == null) return;

        int startIdx = currentPage * PageSize;
        int count = Mathf.Min(PageSize, allRecipes.Count - startIdx);
        
        EnsureSlotCount(count);

        for (int i = 0; i < count; i++)
        {
            int recipeIdx = startIdx + i;
            if (recipeIdx < allRecipes.Count)
            {
                slots[i].Bind(allRecipes[recipeIdx], unknownRecipeSprite, this);
            }
        }
        
        if (prevButton) prevButton.interactable = (currentPage > 0);
        if (nextButton) nextButton.interactable = (currentPage < maxPage);
    }

    private void NextPage() { if (currentPage < maxPage) { currentPage++; RefreshPageDisplay(); } }
    private void PrevPage() { if (currentPage > 0) { currentPage--; RefreshPageDisplay(); } }

    private void EnsureSlotCount(int needed)
    {
        while (slots.Count < needed)
        {
            var go = Instantiate(recipeSlotPrefab, recipeGrid);
            var slotComponent = go.GetComponent<RecipeSlot>();
            if(slotComponent != null)
            {
                slots.Add(slotComponent);
            }
        }
        
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(i < needed);
        }
    }
    
    public void ShowRecipeDetails(RecipeDescription data)
    {
        if(rightArea) rightArea.SetActive(true);
        if(recipeImage) recipeImage.sprite = data.teaImage;
        if(recipeNameText) recipeNameText.text = data.recipeName;
        if(recipeDescriptionText) recipeDescriptionText.text = data.description;
    }

    private void ClearRightPanel()
    {
        if(rightArea) rightArea.SetActive(false);
    }
}