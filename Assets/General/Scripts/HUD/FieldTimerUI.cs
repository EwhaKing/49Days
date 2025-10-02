using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FieldTimer : MonoBehaviour
{
    [SerializeField] private RectTransform handTransform; // 시곗바늘 RectTransform
    [SerializeField] private TMP_Text dateText;
    private bool isRunning = false;

    void Start()
    {
        dateText.text = GameManager.Instance.GetDate().ToString();
        StartTimer();
    }

    public void StartTimer()
    {
        FieldDataManager.Instance.timeElapsedInField = 0f;
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        FieldDataManager.Instance.timeElapsedInField += Time.deltaTime;
        float progress = Mathf.Clamp01(FieldDataManager.Instance.timeElapsedInField / FieldDataManager.duration);

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
        GameFlowManager.FinishField();
    }
}
