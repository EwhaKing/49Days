using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeaPot : MonoBehaviour
{
    enum State { Empty, Ready, Brewing, Done }
    State currentState = State.Empty;

    [SerializeField] Transform ingredientParent;
    [SerializeField] Transform waterEffect;

    List<TeaIngredient> ingredients = new List<TeaIngredient>();
    bool isWaterFirst = false;
    float temperature = 0;
    float brewTime = 5f;
    float timer = 0f;

    void Update()
    {
        if (currentState == State.Brewing)
        {
            timer += Time.deltaTime;
            if (timer >= brewTime)
            {
                currentState = State.Done;
                Debug.Log("차 완성됨!");
                // 완료 이펙트/소리 호출
            }
        }
    }

    void OnMouseUp()
    {
        if (currentState == State.Empty || currentState == State.Ready)
        {
            TryInsertIngredient();
        }
        else if (currentState == State.Done)
        {
            FinishTea();
        }
    }

    void TryInsertIngredient()
    {
        if (Hand.Instance.handIngredient == null) return;

        GameObject ingredientObj = Hand.Instance.Drop();
        ingredientObj.transform.SetParent(ingredientParent);
        ingredientObj.transform.localPosition = Vector3.zero;

        TeaIngredient teaIng = ingredientObj.GetComponent<TeaIngredient>();
        ingredients.Add(teaIng);
        Debug.Log($"{teaIng.ingredientName} 추가됨");

        if (ingredients.Count > 0 && !isWaterFirst)
            currentState = State.Ready;
    }

    public void PourWater(float waterTemp)
    {
        if (ingredients.Count == 0)
        {
            Debug.Log("재료가 없어서 물을 부을 수 없음");
            return;
        }

        if (currentState == State.Brewing || currentState == State.Done) return;

        temperature = waterTemp;
        isWaterFirst = ingredients.Count == 0;
        waterEffect?.gameObject.SetActive(true); // 물 이펙트 켜기

        currentState = State.Brewing;
        timer = 0f;

        Debug.Log($"우림 시작: {temperature}도");
    }

    void FinishTea()
    {
        Debug.Log("차가 완성되었습니다.");
        currentState = State.Empty;
        ingredients.Clear();
        isWaterFirst = false;
        temperature = 0;
        waterEffect?.gameObject.SetActive(false);
    }
}
