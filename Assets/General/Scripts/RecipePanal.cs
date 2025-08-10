using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeftAreaLine : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] private RectTransform recipeTitle;       // 55px 스페이서 위에 위치한 제목
    //[SerializeField] private RectTransform px55; // 55px       // 타일 패널 (별도 스크립트)
    //[SerializeField] private RectTransform px100; // 100px
    [SerializeField] private RectTransform pageControlBox;
    [SerializeField] GameObject recipeTilePrefab;
    [SerializeField] private Transform recipeGrid;

    List<GameObject> tiles = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            var go = Instantiate(recipeTilePrefab, recipeGrid);
            tiles.Add(go);
        }
    }
    /*private void Awake()
    {
        if (leftArea == null) leftArea = GetComponent<RectTransform>();

        // X축 중앙 정렬만 수행 (Y는 건드리지 않음)
        AlignCenterX(recipeTitle);
        AlignCenterX(px55);
        AlignCenterX(recipeGrid);
        AlignCenterX(px100);
        AlignCenterX(pageControlBox);

        // 레이아웃 그룹이 이미 붙어있다면 정렬만 중앙으로
        var vLayout = leftArea.GetComponent<VerticalLayoutGroup>();
        if (vLayout) vLayout.childAlignment = TextAnchor.UpperCenter;

        var hLayout = pageControlBox.GetComponent<HorizontalLayoutGroup>();
        if (hLayout) hLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    private void AlignCenterX(RectTransform target)
    {
        if (!target) return;
        target.anchorMin = new Vector2(0.5f, target.anchorMin.y);
        target.anchorMax = new Vector2(0.5f, target.anchorMax.y);
        target.pivot     = new Vector2(0.5f, target.pivot.y);

        var pos = target.anchoredPosition;
        pos.x = 0f; // 가로 중앙
        target.anchoredPosition = pos;
    }*/

    void Update()
    {

    }
}