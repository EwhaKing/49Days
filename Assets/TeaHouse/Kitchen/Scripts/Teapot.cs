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
    float timer = 0f;
    bool waterPoured = false;
    bool ingredientAddedBeforeWater = false;

    public Tea tea;  // 외부에서 접근 가능하게

    //싱글톤
    public static TeaPot Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Update()
    {
        if (currentState == State.Brewing)
        {
            timer += Time.deltaTime;
        }
    }

    void OnMouseUp()
    {
        if (currentState == State.Done)
        {
            Debug.Log("완성된 차에는 아무 작업도 할 수 없습니다.");
            return;
        }

        TryInsertIngredient();

        if (currentState == State.Brewing)
        {
            TryClickBell();
        }
    }

    void TryInsertIngredient()
    {
        if (Hand.Instance.handIngredient == null) return;

        TeaIngredient ing = Hand.Instance.handIngredient.GetComponent<TeaIngredient>();
        if (ing == null) return;

        GameObject ingredientObj = Hand.Instance.Drop();
        ingredientObj.transform.SetParent(ingredientParent);
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredients.Add(ing);

        if (!waterPoured && ing.ingredientType != IngredientType.Additional)
        {
            ingredientAddedBeforeWater = true;
        }

        if (ing.ingredientType == IngredientType.Additional && tea != null && tea.additionalIngredient == null)
        {
            tea.additionalIngredient = ing;
        }

        if (ing.ingredientType != IngredientType.Additional)
        {
            Debug.Log($"{ing.ingredientName} 핵심 재료 추가됨");

            if (ingredients.Exists(i =>
                i.ingredientType == IngredientType.TeaLeaf ||
                i.ingredientType == IngredientType.Flower ||
                i.ingredientType == IngredientType.Substitute))
            {
                currentState = State.Ready;
            }
        }
        else
        {
            Debug.Log($"[추가재료] {ing.ingredientName} 추가됨");
        }
    }

    public void PourWater(float waterTemp)
    {
        if (currentState != State.Ready && currentState != State.Empty) return;

        // Tea 인스턴스 생성
        tea = new GameObject("Tea").AddComponent<Tea>();
        tea.ingredients = ingredients;
        tea.temperature = (int)waterTemp;
        tea.isWaterFirst = !ingredientAddedBeforeWater; // 재료보다 먼저 물을 부었는지

        waterPoured = true;
        currentState = State.Brewing;
        timer = 0f;

        // waterEffect?.gameObject.SetActive(true); // 물 효과

        Debug.Log($"우림 시작: {waterTemp}도");
    }

    void TryClickBell()
    {
        if (currentState != State.Brewing) return;

        currentState = State.Done;

        if (tea != null)
        {
            tea.timeBrewed = (int)timer;
        }

        // 평가 및 처리 책임은 외부 시스템에서 담당
    }

    public void FinishTea()
    {
        Debug.Log("다병 초기화됨");
        currentState = State.Empty;
        ingredients.Clear();
        timer = 0f;
        waterPoured = false;
        ingredientAddedBeforeWater = false;

        waterEffect?.gameObject.SetActive(false);

        if (tea != null)
        {
            Destroy(tea.gameObject);
            tea = null;
        }
    }
}
