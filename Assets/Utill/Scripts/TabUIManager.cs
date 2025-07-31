using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabUIManager : MonoBehaviour
{
    public static TabUIManager Instance { get; private set; }

    [SerializeField] private GameObject TabCanvasPrefab;
    private GameObject inventoryCanvas;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (TabCanvasPrefab != null)
        {
            inventoryCanvas = Instantiate(TabCanvasPrefab);
            inventoryCanvas.SetActive(false);
            DontDestroyOnLoad(inventoryCanvas);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryCanvas.SetActive(isOpen);
    }

    public void CloseInventory()
    {
        isOpen = false;
        inventoryCanvas.SetActive(false);
    }
}
