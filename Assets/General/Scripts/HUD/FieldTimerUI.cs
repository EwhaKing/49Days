using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FieldTimer : MonoBehaviour
{
    private Image timerImage;       // Radial Fill UI 이미지
    public float duration = 300f;  // 타이머 총 시간 (초 단위, 5분 = 300초)
    [SerializeField] private TMP_Text dateText;
    private bool isRunning = false;

    void Start()
    {
        timerImage = GetComponent<Image>();
        dateText.text = GameManager.Instance.GetDate().ToString();
        timerImage.fillAmount = 0f;
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

        // 채워지는 방식 (0 → 1)
        timerImage.fillAmount = progress;

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
