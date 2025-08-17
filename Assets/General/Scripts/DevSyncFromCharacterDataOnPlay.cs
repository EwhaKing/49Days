using System.Collections;
using UnityEngine;

public class DevSyncFromCharacterDataOnPlay : MonoBehaviour
{
    public bool useCharacterData = true;   // CharacterData의 hasMet/affinity를 그대로 씀
    public bool overrideAllMet = false;    // 전부 만난 상태로 강제
    public bool randomizeAffinity = false; // 5단위 랜덤(0~100)

    IEnumerator Start()
    {
        // 로드가 db에 반영된 다음 실행되도록 한 프레임 대기
        yield return null;

        var cm = CharacterManager.Instance;
        if (cm == null) { Debug.LogWarning("[DevSync] CharacterManager.Instance 없음"); yield break; }

        var rand = new System.Random(1234);

        for (int i = 0; i < cm.Count; i++)
        {
            var cd = cm.GetStatic(i);  // CharacterData (정적)
            bool met;
            int aff;

            if (overrideAllMet)
            {
                met = true;
                aff = randomizeAffinity ? (rand.Next(0, 21) * 5) : cd.affinity;
            }
            else if (useCharacterData)
            {
                // SO에 설정해둔 값으로 동기화
                met = cd.hasMet;
                aff = cd.affinity;
            }
            else
            {
                // 기본(모두 미만남)
                met = false;
                aff = 0;
            }

            cm.Meet(i, met);
            cm.SetAffinity(i, Mathf.Clamp(aff, 0, 100));
        }

        // 왼쪽 그리드/오른쪽 패널 갱신
        var panels = FindObjectsOfType<AffinityPanel>(true);
        foreach (var p in panels) p.RefreshPage();

        Debug.Log("[DevSync] 완료");
    }
}
