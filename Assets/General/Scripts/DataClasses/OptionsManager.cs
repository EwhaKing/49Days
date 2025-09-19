using UnityEngine;

public class OptionsManager : Singleton<OptionsManager>
{
    public GameObject optionsPanel;
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
        if (optionsPanel.activeSelf)
        {
            CloseOptionsPanel();
        }
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
