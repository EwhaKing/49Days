using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientName
{
    TeaLeaf,
    Rose,
    SolomonsSeal,
    Honey
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

