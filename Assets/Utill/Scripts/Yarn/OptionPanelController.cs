#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class OptionPanelController : MonoBehaviour
{
    public RectTransform? panel;                // 옵션 버튼 부모
    public GameObject? optionButtonPrefab;      // 버튼 프리팹
    public VerticalLayoutGroup? layoutGroup;    // spacing 제어용

    // 버튼 스타일 상수
    private const float ButtonWidth = 635f;
    private const float ButtonHeight = 90f;
    private const int FontSize = 35;

    private TaskCompletionSource<int>? selectionSource;
    private DialogueOption[]? currentOptions; // 현재 옵션 저장용 필드 추가

    public async Task<int> ShowOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        // 기존 버튼 제거
        foreach (Transform child in panel!)
            Destroy(child.gameObject);

        // 옵션 배열 저장
        currentOptions = options;

        // spacing 조절
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

        // 패널 위치 조정 (상단 기준)
        if (panel != null)
        {
            panel.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙 anchor
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

        // 버튼 생성 및 스타일 적용
        for (int i = 0; i < options.Length; i++)
        {
            var btnObj = Instantiate(optionButtonPrefab!, panel);
            var btnRect = btnObj.GetComponent<RectTransform>();
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // 버튼 크기 고정
            if (btnRect != null)
                btnRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            // 텍스트 스타일 적용
            if (txt != null)
            {
                txt.text = options[i].Line.TextWithoutCharacterName.Text;
                txt.fontSize = FontSize;
                txt.alignment = TextAlignmentOptions.Center;
                txt.enableWordWrapping = true;
            }

            int idx = i;

            // 각 버튼에 클릭 이벤트 등록
            btn.onClick.AddListener(() => OnOptionSelected(idx));
        }

        // 옵션 패널 활성화
        panel!.gameObject.SetActive(true);

        // 선택된 옵션을 전달하는 객체 생성
        selectionSource = new TaskCompletionSource<int>();

        // 취소 토큰 등록 및 대기
        using (cancellationToken.Register(() => selectionSource.TrySetCanceled()))
        {
            int result = await selectionSource.Task;
            panel.gameObject.SetActive(false);
            return result;
        }
    }

    // 옵션 선택 시 호출 (선택된 인덱스 전달)
    private void OnOptionSelected(int index)
    {
        // 로그에 저장
        if (currentOptions != null && index >= 0 && index < currentOptions.Length)
        {
            var option = currentOptions[index];
            DialogueLogManager.Instance.AddLog("", option.Line.TextWithoutCharacterName.Text);
        }

        selectionSource?.TrySetResult(index);
    }
}
