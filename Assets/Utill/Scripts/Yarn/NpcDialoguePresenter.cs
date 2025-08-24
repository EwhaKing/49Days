#nullable enable
using System.Threading;
using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.UI;

public class NpcDialoguePresenter : DialoguePresenterBase
{
    [Header("UI References")]
    public RectTransform? dialogueBox;
    public TextMeshProUGUI? dialogueText;
    public RectTransform? nameBox;
    public TextMeshProUGUI? nameText;

    [Header("NPC Settings")]
    public Transform? npcTransform;                // NPC 월드 좌표 (동적 할당)
    public Vector2 nameBoxOffset = new(-10f, 20f); // NameBox 오프셋 (좌상단 기준)

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;

    public void OnPanelClicked()
    {
        // 클릭의 의미를 현재 단계 기준으로 결정
        if (phase == Phase.Typing)
            isClickedForSkip = true;
        else
            isClickedForNext = true;
    }

    private void Awake()
    {
        // UI 기본 스타일은 한 번만 설정
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

        string characterName = line.CharacterName ?? "";
        string processedText = !string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.TextWithoutCharacterName.Text
            : line.Text.Text;

        // 텍스트 적용
        dialogueText!.text = processedText;
        dialogueText.ForceMeshUpdate();
        nameText!.text = characterName;
        nameText.ForceMeshUpdate();

        // 크기 계산
        CalculateDialogueBoxSize(processedText);
        CalculateNameBoxSize(characterName);

        // 위치 계산
        PositionDialogueBox();
        PositionNameBox();

        // 로그 추가
        DialogueLogManager.Instance.AddLog(characterName, processedText);

        // 타이핑 효과 (클릭 시 스킵 가능)
        await TypeTextWithSkipAsync(processedText);

        // 다음 대사로 넘어가기 전 클릭 대기
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
        if (npcTransform == null) throw new System.InvalidOperationException("npcTransform이 할당되지 않았습니다.");

        mainCamera = Camera.main ?? throw new System.InvalidOperationException("Main Camera를 찾을 수 없습니다.");
        if (dialogueBox.GetComponentInParent<Canvas>() == null) throw new System.InvalidOperationException("dialogueBox의 상위에 Canvas가 없습니다.");
    }

    private void CalculateDialogueBoxSize(string processedText)
    {
        float minWidth = 180f, minHeight = 65f;
        float paddingLeft = 30f, paddingRight = 80f;
        float paddingTop = 60f, paddingBottom = 40f;
        float maxWidth = 650f;

        // TMP로 폭 제한 적용 후 높이 계산
        var preferredSize = dialogueText!.GetPreferredValues(processedText, maxWidth - (paddingLeft + paddingRight), 0f);

        float finalWidth = Mathf.Clamp(preferredSize.x + paddingLeft + paddingRight, minWidth, maxWidth);
        float finalHeight = Mathf.Max(minHeight, preferredSize.y + paddingTop + paddingBottom);

        dialogueBox!.sizeDelta = new Vector2(finalWidth, finalHeight);
        dialogueText.rectTransform.sizeDelta = new Vector2(finalWidth - (paddingLeft + paddingRight), finalHeight - (paddingTop + paddingBottom));
        dialogueText.rectTransform.anchoredPosition = new Vector2(paddingLeft, -paddingTop);
    }

    private void CalculateNameBoxSize(string characterName)
    {
        float nameMinWidth = 60f;
        float namePaddingX = 20f;
        float namePaddingY = 10f;

        float nameTextWidth = nameText!.preferredWidth;
        float nameTextHeight = nameText.preferredHeight;

        float nameBoxWidth = Mathf.Max(nameMinWidth, nameTextWidth + namePaddingX * 2);
        float nameBoxHeight = nameTextHeight + namePaddingY * 2;

        nameBox!.sizeDelta = new Vector2(nameBoxWidth, nameBoxHeight);
    }

    private void PositionDialogueBox()
    {
        Canvas canvas = dialogueBox!.GetComponentInParent<Canvas>()!;
        Camera? cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera;
        var parentRect = dialogueBox.parent as RectTransform;

        // Anchor/Pivot 상단 중앙
        dialogueBox.anchorMin = new Vector2(0.5f, 1f);
        dialogueBox.anchorMax = new Vector2(0.5f, 1f);
        dialogueBox.pivot = new Vector2(0.5f, 1f);

        Vector3 screenPos = mainCamera!.WorldToScreenPoint(npcTransform!.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect!, screenPos, cam, out Vector2 canvasPos);

        // NPC 머리 위에서 아래로 350px
        dialogueBox.anchoredPosition = canvasPos + new Vector2(0f, -350f);
    }

    private void PositionNameBox()
    {
        // NameBox를 DialogueBox 자식으로 두고 좌상단 기준 오프셋 적용
        nameBox!.SetParent(dialogueBox, worldPositionStays: false);
        nameBox.anchorMin = new Vector2(0f, 1f);
        nameBox.anchorMax = new Vector2(0f, 1f);
        nameBox.pivot = new Vector2(0f, 1f);
        nameBox.anchoredPosition = nameBoxOffset;
    }

    private async YarnTask TypeTextWithSkipAsync(string processedText)
    {
        const float typingSpeed = 0.04f;
        dialogueText!.text = "";

        // 타이핑 단계 진입
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

        // 타이핑이 자연 종료된 경우에도 전체 표시
        if (!skipped && dialogueText.text != processedText)
            dialogueText.text = processedText;

        // 다음 클릭 대기 단계로 전환
        phase = Phase.AwaitingNext;

        // 스킵 플래그는 여기서 초기화
        isClickedForSkip = false;
    }

    private async YarnTask WaitOrSkipAsync(float seconds, System.Action onSkip)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (isClickedForSkip)
            {
                // 스킵 소비
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

    public void SetTargetTransform(Transform target)
    {
        npcTransform = target;
    }
}
