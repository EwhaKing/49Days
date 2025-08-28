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

public class CoroutineUtil : MonoBehaviour
{
    private static CoroutineUtil _instance;
    public static CoroutineUtil Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("CoroutineUtil");
                _instance = go.AddComponent<CoroutineUtil>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
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
}

