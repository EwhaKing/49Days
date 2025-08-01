using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class RollingMachine : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    enum RollingState
    {
        Idle,
        Rolling,
        Rolled
    }

    [Header("스프라이트 관련")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite highlight;

    [Header("키 패널 관련")]
    [SerializeField] private GameObject keyPanelPrefab;
    [SerializeField] private GameObject canvasPrefab;
    private Transform keyPanelSpawnPoint;

    [Header("찻잎 유념 모션")]
    [SerializeField] private GameObject rollingLeafPrefab;
    [SerializeField] private Transform leafSpawnPoint;
    [Tooltip("중앙에 표시될 기본 찻잎 이미지")]
    [SerializeField] private Sprite rollingLeavesCenter;
    [SerializeField] private Sprite rollingLeavesRight;
    [SerializeField] private Sprite rollingLeavesUp;


    private KeyPanel currentKeyPanel;
    private GameObject currentIngredient;
    RollingState state = RollingState.Idle;

    private GameObject rollingLeavesVisual;

    private void SetCanvas()
    {
        if (keyPanelSpawnPoint == null)
        {
            GameObject canvas = GameObject.Find("UIRollingMachineCanvas");

            if (canvas == null && canvasPrefab != null)
            {
                canvas = Instantiate(canvasPrefab);
                canvas.name = "UIRollingMachineCanvas";
                Debug.Log("UIRollingMachineCanvas 프리팹에서 자동 생성");
            }

            if (canvas != null)
            {
                Transform spawnPoint = canvas.transform.Find("KeyPanelSetUpPoint");
                if (spawnPoint != null)
                {
                    keyPanelSpawnPoint = spawnPoint;
                    Debug.Log("keyPanelSpawnPoint 자동 연결 완료");
                }
                else
                {
                    Debug.LogWarning("'KeyPanelSetUpPoint'를 Canvas 안에서 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("UIRollingMachineCanvas를 찾거나 생성할 수 없습니다.");
            }
        }
    }

    private void DestroyCanvas()
    {
        GameObject canvas = GameObject.Find("UIRollingMachineCanvas");
        Destroy(canvas);

    }

    private void Start()
    {
        ClearRollingMachine();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        TeaIngredient ingredient = Hand.Instance.handIngredient;
        
        if ((ingredient == null && state == RollingState.Rolled) || (ingredient != null && ValidateRollingCondition() && state == RollingState.Idle))
        {
            spriteRenderer.sprite = highlight;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        spriteRenderer.sprite = defaultSprite;
    }

    public void OnPointerClick(PointerEventData e)
    {
        switch (state)
        {
            case RollingState.Idle:
                StartRolling();
                break;

            case RollingState.Rolling:
                if (Hand.Instance.handIngredient != null && currentIngredient != null)
                {
                    Debug.LogWarning($"유념기에 이미 {currentIngredient}이(가) 들어있으므로 새로운 재료는 넣을 수 없습니다.");
                }
                else
                {
                    Debug.LogWarning("유념 중에는 재료를 건드릴 수 없습니다.");
                }
                break;

            case RollingState.Rolled:
                if (Hand.Instance.handIngredient != null)
                {
                    Debug.LogWarning($"손에 재료를 든 상태로 다른 재료를 들 수 없습니다. 현재 {Hand.Instance.handIngredient.name}을(를) 쥐고 있습니다.");
                }
                else
                {
                    currentIngredient.SetActive(true);
                    Destroy(rollingLeavesVisual);
                    Hand.Instance.Grab(currentIngredient);
                    currentIngredient = null;
                    ClearRollingMachine();
                }
                break;
        }
    }

    private void StartRolling()
    {
        if (!ValidateRollingCondition()) return;

        currentIngredient = Hand.Instance.Drop();
        currentIngredient.transform.position = transform.position;
        currentIngredient.SetActive(false);

        state = RollingState.Rolling;
        Debug.Log("유념을 시작합니다.");
        SetCanvas();
        SetKeyPanel();
        SetRollingLeaves();        
    }

    private bool ValidateRollingCondition()
    {
        TeaIngredient handIngredient = Hand.Instance?.handIngredient;

        // 손에 쥔 재료가 없으면 거부
        if (handIngredient == null)
        {
            Debug.LogWarning("재료를 들고 있지 않습니다. Hand가 null입니다.");
            return false;
        }

        // 손에 쥔 재료가 찻잎이 아니라면 거부
        if (handIngredient.ingredientType != IngredientType.TeaLeaf)
        {
            Debug.LogWarning($"{handIngredient.name}은(는) 찻잎이 아니므로 유념기에 넣을 수 없습니다.");
            return false;
        }

        // 이미 유념한 재료는 거부
        if (handIngredient.rolled != ResultStatus.None)
        {
            Debug.LogWarning("이미 유념한 재료는 유념기에 재차 넣을 수 없습니다.");
            return false;
        }

        Debug.Log($"{handIngredient.spriteStatus} 상태의 {handIngredient.ingredientName}을(를) 유념기에 넣기 시도");
        return true;
    }

    private void SetKeyPanel()
    {
        if (keyPanelPrefab == null || keyPanelSpawnPoint == null)
        {
            Debug.LogError("keyPanelPrefab 또는 keyPanelSpawnPoint가 설정되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = transform.position + new Vector3(0, 3f, 0);     // 미니 게임 패널 위치 설정 - 유념기 위
        Quaternion faceCameraRotation = Quaternion.LookRotation(Camera.main.transform.forward);     // 카메라를 향하도록 회전
        GameObject panel = Instantiate(keyPanelPrefab, spawnPosition, faceCameraRotation, keyPanelSpawnPoint);      // 유념 키 패널 생성
        currentKeyPanel = panel.GetComponent<KeyPanel>();       // KeyPanel 컴포넌트 가져오기
        if (currentKeyPanel == null)
        {
            Debug.LogError("생성된 KeyPanel 오브젝트에 KeyPanel 컴포넌트가 없습니다.");
            Destroy(panel);
            return;
        }
        
        // 시퀀스 시작 후 이벤트 등록
        currentKeyPanel.StartSequence(GenerateRandomKeySequence(10));
        currentKeyPanel.OnComplete += OnRollingComplete;
    }

    private void SetRollingLeaves()
    {
        rollingLeavesVisual = Instantiate(rollingLeafPrefab, leafSpawnPoint.position, Quaternion.identity, transform);
        SpriteRenderer visualRenderer = rollingLeavesVisual.GetComponentInChildren<SpriteRenderer>();
        if (visualRenderer != null)
        {
            visualRenderer.sprite = rollingLeavesCenter;
            SpriteRenderer ingredientRenderder = currentIngredient.GetComponent<SpriteRenderer>();
            if (ingredientRenderder != null)
            {
                visualRenderer.color = ingredientRenderder.color;
            }
        }
    }

    private List<char> GenerateRandomKeySequence(int length)
    {
        char[] keys = new char[] { 'W', 'A', 'S', 'D' };
        List<char> sequence = new List<char>();
        for (int i = 0; i < length; i++)
        {
            sequence.Add(keys[Random.Range(0, keys.Length)]);
        }
        return sequence;
    }

    private void Update()
    {
        // 패널 활성화가 안 된 상황에는 아무것도 하지 않고 리턴
        if (currentKeyPanel == null || !currentKeyPanel.gameObject.activeInHierarchy)
        {
            return;
        }
    
        char input = GetKeyInput();
        if (input != '\0')
        {
            currentKeyPanel.ReceiveInput(input);
            ShowRollingMotion(input);
        }
    }

    private void ShowRollingMotion(char key)
    {
        if (rollingLeavesVisual == null) return;
        SpriteRenderer spriteRenderer = rollingLeavesVisual.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) return;

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;

        Vector3 offset = Vector3.zero;

        switch (key)
        {
            case 'W':
                spriteRenderer.sprite = rollingLeavesUp;
                offset = Vector3.up * 1f;
                break;
            case 'A':
                spriteRenderer.sprite = rollingLeavesRight;
                spriteRenderer.flipX = true;
                offset = Vector3.left * 2f;
                break;
            case 'S':
                spriteRenderer.sprite = rollingLeavesUp;
                spriteRenderer.flipY = true;
                offset = Vector3.down * 0.7f;
                break;
            case 'D':
                spriteRenderer.sprite = rollingLeavesRight;
                offset = Vector3.right * 2f;
                break;
        }
        rollingLeavesVisual.transform.localPosition = offset;
    }

    private char GetKeyInput()
    {
        if (Keyboard.current == null) return '\0';

        if (Keyboard.current.wKey.wasPressedThisFrame) return 'W';
        else if (Keyboard.current.aKey.wasPressedThisFrame) return 'A';
        else if (Keyboard.current.sKey.wasPressedThisFrame) return 'S';
        else if (Keyboard.current.dKey.wasPressedThisFrame) return 'D';
        return '\0';
    }


    private void OnRollingComplete(bool success)
    {
        Debug.Log($"유념에 {(success ? "성공" : "실패")}했습니다.");

        StopAllCoroutines();

        if (currentKeyPanel != null)
        {
            currentKeyPanel.OnComplete -= OnRollingComplete;
            Destroy(currentKeyPanel.gameObject);
            currentKeyPanel = null;
        }
        DestroyCanvas();

        if (currentIngredient != null)
        {
            TeaIngredient tea = currentIngredient.GetComponent<TeaIngredient>();
            if (tea != null)
            {
                tea.Roll(success);
                SpriteRenderer visualRenderer = rollingLeavesVisual.GetComponentInChildren<SpriteRenderer>();
                SpriteRenderer ingredientRenderer = currentIngredient.GetComponent<SpriteRenderer>();
                if (visualRenderer != null && ingredientRenderer != null)
                {
                    visualRenderer.color = ingredientRenderer.color;
                }
            }
            else
            {
                Debug.LogWarning("currentIngredient에 TeaIngredient 컴포넌트가 없습니다.");
            }
            state = RollingState.Rolled;
        }
    }

    private void ClearVisuals()
    {
        StopAllCoroutines();
        if (currentKeyPanel != null)
        {
            currentKeyPanel.OnComplete -= OnRollingComplete;
            Destroy(currentKeyPanel.gameObject);
            currentKeyPanel = null;
        }

        if (rollingLeavesVisual != null)
        {
            Destroy(rollingLeavesVisual);
        }

        DestroyCanvas();
    }

    public void ClearRollingMachine()
    {
        state = RollingState.Idle;
        if (currentIngredient != null)
        {
            currentIngredient = null;
        }
        ClearVisuals();
        return;
    }
}
