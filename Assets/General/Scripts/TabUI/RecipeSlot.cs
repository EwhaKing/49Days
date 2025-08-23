// RecipeSlot.cs
// UI의 레시피 목록에 들어갈 개별 슬롯 하나하나를 제어하는 스크립트입니다.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecipeSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image recipeImage; // 레시피 슬롯에 표시될 썸네일 이미지
    [SerializeField] private Text recipeNameText; // 레시피 이름 텍스트 (선택 사항)

    private RecipeDescription boundData; // 이 슬롯에 연결된 레시피의 원본 데이터
    private RecipePanel panel; // 이 슬롯을 관리하는 부모 패널 (UI)
    private bool isUnlocked; // 레시피 해금 여부 (클릭 가능 여부를 결정)

    public void Bind(RecipeDescription data, Sprite unknownSprite, RecipePanel ownerPanel)
    {
        // 필요한 정보들을 멤버 변수에 저장
        boundData = data;
        panel = ownerPanel;

        var recipeManager = RecipeDescriptionManager.Instance;

        // 1) 표시할 데이터가 없거나(페이지의 남는 칸), 매니저가 없으면 슬롯을 아예 숨깁니다.
        if (recipeManager == null || data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 2) 정상 데이터가 있다면 슬롯을 보이게 하고, 해금 여부에 따라 내용을 결정합니다.
        gameObject.SetActive(true);

        isUnlocked = recipeManager.IsRecipeUnlocked(data.recipeName);

        // 3) 해금 여부에 따라 이미지와 텍스트를 설정합니다.
        if (isUnlocked)
        {
            // 해금되었을 때: 레시피의 실제 이미지와 이름 표시
            recipeImage.sprite = data.simpleRecipeImage; // 또는 teaImage, 원하는 썸네일을 사용하세요.
            if (recipeNameText != null) recipeNameText.text = data.recipeName;
        }
        else
        {
            // 해금되지 않았을 때: '물음표' 이미지와 "???" 텍스트 표시
            recipeImage.sprite = unknownSprite;
            if (recipeNameText != null) recipeNameText.text = "???";
        }
    }

    /// <summary>
    /// 이 슬롯이 클릭되었을 때 호출됩니다. (IPointerClickHandler 인터페이스)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 레시피가 해금되지 않았거나, 데이터가 없으면 아무 일도 하지 않습니다.
        if (!isUnlocked || boundData == null || panel == null)
        {
            return;
        }

        // 부모 패널(RecipePanel)에게 이 레시피의 상세 정보를 보여달라고 요청합니다.
        panel.ShowRecipeDetails(boundData);
    }
}
