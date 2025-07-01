using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tea : MonoBehaviour
{
    List<TeaIngredient> ingredients;  // 들어간 재료 리스트

    int temperature;                  // 물 온도 (섭씨)

    int timeBrewed;                   // 우려낸 시간 (초)

    bool isWaterFirst;  // 다병에 물을 넣으면 true, 재료를 넣으면 false (단, 추가재료 제외)

    TeaIngredient additionalIngredient; // 추가 재료 (nullable)
}
