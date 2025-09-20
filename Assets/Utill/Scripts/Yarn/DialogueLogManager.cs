using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLogManager : SceneSingleton<DialogueLogManager>
{
    private static List<DialogueLogEntry> dialogueLogs = new();
    [SerializeField] private TextMeshProUGUI logTextUI;
    [SerializeField] private LogScrollController logScrollController;
    [SerializeField] private GameObject logPanel;
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

        // LogText�� Ȱ��ȭ�� ��쿡�� �ڷ�ƾ ����
        if (logTextUI.gameObject.activeInHierarchy)
            StartCoroutine(ScrollAndUpdate());
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
        return logPanel.activeSelf;
    }

    public void ToggleLog()
    {
        if (GameFlowManager.IsInField()) return;

        logPanel.SetActive(!logPanel.activeSelf);
        if (logPanel.activeSelf)
            GameManager.Instance.onUIOn.Invoke();
    }

    public void CloseLog()
    {
        logPanel.SetActive(false);
    }
}