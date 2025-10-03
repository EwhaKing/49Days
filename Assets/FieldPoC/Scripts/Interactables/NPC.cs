using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
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
