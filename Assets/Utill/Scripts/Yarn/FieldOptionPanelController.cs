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

    // 버튼 스타일 상수
    private const float ButtonWidth = 1000f;
    private const float ButtonHeight = 100f;
    private const int FontSize = 42;

    private TaskCompletionSource<int>? selectionSource;
    private DialogueOption[]? currentOptions;


    /// <summary>
    /// 일반적인 옵션 처리할 때 불려옴
    /// </summary>
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
            var btnObj = Instantiate(fieldOptionButtonPrefab!, panel);
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
        selectionSource?.TrySetResult(index);
    }

    /// <summary>
    /// FieldDialoguePresenter가 E키로 최초 호출될 때 불려옴
    /// Yarn의 기본 패널이 아닌 커스텀 UI 띄움
    /// </summary>
    public event Action<int>? OnEntryOptionSelected;
    public void ShowEntryOptions(DialogueOption[] options, bool[]? clickedStates = null)
    {
        foreach (Transform child in panel!)
            Destroy(child.gameObject);

        if (layoutGroup != null)
        {
            layoutGroup.spacing = 0f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        }

        if (panel != null)
        {
            panel.anchorMin = new Vector2(0.5f, 1f);
            panel.anchorMax = new Vector2(0.5f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(800f, -600f);
        }

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

            // 클릭 상태에 따라 버튼 처리
            if (clickedStates != null && clickedStates.Length == 3 && clickedStates[idx])
            {
                if (idx == 2)
                {
                    btn.gameObject.SetActive(false);
                }
                else
                {
                    btn.interactable = false;
                    var colors = btn.colors;
                    colors.normalColor = new Color(1f, 1f, 1f, 0.6f);
                    btn.colors = colors;
                }
            }
            else
            {
                btn.onClick.AddListener(() => {
                    panel!.gameObject.SetActive(false);
                    OnEntryOptionSelected?.Invoke(idx);
                });
            }
        }

        panel!.gameObject.SetActive(true);
    }

    /// <summary>
    /// Entry에서 대화 종료했을 때
    /// FieldDialoguePresenter가 옵션 종료를 위해 호출하는 메서드
    /// 커스텀 옵션이기 때문에 리스너 수동 제거 필요
    /// </summary>
    public void ForceCloseOptions()
    {
        panel?.gameObject.SetActive(false);

        if (panel != null)
        {
            foreach (Transform child in panel)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        selectionSource?.TrySetCanceled();
        selectionSource = null;

        OnEntryOptionSelected = null;
    }
}
