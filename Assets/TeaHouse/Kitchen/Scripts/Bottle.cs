using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{

    [SerializeField] IngredientName ingredientName;
    [SerializeField] IngredientType ingredientType;
    [SerializeField] GameObject ingredientPrefab;

    GameObject Fill;
    Sprite sprite;

    void Awake() 
    {
        Cabinet.AfterCabinetInit += () => {
            FillDecision();
            Debug.Log($"{ingredientName} 초기화: {Cabinet.Instance.ingredientCounts[ingredientName]}개");
        };
    }
    void Start() 
    {
        Fill = transform.Find("Fill").gameObject;
        sprite = Resources.Load<Sprite>($"Arts/{ingredientName.ToLowerString()}_default");
        if (sprite == null)
        {
            Debug.LogError($"{ingredientName}의 Fill 스프라이트를 찾을 수 없습니다.");
            return;
        }
        Fill.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void OnMouseUp() 
    {
        if (Hand.Instance.handIngredient != null)
        {
            TeaIngredient handIngredient = Hand.Instance.handIngredient;

            if (handIngredient.ingredientName != ingredientName) return;
            if (handIngredient.isChopped) return;
            if (handIngredient.oxidizedDegree != OxidizedDegree.None) return;
            if (handIngredient.roasted != ResultStatus.None) return;
            if (handIngredient.rolled != ResultStatus.None) return;

            Cabinet.Instance.ingredientCounts[ingredientName] += 1;
            Destroy(Hand.Instance.Drop());
        }
        else
        {
            if (Cabinet.Instance.ingredientCounts[ingredientName] == 0) return;

            Cabinet.Instance.ingredientCounts[ingredientName] -= 1;
            GameObject ingredientObject = Instantiate(ingredientPrefab, transform.position, Quaternion.identity);
            ingredientObject.GetComponent<TeaIngredient>().Init(ingredientName, ingredientType);
            Hand.Instance.Grab(ingredientObject);
            Debug.Log($"{ingredientName}을(를) 꺼냈습니다. 남은 개수: {Cabinet.Instance.ingredientCounts[ingredientName]}개");
        }

        FillDecision();
    }

    void FillDecision()
    {
        if (Cabinet.Instance.ingredientCounts[ingredientName] == 0)
        {
            Fill.SetActive(false);
        }
        else
        {
            Fill.SetActive(true);
        }
    }
}
