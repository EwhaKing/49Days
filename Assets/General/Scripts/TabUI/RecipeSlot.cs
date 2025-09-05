using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class RecipeSlot : MonoBehaviour, IPointerClickHandler
{
    // 인스펙터에서 연결할 UI 요소들
    [SerializeField] private Image recipeImage;
    [SerializeField] TextMeshProUGUI recipeNameText;

    private RecipeDescription boundData; // 이 슬롯에 연결된 레시피 데이터
    private RecipePanel panel;           // 부모 패널
    private bool isUnlocked;             // 해금 여부

    /// <summary>
    /// RecipePanel에서 호출하여 슬롯의 내용물을 채우는 함수
    /// </summary>
    public void Bind(RecipeDescription data, Sprite unknownSprite, RecipePanel ownerPanel)
    {
        // 참조 저장
        boundData = data;
        panel = ownerPanel;

        // 데이터가 없으면 슬롯을 숨김
        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 데이터가 있으면 슬롯을 표시
        gameObject.SetActive(true);

        // 해금 여부 확인
        isUnlocked = RecipeDescriptionManager.Instance.IsRecipeUnlocked(data.recipeName);

        // 해금 상태에 따라 UI 업데이트
        if (isUnlocked)
        {
            // 해금되었을 때: 실제 이미지와 이름 표시
            if (recipeImage != null) recipeImage.sprite = data.teaImage; // 썸네일용 이미지
            if (recipeNameText != null) recipeNameText.text = data.recipeName;
        }
        else
        {
            // 해금되지 않았을 때: '물음표' 이미지와 "???" 표시
            if (recipeImage != null) recipeImage.sprite = unknownSprite;
            if (recipeNameText != null) recipeNameText.text = "???";
        }
    }

    /// <summary>
    /// 슬롯이 클릭되었을 때 호출되는 함수
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (boundData == null || panel == null) return;
        panel.ShowRecipeDetails(boundData);
    }
}