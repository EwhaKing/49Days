using System;
using System.Collections.Generic;
using UnityEngine;

public class TeaIngredient : MonoBehaviour
{
    public IngredientName ingredientName {get; private set;}  // 재료 이름 

    public IngredientType ingredientType {get; private set;}  // 종류 (찻잎, 꽃, 대용차재료, 추가 재료)

    public SpriteStatus spriteStatus {get; private set;}      // 상태 (기본 상태, 손질된 상태, 유념된 상태, 뭉개진 상태, 덖은 상태)

    public bool isChopped {get; private set;}                 // 손질 여부

    public OxidizedDegree oxidizedDegree {get; private set;}  // 산화 정도 (x, 0, 50, 100, 탐)

    public ResultStatus roasted {get; private set;}           // 덖은 여부 (x, 성공, 실패) 

    public ResultStatus rolled {get; private set;}            // 유념 여부 (x, 성공, 실패)

    // 순서 리스트 (순서: 손질, 산화, 덖기, 유념)
    public List<ProcessStep> processSequence {get; private set;} = new List<ProcessStep>();  

    // 상태에 따른 이미지 딕셔너리: Init에서 재료 이름을 통해 Resources.Load함
    public Dictionary<SpriteStatus, Sprite> spriteVariants {get; private set;} = new Dictionary<SpriteStatus, Sprite>();


    /// <summary>
    /// 덖기 완료 시 호출
    /// </summary>
    /// <param name="isSuccess">성공 여부</param>
    public void Roast(bool isSuccess)
    {
        processSequence.Add(ProcessStep.Roast);
        changeSprite(SpriteStatus.Roasted);

        if (isSuccess)
        {
            roasted = ResultStatus.Success;
            Debug.Log($"{ingredientName}이(가) 덖어졌습니다.");
        }
        else
        {
            roasted = ResultStatus.Failed;
            Debug.Log($"{ingredientName}이(가) 덖기에 실패하여 탄 상태가 되었습니다.");
        }
    }

    /// <summary>
    /// 산화 완료 시 호출
    /// </summary>
    /// <param name="oxidizedDegree">산화 정도</param>
    public void Oxidize(OxidizedDegree oxidizedDegree)
    {
        this.oxidizedDegree = oxidizedDegree;
        processSequence.Add(ProcessStep.Oxidize);

        Debug.Log($"{ingredientName}이(가) {oxidizedDegree}로 산화되었습니다.");
    }

    /// <summary>
    ///  유념 완료 시 호출
    /// </summary>
    /// <param name="isSuccess">성공 여부</param>
    public void Roll(bool isSuccess)
    {
        processSequence.Add(ProcessStep.Roll);

        if (isSuccess)
        {
            rolled = ResultStatus.Success;
            changeSprite(SpriteStatus.Rolled);
            Debug.Log($"{ingredientName}이(가) 유념되었습니다.");
        }
        else
        {
            rolled = ResultStatus.Failed;
            changeSprite(SpriteStatus.Mashed);
            Debug.Log($"{ingredientName}이(가) 유념에 실패하여 뭉개졌습니다.");
        }
    }

    /// <summary>
    /// 손질 완료 시 호출
    /// </summary>
    public void Chop()
    {
        isChopped = true;
        processSequence.Add(ProcessStep.Chop);
        changeSprite(SpriteStatus.Chopped);
        Debug.Log($"{ingredientName}이(가) 손질되었습니다.");
    }

    void changeSprite(SpriteStatus newStatus)
    {
        if (!spriteVariants.ContainsKey(newStatus))
        {
            Debug.LogWarning($"{ingredientName}은(는) {newStatus}에 해당하는 스프라이트가 없습니다.");
            return;
        }
        spriteStatus = newStatus;
        spriteRenderer.sprite = spriteVariants[spriteStatus];
    }

    public void Init(IngredientName ingredientName, IngredientType ingredientType)
    {
        this.ingredientName = ingredientName;
        this.ingredientType = ingredientType;

        foreach (SpriteStatus status in Utills.GetValues<SpriteStatus>())
        {
            string spriteName = $"{ingredientName.ToLowerString()}_{status.ToLowerString()}";
            Sprite sprite = Resources.Load<Sprite>($"Arts/{spriteName}");
            if (sprite != null)
            {
                spriteVariants[status] = sprite;
            }
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        changeSprite(SpriteStatus.Default);
    }

    SpriteRenderer spriteRenderer;

}
