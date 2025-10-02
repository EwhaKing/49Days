using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleStaticAgent : MonoBehaviour
{
    //[SerializeField] Transform target;

    //TODO : 시간에 따라 이동하는 걸로 바꿔야함. / 임시로 날짜에 따라 이동하고 있음.
    [System.Serializable]
    public class Schedule
    {
        public int week;          // 몇 주차
        public int day;           // 몇 요일
        public Transform target;  // 가야 할 목적지
    }

    [SerializeField] private Schedule[] schedules; // NPC의 이동 스케줄
    private NavMeshAgent agent;

    //TODO : 시간에 따른 이동으로 바꿔야함..
    private void OnDayChanged()
    {
        UpdateDestination();
    }

    void OnEnable()
    {
        GameManager.Instance.onDayChanged += OnDayChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.onDayChanged -= OnDayChanged;
    }


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{name}에 NavMeshAgent가 없습니다.");
        }
    }

    void Start()
    {
        //agent.SetDestination(target.position);
    }

    private void UpdateDestination()
    {
        int currentWeek = GameManager.Instance.GetWeek();
        int currentDay = GameManager.Instance.GetDay();

        foreach (var schedule in schedules)
        {
            if (schedule.week == currentWeek && schedule.day == currentDay)
            {
                if (schedule.target != null)
                {
                    agent.SetDestination(schedule.target.position);
                    Debug.Log($"{name} → {schedule.target.name} 로 이동");
                }
                return;
            }
        }
    }

}
