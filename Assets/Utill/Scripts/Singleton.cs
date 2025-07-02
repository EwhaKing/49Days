using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance { get { Init(); return _instance; } }

    public void Awake()
    {
        if(Init())
            Destroy(gameObject);
    }

    static bool Init()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<T>();
            GameObject obj;
            
            if (_instance == null)
            {
                obj = new GameObject { name = typeof(T).ToString() + "(Singleton)" };
                _instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
                return true;
            }

            obj = _instance.gameObject;
            DontDestroyOnLoad(obj);
            return false;
        }

        return true;
    }
}
