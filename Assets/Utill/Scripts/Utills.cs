using System.Collections;
using System.Collections.Generic;
using System;

public static class Utills
{
    /// <summary>
    /// Enum T 타입의 모든 값을 순회 가능한 형태로 가져옵니다.
    /// </summary>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }
}
