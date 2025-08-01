using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Bottle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] IngredientName ingredientName;
    [SerializeField] IngredientType ingredientType;
    [SerializeField] GameObject ingredientPrefab;
    [SerializeField] GameObject highlight;
    GameObject nameTag;
    TMP_Text nameText;
    TMP_Text countText;
    GameObject Fill;

    void Awake()
    {
        Cabinet.AfterCabinetInit += () =>
        {
            Init();
            FillDecision();
            countText.text = Cabinet.Instance.ingredientCounts[ingredientName] + "개";
            Debug.Log($"{ingredientName} 초기화: {Cabinet.Instance.ingredientCounts[ingredientName]}개");
        };
    }
    void Init()
    {
        highlight.SetActive(false);
        Fill = transform.Find("Fill").gameObject;
        nameTag = transform.Find("Canvas").Find("NameTag").gameObject;
        nameTag.SetActive(false);
        nameText = nameTag.transform.Find("Name").GetComponent<TMP_Text>();
        countText = nameTag.transform.Find("Count").GetComponent<TMP_Text>();

        nameText.text = ingredientName.ToKorean();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (Hand.Instance.handIngredient != null)  // 재료 다시 집어넣기
        {
            TeaIngredient handIngredient = Hand.Instance.handIngredient;

            if (!CanGetBackIn(handIngredient)) return;

            Cabinet.Instance.ingredientCounts[ingredientName] += 1;
            Destroy(Hand.Instance.Drop());
        }
        else  // 재료 빼기
        {
            if (Cabinet.Instance.ingredientCounts[ingredientName] == 0) return;

            Cabinet.Instance.ingredientCounts[ingredientName] -= 1;

            GameObject ingredientObject = Instantiate(ingredientPrefab, transform.position, Quaternion.identity);
            ingredientObject.GetComponent<TeaIngredient>().Init(ingredientName, ingredientType);
            Hand.Instance.Grab(ingredientObject);

            Debug.Log($"{ingredientName}을(를) 꺼냈습니다. 남은 개수: {Cabinet.Instance.ingredientCounts[ingredientName]}개");
        }

        countText.text = Cabinet.Instance.ingredientCounts[ingredientName] + "개";
        FillDecision();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        nameTag.SetActive(true);
        if (Hand.Instance.handIngredient != null && CanGetBackIn(Hand.Instance.handIngredient)
        || Hand.Instance.handIngredient == null && Cabinet.Instance.ingredientCounts[ingredientName] > 0)
            highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        nameTag.SetActive(false);
        highlight.SetActive(false);
    }

    void FillDecision()
    {
        if (Cabinet.Instance.ingredientCounts[ingredientName] == 0)
        {
            Fill.SetActive(false);
        }
        else
        {
            Fill.SetActive(true);
        }
    }

    bool CanGetBackIn(TeaIngredient handIngredient)
    {
        if (handIngredient.ingredientName != ingredientName) return false;
        if (handIngredient.isChopped) return false;
        if (handIngredient.oxidizedDegree != OxidizedDegree.None) return false;
        if (handIngredient.roasted != ResultStatus.None) return false;
        if (handIngredient.rolled != ResultStatus.None) return false;
        
        return true;
    }
}
