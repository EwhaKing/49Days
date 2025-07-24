using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// Enum 값을 소문자 알파벳 문자열로 변환합니다.
    /// 예: MyEnum.ValueOne → "valueone"
    /// </summary>
    public static string ToLowerString(this Enum value)
    {
        return value.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// 매개번수로 주어진 리스트와 순서 상관없이 요소들이 일치하는지 비교합니다.
    /// </summary>
    public static bool EqualIgnoreOrder<T>(this List<T> list, List<T> otherList)
    {
        return list.Count == otherList.Count && !list.Except(otherList).Any() && !otherList.Except(list).Any();
    }

    /// <summary>
    /// 주어진 정수값과 오차범위(기본값: 5) 내에서 일치하는지 판단합니다.
    /// </summary>
    public static bool IsNear(this int value, int target, int tolerance = 5)
    {
        return Mathf.Abs(value - target) <= tolerance;
    }

}
