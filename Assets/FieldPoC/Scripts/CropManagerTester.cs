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
        if (CropManager.Instance == null) return;

        int currentDay = GameManager.Instance.GetDate();

        // 앞으로만 날짜 진행
        while (currentDay < testDay)
        {
            GameManager.Instance.NextDay(); // 날짜 증가
            currentDay = GameManager.Instance.GetDate();

            // CropManager.Start() 재실행
            CropManager.Instance.Start();
        }

        Debug.Log($"==== Day {currentDay} (Tester, Start() 반복 실행) ====");
    }
#endif
}
