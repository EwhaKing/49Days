using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{

    [SerializeField] IngredientName ingredientName;
    [SerializeField] IngredientType ingredientType;
    [SerializeField] GameObject ingredientPrefab;

    GameObject Fill;

    void Start() 
    {
        Fill = transform.Find("Fill").gameObject;
        Cabinet.Instance.AfterCabinetInit += FillDecision;
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
    }
}
