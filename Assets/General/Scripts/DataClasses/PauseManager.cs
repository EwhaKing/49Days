using UnityEditor.SearchService;
using UnityEngine;
public class PauseManager : MonoBehaviour
{
    public GameObject pausedPanel;
    private UIInputHandler uiInputHandler;

    void Start()
    {
        uiInputHandler = FindObjectOfType<UIInputHandler>();
        uiInputHandler.OnCloseUIRequested += PauseMenuUI;
        pausedPanel.SetActive(false);
    }
    void OnDestroy()
    {
        uiInputHandler.OnCloseUIRequested -= PauseMenuUI;
    }
    public void PauseMenuUI()
    {
        pausedPanel.SetActive(!pausedPanel.activeSelf);
    }
    public void GotoMainMenu()
    {
        GameFlowManager.LoadScene(GameFlowManager.START_SECENE_NAME);
    }
}