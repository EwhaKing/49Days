using UnityEditor.SearchService;
using UnityEngine;
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public GameObject pausedPanel;
    public GameObject mainMenuPanel;
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
        {
            pausedPanel.SetActive(false);
        }
    }
    void OnDisable()
    {
        ESCManager.OnCancelPressed -= ESCInput;
    }
    private void ESCInput()
    {
        if (mainMenuPanel.activeSelf)
        {
            return; 
        }
        pausedPanel.SetActive(!pausedPanel.activeSelf);
        Debug.Log("일시정지");
    }
}