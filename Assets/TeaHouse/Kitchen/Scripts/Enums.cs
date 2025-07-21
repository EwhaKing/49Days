using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientName
{
    None,        // None은 재료가 없는 상태를 나타냄 (추가 재료가 없는 경우를 위함)
    TeaLeaf,
    Rose,
    SolomonsSeal,
    Honey,
    Water

    // 차 평가 중 TeaLeaf가 산화 정도에 따라 다음 중 하나로 바뀜, 주방 내에선 다른 영향 없음
    TeaLeaf_Black,
    TeaLeaf_Oolong,
    TeaLeaf_Green
}

public static class IngredientNameExtensions
{ 
    public static string ToKorean(this IngredientName ingredient)
    {
        switch (ingredient)
        {
            case IngredientName.TeaLeaf: return "찻잎";
            case IngredientName.Rose: return "장미";
            case IngredientName.SolomonsSeal: return "둥굴레";
            case IngredientName.Honey: return "꿀";
            default: return ingredient.ToString();
        }
    }
}

public enum IngredientType
{
    TeaLeaf,     // 찻잎
    Flower,      // 꽃
    Substitute,  // 대용차재료
    Additional   // 추가 재료
}

public enum SpriteStatus
{
    Default,     // 기본 상태
    Chopped,     // 손질된 상태(대용차, 꽃차)
    Rolled,      // 유념된 상태
    Mashed,      // 뭉개진(유념실패) 상태
    Roasted      // 덖은 상태
}

public enum OxidizedDegree
{
    None,        // 산화 안함 = x
    Zero,        // 산화 미미 = 0
    Half,        // 반산화 = 50
    Full,        // 완전 산화 = 100
    Over         // 탐
}

public enum ResultStatus
{
    None,        // X
    Success,     // 성공
    Failed       // 실패
}

public enum ProcessStep
{
    Chop,             // 손질
    Oxidize,          // 산화
    Roast,            // 덖기
    Roll              // 유념
}

