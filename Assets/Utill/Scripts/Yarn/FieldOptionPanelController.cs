#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class FieldOptionPanelController : MonoBehaviour
{
    public RectTransform? panel;
    public GameObject? fieldOptionButtonPrefab;
    public VerticalLayoutGroup? layoutGroup;

    // ��ư ��Ÿ�� ���
    private const float ButtonWidth = 1000;
    private const float ButtonHeight = 100f;
    private const int FontSize = 42;

    private TaskCompletionSource<int>? selectionSource;
    private DialogueOption[]? currentOptions;


    /// <summary>
    /// �Ϲ����� �ɼ� ó���� �� �ҷ���
    /// </summary>
    public async Task<int> ShowOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        // ���� ��ư ����
        foreach (Transform child in panel!)
            Destroy(child.gameObject);

        // �ɼ� �迭 ����
        currentOptions = options;

        // spacing ����
        if (layoutGroup != null)
        {
            switch (options.Length)
            {
                case 1: layoutGroup.spacing = 0f; break;
                case 2: layoutGroup.spacing = 55f; break;
                case 3: layoutGroup.spacing = 30f; break;
                default: break;
            }
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        }

        // �г� ��ġ ���� (��� ����)
        if (panel != null)
        {
            panel.anchorMin = new Vector2(0.5f, 1f); // ��� �߾� anchor
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);

            float yOffset = 0f;
            switch (options.Length)
            {
                case 1: yOffset = -1150f; break;
                case 2: yOffset = -1080f; break;
                case 3: yOffset = -1030f; break;
                default: break;
            }
            panel.anchoredPosition = new Vector2(0f, yOffset);
        }

        // ��ư ���� �� ��Ÿ�� ����
        for (int i = 0; i < options.Length; i++)
        {
            var btnObj = Instantiate(fieldOptionButtonPrefab!, panel);
            var btnRect = btnObj.GetComponent<RectTransform>();
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // ��ư ũ�� ����
            if (btnRect != null)
                btnRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            // �ؽ�Ʈ ��Ÿ�� ����
            if (txt != null)
            {
                txt.text = options[i].Line.TextWithoutCharacterName.Text;
                txt.fontSize = FontSize;
                txt.alignment = TextAlignmentOptions.Center;
                txt.enableWordWrapping = true;
            }

            int idx = i;

            // �� ��ư�� Ŭ�� �̺�Ʈ ���
            btn.onClick.AddListener(() => OnOptionSelected(idx));
        }

        // �ɼ� �г� Ȱ��ȭ
        panel!.gameObject.SetActive(true);

        // ���õ� �ɼ��� �����ϴ� ��ü ����
        selectionSource = new TaskCompletionSource<int>();

        // ��� ��ū ��� �� ���
        using (cancellationToken.Register(() => selectionSource.TrySetCanceled()))
        {
            int result = await selectionSource.Task;
            panel.gameObject.SetActive(false);
            return result;
        }
    }

    // �ɼ� ���� �� ȣ�� (���õ� �ε��� ����)
    private void OnOptionSelected(int index)
    {
        // �α׿� ����
        if (currentOptions != null && index >= 0 && index < currentOptions.Length)
        {
            var option = currentOptions[index];
            DialogueLogManager.Instance.AddLog("", option.Line.TextWithoutCharacterName.Text);
        }

        selectionSource?.TrySetResult(index);
    }

    /// <summary>
    /// FieldDialoguePresenter�� EŰ�� ���� ȣ��� �� �ҷ���
    /// Yarn�� �⺻ �г��� �ƴ� Ŀ���� UI ���
    /// </summary>
    public event Action<int>? OnEntryOptionSelected;
    public void ShowEntryOptions(DialogueOption[] options)
    {
        // ���� ��ư ����
        foreach (Transform child in panel!)
            Destroy(child.gameObject);

        // spacing/���� ����
        if (layoutGroup != null)
        {
            layoutGroup.spacing = 0f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        }

        // �г� ��ġ ����
        if (panel != null)
        {
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(800f, -600f);
        }

        // ��ư ���� �� ��Ÿ�� ����
        for (int i = 0; i < options.Length; i++)
        {
            var btnObj = Instantiate(fieldOptionButtonPrefab!, panel);
            var btnRect = btnObj.GetComponent<RectTransform>();
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (btnRect != null)
                btnRect.sizeDelta = new Vector2(700f, 120f);

            if (txt != null)
            {
                txt.text = options[i].Line.TextWithoutCharacterName.Text;
                txt.fontSize = 40;
                txt.alignment = TextAlignmentOptions.Center;
                txt.enableWordWrapping = true;
            }

            int idx = i;
            btn.onClick.AddListener(() => {
                panel!.gameObject.SetActive(false);
                OnEntryOptionSelected?.Invoke(idx);
            });
        }

        panel!.gameObject.SetActive(true);
    }
}
