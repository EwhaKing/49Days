using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{

    [Header("UI 아이콘 프리팹")]
    [SerializeField] private GameObject KeyIconPrefab;
    private static Transform keyIconParent;
    private KeyIcon activeIcon;

    [Header("아이콘 오프셋")]
    [SerializeField] private float verticalOffset = 2f;

    void Awake()
    {
        base.Awake(); // Interactable 기본 초기화
        if (keyIconParent == null)
        {
            GameObject canvasObj = GameObject.Find("KeyIconCanvas");
            if (canvasObj != null)
            {
                keyIconParent = canvasObj.transform;
            }
            else
            {
                Debug.LogError("씬에 KeyIconCanvas가 없습니다! World Space Canvas를 미리 배치하세요.");
            }
        }
    }

    // === 아이콘 관련 ===
    public void ShowEnterIcon()
    {
        Debug.Log("showentericon 실행");
        ClearIcon();

        if (keyIconParent == null) return;

        GameObject obj = Instantiate(KeyIconPrefab, keyIconParent);

        activeIcon = obj.GetComponent<KeyIcon>();
        activeIcon.SetKey('E');

        // NPC만 따라다니도록 true 플래그 전달
        activeIcon.SetTarget(transform, Vector3.up * verticalOffset, true);
    }

    public void ClearIcon()
    {
        if (activeIcon != null)
        {
            Destroy(activeIcon.gameObject);
            activeIcon = null;
        }
    }

    public override void Interact(PlayerHarvestController player)
    {
        Debug.Log("NPC와 상호작용");
        // 대화 시스템 호출 등
        SimpleStaticAgent agent = GetComponent<SimpleStaticAgent>();
        FieldYarnManager.Instance.RunDialogue($"NPC_{name}", agent);
    }

    // Start is called before the first frame update
    void Start()
    {
        type = InteractableType.NPC;
    }


}
