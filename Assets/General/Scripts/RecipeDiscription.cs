using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipeDiscription", menuName = "RecipeDiscription")]
public class RecipeDiscription : ScriptableObject
{
    public string recipeName; // 레시피 이름

    public Sprite teaImage;  // 차 이미지 (UI에 표시)
    
    public Sprite simpleRecipeImage;  // 차 간단 레시피 이미지 (UI에 표시)

    public string description;  // 차 설명 (UI에 표시)
}