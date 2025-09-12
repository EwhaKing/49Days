using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTeaRecipe", menuName = "TeaRecipe")]
public class TeaRecipe : ScriptableObject
{
    public TeaName teaName;  // 차 이름 (이미지 및 컷씬, 설명 연결, npc의 주문 등에 사용)

    public List<IngredientName> ingredients;  // 차에 들어가는 재료 리스트

    public int temperature;  // 우려낼 때의 물 온도 (섭씨)

    public int brewTime;  // 우려내는 시간 (초)

    public List<IngredientName> availableAdditionalIngredients;  // 가능한 추가 재료 리스트

}
