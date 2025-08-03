using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabToggleAnimator : MonoBehaviour
{
    private Toggle toggle;          // 책갈피 = 토글임.
    private Animator animator;      // Background에 붙여둔 animator

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        animator = GetComponentInChildren<Animator>();
        // 토글 On/Off 시 animator의 파라미터를 업데이트하기 위함.
        toggle.onValueChanged.AddListener(OnToggleChanged); 
    }

    private void OnEnable()
    {
        animator.SetBool("IsOn", toggle.isOn);
    }

    private void OnDestroy()
    {
        // 사라질 때 이벤트 해제해주고 쿨빠.
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        animator.SetBool("IsOn", isOn);    
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
