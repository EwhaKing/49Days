using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TeaMaker : SceneSingleton<TeaMaker>
{
    private static List<TeaRecipe> teaRecipes;

    void Start()
    {
        LoadTeaRecipes();
    }

    async void LoadTeaRecipes()
    {
        AsyncOperationHandle<IList<TeaRecipe>> handle =
            Addressables.LoadAssetsAsync<TeaRecipe>("tearecipe", null); // label 기반 로드

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            teaRecipes = new List<TeaRecipe>(handle.Result);
        }
        else
        {
            Debug.LogError("tearecipe 그룹 로드 실패");
        }
    }

    /// <summary>
    /// Tea를 기반으로 레시피를 판단하여 MakedTea 생성 (채점은 하지않음)
    /// </summary>
    /// <param name="tea"></param>
    /// <returns></returns>
    public static MakedTea MakeTea(Tea tea)
    {
        foreach (TeaIngredient ingredient in tea.ingredients)
        {
            // 찻잎일 경우 산화정도에 따라 이름 바꿈
            if (ingredient.ingredientName == IngredientName.TeaLeaf)
                ChangeTeaLeafName(ingredient);
            
            // 재료가 옳게 가공되었는지 평가
            if (!IsIngredientProcessRight(ingredient))
            {
                Debug.Log($"{ingredient.ingredientName}의 가공이 잘못되었습니다: 알 수 없는 차 생성");
                return new MakedTea { teaName = TeaName.Unknown };
            }
        }

        // 레시피 찾기 (기준: 재료들이 정확히 일치하는지)
        TeaRecipe recipe = teaRecipes.Find(r =>
            r.ingredients.EqualIgnoreOrder(tea.ingredients.ConvertAll(i => i.ingredientName)));

        if (!recipe)
        {
            Debug.Log("재료에 해당하는 레시피를 찾을 수 없습니다: 알 수 없는 차 생성");
            return new MakedTea { teaName = TeaName.Unknown };
        }

        MakedTea makedTea = new MakedTea
        {
            teaName = recipe.teaName,
            brewTimeGap = tea.timeBrewed - recipe.brewTime,
            temperatureGap = tea.temperature - recipe.temperature
        };


        if (tea.additionalIngredient)
        {
            makedTea.additionalIngredient = tea.additionalIngredient.ingredientName;
        }

        // 아래는 평가로직에 넣기
        // short failCount = 0;

        // if (!tea.temperature.IsNear(recipe.temperature))
        // {
        //     Debug.Log($"차의 온도가 레시피에 근접하지 않습니다. (레시피: {recipe.temperature}°C, 실제: {tea.temperature}°C)");
        //     failCount++;
        // }

        // if (tea.timeBrewed != recipe.brewTime)
        // {
        //     Debug.Log($"차를 우린 시간이 레시피와 다릅니다. (레시피: {recipe.temperature})초, 실제: {tea.temperature}초");
        //     failCount++;
        // }
        // 
        // TODO: 추가재료 채점

        // switch (failCount)
        // {
        //     case 0:
        //         makedTea.Evaluation = EvaluationResult.Excellent;
        //         break;
        //     case 1:
        //         makedTea.Evaluation = EvaluationResult.Normal;
        //         break;
        //     default:
        //         makedTea.Evaluation = EvaluationResult.Bad;
        //         break;
        // }

        return makedTea;
    }


    private static bool IsIngredientProcessRight(TeaIngredient ingredient)
    {
        // 가공 순서 평가
        ProcessStep prevProcess = 0;
        foreach (ProcessStep step in ingredient.processSequence)
        {
            if (step < prevProcess)
            {
                return false;
            }
            prevProcess = step;
        }

        // 재료의 종류에 따라 평가
        switch (ingredient.ingredientType)
        {
            case IngredientType.TeaLeaf:
                if (ingredient.ingredientName == IngredientName.TeaLeaf_White)  // 백차 예외처리
                    if(ingredient.rolled == ResultStatus.None && ingredient.roasted == ResultStatus.None)
                        return true;
                    else 
                        return false;
                if (ingredient.roasted != ResultStatus.Success) return false;
                if (ingredient.rolled != ResultStatus.Success) return false;
                if (ingredient.oxidizedDegree == OxidizedDegree.Over) return false;
                break;

            case IngredientType.Flower:
                if (!ingredient.isChopped) return false;
                if (ingredient.roasted != ResultStatus.Success) return false;
                break;

            case IngredientType.Substitute:
                if (!ingredient.isChopped) return false;
                break;

            case IngredientType.Additional:
                Debug.LogError($"{ingredient.ingredientName}은(는) 추가 재료임: 로직 에러");
                break;
        }

        return true;
    }

    private static void ChangeTeaLeafName(TeaIngredient ingredient)
    {
        if (ingredient.ingredientName != IngredientName.TeaLeaf)
            Debug.LogError("찻잎이 아닌데 이름을 바꾸려 함");

        switch (ingredient.oxidizedDegree)
        {
            case OxidizedDegree.Zero:
            case OxidizedDegree.None:
                ingredient.ChangeIngredientName(IngredientName.TeaLeaf_Green);
                break;
            case OxidizedDegree.Full:
                ingredient.ChangeIngredientName(IngredientName.TeaLeaf_Black);
                break;
            case OxidizedDegree.Half:
                ingredient.ChangeIngredientName(IngredientName.TeaLeaf_Oolong);
                break;
            case OxidizedDegree.Low:
                ingredient.ChangeIngredientName(IngredientName.TeaLeaf_White);
                break;
            case OxidizedDegree.Over:
                break;
        }
    }
}
