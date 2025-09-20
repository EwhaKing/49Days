using UnityEngine;

public class CropManagerTester : MonoBehaviour
{
    [Header("테스트용 날짜 (Inspector에서 직접 수정)")]
    [SerializeField] private int testDay = 1;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (GameManager.Instance == null) return;

        int currentDay = GameManager.Instance.GetDate();

        // 앞으로만 날짜 진행
        while (currentDay < testDay)
        {
            GameManager.Instance.NextDay(); // 이벤트 발행 → CropManager.OnDayChanged() 호출됨
            currentDay = GameManager.Instance.GetDate();
        }

        Debug.Log($"==== Day {currentDay} (Tester) ====");
    }
#endif
}
