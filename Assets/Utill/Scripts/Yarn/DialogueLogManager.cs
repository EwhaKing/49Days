using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueLogManager : MonoBehaviour
{
    public static DialogueLogManager Instance { get; private set; }

    private readonly List<DialogueLogEntry> dialogueLogs = new();
    [SerializeField] private TextMeshProUGUI logTextUI;
    [SerializeField] private LogScrollController logScrollController;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddLog(string characterName, string text)
    {
        dialogueLogs.Add(new DialogueLogEntry(characterName, text));
        Debug.Log("�α� ȣ�� �Ϸ�");
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
}