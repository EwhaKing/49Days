using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeaName  // 무조건 끝에 추가하기!!
{
    Unknown,
    HotWater,
    BlackTea,
    GreenTea,
    OolongTea,
    RoseTea,
    SolomonsSealTea,
    WhiteTea,
    MaghrebMint,
}

public enum IngredientName  // 무조건 끝에 추가하기!!
{
    None,        // None은 재료가 없는 상태를 나타냄 (추가 재료가 없는 경우를 위함)
    TeaLeaf,
    Rose,
    SolomonsSeal,
    Honey,

    // 차 평가 중 TeaLeaf가 산화 정도에 따라 다음 중 하나로 바뀜, 주방 내에선 다른 영향 없음
    TeaLeaf_Black,
    TeaLeaf_Oolong,
    TeaLeaf_Green,
    TeaLeaf_White,

    Mint,
    LotusLeaf,
    LotusFlower,
    Peach,
    Sugar,
    Jasmine,
    ForgetfulnessPotion
}

public static class NameExtensions
{ 
    public static string ToKorean(this TeaName tea)
    {
        switch (tea)
        {
            case TeaName.Unknown: return "알 수 없는 차";
            case TeaName.HotWater: return "뜨거운 물";
            case TeaName.BlackTea: return "홍차";
            case TeaName.GreenTea: return "녹차";
            case TeaName.OolongTea: return "우롱차";
            case TeaName.RoseTea: return "장미차";
            case TeaName.SolomonsSealTea: return "둥굴레차";
            case TeaName.WhiteTea: return "백차";
            case TeaName.MaghrebMint: return "마그레브 민트";

            default:
                Debug.LogWarning("한글 이름이 없습니다: " + tea.ToString()); 
                return tea.ToString();
        }
    }

    public static string ToKorean(this IngredientName ingredient)
    {
        switch (ingredient)
        {
            case IngredientName.None: return "없음";
            case IngredientName.TeaLeaf: return "찻잎";
            case IngredientName.Rose: return "장미";
            case IngredientName.SolomonsSeal: return "둥굴레";
            case IngredientName.Honey: return "꿀";
            case IngredientName.Mint: return "민트";
            case IngredientName.LotusLeaf: return "연잎";
            case IngredientName.LotusFlower: return "연꽃";
            case IngredientName.Peach: return "복숭아";
            case IngredientName.Sugar: return "설탕";
            case IngredientName.Jasmine: return "재스민";
            case IngredientName.ForgetfulnessPotion: return "망각제";

            default:
                Debug.LogWarning("한글 이름이 없습니다: " + ingredient.ToString()); 
                return ingredient.ToString();
        }
    }

    public static bool IsAdditionalIngredient(this IngredientName ingredient)
    {
        switch (ingredient)
        {
            case IngredientName.Honey:
            case IngredientName.Peach:
            case IngredientName.Sugar:
            case IngredientName.Jasmine:
                return true;

            default:
                return false;
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
    Low,         // 약산화 = 25
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

