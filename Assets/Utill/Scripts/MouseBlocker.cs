using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseBlocker : SceneSingleton<MouseBlocker>
{
    void Start()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 마우스 이벤트를 막는 투명한 이미지를 활성화한다. <br/>
    /// 사용 후엔 *반드시* Disable을 불러 해제할 것
    /// </summary>
    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
