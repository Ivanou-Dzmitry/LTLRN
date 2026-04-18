using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[System.Serializable]
public class GridLayoutConfig
{
    public Vector2 cellSize;
    public Vector2 spacing;
}

public class ADV_InventoryPanel : Panel
{
    private GameObjectsState objState;
    private GameLogic gameLogic;
    protected ADV_MapManager mapManager;

    //localization
    private LanguageSwitcher locManager;
    private Languages currentLang;

    //root panel
    [SerializeField] private Transform inventoryContent;

    [Header("Buttons")]
    [SerializeField] private Button closeInventoryButton;

    [Header("Tab Buttons")]
    [SerializeField] private Button[] tabButton;
    private System.Action[] tabActions;
    private int currentTabIndex = -1;

    [SerializeField] private GridLayoutGroup grid;

    [Header("Layouts")]
    [SerializeField] private GridLayoutConfig inventoryLayout;
    [SerializeField] private GridLayoutConfig taskLayout;
    [SerializeField] private GridLayoutConfig mapLayout;

    [Header("Invent Content")]
    [SerializeField] private ADV_InventorySlotUI inventSlotPrefab;  //Inventory slot prefab    

    [Header("Tasks Content")]
    [SerializeField] private ADV_TaskSlotUI taskSlotPrefab;  //Inventory slot prefab

    [Header("Description")]
    [SerializeField] private TMP_Text inventoryDescriptionText;

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

        // Setup tab buttons
        tabActions = new System.Action[]
        {
            LoadInventory,
            LoadTasks,
            LoadMap
        };

        for (int i = 0; i < tabButton.Length; i++)
        {
            int index = i; // IMPORTANT (closure fix)
            tabButton[i].onClick.AddListener(() =>
            {
                SelectTab(index);
                tabActions[index].Invoke();
            });
        }

        //default to inventory tab #1
        SelectTab(0);
        LoadClasses();
        LoadInventory();              
    }

    private void LoadClasses()
    {
        //get current language
        locManager = GameObject.FindWithTag("LangSwitcher").GetComponent<LanguageSwitcher>();
        if (locManager != null)       
            currentLang = LanguageSwitcher.GetLanguageFromLocale(locManager.GetLocale());

        mapManager = GameObject.FindWithTag("MapManager").GetComponent<ADV_MapManager>();
    }

    private void LoadInventory()
    {
        //Debug.Log("Loading inventory panel...");

        // clear old slots
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);
        
        ApplyLayout(inventoryLayout);
        
        // spawn one slot per item
        foreach (var (def, qty) in ADV_Inventory.Instance.GetAllItems())
        {
            //Debug.Log($"Loading inventory item: {qty}");
            ADV_InventorySlotUI slot = Instantiate(inventSlotPrefab, inventoryContent);

            slot.name = def.name;
            slot.Setup(def, qty);
        }

        //load description from loctable
        inventoryDescriptionText.text = LocalizationHelper.GetSafe(
            "LTLRN",
            "CollectedObjDescTxt",
            "No description"
        );
    }
    private void CloseInventoryPanel()
    {
        gameLogic.gameState = GameLogic.GameState.Play;
        PanelManager.CloseAll();
    }

    private void OnSlotSelected(ADV_InventorySlotUI slot)
    {
        inventoryDescriptionText.text = slot.GetDescription();
    }

    private void OnDestroy()
    {
        ADV_InventorySlotUI.OnSlotSelected -= OnSlotSelected;
    }

    private void LoadTasks()
    {
        // clear old slots
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        if(mapManager == null)
            return;

        if(mapManager.currentMapEvent == null)
        {
            inventoryDescriptionText.text = "...";
            return;
        }

        ApplyLayout(taskLayout);

        //load description  from event      
        inventoryDescriptionText.text = mapManager.currentMapEvent.GetDescription();        

        foreach (var condition in mapManager.currentMapEvent.conditions)
        {
            var slot = Instantiate(taskSlotPrefab, inventoryContent);
            slot.SetupTask(condition);
            slot.name = condition.name;
        }

        Debug.Log("Loading tasks panel...");
    }

    private void LoadMap()
    {
        // clear old slots
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        ApplyLayout(mapLayout);

        inventoryDescriptionText.text = "...";

        Debug.Log("Loading map panel...");
    }

    private void SelectTab(int index)
    {
        currentTabIndex = index;

        for (int i = 0; i < tabButton.Length; i++)
        {
            var img = tabButton[i].GetComponent<Image>();

            if (img == null) continue;

            img.color = (i == index)
                ? palette.Gray2Light   // selected
                : palette.Panel01;  // deselected
        }
    }

    private void ApplyLayout(GridLayoutConfig config)
    {
        grid.cellSize = config.cellSize;
        grid.spacing = config.spacing;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)inventoryContent);
    }
}