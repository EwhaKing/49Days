using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tea
{
    public List<TeaIngredient> ingredients;    // 들어간 재료 리스트

    public int temperature;                    // 물 온도 (섭씨)

    public int timeBrewed;                     // 우려낸 시간 (초)

    public TeaIngredient additionalIngredient; // 추가 재료 (nullable)
}
