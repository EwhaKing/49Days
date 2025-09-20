#nullable enable
using System.Threading;
using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.UI;

public class FieldDialogueExtraPresenter : DialoguePresenterBase
{
    [Header("UI References")]
    public RectTransform? dialogueBox;
    public RectTransform? nameBox;
    public TextMeshProUGUI? dialogueText;
    public TextMeshProUGUI? nameText;

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;

    [SerializeField] private DialogueInputHandler? dialogueInputHandler;

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
        // 타이핑 상태에 따라 동작 실행
        if (phase == Phase.Typing)
            isClickedForSkip = true;
        else
            isClickedForNext = true;
    }

    private void Awake()
    {
        // TMP 설정
        if (dialogueText != null)
        {
            dialogueText.fontSize = 42;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
        }
        if (nameText != null)
        {
            nameText.fontSize = 42;
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

        string characterName = line.CharacterName ?? "";
        string processedText = string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.Text.Text
            : line.TextWithoutCharacterName.Text;

        // TMP에 텍스트 할당
        dialogueText!.text = processedText;
        dialogueText.ForceMeshUpdate();
        nameText!.text = characterName;
        nameText.ForceMeshUpdate();

        CalculateNameBoxSize(characterName);
        PositionNameBox();

        // 로그 추가
        // DialogueLogManager.Instance.AddLog(characterName, processedText);

        // 타이핑 스킵 입력 대기
        await TypeTextWithSkipAsync(processedText);

        // 다음 패널으로 넘기는 입력 대기
        await WaitForClickAsync();

        dialogueBox!.gameObject.SetActive(false);
        nameBox!.gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (dialogueBox == null) throw new System.InvalidOperationException("dialogueBox가 할당되지 않았습니다.");
        if (nameBox == null) throw new System.InvalidOperationException("nameBox가 할당되지 않았습니다.");
        if (dialogueText == null) throw new System.InvalidOperationException("dialogueTex가 할당되지 않았습니다.");
        if (nameText == null) throw new System.InvalidOperationException("nameText가 할당되지 않았습니다.");
        if (dialogueInputHandler == null) throw new System.InvalidOperationException("dialogueInputHandler가 할당되지 않았습니다.");

        mainCamera ??= Camera.main ?? throw new System.InvalidOperationException("Main Camera가 할당되지 않았습니다.");
        if (dialogueBox.GetComponentInParent<Canvas>() == null) throw new System.InvalidOperationException("dialogueBox가 할당되지 않았습니다.");
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

    private void PositionNameBox()
    {
        // NameBox 위치를 DialogueBox 위치에 맞춰 설정
        nameBox!.SetParent(dialogueBox, false);
        nameBox.anchorMin = new Vector2(0f, 1f);
        nameBox.anchorMax = new Vector2(0f, 1f);
        nameBox.pivot = new Vector2(0f, 1f);
        nameBox.anchoredPosition = new Vector2(10f, 20f);
    }

    private async YarnTask TypeTextWithSkipAsync(string processedText)
    {
        const float typingSpeed = 0.04f;
        dialogueText!.text = "";

        // 상태: 타이핑 중
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

        // 텍스트 전부 표시
        if (!skipped && dialogueText.text != processedText)
            dialogueText.text = processedText;

        // 상태: 다음 패널 대기
        phase = Phase.AwaitingNext;

        // 클릭 소비
        isClickedForSkip = false;
    }

    private async YarnTask WaitOrSkipAsync(float seconds, System.Action onSkip)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (isClickedForSkip)
            {
                // 클릭 소비
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

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
        => YarnTask.FromResult<DialogueOption?>(null);

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;
}
