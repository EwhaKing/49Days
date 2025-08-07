using UnityEngine;

/// <summary>
/// 씬이 변경될 시 데이터를 초기화하는 싱글톤<br/>
/// 한 씬 내부에서만 사용하는 게 일반적이나 자유롭게 활용 가능 <br/>
/// 일반 싱글톤과 마찬가지로 상속해서 사용 가능
/// </summary>
/// <typeparam name="T">싱글톤으로 구현할 클래스</typeparam>
public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _initialized = false;

    public static T Instance
    {
        get
        {
            if (!_initialized)
                Initialize();
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            _initialized = true;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private static void Initialize()
    {
        if (_initialized) return;

        _instance = FindObjectOfType<T>();

        if (_instance == null)
        {
            GameObject obj = new GameObject($"{typeof(T).Name}(SceneSingleton)");
            _instance = obj.AddComponent<T>();
        }

        _initialized = true;
    }
}