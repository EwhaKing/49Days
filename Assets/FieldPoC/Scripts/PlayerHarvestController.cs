using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHarvestController : MonoBehaviour
{
    [SerializeField] private float interactDistance = 1.5f;

    private Interactable target;
    public bool inHarvestMode = false;
    private float harvestTimer = 0f;
    private int pressCount = 0;
    private int lastDir = 0; // A/D 교대 입력 체크용

    InteractableInputHandler interactableInputHandler;
    GameInputHandler gameInputHandler;

    void OnEnable()
    {
        Debug.Assert(interactableInputHandler != null && gameInputHandler, "InputHandler not assigned in PlayerHarvestController");
        interactableInputHandler.OnHarvestRequested += OnWASD;
        interactableInputHandler.OnHarvestCancelRequested += OnCancel;
        gameInputHandler.OnPlayerInteractRequested += OnInteract;
    }
    void OnDisable()
    {
        interactableInputHandler.OnHarvestRequested -= OnWASD;
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
                    if (harvestTimer > 3f) ResetHarvest();
                    break;
                case InteractableType.Root:
                    if (harvestTimer > 2.5f) ResetHarvest();
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


    //esc키에 따른 동작
    public void OnCancel()
    {
        ResetHarvest();
    }

    public void OnWASD(InputAction.CallbackContext context)
    {
        if (!inHarvestMode || target == null) return;
        if (!(target is Harvestable harvestable)) return;

        // ==== InputSystem에서 key path 가져오기 ====
        string keyPath = context.control.path;
        char input = '\0';
        if (keyPath.Contains("/w")) input = 'W';
        else if (keyPath.Contains("/a")) input = 'A';
        else if (keyPath.Contains("/s")) input = 'S';
        else if (keyPath.Contains("/d")) input = 'D';
        // ==

        if (harvestable.Type == InteractableType.Tree)
        {
            int dir = 0;
            if (input == 'A') dir = -1; //A
            else if (input == 'D') dir = 1; //D

            if (dir != 0 && dir != lastDir)
            {
                pressCount++;
                lastDir = dir;
                Debug.Log("Tree press count: " + pressCount);

                if (pressCount >= 8) // A-D-A-D 4쌍
                {
                    DropItem(harvestable);
                    ResetHarvest();
                }
            }
        }
        else if (harvestable.Type == InteractableType.Root)
        {
            if (input == 'W') //W
            {
                pressCount++;
                Debug.Log("Root press count: " + pressCount);

                if (pressCount >= 5)
                {
                    DropItem(harvestable);
                    ResetHarvest();
                }
            }
        }
    }

    // ========== 모드 관리 ==========
    public void EnterHarvestMode(Harvestable h)
    {
        if (inHarvestMode) return;

        inHarvestMode = true;
        harvestTimer = 0f;
        pressCount = 0;
        lastDir = 0;
        target = h;

        Debug.Log("Harvest mode entered: " + h.Type);

        if (h.Type == InteractableType.Flower)
        {
            DropItem(h);
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactDistance);

        Interactable closest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            var interactable = h.GetComponent<Interactable>();
            if (interactable == null) continue;

            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = interactable;
            }
        }
        return closest;
    }

    private void DropItem(Harvestable h)
    {
        if (h.DropPrefab != null)
        {
            GameObject drop = Instantiate(h.DropPrefab, h.transform.position, Quaternion.identity);
            DroppedItem di = drop.GetComponent<DroppedItem>();
            if (di != null)
            {
                di.itemData = h.ItemData;
                di.amount = h.Amount;
            }
        }
        Destroy(h.gameObject);
        Debug.Log("Dropped: " + h.ItemData.name);
    }



}


