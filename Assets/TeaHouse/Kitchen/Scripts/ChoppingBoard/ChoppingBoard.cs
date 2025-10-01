using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public struct IngredientPrefabMap
{
    public IngredientName name;
    public GameObject prefab;
}

public class ChoppingBoard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] IngredientPrefabMap[] prefabMappings;
    private Dictionary<IngredientName, GameObject> prefabDict;
    [SerializeField] Sprite highlightSprite;
    [SerializeField] GameObject ChoppingBoardUI;
    [SerializeField] GameObject finishButton;
    Sprite originSprite;
    SpriteRenderer spriteRenderer;
    GameObject ingredientObject;
    TeaIngredient ingredient;
    GameObject sliceObject;

    public void OnPointerClick(PointerEventData e) 
    {
        if (CanDrop(true))
        {
            ingredient = Hand.Instance.handIngredient;
            ingredientObject = Hand.Instance.Drop();
            spriteRenderer.sprite = originSprite;

            sliceObject = Instantiate(prefabDict[ingredient.ingredientName], ChoppingBoardUI.transform);
            sliceObject.GetComponent<SliceController>().Init();

            Debug.Log("손질 시작: "+ ingredient.ingredientName);
        }
    }

    public void FinishChopping()
    {
        if (ingredientObject == null)
        {
            Debug.LogWarning("No ingredient object to finish chopping.");
            return;
        }
        
        ingredient.Chop();
        Hand.Instance.Grab(ingredientObject);
        Destroy(sliceObject);
        finishButton.SetActive(false);
        ChoppingBoardUI.transform.parent.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        Tooltip.Instance.Show("도마");

        if (CanDrop())
        {
            spriteRenderer.sprite = highlightSprite;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        Tooltip.Instance.Hide();
        Tooltip.Instance.HideFadeImmidately();
        spriteRenderer.sprite = originSprite;
    }

    bool CanDrop(bool isOnClicked = false)
    {
        if (Hand.Instance.handIngredient == null) return false;

        TeaIngredient ingredient = Hand.Instance.handIngredient;

        if (ingredient.isChopped == true)
        {
            if (isOnClicked) Tooltip.Instance.ShowFade("이미 손질한 재료는 다시 손질할 수 없습니다.");
            return false;
        }
        
        if (!(ingredient.ingredientType == IngredientType.Flower
        || ingredient.ingredientType == IngredientType.Substitute))
        {
            if (isOnClicked) Tooltip.Instance.ShowFade("손질할 수 없는 재료입니다.");
            return false;
        }

        return true;
    }
    
    void Start()
    {
        prefabDict = new Dictionary<IngredientName, GameObject>();
        foreach (var map in prefabMappings)
        {
            prefabDict[map.name] = map.prefab;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on ChoppingBoard.");
        }
        else
        {
            originSprite = spriteRenderer.sprite;
        }
    }
}
