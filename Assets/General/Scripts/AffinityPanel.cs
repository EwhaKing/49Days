using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class AffinityPanel : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] Transform characterGrid;
    [SerializeField] GameObject characterSlotPrefab;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;

    [Header("Right Side")]
    [SerializeField] Image profileImage;
    [SerializeField] TextMeshProUGUI profileText;
    [SerializeField] Transform heartContainer;
    [SerializeField] TextMeshProUGUI likesText;
    [SerializeField] TextMeshProUGUI dislikesText;

    List<GameObject> slots = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        //캐릭터 슬롯 생성(프리팹으로 만들어진 characterSlotPrefab)
        for (int i = 0; i < 9; i++)
        {
            GameObject go = Instantiate(characterSlotPrefab, characterGrid);
            slots.Add(go);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
