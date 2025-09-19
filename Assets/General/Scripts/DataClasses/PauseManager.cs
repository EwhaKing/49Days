using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public GameObject pausedPanel;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        ESCManager.OnCancelPressed += ESCInput;
    }
    void OnDisable()
    {
        ESCManager.OnCancelPressed -= ESCInput;
    }
    private void ESCInput()
    {
         Debug.Log("등장");
        pausedPanel.SetActive(!pausedPanel.activeSelf);
        if (pausedPanel.activeSelf)
        {
            //Time.timeScale = 0f;
            Debug.Log("일시정지");
        }
        else
        {
            //Time.timeScale = 1f;
            Debug.Log("재생");
        }
    }
}