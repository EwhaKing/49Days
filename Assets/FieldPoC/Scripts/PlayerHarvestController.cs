using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHarvestController : MonoBehaviour
{
    [SerializeField] private float interactDistance = 1.5f;

    private Interactable target;
    public bool inHarvestMode = false;
    public Action onEnterHarvestMode;
    private float harvestTimer = 0f;
    private int pressCount = 0;
    private int lastDir = 0; // A/D 교대 입력 체크용

    InteractableInputHandler interactableInputHandler;
    GameInputHandler gameInputHandler;

    void OnEnable()
    {
        interactableInputHandler = FindObjectOfType<InteractableInputHandler>();
        gameInputHandler = FindObjectOfType<GameInputHandler>();
        Debug.Assert(interactableInputHandler != null && gameInputHandler, "InputHandler not assigned in PlayerHarvestController");

        // 기존 OnHarvestRequested → 분리된 이벤트로 교체 // 변경됨
        interactableInputHandler.OnHarvestUpRequested += OnW;
        interactableInputHandler.OnHarvestLeftRequested += OnA;
        interactableInputHandler.OnHarvestRightRequested += OnD;

        interactableInputHandler.OnHarvestCancelRequested += OnCancel;
        gameInputHandler.OnPlayerInteractRequested += OnInteract;
    }
    void OnDisable()
    {
        // 분리된 이벤트 해제 // 변경됨
        interactableInputHandler.OnHarvestUpRequested -= OnW;
        interactableInputHandler.OnHarvestLeftRequested -= OnA;
        interactableInputHandler.OnHarvestRightRequested -= OnD;

        interactableInputHandler.OnHarvestCancelRequested -= OnCancel;
        gameInputHandler.OnPlayerInteractRequested -= OnInteract;
    }

    // ========== Update ==========
    void Update()
    {
        UpdateTargetHighlight();

        if (inHarvestMode && target is Harvestable harvestable)
        {
            harvestTimer += Time.deltaTime;

            switch (harvestable.Type)
            {
                case InteractableType.Tree:
                    if (harvestTimer > 5f) ResetHarvest();
                    break;
                case InteractableType.Root:
                    if (harvestTimer > 4f) ResetHarvest();
                    break;
            }
        }
    }

    // ========== 입력 처리 ==========
    public void OnInteract()
    {
        if (target == null) return;

        target.Interact(this); // 대상에 따라 알아서 처리
    }


    //TODO : esc키에 따른 동작
    public void OnCancel()
    {
        ResetHarvest();
    }

    private void OnW(InputAction.CallbackContext ctx)
    {
        if (!inHarvestMode || !(target is Harvestable h)) return;
        if (h.Type != InteractableType.Root) return;

        pressCount++;
        Debug.Log("Root press count: " + pressCount);

        if (pressCount >= 5)
        {
            int currentDay = GameManager.Instance.GetDate();   // 날짜 가져오기
            h.Harvest(currentDay);
            ResetHarvest();
        }
    }

    private void OnA(InputAction.CallbackContext ctx)
    {
        HandleTreeInput(-1); // A = -1
    }

    private void OnD(InputAction.CallbackContext ctx)
    {
        HandleTreeInput(1);  // D = 1
    }

    private void HandleTreeInput(int dir)
    {
        if (!inHarvestMode || !(target is Harvestable h)) return;
        if (h.Type != InteractableType.Tree) return;

        if (pressCount == 0)
        {
            // ✅ 첫 입력은 A(-1)든 D(1)든 상관없이 허용
            pressCount++;
            lastDir = dir; // 기준 방향 저장
            Debug.Log($"Tree first press {(dir == -1 ? "A" : "D")} count: {pressCount}");
        }
        else
        {
            // ✅ 이후부터는 교차 입력만 허용
            int expected = -lastDir; // 직전 입력의 반대
            if (dir == expected)
            {
                pressCount++;
                lastDir = dir; // 새 기준 업데이트
                Debug.Log($"Tree press {(dir == -1 ? "A" : "D")} count: {pressCount}");

                if (pressCount >= 8) // A-D 4쌍
                {
                    int currentDay = GameManager.Instance.GetDate();   // 날짜 가져오기
                    h.Harvest(currentDay);
                    ResetHarvest();
                }
            }
            else
            {
                Debug.Log("Wrong key order, ignored");
            }
        }
    }

    // public void OnWASD(InputAction.CallbackContext context)
    // {
    //     if (!inHarvestMode || target == null) return;
    //     if (!(target is Harvestable harvestable)) return;

    //     // ==== InputSystem에서 key path 가져오기 ====
    //     string keyPath = context.control.path;
    //     char input = '\0';
    //     if (keyPath.Contains("/w")) input = 'W';
    //     else if (keyPath.Contains("/a")) input = 'A';
    //     else if (keyPath.Contains("/s")) input = 'S';
    //     else if (keyPath.Contains("/d")) input = 'D';
    //     // ==

    //     if (harvestable.Type == InteractableType.Tree) //AD 교차 입력
    //     {
    //         int dir = 0;
    //         if (input == 'A') dir = -1; // A
    //         else if (input == 'D') dir = 1; // D

    //         // ✅ 교차 입력만 카운트 (같은 키 연속은 무시)
    //         if (dir != 0 && dir != lastDir)
    //         {
    //             pressCount++;
    //             lastDir = dir;

    //             if (dir == -1)
    //             {
    //                 Debug.Log("Tree press A count: " + pressCount);
    //             }
    //             else if (dir == 1)
    //             {
    //                 Debug.Log("Tree press D count: " + pressCount);
    //             }

    //             if (pressCount >= 8) // A-D 4쌍
    //             {
    //                 harvestable.Harvest(1);
    //                 ResetHarvest();
    //             }
    //         }
    //         else if (dir == lastDir)
    //         {
    //             // 같은 방향 키가 연속으로 들어올 때 로그 확인용
    //             Debug.Log("Ignored same key input: " + input);
    //         }
    //     }

    //     else if (harvestable.Type == InteractableType.Root)
    //     {
    //         if (input == 'W') //W
    //         {
    //             pressCount++;
    //             Debug.Log("Root press count: " + pressCount);

    //             if (pressCount >= 5)
    //             {
    //                 harvestable.Harvest(1); // 상태 갱신 //실제로는 나중에 인게임 날짜를 받아와야 함. 
    //                 ResetHarvest();
    //             }
    //         }
    //     }
    // }

    // ========== 모드 관리 ==========
    public void EnterHarvestMode(Harvestable h)
    {
        if (inHarvestMode) return;

        onEnterHarvestMode?.Invoke();

        inHarvestMode = true;
        harvestTimer = 0f;
        pressCount = 0;
        lastDir = 0;
        target = h;

        Debug.Log("Harvest mode entered: " + h.Type);

        if (h.Type == InteractableType.Flower)
        {
            int currentDay = GameManager.Instance.GetDate();   // 날짜 가져오기
            h.Harvest(currentDay);
            ResetHarvest();
        }
    }

    private void ResetHarvest()
    {
        inHarvestMode = false;
        harvestTimer = 0f;
        pressCount = 0;
        lastDir = 0;

        if (target != null)
        {
            target.SetHighlight(false);
            target = null;
        }

        Debug.Log("Harvest reset");
    }

    // ========== 유틸리티 ==========
    private void UpdateTargetHighlight()
    {
        Interactable closest = FindClosestInteractable();

        if (closest != target)
        {
            if (target != null) target.SetHighlight(false);
            if (closest != null) closest.SetHighlight(true);
            target = closest;
        }
    }

    private Interactable FindClosestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactDistance);

        Interactable closest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            var interactable = h.GetComponent<Interactable>();
            if (interactable == null) continue;

            //수확 불가능한 Harvestable은 후보에서 제외
            if (interactable is Harvestable harvestable && !harvestable.IsAvailable)
                continue;

            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = interactable;
            }
        }
        return closest;
    }




}


