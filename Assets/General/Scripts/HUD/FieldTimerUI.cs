using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FieldTimer : MonoBehaviour
{
    [SerializeField] private RectTransform handTransform; // 시곗바늘 RectTransform
    public float duration = 300f;  // 타이머 총 시간 (초 단위, 5분 = 300초)
    [SerializeField] private TMP_Text dateText;
    private bool isRunning = false;

    void Start()
    {
        dateText.text = GameManager.Instance.GetDate().ToString();
        StartTimer();
    }

    public void StartTimer()
    {
        GameManager.timeElapsedInField = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        GameManager.timeElapsedInField += Time.deltaTime;
        float progress = Mathf.Clamp01(GameManager.timeElapsedInField / duration);

        // 시곗바늘 회전 (시계방향)
        float angle = -360f * progress;  // -360이면 한 바퀴 도는 것
        handTransform.localRotation = Quaternion.Euler(0, 0, angle);

        if (progress >= 1f)
        {
            isRunning = false;
            FinishField();
        }
    }

    public void FinishField()
    {
        Debug.Log("필드 끝! 찻집으로 이동");
        GameManager.timeElapsedInField = 0f;
        GameFlowManager.FinishField();
    }
}
