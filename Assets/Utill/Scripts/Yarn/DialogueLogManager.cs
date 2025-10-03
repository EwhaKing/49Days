using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLogManager : SceneSingleton<DialogueLogManager>
{
    private static readonly List<DialogueLogEntry> dialogueLogs = new();
    [SerializeField] private TextMeshProUGUI logTextUI;
    [SerializeField] private GameObject logPanel;
    [SerializeField] private ScrollRect scrollRect;
    private UIInputHandler uiInputHandler;

    private void OnEnable() {
        uiInputHandler = FindObjectOfType<UIInputHandler>();
        Debug.Assert(uiInputHandler != null, "DialogueLogManager: UIInputHandler not found.");
        uiInputHandler.OnCloseUIRequested += CloseLog;
    }
    private void OnDisable() {
        uiInputHandler.OnCloseUIRequested -= CloseLog;
    }

    private void Start()
    {
        Debug.Assert(scrollRect != null, "ScrollRect 없음");
        Debug.Assert(logPanel != null, "로그패널 없음");
        Debug.Assert(logTextUI != null, "로그텍스트UI 없음");
        logPanel.SetActive(false);
        logTextUI.text = "";
        foreach (var log in dialogueLogs)
        {
            logTextUI.text += $"{log.CharacterName}: {log.Text}\n";
        } 
    }

    public void AddLog(string characterName, string text)
    {
        dialogueLogs.Add(new DialogueLogEntry(characterName, text));
        logTextUI.text += $"{characterName}: {text}\n";
    }

    public IReadOnlyList<DialogueLogEntry> GetLogs() => dialogueLogs;

    public bool IsLogPanelOpen()
    {
        return logPanel.activeSelf;
    }

    public void ToggleLog() 
    {
        if (GameFlowManager.IsInField()) return;
        if (GameFlowManager.IsInStart()) return;

        logPanel.SetActive(!logPanel.activeSelf);
        if (logPanel.activeSelf)
        {
            logTextUI.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            CoroutineUtil.Instance.RunAfterFirstFrame(()=>scrollRect.verticalNormalizedPosition = 0f);
            GameManager.Instance.onUIOn?.Invoke();
        }
    }

    public void CloseLog()
    {
        logPanel.SetActive(false);
    }
}