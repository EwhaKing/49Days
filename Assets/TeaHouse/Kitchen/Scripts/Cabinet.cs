using System;
using System.Collections;
using System.Collections.Generic;

public class Cabinet : SceneSingleton<Cabinet>
{
    public Dictionary<IngredientName, int> ingredientCounts = new Dictionary<IngredientName, int>();
    public Action AfterCabinetInit = () => { };

    void Start()
    {
        foreach (IngredientName ingredientName in Utills.GetValues<IngredientName>())
        {
            ingredientCounts[ingredientName] = 0;
        }

        AfterCabinetInit.Invoke();
    }
}
