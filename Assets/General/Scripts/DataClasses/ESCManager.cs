using UnityEngine;
using System;

public class ESCManager : MonoBehaviour
{
    public static ESCManager Instance { get; private set; }
    public static Action OnCancelPressed;
    private UIInputHandler uiInputHandler;

    void OnEnable()
    {
        uiInputHandler = FindObjectOfType<UIInputHandler>();
        uiInputHandler.OnCloseUIRequested += PauseMenuUI;
    }

    void OnDisable()
    {
        uiInputHandler.OnCloseUIRequested += PauseMenuUI;

    }

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

    void PauseMenuUI()
    {
        OnCancelPressed?.Invoke();
    }
}
