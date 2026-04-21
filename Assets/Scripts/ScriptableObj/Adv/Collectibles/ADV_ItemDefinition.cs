using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Item", menuName = "ADV/Item Definition")]
public class ADV_ItemDefinition : ScriptableObject
{
    public string itemId;
    public Sprite icon;
    public string mapName;

    [Header("Localization object name")]
    public LocalizedString itemName;

    [Header("Localization object Description")]
    public LocalizedString itemDescription;

    public int stackLimit = 1;
}