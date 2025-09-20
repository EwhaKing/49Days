using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SuccessTea
{
    public TeaName teaName;
    public IngredientName additionalIngredient;
}

public class unlockedTeaList
{
    public List<TeaName> past = new List<TeaName>();
    public List<TeaName> recent = new List<TeaName>();
    public List<IngredientName> additionalIngredients = new List<IngredientName>{IngredientName.None};
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

    // 낮 시간대 주문 관련
    private unlockedTeaList unlockedTeaList = new unlockedTeaList();
    private int dayOrderCount = 0;

    // 주방 넘어가서 실행해야하는 노드
    public string kitchenNodeTitle = string.Empty;

    void Start()
    {
        GameManager.Instance.onWeekChanged += () => { ClearRecentUnlockedTea(); };
        CabinetManager.Instance.onIngredientUnlocked += (ingredient) => 
        {
            if(ingredient.IsAdditionalIngredient())
                UnlockDayOrderAdditionalIngredient(ingredient);
        };
    }

    public void UnlockDayOrderTea(TeaName tea)
    {
        Debug.Log($"낮 주문 가능 차 잠금 해제: {tea}");
        unlockedTeaList.recent.Add(tea);
    }

    public void UnlockDayOrderAdditionalIngredient(IngredientName ingredient)
    {
        Debug.Assert(ingredient.IsAdditionalIngredient(), $"{ingredient}는 추가 재료가 아닙니다.");
        Debug.Log($"낮 주문 가능 추가 재료 잠금 해제: {ingredient}");
        unlockedTeaList.additionalIngredients.Add(ingredient);
    }

    /// <summary>
    /// 조합을 하나 무작위로 생성하여 SuccessTea에 추가 <br/>
    /// 예외: 마그레브 민트는 추가재료 설탕이 필수
    /// </summary>
    public void GenerateRandomSuccessTea()
    {        
        if (unlockedTeaList.recent.Count == 0 && unlockedTeaList.past.Count == 0)
        {
            Debug.LogWarning("성공 차를 생성할 수 없습니다. 잠금 해제된 차가 없습니다.");
            return;
        }

        List<TeaName> probabilityUp = null;
        List<TeaName> probabilityDown = null;

        switch (GameManager.Instance.GetDay())
        {
            case 1:
            case 2:
            case 3:
                probabilityUp = unlockedTeaList.recent;  // 1~3일차는 최근 해금된 차의 비중을 높임
                probabilityDown = unlockedTeaList.past;
                break;
            case 4:
            case 5:
                probabilityUp = unlockedTeaList.past;    // 4~5일차는 과거 해금된 차의 비중을 높임
                probabilityDown = unlockedTeaList.recent;
                break;
            case 6:
            case 7:
                // 기본 비중
                break;
        }
        
        TeaName selectedTea;

        if (probabilityUp == null)
        {
            if (UnityEngine.Random.Range(0, 2) == 0 || unlockedTeaList.past.Count == 0)
                selectedTea = unlockedTeaList.recent[UnityEngine.Random.Range(0, unlockedTeaList.recent.Count)];
            else
                selectedTea = unlockedTeaList.past[UnityEngine.Random.Range(0, unlockedTeaList.past.Count)];
        }
        else
        {
            if (UnityEngine.Random.Range(0, 3) > 0 || probabilityDown.Count == 0)
                selectedTea = probabilityUp[UnityEngine.Random.Range(0, probabilityUp.Count)];
            else
                selectedTea = probabilityDown[UnityEngine.Random.Range(0, probabilityDown.Count)];
        } 

        IngredientName selectedAdditionalIngredient = IngredientName.None;

        float rand = UnityEngine.Random.value;
        if (rand < 0.25f || ExceptionCheck(selectedTea))
        {
            // 25% 확률로 랜덤 선택 (None 포함)
            List<IngredientName> availableAdditionalIngredients = TeaMaker.GetAvilableAdditionalIngredients(selectedTea);
            availableAdditionalIngredients = availableAdditionalIngredients.FindAll(i => unlockedTeaList.additionalIngredients.Contains(i));
            selectedAdditionalIngredient = availableAdditionalIngredients[UnityEngine.Random.Range(0, availableAdditionalIngredients.Count)];
        }
        else if (rand < 0.85f && unlockedTeaList.additionalIngredients.Contains(IngredientName.ForgetfulnessPotion))
        {
            // 60% 확률로 망각제 선택
            selectedAdditionalIngredient = IngredientName.ForgetfulnessPotion;
        }
        else
        {
            // 15% 확률로 추가 재료 없음
            selectedAdditionalIngredient = IngredientName.None;
        }

        AddSuccessTea(selectedTea, selectedAdditionalIngredient);
    }
    
    private bool ExceptionCheck(TeaName tea)  // 추가 재료가 정해진 차 예외처리
    {
        switch (tea)
        {
            case TeaName.MaghrebMint:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 낮 한정 사용 권장, 첫 SuccessTea의 이름 반환
    /// </summary>
    /// <returns></returns>
    public TeaName GetOrderedTea()
    {
        return successTeaList[0].teaName;
    }

    /// <summary>
    /// 낮 한정 사용 권장, 첫 SuccessTea의 추가 재료 반환
    /// </summary>
    /// <returns></returns>
    public IngredientName GetOrderedAdditionalIngredient()
    {
        return successTeaList[0].additionalIngredient;
    }

    /// <summary>
    /// 첫 SuccessTea가 최근 해금된 차인지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsRecentUnlockedTea()
    {
        return unlockedTeaList.recent.Contains(successTeaList[0].teaName);
    }

    public bool IsTeaUnlocked(TeaName teaName)
    {
        return unlockedTeaList.past.Contains(teaName) || unlockedTeaList.recent.Contains(teaName);
    }

    public int GetDayOrderCount()
    {
        return dayOrderCount;
    }
    public void IncrementDayOrderCount()
    {
        dayOrderCount++;
    }
    public void ResetDayOrderCount()
    {
        dayOrderCount = 0;
    }

    private void ClearRecentUnlockedTea()
    {
        unlockedTeaList.past.AddRange(unlockedTeaList.recent);
        unlockedTeaList.recent.Clear();
    }

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

        GameFlowManager.LoadScene(GameFlowManager.KITCHEN_SCENE_NAME);
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

        if (makedTea.teaName != TeaName.HotWater && makedTea.brewTimeGap != 0) // HotWater는 우린 시간 상관없음
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
            CoroutineUtil.Instance.RunAfterSeconds(Pay, 1f);
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

    void OnEnable()
    {
        SaveLoadManager.Instance.onLoad += () => { unlockedTeaList = SaveLoadManager.Instance.Load<unlockedTeaList>(); };
        SaveLoadManager.Instance.onSave += () => SaveLoadManager.Instance.Save<unlockedTeaList>(unlockedTeaList);
    }
    void OnDisable()
    {
        SaveLoadManager.Instance.onLoad -= () => { unlockedTeaList = SaveLoadManager.Instance.Load<unlockedTeaList>(); };
        SaveLoadManager.Instance.onSave -= () => SaveLoadManager.Instance.Save<unlockedTeaList>(unlockedTeaList);
    }
}
