using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public static bool EqualIgnoreOrder<T>(this List<T> a, List<T> b)
    {
        return a.Count == b.Count && !a.Except(b).Any() && !b.Except(a).Any();
    }

}
