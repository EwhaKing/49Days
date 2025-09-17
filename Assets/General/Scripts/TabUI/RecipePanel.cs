using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RecipePanel : MonoBehaviour
{
    [SerializeField] GameObject recipePanel;
    [SerializeField] TabUIController tabUIController;

    [Header("Left Side")]
    [SerializeField] GameObject recipeSlotPrefab;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] Transform recipeGrid;
    [SerializeField] Sprite unknownRecipeSprite;

    [Header("Right Side")]
    [SerializeField] GameObject rightPanel;
    [SerializeField] Image recipeImage;
    [SerializeField] Image simpleRecipeImage;
    [SerializeField] TextMeshProUGUI recipeNameText;
    [SerializeField] TextMeshProUGUI recipeDescriptionText;
    [SerializeField] Button recipeButton;

    private SpriteRenderer targetRecipeRenderer;
    private List<RecipeSlot> slots = new List<RecipeSlot>();
    private const int PageSize = 9;
    private int currentPage = 0;
    private int maxPage;
    private RecipeDescription currentSelectedRecipe;
    private Vector3 targetInitialPosition;

void Start()
{
        // 시작할 때 레시피 버튼을 비활성화하고, 클릭 이벤트를 연결
    Debug.Assert(recipeButton != null, "Recipe Button is not assigned in the inspector.");
    Debug.Assert(rightPanel != null, "Recipe Panel is not assigned in the inspector.");

    recipeButton.gameObject.SetActive(false);
    recipeButton.onClick.AddListener(OnRecipeButtonClick);
    
    rightPanel.SetActive(false);

    if (SceneManager.GetActiveScene().name != "Kitchen") return;  // 주방에서만 코르크보드 찾음

    // "Recipes" 라는 이름의 GameObject를 Scene에서 찾습니다.
    GameObject recipeObject = GameObject.Find("Recipes");

    // recipeObject를 성공적으로 찾았는지 확인합니다.
    if (recipeObject != null)
    {
        // 찾은 GameObject에서 SpriteRenderer 컴포넌트를 가져옵니다.
        targetRecipeRenderer = recipeObject.GetComponent<SpriteRenderer>();

        // SpriteRenderer 컴포넌트를 성공적으로 가져왔는지 확인합니다.
        if (targetRecipeRenderer != null)
        {
            // 초기 위치를 저장합니다.
            targetInitialPosition = targetRecipeRenderer.transform.position;
        }
        else
        {
            Debug.LogError("'Recipes' GameObject에 SpriteRenderer 컴포넌트가 없습니다.");
        }
    }
    else
    {
        Debug.LogError("Scene에서 'Recipes' GameObject를 찾을 수 없습니다.");
    }
}
    void OnEnable()
    {
        if (RecipeDescriptionManager.Instance != null && RecipeDescriptionManager.Instance.IsLoaded)
        {
            RefreshPanel();
        }

        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (prevButton) prevButton.onClick.AddListener(PrevPage);
    }

    void OnDisable()
    {
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
            if (slotComponent != null)
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
        currentSelectedRecipe = data;
        // 해금 여부를 확인
        bool isUnlocked = RecipeDescriptionManager.Instance.IsRecipeUnlocked(data.recipeName);

        // 오른쪽 상세 정보 패널을 활성화
        rightPanel.SetActive(true);

        if (isUnlocked)
        {
            // [해금된 경우] 실제 레시피 정보를 표시합니다.
            if (recipeImage) recipeImage.sprite = data.teaImage;
            if (simpleRecipeImage) simpleRecipeImage.sprite = data.simpleRecipeImage;
            if (recipeNameText) recipeNameText.text = data.recipeName;
            if (recipeDescriptionText) recipeDescriptionText.text = data.description;
        }
        else
        {
            // [잠긴 경우] 모든 정보를 '알 수 없음' 상태로 표시합니다.
            if (recipeImage) recipeImage.sprite = unknownRecipeSprite;
            if (simpleRecipeImage) simpleRecipeImage.sprite = unknownRecipeSprite;
            if (recipeNameText) recipeNameText.text = "???";
            if (recipeDescriptionText) recipeDescriptionText.text = "???";
        }

        recipeImage.SetNativeSize();
        simpleRecipeImage.SetNativeSize();

        // 해금된 레시피, 주방 씬일 때만 recipeButton을 활성화
        if (isUnlocked && SceneManager.GetActiveScene().name == "Kitchen")
        {
            recipeButton.gameObject.SetActive(true);
        }
        else
        {
            recipeButton.gameObject.SetActive(false);
        }
    }

    private void OnRecipeButtonClick()
    {
        if (currentSelectedRecipe != null && targetRecipeRenderer != null)
        {
            // 이 스크립트 안에 있는 이미지 변경 함수를 직접 호출
            ChangeTargetSprite(currentSelectedRecipe.simpleRecipeImage);
            tabUIController.CloseUI();
        }
        else
        {
            Debug.LogWarning("선택된 레시피가 없거나 Target Recipe Renderer가 연결되지 않았습니다.");
        }
    }

    /// 타겟 SpriteRenderer의 이미지를 변경하는 함수
    public void ChangeTargetSprite(Sprite newSprite)
    {
        if (targetRecipeRenderer != null && newSprite != null)
        {
            targetRecipeRenderer.sprite = newSprite;
            targetRecipeRenderer.transform.position = targetInitialPosition;
        }
        else
        {
            Debug.LogError("타겟 렌더러가 없거나 전달된 Sprite가 null입니다.");
        }
    }

}