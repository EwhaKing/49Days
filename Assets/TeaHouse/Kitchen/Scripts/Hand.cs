using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : SceneSingleton<Hand>
{
    /// <summary>
    /// 잡고 있는 재료 정보 <br/>
    /// null 일시 빈손 <br/>
    /// 게터로 가져가서 상태 체크 후 Drop해갈지 결정하기 <br/>
    /// (ex: oxidizedDegree 가 None일때만 산화기에 넣을 수 있음)
    /// </summary>
    /// <value></value>
    public TeaIngredient handIngredient {get; private set;}


    public void Grab (GameObject teaIngredientObject)  // 잡기: 커서가 재료를 잡음
    {
        if (handIngredient != null) return;
        
        handIngredient = teaIngredientObject.GetComponent<TeaIngredient>();
        teaIngredientObject.GetComponent<FollowMouse>().enabled = true;
    }

    public GameObject Drop()  // 놓기: 커서가 재료를 놓음
    {
        GameObject handIngredientObject = handIngredient.gameObject;
        handIngredient = null;
        handIngredientObject.GetComponent<FollowMouse>().enabled = false;
        return handIngredientObject;
    }
}
