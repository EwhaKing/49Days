using System.Collections;
using UnityEngine;

//해당 코드도 비동기에 맞춰 수정.
public class DevSyncFromCharacterDataOnPlay : MonoBehaviour
{
    public bool useCharacterData = true;
    public bool overrideAllMet = false;
    public bool randomizeAffinity = false;

    IEnumerator Start()
    {
        var cm = CharacterManager.Instance;
        if (cm == null) yield break;

        // CharacterData 로딩 끝날 때까지 대기
        while (cm.Count == 0)
            yield return null;

        var rand = new System.Random(1234);

        for (int i = 0; i < cm.Count; i++)
        {
            var cd = cm.GetStatic(i);
            bool met;
            int aff;

            if (overrideAllMet)
            {
                met = true;
                aff = randomizeAffinity ? (rand.Next(0, 21) * 5) : cd.affinity;
            }
            else if (useCharacterData)
            {
                met = cd.hasMet;
                aff = cd.affinity;
            }
            else
            {
                met = false;
                aff = 0;
            }

            cm.Meet(cd.characterName, met);
            cm.SetAffinity(cd.characterName, Mathf.Clamp(aff, 0, 100));
        }

        foreach (var p in FindObjectsOfType<AffinityPanel>(true))
            p.RefreshPage();

        Debug.Log("[DevSync] 완료");
    }
}
