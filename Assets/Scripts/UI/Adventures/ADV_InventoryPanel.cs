using LTLRN.UI;
using UnityEngine;
using UnityEngine.UI;

public class ADV_InventoryPanel : Panel
{
    private GameObjectsState objState;
    private GameLogic gameLogic;

    [Header("Buttons")]
    [SerializeField] private Button closeInventoryButton;

    [Header("Invent Content")]
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private ADV_InventorySlotUI slotPrefab;  //assign in Inspector

    public override void Initialize()
    {
        if (IsInitialized) return;
        closeInventoryButton.onClick.AddListener(CloseInventoryPanel);
        base.Initialize();
    }

    public override void Open()
    {
        objState = GameObject.FindWithTag("GameObjState").GetComponent<GameObjectsState>();
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();
        base.Open();

        LoadInventory();
    }

    private void LoadInventory()
    {
        // clear old slots
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        // spawn one slot per item
        foreach (var (def, qty) in ADV_Inventory.Instance.GetAllItems())
        {
            Debug.Log($"Loading inventory item: {qty}");
            ADV_InventorySlotUI slot = Instantiate(slotPrefab, inventoryContent);
            slot.Setup(def, qty);
        }
    }

    private void CloseInventoryPanel()
    {
        gameLogic.gameState = GameLogic.GameState.Play;
        PanelManager.CloseAll();
    }
}