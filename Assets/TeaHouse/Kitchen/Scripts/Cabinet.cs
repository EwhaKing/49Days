using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cabinet : SceneSingleton<Cabinet>
{
    public Dictionary<IngredientName, int> ingredientCounts = new Dictionary<IngredientName, int>();
    public static Action AfterCabinetInit = () => {};

    void Start()
    {
        foreach (IngredientName ingredientName in Utills.GetValues<IngredientName>())
        {
            ingredientCounts[ingredientName] = 2;
        }

        Debug.Log("Cabinet 초기화");
        AfterCabinetInit.Invoke();
    }
}
