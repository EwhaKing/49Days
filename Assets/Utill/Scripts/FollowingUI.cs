using UnityEngine;


/// <summary>
/// 해당 스크립트를 UI요소에 붙이고 따라다닐 대상(target)을 설정하면 <br/>
/// 대상의 위치를 해당 UI요소가 따라다니게 됩니다. <br/> 
/// offset을 통해 대상의 위치에 추가적인 오프셋을 줄 수 있습니다.
/// </summary>
public class FollowingUI : MonoBehaviour
{
    [SerializeField] GameObject target;

    [SerializeField] Vector3 offset;
    void LateUpdate()
    {
        Debug.Assert(target != null, $"{this.name}: Target이 설정되지 않았습니다!");

        Vector3 targetPosition = Camera.main.WorldToScreenPoint(target.transform.position + offset);
        if(transform.position != targetPosition)
        {
            transform.position = targetPosition;
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }
}
