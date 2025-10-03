using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cabinet
{
    public Dictionary<IngredientName, int> ingredients = new Dictionary<IngredientName, int>();
}

public class CabinetManager : SceneSingleton<CabinetManager>
{
    Cabinet cabinet = new Cabinet();
    public Action<IngredientName> onIngredientUnlocked;

    public bool isUnlocked(IngredientName ingredient)
    {
        return cabinet.ingredients.ContainsKey(ingredient);
    }

    public int GetIngredientCount(IngredientName ingredient)
    {
        if (isUnlocked(ingredient))
        {
            return cabinet.ingredients[ingredient];
        }
        else
        {
            Debug.LogError($"재료 {ingredient}가 찬장에 없는데 가져오려고 함");
            return 0;
        }
    }

    public void AddIngredient(IngredientName ingredient, int count)
    {
        if (isUnlocked(ingredient))
        {
            cabinet.ingredients[ingredient] += count;
        }
        else
        {
            cabinet.ingredients[ingredient] = count;
            onIngredientUnlocked?.Invoke(ingredient);
        }
    }

    public void SubtractIngredient(IngredientName ingredient, int count)
    {
        if (isUnlocked(ingredient))
        {
            cabinet.ingredients[ingredient] = Mathf.Max(0, cabinet.ingredients[ingredient] - count);  
        }
        else
        {
            Debug.LogError($"재료 {ingredient}가 없는데 찬장에서 빼려고 함");
        }
    }

    void Start()
    {
        // 유니콘 제출 빌드용
        AddIngredient(IngredientName.TeaLeaf, 15);
        AddIngredient(IngredientName.Rose, 15);
        AddIngredient(IngredientName.SolomonsSeal, 15);

        Debug.Log("Cabinet 초기화");
    }

    void OnEnable() {
        SaveLoadManager.Instance.onSave += () => SaveLoadManager.Instance.Save<Cabinet>(cabinet);
        SaveLoadManager.Instance.onLoad += () => {cabinet = SaveLoadManager.Instance.Load<Cabinet>();};
    }

    void OnDisable() {
        SaveLoadManager.Instance.onSave -= () => SaveLoadManager.Instance.Save<Cabinet>(cabinet);
        SaveLoadManager.Instance.onLoad -= () => {cabinet = SaveLoadManager.Instance.Load<Cabinet>();};
    }
}
