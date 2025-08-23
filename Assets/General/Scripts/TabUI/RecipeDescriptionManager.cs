using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RecipeDescriptionManager : SceneSingleton<RecipeDescriptionManager>
{
    private HashSet<string> unlockedRecipeNames = new HashSet<string>();
    private List<RecipeDescription> recipeDescriptions;
    private Dictionary<string, RecipeDescription> recipeDescriptionDict;

    public int Count
    {
        get { return recipeDescriptions?.Count ?? 0; }
    }
    void Start()
    {
        LoadRecipeDescriptions();
    }
    async void LoadRecipeDescriptions()
    {
        AsyncOperationHandle<IList<RecipeDescription>> handle =
            Addressables.LoadAssetsAsync<RecipeDescription>("recipedescription", null); // label 기반 로드

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            recipeDescriptions = new List<RecipeDescription>(handle.Result);
        }
        else
        {
            Debug.LogError("recipedescription 그룹 로드 실패");
        }

        // 레시피 설명을 딕셔너리에 저장
        recipeDescriptionDict = new Dictionary<string, RecipeDescription>();
        foreach (var recipe in recipeDescriptions)
        {
            if (!recipeDescriptionDict.ContainsKey(recipe.recipeName))
            {
                recipeDescriptionDict.Add(recipe.recipeName, recipe);
            }
            else
            {
                Debug.LogWarning($"레시피 이름 중복: {recipe.recipeName}");
            }
        }
    }

    public RecipeDescription GetRecipeDescription(string recipeName)
    {
        if (recipeDescriptions == null || recipeDescriptions.Count == 0)
        {
            Debug.LogWarning("레시피 설명이 로드되지 않았습니다.");
            return null;
        }

        return recipeDescriptionDict[recipeName];
    }

    public bool IsRecipeUnlocked(string recipeName)
    {
        return unlockedRecipeNames.Contains(recipeName);
    }

    void OnEnable()
    {
        SaveLoadManager.Instance.onSave += () => SaveLoadManager.Instance.Save<HashSet<string>>(unlockedRecipeNames);
        SaveLoadManager.Instance.onLoad += () => { unlockedRecipeNames = SaveLoadManager.Instance.Load<HashSet<string>>(); };
    }

    void OnDisable()
    {
        SaveLoadManager.Instance.onSave -= () => SaveLoadManager.Instance.Save<HashSet<string>>(unlockedRecipeNames);
        SaveLoadManager.Instance.onLoad -= () => { unlockedRecipeNames = SaveLoadManager.Instance.Load<HashSet<string>>(); };
    }
    public List<RecipeDescription> GetAllRecipeDescriptions()
    {
        return recipeDescriptions;
    }

    public int GetRecipeCount()
    {
        return recipeDescriptions?.Count ?? 0;
    }
}
