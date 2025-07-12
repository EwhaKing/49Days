using UnityEngine;

/// <summary>
/// 해당 스크립트를 UI요소에 붙이고 대상(target)을 설정하면 <br/>
/// 대상의 위치에 해당 UI요소가 위치하게 됩니다. <br/> 
/// 한 번 위치를 정한 이후에는 고정됩니다. <br/>
/// 고정된 오브젝트에 대한 UI 요소를 만들 때 유용합니다. <br/>
/// offset을 통해 대상의 위치에 추가적인 오프셋을 줄 수 있습니다.
/// </summary>
public class AttachOnceUI : MonoBehaviour
{
    [SerializeField] GameObject target;

    [SerializeField] Vector3 offset;
    void Start()
    {
        Debug.Assert(target != null, $"{this.name}: Target이 설정되지 않았습니다!");

        Vector3 targetPosition = Camera.main.WorldToScreenPoint(target.transform.position + offset);
        transform.position = targetPosition;
    }
}
