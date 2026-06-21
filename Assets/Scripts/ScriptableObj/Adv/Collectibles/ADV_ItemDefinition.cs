using UnityEngine;
using UnityEngine.Localization;

public enum CollectibleItemType
{
    Object,
    Word
}

[CreateAssetMenu(fileName = "Item", menuName = "ADV/Item Definition")]
public class ADV_ItemDefinition : ScriptableObject
{
    [Header("Type")]
    public CollectibleItemType itemType;

    [Header("Description")]
    public string itemId;
    public Sprite icon;
    public string mapName;

    [Header("Localization object name")]
    public LocalizedString itemName;

    [Header("Localization object Description")]
    public LocalizedString itemDescription;

    public int stackLimit = 1;

    [Header("Word Source (DB) — only for itemType = Word")]
    [Tooltip("Identifies the DB row this word comes from. Must match the #dbtable/#id/#column " +
             "Ink tags on the dialogue line that reveals this word.")]
    public DatabaseReference wordReference;
}