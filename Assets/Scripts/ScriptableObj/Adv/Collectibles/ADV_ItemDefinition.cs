using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Item", menuName = "ADV/Item Definition")]
public class ADV_ItemDefinition : ScriptableObject
{
    public string itemId;
    public Sprite icon;
    public string mapName;

    [Header("Name")]
    public string displayNameRU;
    public string displayNameEN;
    
    [Header("Descriptions")]
    [TextArea] public string descriptionLang01;
    [TextArea] public string descriptionLang02;

    [Header("Localization object name")]
    public LocalizedString itemName;

    [Header("Localization objectDescription")]
    public LocalizedString itemDescription;

    public int stackLimit = 1;
}