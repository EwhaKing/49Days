using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    public GameObject optionsPanel;
    private UIInputHandler uiInputHandler;

    void Start()
    {
        uiInputHandler = FindObjectOfType<UIInputHandler>();
        uiInputHandler.OnCloseUIRequested += CloseOptionsPanel;
        optionsPanel.SetActive(false);
    }

    void OnDestroy()
    {
        uiInputHandler.OnCloseUIRequested -= CloseOptionsPanel;
    }

    public void OpenOptionsPanel()
    {
        optionsPanel.SetActive(true);
    }
    public void CloseOptionsPanel()
    {
        optionsPanel.SetActive(false);
    }
}
