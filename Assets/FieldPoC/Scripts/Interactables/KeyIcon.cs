using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;


public class KeyIcon : MonoBehaviour
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Image background;
    private Coroutine blinkRoutine;
    public char Key { get; private set; }

    public void SetKey(char key)
    {
        Key = key;
        keyText.text = key.ToString();
        gameObject.SetActive(true);
    }

    //나무용 하이라이트
    public void SetHighlight(bool active)
    {
        // 하이라이트 처리
        background.color = active ? Color.red : Color.white;
    }

    /// <summary>
    /// Root 용: 깜빡거리는 하이라이트 시작
    /// </summary>
    public void StartBlinkHighlight()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkHighlight());
    }

    /// <summary>
    /// Root 용: 깜빡거림 중단
    /// </summary>
    public void StopBlinkHighlight()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
        // 원래 색상으로 복귀
        keyText.color = Color.black;
        background.color = Color.white;
    }

    private IEnumerator BlinkHighlight()
    {
        while (true)
        {
            background.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            background.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    //==NPC==
    private Transform target;
    private Vector3 offset;
    private bool isNPC = false;   // 기본은 false → 작물은 그대로

    // NPC에서만 호출: target + offset + NPC 여부
    public void SetTarget(Transform t, Vector3 o, bool npc = false)
    {
        target = t;
        offset = o;
        isNPC = npc;
    }

    void LateUpdate()
    {
        if (isNPC && target != null)
        {
            transform.position = target.position + offset;
        }
    }

}