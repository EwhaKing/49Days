#nullable enable
using System.Threading;
using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.UI;

public class PlayerDialoguePresenter : DialoguePresenterBase
{
    [Header("UI References")]
    public RectTransform? dialogueBox;
    public RectTransform? nameBox;
    public TextMeshProUGUI? dialogueText;
    public TextMeshProUGUI? nameText;

    public OptionPanelController? optionPanelController;

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;

    public void OnPanelClicked()
    {
        // Ŭ���� �ǹ̸� ���� �ܰ� �������� ����
        if (phase == Phase.Typing)
            isClickedForSkip = true;
        else
            isClickedForNext = true;
    }

    private void Awake()
    {
        // UI �⺻ ��Ÿ���� �� ���� ����
        if (dialogueText != null)
        {
            dialogueText.fontSize = 34;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
        }
        if (nameText != null)
        {
            nameText.fontSize = 34;
            nameText.alignment = TextAlignmentOptions.Center;
        }
    }

    public void OnDialogueContinue()
    {
        if (DialoguePresenterRouter.isOptionPanelActive)
            return;
        if (DialogueLogManager.Instance != null && DialogueLogManager.Instance.IsLogPanelOpen())
            return;
        if (dialogueBox != null && dialogueBox.gameObject.activeSelf)
            OnPanelClicked();
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken cancellationToken)
    {
        ValidateReferences();

        isClickedForSkip = false;
        isClickedForNext = false;

        dialogueBox!.gameObject.SetActive(true);
        nameBox!.gameObject.SetActive(true);

        string characterName = line.CharacterName ?? "Player";
        string processedText = string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.Text.Text
            : line.TextWithoutCharacterName.Text;

        // �ؽ�Ʈ ����
        dialogueText!.text = processedText;
        dialogueText.ForceMeshUpdate();
        nameText!.text = characterName;
        nameText.ForceMeshUpdate();

        // ũ�� ���
        CalculateDialogueBoxSize(processedText);
        CalculateNameBoxSize(characterName);

        // ��ġ ���
        PositionDialogueBox();
        PositionNameBox();

        // �α� �߰�
        DialogueLogManager.Instance.AddLog(characterName, processedText);

        // Ÿ���� ȿ�� (Ŭ�� �� ��ŵ ����)
        await TypeTextWithSkipAsync(processedText);

        // ���� ���� �Ѿ�� �� Ŭ�� ���
        await WaitForClickAsync();

        dialogueBox!.gameObject.SetActive(false);
        nameBox!.gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (dialogueBox == null) throw new System.InvalidOperationException("dialogueBox�� �Ҵ���� �ʾҽ��ϴ�.");
        if (nameBox == null) throw new System.InvalidOperationException("nameBox�� �Ҵ���� �ʾҽ��ϴ�.");
        if (dialogueText == null) throw new System.InvalidOperationException("dialogueText�� �Ҵ���� �ʾҽ��ϴ�.");
        if (nameText == null) throw new System.InvalidOperationException("nameText�� �Ҵ���� �ʾҽ��ϴ�.");

        mainCamera ??= Camera.main ?? throw new System.InvalidOperationException("Main Camera�� ã�� �� �����ϴ�.");
        if (dialogueBox.GetComponentInParent<Canvas>() == null) throw new System.InvalidOperationException("dialogueBox�� ������ Canvas�� �����ϴ�.");
    }

    private void CalculateDialogueBoxSize(string processedText)
    {
        const float minWidth = 180f, minHeight = 65f;
        const float paddingLeft = 30f, paddingRight = 80f;
        const float paddingTop = 60f, paddingBottom = 40f;
        const float maxWidth = 650f;

        float textWidthLimit = maxWidth - (paddingLeft + paddingRight);

        // TMP�� �� ���� ���� �� ���� ���
        var preferredSize = dialogueText!.GetPreferredValues(processedText, textWidthLimit, 0f);

        float finalWidth = Mathf.Clamp(preferredSize.x + paddingLeft + paddingRight, minWidth, maxWidth);
        float finalHeight = Mathf.Max(minHeight, preferredSize.y + paddingTop + paddingBottom);

        dialogueBox!.sizeDelta = new Vector2(finalWidth, finalHeight);
        dialogueText.rectTransform.sizeDelta = new Vector2(finalWidth - (paddingLeft + paddingRight), finalHeight - (paddingTop + paddingBottom));
        dialogueText.rectTransform.anchoredPosition = new Vector2(paddingLeft, -paddingTop);
    }

    private void CalculateNameBoxSize(string characterName)
    {
        const float minWidth = 60f;
        const float paddingX = 20f;
        const float paddingY = 10f;

        float nameTextWidth = nameText!.preferredWidth;
        float nameTextHeight = nameText.preferredHeight;

        float finalWidth = Mathf.Max(minWidth, nameTextWidth + paddingX * 2);
        float finalHeight = nameTextHeight + paddingY * 2;

        nameBox!.sizeDelta = new Vector2(finalWidth, finalHeight);
    }

    private void PositionDialogueBox()
    {
        // ��� �߾� ����
        dialogueBox!.anchorMin = new Vector2(0.5f, 1f);
        dialogueBox.anchorMax = new Vector2(0.5f, 1f);
        dialogueBox.pivot = new Vector2(0.5f, 1f);

        dialogueBox.anchoredPosition = new Vector2(0, -470f);
    }

    private void PositionNameBox()
    {
        // NameBox�� DialogueBox�� �»�ܿ� ���̱�
        nameBox!.SetParent(dialogueBox, false);
        nameBox.anchorMin = new Vector2(0f, 1f);
        nameBox.anchorMax = new Vector2(0f, 1f);
        nameBox.pivot = new Vector2(0f, 1f);
        nameBox.anchoredPosition = new Vector2(-10f, 20f);
    }

    private async YarnTask TypeTextWithSkipAsync(string processedText)
    {
        const float typingSpeed = 0.04f;
        dialogueText!.text = "";

        // Ÿ���� �ܰ� ����
        phase = Phase.Typing;

        bool skipped = false;
        foreach (char c in processedText)
        {
            if (skipped)
            {
                dialogueText.text = processedText;
                break;
            }

            dialogueText.text += c;
            await WaitOrSkipAsync(typingSpeed, () => skipped = true);
        }

        // Ÿ������ �ڿ� ����� ��쿡�� ��ü ǥ��
        if (!skipped && dialogueText.text != processedText)
            dialogueText.text = processedText;

        // ���� Ŭ�� ��� �ܰ�� ��ȯ
        phase = Phase.AwaitingNext;

        // ��ŵ �÷��״� ���⼭ �ʱ�ȭ
        isClickedForSkip = false;
    }

    private async YarnTask WaitOrSkipAsync(float seconds, System.Action onSkip)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (isClickedForSkip)
            {
                // ��ŵ �Һ�
                isClickedForSkip = false;
                onSkip();
                break;
            }
            elapsed += Time.deltaTime;
            await YarnTask.Yield();
        }
    }

    private async YarnTask WaitForClickAsync()
    {
        while (!isClickedForNext)
            await YarnTask.Yield();

        isClickedForNext = false;
    }


    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        ValidateReferences();
        if (optionPanelController == null)
            throw new System.InvalidOperationException("optionPanelController�� �Ҵ���� �ʾҽ��ϴ�.");

        DialoguePresenterRouter.isOptionPanelActive = true; // �ɼ� �г� Ȱ��ȭ

        int selected = await optionPanelController.ShowOptionsAsync(dialogueOptions, cancellationToken);

        DialoguePresenterRouter.isOptionPanelActive = false; // �ɼ� �г� ��Ȱ��ȭ

        return dialogueOptions[selected];
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;
}
