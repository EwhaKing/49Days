#nullable enable
using System;
using System.Threading;
using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;


public class FieldDialoguePresenter : DialoguePresenterBase
{
    [Header("UI References")]
    public RectTransform? dialogueBox;
    public RectTransform? nameBox;
    public TextMeshProUGUI? dialogueText;
    public TextMeshProUGUI? nameText;
    public GameObject? characterPanel;

    public FieldOptionPanelController? fieldOptionPanelController;

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;

    [SerializeField] private DialogueInputHandler? dialogueInputHandler;
    [SerializeField] private DialogueRunner? runner;

    [SerializeField] private Image? bodyImage;

    private CharacterData? currentCharacterData;
    private CharacterPose? currentPose;

    private int entryCount = 0;

    private Dictionary<string, bool[]> entryOptionClicked = new(); // [0]:잡담, [1]:질문, [2]:선물

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
        {
            isClickedForSkip = true;
            return;
        }

        // 옵션 패널이 활성화되어 있고, 첫 진입 상태라면 대화 종료
        if (ShouldEndDialogue())
        {
            EndDialogue();
            return;
        }

        // 그 외에는 다음 입력 처리
        isClickedForNext = true;
    }

    private bool ShouldEndDialogue()
    {
        return FieldDialoguePresenterRouter.isOptionPanelActive && entryCount == 0;
    }

    private void EndDialogue()
    {
        dialogueBox?.gameObject.SetActive(false);
        nameBox?.gameObject.SetActive(false);
        characterPanel?.gameObject.SetActive(false);

        fieldOptionPanelController?.ForceCloseOptions();

        runner?.Stop();
    }

    /// <summary>
    /// Router에서 호출해서 데이터 할당해줌
    /// </summary>
    public void SetCharacterData(CharacterData? data)
    {
        currentCharacterData = data;
    }

    private void ShowCharacter(string characterName, string poseName = "기본")
    {
        if (currentCharacterData == null)
        {
            return;
        }

        currentPose = currentCharacterData.poses.Find(p => p.poseName == poseName)
            ?? currentCharacterData.poses.FirstOrDefault();

        if (currentPose != null)
        {
            if (bodyImage != null)
            {
                bodyImage.sprite = currentPose.bodySprite;
                bodyImage.preserveAspect = true;
            }
        }
    }

    public void ChangeCharacterPose(string poseName)
    {
        var newPose = currentCharacterData.poses.Find(p => p.poseName == poseName);
        if (newPose != null)
        {
            currentPose = newPose;
            if (bodyImage != null)
            {
                bodyImage.sprite = currentPose.bodySprite;
                bodyImage.preserveAspect = true;
            }
        }
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
        if (FieldDialoguePresenterRouter.isOptionPanelActive)
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
        characterPanel!.gameObject.SetActive(true);

        string characterName = line.CharacterName ?? "";
        string processedText = string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.Text.Text
            : line.TextWithoutCharacterName.Text;

        string poseName = currentPose?.poseName ?? currentCharacterData.poses[0].poseName;
        ShowCharacter(characterName, poseName);

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

        if (entryCount == 0)
        {
            return;
        }

        // 다음 패널로 넘기는 입력 대기
        await WaitForClickAsync();

        dialogueBox!.gameObject.SetActive(false);
        nameBox!.gameObject.SetActive(false);
        characterPanel!.gameObject.SetActive(false);
    }

    private void ValidateReferences()
    {
        Debug.Assert(dialogueBox != null, "dialogueBox가 할당되지 않았습니다.");
        Debug.Assert(nameBox != null, "nameBox가 할당되지 않았습니다.");
        Debug.Assert(dialogueText != null, "dialogueText가 할당되지 않았습니다.");
        Debug.Assert(nameText != null, "nameText가 할당되지 않았습니다.");
        Debug.Assert(dialogueInputHandler != null, "dialogueInputHandler가 할당되지 않았습니다.");

        mainCamera ??= Camera.main;
        Debug.Assert(mainCamera != null, "Main Camera가 할당되지 않았습니다.");
        Debug.Assert(dialogueBox.GetComponentInParent<Canvas>() != null, "dialogueBox의 부모인 Canvas가 존재하지 않습니다.");
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

    /// <summary>
    /// 첫 진입 시(entryCount == 0) 엔트리 옵션 패널을 커스텀 방식으로 띄움
    /// 이후에는 일반 옵션 패널을 Yarn 기본 방식으로 띄움
    /// 캐릭터별 entryOption 클릭 상태 저장해서 FieldOptionPanelController에 넘김
    /// </summary>
    public override async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        ValidateReferences();
        Debug.Assert(fieldOptionPanelController != null, "FieldOptionPanelController가 할당되지 않았습니다.");

        FieldDialoguePresenterRouter.isOptionPanelActive = true;

        string characterName = currentCharacterData?.characterName ?? "";

        if (entryCount == 0)
        {
            var clickedStates = GetEntryOptionClicked(characterName);
            fieldOptionPanelController.ShowEntryOptions(dialogueOptions, clickedStates);

            Action<int> entryHandler = null!;
            var tcs = new TaskCompletionSource<int>();

            entryHandler = (selectedIdx) => {
                SetEntryOptionClicked(characterName, selectedIdx); // 클릭 상태 저장
                tcs.TrySetResult(selectedIdx);
                fieldOptionPanelController.OnEntryOptionSelected -= entryHandler;
            };

            fieldOptionPanelController.OnEntryOptionSelected += entryHandler;

            int selected = await tcs.Task;

            FieldDialoguePresenterRouter.isOptionPanelActive = false;
            entryCount++;
            return dialogueOptions[selected];
        }

        int normalSelected = await fieldOptionPanelController.ShowOptionsAsync(dialogueOptions, cancellationToken);

        FieldDialoguePresenterRouter.isOptionPanelActive = false;
        return dialogueOptions[normalSelected];
    }

    // 해당 캐릭터의 entryOption 클릭 상태 배열 Get
    public bool[] GetEntryOptionClicked(string characterName)
    {
        if (!entryOptionClicked.ContainsKey(characterName))
            entryOptionClicked[characterName] = new bool[3];
        return entryOptionClicked[characterName];
    }

    // 해당 캐릭터의 entryOption 클릭 상태 배열 Set
    public void SetEntryOptionClicked(string characterName, int idx)
    {
        var arr = GetEntryOptionClicked(characterName);
        arr[idx] = true;
    }

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync()
    {
        entryCount = 0;
        ChangeCharacterPose(currentCharacterData.poses[0].poseName);
        return YarnTask.CompletedTask;
    }
}
