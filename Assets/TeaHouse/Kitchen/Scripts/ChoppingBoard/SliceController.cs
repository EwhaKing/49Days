using UnityEngine;
using System.Collections;
 
[System.Serializable]
public class SlicePath
{
    public RectTransform[] checkpoints; // 경로를 이루는 포인트
}

public class SliceController : MonoBehaviour
{
    [SerializeField] RectTransform[] pieces; // 잘린 조각 순서
    [SerializeField] GameObject[] sliceLines; // 슬라이스 라인
    [HideInInspector] public SlicePath[] slicePaths;  // 슬라이스 경로
    [SerializeField] float moveDistance = -90f; // px 단위 이동 거리
    [SerializeField] float moveSpeed = 200f;   // px/s
    [SerializeField] float rotateAngle = 0f;  // 회전 각도

    private int currentIndex = 0;

    public void SliceNext()
    {
        RectTransform piece = pieces[currentIndex];
        Vector2 targetPos = piece.anchoredPosition + new Vector2(moveDistance, 0);
        float targetRot = rotateAngle;

        StartCoroutine(MoveAndRotate(piece, targetPos, targetRot));
        sliceLines[currentIndex].SetActive(false); // 현재 슬라이스 라인 비활성화

        currentIndex++;
        if (currentIndex >= sliceLines.Length) return;
        sliceLines[currentIndex].SetActive(true); // 다음 슬라이스 라인 활성화
        moveDistance += 30f;
    }

    private IEnumerator MoveAndRotate(RectTransform piece, Vector2 targetPos, float targetRot)
    {
        Vector2 startPos = piece.anchoredPosition;
        float startRot = piece.localEulerAngles.z;

        float t = 0;
        float dist = Vector2.Distance(startPos, targetPos);
        float duration = dist / moveSpeed;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            piece.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            piece.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(startRot, targetRot, progress));

            yield return null;
        }
    }

    public void Init()
    {
        int lineIndex = 0;
        slicePaths = new SlicePath[sliceLines.Length];

        foreach (var line in sliceLines) // 슬라이스 라인에서 슬라이스 경로 추출
        {
            int childs = line.transform.childCount;
            slicePaths[lineIndex] = new SlicePath();
            slicePaths[lineIndex].checkpoints = new RectTransform[childs];
            for (int i = 0; i < childs; i++)
            {
                slicePaths[lineIndex].checkpoints[i] = line.transform.GetChild(i).GetComponent<RectTransform>();
            }
            line.SetActive(false); // 슬라이스 라인 비활성화
            lineIndex++;
        }

        sliceLines[0].SetActive(true); // 첫 번째 슬라이스 라인 활성화
        MultiSlicePathChecker.Instance.StartSlice(this); // 슬라이스 시작
    }
}
