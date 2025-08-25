using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RecipeDescriptionManager : SceneSingleton<RecipeDescriptionManager>
{
    public static event Action OnRecipesLoaded;
    
    private HashSet<string> unlockedRecipeNames = new HashSet<string>();
    private List<RecipeDescription> recipeDescriptions;
    private Dictionary<string, RecipeDescription> recipeDescriptionDict;
    
    public bool IsLoaded { get; private set; }

    public int Count => recipeDescriptions?.Count ?? 0;
    public List<RecipeDescription> GetAllRecipeDescriptions() => recipeDescriptions;

    void Start()
    {
        LoadRecipeDescriptions();
    }

    async void LoadRecipeDescriptions()
    {
        AsyncOperationHandle<IList<RecipeDescription>> handle =
            Addressables.LoadAssetsAsync<RecipeDescription>("recipedescription", null);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            recipeDescriptions = new List<RecipeDescription>(handle.Result);
            recipeDescriptionDict = new Dictionary<string, RecipeDescription>();
            foreach (var recipe in recipeDescriptions)
            {
                if (!recipeDescriptionDict.ContainsKey(recipe.recipeName))
                {
                    recipeDescriptionDict.Add(recipe.recipeName, recipe);
                }
            }
            
            IsLoaded = true;
            OnRecipesLoaded?.Invoke();
            Debug.Log("[매니저] 레시피 로딩 성공 및 신호 발송 완료!");
        }
        else
        {
            Debug.LogError("recipedescription 그룹 로드 실패");
        }
    }

    public RecipeDescription GetRecipeDescription(string recipeName)
    {
        if (!IsLoaded || recipeDescriptionDict == null) return null;
        recipeDescriptionDict.TryGetValue(recipeName, out var recipe);
        return recipe;
    }

    public bool IsRecipeUnlocked(string recipeName) => unlockedRecipeNames.Contains(recipeName);
    
    void OnEnable() {
        SaveLoadManager.Instance.onSave += SaveUnlockedRecipes;
        SaveLoadManager.Instance.onLoad += LoadUnlockedRecipes;
    }

    void OnDisable() {
        SaveLoadManager.Instance.onSave -= SaveUnlockedRecipes;
        SaveLoadManager.Instance.onLoad -= LoadUnlockedRecipes;
    }

    private void SaveUnlockedRecipes() => SaveLoadManager.Instance.Save(unlockedRecipeNames);
    private void LoadUnlockedRecipes()
    {
        unlockedRecipeNames = SaveLoadManager.Instance.Load<HashSet<string>>();
        if (unlockedRecipeNames == null)
        {
            unlockedRecipeNames = new HashSet<string>();
        }
    }
}