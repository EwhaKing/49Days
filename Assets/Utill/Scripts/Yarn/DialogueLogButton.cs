using UnityEngine;
using UnityEngine.UI;

public class DialogueLogButtons : MonoBehaviour
{
    [SerializeField] private GameObject logPanel;
    [SerializeField] private Button targetButton;

    private void Start()
    {
        if (logPanel != null)
            logPanel.SetActive(false);
    }

    public void ToggleLog()
    {
        if (logPanel != null)
            logPanel.SetActive(!logPanel.activeSelf);
    }
}