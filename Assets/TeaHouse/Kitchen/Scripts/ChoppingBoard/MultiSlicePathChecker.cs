using UnityEngine;
using UnityEngine.EventSystems;

public class MultiSlicePathChecker : SceneSingleton<MultiSlicePathChecker>, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject finishButton;
    [SerializeField] float radius = 100f;            // 판정 거리(px)

    private SlicePath[] slicePaths;
    private SliceController sliceController;

    private int currentPathIndex = 0;     // 현재 진행 중인 궤적 인덱스
    private int currentCheckpointIndex = 0; // 현재 궤적의 진행 포인트
    private bool isDragging = false;
    Canvas canvasComponent;

    public void StartSlice(SliceController controller)
    {
        canvas.SetActive(true);
        sliceController = controller;
        slicePaths = sliceController.slicePaths;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag started");
        // 드래그 시작 시 현재 궤적의 첫 포인트부터
        currentCheckpointIndex = 0;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (currentPathIndex >= slicePaths.Length) return; // 모든 궤적 완료

        var currentPath = slicePaths[currentPathIndex];
        if (currentCheckpointIndex >= currentPath.checkpoints.Length) return;

        // 마우스 좌표 → 로컬 UI 좌표 변환
        Vector3 localPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvasComponent.worldCamera,
            out localPos
        );

        Vector2 checkpointPos = currentPath.checkpoints[currentCheckpointIndex].position;

        // 포인트에 도달했는지 판정
        if (Vector2.Distance(localPos, checkpointPos) <= radius)
        {
            Debug.Log(Vector2.Distance(localPos, checkpointPos));
            currentCheckpointIndex++;

            // 궤적의 모든 포인트를 통과했으면 슬라이스
            if (currentCheckpointIndex >= currentPath.checkpoints.Length)
            {
                sliceController.SliceNext(); // 조각 이동/회전
                currentPathIndex++;          // 다음 궤적으로 이동
                if (currentPathIndex == slicePaths.Length)
                {
                    currentPathIndex = 0;
                    finishButton.SetActive(true); // 모든 슬라이스 완료 시 버튼 활성화
                }
                isDragging = false;          // 드래그 종료
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void Start() {
        canvasComponent = canvas.GetComponent<Canvas>();
        canvas.SetActive(false);
        finishButton.SetActive(false);
    }
}
