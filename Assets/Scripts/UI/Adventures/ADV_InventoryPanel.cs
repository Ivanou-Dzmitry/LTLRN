using LTLRN.UI;
using TMPro;
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
    [SerializeField] private TMP_Text descriptionText;

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

        ADV_InventorySlotUI.OnSlotSelected += OnSlotSelected;

        base.Open();

        LoadInventory();
    }

    private void LoadInventory()
    {
        Debug.Log("Loading inventory panel...");

        // clear old slots
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        // spawn one slot per item
        foreach (var (def, qty) in ADV_Inventory.Instance.GetAllItems())
        {
            Debug.Log($"Loading inventory item: {qty}");
            ADV_InventorySlotUI slot = Instantiate(slotPrefab, inventoryContent);

            slot.name = def.name;
            slot.Setup(def, qty);
        }

        descriptionText.text = "...";
    }
    private void CloseInventoryPanel()
    {
        gameLogic.gameState = GameLogic.GameState.Play;
        PanelManager.CloseAll();
    }

    private void OnSlotSelected(ADV_InventorySlotUI slot)
    {
        descriptionText.text = slot.GetDescription();
    }

    private void OnDestroy()
    {
        ADV_InventorySlotUI.OnSlotSelected -= OnSlotSelected;
    }
}