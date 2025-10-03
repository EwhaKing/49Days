using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic; // Queue

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

    //빠른 입력 시 먹통 되는 문제 때문에 > 큐 사용
    private Queue<int> inputQueue = new Queue<int>(); // A/D 입력 버퍼

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

            // === 큐 처리 ===
            if (inputQueue.Count > 0)
            {
                int dir = inputQueue.Dequeue();
                HandleTreeInput(dir);
            }

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

        h.AddProgress(); //게이지 바 증가

        if (pressCount >= 5)
        {
            int currentDay = GameManager.Instance.GetDate();   // 날짜 가져오기
            h.Harvest(currentDay);
            ResetHarvest();
        }
    }

    private void OnA(InputAction.CallbackContext ctx)
    {
        if (!inHarvestMode || !(target is Harvestable h)) return;
        if (h.Type != InteractableType.Tree) return;

        inputQueue.Enqueue(-1); // A
    }

    private void OnD(InputAction.CallbackContext ctx)
    {
        if (!inHarvestMode || !(target is Harvestable h)) return;
        if (h.Type != InteractableType.Tree) return;

        inputQueue.Enqueue(1); // D
    }

    private void HandleTreeInput(int dir)
    {
        if (!inHarvestMode || !(target is Harvestable h)) return;
        if (h.Type != InteractableType.Tree) return;

        if (pressCount == 0)
        {
            if (dir != -1)
            {
                Debug.Log("첫 입력은 A여야 함. 무시됨.");
                return;
            }

            pressCount++;
            lastDir = dir; // 기준 방향 저장
            Debug.Log($"Tree first press {(dir == -1 ? "A" : "D")} count: {pressCount}");

            //게이지 바 증가
            h.AddProgress();
            // 첫 입력 성공 → 하이라이트 갱신
            h.HighlightNextKey();
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

                //게이지 바 증가
                h.AddProgress();
                // 교차 입력 성공 → 하이라이트 갱신
                h.HighlightNextKey();

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
            return;
        }

        // E 아이콘 지우고 → 수확 아이콘 표시
        h.ClearIcons();
        h.ShowHarvestIcons();

        // 첫 키 하이라이트는 Tree 전용
        if (h.Type == InteractableType.Tree)
        {
            h.SpawnProgressBar(8);  // 8번 누르면 완료
            h.HighlightNextKey();
        }
        else if (h.Type == InteractableType.Root)
        {
            h.SpawnProgressBar(5);  // 5번 누르면 완료
            h.StartRootHighlight();
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
            if (target is Harvestable h)
            {
                h.ClearIcons();
                h.ClearProgressBar();
            }

            target.SetHighlight(false);
            target = null;
        }

        Debug.Log("Harvest reset");
    }

    // ========== 유틸리티 ==========
    private void UpdateTargetHighlight() //하이라이트 표시
    {
        Interactable closest = FindClosestInteractable();

        if (closest != target)
        {
            // 이전 타겟 처리
            if (target != null)
            {
                target.SetHighlight(false);

                if (target is Harvestable oldH)
                    oldH.ClearIcons();

                if (target is NPC oldNpc)
                    oldNpc.ClearIcon();
            }

            // 새 타겟 처리
            if (closest != null)
            {
                closest.SetHighlight(true);

                if (!inHarvestMode && closest is Harvestable newH)
                    newH.ShowEnterIcon();  // 하이라이트 시점에만 E 표시

                if (closest is NPC newNpc)
                    newNpc.ShowEnterIcon(); // E 아이콘
            }

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

    // private void UpdateKeyIcons()
    // {
    //     if (target is Harvestable h)
    //     {
    //         h.ClearIcons();

    //         if (!inHarvestMode)
    //         {
    //             // 아직 수확 모드 전 → E 키만 표시
    //             h.ShowEnterIcon();
    //         }
    //         else
    //         {
    //             // 수확 모드 중 → 타입별 입력 표시
    //             h.ShowHarvestIcons();
    //         }
    //     }
    // }

}


