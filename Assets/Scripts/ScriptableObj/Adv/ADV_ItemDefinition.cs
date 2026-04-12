using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ADV/Item Definition")]
public class ADV_ItemDefinition : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public int stackLimit = 1;
}