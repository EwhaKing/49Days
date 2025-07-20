using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class RoastingIngredient : MonoBehaviour
{
    public event Action OnBurnt;

    public bool IsBurnt { get; private set; }
    [Tooltip("가마솥 중간으로 모이게 하는 중력")]
    [SerializeField] private float centralGravityForce;

    private enum IngredientState
    {
        Default,
        Roasting,
        Roasted,
        Burnt
    }

    [SerializeField] private float burnTime;
    [SerializeField] private float maxRoastTime;
    [SerializeField] private Sprite teaSingle;
    [SerializeField] private Sprite roseSingle;

    private float timeLastStirred;
    private float roastTimer;
    private IngredientState ingredientState;
    private SpriteRenderer spriteRenderer;
    private new Rigidbody2D rigidbody2D;
    private Dictionary<IngredientName, Sprite> spriteMap;
    private Transform cauldronCenter;

    private void Awake()
    {
        spriteRenderer = transform.Find("RoastingVisual")?.GetComponent<SpriteRenderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        spriteMap = new Dictionary<IngredientName, Sprite>
        {
            { IngredientName.TeaLeaf, teaSingle },
            { IngredientName.Rose, roseSingle }
        };
    }

    public void Init(TeaIngredient currentIngredientData, Transform centerPoint)
    {
        cauldronCenter = centerPoint;
        timeLastStirred = 0f;
        roastTimer = 0f;
        IsBurnt = false;
        ingredientState = IngredientState.Roasting;
        OxidizedDegree oxidizedDegree = currentIngredientData.oxidizedDegree;

        if (spriteRenderer != null && spriteMap.TryGetValue(currentIngredientData.ingredientName, out Sprite sprite))
        {
            spriteRenderer.sprite = sprite;
            switch (oxidizedDegree)
            {
                case OxidizedDegree.Zero: spriteRenderer.color = new Color(0.8f, 1f, 0.8f); break;
                case OxidizedDegree.Half: spriteRenderer.color = new Color(1f, 0.8f, 0.3f); break;
                case OxidizedDegree.Full: spriteRenderer.color = new Color(0.8f, 0.4f, 0.2f); break;
                case OxidizedDegree.Over: spriteRenderer.color = Color.black; break;
            }
        }
        else
        {
            Debug.LogWarning("해당 재료의 스프라이트가 없거나, SpriteRenderer가 없습니다.");
        }
    }

    private void FixedUpdate()
    {
        if (ingredientState == IngredientState.Roasting && cauldronCenter != null)
        {
            Vector2 directionToCenter = (cauldronCenter.position - transform.position).normalized;
            rigidbody2D.AddForce(directionToCenter * centralGravityForce);
        }
    }

    private void Update()
    {
        if (ingredientState != IngredientState.Roasting) return;

        roastTimer += Time.deltaTime;
        timeLastStirred += Time.deltaTime;

        CheckForStirring();

        if (timeLastStirred > burnTime)
        {
            OnBurnt?.Invoke();
        }

        if (roastTimer >= maxRoastTime)
        {
            ingredientState = IngredientState.Roasted;
            Debug.Log("무사히 재료 덖기 완료!");
        }
    }

    private void CheckForStirring()
    {
        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (GetComponent<Collider2D>().OverlapPoint(mousePos2D))
        {
            Stir(mousePos2D);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Cursor"))
        {
            Stir(other.transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ingredientState != IngredientState.Roasting) return;

        if (collision.gameObject.CompareTag("RoastingIngredient"))
        {
            timeLastStirred = 0f;
            Debug.Log($"{name}과 {collision.gameObject.name}의 충돌 발생. 타이머를 리셋합니다.");
        }
    }

    private void Stir(Vector2 stirringPosition)
    {
        timeLastStirred = 0f;
        Vector2 direction = ((Vector2)transform.position - stirringPosition).normalized;
        rigidbody2D?.AddForce(direction *20f, ForceMode2D.Force);
    }

    public void Burn()
    {
        if (IsBurnt) return; 

        IsBurnt = true;
        ingredientState = IngredientState.Burnt;
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
        ingredientState = IngredientState.Roasted;
        enabled = false;
        if (rigidbody2D != null)
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
            rigidbody2D.bodyType = RigidbodyType2D.Static;
        }
    }

}