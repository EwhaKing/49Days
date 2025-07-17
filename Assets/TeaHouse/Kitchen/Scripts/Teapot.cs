using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeaPot : SceneSingleton<TeaPot>  //싱글톤(알아보기)
{
    enum State { Empty, Ready, Brewing, Done }
    State currentState = State.Empty;

    //다병에 마우스 오버 시 팝업
    [SerializeField] GameObject ingredientTooltipPanel2; //나중에 이름 바꾸기
    [SerializeField] GameObject ingredientImagePrefab; // 재료 하나당 표시할 프리팹 (Image)
    [SerializeField] Transform ingredientListParent; // 재료 이미지들을 담을 부모 오브젝트
                                                     //reset 버튼
    [SerializeField] GameObject resetButton;

    [SerializeField] Transform ingredientParent;
    [SerializeField] Transform waterEffect;
    public Transform pourPosition;

    List<TeaIngredient> ingredients = new List<TeaIngredient>();
    float timer = 0f;
    bool waterPoured = false;
    bool ingredientAddedBeforeWater = false;

    public Tea tea;  // 외부에서 접근 가능하게

    void Update()
    {
        if (currentState == State.Brewing)
        {
            timer += Time.deltaTime;
        }

        // ✅ 마우스 클릭 시 버튼 숨기기 (다병 아닌 경우)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform != this.transform)  // 클릭한 대상이 다병이 아니면
                {
                    resetButton?.SetActive(false);
                }
            }
            else
            {
                // 아무 것도 안 누른 빈 공간 클릭
                resetButton?.SetActive(false);
            }
        }
    }

    void OnMouseUp()
    {
        if (currentState == State.Done)
        {
            Debug.Log("완성된 차에는 아무 작업도 할 수 없습니다.");
            return;
        }

        bool wasHoldingIngredient = Hand.Instance.handIngredient != null;

        TryInsertIngredient();

        if (currentState == State.Brewing)
        {
            TryClickBell();
        }

        // ✅ 클릭 직전 손이 비어 있었을 때만 버튼 표시
        if (!wasHoldingIngredient)
        {
            resetButton?.SetActive(true);
        }
    }

    void TryInsertIngredient()
    {
        if (Hand.Instance.handIngredient == null) return;

        TeaIngredient ing = Hand.Instance.handIngredient.GetComponent<TeaIngredient>();
        if (ing == null) return;

        // 중복 재료 방지 //나중에 알림창으로 해야 됨. 
        if (ingredients.Exists(i => i.ingredientName == ing.ingredientName))
        {
            Debug.LogWarning($"{ing.ingredientName}은 이미 추가된 재료입니다.");
            return;
        }

        //애니메이션으로 재료 떨어지는 부분 추가
        GameObject ingredientObj = Hand.Instance.Drop();
        ingredientObj.transform.SetParent(ingredientParent);

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

        //주방 초기화!!
        FinishTea();
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
        ingredientTooltipPanel2?.SetActive(false);

        if (tea != null)
        {
            Destroy(tea);
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

        if (ingredientTooltipPanel2 != null)
        {
            ingredientTooltipPanel2.SetActive(true);
            ShowIngredientListUI();
        }
    }


    void OnMouseExit()
    {
        if (ingredientTooltipPanel2 != null)
        {
            ingredientTooltipPanel2.SetActive(false);
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
                GameObject imgObj = Instantiate(ingredientImagePrefab, ingredientListParent);
                UnityEngine.UI.Image img = imgObj.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    img.sprite = sr.sprite;
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

        UpdateBackgroundSize();
    }
    void UpdateBackgroundSize()
    {
        int itemCount = ingredientListParent.childCount;
        if (itemCount == 0) return;

        // HorizontalLayoutGroup 설정 가져오기
        HorizontalLayoutGroup layout = ingredientListParent.GetComponent<HorizontalLayoutGroup>();
        if (layout == null) return;

        float itemWidth = ((RectTransform)ingredientListParent.GetChild(0)).sizeDelta.x;
        float spacing = layout.spacing;
        float paddingLeft = layout.padding.left;
        float paddingRight = layout.padding.right;
        float extraBackgroundPadding = 10f; // 픽셀 단위 여유 padding

        float listWidth = (itemWidth * itemCount) + (spacing * Mathf.Max(0, itemCount - 1)) + paddingLeft + paddingRight;
        float finalWidth = listWidth + extraBackgroundPadding;

        RectTransform bgRect = ingredientTooltipPanel2.transform.Find("Background").GetComponent<RectTransform>();
        Vector2 size = bgRect.sizeDelta;
        size.x = finalWidth;
        bgRect.sizeDelta = size;

        Debug.Log($"✅ 계산된 width: item({itemWidth}) × count({itemCount}) + spacing({spacing}) + padding({paddingLeft}+{paddingRight}) + extra({extraBackgroundPadding}) = {finalWidth}");
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
