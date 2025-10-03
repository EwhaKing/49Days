using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleStaticAgent : MonoBehaviour
{
    [System.Serializable]
    public class Schedule
    {
        public float time;
        public Transform target;  // 가야 할 목적지
    }

    [SerializeField] private Schedule[] schedules; // NPC의 이동 스케줄
    private NavMeshAgent agent;
    private int currentIndex = -1;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{name}에 NavMeshAgent가 없습니다.");
        }

        agent.updateRotation = false; // 회전 자동 업데이트 끄기
    }

    void OnEnable()
    {
        // 대화 이벤트 구독
        if (FieldYarnManager.Instance != null)
        {
            FieldYarnManager.Instance.onDialogueStart += PauseMovement;
            FieldYarnManager.Instance.onDialogueEnd += ResumeMovement;
        }
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        if (FieldYarnManager.Instance != null)
        {
            FieldYarnManager.Instance.onDialogueStart -= PauseMovement;
            FieldYarnManager.Instance.onDialogueEnd -= ResumeMovement;
        }
    }

    private void PauseMovement(SimpleStaticAgent target)
    {
        if (target == this)
        {
            agent.isStopped = true;
            Debug.Log($"{name} 이동 멈춤 (대화 시작)");
        }
    }

    private void ResumeMovement(SimpleStaticAgent target)
    {
        if (target == this)
        {
            agent.isStopped = false;
            Debug.Log($"{name} 이동 재개 (대화 종료)");
        }
    }


    void Update()
    {
        float elapsed = FieldDataManager.Instance.timeElapsedInField;

        // 아직 안 간 스케줄 중 다음 순서를 찾음
        for (int i = currentIndex + 1; i < schedules.Length; i++)
        {
            if (elapsed >= schedules[i].time)
            {
                MoveTo(i);
            }
        }
    }

    private void MoveTo(int index)
    {
        if (index < 0 || index >= schedules.Length) return;

        var schedule = schedules[index];
        if (schedule.target != null)
        {
            agent.SetDestination(schedule.target.position);
            Debug.Log($"{name} → {schedule.target.name} 로 이동 (time={schedule.time})");
        }
        currentIndex = index;
    }

}
