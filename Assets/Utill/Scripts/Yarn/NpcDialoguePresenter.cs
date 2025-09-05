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
    public Transform? npcTransform;                // NPC ���� ��ǥ (���� �Ҵ�)
    public Vector2 nameBoxOffset = new(-10f, 20f); // NameBox ������ (�»�� ����)

    private bool isClickedForSkip = false;
    private bool isClickedForNext = false;
    private Camera? mainCamera;

    private enum Phase { Typing, AwaitingNext }
    private Phase phase = Phase.AwaitingNext;
    public FollowingUI? followingUI;

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

        string characterName = line.CharacterName ?? "";
        string processedText = !string.IsNullOrEmpty(line.TextWithoutCharacterName.Text)
            ? line.TextWithoutCharacterName.Text
            : line.Text.Text;

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
        if (npcTransform == null) throw new System.InvalidOperationException("npcTransform�� �Ҵ���� �ʾҽ��ϴ�.");
        if (dialogueInputHandler == null) throw new System.InvalidOperationException("dialogueInputHandler가 할당되지 않았습니다.");

        mainCamera = Camera.main ?? throw new System.InvalidOperationException("Main Camera�� ã�� �� �����ϴ�.");
        if (dialogueBox.GetComponentInParent<Canvas>() == null) throw new System.InvalidOperationException("dialogueBox�� ������ Canvas�� �����ϴ�.");
    }

    private void CalculateDialogueBoxSize(string processedText)
    {
        float minWidth = 180f, minHeight = 65f;
        float paddingLeft = 30f, paddingRight = 80f;
        float paddingTop = 60f, paddingBottom = 40f;
        float maxWidth = 650f;

        // TMP�� �� ���� ���� �� ���� ���
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

        // Anchor/Pivot ��� �߾�
        dialogueBox.anchorMin = new Vector2(0.5f, 1f);
        dialogueBox.anchorMax = new Vector2(0.5f, 1f);
        dialogueBox.pivot = new Vector2(0.5f, 1f);

        //Vector3 screenPos = mainCamera!.WorldToScreenPoint(npcTransform!.position);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect!, screenPos, cam, out Vector2 canvasPos);

        // NPC �Ӹ� ������ �Ʒ��� 200px
        //dialogueBox.anchoredPosition = canvasPos + new Vector2(0f, -200f);
    }

    private void PositionNameBox()
    {
        // NameBox�� DialogueBox �ڽ����� �ΰ� �»�� ���� ������ ����
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

    public override YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
        => YarnTask.FromResult<DialogueOption?>(null);

    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;

    public void SetTargetTransform(Transform target)
    {
        npcTransform = target;
        if (followingUI != null)
            followingUI.SetTarget(target!.gameObject);
    }
}
