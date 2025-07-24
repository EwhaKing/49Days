using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]  // 얘네를 저장할 일이 없지 싶긴한데...
public enum EvaluationResult  // 평가는 주방에서 하지 않음, 찻집으로 돌아가서 함
{
    Excellent,
    Normal,
    Bad
}

[JsonConverter(typeof(StringEnumConverter))]
public enum TeaName
{
    Unknown,
    BlackTea,
    GreenTea,
    OolongTea,
    WhiteTea,
    HerbalTea
}

/// <summary>
/// 차 제작 완료 후 씬이 넘어갈 때 전달용
/// </summary>
/// <value></value>
public record MakedTea
{
    // 유니티 버전이 C# 9.0을 지원하지 않아서 init을 사용할 수 없음... 레코드 사용의미 퇴색
    public TeaName TeaName;
    public IngredientName additionalIngredient;
    public int brewTimeGap;
    public int temperatureGap;
}