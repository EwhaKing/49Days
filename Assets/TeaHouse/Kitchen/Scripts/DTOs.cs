using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// 차 제작 완료 후 씬이 넘어갈 때 전달용
/// </summary>
/// <value></value>
public class MakedTea
{
    // 유니티 버전이 C# 9.0을 지원하지 않아서 init을 사용할 수 없음... 레코드 사용의미 퇴색
    public TeaName teaName;
    public IngredientName additionalIngredient;
    public int brewTimeGap;
    public int temperatureGap;
}