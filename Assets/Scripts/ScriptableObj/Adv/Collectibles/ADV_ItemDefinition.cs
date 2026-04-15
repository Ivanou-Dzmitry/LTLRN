using UnityEngine;

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
    
    public int stackLimit = 1;
}