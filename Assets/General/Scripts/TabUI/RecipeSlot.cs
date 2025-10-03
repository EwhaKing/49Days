// 파일명: RecipeSlot.cs (최종 수정본)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// [수정] 마우스 이벤트를 받기 위해 IPointerEnterHandler, IPointerExitHandler 추가
public class RecipeSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image recipeImage;
    [SerializeField] private Image highlight; 
    [SerializeField] private TextMeshProUGUI recipeNameText;
    private RecipeDescription boundData;
    private RecipePanel panel;
    private bool isUnlocked; 
    public void Bind(RecipeDescription data, Sprite unknownSprite, RecipePanel ownerPanel)
    {
        // 참조 저장
        boundData = data;
        panel = ownerPanel;

        // 데이터가 없으면 슬롯 숨기기
        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 데이터가 있으면 슬롯 표시
        gameObject.SetActive(true);

        // 해금 여부를 isUnlocked 변수에 저장 (이후 다른 함수들에서 이 변수를 사용)
        isUnlocked = RecipeDescriptionManager.Instance.IsRecipeUnlocked(data.recipeName);

        // 해금 상태에 따라 UI 업데이트
        if (isUnlocked)
        {
            // 해금 시: 실제 이미지와 이름 표시
            if (recipeImage != null) recipeImage.sprite = data.teaImage;
            if (recipeNameText != null) recipeNameText.text = data.recipeName;
        }
        else
        {
            // 미해금 시: 물음표 이미지와 "???" 표시
            if (recipeImage != null) recipeImage.sprite = unknownSprite;
            if (recipeNameText != null) recipeNameText.text = "???";
        }
        // 하이라이트는 항상 꺼진 상태로 시작
        if (highlight != null)
            highlight.enabled = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUnlocked || boundData == null || panel == null) return;
        
        panel.ShowRecipeDetails(boundData);
    }
    // [수정] 마우스 오버 시 하이라이트 효과 추가
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isUnlocked && highlight != null)
            highlight.enabled = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null)
            highlight.enabled = false;
    }
}