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
    /// 마우스 이벤트, 키입력을 막는 투명한 이미지를 활성화한다. <br/>
    /// 사용 후엔 *반드시* Disable을 불러 해제할 것
    /// </summary>
    public void Enable()
    {
        UIManager.Instance.BlockingUIOn(gameObject);
    }

    public void Disable()
    {
        UIManager.Instance.BlockingUIOff(gameObject);
    }
}
