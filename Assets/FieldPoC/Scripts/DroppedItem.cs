using UnityEngine;
using System.Collections;

public class DroppedItem : Interactable
{
    private ItemData itemData;
    private SpriteRenderer spriteRenderer;
    // public int amount;

    [SerializeField] private float floatingHeight = 0.1f;
    [SerializeField] private float floatingSpeed = 2f;
    [SerializeField] private float rotationSpeed = 50f;
    private bool hasLanded = false;

    public void Initialize(ItemData data)
    {
        itemData = data;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = data.itemIcon;
        originalSprite = data.itemIcon;
        highlightSprite = data.itemIcon;
        type = InteractableType.DroppedItem;
        DropAnimation();
    }

    public override void Interact(PlayerHarvestController player)
    {
        InventoryManager.Instance.AddItem(itemData, 1);
        Destroy(gameObject);
        Debug.Log($"Picked up {itemData.itemName}");
    }

    // public void Pickup()
    // {
    //     InventoryManager.Instance.AddItem(itemData, amount);
    //     Destroy(gameObject);
    //     Debug.Log($"Picked up {itemData.itemName} x{amount}");
    // }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            hasLanded = true;
            StopFalling();
        }
    }

   private void DropAnimation()
    {
        Vector3 originalPosition = transform.position;
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 3f)).normalized;
        float initialSpeed = Random.Range(4f, 6f);
        Vector3 initialVelocity = randomOffset * initialSpeed;
        StartCoroutine(ParabolicFall(originalPosition, initialVelocity));
    }

    private IEnumerator ParabolicFall(Vector3 startPosition, Vector3 initialVelocity)
    {
        float timeElapsed = 0f;
        float gravity = -9.8f;
        float flightDuration = Mathf.Abs((2 * initialVelocity.y) / gravity);

        Vector3 position = startPosition;
        while (timeElapsed < flightDuration && !hasLanded)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / flightDuration;

            position.x = startPosition.x + initialVelocity.x * timeElapsed;
            position.y = startPosition.y + initialVelocity.y * timeElapsed + 0.5f * gravity * Mathf.Pow(timeElapsed, 2);
            position.z = startPosition.z + initialVelocity.z * timeElapsed;

            transform.position = position;

            yield return null;
        }
        StartCoroutine(FloatingAndRotating());
    }

    private void StopFalling()
    {
        hasLanded = true;
        StartCoroutine(FloatingAndRotating());
    }

    private IEnumerator FloatingAndRotating()
    {
        while (true)
        {
            float newYPosition = Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;
            transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
