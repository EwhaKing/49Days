using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 반경 내 오브젝트의 SpriteRenderer 정렬 제어
/// - Ground 태그 제외
/// - z값 비교해서 FrontLayer / BehindLayer 배치
/// - 히스테리시스 적용 (깜빡임 방지)
/// - 모든 컬렉션/배열 재사용 (GC 제로화)
/// </summary>
public class SpriteLayerController : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float checkRadius = 10f;
    [SerializeField] private string frontLayer = "FrontLayer";
    [SerializeField] private string behindLayer = "BehindLayer";
    [SerializeField] private float hysteresis = 0.05f;
    [SerializeField] private int maxColliders = 100; // NonAlloc 버퍼 크기

    // 상태 기억 (Front = true, Behind = false)
    private Dictionary<SpriteRenderer, bool> rendererState = new();

    // 매 프레임 재사용되는 컬렉션들
    private HashSet<SpriteRenderer> currentFrame = new();
    private List<SpriteRenderer> toRemoveBuffer = new();
    private Collider[] hitBuffer;

    void Awake()
    {
        hitBuffer = new Collider[maxColliders];
    }

    void Update()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, hitBuffer);

        currentFrame.Clear();

        float playerZ = transform.position.z;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hitBuffer[i];
            if (hit == null || hit.CompareTag("Ground")) continue;

            SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            currentFrame.Add(sr);

            float targetZ = hit.transform.position.z;

            if (!rendererState.TryGetValue(sr, out bool isFront))
            {
                // 초기 상태 등록
                isFront = targetZ < playerZ;
                rendererState[sr] = isFront;
                sr.sortingLayerName = isFront ? frontLayer : behindLayer;
                continue;
            }

            // 히스테리시스 적용 전환
            if (isFront && targetZ > playerZ + hysteresis)
            {
                rendererState[sr] = false;
                sr.sortingLayerName = behindLayer;
            }
            else if (!isFront && targetZ < playerZ - hysteresis)
            {
                rendererState[sr] = true;
                sr.sortingLayerName = frontLayer;
            }
        }

        Cleanup();
    }

    /// <summary>
    /// 이번 프레임에 잡히지 않은 SpriteRenderer 제거
    /// </summary>
    private void Cleanup()
    {
        toRemoveBuffer.Clear();

        foreach (var kvp in rendererState)
        {
            if (kvp.Key == null || !currentFrame.Contains(kvp.Key))
                toRemoveBuffer.Add(kvp.Key);
        }

        foreach (var sr in toRemoveBuffer)
        {
            rendererState.Remove(sr);
        }
    }
}
