using UnityEngine;

public class DayTester : MonoBehaviour
{
    [SerializeField] private bool nextDay;
    [SerializeField] private bool nextWeek;

    void OnValidate()
    {
        if (nextDay)
        {
            nextDay = false;
            GameManager.Instance.NextDay();
            Debug.Log($"테스트: {GameManager.Instance.GetWeek()}주차 {GameManager.Instance.GetDay()}일차");
        }

        if (nextWeek)
        {
            nextWeek = false;
            for (int i = 0; i < 7; i++)
                GameManager.Instance.NextDay();
            Debug.Log($"테스트: {GameManager.Instance.GetWeek()}주차 {GameManager.Instance.GetDay()}일차");
        }
    }
}
