using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeaEvaluator : MonoBehaviour
{
    public static readonly List<TeaRecipe> teaRecipes;
    public static MakedTea EvaluateTea(Tea tea)
    {
        foreach (TeaIngredient ingredient in tea.ingredients)
        {
            // 재료의 종류에 따라 평가
            if (!EvaluateIngredient(ingredient))
            {
                Debug.Log($"{ingredient.ingredientName}의 가공이 잘못되었습니다: 알 수 없는 차 생성");
                return new MakedTea{
                    TeaName = TeaName.Unknown,
                    Evaluation = EvaluationResult.Bad
                };
            }
        }

        // 레시피 찾기
        
        TeaRecipe recipe = teaRecipes.Find(r => 
            r.ingredients.EqualIgnoreOrder(tea.ingredients.ConvertAll(i => i.ingredientName)) && 
            r.additionalIngredient == tea.additionalIngredient.ingredientName);

        // if (recipes.Count == 0)
        // {
        //     Debug.Log("재료에 해당하는 레시피를 찾을 수 없습니다: 알 수 없는 차 생성");
        //     return new MakedTea
        //     {
        //         TeaName = TeaName.Unknown,
        //         Evaluation = EvaluationResult.Bad
        //     };
        // }

        // TeaRecipe recipe;
        // int failCount = 0;

        // if (tea.temperature != recipe.temperature)
        // {
        //     Debug.Log($"차의 온도가 레시피에 근접하지 않습니다. (레시피: {recipe.temperature}°C, 실제: {tea.temperature}°C)");
        // } 

        return new MakedTea
        {
            TeaName = recipe.teaName,
            Evaluation = EvaluationResult.Excellent
        };
    }


    private static bool EvaluateIngredient(TeaIngredient ingredient)
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
                if (ingredient.roasted != ResultStatus.Success) return false;
                if (ingredient.rolled != ResultStatus.Success) return false;
                switch (ingredient.oxidizedDegree)
                {
                    case OxidizedDegree.Zero:
                    case OxidizedDegree.None:
                        // ingredient.ingredientName
                        break;
                    case OxidizedDegree.Full:
                        break;
                    case OxidizedDegree.Half:
                        break;
                    case OxidizedDegree.Over:
                        return false;
                }
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
}
