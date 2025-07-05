using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class RoastingIngredient : MonoBehaviour
{
    
    private float timeLastStirred = 0f;
    private float roastTimer = 0f;
    private bool isBurnt = false;
    public bool IsBurnt => isBurnt;
    [SerializeField] private float burnTime = 3f;
    [SerializeField] private float maxRoastTime = 8f;
    
    private SpriteRenderer spriteRenderer;
    private new Rigidbody2D rigidbody2D;

    // private bool isRoasting = false;
    // private bool isComplete = false;
    // public bool IsRoastingComplete => isComplete;

    // [SerializeField] private float pushRadius = 0.5f;
    // [SerializeField] private float pushForce = 0.5f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        rigidbody2D = GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            Debug.LogError("Rigidbody2D가 없습니다. 큰일이죠.");
        }
    }

    public void Init(TeaIngredient currentIngredientData, Sprite currentIngredientVisual)
    {
        timeLastStirred = 0f;
        roastTimer = 0f;
        isBurnt = false;

        SpriteRenderer renderer = transform.Find("RoastingVisual")?.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = currentIngredientVisual;
        }
    }

    void Update()
    {
        if (isBurnt) return;

        roastTimer += Time.deltaTime;
        timeLastStirred += Time.deltaTime;

        // UnityEngine.Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // mouseWorldPos.z = 0f;
        // float distance = UnityEngine.Vector2.Distance(transform.position, mouseWorldPos);

        UnityEngine.Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D ingredientCollider = GetComponent<Collider2D>();
        
        if (ingredientCollider.OverlapPoint(mousePos2D))
        {
            timeLastStirred = 0f;

            UnityEngine.Vector2 direction = (transform.position - (UnityEngine.Vector3)mousePos2D).normalized;
            GetComponent<Rigidbody2D>()?.AddForce(direction * 2f, ForceMode2D.Force);
        }

        if (timeLastStirred > burnTime)
        {
            CauldronLid.Instance?.OnIngredientBurned();
        }

        if (roastTimer >= maxRoastTime)
        {
            Debug.Log("무사히 재료 덖기 완료!");
            // OnRoastingComplete();
        }
    }

    public void Burn()
    {
        isBurnt = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }

        Debug.Log("재료를 태웁니다.");
    }

    public void DarkenColor()
    {
        if (spriteRenderer == null) return;

        Color color = spriteRenderer.color;
        color.r *= 0.9f;
        color.g *= 0.9f;
        color.b *= 0.9f;
        spriteRenderer.color = color;
    }

    public void Stop()
    {
        enabled = false;
    }
}
