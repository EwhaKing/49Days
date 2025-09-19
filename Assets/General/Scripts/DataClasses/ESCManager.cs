using UnityEngine;
using System;

public class ESCManager : MonoBehaviour
{
    public static ESCManager Instance { get; private set; }
    public static Action OnCancelPressed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCancelPressed?.Invoke();
        }
    }
}
