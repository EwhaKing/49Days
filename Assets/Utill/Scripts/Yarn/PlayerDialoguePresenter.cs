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
    public RectTransform? playerNicknamePanel;

    public OptionPanelController? optionPanelController;

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;

    [SerializeField] private DialogueInputHandler? dialogueInputHandler;
    [SerializeField] private DialogueRunner? runner;

    void OnEnable()
    {
        if (dialogueInputHandler != null)
            dialogueInputHandler.OnDialogueContinueRequested += OnDialogueContinue;
    }

    void OnDestroy()
    {
        if (dialogueInputHandler != null)
            dialogueInputHandler.OnDialogueContinueRequested -= OnDialogueContinue;
    }

    public void OnPanelClicked()
    {
        if (phase == Phase.Typing)
            isClickedForSkip = true;
        else
            isClickedForNext = true;
    }

    private void Awake()
    {
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

        string characterName = GetPlayerNameFromVariableStorage();

        string processedText = string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.Text.Text
            : line.TextWithoutCharacterName.Text;

        dialogueText!.text = processedText;
        dialogueText.ForceMeshUpdate();
        nameText!.text = characterName;
        nameText.ForceMeshUpdate();

        CalculateDialogueBoxSize(processedText);
        CalculateNameBoxSize(characterName);

        PositionDialogueBox();
        PositionNameBox();

        DialogueLogManager.Instance.AddLog(characterName, processedText);

        await TypeTextWithSkipAsync(processedText);
        await WaitForClickAsync();

        dialogueBox!.gameObject.SetActive(false);
        nameBox!.gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (dialogueBox == null) throw new System.InvalidOperationException("dialogueBox가 할당되지 않았습니다.");
        if (nameBox == null) throw new System.InvalidOperationException("nameBox가 할당되지 않았습니다.");
        if (dialogueText == null) throw new System.InvalidOperationException("dialogueText가 할당되지 않았습니다.");
        if (nameText == null) throw new System.InvalidOperationException("nameText가 할당되지 않았습니다.");
        if (dialogueInputHandler == null) throw new System.InvalidOperationException("dialogueInputHandler가 할당되지 않았습니다.");
    
            mainCamera ??= Camera.main ?? throw new System.InvalidOperationException("Main Camera가 할당되지 않았습니다.");
        if (dialogueBox.GetComponentInParent<Canvas>() == null) throw new System.InvalidOperationException("dialogueBox의 부모인 Canvas가 존재하지 않습니다.");
    }

    private void CalculateDialogueBoxSize(string processedText)
    {
        const float minWidth = 180f, minHeight = 65f;
        const float paddingLeft = 30f, paddingRight = 80f;
        const float paddingTop = 60f, paddingBottom = 40f;
        const float maxWidth = 650f;

        float textWidthLimit = maxWidth - (paddingLeft + paddingRight);

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
        dialogueBox!.anchorMin = new Vector2(0.5f, 1f);
        dialogueBox.anchorMax = new Vector2(0.5f, 1f);
        dialogueBox.pivot = new Vector2(0.5f, 1f);

        dialogueBox.anchoredPosition = new Vector2(0, -470f);
    }

    private void PositionNameBox()
    {
        nameBox!.SetParent(dialogueBox, false);
        nameBox.anchorMin = new Vector2(0f, 1f);
        nameBox.anchorMax = new Vector2(0f, 1f);
        nameBox.pivot = new Vector2(0f, 1f);
        nameBox.anchoredPosition = new Vector2(-10f, 20f);
    }

    private async YarnTask TypeTextWithSkipAsync(string processedText)
    {
        const float typingSpeed = 0.04f;
        const float commaPause = 0.3f;
        const float dotPause = 0.2f;
        dialogueText!.text = "";

        phase = Phase.Typing;

        bool skipped = false;
        int i = 0;
        string visibleText = "";
        while (i < processedText.Length)
        {
            if (skipped)
            {
                dialogueText.text = processedText;
                break;
            }

            // < > 태그 즉시 적용
            if (processedText[i] == '<')
            {
                int tagEnd = processedText.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    visibleText += processedText.Substring(i, tagEnd - i + 1);
                    i = tagEnd + 1;
                    continue;
                }
            }

            // ... 처리
            if (processedText[i] == '.' && i + 2 < processedText.Length &&
                processedText[i + 1] == '.' && processedText[i + 2] == '.')
            {
                for (int d = 0; d < 3; d++)
                {
                    visibleText += ".";
                    dialogueText.text = visibleText;
                    i++;
                    await WaitOrSkipAsync(dotPause, () => skipped = true);
                    if (skipped) break;
                }
                continue;
            }

            // 쉼표 처리
            visibleText += processedText[i];
            dialogueText.text = visibleText;
            if (processedText[i] == ',')
            {
                i++;
                await WaitOrSkipAsync(commaPause, () => skipped = true);
                continue;
            }

            i++;
            await WaitOrSkipAsync(typingSpeed, () => skipped = true);
        }

        if (!skipped && dialogueText.text != processedText)
            dialogueText.text = processedText;

        phase = Phase.AwaitingNext;
        isClickedForSkip = false;
    }

    private async YarnTask WaitOrSkipAsync(float seconds, System.Action onSkip)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (isClickedForSkip)
            {
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
            throw new System.InvalidOperationException("optionPanelController가 할당되지 않았습니다.");

        DialoguePresenterRouter.isOptionPanelActive = true;

        int selected = await optionPanelController.ShowOptionsAsync(dialogueOptions, cancellationToken);

        DialoguePresenterRouter.isOptionPanelActive = false;

        return dialogueOptions[selected];
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;

    private string GetPlayerNameFromVariableStorage()
    {
        if (runner?.VariableStorage == null)
            return "Player";
        if (runner.VariableStorage.TryGetValue("$playerName", out object value) && value != null)
            return value.ToString();
        return "Player";
    }

    public void ShowNicknameInputPanel(System.Action<string> onConfirm)
    {
        if (playerNicknamePanel == null)
        {
            Debug.LogError("playerNicknamePanel이 할당되지 않았습니다.");
            return;
        }
        playerNicknamePanel.gameObject.SetActive(true);
        InputManager.Instance.BlockAllInput(true);

        var inputField = playerNicknamePanel.GetComponentInChildren<TMPro.TMP_InputField>();
        var confirmButton = playerNicknamePanel.GetComponentInChildren<Button>();

        if (inputField == null || confirmButton == null)
        {
            Debug.LogError("닉네임 입력 UI 구성요소를 찾을 수 없습니다.");
            return;
        }

        inputField.characterLimit = 10;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            string nickname = inputField.text.Trim();
            if (string.IsNullOrEmpty(nickname))
            {
                Debug.Log("닉네임이 입력되지 않았습니다.");
                return;
            }
            playerNicknamePanel.gameObject.SetActive(false);
            InputManager.Instance.BlockAllInput(false);
            onConfirm?.Invoke(nickname);
        });
    }
}
