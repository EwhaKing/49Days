using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeaPot : SceneSingleton<TeaPot>  //싱글톤(알아보기)
{
    enum State { Empty, Ready, Brewing, Done }
    State currentState = State.Empty;

    //다병에 마우스 오버 시 팝업
    [SerializeField] GameObject ingredientTooltipPanel;
    [SerializeField] GameObject ingredientImagePrefab; // 재료 하나당 표시할 프리팹 (Image)
    [SerializeField] Transform ingredientListParent; // 재료 이미지들을 담을 부모 오브젝트
                                                     //reset 버튼
    [SerializeField] GameObject resetButton;

    [SerializeField] Transform ingredientParent;
    [SerializeField] Transform waterEffect;
    public Transform pourPosition;

    GameObject teapotSmoke;
    Animator smokeAnimator;

    List<TeaIngredient> ingredients = new List<TeaIngredient>();
    float timer = 0f;
    bool waterPoured = false;
    bool ingredientAddedBeforeWater = false;

    private Tea tea;  // 외부에서 접근 가능하게

    void Start()
    {
        teapotSmoke = transform.Find("teapotsmokeanimation")?.gameObject;
        if (teapotSmoke != null)
            smokeAnimator = teapotSmoke.GetComponent<Animator>();
        else
            Debug.LogWarning("[연기] teapotsmokeanimation를 찾을 수 없습니다.");
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
            getTea();
        }

    }

    void TryInsertIngredient()
    {
        if (Hand.Instance.handIngredient == null) return;

        // Drop() 먼저 실행해서 '손에 들린 실제 오브젝트'를 가져온다
        GameObject ingredientObj = Hand.Instance.Drop();
        TeaIngredient ing = ingredientObj.GetComponent<TeaIngredient>();
        if (ing == null) return;

        // 중복 재료 방지 //나중에 알림창으로 해야 됨. 
        if (ingredients.Exists(i => i.ingredientName == ing.ingredientName))
        {
            Debug.LogWarning($"{ing.ingredientName}은 이미 추가된 재료입니다.");
            return;
        }

        ingredientObj.transform.SetParent(ingredientParent);

        ing.GetComponent<SpriteRenderer>().sortingOrder = 1;

        // 애니메이션으로 자연스럽게 떨어지게
        Vector3 targetPos = ingredientParent.position;
        Vector3 startAbove = targetPos + new Vector3(0.3f, 2.5f, 0); // 다병보다 1.5 위에서 떨어짐 (원래 x=0이 맞는데 좀 예쁘게 수정하고자...)
        ingredientObj.transform.position = startAbove; // 시작 위치 조정
        ing.PlayDropAnimation(targetPos, 2.0f);

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

        Debug.Log($"🧩 다병에 들어간 재료 상태: {ing.ingredientName}, 산화: {ing.oxidizedDegree}, 스프라이트: {ing.GetComponent<SpriteRenderer>().sprite.name}");

    }

    public void PourWater(float waterTemp)
    {
        if (currentState != State.Ready && currentState != State.Empty) return;

        // Tea 인스턴스 생성
        //tea = new GameObject("Tea").AddComponent<Tea>(); //
        tea = new Tea();
        tea.ingredients = ingredients;
        tea.temperature = (int)waterTemp;
        tea.isWaterFirst = !ingredientAddedBeforeWater; // 재료보다 먼저 물을 부었는지

        waterPoured = true;
        currentState = State.Brewing;
        timer = 0f;

        // waterEffect?.gameObject.SetActive(true); // 물 효과

        Debug.Log($"우림 시작: {waterTemp}도");

        if (smokeAnimator != null)
        {
            teapotSmoke.SetActive(true);           // 연기 오브젝트를 켜고
            smokeAnimator.SetTrigger("Play");      // 애니메이션 트리거 작동
        }

    }

    public Tea getTea()
    {
        if (currentState != State.Brewing) return null;

        currentState = State.Done;

        if (tea != null)
        {
            tea.timeBrewed = (int)timer;
        }

        // 평가 및 처리 책임은 외부 시스템에서 담당

        //주방 초기화!!
        FinishTea();

        return tea;  // 완성된 차 객체 반환
    }

    public void FinishTea()
    {
        Debug.Log("다병 초기화됨");
        currentState = State.Empty;
        ingredients.Clear();
        timer = 0f;
        waterPoured = false;
        ingredientAddedBeforeWater = false;

        //   waterEffect?.gameObject.SetActive(false); // 물 효과 비활성화인데 이거 나중에 다시 살려야 함. 

        // ✅ 재료 오브젝트 제거 추가
        foreach (Transform child in ingredientParent)
        {
            Destroy(child.gameObject);
        }

        ClearIngredientListUI();
        ingredientTooltipPanel?.SetActive(false);

        if (tea != null)
        {
            tea = null;
        }

        Debug.Log("🔥 ingredientParent 자식 개수: " + ingredientParent.childCount);

    }

    // UI 버튼에서 호출할 초기화 함수
    public void OnClickResetButton()
    {
        Debug.Log("초기화 버튼 눌림");
        FinishTea();
    }


    void OnMouseEnter()
    {
        if (currentState == State.Empty) return;  //상태가 비었으면 재료 UI 안 띄움

        if (ingredientTooltipPanel != null)
        {
            ingredientTooltipPanel.SetActive(true);
            ShowIngredientListUI();
        }

    }

    void OnMouseExit()
    {
        if (ingredientTooltipPanel != null)
        {
            ingredientTooltipPanel.SetActive(false);
            ClearIngredientListUI();
        }
    }

    void ShowIngredientListUI()
    {

        ClearIngredientListUI();

        if (ingredients.Count == 0)
        {
            Debug.Log("재료가 없어서 UI 생성 안 함");
            return;
        }

        foreach (TeaIngredient ing in ingredients)
        {
            SpriteRenderer sr = ing.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Debug.Log($"🧪 Tooltip에 들어갈 스프라이트: {sr.sprite?.name}");
                GameObject imgObj = Instantiate(ingredientImagePrefab, ingredientListParent);
                UnityEngine.UI.Image img = imgObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    img.sprite = sr.sprite;
                img.color = sr.color;  // ✅ 색상까지 복사
                Debug.Log($"🖼 UI Image에 할당된 스프라이트: {img.sprite?.name}");
            }
        }

        // 🟡 레이아웃 갱신 먼저
        RectTransform listRect = ingredientListParent.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);

        // 로그 찍기
        foreach (Transform child in ingredientListParent)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            Debug.Log($"📦 Child Width: {childRect.rect.width}");
        }

    }

    void ClearIngredientListUI()
    {
        Debug.Log("IngredientListUI cleared");
        foreach (Transform child in ingredientListParent)
        {
            Debug.Log("삭제된 UI 이미지: " + child.name);
            Destroy(child.gameObject);
        }
    }


}
