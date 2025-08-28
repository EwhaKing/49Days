using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SuccessTea
{
    public TeaName teaName;
    public IngredientName additionalIngredient;
}

public enum EvaluationResult
{
    Excellent,
    Normal,
    Bad
}

public class OrderManager : SceneSingleton<OrderManager>
{
    // 기존 찻집 씬 정보 저장용
    public Dictionary<int, string> seatedCustomerInfo = new Dictionary<int, string>();
    public int zoomPreset = 0;

    // 주문 관련
    private List<SuccessTea> successTeaList = new List<SuccessTea>();
    private bool autoPay = true;
    private string afterNodeTitle = "";
    private bool isNotAfterOrder = false;
    private MakedTea makedTea;
    private int price = 0;  // 주방에서 차 설명 띄울 때 설정
    private EvaluationResult evaluationResult;

    public void AddSuccessTea(TeaName successTea, IngredientName additionalIngredient = IngredientName.None)
    {
        successTeaList.Add(new SuccessTea { teaName = successTea, additionalIngredient = additionalIngredient });
        Debug.Log($"성공 차 추가: {successTea}, 추가 재료: {additionalIngredient}");
    }

    public void Order(string afterNodeTitle, bool autoPay = true)
    {
        isNotAfterOrder = false;
        this.afterNodeTitle = afterNodeTitle;
        this.autoPay = autoPay;

        SceneManager.LoadScene("Kitchen");
    }

    public void Evaluate()  // afterNodeTitle 재생할때 자동으로 채점
    {
        if (isNotAfterOrder) return;

        if (makedTea == null)
        {
            Debug.LogError("만든 차 정보가 없습니다.");
            return;
        }
        
        // 평가 로직
        short failCount = 0;

        List<SuccessTea> matchTeaList = successTeaList.FindAll(t => t.teaName == makedTea.teaName);
        if (matchTeaList.Count == 0)
        {
            Debug.Log($"만든 차 {makedTea.teaName}은(는) 성공 차 리스트에 없습니다.");
            failCount += 2;  // 차 이름이 틀리면 나머지와 상관없이 바로 bad 처리
        }

        if (Math.Abs(makedTea.temperatureGap) > 5)
        {
            Debug.Log($"차의 온도가 레시피에 근접하지 않습니다. 차이: {makedTea.temperatureGap}°C)");
            failCount++;
        }

        if (makedTea.brewTimeGap != 0)
        {
            Debug.Log($"차를 우린 시간이 레시피와 다릅니다. 차이: {makedTea.brewTimeGap}초");
            failCount++;
        }
        
        if(matchTeaList.FindAll(t => t.additionalIngredient == makedTea.additionalIngredient).Count == 0)
        {
            Debug.Log($"추가 재료가 다릅니다. 만든 차의 추가 재료: {makedTea.additionalIngredient}");
            failCount++;
        }

        switch (failCount)
        {
            case 0:
                evaluationResult = EvaluationResult.Excellent;
                break;
            case 1:
                evaluationResult = EvaluationResult.Normal;
                break;
            default:
                evaluationResult = EvaluationResult.Bad;
                break;
        }

        Debug.Log($"평가 결과: {evaluationResult}");
        successTeaList.Clear();  // 평가 후 성공 차 리스트 초기화
        
        if (autoPay)
        {
            Pay();
        }
    }

    public void Pay()
    {
        int finalPrice = price;
        switch (evaluationResult)
        {
            case EvaluationResult.Excellent:
                finalPrice = price * 2;
                break;
            case EvaluationResult.Normal:
                finalPrice = price;
                break;
            case EvaluationResult.Bad:
                finalPrice = 0;
                break;
        }

        GameManager.Instance.AddMoney(finalPrice);
        Debug.Log($"결제 완료: {finalPrice}원 (기본 가격: {price}원, 평가: {evaluationResult})");
    }

    public string GetAfterNodeTitle()
    {
        return afterNodeTitle;
    }

    public void SetAfterNodeTitle(string afterNodeTitle)
    {
        isNotAfterOrder = true;
        this.afterNodeTitle = afterNodeTitle;
    }

    public void SetMakedTea(MakedTea makedTea)
    {
        Debug.Log($"만든 차 설정: {makedTea.teaName}, 추가 재료: {makedTea.additionalIngredient}");
        this.makedTea = makedTea;
    }

    public EvaluationResult GetEvaluationResult()
    {
        return evaluationResult;
    }

    public TeaName GetMakedTeaName()
    {
        return makedTea.teaName;
    }

    public IngredientName GetMakedAdditionalIngredient()
    {
        return makedTea.additionalIngredient;
    }

    public int GetMakedTemperatureGap()
    {
        return makedTea.temperatureGap;
    }

    public int GetMakedBrewTimeGap()
    {
        return makedTea.brewTimeGap;
    }

    public void SetPrice(int price)
    {
        this.price = price;
    }
}
