using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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

public class CoroutineUtil : Singleton<CoroutineUtil>
{
    public void RunCoroutine(IEnumerator action)
    {
        StartCoroutine(action);
    }

    public void RunAfterFirstFrame(Action action)
    {
        StartCoroutine(AfterFirstFrame(action));
    }
    private IEnumerator AfterFirstFrame(Action action)
    {
        yield return null; // 한 프레임 대기
        action?.Invoke();
    }

    public void RunAfterSeconds(Action action, float seconds)
    {
        StartCoroutine(AfterSeconds(seconds, action));
    }
    private IEnumerator AfterSeconds(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}

