using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 1. 새로운 Input System을 사용하기 위해 추가

public class Cursor : MonoBehaviour
{
    public static Cursor Instance { get; private set; }

    [Header("상태별 커서 스프라이트")]
    [SerializeField] private Sprite defaultCursor;
    [SerializeField] private Sprite holdingCursor;
    [SerializeField] private Sprite mouseOverCursor;

    private Image cursorImage;
    private RectTransform rectTransform;

    public bool isMouseOverObject = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 인스턴스가 있다면 이 프리팹 전체를 파괴
            Destroy(transform.root.gameObject);
            return;
        }
        Instance = this;

        // 이 오브젝트(Image)가 아닌, 최상위 부모(Canvas)를 파괴하지 않도록 설정
        DontDestroyOnLoad(transform.root.gameObject);
        
        // --- 나머지 초기화 코드 ---
        cursorImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetCursorVisible()
    {
        gameObject.SetActive(true);
    }

    public void SetCursorInvisible()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        UnityEngine.Cursor.visible = false;
    }

    void Update()
    {
        Debug.Log("커서 업데이트 실행! 시간: " + Time.time);
        // 2. 마우스 위치 따라가는 코드를 새로운 방식으로 변경
        // rectTransform.position = Input.mousePosition; (기존 코드)
        rectTransform.position = Mouse.current.position.ReadValue(); // (새로운 코드)

        if (Hand.Instance.handIngredient != null)
        {
            cursorImage.sprite = holdingCursor;
        }
        else if (isMouseOverObject)
        {
            cursorImage.sprite = mouseOverCursor;
        }
        else
        {
            cursorImage.sprite = defaultCursor;
        }
    }
}