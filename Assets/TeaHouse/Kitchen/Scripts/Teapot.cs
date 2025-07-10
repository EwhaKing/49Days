using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeaPot : MonoBehaviour
{
    enum State { Empty, Ready, Brewing, Done }
    State currentState = State.Empty;

    [SerializeField] Transform ingredientParent;
    [SerializeField] Transform waterEffect;
    public Transform pourPosition;

    List<TeaIngredient> ingredients = new List<TeaIngredient>();
    float temperature = 0;
    float brewTime = 5f;
    float timer = 0f;

    void Update()
    {
        if (currentState == State.Brewing)
        {
            timer += Time.deltaTime;
        }
    }

    void OnMouseUp()
    {
        if (currentState == State.Empty || currentState == State.Ready)
        {
            TryInsertIngredient();
        }
        else if (currentState == State.Brewing)
        {
            TryClickBell();
        }
        else if (currentState == State.Done)
        {
            TryInsertAdditional(); // 차 완성 후 추가 재료 넣기 허용
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

        if (ingredients.Count > 0)
        {
            currentState = State.Ready;
        }
    }

    void TryInsertAdditional()
    {
        if (Hand.Instance.handIngredient == null) return;

        TeaIngredient ing = Hand.Instance.handIngredient.GetComponent<TeaIngredient>();
        if (ing == null || ing.ingredientType != IngredientType.Additional)
        {
            Debug.Log("완성된 차에는 추가 재료만 넣을 수 있습니다.");
            return;
        }

        GameObject ingredientObj = Hand.Instance.Drop();
        ingredientObj.transform.SetParent(ingredientParent);
        ingredientObj.transform.localPosition = Vector3.zero;

        ingredients.Add(ing);
        Debug.Log($"[추가재료] {ing.ingredientName} 추가됨");
    }

    public void PourWater(float waterTemp)
    {
        if (currentState != State.Ready && currentState != State.Empty) return;

        temperature = waterTemp;
        //   waterEffect?.gameObject.SetActive(true); // 물 효과 활성화 (필요시), 파티클 이용.

        currentState = State.Brewing;
        timer = 0f;

        Debug.Log($"우림 시작: {temperature}도");
    }

    void TryClickBell()
    {
        if (currentState != State.Brewing) return;

        currentState = State.Done;
        Debug.Log($"차 완성됨! 우린 시간: {timer:F1}초");

        EvaluateTea();
    }

    void EvaluateTea()
    {
        // Tea 객체 생성
        Tea tea = new GameObject("Tea").AddComponent<Tea>();
        typeof(Tea).GetField("ingredients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tea, ingredients);
        typeof(Tea).GetField("temperature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tea, (int)temperature);
        typeof(Tea).GetField("timeBrewed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(tea, (int)timer);

        // 핵심 재료 개수 체크
        int coreCount = ingredients.FindAll(i =>
            i.ingredientType == IngredientType.TeaLeaf ||
            i.ingredientType == IngredientType.Flower ||
            i.ingredientType == IngredientType.Substitute).Count;

        int additionalCount = ingredients.FindAll(i =>
            i.ingredientType == IngredientType.Additional).Count;

        // 평가 로그 출력만 처리 (실제 로직은 TeaManager 등에서)
        if (coreCount == 0)
        {
            if (additionalCount > 0)
                Debug.Log("평가: 뜨거운 물 + 추가재료 (예: 꿀물)");
            else
                Debug.Log("평가: 그냥 뜨거운 물");
        }
        else if (coreCount > 1)
        {
            Debug.LogWarning("평가: 알 수 없는 차 (핵심 재료가 2개 이상)");
        }
        else
        {
            Debug.Log("평가: 정상적인 차");

            // TODO: 핵심 재료의 타입에 따라 차 종류 판정
            /*
                if (coreType == TeaLeaf) => 일반 잎차
                if (coreType == Substitute) => 대용차
                if (coreType == Flower) => 꽃차
            */
        }

        // TODO: 차 전달, 보상, 이름 설정 등
        /*
            TeaManager.Instance.ProcessTea(tea);
        */
    }

    void FinishTea()
    {
        Debug.Log("다병 초기화됨");
        currentState = State.Empty;
        ingredients.Clear();
        temperature = 0;
        timer = 0;
        waterEffect?.gameObject.SetActive(false);
    }
}
