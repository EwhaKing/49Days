using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TeaPot : SceneSingleton<TeaPot>  //싱글톤(알아보기)
{
    public enum State { Empty, Ready, Brewing, Done }
    State currentState = State.Empty;

    //다병에 마우스 오버 시 팝업
    [SerializeField] GameObject ingredientTooltipPanel;
    [SerializeField] GameObject ingredientImagePrefab; // 재료 하나당 표시할 프리팹 (Image)
    [SerializeField] Transform ingredientListParent; // 재료 이미지들을 담을 부모 오브젝트
    [SerializeField] GameObject resetButton;//reset 버튼

    [SerializeField] Transform ingredientParent;
    [SerializeField] Transform waterEffect;
    [SerializeField] private Slider pourSlider;
    public Transform pourPosition;

    [SerializeField] private SpriteRenderer smokeRenderer;


    GameObject teapotSmoke;
    Animator smokeAnimator;

    List<TeaIngredient> ingredients = new List<TeaIngredient>();
    float timer = 0f;
    bool waterPoured = false;

    private Tea tea;  // 외부에서 접근 가능하게

    void Start()
    {
        // Tea 인스턴스 생성
        tea = new Tea();

        if (resetButton != null)
            resetButton.SetActive(false);

        if (ingredientTooltipPanel != null)
            ingredientTooltipPanel.SetActive(false);

        teapotSmoke = transform.Find("teapotsmokeanimation")?.gameObject;
        if (teapotSmoke != null)
        {
            smokeAnimator = teapotSmoke.GetComponent<Animator>();
            teapotSmoke.SetActive(false); // 시작 시 꺼두기
            smokeAnimator.SetBool("isSmoking", false); // 안전하게 초기화
        }
        else
            Debug.LogWarning("[연기] teapotsmokeanimation를 찾을 수 없습니다.");

        if (pourSlider != null)
            pourSlider.value = 0f;

        StartCoroutine(SmoothSlider());
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

    }

    void TryInsertIngredient()
    {
        //손이 비어있으면 return하세요
        if (Hand.Instance.handIngredient == null) return;

        // 미리 재료 정보만 얻는다 (Drop 안 함)
        TeaIngredient ing = Hand.Instance.handIngredient.GetComponent<TeaIngredient>();
        if (ing == null) return;

        // 중복 재료 방지
        if (ingredients.Exists(i => i.ingredientName == ing.ingredientName))
        {
            Debug.Log($"{ing.ingredientName}은 이미 추가된 재료입니다.");
            return;
        }

        //추가 재료는 한 개만 가능
        if (tea.additionalIngredient != null)
            return;

        //물 > 주요 재료는 안 됨. 
        if (currentState == State.Brewing && ing.ingredientType != IngredientType.Additional)
        {
            Debug.Log("우림 중에는 주요 재료를 넣을 수 없습니다.");
            return;
        }

        // 중복이 아니면 실제로 놓기
        GameObject ingredientObj = Hand.Instance.Drop();
        ingredientObj.transform.SetParent(ingredientParent);
        ing.GetComponent<SpriteRenderer>().sortingOrder = 1;


        // 애니메이션으로 자연스럽게 떨어지게
        Vector3 targetPos = ingredientParent.position;
        Vector3 startAbove = targetPos + new Vector3(0.3f, 2.5f, 0); // 다병보다 1.5 위에서 떨어짐 (원래 x=0이 맞는데 좀 예쁘게 수정하고자...)
        ingredientObj.transform.position = startAbove; // 시작 위치 조정
        ing.PlayDropAnimation(targetPos, 2.0f);

        //추가한 재료는 무엇인가? 핵심자료라면 ready 상태가 됩니다. 추가재료라면 걍 추가합니다. 
        if (ing.ingredientType != IngredientType.Additional)
        {
            // 재료 추가
            ingredients.Add(ing);
            Debug.Log($"{ing.ingredientName} 핵심 재료 추가됨");

            if (ingredients.Exists(i =>
                i.ingredientType == IngredientType.TeaLeaf ||
                i.ingredientType == IngredientType.Flower ||
                i.ingredientType == IngredientType.Substitute))
            {
                currentState = State.Ready;
            }
        }
        else if (ing.ingredientType == IngredientType.Additional)
        {
            tea.additionalIngredient = ing;

            Debug.Log($"[추가재료] {ing.ingredientName} 추가됨");
        }

        Debug.Log($"다병에 들어간 재료 상태: {ing.ingredientName}, 산화: {ing.oxidizedDegree}, 스프라이트: {ing.GetComponent<SpriteRenderer>().sprite.name}");

    }

    public bool PourWater(float waterTemp)
    {
        //물은 한 번만 부을 수 있음.
        if (waterPoured)
        {
            Debug.Log("물은 한 번만 부을 수 있습니다.");
            return false;
        }

        tea.ingredients = ingredients;
        tea.temperature = (int)waterTemp;

        waterPoured = true;
        currentState = State.Brewing;
        timer = 0f;

        // waterEffect?.gameObject.SetActive(true); // 물 효과

        Debug.Log($"우림 시작: {waterTemp}도");

        //다병 연기 애니메이션 
        if (smokeAnimator != null)
        {
            teapotSmoke.SetActive(true);
            smokeAnimator.Play("startSmoke", 0, 0f); // 애니메이션 처음부터 재생
            smokeAnimator.SetBool("isSmoking", true);

            smokeRenderer.color = new Color(smokeRenderer.color.r, smokeRenderer.color.g, smokeRenderer.color.b, 1f); // 무조건 시작 시 알파 1

            if (waterTemp >= 70f)
                StartCoroutine(FadeOutSmokeAfterDelay(30f, 10f)); // 30초 유지 후 10초 페이드
            else if (waterTemp >= 40f)
                StartCoroutine(FadeOutSmokeAfterDelay(5f, 10f)); // 5초 유지 후 10초 페이드
            else
                teapotSmoke.SetActive(false); // 너무 차가우면 아예 끔
        }

        return true;

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
        //FinishTea();

        return tea;  // 완성된 차 객체 반환
    }

    public void FinishTea()
    {
        Debug.Log("다병 초기화됨");
        currentState = State.Empty;
        ingredients.Clear();
        timer = 0f;
        waterPoured = false;

        currentSliderValue = 0f;
        targetSliderValue = 0f;
        pourSlider.value = 0f;

        //애니메이션 꺼주기
        if (teapotSmoke != null)
            teapotSmoke.SetActive(false);

        //   waterEffect?.gameObject.SetActive(false); // 물 효과 비활성화인데 이거 나중에 다시 살려야 함. 

        // 재료 오브젝트 제거 추가
        foreach (Transform child in ingredientParent)
        {
            Destroy(child.gameObject);
        }

        ClearIngredientListUI();
        ingredientTooltipPanel?.SetActive(false);

        //현재 차는 폐기하고
        if (tea != null)
        {
            tea = null;
        }

        //새 Tea 생성 (리셋 후 다시 재료를 넣을 수 있게) + void start에서 차를 생성하기 때문에 여기서 기존 차를 없앨 때마다 새 차 생성해야 함. (사실 GC가 있으니까 윗 두 줄은 필요없지만 의미상 남겨둠)
        tea = new Tea();

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
        if (ingredients.Count == 0) return; // 재료가 하나도 없으면, 즉 물만 들어간 경우도 안 띄움

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

    //슬라이더로 물 붓는 진행 상황 업데이트
    [SerializeField] private float sliderSpeed = 0.7f; // 1초에 n씩 증가 (느릴수록 천천히)

    private float currentSliderValue = 0f;
    private float targetSliderValue = 0f;
    public void UpdatePourProgress(float target)
    {
        targetSliderValue = Mathf.Clamp01(target); // 계속 갱신됨
    }

    IEnumerator SmoothSlider()
    {
        while (true)
        {
            // 비율 progress 계산
            float progress = currentSliderValue / Mathf.Max(targetSliderValue, 0.0001f);

            // 감속 곡선 적용 (t → 0~1 → EaseOut)
            float easedSpeed = sliderSpeed * (1f - progress); // 마지막에 0에 가까워짐
            float delta = Time.deltaTime * Mathf.Max(easedSpeed, 0.03f); // 너무 느려지지 않게 최소 보장

            currentSliderValue = Mathf.MoveTowards(currentSliderValue, targetSliderValue, delta);

            if (pourSlider != null)
                pourSlider.value = currentSliderValue;

            yield return null;
        }
    }

    IEnumerator FadeOutSmoke(float duration)
    {
        if (smokeRenderer == null) yield break;

        Color c = smokeRenderer.color;
        float startAlpha = c.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            smokeRenderer.color = c;
            yield return null;
        }

        c.a = 0f;
        smokeRenderer.color = c;

        // 다 꺼졌으면 비활성화
        if (teapotSmoke != null)
            teapotSmoke.SetActive(false);
    }

    IEnumerator FadeOutSmokeAfterDelay(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Color c = smokeRenderer.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            smokeRenderer.color = new Color(c.r, c.g, c.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        smokeRenderer.color = new Color(c.r, c.g, c.b, 0f);
        teapotSmoke.SetActive(false);
    }

    /// <summary>
    /// 연기 애니메이션 정지 (Animator bool 파라미터 'isSmoking'을 false로 설정)
    /// </summary>
    public void StopSmokeAnimation()
    {
        if (smokeAnimator != null)
            smokeAnimator.SetBool("isSmoking", false);
    }

    /// <summary>
    /// (tea를 인자로 받아) state를 return 하는 함수
    /// </summary>

    public State GetCurrentState()
    {
        return currentState;
    }

}
