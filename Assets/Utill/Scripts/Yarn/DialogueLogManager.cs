using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLogManager : Singleton<DialogueLogManager>
{
    private readonly List<DialogueLogEntry> dialogueLogs = new();
    [SerializeField] private TextMeshProUGUI logTextUI;
    [SerializeField] private LogScrollController logScrollController;
    [SerializeField] private GameObject logPanel;

    public void AddLog(string characterName, string text)
    {
        dialogueLogs.Add(new DialogueLogEntry(characterName, text));
        UpdateLogUI();

        // LogText�� Ȱ��ȭ�� ��쿡�� �ڷ�ƾ ����
        if (logScrollController != null && logTextUI != null && logTextUI.gameObject.activeInHierarchy)
            StartCoroutine(ScrollAndUpdate());
    }

    private void UpdateLogUI()
    {
        if (logTextUI == null) return;
        logTextUI.text = "";
        foreach (var log in dialogueLogs)
        {
            logTextUI.text += $"{log.CharacterName}: {log.Text}\n";
        }
    }

    private IEnumerator ScrollAndUpdate()
    {
        yield return null; // �� ������ ���(���̾ƿ� ���� ��)
        logScrollController.UpdateScrollInteractable();
        logScrollController.ScrollToBottom();
    }

    public IReadOnlyList<DialogueLogEntry> GetLogs() => dialogueLogs;

    public bool IsLogPanelOpen()
    {
        return logPanel != null && logPanel.activeSelf;
    }
}