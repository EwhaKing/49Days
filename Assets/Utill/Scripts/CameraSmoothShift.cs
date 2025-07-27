using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraSmoothShift : MonoBehaviour
{
    [SerializeField]
    public CinemachineVirtualCamera virtualCam;
    [SerializeField]
    public float offsetAmount = 25.6f;
    public float transitionDuration = 0.5f;

    private bool isShifted = false;
    private Vector3 defaultOffset;
    private Coroutine currentTransition;

    void Start()
    {
        // 기본 Offset 저장
        defaultOffset = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 이미 진행 중이면 중단
            if (currentTransition != null)
                StopCoroutine(currentTransition);

            Vector3 targetOffset = isShifted
                ? defaultOffset
                : defaultOffset + new Vector3(-offsetAmount, 0, 0);

            currentTransition = StartCoroutine(SmoothTransition(targetOffset));
            isShifted = !isShifted;
        }
    }

    IEnumerator SmoothTransition(Vector3 targetOffset)
    {
        var transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        Vector3 startOffset = transposer.m_TrackedObjectOffset;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.unscaledDeltaTime; // 타임스케일 무시
            float t = Mathf.Clamp01(time / transitionDuration);
            transposer.m_TrackedObjectOffset = Vector3.Lerp(startOffset, targetOffset, t);
            yield return null;
        }

        transposer.m_TrackedObjectOffset = targetOffset;
    }
}
